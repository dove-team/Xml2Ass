using System;
using System.Collections.Generic;
using System.Linq;

namespace Xml2Ass
{
    internal class AssSubtitle
    {
        private readonly Danmaku danmaku;
        private readonly Dictionary<int, float> bottomSubtitles;
        private readonly Dictionary<int, float> topSubtitles;
        private readonly int videoWidth;
        private readonly int videoHeight;
        private readonly int baseFontSize;
        private readonly int lineCount;
        private readonly int bottomMargin;
        private readonly float tuneSeconds;
        private readonly int textLength;
        private readonly float startTime;
        private readonly float endTime;
        private readonly int fontSize;
        private readonly Position position;
        private readonly string styledText;
        public AssSubtitle(Danmaku danmaku, Dictionary<int, float> bottomSubtitles, Dictionary<int, float> topSubtitles, int videoWidth, int videoHeight, int baseFontSize, int lineCount, int bottomMargin, float tuneSeconds)
        {
            this.danmaku = danmaku;
            this.bottomSubtitles = bottomSubtitles;
            this.topSubtitles = topSubtitles;
            this.videoWidth = videoWidth;
            this.videoHeight = videoHeight;
            this.baseFontSize = baseFontSize;
            this.lineCount = lineCount;
            this.bottomMargin = bottomMargin;
            this.tuneSeconds = tuneSeconds;
            textLength = danmaku.Content.Length;
            startTime = danmaku.ShowTime;
            endTime = GetEndTime(danmaku.ShowTime, this.tuneSeconds);
            fontSize = (int)danmaku.Size - 25 + baseFontSize;
            position = GetPosition();
            styledText = GetStyledText();
        }
        public override string ToString()
        {
            string format(string s)
            {
                if (!s.Contains('.')) return s.Insert(s.Length, ".00");
                if (s.Substring(s.LastIndexOf('.')).Length == 2) return s + "0";
                if (s.Substring(s.LastIndexOf('.')).Length > 3)
                {
                    var index = s.LastIndexOf('.');
                    return s.Remove(index + 3, s.Length - index - 3);
                }
                return s;
            }
            var start = format(TimeSpan.FromSeconds(startTime).ToString("c"));
            var end = format(TimeSpan.FromSeconds(endTime).ToString("c"));
            return $"Dialogue: 3,{start},{end},AcplayDefault,,0000,0000,0000,,{styledText}";
        }
        private string GetStyledText()
        {
            string colourMarkup;
            string borderMarkup;
            string fontSizeMarkup;
            string styleMarkup;
            if (danmaku.Colour == 16777215) colourMarkup = "";
            else colourMarkup = $"\\c&H{danmaku.Colour.ToHexString()}";
            if (NeedWhiteBorder(danmaku)) borderMarkup = "\\3c&HFFFFFF";
            else borderMarkup = "";
            if (fontSize == baseFontSize) fontSizeMarkup = "";
            else fontSizeMarkup = $"\\fs{fontSize}";
            if (danmaku.Type == DanmakuType.Normal || danmaku.Type == DanmakuType.Normal2
                || danmaku.Type == DanmakuType.Normal3 || danmaku.Type == DanmakuType.Reverse)
                styleMarkup = $"\\move({position.X1},{position.Y1},{position.X2},{position.Y2})";
            else
                styleMarkup = $"\\a6\\pos({position.X1},{position.Y1})";
            return $"{{{string.Join(string.Empty, styleMarkup, colourMarkup, borderMarkup, fontSizeMarkup)}}}{danmaku.Content}";
        }
        private bool NeedWhiteBorder(Danmaku danmaku)
        {
            var colour = danmaku.Colour;
            if (colour == 0) return true;
            var hls = danmaku.Colour.ToRgb().ToHls();
            if (hls.H > 30 && hls.H < 210 && hls.L < 33) return true;
            if ((hls.L < 30 || hls.H > 210) && hls.L < 66) return true;
            return false;
        }
        private Position GetPosition()
        {
            var position = new Position();
            if (danmaku.Type == DanmakuType.Normal || danmaku.Type == DanmakuType.Normal2 || danmaku.Type == DanmakuType.Normal3)
            {
                position.X1 = videoWidth + baseFontSize * textLength / 2;
                position.X2 = -(baseFontSize * textLength) / 2;
                int y = (danmaku.Index % lineCount + 1) * baseFontSize;
                if (y < fontSize) y = fontSize;
                position.Y1 = position.Y2 = y;
            }
            else if (danmaku.Type == DanmakuType.Bottom)
            {
                var lineIndex = ChooseLineCount(bottomSubtitles, startTime);
                if (bottomSubtitles.ContainsKey(lineIndex))
                    bottomSubtitles[lineIndex] = endTime;
                else
                    bottomSubtitles.Add(lineIndex, endTime);
                int x = videoWidth / 2;
                int y = videoHeight - (baseFontSize * lineIndex + bottomMargin);
                position.X1 = position.X2 = x;
                position.Y1 = position.Y2 = y;
            }
            else if (danmaku.Type == DanmakuType.Top)
            {
                var lineIndex = ChooseLineCount(topSubtitles, startTime);
                if (topSubtitles.ContainsKey(lineIndex))
                    topSubtitles[lineIndex] = endTime;
                else
                    topSubtitles.Add(lineIndex, endTime);
                int x = videoWidth / 2;
                int y = baseFontSize * lineIndex + 1;
                position.X1 = position.X2 = x;
                position.Y1 = position.Y2 = y;
            }
            else if (danmaku.Type == DanmakuType.Reverse)
            {
                position.X2 = videoWidth + baseFontSize * textLength / 2;
                position.X1 = -(baseFontSize * textLength) / 2;
                int y = (danmaku.Index % lineCount + 1) * baseFontSize;
                if (y < fontSize) y = fontSize;
                position.Y1 = position.Y2 = y;
            }
            else
            {
                throw new ArgumentException($"不支持的弹幕类型:内容为{danmaku.Content}，类型为{danmaku.Type}");
            }
            return position;
        }
        private int ChooseLineCount(Dictionary<int, float> subtitles, float startTime)
        {
            int lineIndex;
            var toRemove = new List<int>();
            foreach (var item in subtitles)
            {
                if (item.Value <= startTime)
                    toRemove.Add(item.Key);
            }
            toRemove.ForEach(s => subtitles.Remove(s));
            if (subtitles.Count == 0)
                lineIndex = 0;
            else if (subtitles.Count == subtitles.Keys.Max())
                lineIndex = subtitles.Keys.Min();
            else
            {
                lineIndex = 0;
                for (int i = 0; i < subtitles.Count; i++)
                {
                    if (!subtitles.ContainsKey(i))
                    {
                        lineIndex = i;
                        break;
                    }
                }
                if (lineIndex == 0)
                    lineIndex = subtitles.Count;
            }
            return lineIndex;
        }
        private float GetEndTime(float showTime, float offset)
        {
            if (danmaku.Type == DanmakuType.Bottom || danmaku.Type == DanmakuType.Top)
                return showTime + 4;
            float endTime;
            if (textLength < 5) endTime = showTime + 7 + textLength / 1.5f;
            else if (textLength < 12) endTime = showTime + 7 + textLength / 2;
            else endTime = showTime + 13;
            endTime += offset;
            return endTime;
        }
        private struct Position
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }
        }
    }
}