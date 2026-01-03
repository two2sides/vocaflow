using QQLyric2Roma.Models;
using SQLite;

namespace QQLyric2Roma.Services
{
    /// <summary>
    /// SQLite 数据库服务实现
    /// </summary>
    public class DatabaseService : IDatabase
    {
        private SQLiteAsyncConnection _db;
        private readonly string _dbPath;

        public DatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "qqlr.db3");
        }

        /// <summary>
        /// 初始化数据库连接和表
        /// </summary>
        private async Task InitAsync()
        {
            if (_db != null) return;

            _db = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

            // 创建表（如果不存在）
            await _db.CreateTableAsync<SavedLyric>();
            await _db.CreateTableAsync<VocabEntry>();
        }

        #region 歌词相关

        public async Task<List<SavedLyric>> GetLyricsAsync()
        {
            await InitAsync();
            return await _db.Table<SavedLyric>()
                            .OrderByDescending(x => x.CreatedAt)
                            .ToListAsync();
        }

        public async Task<SavedLyric> GetLyricByIdAsync(int id)
        {
            await InitAsync();
            return await _db.Table<SavedLyric>()
                            .Where(x => x.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveLyricAsync(SavedLyric lyric)
        {
            await InitAsync();
            lyric.CreatedAt = DateTime.Now;
            return await _db.InsertAsync(lyric);
        }

        public async Task<int> UpdateLyricAsync(SavedLyric lyric)
        {
            await InitAsync();
            return await _db.UpdateAsync(lyric);
        }

        public async Task<int> DeleteLyricAsync(SavedLyric lyric)
        {
            await InitAsync();
            return await _db.DeleteAsync(lyric);
        }

        public async Task<bool> IsLyricSavedAsync(string songMid)
        {
            await InitAsync();
            var count = await _db.Table<SavedLyric>()
                                 .Where(x => x.SongMid == songMid)
                                 .CountAsync();
            return count > 0;
        }

        #endregion

        #region 词汇相关

        public async Task<List<VocabEntry>> GetVocabsAsync()
        {
            await InitAsync();
            return await _db.Table<VocabEntry>()
                            .OrderByDescending(x => x.CreatedAt)
                            .ToListAsync();
        }

        public async Task<List<VocabEntry>> GetVocabsByLanguageAsync(string language)
        {
            await InitAsync();
            return await _db.Table<VocabEntry>()
                            .Where(x => x.Language == language)
                            .OrderByDescending(x => x.CreatedAt)
                            .ToListAsync();
        }

        public async Task<VocabEntry> GetVocabByIdAsync(int id)
        {
            await InitAsync();
            return await _db.Table<VocabEntry>()
                            .Where(x => x.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveVocabAsync(VocabEntry vocab)
        {
            await InitAsync();
            vocab.CreatedAt = DateTime.Now;
            return await _db.InsertAsync(vocab);
        }

        public async Task<int> UpdateVocabAsync(VocabEntry vocab)
        {
            await InitAsync();
            return await _db.UpdateAsync(vocab);
        }

        public async Task<int> DeleteVocabAsync(VocabEntry vocab)
        {
            await InitAsync();
            return await _db.DeleteAsync(vocab);
        }

        public async Task<List<VocabEntry>> SearchVocabAsync(string keyword, string language = null)
        {
            await InitAsync();

            var query = _db.Table<VocabEntry>();

            if (!string.IsNullOrEmpty(language))
            {
                query = query.Where(x => x.Language == language);
            }

            var allItems = await query.ToListAsync();

            // SQLite-net 的 Contains 支持有限，在内存中过滤
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                allItems = allItems.Where(x =>
                    (x.Word?.ToLower().Contains(keyword) ?? false) ||
                    (x.Meaning?.ToLower().Contains(keyword) ?? false) ||
                    (x.Reading?.ToLower().Contains(keyword) ?? false)
                ).ToList();
            }

            return allItems.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<bool> IsVocabSavedAsync(string word, string language)
        {
            await InitAsync();
            var count = await _db.Table<VocabEntry>()
                                 .Where(x => x.Word == word && x.Language == language)
                                 .CountAsync();
            return count > 0;
        }

        public async Task<VocabEntry> GetVocabByWordAsync(string word, string language)
        {
            await InitAsync();
            return await _db.Table<VocabEntry>()
                            .Where(x => x.Word == word && x.Language == language)
                            .FirstOrDefaultAsync();
        }

        public async Task<List<VocabEntry>> GetRandomVocabsAsync(int count, string language = null)
        {
            await InitAsync();

            var query = _db.Table<VocabEntry>();
            if (!string.IsNullOrEmpty(language))
            {
                query = query.Where(x => x.Language == language);
            }

            var allItems = await query.ToListAsync();

            // 随机打乱并取前 N 个
            var random = new Random();
            return allItems.OrderBy(x => random.Next()).Take(count).ToList();
        }

        #endregion
    }
}
