using System;
using System.Collections.Generic;
using System.Text;

namespace QQLyric2Roma.Models
{
    public class Song
    {
        public string SongMid { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        // 用于显示在列表中的格式化字符串
        public string DisplayText => $"{Title} - {Artist}";
    }
}
