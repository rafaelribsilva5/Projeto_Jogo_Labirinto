namespace Projeto_Jogo_Labirinto;

public partial class PageFim : ContentPage
{
    string tempo_restante = "";
    public PageFim(string tempo_restante_str)
	{
		InitializeComponent();
        tempo_restante = tempo_restante_str;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        lbl_Tempo_execucao.Text = tempo_restante;
    }
    private void Video2_MediaEnded(object? sender, EventArgs e)
    {
        video2.Stop();
        video2.Source = null;
        video2.Handler?.DisconnectHandler();
    }
    private void Video2_MediaFailed(object? sender, CommunityToolkit.Maui.Core.MediaFailedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PageFim] Vídeo falhou: {e?.ErrorMessage}");
    }

    private async void btn_Pagina_inicial_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}