
using CommunityToolkit.Maui.Views;

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

        video1.Source = MediaSource.FromFile("video1.mp4");
    }
}