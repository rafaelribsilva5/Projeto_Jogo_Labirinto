using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using System.Threading.Tasks;
using System.Text.Json;

namespace Projeto_Jogo_Labirinto;

public partial class PageAgentePorta : ContentPage
{
    string codigo = "";
    private readonly SupabaseService _supabase = new SupabaseService();
    private Task _supabaseInitializationTask = null!;
    int codigo_porta = 7;
    int codigo_porta_anterior = 7;
    public PageAgentePorta(string codigoo)
	{
		InitializeComponent();
        DeviceDisplay.Current.KeepScreenOn = true;
        codigo = codigoo;
        _supabaseInitializationTask = InicializarSupabaseAsync();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        criar_sequencia();
    }

    private async Task InicializarSupabaseAsync()
    {
        await _supabase.InitializeAsync();
    }

    string[] simbolos = new string[] { "⟡", "⌬", "⟁", "⟢", "⟠", "⌘", "⌖", "⧖", "⌭", "⥀", "⟲", "⟴", "⧗"};
    int[] sequencia = new int[5];
    int total = 1;


    string codigo_encriptado = "";
    private void criar_sequencia()
    {
        
        codigo_encriptado = "⟡";
        Random rnd = new Random();
        sequencia[0] = 0;
        for (int i = 1; i < 5; i++)
        {
            int num = rnd.Next(1, 13);
            codigo_encriptado += simbolos[num];
            sequencia[i] = num;
            
            if (num == 1)
            {
                simbolo1();
            }
            else if (num == 2)
            {
                simbolo2();
            }
            else if (num == 3)
            {
                simbolo3();
            }
            else if (num == 4)
            {
                simbolo4();
            }
            else if (num == 5)
            {
                simbolo5();
            }
            else if (num == 6)
            {
                simbolo6();
            }
            else if (num == 7)
            {
                simbolo7();
            }
            else if (num == 8)
            {
                simbolo8();
            }
            else if (num == 9)
            {
                simbolo9();
            }
            else if (num == 10)
            {
                simbolo10();
            }
            else if (num == 11)
            {
                simbolo11();
            }
            else if (num == 12)
            {
                simbolo12();
            }
            total += 1;
        }
        lbl_CodigoEncriptado.Text = codigo_encriptado;
    }

    private void simbolo1()
    {
        codigo_porta_anterior = codigo_porta;
        codigo_porta = (codigo_porta * codigo_porta);
    }
    private void simbolo2()
    {
        codigo_porta_anterior = codigo_porta;
        codigo_porta -= total;
    }
    private void simbolo3()
    {
        codigo_porta_anterior = codigo_porta;
        if (codigo_porta % 2 == 0)
        {
            codigo_porta /= 2;
        }
        else
        {
            codigo_porta *= 2;
        }
    }
    private void simbolo4()
    {
        string codigo_porta_str = codigo_porta.ToString();
        if (codigo_porta_str.Length <= 1)
        {
            return;
        }

        if (total >= 2)
        {
            codigo_porta_anterior = codigo_porta;
            char primeiro = codigo_porta_str[0];
            char ultimo = codigo_porta_str[codigo_porta_str.Length - 1];
            codigo_porta_str = ultimo + codigo_porta_str.Substring(1, codigo_porta_str.Length - 2) + primeiro;
            codigo_porta = int.Parse(codigo_porta_str);
        }
        
    }
    private void simbolo5()
    {
        codigo_porta_anterior = codigo_porta;
        string codigo_porta_str = codigo_porta.ToString();
        codigo_porta_str = new string(codigo_porta_str.Reverse().ToArray());
        codigo_porta = int.Parse(codigo_porta_str);
    }
    private void simbolo6()
    {
        codigo_porta_anterior = codigo_porta;
        int diferentes = 0;
        int[] lista_unicos = new int[5] {20, 20, 20, 20, 20};
        for (int j = 0; j < sequencia.Length; j++)
        {
            if (!lista_unicos.Contains(sequencia[j]))
            {
                lista_unicos[diferentes] = sequencia[j];
                diferentes++;
            }
        }
        codigo_porta *= diferentes;
    }
    private void simbolo7()
    {
        codigo_porta_anterior = codigo_porta;
        if (codigo_porta > 20)
        {
            codigo_porta -= 7;
        }
        else
        {
            codigo_porta += 5;
        }
    }
    private void simbolo8()
    {
        codigo_porta_anterior = codigo_porta;
        if (total >= 2)
        {
            int qual = sequencia[sequencia.Length - 2];
            string evento = "simbolo" + qual;
            var metodo = this.GetType().GetMethod(evento, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (metodo != null)
            {
                metodo.Invoke(this, null);
            }
                
        }
    }
    private void simbolo9()
    {
        codigo_porta = codigo_porta_anterior;
        
    }
    private void simbolo10()
    {
        codigo_porta_anterior = codigo_porta;
        string codigo_porta_str = codigo_porta.ToString();
        char primeiro = codigo_porta_str[0];
        codigo_porta_str = codigo_porta_str.Substring(1, codigo_porta_str.Length - 1) + primeiro;
        codigo_porta = int.Parse(codigo_porta_str);
    }

    private void simbolo11()
    {
        codigo_porta_anterior = codigo_porta;
        string codigo_porta_str = codigo_porta.ToString();
        codigo_porta = 0;
        for (int i = 0; i < codigo_porta_str.Length; i++)
        {
            int digito = int.Parse(codigo_porta_str[i].ToString());
            codigo_porta += digito;
        }
    }

    private void simbolo12()
    {
        if (total == 4)
        {
            codigo_porta *= 3;
        }
    }

    private async void btn_Verificar_Clicked(object sender, EventArgs e)
    {
        if (txt_CodigoPorta.Text == codigo_porta.ToString())
        {
            await DisplayAlert("Sucesso", "Código correto! A porta se abriu.", "OK");
            await Task.Delay(1000);
            var parametro = new Dictionary<string, object> { { "p_codigo", codigo}};
            var resposta = await _supabase.Client!.Rpc("porta_resolucao", parametro);
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Erro", "Código incorreto! Tente novamente.", "OK");
        }
    }
}