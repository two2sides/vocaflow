using VocaFlow.Views;

namespace VocaFlow
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // 注册页面路由
            Routing.RegisterRoute(nameof(LyricResultPage), typeof(LyricResultPage));
            Routing.RegisterRoute(nameof(VocabSelectPage), typeof(VocabSelectPage));
            Routing.RegisterRoute(nameof(VocabEditPage), typeof(VocabEditPage));
            Routing.RegisterRoute(nameof(VocabReviewPage), typeof(VocabReviewPage));
        }
    }
}