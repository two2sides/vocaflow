using VocaFlow.Models;
using VocaFlow.Services;

namespace VocaFlow.Views;

public partial class VocabularyPage : ContentPage
{
    private readonly IDatabase _db;
    private string _currentTab = "lyrics"; // lyrics, ja, en
    private bool _isEditMode = false;
    private string _searchKeyword = "";
    
    private List<SavedLyric> _lyrics;
    private List<VocabEntry> _jaVocabs;
    private List<VocabEntry> _enVocabs;

    public bool IsEditMode => _isEditMode;

    public VocabularyPage(IDatabase db)
    {
        InitializeComponent();
        _db = db;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // 加载歌词
        _lyrics = await _db.GetLyricsAsync();
        LyricsList.ItemsSource = _lyrics;

        // 加载日语词汇
        _jaVocabs = await _db.GetVocabsByLanguageAsync("ja");
        JapaneseList.ItemsSource = _jaVocabs;

        // 加载英语词汇
        _enVocabs = await _db.GetVocabsByLanguageAsync("en");
        EnglishList.ItemsSource = _enVocabs;
    }

    #region 搜索功能

    private async void OnVocabSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchKeyword = e.NewTextValue ?? "";
        await PerformSearchAsync();
    }

    private async void OnVocabSearchButtonPressed(object sender, EventArgs e)
    {
        await PerformSearchAsync();
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(_searchKeyword))
        {
            // 关键词为空，显示全部
            if (_currentTab == "ja")
            {
                _jaVocabs = await _db.GetVocabsByLanguageAsync("ja");
                JapaneseList.ItemsSource = _jaVocabs;
            }
            else if (_currentTab == "en")
            {
                _enVocabs = await _db.GetVocabsByLanguageAsync("en");
                EnglishList.ItemsSource = _enVocabs;
            }
        }
        else
        {
            // 执行搜索
            string language = _currentTab == "ja" ? "ja" : "en";
            var results = await _db.SearchVocabAsync(_searchKeyword, language);

            if (_currentTab == "ja")
            {
                _jaVocabs = results;
                JapaneseList.ItemsSource = _jaVocabs;
            }
            else if (_currentTab == "en")
            {
                _enVocabs = results;
                EnglishList.ItemsSource = _enVocabs;
            }
        }
    }

    #endregion

    private async void OnTabClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        // 重置所有按钮样式 - 非选中状态
        BtnLyrics.BackgroundColor = Colors.Transparent;
        BtnLyrics.TextColor = Application.Current.Resources["Gray600"] as Color ?? Colors.Gray;
        BtnJapanese.BackgroundColor = Colors.Transparent;
        BtnJapanese.TextColor = Application.Current.Resources["Gray600"] as Color ?? Colors.Gray;
        BtnEnglish.BackgroundColor = Colors.Transparent;
        BtnEnglish.TextColor = Application.Current.Resources["Gray600"] as Color ?? Colors.Gray;

        // 隐藏所有列表
        LyricsList.IsVisible = false;
        JapaneseList.IsVisible = false;
        EnglishList.IsVisible = false;

        // 激活选中的 Tab - 使用主题色
        var primaryColor = Application.Current.Resources["Primary"] as Color ?? Colors.Purple;
        button.BackgroundColor = primaryColor;
        button.TextColor = Colors.White;

        if (button == BtnLyrics)
        {
            _currentTab = "lyrics";
            LyricsList.IsVisible = true;
            VocabToolbar.IsVisible = false;
        }
        else if (button == BtnJapanese)
        {
            _currentTab = "ja";
            JapaneseList.IsVisible = true;
            VocabToolbar.IsVisible = true;
        }
        else if (button == BtnEnglish)
        {
            _currentTab = "en";
            EnglishList.IsVisible = true;
            VocabToolbar.IsVisible = true;
        }

        // 切换 Tab 时清空搜索并重新加载
        VocabSearchBar.Text = "";
        _searchKeyword = "";
        await LoadDataAsync();
    }

    private void OnEditModeClicked(object sender, EventArgs e)
    {
        _isEditMode = !_isEditMode;
        
        var primaryColor = Application.Current.Resources["Primary"] as Color ?? Colors.Purple;
        var primaryLightColor = Application.Current.Resources["PrimaryLight"] as Color ?? Colors.LightGray;
        
        BtnEdit.Text = _isEditMode ? "完成" : "编辑";
        BtnEdit.BackgroundColor = _isEditMode ? primaryColor : primaryLightColor;
        BtnEdit.TextColor = _isEditMode ? Colors.White : primaryColor;
        BtnDeleteSelected.IsVisible = _isEditMode;

        // 退出编辑模式时清除所有选中状态
        if (!_isEditMode)
        {
            ClearAllSelections();
        }

        // 刷新列表以更新 CheckBox 可见性
        OnPropertyChanged(nameof(IsEditMode));
        RefreshLists();
    }

    private void ClearAllSelections()
    {
        if (_lyrics != null)
            foreach (var item in _lyrics) item.IsSelected = false;
        if (_jaVocabs != null)
            foreach (var item in _jaVocabs) item.IsSelected = false;
        if (_enVocabs != null)
            foreach (var item in _enVocabs) item.IsSelected = false;
    }

    private void RefreshLists()
    {
        LyricsList.ItemsSource = null;
        LyricsList.ItemsSource = _lyrics;

        JapaneseList.ItemsSource = null;
        JapaneseList.ItemsSource = _jaVocabs;

        EnglishList.ItemsSource = null;
        EnglishList.ItemsSource = _enVocabs;
    }

    private async void OnLyricTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not SavedLyric lyric) return;

        if (_isEditMode)
        {
            lyric.IsSelected = !lyric.IsSelected;
            RefreshLists();
        }
        else
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "SongMid", lyric.SongMid },
                { "SongTitle", lyric.SongTitle },
                { "SavedLyricId", lyric.Id }
            };
            await Shell.Current.GoToAsync(nameof(LyricResultPage), navigationParameter);
        }
    }

    private void OnVocabTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not VocabEntry vocab) return;

        if (_isEditMode)
        {
            // 编辑模式：切换选中状态
            vocab.IsSelected = !vocab.IsSelected;
        }
        else
        {
            // 普通模式：切换展开状态
            vocab.IsExpanded = !vocab.IsExpanded;
        }
        RefreshLists();
    }

    private async void OnEditVocabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is VocabEntry vocab)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "VocabId", vocab.Id }
            };
            await Shell.Current.GoToAsync(nameof(VocabEditPage), navigationParameter);
        }
    }

    private async void OnRandomReviewClicked(object sender, EventArgs e)
    {
        // 弹出选择数量
        var result = await DisplayActionSheet("选择复习数量", "取消", null, "5个", "10个", "20个", "全部");
        
        if (result == null || result == "取消") return;

        int count = result switch
        {
            "5个" => 5,
            "10个" => 10,
            "20个" => 20,
            "全部" => int.MaxValue,
            _ => 10
        };

        // 导航到复习页面
        var navigationParameter = new Dictionary<string, object>
        {
            { "Language", _currentTab },
            { "Count", count }
        };

        await Shell.Current.GoToAsync(nameof(VocabReviewPage), navigationParameter);
    }

    private async void OnDeleteSelectedClicked(object sender, EventArgs e)
    {
        int deleteCount = 0;

        if (_currentTab == "lyrics")
        {
            var selectedLyrics = _lyrics?.Where(x => x.IsSelected).ToList();
            if (selectedLyrics == null || selectedLyrics.Count == 0)
            {
                await DisplayAlertAsync("提示", "请先选择要删除的歌词", "OK");
                return;
            }

            bool confirm = await DisplayAlertAsync("确认删除", $"确定要删除选中的 {selectedLyrics.Count} 首歌词吗？", "删除", "取消");
            if (!confirm) return;

            foreach (var lyric in selectedLyrics)
            {
                await _db.DeleteLyricAsync(lyric);
                deleteCount++;
            }
        }
        else
        {
            var vocabList = _currentTab == "ja" ? _jaVocabs : _enVocabs;
            var selectedVocabs = vocabList?.Where(x => x.IsSelected).ToList();
            if (selectedVocabs == null || selectedVocabs.Count == 0)
            {
                await DisplayAlertAsync("提示", "请先选择要删除的词汇", "OK");
                return;
            }

            bool confirm = await DisplayAlertAsync("确认删除", $"确定要删除选中的 {selectedVocabs.Count} 个词汇吗？", "删除", "取消");
            if (!confirm) return;

            foreach (var vocab in selectedVocabs)
            {
                await _db.DeleteVocabAsync(vocab);
                deleteCount++;
            }
        }

        await DisplayAlertAsync("完成", $"已删除 {deleteCount} 项", "OK");
        await LoadDataAsync();
    }

    private async void OnDeleteLyricSwiped(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is SavedLyric lyric)
        {
            bool confirm = await DisplayAlertAsync("确认删除", $"确定要删除《{lyric.SongTitle}》吗？", "删除", "取消");
            if (confirm)
            {
                await _db.DeleteLyricAsync(lyric);
                await LoadDataAsync();
            }
        }
    }

    private async void OnDeleteVocabSwiped(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is VocabEntry vocab)
        {
            bool confirm = await DisplayAlertAsync("确认删除", $"确定要删除「{vocab.Word}」吗？", "删除", "取消");
            if (confirm)
            {
                await _db.DeleteVocabAsync(vocab);
                await LoadDataAsync();
            }
        }
    }
}