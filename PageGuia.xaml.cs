using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using Projeto_Jogo_Labirinto.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projeto_Jogo_Labirinto;

public partial class PageGuia : ContentPage
{
    string codigo = "";
    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;

    private IAudioPlayer click_som;
    private IAudioManager _audioManager = AudioManager.Current;

    int tempo = 180;
    IDispatcherTimer timer;
    TimeSpan t;

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
    bool porta_resolvida = false;
    bool interferencia = false;
    int total_agitar = 0;
    bool passou_interferencia = false;
    bool esta_no_morse = false;

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    Image mira;

    protected override async void OnAppearing()
	{
		base.OnAppearing();

        if (click_som == null)
        {
            var stream2 = await FileSystem.OpenAppPackageFileAsync("click_som.mp3");
            click_som = _audioManager.CreatePlayer(stream2);
        }

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

    private void IniciarTimer()
    {
        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        t = TimeSpan.FromSeconds(tempo);

        timer.Tick += (s, e) =>
        {
            if (tempo > 0)
            {
                tempo--;

                t = TimeSpan.FromSeconds(tempo);
                lbl_Tempo_execucao.Text = t.ToString(@"mm\:ss");
                if(tempo >= 120)
                {
                    lbl_Tempo_execucao.TextColor = Colors.Green;
                }
                else if (tempo >= 60)
                {
                    lbl_Tempo_execucao.TextColor = Colors.Yellow;
                }
                else if (tempo >= 0)
                {
                    lbl_Tempo_execucao.TextColor = Colors.Red;
                }
            }
            else
            {
                timer.Stop();
                labirinto = false;
                invertido = false;
                esta_na_porta = false;
                entrou_na_porta = false;
                interferencia = false;
                total_agitar = 0;
                passou_interferencia = false;
                esta_no_morse = false;
                porta_resolvida = false;

                tempo = 180;
                Tempo_acabou.IsVisible = true;
            }
        };
        timer.Start();
    }

    private async void btn_Recomecar_Clicked (object? sender, EventArgs e)
    {
        click_som.Play(); 
        var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 1 }};
        try
        {
            await _supabase.Client!.Rpc("atualizar_digito", parametros);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
        Grid.SetColumn(mira, 1);
        Grid.SetRow(mira, 6);

        lbl_Tempo_execucao.Text = TimeSpan.FromSeconds(tempo).ToString(@"mm\:ss");
        lbl_Tempo_execucao.TextColor = Colors.Green;
        Tempo_acabou.IsVisible = false;

        IniciarTimer();
    }
    private async void btn_Pagina_inicial_Clicked(object? sender, EventArgs e)
    {
        click_som.Play();
        Application.Current.MainPage = new NavigationPage(new MainPage());

        var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 2 } };
        try
        {
            await _supabase.Client!.Rpc("atualizar_digito", parametros);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
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
        IniciarTimer();
        Tempo.IsVisible = true;
    }

    private void btn_Avancar_Clicked(object? sender, EventArgs e)
    {
        click_som.Play();
        botao.IsVisible = false;
        Video1_MediaEnded(sender, e);
    }

    private async Task atualizar_pos()
        {
        while (esta_na_porta == false && interferencia == false && esta_no_morse == false)
        {
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo }};
            var resposta = await _supabase.Client!.Rpc("obter_posicao", parametro);

            var json = JsonDocument.Parse(resposta.Content);
            var root = json.RootElement;

            int posX = root.GetProperty("posX").GetInt32();
            int posY = root.GetProperty("posY").GetInt32();

            MapaGrid.Children.Remove(mira);
            MapaGrid.Children.Add(mira);

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
                controlos_invertidos.IsVisible = true;
                await Task.Delay(5000);
                controlos_invertidos.IsVisible = false;

            }
            else if(Grid.GetColumn(mira) == 8 && Grid.GetRow(mira) == 3)
            {
                invertido = true;
                controlos_invertidos.IsVisible = true;
                await Task.Delay(5000);
                controlos_invertidos.IsVisible = false;
            }
            else if(Grid.GetColumn(mira) == 6 && Grid.GetRow(mira) == 9)
            {
                invertido = true;
                controlos_invertidos.IsVisible = true;
                await Task.Delay(5000);
                controlos_invertidos.IsVisible = false;
            }
        }
        if (invertido == true)
        {
            if (Grid.GetColumn(mira) == 4 && Grid.GetRow(mira) == 3)
            {
                invertido = false;
                controlos_normais.IsVisible = true;
                await Task.Delay(5000);
                controlos_normais.IsVisible = false;
            }
            else if (Grid.GetColumn(mira) == 8 && Grid.GetRow(mira) == 2)
            {
                invertido = false;
                controlos_normais.IsVisible = true;
                await Task.Delay(5000);
                controlos_normais.IsVisible = false;
            }
            else if (Grid.GetColumn(mira) == 7 && Grid.GetRow(mira) == 9)
            {
                invertido = false;
                controlos_normais.IsVisible = true;
                await Task.Delay(5000);
                controlos_normais.IsVisible = false;
            }
        }
        if (Grid.GetColumn(mira) == 10 && Grid.GetRow(mira) == 5 && entrou_na_porta == false)
        {
            esta_na_porta = true;
            entrou_na_porta = true;
            porta_resolvida = true;
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
        if (Grid.GetColumn(mira) == 16 && Grid.GetRow(mira) == 9)
        {
            Morse.IsVisible = true;
            esta_no_morse = true;
            verificar_morse();
        }
    }

    private async Task verificar_morse()
    {
        bool morse_feito = false;
        while (morse_feito == false)
        {
            var parametro = new Dictionary<string, object?> {{ "p_codigo", codigo }};
            var resposta = await _supabase.Client!.Rpc("verificar_morse", parametro);
            if (resposta.Content == "true")
            {
                var parametros = new Dictionary<string, object?> { { "p_codigo", codigo }, {"p_tempo_restante", tempo} };
                await _supabase.Client!.Rpc("atualizar_tempo", parametros);
                morse_feito = true;
                Morse.IsVisible = false;
                timer.Stop();
                int tempo_restante = 180 - tempo;
                string tempo_restante_str = TimeSpan.FromSeconds(tempo_restante).ToString(@"mm\:ss");
                await Navigation.PushAsync(new PageFim(tempo_restante_str));
                break;
            }
            else
                await Task.Delay(500);
        }
    }



    private async void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;

        double aceleracao = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        if (aceleracao >= 5)
        {
            total_agitar++;
        }
        if (total_agitar >= 200)
        {
            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("telemovel_abanou", parametro);
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
        click_som.Play();
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
    }

    private void btn_fechar2(object sender, EventArgs e)
    {
        click_som.Play();
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}
