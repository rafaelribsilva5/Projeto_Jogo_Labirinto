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
        bool porta_resolvida = false;

        while (porta_resolvida == false)
        {
            var parametro = new Dictionary<string, object?> {{ "p_codigo", codigo }};
            var resposta = await _supabase.Client!.Rpc("porta_aberta", parametro);
            if (resposta.Content == "true")
            {
                porta_resolvida = true;
                await Navigation.PopAsync();
                break;
            }
            else
                await Task.Delay(500);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}