using CommunityToolkit.Maui.Views;
using static Android.Provider.MediaStore;

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

        video1.Source = MediaSource.FromFile("video1.mp4");
    }
}