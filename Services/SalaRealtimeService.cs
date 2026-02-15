using Supabase;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;
using System;
using System.Threading.Tasks;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;

namespace Projeto_Jogo_Labirinto.Services
{
    /// <summary>
    /// Serviço de comunicação em tempo real para a tabela "salas" do Supabase.
    /// Permite que dois dispositivos (Guia e Agente) recebam a notificação quando a sala entra em jogo.
    /// </summary>
    public class SalaRealtimeService
    {
        private readonly Func<Task<Supabase.Client?>> _getClient;
        private RealtimeChannel? _channel;
        private string? _codigoAtual;

        public SalaRealtimeService(Func<Task<Supabase.Client?>> getClient)
        {
            _getClient = getClient ?? throw new ArgumentNullException(nameof(getClient));
        }

        /// <summary>
        /// Subscreve às alterações da sala com o código dado. Quando a sala passar a em_jogo, invoca onSalaEmJogo no main thread.
        /// </summary>
        public async Task SubscribeAsync(string codigo, Action onSalaEmJogo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                System.Diagnostics.Debug.WriteLine("SalaRealtimeService: codigo vazio.");
                return;
            }

            await UnsubscribeAsync().ConfigureAwait(false);

            var client = await _getClient().ConfigureAwait(false);
            if (client?.Realtime == null)
            {
                System.Diagnostics.Debug.WriteLine("SalaRealtimeService: Cliente Supabase não disponível.");
                return;
            }

            _codigoAtual = codigo;
            var channelName = "salas";

            try
            {
                _channel = client.Realtime.Channel(channelName);

                // Filtro: apenas a linha desta sala. Para coluna texto usar codigo=eq.'valor' se necessário.
                var filter = $"codigo=eq.{codigo}";
                var options = new PostgresChangesOptions("public", "salas", ListenType.All, filter);
                _channel.Register(options);

                _channel.AddPostgresChangeHandler(ListenType.All, (_, change) =>
                {
                    try
                    {
                        var json = change?.ToString() ?? "";
                        if (string.IsNullOrEmpty(json))
                            json = System.Text.Json.JsonSerializer.Serialize(change);
                        System.Diagnostics.Debug.WriteLine($"[SalaRealtime] Alteração: {json}");

                        if ((json.Contains(codigo, StringComparison.Ordinal) || json.Contains($"\"{codigo}\"", StringComparison.Ordinal)) &&
                            json.Contains("em_jogo", StringComparison.OrdinalIgnoreCase))
                        {
                            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(onSalaEmJogo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalaRealtime] Erro no handler: {ex.Message}");
                    }
                });

                await _channel.Subscribe().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine($"[SalaRealtime] Subscrito à sala: {codigo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaRealtime] Erro ao subscrever: {ex.Message}");
                _channel = null;
                _codigoAtual = null;
                throw;
            }
        }

        /// <summary>
        /// Cancela a subscrição ao canal da sala.
        /// </summary>
        public Task UnsubscribeAsync()
        {
            if (_channel == null)
                return Task.CompletedTask;

            try
            {
                _channel.Unsubscribe();
                System.Diagnostics.Debug.WriteLine("[SalaRealtime] Canal desconectado.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalaRealtime] Erro ao desinscrever: {ex.Message}");
            }
            finally
            {
                _channel = null;
                _codigoAtual = null;
            }
            return Task.CompletedTask;
        }
    }
}
