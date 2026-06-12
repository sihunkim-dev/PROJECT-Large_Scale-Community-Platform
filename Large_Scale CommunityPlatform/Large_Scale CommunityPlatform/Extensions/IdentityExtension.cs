using Large_Scale_CommunityPlatform.Configurations;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Large_Scale_CommunityPlatform.Extensions;

public static class IdentityExtension
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services
            .AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        //AddSingleton()
        services.AddSingleton<IConfigureOptions<IdentityOptions>, IdentityOptionsConfigs>();
        
        return services;
    }
}