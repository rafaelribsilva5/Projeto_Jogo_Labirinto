using Supabase;
using Microsoft.Maui.Controls;

namespace Projeto_Jogo_Labirinto.Services
{
    public class SupabaseService
    {
        public Client? Client { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true
            };

            Client = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, options);

            await Client.InitializeAsync();
        }
    }
}

