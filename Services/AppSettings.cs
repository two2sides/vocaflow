using QQLyric2Roma.Utils;

namespace QQLyric2Roma.Services
{
    /// <summary>
    /// 基于 Preferences 的配置实现
    /// </summary>
    public class AppSettings : IAppSettings
    {
        public string ApiKey
        {
            get => Preferences.Get(Constants.PrefKeyApiKey, string.Empty);
            set => Preferences.Set(Constants.PrefKeyApiKey, value);
        }

        public string AiModel
        {
            get => Preferences.Get(Constants.PrefKeyAiModel, Constants.DefaultAiModel);
            set => Preferences.Set(Constants.PrefKeyAiModel, value);
        }

        public string RomaPrompt
        {
            get => Preferences.Get(Constants.PrefKeyAiPrompt, Constants.DefaultRomaPrompt);
            set => Preferences.Set(Constants.PrefKeyAiPrompt, value);
        }

        public string VocabPrompt
        {
            get => Preferences.Get(Constants.PrefKeyVocabPrompt, Constants.DefaultVocabPrompt);
            set => Preferences.Set(Constants.PrefKeyVocabPrompt, value);
        }

        public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);
    }
}
