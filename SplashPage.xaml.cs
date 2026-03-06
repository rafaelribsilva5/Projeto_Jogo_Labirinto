namespace Projeto_Jogo_Labirinto;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        mainpage();
    }
    private async void mainpage()
    {
        await Task.Delay(5000);
        SplashGrid.Opacity = 0;
        await Task.Delay(2500);
        Application.Current.MainPage = new NavigationPage(new MainPage());
        await Navigation.PushAsync(new MainPage());
    }
    private void OnAppearing(object sender, EventArgs e)
    {
        mainpage();
    }
}