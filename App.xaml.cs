using Projeto_Jogo_Labirinto.Services;
using Supabase;

namespace Projeto_Jogo_Labirinto

{
    public partial class App : Application
    {
        private readonly Supabase.Client _supabase;
        public App(Supabase.Client supabase)
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new MainPage());
            MainPage = new AppShell();

            _supabase = supabase;
            InitializeSupabase();
        }

        private async void InitializeSupabase()
        {
            await _supabase.InitializeAsync();
            await _supabase.Realtime.ConnectAsync();
        }
    }

}