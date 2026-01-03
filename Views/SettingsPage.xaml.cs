using QQLyric2Roma.Services;
using QQLyric2Roma.Utils;

namespace QQLyric2Roma.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly IAppSettings _settings;

        public SettingsPage(IAppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // 页面显示时读取已保存的配置
            EntryApiKey.Text = _settings.ApiKey;
            EntryModel.Text = _settings.AiModel;
            RomaPrompt.Text = _settings.RomaPrompt;
            VocabPrompt.Text = _settings.VocabPrompt;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // 保存配置到本地存储
            _settings.ApiKey = EntryApiKey.Text ?? string.Empty;
            _settings.AiModel = EntryModel.Text ?? string.Empty;
            _settings.RomaPrompt = RomaPrompt.Text ?? Constants.DefaultRomaPrompt;
            _settings.VocabPrompt = VocabPrompt.Text ?? Constants.DefaultVocabPrompt;
            await DisplayAlertAsync("成功", "设置已保存", "OK");
        }
    }
}