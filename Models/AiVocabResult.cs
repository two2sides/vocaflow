using System.Text.Json.Serialization;

namespace QQLyric2Roma.Models
{
    /// <summary>
    /// AI 返回的词汇提取结果
    /// </summary>
    public class AiVocabResult
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("words")]
        public List<AiVocabItem> Words { get; set; } = new();
    }

    /// <summary>
    /// 单个词汇项
    /// </summary>
    public class AiVocabItem
    {
        [JsonPropertyName("word")]
        public string Word { get; set; }

        [JsonPropertyName("readings")]
        public List<AiVocabReading> Readings { get; set; } = new();

        [JsonPropertyName("context")]
        public string Context { get; set; }

        [JsonPropertyName("usage")]
        public string Usage { get; set; }

        /// <summary>
        /// 用于 UI 绑定：是否被选中
        /// </summary>
        [JsonIgnore]
        public bool IsSelected { get; set; } = true;

        /// <summary>
        /// 显示用的主要读音
        /// </summary>
        [JsonIgnore]
        public string DisplayReading => Readings?.FirstOrDefault()?.Reading ?? "";

        /// <summary>
        /// 显示用的主要意思
        /// </summary>
        [JsonIgnore]
        public string DisplayMeaning => Readings?.FirstOrDefault()?.Meaning ?? "";

        /// <summary>
        /// 是否有多个读音/意思
        /// </summary>
        [JsonIgnore]
        public bool HasMultipleReadings => Readings != null && Readings.Count > 1;
    }

    /// <summary>
    /// 词汇的读音/意思/例句
    /// </summary>
    public class AiVocabReading
    {
        [JsonPropertyName("reading")]
        public string Reading { get; set; }

        [JsonPropertyName("meaning")]
        public string Meaning { get; set; }

        [JsonPropertyName("example")]
        public string Example { get; set; }
    }
}
