using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using Projeto_Jogo_Labirinto.Services;
using System.Text.Json;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Projeto_Jogo_Labirinto
{
    public partial class MainPage : ContentPage
    {
        private IAudioPlayer musica_espera;
        private IAudioPlayer click_som;
        private IAudioManager _audioManager = AudioManager.Current;

        private readonly SupabaseService _supabase = new SupabaseService();
        private Task _supabaseInitializationTask = null!;
        string codigo = "";

        private string minhaFuncao = "";
        bool clicouSair = false;

        public MainPage()
        {
            InitializeComponent();
            DeviceDisplay.Current.KeepScreenOn = true;
            _supabaseInitializationTask = InicializarSupabaseAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (musica_espera == null)
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("musica_espera.mp3");
                musica_espera = _audioManager.CreatePlayer(stream);
                musica_espera.Volume = 0.5;
            }
            if (click_som == null)
            {
                var stream2 = await FileSystem.OpenAppPackageFileAsync("click_som.mp3");
                click_som = _audioManager.CreatePlayer(stream2);
            }
        }

        private async Task InicializarSupabaseAsync()
        {
            await _supabase.InitializeAsync();

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
        }


        private async void btn_Criar_Sala_Clicked(object sender, EventArgs e)
        {

            click_som.Play();
            btn_Criar_Sala.IsEnabled = false;
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                return;
            }
            try
            {
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

        
        private async void btn_Sim_Clicked(object sender, EventArgs e)
        {
            click_som.Play();
            musica_espera.Stop();
            PaginaPrincipal.IsVisible = true;
            CriarSalaView.IsVisible = false;
            CriarSalaView.Opacity = 0;
            PaginaPrincipal.Opacity = 1;
            clicouSair = true;
            btn_Criar_Sala.IsEnabled = true;

            btn_Guia.BorderWidth = 2;
            btn_Guia.FontAttributes = FontAttributes.None;
            btn_Guia.Opacity = 1;
            btn_Agente.BorderWidth = 2;
            btn_Agente.FontAttributes = FontAttributes.None;
            btn_Agente.Opacity = 1;
            lbl_Aguardar.Text = "";
            btn_Agente.IsEnabled = true;
            btn_Guia.IsEnabled = true;

            fechar_sala.IsVisible = false;

            var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
            var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client.Rpc("eliminar_jogador", parametroJogador);
        }

        private void btn_Nao_Clicked(object sender, EventArgs e)
        {
            click_som.Play();
            fechar_sala.IsVisible = false;
        }
        private void btn_Voltar(object sender, EventArgs e)
        {
            click_som.Play();
            fechar_sala.IsVisible = true;
        }

        private async void btn_Voltar2(object sender, EventArgs e)
        {
            click_som.Play();
            PaginaPrincipal.IsVisible = true;
            InserirCodigoView.IsVisible = false;
            InserirCodigoView.Opacity = 0;
            PaginaPrincipal.Opacity = 1;
        }
        private async void btn_Voltar3(object sender, EventArgs e)
        {
            click_som.Play();
            musica_espera.Stop();
            PaginaPrincipal.IsVisible = true;
            ProcurarSalaView.IsVisible = false;
            ProcurarSalaView.Opacity = 0;
            PaginaPrincipal.Opacity = 1;
            clicouSair = true;
            lbl_Aguardar2.Text = "";
            lbl_funcaoProcurar.Text = "";
            btn_Procurar_Sala.IsEnabled = true;

            var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client!.Rpc("eliminar_sala", parametroSala);
            var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo } };
            await _supabase.Client.Rpc("eliminar_jogador", parametroJogador);

        }

        private async void btnGuia_Click(object sender, EventArgs e)
        {
            click_som.Play();
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
            click_som.Play();
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
            musica_espera.Play();
            bool comecar = false;

            while(comecar == false || clicouSair != true)
            {
                var parametro = new Dictionary<string, object?> { { "p_codigo", codigo }};
                var resposta = await _supabase.Client!.Rpc("comecar", parametro);
                if (resposta.Content == "true")
                {
                    comecar = true;
                    await Task.Delay(1800);
                    IniciarJogo();
                    clicouSair = false;
                    musica_espera.Stop();
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
            click_som.Play();
            PaginaPrincipal.IsVisible = false;
            InserirCodigoView.IsVisible = true;
            InserirCodigoView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
        }

        private async void btn_Iniciar_Protocolo(object sender, EventArgs e)
        {
            click_som.Play();
            btn_Entrar.IsEnabled = true;
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
                var parametro = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var resposta = await _supabase.Client!.Rpc("entrar_em_sala", parametro);

                if (resposta.Content == "false" || string.IsNullOrEmpty(resposta.Content))
                {
                    codigo_errado.IsVisible = true;
                    await Task.Delay(3000);
                    codigo_errado.IsVisible = false;
                    return;
                }
                btn_Entrar.IsEnabled = false;
                var parametroFuncao = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var respostaFuncao = await _supabase.Client.Rpc("qual_funcao", parametroFuncao);
                string funcao = respostaFuncao.Content?.Trim('"') ?? "";

                if (funcao == "Guia")
                {
                    lbl_funcao.Text = "Agente";
                    minhaFuncao = "Agente";
                    await Task.Delay(2000);
                    IniciarJogo();
                }

                else if (funcao == "Agente")
                {
                    lbl_funcao.Text = "Guia";
                    minhaFuncao = "Guia";
                    await Task.Delay(2000);
                    IniciarJogo();
                }
                //await EntrarModoEsperaAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao entrar na sala: {ex.Message}", "OK");
            }
        }
        private void IniciarJogo()
        {
            PaginaPrincipal.IsVisible = true;
            CriarSalaView.IsVisible = false;
            InserirCodigoView.IsVisible = false;
            ProcurarSalaView.IsVisible = false;
            if (minhaFuncao == "Guia")
                Navigation.PushAsync(new PageGuia(codigo));
            else
                Navigation.PushAsync(new PageAgente(codigo));
        }

        private async void btn_Procurar_Sala_Clicked(object sender, EventArgs e)
        {
            click_som.Play();
            PaginaPrincipal.IsVisible = false;
            ProcurarSalaView.IsVisible = true;
            ProcurarSalaView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
            btn_Procurar_Sala.IsEnabled = false;

            try
            {
                var resposta = await _supabase.Client!.Rpc("entrar_sala_publica", null);

                var json = JsonDocument.Parse(resposta.Content);
                var root = json.RootElement;

                codigo = root.GetProperty("codigo").GetString()!;
                bool criado = root.GetProperty("criado").GetBoolean();

                codigo = codigo.Trim('"');

                if (criado == true)
                {
                    lbl_funcaoProcurar.Text = "Guia";
                    minhaFuncao = "Guia";
                    var parametroJogador = new Dictionary<string, object?> {{ "p_codigo", codigo }, { "p_funcao", "Guia" }};
                    await _supabase.Client.Rpc("criar_jogador", parametroJogador);
                    await Task.Delay(2000);
                    IniciarJogo();
                }
                if (criado == false)
                {
                    lbl_funcaoProcurar.Text = "Agente";
                    minhaFuncao = "Agente";
                    lbl_Aguardar2.Text = "A aguardar pelo guia...";
                    var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo }, { "p_funcao", "Agente" } };
                    await _supabase.Client.Rpc("criar_jogador", parametroJogador);
                    comecarJogo();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", "Não foi possível procurar protocolo. Tente novamente.", "Ok");
            }
        }
    }
}