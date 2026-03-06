namespace Projeto_Jogo_Labirinto;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        DeviceDisplay.Current.KeepScreenOn = true;

        await Task.Delay(5000);
        SplashGrid.Opacity = 0;
        var main = new NavigationPage(new MainPage());
        await Task.Delay(2500);
        Application.Current.MainPage = main;
    }
}