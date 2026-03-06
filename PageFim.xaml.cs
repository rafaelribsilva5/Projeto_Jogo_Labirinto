using Plugin.Maui.Audio;

namespace Projeto_Jogo_Labirinto;

public partial class PageFim : ContentPage
{
    string tempo_restante = "";

    private IAudioPlayer click_som;
    private IAudioManager _audioManager = AudioManager.Current;
    public PageFim(string tempo_restante_str)
	{
		InitializeComponent();
        tempo_restante = tempo_restante_str;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        lbl_Tempo_execucao.Text = tempo_restante;

        if (click_som == null)
        {
            var stream2 = await FileSystem.OpenAppPackageFileAsync("click_som.mp3");
            click_som = _audioManager.CreatePlayer(stream2);
        }
    }
    private void Video2_MediaEnded(object? sender, EventArgs e)
    {
        video2.Stop();
        video2.Source = null;
        video2.Handler?.DisconnectHandler();
        Tempo_final.IsVisible = true;
    }

    private void btn_Avancar_Clicked(object? sender, EventArgs e)
    {
        click_som.Play();
        botao.IsVisible = false;
        Video2_MediaEnded(sender, e);
    }
    private void Video2_MediaFailed(object? sender, CommunityToolkit.Maui.Core.MediaFailedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PageFim] Vídeo falhou: {e?.ErrorMessage}");
        Tempo_final.IsVisible = true;
    }

    private async void btn_Pagina_inicial_Clicked(object? sender, EventArgs e)
    {
        click_som.Play();
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}