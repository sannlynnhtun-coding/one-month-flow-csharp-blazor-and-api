
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

    builder.Services.AddMudServices();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddScoped<IDbConnectionFactory, MssqlConnectionFactory>(n => new MssqlConnectionFactory(connectionString));
    builder.Services.AddScoped<ISqlService, MssqlService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<UserTechStackService>();
    builder.Services.AddScoped<TechStackService>();
    builder.Services.AddScoped<TeamService>();
    builder.Services.AddScoped<TeamUserService>();
    builder.Services.AddScoped<ProjectService>();
    builder.Services.AddScoped<ProjectTechStackService>();
    builder.Services.AddScoped<ProjectTeamActivityService>();
    builder.Services.AddScoped<DashboardService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}