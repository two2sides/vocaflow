using VocaFlow.Models;
using VocaFlow.Services;

namespace VocaFlow.Views;

[QueryProperty(nameof(Language), "Language")]
[QueryProperty(nameof(Count), "Count")]
public partial class VocabReviewPage : ContentPage
{
    private readonly IDatabase _db;
    private List<VocabEntry> _vocabList = new();
    private int _currentIndex = 0;
    private bool _isRevealed = false;
    private bool _isCompleted = false;

    public string Language { get; set; }
    public int Count { get; set; }

    public VocabReviewPage(IDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRandomVocabsAsync();
    }

    private async Task LoadRandomVocabsAsync()
    {
        _vocabList = await _db.GetRandomVocabsAsync(Count, Language);

        if (_vocabList.Count == 0)
        {
            await DisplayAlertAsync("提示", "没有可复习的词汇", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        _currentIndex = 0;
        _isCompleted = false;
        BtnShuffleAgain.IsVisible = false;

        // 更新标题
        string langName = Language == "ja" ? "日语" : "英语";
        LblLanguage.Text = $"{langName}复习";

        DisplayCurrentVocab();
    }

    private void DisplayCurrentVocab()
    {
        if (_vocabList.Count == 0) return;

        var vocab = _vocabList[_currentIndex];

        // 重置显示状态
        _isRevealed = false;
        DetailSection.IsVisible = false;
        LblHint.IsVisible = true;
        LblHint.Text = "点击卡片查看详情";

        // 显示词语
        LblWord.Text = vocab.Word;

        // 更新进度
        LblProgress.Text = $"进度: {_currentIndex + 1} / {_vocabList.Count}";

        // 更新导航按钮状态
        BtnPrevious.IsEnabled = _currentIndex > 0;
        BtnPrevious.BackgroundColor = _currentIndex > 0 ? Colors.LightGray : Colors.WhiteSmoke;

        // 准备详细信息
        LblReading.Text = vocab.Reading ?? "";
        LblMeaning.Text = vocab.Meaning ?? "";

        // 上下文
        if (vocab.HasContext)
        {
            ContextSection.IsVisible = true;
            LblContext.Text = vocab.Context;
        }
        else
        {
            ContextSection.IsVisible = false;
        }

        // 例句
        if (vocab.HasExamples)
        {
            ExamplesSection.IsVisible = true;
            LblExamples.Text = vocab.ExamplesDisplay;
        }
        else
        {
            ExamplesSection.IsVisible = false;
        }
    }

    private void OnCardTapped(object sender, TappedEventArgs e)
    {
        _isRevealed = !_isRevealed;
        DetailSection.IsVisible = _isRevealed;
        LblHint.Text = _isRevealed ? "点击卡片收起详情" : "点击卡片查看详情";
    }

    private void OnPreviousClicked(object sender, EventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            DisplayCurrentVocab();
        }
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        if (_currentIndex < _vocabList.Count - 1)
        {
            _currentIndex++;
            DisplayCurrentVocab();
        }
        else
        {
            // 已经是最后一个，标记完成
            _isCompleted = true;
            BtnShuffleAgain.IsVisible = true;
            LblHint.Text = "复习完成！";
            LblHint.IsVisible = true;
        }
    }

    private async void OnShuffleAgainClicked(object sender, EventArgs e)
    {
        await LoadRandomVocabsAsync();
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
