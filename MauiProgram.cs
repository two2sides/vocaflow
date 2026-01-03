using Microsoft.Extensions.Logging;
using VocaFlow.Services;
using VocaFlow.Views;

namespace VocaFlow
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 注册服务
            builder.Services.AddSingleton<IAppSettings, AppSettings>();
            builder.Services.AddSingleton<IDatabase, DatabaseService>();

            // 注册页面（用于依赖注入）
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<LyricResultPage>();
            builder.Services.AddTransient<VocabularyPage>();
            builder.Services.AddTransient<VocabSelectPage>();
            builder.Services.AddTransient<VocabEditPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<VocabReviewPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
