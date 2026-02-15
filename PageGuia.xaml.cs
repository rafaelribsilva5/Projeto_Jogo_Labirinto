namespace Projeto_Jogo_Labirinto;

public partial class PageGuia : ContentPage
{
	public PageGuia(string codigo)
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	private void Video1_MediaEnded(object? sender, EventArgs e)
	{
		MostrarLabirinto();
	}

	private void Video1_MediaFailed(object? sender, CommunityToolkit.Maui.Core.MediaFailedEventArgs e)
	{
		// Se o vídeo não carregar (ex.: ficheiro em falta), mostrar labirinto na mesma para permitir testes
		System.Diagnostics.Debug.WriteLine($"[PageGuia] Vídeo falhou: {e?.ErrorMessage}");
		MostrarLabirinto();
	}

	private void MostrarLabirinto()
	{
		VideoLayout.IsVisible = false;
		LabirintoLayout.IsVisible = true;
	}
}
