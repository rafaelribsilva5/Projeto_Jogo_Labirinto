using Projeto_Jogo_Labirinto.Services;

namespace Projeto_Jogo_Labirinto

{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
    }

}