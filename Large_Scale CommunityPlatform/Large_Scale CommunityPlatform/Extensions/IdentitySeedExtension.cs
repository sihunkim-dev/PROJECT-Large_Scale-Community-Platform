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

            string[] userRoles = { "Admin", "CategoryManager", "User" };

            foreach (var role in userRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            //Admin Account Manager
            string adminEmail = "admin@test.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin",
                    DoB = new DateTime(1900, 1, 1)
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    logger.LogInformation("Admin is created successfully");
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