using EquipmentProject.Data;
using EquipmentProject.Services;
using Microsoft.Extensions.Logging;
using Radzen;

namespace EquipmentProject
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            var apiBaseUrl = "http://127.0.0.1:5028/";
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

            builder.Services.AddDbContext<LocalDbContext>();
            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddScoped<SyncService>();
            builder.Services.AddScoped<AuthSessionService>();
            builder.Services.AddScoped<DbInitializerService>();
            builder.Services.AddSingleton<PasswordHasher>();
            builder.Services.AddSingleton<ApiHostService>();
            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddRadzenComponents();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializerService>();
                dbInitializer.InitializeAsync().GetAwaiter().GetResult();
            }

            return app;
        }
    }
}
