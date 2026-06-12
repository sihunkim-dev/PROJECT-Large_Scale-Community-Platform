using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Large_Scale_CommunityPlatform.Extensions;

public static class IdentitySeedExtension
{
    public static async Task<WebApplication> SeedRoleAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(IdentitySeedExtension));
        
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            
            //Roles in Application
            string[] userRoles = { "Admin", "CategoryManager", "User" };

            foreach (var role in userRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));

                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Role created successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to create Roles");
                    }
                }
            }

            //Admin Account Manager
            string adminEmail = "admin@test.com";
            string adminPassword = app.Configuration["SeedAdmin:Password"] ?? "Admin123!";
            
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin",
                    DoB = new DateTime(1900, 1, 1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            
                var result = await userManager.CreateAsync(newAdmin, adminPassword);
            
                if (result.Succeeded)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(newAdmin, "Admin");
                    if (addRoleResult.Succeeded)
                    {
                        logger.LogInformation("Admin is created successfully");
                    }else
                    {
                        logger.LogError(
                            "Admin created but failed to assign role: {Errors}",
                            string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    }
                    
                }else
                {
                    logger.LogError(
                        "Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ISSUE is occurred in Seeding Role");
        }

        return app;
    }
}