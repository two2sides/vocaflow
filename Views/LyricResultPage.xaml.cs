using VocaFlow.Models;
using VocaFlow.Services;
using VocaFlow.Utils;
using System.Text.Json;

namespace VocaFlow.Views
{
    // QueryProperty 用于接收上一个页面传来的参数
    [QueryProperty(nameof(SongMid), "SongMid")]
    [QueryProperty(nameof(SongTitle), "SongTitle")]
    [QueryProperty(nameof(SavedLyricId), "SavedLyricId")]
    public partial class LyricResultPage : ContentPage
    {
        private readonly IAppSettings _settings;
        private readonly IDatabase _db;
        private string _artist;

        public string SongMid { get; set; }
        public string SongTitle { get; set; }
        public int SavedLyricId { get; set; }

        public LyricResultPage(IAppSettings settings, IDatabase db)
        {
            InitializeComponent();
            _settings = settings;
            _db = db;
        }

        // 页面加载时执行
        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            LblTitle.Text = SongTitle;

            // 如果是从回忆页面进入，加载已保存的数据
            if (SavedLyricId > 0)
            {
                var savedLyric = await _db.GetLyricByIdAsync(SavedLyricId);
                if (savedLyric != null)
                {
                    EditorOriginal.Text = savedLyric.OriginalLyric;
                    EditorRomaji.Text = savedLyric.Romaji;
                    _artist = savedLyric.Artist;
                    return;
                }
            }

            // 从 QQ 音乐获取歌词
            if (!string.IsNullOrEmpty(SongMid))
            {
                // 注意：这里需要类型转换，因为我们之前定义的 Utils 返回的是 object
                var lyricData = await QQMusicApi.GetSongLyricAsync(SongMid, parse: true) as QQMusicApi.LyricResult;

                if (lyricData != null)
                {
                    // 将所有歌词行拼接起来
                    string fullLyric = string.Join("\n", lyricData.LyricList.Select(x => x.Lyric));
                    EditorOriginal.Text = fullLyric;
                    _artist = lyricData.Artist;
                }
                else
                {
                    EditorOriginal.Text = "未找到歌词或出错。";
                }
            }
        }

        private async void OnGenerateRomajiClicked(object sender, EventArgs e)
        {
            string originalText = EditorOriginal.Text;
            if (string.IsNullOrEmpty(originalText)) return;

            if (!_settings.HasApiKey)
            {
                await DisplayAlertAsync("错误", "请先在设置页配置 API Key", "OK");
                return;
            }

            AiLoading.IsRunning = true;

            string romaji = await AiService.GetRomajiAsync(_settings, EditorOriginal.Text);

            EditorRomaji.Text = romaji;

            AiLoading.IsRunning = false;
        }

        private async void OnExtractVocabClicked(object sender, EventArgs e)
        {
            string originalText = EditorOriginal.Text;
            if (string.IsNullOrEmpty(originalText))
            {
                await DisplayAlertAsync("提示", "没有歌词内容", "OK");
                return;
            }

            if (!_settings.HasApiKey)
            {
                await DisplayAlertAsync("错误", "请先在设置页配置 API Key", "OK");
                return;
            }

            AiLoading.IsRunning = true;

            var (result, error) = await AiService.ExtractVocabAsync(_settings, originalText);

            AiLoading.IsRunning = false;

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

            // 将结果序列化后传递到选择页面
            var jsonResult = JsonSerializer.Serialize(result);
            var navigationParameter = new Dictionary<string, object>
            {
                { "VocabResultJson", jsonResult }
            };

            await Shell.Current.GoToAsync(nameof(VocabSelectPage), navigationParameter);
        }

        private async void OnSaveVocabClicked(object sender, EventArgs e)
        {
            await SaveLyricAsync();
        }

        private async Task SaveLyricAsync()
        {
            // 检查是否已保存
            bool alreadySaved = await _db.IsLyricSavedAsync(SongMid);
            if (alreadySaved)
            {
                bool overwrite = await DisplayAlertAsync("提示", "该歌词已保存过，是否覆盖？", "覆盖", "取消");
                if (!overwrite) return;

                // 删除旧的再保存新的
                var existingLyrics = await _db.GetLyricsAsync();
                var existing = existingLyrics.FirstOrDefault(x => x.SongMid == SongMid);
                if (existing != null)
                {
                    await _db.DeleteLyricAsync(existing);
                }
            }

            var lyric = new SavedLyric
            {
                SongMid = SongMid,
                SongTitle = SongTitle,
                Artist = _artist ?? "",
                OriginalLyric = EditorOriginal.Text,
                Romaji = EditorRomaji.Text ?? ""
            };

            await _db.SaveLyricAsync(lyric);
            await DisplayAlertAsync("成功", "歌词已保存到回忆", "OK");
        }

        private async void OnCopyOriginalClicked(object sender, EventArgs e)
        {
            await Clipboard.Default.SetTextAsync(EditorOriginal.Text);
            await DisplayAlertAsync("提示", "已复制到剪贴板", "OK");
        }
    }
}