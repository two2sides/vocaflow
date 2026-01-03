using Microsoft.Extensions.DependencyInjection;
using VocaFlow.Utils;

namespace VocaFlow
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        protected override void OnStart()
        {
            base.OnStart();

            // 检查是否存在 "AiPrompt" 这个 Key
            // 如果不存在（说明是第一次安装或数据被清除了），则写入默认值
            if (!Preferences.ContainsKey("RomaPrompt"))
            {
                Preferences.Set("RomaPrompt", Constants.DefaultRomaPrompt);
            }
        }
    }
}