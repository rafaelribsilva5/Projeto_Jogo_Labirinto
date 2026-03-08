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
    public PageGuiaPorta(string codigoo)
	{
		InitializeComponent();
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
        continuarLabirinto();
    }

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    private async Task continuarLabirinto()
    {
        await Task.Delay(5000);

        bool porta_resolvida = false;

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
                System.Diagnostics.Debug.WriteLine($"[PageGuiaPorta] Erro: {ex.Message}");
            }
            await Task.Delay(500);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}