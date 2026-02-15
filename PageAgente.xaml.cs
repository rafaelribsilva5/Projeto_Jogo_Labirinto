namespace Projeto_Jogo_Labirinto;

public partial class PageAgente : ContentPage
{
	public PageAgente(string codigo)
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
		System.Diagnostics.Debug.WriteLine($"[PageAgente] Vídeo falhou: {e?.ErrorMessage}");
		MostrarLabirinto();
	}

	private void MostrarLabirinto()
	{
		VideoLayout.IsVisible = false;
		LabirintoLayout.IsVisible = true;
	}
}
