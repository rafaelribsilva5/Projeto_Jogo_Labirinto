using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using Projeto_Jogo_Labirinto.Services;
using Microsoft.Maui.Networking;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projeto_Jogo_Labirinto;

public partial class PageAgente : ContentPage
{
    private IAudioPlayer click_som;
    private IAudioManager _audioManager = AudioManager.Current;
    private CancellationTokenSource _gameCancellation;

    string codigo = "";
    int digito = 0;
    int posX = 1;
    int posY = 6;
    bool invertido = false;
    bool esta_na_porta = false;
    bool porta_resolvida = false;
    bool agitar_resolvido = false;
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

        Connectivity.Current.ConnectivityChanged += Connectivity_ConnectivityChanged;
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



    protected override async void OnAppearing()
	{
		base.OnAppearing();

        _gameCancellation = new CancellationTokenSource();

        if (click_som == null)
        {
            var stream2 = await FileSystem.OpenAppPackageFileAsync("click_som.mp3");
            click_som = _audioManager.CreatePlayer(stream2);
        }
    }

	private async void Video1_MediaEnded(object? sender, EventArgs e)
	{
        video1.Stop();
        video1.Source = null;
        video1.Handler?.DisconnectHandler();
        MostrarLabirinto();
        botao.IsVisible = false;
        await verificar_digito(_gameCancellation.Token);
    }

    private void btn_Avancar_Clicked(object? sender, EventArgs e)
    {
        click_som.Play();
        botao.IsVisible = false;
        Video1_MediaEnded(sender, e);
    }

    private void Video1_MediaFailed(object? sender, CommunityToolkit.Maui.Core.MediaFailedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"[PageAgente] Vídeo falhou: {e?.ErrorMessage}");
		MostrarLabirinto();
	}


private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                sem_net.IsVisible = true;
                await Task.Delay(10000);
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 3 } };
                    await _supabase.Client!.Rpc("atualizar_digito", parametros);

                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    sem_net.IsVisible = false;
                    return;
                }
            });
        }
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
        click_som.Play();
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
        click_som.Play();
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
        click_som.Play();
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
        click_som.Play();
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


    private async Task analise()
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

        if (posX == 10 && posY == 5 && porta_resolvida == false)
        {
            porta_resolvida = true;
            await Navigation.PushAsync(new PageAgentePorta(codigo));
        }
        if (posX == 14 && posY == 1 && agitar_resolvido == false)
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
            await mostrar_morse(_gameCancellation.Token);
            esta_no_morse = true;
        }
    }

    private async void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        if (_gameCancellation?.IsCancellationRequested == true)
        {
            return;
        }

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
            await esperar_calibragem(_gameCancellation.Token);
        }
    }

    private async Task esperar_calibragem(CancellationToken token)
    {
        bool calibragem_feita = false;
        while (calibragem_feita == false)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var parametro = new Dictionary<string, object?> { { "p_codigo", codigo } };
            var resposta = await _supabase.Client!.Rpc("calibragem_feita", parametro);
            if (resposta.Content == "true")
            {
                calibragem_feita = true;
                btn_baixo.IsEnabled = true;
                btn_cima.IsEnabled = true;
                btn_esquerda.IsEnabled = true;
                btn_direita.IsEnabled = true;
                agitar_resolvido = true;
                break;
            }
            else
                await Task.Delay(500, token);
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

    private async Task mostrar_morse(CancellationToken token)
    {
        var beep_morse = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("beep_morse.mp3"));
        try
        {
            token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        await Task.Delay(4000, token);
        Contagem.IsVisible = true;
        lbl_Contagem.Text = "COMEÇA EM";
        await Task.Delay(1000, token);
        lbl_Contagem.Text = "3";
        await Task.Delay(1000, token);
        lbl_Contagem.Text = "2";
        await Task.Delay(1000, token);
        lbl_Contagem.Text = "1";
        await Task.Delay(1000, token);
        Contagem.IsVisible = false;
        foreach (string simbolo in morse)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (simbolo == "--")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500, token);
                await Flashlight.TurnOffAsync();
                await Task.Delay(300, token);
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500, token);
                await Flashlight.TurnOffAsync();
            }
            else if (simbolo == ".")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300, token);
                await Flashlight.TurnOffAsync();
            }
                
            else if (simbolo == "..")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300, token);
                await Flashlight.TurnOffAsync();
                await Task.Delay(300, token);
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(300, token);
                await Flashlight.TurnOffAsync();
            }
            else if (simbolo == "-")
            {
                beep_morse.Play();
                await Flashlight.TurnOnAsync();
                await Task.Delay(1500, token);
                await Flashlight.TurnOffAsync();
            }
            await Task.Delay(2300, token);
        }
        await Task.Delay(2300, token);
        string mensagem_morse = "";
        try
        {
            token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            return;
        }
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
                try
                {
                    token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var parametr = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("verificar_tempo", parametr);
                tempo_restante = int.Parse(resposta.Content);
                
                if (tempo_restante != 900)
                {
                    tempo_restante = 180 - tempo_restante;
                    string tempo_restante_str = TimeSpan.FromSeconds(tempo_restante).ToString(@"mm\:ss");
                    await Navigation.PushAsync(new PageFim(tempo_restante_str));
                    var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                    await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
                    break;
                }

                else
                {
                    await Task.Delay(400, token);
                }
            }
        }
        else
        {
            morse_errado.IsVisible = true;
            await Task.Delay(2500, token);
            morse_errado.IsVisible = false;
            correcao_morse = new int[4];
            await mostrar_morse(_gameCancellation.Token);
        }
    }


    private async Task verificar_digito(CancellationToken token)
    {
        while (true)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
            var resposta = await _supabase.Client!.Rpc("qual_digito", parametro);

            digito = int.Parse(resposta.Content);

            analise2();

            await Task.Delay(300, token);
        }
    }


    private async void analise2()
    {
        if (digito == 1)
        {
            posX = 1;
            posY = 6;
            esta_no_morse = false;
            invertido = false;
            esta_na_porta = false;
            total_agitar = 0;
            correcao_morse = new int[4];
            porta_resolvida = false;
            agitar_resolvido = false;
            btn_baixo.Clicked -= BtnBaixo_Clicked;
            btn_cima.Clicked -= BtnCima_Clicked;
            btn_esquerda.Clicked -= BtnEsquerda_Clicked;
            btn_direita.Clicked -= BtnDireita_Clicked;
            btn_baixo.Clicked += BtnBaixo_Clicked;
            btn_cima.Clicked += BtnCima_Clicked;
            btn_esquerda.Clicked += BtnEsquerda_Clicked;
            btn_direita.Clicked += BtnDireita_Clicked;
            /*Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            Accelerometer.Stop();
            _gameCancellation?.Cancel();
            _gameCancellation = new CancellationTokenSource();*/

            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 0 } };
            try
            {
                await _supabase.Client!.Rpc("atualizar_digito", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            await Navigation.PushAsync(new PageAgente(codigo));
            Navigation.RemovePage(this);
        }
        if (digito == 2)
        {
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 0 } };
            try
            {
                await _supabase.Client!.Rpc("atualizar_digito", parametros);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }

            Application.Current.MainPage = new NavigationPage(new MainPage());

            var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
        }
        if (digito == 3)
        {
            parceiro_sem_net.IsVisible = true;
            await Task.Delay(2000);
            Application.Current.MainPage = new NavigationPage(new MainPage());
            var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _gameCancellation?.Cancel();

        Accelerometer.Stop();
        Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;

        Connectivity.Current.ConnectivityChanged -= Connectivity_ConnectivityChanged;
    }
}
