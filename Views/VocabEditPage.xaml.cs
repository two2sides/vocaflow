using QQLyric2Roma.Models;
using QQLyric2Roma.Services;
using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;

namespace QQLyric2Roma.Views;

[QueryProperty(nameof(VocabId), "VocabId")]
public partial class VocabEditPage : ContentPage
{
    private readonly IDatabase _db;
    private VocabEntry _vocab;
    private List<string> _examples = new();

    public int VocabId { get; set; }

    public VocabEditPage(IDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVocabAsync();
    }

    private async Task LoadVocabAsync()
    {
        if (VocabId > 0)
        {
            _vocab = await _db.GetVocabByIdAsync(VocabId);
            if (_vocab != null)
            {
                PopulateForm();
            }
            else
            {
                await DisplayAlertAsync("错误", "找不到该词汇", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            await DisplayAlertAsync("错误", "无效的词汇ID", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private void PopulateForm()
    {
        // 显示语言（只读）
        LblLanguage.Text = _vocab.Language == "ja" ? "日语" : "英语";

        // 更新读音标签
        bool isJapanese = _vocab.Language == "ja";
        LblReadingTitle.Text = isJapanese ? "读音（假名） *" : "读音（音标） *";
        EntryReading.Placeholder = isJapanese ? "例如：さくら" : "例如：/ˈeksəmpl/";

        // 填充表单
        EntryWord.Text = _vocab.Word;
        EntryReading.Text = _vocab.Reading;
        EntryMeaning.Text = _vocab.Meaning;
        EditorContext.Text = _vocab.Context;
        EditorUsage.Text = _vocab.Usage;

        // 解析例句
        if (!string.IsNullOrEmpty(_vocab.Examples))
        {
            try
            {
                _examples = JsonSerializer.Deserialize<List<string>>(_vocab.Examples) ?? new List<string>();
            }
            catch
            {
                _examples = new List<string>();
            }
        }

        RefreshExamplesUI();
    }

    private void OnRequiredFieldChanged(object sender, TextChangedEventArgs e)
    {
        // 实时验证
        ValidateForm(showErrors: false);
    }

    private bool ValidateForm(bool showErrors = true)
    {
        bool isValid = true;

        // 验证词语
        if (string.IsNullOrWhiteSpace(EntryWord.Text))
        {
            if (showErrors) LblWordError.IsVisible = true;
            isValid = false;
        }
        else
        {
            LblWordError.IsVisible = false;
        }

        // 验证读音
        if (string.IsNullOrWhiteSpace(EntryReading.Text))
        {
            if (showErrors) LblReadingError.IsVisible = true;
            isValid = false;
        }
        else
        {
            LblReadingError.IsVisible = false;
        }

        // 验证意思
        if (string.IsNullOrWhiteSpace(EntryMeaning.Text))
        {
            if (showErrors) LblMeaningError.IsVisible = true;
            isValid = false;
        }
        else
        {
            LblMeaningError.IsVisible = false;
        }

        return isValid;
    }

    private void OnAddExampleClicked(object sender, EventArgs e)
    {
        _examples.Add("");
        RefreshExamplesUI();
    }

    private void RefreshExamplesUI()
    {
        ExamplesContainer.Children.Clear();

        LblNoExamples.IsVisible = _examples.Count == 0;

        for (int i = 0; i < _examples.Count; i++)
        {
            var index = i;
            var exampleBorder = CreateExampleItem(index, _examples[i]);
            ExamplesContainer.Children.Add(exampleBorder);
        }
    }

    private Border CreateExampleItem(int index, string exampleText)
    {
        var border = new Border
        {
            Padding = 10,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#F5F5F5"),
            Stroke = Colors.Transparent
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };

        var editor = new Editor
        {
            Text = exampleText,
            Placeholder = $"例句 {index + 1}",
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 60
        };

        // 当文本改变时更新列表
        editor.TextChanged += (s, e) =>
        {
            if (index < _examples.Count)
            {
                _examples[index] = e.NewTextValue ?? "";
            }
        };

        // 删除按钮
        var deleteTextButton = new Button
        {
            Text = "×",
            FontSize = 20,
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            WidthRequest = 36,
            HeightRequest = 36,
            CornerRadius = 18,
            Padding = 0,
            VerticalOptions = LayoutOptions.Start
        };

        deleteTextButton.Clicked += (s, e) =>
        {
            if (index < _examples.Count)
            {
                _examples.RemoveAt(index);
                RefreshExamplesUI();
            }
        };

        grid.Add(editor, 0, 0);
        grid.Add(deleteTextButton, 1, 0);

        border.Content = grid;
        return border;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!ValidateForm(showErrors: true))
        {
            await DisplayAlertAsync("提示", "请填写所有必填项", "OK");
            return;
        }

        // 移除空例句
        var validExamples = _examples.Where(ex => !string.IsNullOrWhiteSpace(ex)).ToList();

        // 更新词汇对象（语言不变）
        _vocab.Word = EntryWord.Text.Trim();
        _vocab.Reading = EntryReading.Text.Trim();
        _vocab.Meaning = EntryMeaning.Text.Trim();
        _vocab.Context = EditorContext.Text?.Trim() ?? "";
        _vocab.Usage = EditorUsage.Text?.Trim() ?? "";
        _vocab.Examples = validExamples.Count > 0 ? JsonSerializer.Serialize(validExamples) : "";

        try
        {
            await _db.UpdateVocabAsync(_vocab);
            await DisplayAlertAsync("成功", "词汇已更新", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("错误", $"保存失败: {ex.Message}", "OK");
        }
    }
}
