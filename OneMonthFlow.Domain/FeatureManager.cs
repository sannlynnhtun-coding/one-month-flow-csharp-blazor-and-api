namespace OneMonthFlow.Domain;

public static class FeatureManager
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ProjectService>();
        services.AddScoped<ProjectTeamActivityService>();
        services.AddScoped<ProjectTechStackService>();
        services.AddScoped<TeamService>();
        services.AddScoped<TeamActivityService>();
        services.AddScoped<TeamUserService>();
        services.AddScoped<TechStackService>();
        services.AddScoped<UserService>();
        services.AddScoped<ISqlService, MySqlService>();
        services.AddScoped<IDbConnectionFactory, MySqlConnectionFactory>();

        return services;
    }
}