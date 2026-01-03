using SQLite;

namespace QQLyric2Roma.Models
{
    /// <summary>
    /// 保存的歌词实体
    /// </summary>
    public class SavedLyric
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// 歌曲 MID
        /// </summary>
        public string SongMid { get; set; }

        /// <summary>
        /// 歌曲标题
        /// </summary>
        public string SongTitle { get; set; }

        /// <summary>
        /// 歌手
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// 原始歌词
        /// </summary>
        public string OriginalLyric { get; set; }

        /// <summary>
        /// 罗马音
        /// </summary>
        public string Romaji { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 用于列表显示
        /// </summary>
        [Ignore]
        public string DisplayText => $"{SongTitle} - {Artist}";

        /// <summary>
        /// 格式化的创建时间
        /// </summary>
        [Ignore]
        public string CreatedAtText => CreatedAt.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 是否选中（用于批量删除）
        /// </summary>
        [Ignore]
        public bool IsSelected { get; set; }
    }
}
