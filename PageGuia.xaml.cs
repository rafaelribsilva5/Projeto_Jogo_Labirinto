using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using System.Threading.Tasks;
using System.Text.Json;

namespace Projeto_Jogo_Labirinto;

public partial class PageGuia : ContentPage
{
    string codigo = "";
    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;

    public PageGuia(string codigoo)
	{
		InitializeComponent();
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
    }
    bool labirinto = false;
    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    Image mira;

    protected override void OnAppearing()
	{
		base.OnAppearing();
        if (labirinto == true)
            return;
    
        for (int i = 0; i < 24; i++)
        {
            MapaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        for (int j = 0; j < 14; j++)
        {
            MapaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        }

        mira = new Image
        {
            Source = "mira.png",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        MapaGrid.Children.Add(mira);

        Grid.SetColumn(mira, 1);
        Grid.SetRow(mira, 6);

        labirinto = true;
        atualizar_pos();
    }

	private void Video1_MediaEnded(object? sender, EventArgs e)
	{
        VideoView.IsVisible = false;
        VideoView.Opacity = 0;
        LabirintoView.IsVisible = true;
        LabirintoView.Opacity = 1;
        video1.Stop();
    }

    private async Task atualizar_pos()
        {
        while (true)
        {
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
            var resposta = await _supabase.Client!.Rpc("obter_posicao", parametro);

            var json = JsonDocument.Parse(resposta.Content);
            var root = json.RootElement;

            int posX = root.GetProperty("posX").GetInt32();
            int posY = root.GetProperty("posY").GetInt32();

            Grid.SetColumn(mira, posX);
            Grid.SetRow(mira, posY);

            await Task.Delay(300);
        }
    }
}
