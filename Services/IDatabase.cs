using QQLyric2Roma.Models;

namespace QQLyric2Roma.Services
{
    /// <summary>
    /// 数据库服务接口
    /// </summary>
    public interface IDatabase
    {
        #region 歌词相关

        /// <summary>
        /// 获取所有保存的歌词
        /// </summary>
        Task<List<SavedLyric>> GetLyricsAsync();

        /// <summary>
        /// 根据 ID 获取歌词
        /// </summary>
        Task<SavedLyric> GetLyricByIdAsync(int id);

        /// <summary>
        /// 保存歌词
        /// </summary>
        Task<int> SaveLyricAsync(SavedLyric lyric);

        /// <summary>
        /// 更新歌词
        /// </summary>
        Task<int> UpdateLyricAsync(SavedLyric lyric);

        /// <summary>
        /// 删除歌词
        /// </summary>
        Task<int> DeleteLyricAsync(SavedLyric lyric);

        /// <summary>
        /// 检查歌词是否已保存
        /// </summary>
        Task<bool> IsLyricSavedAsync(string songMid);

        #endregion

        #region 词汇相关

        /// <summary>
        /// 获取所有词汇
        /// </summary>
        Task<List<VocabEntry>> GetVocabsAsync();

        /// <summary>
        /// 按语言获取词汇
        /// </summary>
        Task<List<VocabEntry>> GetVocabsByLanguageAsync(string language);

        /// <summary>
        /// 根据 ID 获取词汇
        /// </summary>
        Task<VocabEntry> GetVocabByIdAsync(int id);

        /// <summary>
        /// 保存词汇
        /// </summary>
        Task<int> SaveVocabAsync(VocabEntry vocab);

        /// <summary>
        /// 更新词汇
        /// </summary>
        Task<int> UpdateVocabAsync(VocabEntry vocab);

        /// <summary>
        /// 删除词汇
        /// </summary>
        Task<int> DeleteVocabAsync(VocabEntry vocab);

        /// <summary>
        /// 搜索词汇
        /// </summary>
        Task<List<VocabEntry>> SearchVocabAsync(string keyword, string language = null);

        /// <summary>
        /// 检查词汇是否已保存
        /// </summary>
        Task<bool> IsVocabSavedAsync(string word, string language);

        /// <summary>
        /// 根据词语和语言获取已保存的词汇
        /// </summary>
        Task<VocabEntry> GetVocabByWordAsync(string word, string language);

        #endregion
    }
}
