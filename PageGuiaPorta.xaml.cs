using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using System.Threading.Tasks;
using System.Text.Json;

namespace Projeto_Jogo_Labirinto;

public partial class PageGuiaPorta : ContentPage
{
	string codigo = "";
    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;
    bool porta_resolvida = false;
    public PageGuiaPorta(string codigoo)
	{
		InitializeComponent();
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        _ = continuarLabirinto();
    }

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    sem_net.IsVisible = true;
                    await Task.Delay(10000);
                    if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                    {
                        porta_resolvida = true;
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    }
                    else
                    {
                        sem_net.IsVisible = false;
                    }
                }
                catch (Exception ex)
                {
                    porta_resolvida = true;
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
            });
        }
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            sem_net.IsVisible = false;
            _supabaseInitializationTask = InicializarSupabaseAsync();
        }
    }

    private async Task continuarLabirinto()
    {
        await Task.Delay(5000);

        while (porta_resolvida == false)
        {
            try
            {
                var parametro = new Dictionary<string, object?> {{ "p_codigo", codigo }};
                var resposta = await _supabase.Client!.Rpc("porta_aberta", parametro);
                if (resposta.Content == "true")
                {
                    porta_resolvida = true;
                    MainThread.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
                    break;
                }
            }
            catch (Exception ex)
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    _supabaseInitializationTask = InicializarSupabaseAsync();
                    await _supabaseInitializationTask;
                }
            }
            await Task.Delay(500);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}