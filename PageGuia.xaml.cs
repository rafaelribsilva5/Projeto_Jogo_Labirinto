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
    private CancellationTokenSource _gameCancellation;

    private IAudioPlayer click_som;
    private IAudioManager _audioManager = AudioManager.Current;

    int tempo = 180;
    int digito = 0;
    IDispatcherTimer timer;
    TimeSpan t;

    public PageGuia(string codigoo)
	{
		InitializeComponent();
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
        Connectivity.Current.ConnectivityChanged += Connectivity_ConnectivityChanged;
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

        _gameCancellation = new CancellationTokenSource();

        try
        {
            await _supabaseInitializationTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PageGuia] Erro ao inicializar Supabase: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível conectar. Tente novamente.", "OK");
            await Navigation.PopAsync();
            return;
        }

        try
        {
            if (click_som == null)
            {
                var stream2 = await FileSystem.OpenAppPackageFileAsync("click_som.mp3");
                click_som = _audioManager.CreatePlayer(stream2);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PageGuia] Erro ao carregar áudio: {ex.Message}");
        }

        if (labirinto == true)
        {
            esta_na_porta = false;
            _ = atualizar_pos(_gameCancellation.Token);
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
        AnimarMira();
        AnimarEscala();

        labirinto = true;
        var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
        await _supabase.Client!.Rpc("ultima_vez_agente", parametro);
        _ = atualizar_pos(_gameCancellation.Token);
        _ = Verificar_agente_online(_gameCancellation.Token);
        _ = Atulaizar_conexao(_gameCancellation.Token);
    }

    private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            timer?.Stop();
            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    sem_net.IsVisible = true;
                    _gameCancellation?.Cancel();
                    await Task.Delay(10000);
                    if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                    {
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    }
                    else
                    {
                        sem_net.IsVisible = false;
                        timer?.Stop();
                        IniciarTimer();

                        _gameCancellation = new CancellationTokenSource();
                        _ = atualizar_pos(_gameCancellation.Token);
                        _ = verificar_morse(_gameCancellation.Token);
                        _ = esperar_calibragem(_gameCancellation.Token);
                        _ = Verificar_agente_online(_gameCancellation.Token);
                        _ = Atulaizar_conexao(_gameCancellation.Token);
                        Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                    }
                }
                catch (Exception ex)
                {
                     Application.Current.MainPage = new NavigationPage(new MainPage());
                }
            });
        }
    }

    private async Task Verificar_agente_online(CancellationToken token)
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

            try
            {
                var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("agente_vivo", parametro);
                bool vivo = bool.Parse(resposta.Content);
                if (vivo == false)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        parceiro_sem_net.IsVisible = true;
                        await Task.Delay(2000);
                        var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                        await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                        return;
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try { await Task.Delay(5000, token); } catch (OperationCanceledException) { return; }
        }
    }

    private async Task Atulaizar_conexao(CancellationToken token)
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
            try
            {
                var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
                await _supabase.Client!.Rpc("ultima_vez_guia", parametro);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PageGuia] atualizar_conexao: {ex.Message}");
            }

            try { await Task.Delay(5000, token); } catch (OperationCanceledException) { return; }
        }
    }
    private async void AnimarMira()
    {
        while (true)
        {
            await Task.WhenAll(
                mira.RotateTo(360, 5000)
            );

            mira.Rotation = 0;
        }
    }

    private async void AnimarEscala()
    {
        while (true)
        {
            await mira.ScaleTo(1.2, 800);
            await mira.ScaleTo(1, 800);
        }
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
                controlos_invertidos.IsVisible = false;
                controlos_normais.IsVisible = false;
                Abanar_necessario.IsVisible = false;
                Morse.IsVisible = false;
                Tempo_acabou.IsVisible = false;
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();


                tempo = 180;
                Tempo_acabou.IsVisible = true;
            }
        };
        timer.Start();
    }

    private void TocarClickSom()
    {
        click_som.Play();
    }

    private async void btn_Recomecar_Clicked (object? sender, EventArgs e)
    {
        TocarClickSom();

        timer?.Stop();
        _gameCancellation?.Cancel();

        var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 1 }};
        await _supabase.Client!.Rpc("atualizar_digito", parametros);

        var parametross = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_posx", 1 }, { "p_posy", 6 } };
        await _supabase.Client!.Rpc("atualizar_posicao", parametross);

        var parametrosss = new Dictionary<string, object> { { "p_codigo", codigo } };
        await _supabase.Client!.Rpc("reset_estado_sala", parametrosss);

        Tempo_acabou.IsVisible = false;

        await Navigation.PushAsync(new PageGuia(codigo));
        Navigation.RemovePage(this);
    }
    private async void btn_Pagina_inicial_Clicked(object? sender, EventArgs e)
    {
        try { click_som?.Play(); } catch { }
        Application.Current!.MainPage = new NavigationPage(new MainPage());

        try
        {
            var parametros = new Dictionary<string, object> { { "p_codigo", codigo }, { "p_digito", 2 } };
            await _supabase.Client!.Rpc("atualizar_digito", parametros);
            await Task.Delay(500);
            var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PageGuia] Erro ao sair: {ex.Message}");
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
        botao.IsVisible = false;
        Tempo.IsVisible = true;
    }

    private void btn_Avancar_Clicked(object? sender, EventArgs e)
    {
        TocarClickSom();
        botao.IsVisible = false;
        Video1_MediaEnded(sender, e);
    }

    private async Task atualizar_pos(CancellationToken token)
        {
        while (esta_na_porta == false && interferencia == false && esta_no_morse == false)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                var parametro = new Dictionary<string, object> { { "p_codigo", codigo }};
                var resposta = await _supabase.Client!.Rpc("obter_posicao", parametro);

                if (string.IsNullOrEmpty(resposta?.Content))
                    continue;

                var json = JsonDocument.Parse(resposta.Content);
                var root = json.RootElement;

                int posX = root.GetProperty("posX").GetInt32();
                int posY = root.GetProperty("posY").GetInt32();

                Grid.SetColumn(mira, posX);
                Grid.SetRow(mira, posY);

                analise();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PageGuia] atualizar_pos: {ex.Message}");
            }

            try { await Task.Delay(300, token); } catch (OperationCanceledException) { return; }
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
            await verificar_morse(_gameCancellation.Token);
        }
    }

    private async Task verificar_morse(CancellationToken token)
    {
        bool morse_feito = false;
        while (morse_feito == false)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                var parametro = new Dictionary<string, object?> {{ "p_codigo", codigo }};
                var resposta = await _supabase.Client!.Rpc("verificar_morse", parametro);
                if (resposta?.Content == "true")
                {
                    var parametros = new Dictionary<string, object?> { { "p_codigo", codigo }, {"p_tempo_restante", tempo} };
                    await _supabase.Client!.Rpc("atualizar_tempo", parametros);
                    morse_feito = true;
                    Morse.IsVisible = false;
                    timer.Stop();
                    int tempo_restante = 180 - tempo;
                    string tempo_restante_str = TimeSpan.FromSeconds(tempo_restante).ToString(@"mm\:ss");
                    await Navigation.PushAsync(new PageFim(tempo_restante_str));
                    var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                    await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PageGuia] verificar_morse: {ex.Message}");
            }

            try { await Task.Delay(500, token); } catch (OperationCanceledException) { return; }
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
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            try
            {
                var parametro = new Dictionary<string, object> { { "p_codigo", codigo } };
                await _supabase.Client!.Rpc("telemovel_abanou", parametro);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PageGuia] telemovel_abanou: {ex.Message}");
            }
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

            try
            {
                var parametro = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("calibragem_feita", parametro);
                if (resposta?.Content == "true")
                {
                    calibragem_feita = true;
                    Abanar_necessario.IsVisible = false;
                    interferencia = false;
                    passou_interferencia = true;
                    await atualizar_pos(_gameCancellation.Token);
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PageGuia] esperar_calibragem: {ex.Message}");
            }

            try { await Task.Delay(500, token); } catch (OperationCanceledException) { return; }
        }
    }


    private void btn_fechar(object sender, EventArgs e)
    {
        try { click_som?.Play(); } catch { }
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
    }

    private void btn_fechar2(object sender, EventArgs e)
    {
        try { click_som?.Play(); } catch { }
        ControlosCorretos.IsVisible = false;
        ControlosInvertidos.IsVisible = false;
        LabirintoView.Opacity = 1;
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
