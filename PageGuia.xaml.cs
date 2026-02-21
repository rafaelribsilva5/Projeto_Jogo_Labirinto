using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using Microsoft.Maui.Devices.Sensors;
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
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
    }
    bool labirinto = false;
    bool invertido = false;
    bool esta_na_porta = false;
    bool entrou_na_porta = false;
    bool interferencia = false;
    int total_agitar = 0;
    bool passou_interferencia = false;
    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    Image mira;

    protected override void OnAppearing()
	{
		base.OnAppearing();

        if (labirinto == true)
        {
            esta_na_porta = false;
            atualizar_pos();
            return;
        }

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
        video1.Source = null;
        video1.Handler?.DisconnectHandler();
    }

    private async Task atualizar_pos()
        {
        while (esta_na_porta == false && interferencia == false)
        {
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo }};
            var resposta = await _supabase.Client!.Rpc("obter_posicao", parametro);

            var json = JsonDocument.Parse(resposta.Content);
            var root = json.RootElement;

            int posX = root.GetProperty("posX").GetInt32();
            int posY = root.GetProperty("posY").GetInt32();

            Grid.SetColumn(mira, posX);
            Grid.SetRow(mira, posY);

            analise();

            await Task.Delay(300);
        }
    }


    private async void analise()
    {
        if (invertido == false)
        {
            if (Grid.GetColumn(mira) == 5 && Grid.GetRow(mira) == 3)
            {
                invertido = true;
                DisplayAlert("🔴 CONTROLOS INVERTIDOS!", "↑ agora é ↓\r\n↓ agora é ↑\r\n← agora é →\r\n→ agora é ←", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosInvertidos.IsVisible = true;

            }
            else if(Grid.GetColumn(mira) == 8 && Grid.GetRow(mira) == 3)
            {
                invertido = true;
                DisplayAlert("🔴 CONTROLOS INVERTIDOS!", "↑ agora é ↓\r\n↓ agora é ↑\r\n← agora é →\r\n→ agora é ←", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosInvertidos.IsVisible = true;
            }
            else if(Grid.GetColumn(mira) == 6 && Grid.GetRow(mira) == 9)
            {
                invertido = true;
                DisplayAlert("🔴 CONTROLOS INVERTIDOS!", "↑ agora é ↓\r\n↓ agora é ↑\r\n← agora é →\r\n→ agora é ←", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosInvertidos.IsVisible = true;
            }
        }
        if (invertido == true)
        {
            if (Grid.GetColumn(mira) == 4 && Grid.GetRow(mira) == 3)
            {
                invertido = false;
                DisplayAlert("🟢 CONTROLOS CORRIGIDOS!", "A interferência desapareceu.\r\nOs teus comandos respondem corretamente.", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosCorretos.IsVisible = true;
            }
            else if (Grid.GetColumn(mira) == 8 && Grid.GetRow(mira) == 2)
            {
                invertido = false;
                DisplayAlert("🟢 CONTROLOS CORRIGIDOS!", "A interferência desapareceu.\r\nOs teus comandos respondem corretamente.", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosCorretos.IsVisible = true;
            }
            else if (Grid.GetColumn(mira) == 7 && Grid.GetRow(mira) == 9)
            {
                invertido = false;
                DisplayAlert("🟢 CONTROLOS CORRIGIDOS!", "A interferência desapareceu.\r\nOs teus comandos respondem corretamente.", "Ok");
                //LabirintoView.Opacity = 0.4;
                //ControlosCorretos.IsVisible = true;
            }
        }
        if (Grid.GetColumn(mira) == 10 && Grid.GetRow(mira) == 5 && entrou_na_porta == false)
        {
            esta_na_porta = true;
            entrou_na_porta = true;
            await Navigation.PushAsync(new PageGuiaPorta(codigo));
        }
        if (Grid.GetColumn(mira) == 14 && Grid.GetRow(mira) == 1 && passou_interferencia == false)
        {
            Abanar_necessario.IsVisible = true;
            interferencia = true;
            Accelerometer.Start(SensorSpeed.Game);
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        }
    }

    private async void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;

        double aceleracao = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        if (aceleracao >= 6)
        {
            total_agitar++;
        }
        if (total_agitar >= 300)
        {
            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("telemovel_abanou", parametro);
            await DisplayAlert("🟢 INTERFERÊNCIA REMOVIDA!", "A interferência desapareceu.\r\nOs teus comandos respondem corretamente.", "Ok");
            await esperar_calibragem();
        }
    }

    private async Task esperar_calibragem()
    {
        bool calibragem_feita = false;
        while (calibragem_feita == false)
        {
            var parametro = new Dictionary<string, object?> { { "p_codigo", codigo } };
            var resposta = await _supabase.Client!.Rpc("calibragem_feita", parametro);
            if (resposta.Content == "true")
            {
                calibragem_feita = true;
                Abanar_necessario.IsVisible = false;
                interferencia = false;
                passou_interferencia = true;
                atualizar_pos();
                break;
            }
            else
                await Task.Delay(500);
        }
    }


    private void btn_fechar(object sender, EventArgs e)
    {
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
    }

    private void btn_fechar2(object sender, EventArgs e)
    {
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
    }


}
