using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using Projeto_Jogo_Labirinto.Services;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projeto_Jogo_Labirinto;

public partial class PageAgente : ContentPage
{
     private IAudioManager _audioManager;

    string codigo = "";
    int posX = 1;
    int posY = 6;
    bool invertido = false;
    bool esta_na_porta = false;
    int total_agitar = 0;
    string[] morse = { "--", ".", "--", "--", ".", ".", "-", "..", "--", "..", "-", "--"};
    int[] correcao_morse = new int[4];
    bool esta_no_morse = false;
    public PageAgente(string codigoo)
	{
		InitializeComponent();
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
        _audioManager = AudioManager.Current;
    }

    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    string[,] mapa = new string[14, 24]
{
    { "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5" },
    { "5","1100","0101","0101","0101","0101","0110","1100","0101","0110","1101","0100","0101","0111","1100","0110","1101","0100","0101","0101","0101","0101","0110","5" },
    { "5","1010","1100","0110","1100","0101","0011","1001","0110","1010","1100","0011","1100","0101","0011","1001","0110","1011","1100","0101","0101","0100","0011","5" },
    { "5","1001","0011","1011","1001","0100","0101","0100","0011","1010","1001","0100","0011","1100","0111","1100","0001","0111","0011","1100","0110","1011","1110","5" },
    { "5","1100","0101","0111","1100","0011","1100","0011","1100","0011","1101","0011","1100","0011","1100","0011","1100","0101","0101","0010","5","5","5","5" },
    { "5","1001","0110","1100","0011","1100","0011","1110","1001","0110","1010","1101","0010","1100","0011","1100","0011","1110","1110","5","5","5","5","5" },
    { "5","1101","0010","1001","0110","1001","0110","1010","1100","0011","1001","0100","0011","1001","0110","1001","0110","1010","5","5","5","5","5","5" },
    { "5","1110","1000","0101","0001","0111","1001","0010","1001","0110","1100","0010","1100","0111","1001","0110","1011","5","5","5","5","5","5","5" },
    { "5","1001","0011","1101","0101","0110","1100","0011","1101","0011","1010","1011","1000","0101","0110","1001","0110","5","5","5","5","5","5","5" },
    { "5","1101","0110","1100","0110","1001","0001","0110","1100","0101","0010","1110","1011","1100","0011","1100","0001","0011","5","5","5","5","5","5" },
    { "5","1100","0011","1010","1001","0110","1110","1001","0010","1100","0011","1001","0110","1001","0101","0001","0110","1110","5","5","5","5","5","5" },
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
        video1.Stop();
        video1.Source = null;
        video1.Handler?.DisconnectHandler();
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
        if (esta_no_morse == true)
        {
            correcao_morse[0]++;
            return;
        }
        string quadricula = mapa[posY, posX];
        if (quadricula[0] == '0')
        {
            posX--;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo },{ "p_posx", posX },{ "p_posy", posY} };
            try
            {
                await _supabase.Client!.Rpc("atualizar_posicao", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            analise();
        }
        else
        {
            Mensagem.IsVisible = true;
            Mensagem.Opacity = 1;
            LabirintoView.Opacity = 0.4;
            btn_baixo.IsEnabled = false;
            btn_cima.IsEnabled = false;
            btn_esquerda.IsEnabled = false;
            btn_direita.IsEnabled = false;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
            Mensagem.Opacity = 0;
            LabirintoView.Opacity = 1;
            btn_baixo.IsEnabled = true;
            btn_cima.IsEnabled = true;
            btn_esquerda.IsEnabled = true;
            btn_direita.IsEnabled = true;
        }
    }
    private async void BtnDireita_Clicked(object sender, EventArgs e)
    {
        if (esta_no_morse == true)
        {
            correcao_morse[2]++;
            return;
        }
        string quadricula = mapa[posY, posX];
        if (quadricula[2] == '0')
        {
            posX++;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            try
            {
                await _supabase.Client!.Rpc("atualizar_posicao", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            analise();
        }
        else
        {
            Mensagem.IsVisible = true;
            Mensagem.Opacity = 1;
            LabirintoView.Opacity = 0.4;
            btn_baixo.IsEnabled = false;
            btn_cima.IsEnabled = false;
            btn_esquerda.IsEnabled = false;
            btn_direita.IsEnabled = false;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
            Mensagem.Opacity = 0;
            LabirintoView.Opacity = 1;
            btn_baixo.IsEnabled = true;
            btn_cima.IsEnabled = true;
            btn_esquerda.IsEnabled = true;
            btn_direita.IsEnabled = true;
        }
    }
    private async void BtnCima_Clicked(object sender, EventArgs e)
    {
        if (esta_no_morse == true)
        {
            correcao_morse[1]++;
            return;
        }
        string quadricula = mapa[posY, posX];
        if (quadricula[1] == '0')
        {
            posY--;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            try
            {
                await _supabase.Client!.Rpc("atualizar_posicao", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            analise();
        }
        else
        {
            Mensagem.IsVisible = true;
            Mensagem.Opacity = 1;
            LabirintoView.Opacity = 0.4;
            btn_baixo.IsEnabled = false;
            btn_cima.IsEnabled = false;
            btn_esquerda.IsEnabled = false;
            btn_direita.IsEnabled = false;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
            Mensagem.Opacity = 0;
            LabirintoView.Opacity = 1;
            btn_baixo.IsEnabled = true;
            btn_cima.IsEnabled = true;
            btn_esquerda.IsEnabled = true;
            btn_direita.IsEnabled = true;
        }
    }
    private async void BtnBaixo_Clicked(object sender, EventArgs e)
    {
        if (esta_no_morse == true)
        {
            correcao_morse[3]++;
            return;
        }
        string quadricula = mapa[posY, posX];
        if (quadricula[3] == '0')
        {
            posY++;
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", posX }, { "p_posy", posY } };
            try
            {
                await _supabase.Client!.Rpc("atualizar_posicao", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            analise();
        }
        else
        {
            Mensagem.IsVisible = true;
            Mensagem.Opacity = 1;
            LabirintoView.Opacity = 0.4;
            btn_baixo.IsEnabled = false;
            btn_cima.IsEnabled = false;
            btn_esquerda.IsEnabled = false;
            btn_direita.IsEnabled = false;
            await Task.Delay(2000);
            Mensagem.IsVisible = false;
            Mensagem.Opacity = 0;
            LabirintoView.Opacity = 1;
            btn_baixo.IsEnabled = true;
            btn_cima.IsEnabled = true;
            btn_esquerda.IsEnabled = true;
            btn_direita.IsEnabled = true;
        }
    }


    private async void analise()
    {
        if (invertido == false)
        {
            if (posX == 5 && posY == 3)
            {
                invertido = true;
                mudar_controlos();
            }
            else if (posX == 8 && posY == 3)
            {
                invertido = true;
                mudar_controlos();
            }
            else if (posX == 6 && posY == 9)
            {
                invertido = true;
                mudar_controlos();
            }
        }
        if (invertido == true)
        {
            if (posX == 4 && posY == 3)
            {
                invertido = false;
                mudar_controlos();
            }
            else if (posX == 8 && posY == 2)
            {
                invertido = false;
                mudar_controlos();
            }
            else if (posX == 7 && posY == 9)
            {
                invertido = false;
                mudar_controlos();
            }
        }

        if (posX == 10 && posY == 5)
        {
            esta_na_porta = true;
            await Navigation.PushAsync(new PageAgentePorta(codigo));
        }
        if (posX == 14 && posY == 1)
        {
            btn_baixo.IsEnabled = false;
            btn_cima.IsEnabled = false;
            btn_esquerda.IsEnabled = false;
            btn_direita.IsEnabled = false;
            Accelerometer.Start(SensorSpeed.Game);
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        }
        if (posX == 16 && posY == 9)
        {
            await Task.Delay(15000);
            mostrar_morse();
            esta_no_morse = true;
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
                btn_baixo.IsEnabled = true;
                btn_cima.IsEnabled = true;
                btn_esquerda.IsEnabled = true;
                btn_direita.IsEnabled = true;
                break;
            }
            else
                await Task.Delay(500);
        }
    }
    private void mudar_controlos()
    {
         if (invertido == true)
        {
            btn_cima.Clicked -= BtnCima_Clicked;
            btn_cima.Clicked += BtnBaixo_Clicked;

            btn_baixo.Clicked -= BtnBaixo_Clicked;
            btn_baixo.Clicked += BtnCima_Clicked;

            btn_esquerda.Clicked -= BtnEsquerda_Clicked;
            btn_esquerda.Clicked += BtnDireita_Clicked;

            btn_direita.Clicked -= BtnDireita_Clicked;
            btn_direita.Clicked += BtnEsquerda_Clicked;
        }
        else if (invertido == false)
        {
            btn_cima.Clicked -= BtnBaixo_Clicked;
            btn_cima.Clicked += BtnCima_Clicked;

            btn_baixo.Clicked -= BtnCima_Clicked;
            btn_baixo.Clicked += BtnBaixo_Clicked;

            btn_esquerda.Clicked -= BtnDireita_Clicked;
            btn_esquerda.Clicked += BtnEsquerda_Clicked;

            btn_direita.Clicked -= BtnEsquerda_Clicked;
            btn_direita.Clicked += BtnDireita_Clicked;
        }
    }

    private async Task mostrar_morse()
    {
        var beep_morse = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beep_morse.mp3"));
        await Task.Delay(4000);
        Contagem.IsVisible = true;
        lbl_Contagem.Text = "COMEÇA EM";
        await Task.Delay(1000);
        lbl_Contagem.Text = "3";
        await Task.Delay(1000);
        lbl_Contagem.Text = "2";
        await Task.Delay(1000);
        lbl_Contagem.Text = "1";
        await Task.Delay(1000);
        Contagem.IsVisible = false;
        foreach (string simbolo in morse)
        {
            if (simbolo == "--")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500);
                await Flashlight.TurnOffAsync();
                await Task.Delay(300);
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500);
                await Flashlight.TurnOffAsync();
            }
            else if (simbolo == ".")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300);
                await Flashlight.TurnOffAsync();
            }
                
            else if (simbolo == "..")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300);
                await Flashlight.TurnOffAsync();
                await Task.Delay(300);
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300);
                await Flashlight.TurnOffAsync();
            }
            else if (simbolo == "-")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500);
                await Flashlight.TurnOffAsync();
            }
            await Task.Delay(2300);
        }
        await Task.Delay(2300);
        string mensagem_morse = "";
        for (int i = 0; i < 4; i++)
        {
            mensagem_morse += correcao_morse[i].ToString();
        }
        if (mensagem_morse == "2352")
        {
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo }};
            await _supabase.Client!.Rpc("morse_resolvido", parametro);
            int tempo_restante = 900;
            while (tempo_restante == 900)
            {
                var parametr = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("verificar_tempo", parametr);
                tempo_restante = int.Parse(resposta.Content);
                
                if (tempo_restante != 900)
                {
                    tempo_restante = 180 - tempo_restante;
                    string tempo_restante_str = tempo_restante.ToString(@"mm\:ss");
                    await Navigation.PushAsync(new PageFim(tempo_restante_str));
                }

                else
                {
                    await Task.Delay(400);
                }
            }
        }
        else
        {
            Morse_incorreto.IsVisible = true;
            await Task.Delay(2000);
            Morse_incorreto.IsVisible = false;
            correcao_morse = new int[4];
            await mostrar_morse();
        }
    }
}
