using Large_Scale_CommunityPlatform.Configurations;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Extensions;
using Large_Scale_CommunityPlatform.Models.Entities;
using Large_Scale_CommunityPlatform.Services.Reactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


//DbInjection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


//Add DbContext as a service
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
        )
    );

builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();

builder.Services.Configure<RedisOption>(builder.Configuration.GetSection("RedisOption"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisOption = sp.GetRequiredService<IOptions<RedisOption>>().Value;

    if (string.IsNullOrWhiteSpace(redisOption.Configuration))
    {
        throw new InvalidOperationException("RedisOption:Configuration is missing");
    }

    return ConnectionMultiplexer.Connect(redisOption.Configuration);
});

builder.Services.AddScoped<ReactionCacheService>();
builder.Services.AddScoped<PostReactionService>();
builder.Services.AddScoped<CommentReactionService>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT Bearer token"
        };

        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});





//Field : Service 
var app = builder.Build();

await app.SeedRoleAsync();

//Field : Middleware Pipeline
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//app.UseHttpsRedirection();


app.Run();

