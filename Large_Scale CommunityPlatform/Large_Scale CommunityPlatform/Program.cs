using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Extensions;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Field : Service 
var app = builder.Build();


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

await app.SeedRoleAsync();

app.Run();

