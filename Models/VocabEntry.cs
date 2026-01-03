using SQLite;
using System.Text.Json;

namespace VocaFlow.Models
{
    /// <summary>
    /// 词汇条目实体
    /// </summary>
    public class VocabEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// 语言类型: "ja" (日语) / "en" (英语)
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 词语本身
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// 词语意思
        /// </summary>
        public string Meaning { get; set; }

        /// <summary>
        /// 主要读音（日语假名/英语音标）
        /// </summary>
        public string Reading { get; set; }

        /// <summary>
        /// 其他读音（JSON 数组格式存储）
        /// </summary>
        public string AltReadings { get; set; }

        /// <summary>
        /// 出现时的上下文句子
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// 例句（JSON 数组格式存储）
        /// </summary>
        public string Examples { get; set; }

        /// <summary>
        /// 用法说明
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 用于列表显示
        /// </summary>
        [Ignore]
        public string DisplayText => $"{Word} - {Meaning}";

        /// <summary>
        /// 格式化的创建时间
        /// </summary>
        [Ignore]
        public string CreatedAtText => CreatedAt.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 语言显示名称
        /// </summary>
        [Ignore]
        public string LanguageDisplay => Language == "ja" ? "日语" : "英语";

        /// <summary>
        /// 是否选中（用于批量删除）
        /// </summary>
        [Ignore]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否展开（用于显示详情）
        /// </summary>
        [Ignore]
        public bool IsExpanded { get; set; }

        /// <summary>
        /// 例句列表（解析 JSON）
        /// </summary>
        [Ignore]
        public List<string> ExamplesList
        {
            get
            {
                if (string.IsNullOrEmpty(Examples)) return new List<string>();
                try
                {
                    return JsonSerializer.Deserialize<List<string>>(Examples) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        /// <summary>
        /// 例句显示文本
        /// </summary>
        [Ignore]
        public string ExamplesDisplay
        {
            get
            {
                var list = ExamplesList;
                if (list.Count == 0) return "";
                return string.Join("\n", list.Select((ex, i) => $"  {i + 1}. {ex}"));
            }
        }

        /// <summary>
        /// 是否有例句
        /// </summary>
        [Ignore]
        public bool HasExamples => ExamplesList.Count > 0;

        /// <summary>
        /// 是否有上下文
        /// </summary>
        [Ignore]
        public bool HasContext => !string.IsNullOrEmpty(Context);

        /// <summary>
        /// 是否有用法说明
        /// </summary>
        [Ignore]
        public bool HasUsage => !string.IsNullOrEmpty(Usage);
    }
}
