using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Projeto_Jogo_Labirinto.Services;
using Supabase.Realtime;
using Newtonsoft.Json;
using Supabase.Realtime.PostgresChanges;
using System.Threading.Channels;


namespace Projeto_Jogo_Labirinto
{
    public partial class MainPage : ContentPage
    {
        private readonly SupabaseService _supabase = new SupabaseService();
        private RealtimeChannel? _channel;
        public MainPage()
        {
            InitializeComponent();

            var screenWidth = DeviceDisplay.MainDisplayInfo.Width;
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height;
            var density = DeviceDisplay.MainDisplayInfo.Density;

            inicializar_supabase();
        }
        string codigo = "";
        private async void inicializar_supabase()
        {
            await _supabase.InitializeAsync();
        }

        private async void btn_Criar_Sala(object sender, EventArgs e)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                return;
            }
            else
            {
                var response = await _supabase.Client.Rpc("gerar_sala_codigo", null);
                codigo = response.Content;
                codigo = codigo.Trim('"');
                lbl_Codigo.Text = codigo;
            }

            PaginaPrincipal.IsVisible = false;
            CriarSalaView.IsVisible = true;
            CriarSalaView.Opacity = 1;
            PaginaPrincipal.Opacity = 0;
        }
        private async void btn_Voltar(object sender, EventArgs e)
        {
            bool Sair = await DisplayAlert("Confirmar", "Deseja sair da sala?", "Sim", "Não");

            if (!Sair)
            {
                return;
            }
            else
            {
                PaginaPrincipal.IsVisible = true;
                CriarSalaView.IsVisible = false;
                CriarSalaView.Opacity = 0;
                PaginaPrincipal.Opacity = 1;

                btn_Guia.BorderWidth = 2;
                btn_Guia.FontAttributes = FontAttributes.None;
                btn_Guia.Opacity = 1;

                btn_Agente.BorderWidth = 2;
                btn_Agente.FontAttributes = FontAttributes.None;
                btn_Agente.Opacity = 1;

                lbl_Aguardar.Text = "";

                btn_Agente.IsEnabled = true;
                btn_Guia.IsEnabled = true;

                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    await DisplayAlert("Sem ligação", "É necessária ligação à internet para jogar online.", "OK");
                    return;
                }
                /*else
                {
                    var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                    var respostaSala = await _supabase.Client.Rpc("eliminar_sala", parametroSala);

                    var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo } };
                    var respostaJogador = await _supabase.Client.Rpc("eliminar_jogador", parametroJogador);
                }*/
            }
        }

        private async void btn_Voltar2(object sender, EventArgs e)
        {
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
                return;
            }
            else
            {
                var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var respostaSala = await _supabase.Client.Rpc("criar_sala_privada", parametroSala);

                var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo }, { "p_funcao", "Guia" } };
                var respostaJogador = await _supabase.Client.Rpc("criar_jogador", parametroJogador);
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
                return;
            }
            else
            {
                var parametroSala = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var respostaSala = await _supabase.Client.Rpc("criar_sala_privada", parametroSala);

                var parametroJogador = new Dictionary<string, object?> { { "p_codigo", codigo }, { "p_funcao", "Agente" } };
                var respostaJogador = await _supabase.Client.Rpc("criar_jogador", parametroJogador);

            }
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

            codigo = txt_Codigo.Text;
            var parametro = new Dictionary<string, object?> { { "p_codigo", txt_Codigo.Text } };
            var resposta = await _supabase.Client.Rpc("entrar_em_sala", parametro);


            if (resposta.Content == "false")
            {
                await DisplayAlert("Código inválido", "O código inserido não corresponde a nenhuma sala ativa. Por favor, verifique o código ou tente novamente.", "OK");
            }
            else
            {
                var parametroFuncao = new Dictionary<string, object?> { { "p_codigo", codigo } };
                var respostaFuncao = await _supabase.Client.Rpc("qual_funcao", parametroFuncao);
                string funcao = respostaFuncao.Content.Trim('"');

                if (funcao == "Guia")
                {
                    lbl_funcao.Text = "Agente";
                }
                if (funcao == "Agente")
                {
                    lbl_funcao.Text = "Guia";
                }
            }
        }


        /*private async Task EntrarModoEsperaAsync()
        {
            try
            {
                _channel = _supabase.Client.Realtime.Channel("salas-updates");

                _channel.Register(new PostgresChangesOptions("public", "salas"));

                _channel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.All, (sender, change) =>
                {
                    try
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(change);
                        System.Diagnostics.Debug.WriteLine($"📋 Dados: {json}");

                        if (json.Contains($"\"{codigo}\"") && json.Contains("\"em_jogo\""))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                System.Diagnostics.Debug.WriteLine("🎮 INICIANDO JOGO!");
                                IniciarJogo();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                    }
                });

                // Subscrever ao canal
                await _channel.Subscribe();

                System.Diagnostics.Debug.WriteLine($"✅ Subscrito ao canal para sala: {codigo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao conectar ao Realtime: {ex.Message}");
                await DisplayAlert("Erro", $"Erro ao conectar ao Realtime: {ex.Message}", "OK");
            }
        }

        private async Task SairDoModoEspera()
        {
            if (_channel != null)
            {
                try
                {
                    _channel.Unsubscribe();
                    _channel = null;
                    System.Diagnostics.Debug.WriteLine("Canal desconectado");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao desinscrever: {ex.Message}");
                }
            }
        }

        // Placeholder - você precisa implementar este método
        private void IniciarJogo()
        {
            // Navegar para a página do jogo
            // Exemplo: Navigation.PushAsync(new GamePage());
        }*/
    }
}
