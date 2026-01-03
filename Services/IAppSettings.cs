namespace QQLyric2Roma.Services
{
    /// <summary>
    /// 应用配置抽象接口
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// OpenRouter API Key
        /// </summary>
        string ApiKey { get; set; }

        /// <summary>
        /// AI 模型名称
        /// </summary>
        string AiModel { get; set; }

        /// <summary>
        /// 罗马音转换 Prompt
        /// </summary>
        string RomaPrompt { get; set; }

        /// <summary>
        /// 生词提取 Prompt
        /// </summary>
        string VocabPrompt { get; set; }

        /// <summary>
        /// 检查 API Key 是否已配置
        /// </summary>
        bool HasApiKey { get; }
    }
}
