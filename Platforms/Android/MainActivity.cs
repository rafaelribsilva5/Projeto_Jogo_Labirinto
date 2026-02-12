using Android.App;
using Android.Views;
using Android.Content.PM;
using Android.OS;

namespace Projeto_Jogo_Labirinto
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.SetFlags(
                WindowManagerFlags.Fullscreen,
                WindowManagerFlags.Fullscreen);

            Window.DecorView.SystemUiVisibility =
                (StatusBarVisibility)(
                    SystemUiFlags.ImmersiveSticky |
                    SystemUiFlags.HideNavigation |
                    SystemUiFlags.Fullscreen);
        }
    }
}
