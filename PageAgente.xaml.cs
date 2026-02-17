using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using System.Threading.Tasks;
using System.Text.Json;

namespace Projeto_Jogo_Labirinto;

public partial class PageAgente : ContentPage
{
	string codigo = "";
    int posX = 1;
    int posY = 6;
    public PageAgente(string codigoo)
	{
		InitializeComponent();
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
    }

    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    /*int[,] mapa = new int[14, 24]
{
    { 5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5 },
    { 5,1100,0101,0101,0101,0101,0110,1100,0101,0110,1101,0100,0101,0111,1100,0110,1101,0100,0101,0101,0101,0101,0110,5 },
    { 5,1010,1100,0110,1100,0101,0011,1001,0110,1010,1100,0011,1100,0101,0011,1001,0110,1011,1100,0101,0101,0100,0011,5 },
    { 5,1001,0011,1011,1001,0100,0101,0100,0011,1010,1001,0100,0011,1100,0111,1100,0001,0101,0011,1100,0110,1011,1110,5 },
    { 5,1100,0101,0111,1100,0011,1100,0011,1100,0011,1101,0011,1100,0011,1100,0011,1100,0101,0101,0010,5,5,5,5 },
    { 5,1001,0110,1100,0011,1100,0011,1110,1001,0110,8,1101,0010,1100,0011,1100,0011,1110,1100,5,5,5,5,5 },
    { 5,1101,0010,1001,0110,1001,0110,1010,1100,0011,1001,0100,0011,1001,0110,1001,0110,1000,5,5,5,5,5,5 },
    { 5,1110,1000,0101,0001,0111,1001,0010,1001,0110,1100,0010,1100,0111,1001,0110,1001,5,5,5,5,5,5,5 },
    { 5,1001,0011,1101,0101,0110,1100,0011,1101,0011,1010,1011,1000,0101,0110,1001,0110,5,5,5,5,5,5,5 },
    { 5,1101,0110,1100,0110,1001,0001,0110,1100,0101,0010,1110,1011,1100,0011,1100,0001,0011,5,5,5,5,5,5 },
    { 5,1100,0011,1010,1001,0110,1110,1001,0010,1100,0011,1001,0110,1001,0101,0001,0110,1100,5,5,5,5,5,5 },
    { 5,1010,1101,0001,0110,1010,1001,0101,0011,1001,0101,0110,1001,0101,0101,0110,1001,0011,1101,0011,1001,0110,1010,5 },
    { 5,1001,0101,0101,0011,1001,0101,0101,0101,0101,0111,1001,0101,0101,0111,1001,0101,0101,0101,0101,0101,0011,1011,5 },
    { 5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5 }
};*/

    string[,] mapa = new string[14, 24]
{
    { "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5" },
    { "5","1100","0101","0101","0101","0101","0110","1100","0101","0110","1101","0100","0101","0111","1100","0110","1101","0100","0101","0101","0101","0101","0110","5" },
    { "5","1010","1100","0110","1100","0101","0011","1001","0110","1010","1100","0011","1100","0101","0011","1001","0110","1011","1100","0101","0101","0100","0011","5" },
    { "5","1001","0011","1011","1001","0100","0101","0100","0011","1010","1001","0100","0011","1100","0111","1100","0001","0101","0011","1100","0110","1011","1110","5" },
    { "5","1100","0101","0111","1100","0011","1100","0011","1100","0011","1101","0011","1100","0011","1100","0011","1100","0101","0101","0010","5","5","5","5" },
    { "5","1001","0110","1100","0011","1100","0011","1110","1001","0110","8","1101","0010","1100","0011","1100","0011","1110","1100","5","5","5","5","5" },
    { "5","1101","0010","1001","0110","1001","0110","1010","1100","0011","1001","0100","0011","1001","0110","1001","0110","1000","5","5","5","5","5","5" },
    { "5","1110","1000","0101","0001","0111","1001","0010","1001","0110","1100","0010","1100","0111","1001","0110","1001","5","5","5","5","5","5","5" },
    { "5","1001","0011","1101","0101","0110","1100","0011","1101","0011","1010","1011","1000","0101","0110","1001","0110","5","5","5","5","5","5","5" },
    { "5","1101","0110","1100","0110","1001","0001","0110","1100","0101","0010","1110","1011","1100","0011","1100","0001","0011","5","5","5","5","5","5" },
    { "5","1100","0011","1010","1001","0110","1110","1001","0010","1100","0011","1001","0110","1001","0101","0001","0110","1100","5","5","5","5","5","5" },
    { "5","1010","1101","0001","0110","1010","1001","0101","0011","1001","0101","0110","1001","0101","0101","0110","1001","0011","1101","0011","1001","0110","1010","5" },
    { "5","1001","0101","0101","0011","1001","0101","0101","0101","0101","0111","1001","0101","0101","0111","1001","0101","0101","0101","0101","0101","0011","1011","5" },
    { "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5" }
};


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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (VideoView != null)
            {
                VideoView.IsVisible = false;
                VideoView.Opacity = 0;
            }

            if (LabirintoView != null)
            {
                LabirintoView.IsVisible = true;
                LabirintoView.Opacity = 1;
            }
        });
    }

    private async void BtnEsquerda_Clicked(object sender, EventArgs e)
    {
        string quadricula = mapa[posX, posY];
        if (quadricula[0] == '0')
        {
            posY--;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo },{ "p_posx", posX },{ "p_posy", posY} };
            await _supabase.Client!.Rpc("atualizar_posicao", parametros);
        }
        else
        {
            await DisplayAlert("Movimento inválido", "Você não pode se mover para essa direção.", "OK");
            Mensagem.IsVisible = true;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
        }
    }
    private async void BtnDireita_Clicked(object sender, EventArgs e)
    {
        string quadricula = mapa[posX, posY];
        if (quadricula[0] == '2')
        {
            posY++;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            await _supabase.Client!.Rpc("atualizar_posicao", parametros);
        }
        else
        {
            await DisplayAlert("Movimento inválido", "Você não pode se mover para essa direção.", "OK");
            Mensagem.IsVisible = true;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
        }
    }
    private async void BtnCima_Clicked(object sender, EventArgs e)
    {
        string quadricula = mapa[posX, posY];
        if (quadricula[0] == '1')
        {
            posX--;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            await _supabase.Client!.Rpc("atualizar_posicao", parametros);
        }
        else
        {
            await DisplayAlert("Movimento inválido", "Você não pode se mover para essa direção.", "OK");
            Mensagem.IsVisible = true;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
        }
    }
    private async void BtnBaixo_Clicked(object sender, EventArgs e)
    {
        string quadricula = mapa[posX, posY];
        if (quadricula[0] == '3')
        {
            posX++;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            await _supabase.Client!.Rpc("atualizar_posicao", parametros);
        }
        else
        {
            await DisplayAlert("Movimento inválido", "Você não pode se mover para essa direção.", "OK");
            Mensagem.IsVisible = true;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
        }
    }
}
