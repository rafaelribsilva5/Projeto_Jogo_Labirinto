using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;

namespace Projeto_Jogo_Labirinto
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // opcional: registrar o serviço se quiseres DI no futuro
            // builder.Services.AddSingleton<SupabaseService>();
            builder.Services.AddSingleton(AudioManager.Current);

            return builder.Build();
        }
    }
}
