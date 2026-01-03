using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using QQLyric2Roma.Models;
using QQLyric2Roma.Services;

namespace QQLyric2Roma.Utils
{
    public static class AiService
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string OpenRouterUrl = "https://openrouter.ai/api/v1/chat/completions";

        /// <summary>
        /// 发送歌词给 AI 并获取罗马音
        /// </summary>
        public static async Task<string> GetRomajiAsync(IAppSettings settings, string lyrics)
        {
            if (!settings.HasApiKey) return "错误：未设置 API Key";

            var prompt = settings.RomaPrompt + lyrics;
            return await SendRequestAsync(settings, prompt);
        }

        /// <summary>
        /// 发送歌词给 AI 并提取生词
        /// </summary>
        public static async Task<(AiVocabResult Result, string Error)> ExtractVocabAsync(IAppSettings settings, string lyrics)
        {
            if (!settings.HasApiKey)
                return (null, "错误：未设置 API Key");

            var prompt = settings.VocabPrompt + lyrics;
            var responseText = await SendRequestAsync(settings, prompt);

            // 检查是否为错误信息
            if (responseText.StartsWith("错误") || responseText.StartsWith("API 请求失败") || responseText.StartsWith("发生异常"))
                return (null, responseText);

            try
            {
                // 清理可能的 markdown 代码块标记
                var jsonText = CleanJsonResponse(responseText);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AiVocabResult>(jsonText, options);

                if (result == null || result.Words == null || result.Words.Count == 0)
                    return (null, "AI 未返回有效的词汇数据");

                return (result, null);
            }
            catch (JsonException ex)
            {
                return (null, $"JSON 解析失败: {ex.Message}\n\n原始返回:\n{responseText}");
            }
        }

        /// <summary>
        /// 清理 AI 返回的 JSON（去除 markdown 代码块标记）
        /// </summary>
        private static string CleanJsonResponse(string response)
        {
            var text = response.Trim();

            // 去除 ```json 开头
            if (text.StartsWith("```json"))
                text = text.Substring(7);
            else if (text.StartsWith("```"))
                text = text.Substring(3);

            // 去除 ``` 结尾
            if (text.EndsWith("```"))
                text = text.Substring(0, text.Length - 3);

            return text.Trim();
        }

        /// <summary>
        /// 发送请求到 AI API
        /// </summary>
        private static async Task<string> SendRequestAsync(IAppSettings settings, string prompt)
        {
            var requestBody = new
            {
                model = settings.AiModel,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, OpenRouterUrl);
            request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
            request.Headers.Add("HTTP-Referer", "https://github.com/YourName/QQLyric2Roma");
            request.Headers.Add("X-Title", "QQLyric2Roma");
            request.Content = jsonContent;

            try
            {
                var response = await _client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"API 请求失败: {response.StatusCode}\n{responseString}";
                }

                var jsonNode = JsonNode.Parse(responseString);
                string content = jsonNode?["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    return "错误：AI 返回了空内容";
                }

                return content.Trim();
            }
            catch (Exception ex)
            {
                return $"发生异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 发送歌词给 AI 并获取罗马音（兼容旧接口）
        /// </summary>
        [Obsolete("请使用 GetRomajiAsync(IAppSettings, string) 重载")]
        public static async Task<string> GetRomajiAsync(string apiKey, string lyrics, string model)
        {
            if (string.IsNullOrEmpty(apiKey)) return "错误：未设置 API Key";

            var prompt = Preferences.Get(Constants.PrefKeyAiPrompt, Constants.DefaultRomaPrompt) + lyrics;

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, OpenRouterUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("HTTP-Referer", "https://github.com/YourName/QQLyric2Roma");
            request.Headers.Add("X-Title", "QQLyric2Roma");
            request.Content = jsonContent;

            try
            {
                var response = await _client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"API 请求失败: {response.StatusCode}\n{responseString}";
                }

                var jsonNode = JsonNode.Parse(responseString);
                string content = jsonNode?["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    return "AI 返回了空内容，请检查日志。";
                }

                return content.Trim();
            }
            catch (Exception ex)
            {
                return $"发生异常: {ex.Message}";
            }
        }
    }
}