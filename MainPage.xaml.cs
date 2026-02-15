using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using System.Threading.Tasks;

namespace Projeto_Jogo_Labirinto
{
    public partial class MainPage : ContentPage
    {
        private readonly SupabaseService _supabase = new SupabaseService();
        private Task _supabaseInitializationTask = null!;
        private string codigo = "";

        private readonly SalaRealtimeService _salaRealtimeService;
        private string minhaFuncao = "";
        bool clicouSair = false;

        public MainPage()
        {
            InitializeComponent();
            _supabaseInitializationTask = InicializarSupabaseAsync();
            _salaRealtimeService = new SalaRealtimeService(GarantirSupabaseProntoAsync);
        }

        private async Task InicializarSupabaseAsync()
        {
            await _supabase.InitializeAsync();
        }

        private async Task<Supabase.Client?> GarantirSupabaseProntoAsync()
        {
            await _supabaseInitializationTask;
            if (_supabase.Client == null)
                throw new InvalidOperationException("Supabase não foi inicializado corretamente.");
            return _supabase.Client;
        }

        private async void btn_Criar_Sala(object sender, EventArgs e)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                return;
            }
            try
            {
                await GarantirSupabaseProntoAsync();
                var response = await _supabase.Client!.Rpc("gerar_sala_codigo", null);
                codigo = response.Content?.Trim('"') ?? "";
                lbl_Codigo.Text = codigo;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao criar sala: {ex.Message}", "OK");
                return;
            }

            PaginaPrincipal.IsVisible = false;
            CriarSalaView.IsVisible = true;
            CriarSalaView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
        }

        private async void btn_Voltar(object sender, EventArgs e)
        {
            bool sair = await DisplayAlert("Confirmar", "Deseja sair da sala?", "Sim", "Não");
            if (!sair) return;

            PaginaPrincipal.IsVisible = true;
            CriarSalaView.IsVisible = false;
            CriarSalaView.Opacity = 0;
            PaginaPrincipal.Opacity = 1;
            clicouSair = true;

            //await SairDoModoEspera();

            btn_Guia.BorderWidth = 2;
            btn_Guia.FontAttributes = FontAttributes.None;
            btn_Guia.Opacity = 1;
            btn_Agente.BorderWidth = 2;
            btn_Agente.FontAttributes = FontAttributes.None;
            btn_Agente.Opacity = 1;
            lbl_Aguardar.Text = "";
            btn_Agente.IsEnabled = true;
            btn_Guia.IsEnabled = true;
        }

        private async void btn_Voltar2(object sender, EventArgs e)
        {
            //await SairDoModoEspera();
            PaginaPrincipal.IsVisible = true;
            InserirCodigoView.IsVisible = false;
            InserirCodigoView.Opacity = 0;
            PaginaPrincipal.Opacity = 1;
        }

        private async void btnGuia_Click(object sender, EventArgs e)
        {
            btn_Agente.IsEnabled = false;
            btn_Guia.IsEnabled = false;
            btn_Guia.BorderWidth = 4;
            btn_Guia.FontAttributes = FontAttributes.Bold;
            btn_Agente.Opacity = 0.4;
            lbl_Aguardar.Text = "A aguardar pelo agente...";

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                ReativarBotoesAguardar();
                return;
            }
            try
            {
                await GarantirSupabaseProntoAsync();
                var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                await _supabase.Client!.Rpc("criar_sala_privada", parametroSala);
                var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo }, { "p_funcao", "Guia" } };
                await _supabase.Client.Rpc("criar_jogador", parametroJogador);

                minhaFuncao = "Guia";
                comecarJogo();
                //await EntrarModoEsperaAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
                ReativarBotoesAguardar();
            }
        }

        private async void btnAgente_Click(object sender, EventArgs e)
        {
            btn_Agente.IsEnabled = false;
            btn_Guia.IsEnabled = false;
            btn_Agente.BorderWidth = 4;
            btn_Agente.FontAttributes = FontAttributes.Bold;
            btn_Guia.Opacity = 0.4;
            lbl_Aguardar.Text = "A aguardar pelo guia...";

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                ReativarBotoesAguardar();
                return;
            }
            try
            {
                await GarantirSupabaseProntoAsync();
                var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                await _supabase.Client!.Rpc("criar_sala_privada", parametroSala);
                var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo }, { "p_funcao", "Agente" } };
                await _supabase.Client.Rpc("criar_jogador", parametroJogador);

                minhaFuncao = "Agente";
                comecarJogo();
                //await EntrarModoEsperaAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
                ReativarBotoesAguardar();
            }
        }


        private async Task comecarJogo()
        {
            bool comecar = false;

            while(comecar == false || clicouSair != true)
            {
                var parametro = new Dictionary<string, object?> { { "p_codigo", codigo }};
                var resposta = await _supabase.Client!.Rpc("comecar", parametro);
                if (resposta.Content == "true")
                {
                    comecar = true;
                    IniciarJogo();
                    clicouSair = false;
                    break;
                }
                    
                else
                    await Task.Delay(500);
            }
        }

        private void ReativarBotoesAguardar()
        {
            lbl_Aguardar.Text = "";
            btn_Agente.IsEnabled = true;
            btn_Guia.IsEnabled = true;
            btn_Guia.BorderWidth = 2;
            btn_Guia.FontAttributes = FontAttributes.None;
            btn_Agente.Opacity = 1;
            btn_Agente.BorderWidth = 2;
            btn_Agente.FontAttributes = FontAttributes.None;
            btn_Guia.Opacity = 1;
        }

        private void btn_Inserir_Codigo(object sender, EventArgs e)
        {
            PaginaPrincipal.IsVisible = false;
            InserirCodigoView.IsVisible = true;
            InserirCodigoView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
        }

        private async void btn_Iniciar_Protocolo(object sender, EventArgs e)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                return;
            }
            codigo = txt_Codigo.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(codigo))
            {
                await DisplayAlert("Código vazio", "Por favor, insira o código da sala.", "OK");
                return;
            }
            try
            {
                await GarantirSupabaseProntoAsync();
                var parametro = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("entrar_em_sala", parametro);

                if (resposta.Content == "false" || string.IsNullOrEmpty(resposta.Content))
                {
                    await DisplayAlert("Código inválido", "O código inserido não corresponde a nenhuma sala ativa. Por favor, verifique o código ou tente novamente.", "OK");
                    return;
                }
                var parametroFuncao = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var respostaFuncao = await _supabase.Client.Rpc("qual_funcao", parametroFuncao);
                string funcao = respostaFuncao.Content?.Trim('"') ?? "";

                if (funcao == "Guia")
                {
                    lbl_funcao.Text = "Agente";
                    await Task.Delay(2000);
                    IniciarJogo();
                }

                else if (funcao == "Agente")
                {
                    lbl_funcao.Text = "Guia";
                    await Task.Delay(2000);
                    IniciarJogo();
                }

                minhaFuncao = funcao == "Guia" ? "Agente" : "Guia";
                //await EntrarModoEsperaAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao entrar na sala: {ex.Message}", "OK");
            }
        }
        private void IniciarJogo()
        {
            if (minhaFuncao == "Guia")
                Navigation.PushAsync(new PageGuia(codigo));
            else
                Navigation.PushAsync(new PageAgente(codigo));
        }

        private void btn_Procurar_Sala(object sender, EventArgs e)
        {
            PaginaPrincipal.IsVisible = false;
            ProcurarSalaView.IsVisible = true;
            ProcurarSalaView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
        }
    }
}