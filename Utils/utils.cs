using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace VocaFlow.Utils
{
    public static class QQMusicApi
    {
        // 使用静态 HttpClient 以避免套接字耗尽问题
        private static readonly HttpClient _client = new HttpClient();

        // 静态构造函数，用于初始化一些全局通用的 Header
        static QQMusicApi()
        {
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0");
        }

        /// <summary>
        /// 获取音乐 URL
        /// </summary>
        /// <param name="songmid">歌曲 MID</param>
        /// <param name="quality">音质: m4a, 128, 320(默认)</param>
        /// <param name="origin">是否返回原始 JSON 数据</param>
        public static async Task<string> GetMusicUrlAsync(string songmid, string quality = "320", bool origin = false)
        {
            string filePrefix;
            string fileSuffix;

            // 处理音质参数
            if (quality.ToLower() == "m4a")
            {
                filePrefix = "C400";
                fileSuffix = "m4a";
            }
            else if (quality == "128")
            {
                filePrefix = "M500";
                fileSuffix = "mp3";
            }
            else // 默认 320
            {
                filePrefix = "M800";
                fileSuffix = "mp3";
            }

            // 构造文件名
            string filename = $"{filePrefix}{songmid}{songmid}.{fileSuffix}";

            // 构造请求体 JSON 字符串
            string bodyJson = "{\"req_1\":{\"module\":\"vkey.GetVkeyServer\",\"method\":\"CgiGetVkey\",\"param\":{\"filename\":[\"FILENAME\"],\"guid\":\"10000\",\"songmid\":[\"SONGMID\"],\"songtype\":[0],\"uin\":\"0\",\"loginflag\":1,\"platform\":\"20\"}},\"loginUin\":\"0\",\"comm\":{\"uin\":\"0\",\"format\":\"json\",\"ct\":24,\"cv\":0}}"
                .Replace("SONGMID", songmid)
                .Replace("FILENAME", filename);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://u.y.qq.com/cgi-bin/musicu.fcg");
            request.Headers.Add("Referer", "https://y.qq.com/");
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);

                if (origin) return jsonString;

                // 提取 URL
                var reqData = jsonNode?["req_1"]?["data"];
                if (reqData == null) return null;

                string sip = reqData["sip"]?[0]?.ToString();
                string purl = reqData["midurlinfo"]?[0]?["purl"]?.ToString();

                if (string.IsNullOrEmpty(purl)) return null; // 无法获取链接

                return sip + purl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMusicUrlAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取歌单歌曲信息
        /// </summary>
        public static async Task<JsonNode> GetSongListAsync(string categoryID, bool origin = false)
        {
            string url = $"https://i.y.qq.com/qzone-music/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&nosign=1&disstid={categoryID}&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";

            try
            {
                string jsonString = await _client.GetStringAsync(url);
                var jsonNode = JsonNode.Parse(jsonString);

                if (origin) return jsonNode;
                return jsonNode?["cdlist"]?[0]?["songlist"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSongListAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取歌单名称
        /// </summary>
        public static async Task<string> GetSongListNameAsync(string categoryID)
        {
            // 复用 GetSongListAsync 获取完整数据
            var data = await GetSongListAsync(categoryID, true);
            return data?["cdlist"]?[0]?["dissname"]?.ToString();
        }

        /// <summary>
        /// 关键词搜索
        /// </summary>
        public static async Task<JsonNode> SearchWithKeywordAsync(string keyword, int searchType = 0, int resultNum = 50, int pageNum = 1, bool origin = false)
        {
            string bodyJson = "{\"comm\":{\"ct\":\"19\",\"cv\":\"1859\",\"uin\":\"0\"},\"req\":{\"method\":\"DoSearchForQQMusicDesktop\",\"module\":\"music.search.SearchCgiService\",\"param\":{\"grp\":1,\"num_per_page\":RESULTNUM,\"page_num\":PAGENUM,\"query\":\"KEYWORD\",\"search_type\":SEARCHTYPE}}}"
                .Replace("KEYWORD", keyword)
                .Replace("RESULTNUM", resultNum.ToString())
                .Replace("PAGENUM", pageNum.ToString())
                .Replace("SEARCHTYPE", searchType.ToString());

            var request = new HttpRequestMessage(HttpMethod.Post, "https://u.y.qq.com/cgi-bin/musicu.fcg");
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.SendAsync(request);
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);

                if (origin) return jsonNode;

                var body = jsonNode?["req"]?["data"]?["body"];
                if (body == null) return null;

                return searchType switch
                {
                    0 or 7 => body["song"],
                    2 => body["album"],
                    3 => body["songlist"],
                    4 => body["mv"],
                    8 => body["user"],
                    _ => body
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchWithKeywordAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取歌词 (包含解析逻辑)
        /// </summary>
        public static async Task<object> GetSongLyricAsync(string songmid, bool parse = false, bool origin = false)
        {
            string url = $"https://i.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={songmid}&g_tk=5381&format=json&inCharset=utf8&outCharset=utf-8&nobase64=1";

            try
            {
                // 注意：这里需要添加 Referer，否则可能无法获取歌词
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Referer", "https://y.qq.com/");

                var response = await _client.SendAsync(request);
                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);

                if (origin) return jsonNode;

                string lyric = jsonNode?["lyric"]?.ToString() ?? "";
                string trans = jsonNode?["trans"]?.ToString() ?? "";

                if (!parse)
                {
                    return lyric + "\n" + trans;
                }
                else
                {
                    return ParseLyric(lyric, trans);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSongLyricAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 歌词解析对象结构
        /// </summary>
        public class LyricResult
        {
            public string Title { get; set; } = "";
            public string Artist { get; set; } = "";
            public string Album { get; set; } = "";
            public string By { get; set; } = "";
            public string Offset { get; set; } = "";
            public int Count { get; set; } = 0;
            public bool HaveTrans { get; set; } = false;
            public List<LyricLine> LyricList { get; set; } = new List<LyricLine>();
        }

        public class LyricLine
        {
            public string Time { get; set; }
            public string Lyric { get; set; }
            public string Trans { get; set; }
        }

        /// <summary>
        /// 解析歌词 (私有辅助方法)
        /// </summary>
        private static LyricResult ParseLyric(string rawLyric, string rawTrans)
        {
            var result = new LyricResult();

            // 处理 HTML 实体解码 (虽然源JS没做，但在C#中建议做一下，QQ音乐有时会返回编码字符)
            // rawLyric = System.Web.HttpUtility.HtmlDecode(rawLyric); 

            string[] lyricLines = rawLyric.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] transLines = rawTrans.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            result.HaveTrans = transLines.Length > 0;

            // 辅助函数：提取 [key:value] 中的 value
            string GetTagValue(string line)
            {
                int colonIndex = line.IndexOf(':');
                int bracketIndex = line.IndexOf(']');
                if (colonIndex > 0 && bracketIndex > colonIndex)
                {
                    return line.Substring(colonIndex + 1, bracketIndex - colonIndex - 1);
                }
                return "";
            }

            int startIndex = 0;

            // 尝试解析头部元数据 (ti, ar, al, etc.)
            // 假设前几行包含元数据且不以 [0 开头
            if (lyricLines.Length > 0 && !lyricLines[0].StartsWith("[0"))
            {
                // 简单的索引保护，防止越界
                if (lyricLines.Length > 0) result.Title = GetTagValue(lyricLines[0]);
                if (lyricLines.Length > 1) result.Artist = GetTagValue(lyricLines[1]);
                if (lyricLines.Length > 2) result.Album = GetTagValue(lyricLines[2]);
                if (lyricLines.Length > 3) result.By = GetTagValue(lyricLines[3]);
                if (lyricLines.Length > 4) result.Offset = GetTagValue(lyricLines[4]);

                startIndex = 5; // 跳过前5行
            }

            // 如果有翻译，也需要同步跳过头部
            var actualTransLines = new List<string>(transLines);
            if (result.HaveTrans && startIndex > 0 && transLines.Length >= startIndex)
            {
                actualTransLines = new List<string>(transLines[startIndex..]); // C# 切片语法
            }

            // 处理具体的歌词行
            for (int i = startIndex; i < lyricLines.Length; i++)
            {
                string line = lyricLines[i];
                int closeBracket = line.IndexOf(']');
                if (closeBracket == -1) continue;

                var ele = new LyricLine();
                // 提取时间: [00:00.00] -> 00:00.00
                // C# Substring(start, length)
                if (line.Length > 1)
                    ele.Time = line.Substring(1, closeBracket - 1);

                // 提取歌词
                if (line.Length > closeBracket + 1)
                    ele.Lyric = line.Substring(closeBracket + 1);

                // 提取翻译 (匹配索引)
                int transIndex = i - startIndex;
                if (result.HaveTrans && transIndex < actualTransLines.Count)
                {
                    string tLine = actualTransLines[transIndex];
                    int tCloseBracket = tLine.IndexOf(']');
                    if (tCloseBracket != -1 && tLine.Length > tCloseBracket + 1)
                    {
                        ele.Trans = tLine.Substring(tCloseBracket + 1);
                    }
                }

                result.LyricList.Add(ele);
            }

            result.Count = result.LyricList.Count;
            return result;
        }

        // 获取专辑封面小工具
        public static string GetAlbumCoverImage(string albummid)
        {
            return $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albummid}.jpg";
        }
    }
}