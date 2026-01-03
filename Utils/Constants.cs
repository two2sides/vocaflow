using System;
using System.Collections.Generic;
using System.Text;

namespace VocaFlow.Utils
{
    public static class Constants
    {
        // Preferences 的 Key 名称
        public const string PrefKeyApiKey = "OpenAiKey";
        public const string PrefKeyAiModel = "AiModel";
        public const string PrefKeyAiPrompt = "RomaPrompt";
        public const string PrefKeyVocabPrompt = "VocabPrompt";

        // 默认值
        public const string DefaultAiModel = "x-ai/grok-4.1-fast";

        // 默认的 Prompt 内容
        public const string DefaultRomaPrompt = @"
你是一个日语歌词辅助助手。请将以下歌词转换为罗马音（Romaji）。
要求：
1. 只输出转换后的罗马音，不要包含原文，不要包含任何解释或前言后语。
2. 保持原歌词的换行格式。
3. 如果歌词中包含中文或英文，请保留原样。
歌词如下:
";

        // 生词提取的默认 Prompt
        public const string DefaultVocabPrompt = @"
你是一个语言学习助手。请从以下文本中提取生词，并以 JSON 格式输出。

要求：
1. 识别文本的语言，如果是日语设为 ""ja""，英语设为 ""en""
2. 提取可能对学习者有帮助的生词（不要提取过于简单的词如 ""I"", ""the"", ""は"" 等）
3. 对于日语词汇：提供假名读音、中文意思、例句
4. 对于英语词汇：提供音标、中文意思、例句
5. 如果一个词有多种常用读音/意思，请在 readings 数组中分别列出
6. context 填写该词在原歌词中出现的句子
7. 只输出纯 JSON，不要包含 markdown 代码块标记或任何其他文字

JSON 格式如下：
{
  ""language"": ""ja"",
  ""words"": [
    {
      ""word"": ""桜"",
      ""readings"": [
        { ""reading"": ""さくら"", ""meaning"": ""樱花"", ""example"": ""桜が満開だ"" }
      ],
      ""context"": ""歌词中出现该词的原句"",
      ""usage"": ""用法说明""
    }
  ]
}

歌词如下:
";
    }
}
