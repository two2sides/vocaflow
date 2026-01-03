using QQLyric2Roma.Models;
using QQLyric2Roma.Services;
using QQLyric2Roma.Utils;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QQLyric2Roma.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly IAppSettings _settings;

        public SearchPage(IAppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
        }

        #region 文本提取生词

        private async void OnExtractVocabFromTextClicked(object sender, EventArgs e)
        {
            string inputText = EditorTextInput.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                await DisplayAlertAsync("提示", "请先输入文本内容", "OK");
                return;
            }

            if (!_settings.HasApiKey)
            {
                await DisplayAlertAsync("错误", "请先在设置页配置 API Key", "OK");
                return;
            }

            VocabLoading.IsRunning = true;

            var (result, error) = await AiService.ExtractVocabAsync(_settings, inputText);

            VocabLoading.IsRunning = false;

            if (error != null)
            {
                await DisplayAlertAsync("错误", error, "OK");
                return;
            }

            if (result == null || result.Words == null || result.Words.Count == 0)
            {
                await DisplayAlertAsync("提示", "未识别到生词", "OK");
                return;
            }

            // 导航到选择页面
            var jsonResult = JsonSerializer.Serialize(result);
            var navigationParameter = new Dictionary<string, object>
            {
                { "VocabResultJson", jsonResult }
            };

            await Shell.Current.GoToAsync(nameof(VocabSelectPage), navigationParameter);
        }

        #endregion

        #region 歌曲搜索

        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            var keyword = MusicSearchBar.Text;
            if (string.IsNullOrWhiteSpace(keyword)) return;

            LoadingSpinner.IsRunning = true;
            ResultList.ItemsSource = null;

            // 调用 API
            var jsonNode = await QQMusicApi.SearchWithKeywordAsync(keyword);

            var songs = new List<Song>();
            if (jsonNode != null)
            {
                var list = jsonNode["itemlist"]?.AsArray() ?? jsonNode["list"]?.AsArray();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        songs.Add(new Song
                        {
                            Title = item["name"]?.ToString(),
                            Artist = item["singer"]?[0]?["name"]?.ToString(),
                            SongMid = item["mid"]?.ToString()
                        });
                    }
                }
            }

            ResultList.ItemsSource = songs;
            LoadingSpinner.IsRunning = false;
        }

        private async void OnSongSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Song selectedSong)
            {
                // 跳转到歌词页面，传递 SongMid 和 Title
                var navigationParameter = new Dictionary<string, object>
                {
                    { "SongMid", selectedSong.SongMid },
                    { "SongTitle", selectedSong.Title }
                };

                // 清除选中状态
                ResultList.SelectedItem = null;

                await Shell.Current.GoToAsync(nameof(LyricResultPage), navigationParameter);
            }
        }

        #endregion
    }
}