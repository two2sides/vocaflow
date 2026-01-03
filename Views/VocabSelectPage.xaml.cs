using QQLyric2Roma.Models;
using QQLyric2Roma.Services;
using System.Text.Json;

namespace QQLyric2Roma.Views
{
    [QueryProperty(nameof(VocabResultJson), "VocabResultJson")]
    public partial class VocabSelectPage : ContentPage
    {
        private readonly IDatabase _db;
        private AiVocabResult _vocabResult;
        private List<AiVocabItem> _vocabItems;

        public string VocabResultJson { get; set; }

        public VocabSelectPage(IDatabase db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (!string.IsNullOrEmpty(VocabResultJson))
            {
                try
                {
                    _vocabResult = JsonSerializer.Deserialize<AiVocabResult>(VocabResultJson);
                    _vocabItems = _vocabResult?.Words ?? new List<AiVocabItem>();

                    foreach (var item in _vocabItems)
                    {
                        item.IsSelected = true;
                    }

                    VocabList.ItemsSource = _vocabItems;

                    string langName = _vocabResult?.Language == "ja" ? "日语" : 
                                     _vocabResult?.Language == "en" ? "英语" : "未知";
                    LblLanguage.Text = $"语言: {langName}";
                    LblCount.Text = $"共识别 {_vocabItems.Count} 个生词";
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("错误", $"解析数据失败: {ex.Message}", "OK");
                }
            }
        }

        private void OnToggleAllClicked(object sender, EventArgs e)
        {
            if (_vocabItems == null || _vocabItems.Count == 0) return;

            bool allSelected = _vocabItems.All(x => x.IsSelected);

            foreach (var item in _vocabItems)
            {
                item.IsSelected = !allSelected;
            }

            VocabList.ItemsSource = null;
            VocabList.ItemsSource = _vocabItems;
        }

        private async void OnSaveSelectedClicked(object sender, EventArgs e)
        {
            if (_vocabItems == null) return;

            var selectedItems = _vocabItems.Where(x => x.IsSelected).ToList();

            if (selectedItems.Count == 0)
            {
                await DisplayAlertAsync("提示", "请至少选择一个词汇", "OK");
                return;
            }

            int savedCount = 0;
            int skippedCount = 0;
            string language = _vocabResult?.Language ?? "ja";

            foreach (var item in selectedItems)
            {
                // 检查是否已存在
                bool exists = await _db.IsVocabSavedAsync(item.Word, language);
                if (exists)
                {
                    skippedCount++;
                    continue; // 跳过已存在的词汇
                }

                string altReadings = "";
                string examples = "";

                if (item.Readings != null && item.Readings.Count > 1)
                {
                    var altList = item.Readings.Skip(1).Select(r => r.Reading).ToList();
                    altReadings = JsonSerializer.Serialize(altList);

                    var exampleList = item.Readings.Select(r => r.Example).Where(ex => !string.IsNullOrEmpty(ex)).ToList();
                    examples = JsonSerializer.Serialize(exampleList);
                }
                else if (item.Readings != null && item.Readings.Count == 1)
                {
                    var example = item.Readings[0].Example;
                    if (!string.IsNullOrEmpty(example))
                    {
                        examples = JsonSerializer.Serialize(new[] { example });
                    }
                }

                var vocab = new VocabEntry
                {
                    Language = language,
                    Word = item.Word,
                    Meaning = item.DisplayMeaning,
                    Reading = item.DisplayReading,
                    AltReadings = altReadings,
                    Context = item.Context ?? "",
                    Examples = examples,
                    Usage = item.Usage ?? ""
                };

                await _db.SaveVocabAsync(vocab);
                savedCount++;
            }

            // 显示结果
            string message = $"已保存 {savedCount} 个词汇到回忆";
            if (skippedCount > 0)
            {
                message += $"\n跳过 {skippedCount} 个已存在的词汇";
            }

            await DisplayAlertAsync("完成", message, "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
