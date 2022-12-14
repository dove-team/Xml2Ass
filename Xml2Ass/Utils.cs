using System.Linq;

namespace Xml2Ass
{
    internal static class Utils
    {
        public struct HLS
        {
            public HLS(float h, float l, float s)
            {
                H = h;
                L = l;
                S = s;
            }
            public float H { get; set; }
            public float L { get; set; }
            public float S { get; set; }
        }
        public struct RGB
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
        }
        public static string ToHexString(this int num) => num.ToString("X");
        public static HLS ToHls(this RGB rGB)
        {
            var rgb = new[] { rGB.R, rGB.G, rGB.B };
            var maxc = rgb.Max();
            var minc = rgb.Min();
            float l = (minc + maxc) / 2.0f;
            if (minc == maxc) return new HLS(0.0f, l, 0.0f);
            float s;
            if (l <= 0.5f) s = (maxc - minc) / (maxc + minc);
            else s = (maxc - minc) / (2.0f - maxc - minc);
            var rc = (maxc - rgb[0]) / (maxc - minc);
            var gc = (maxc - rgb[1]) / (maxc - minc);
            var bc = (maxc - rgb[2]) / (maxc - minc);
            float h = 0.0f;
            if (rgb[0] == maxc) h = bc - gc;
            if (rgb[1] == maxc) h = 2.0f + rc - bc;
            if (rgb[2] == maxc) h = 4.0f + gc - rc;
            h = h / 6.0f % 1.0f;
            return new HLS(h * 360, l * 100, s * 100);
        }
        public static RGB ToRgb(this int num)
        {
            return new RGB
            {
                B = num & 0xff,
                G = (num >> 8) & 0xff,
                R = (num >> 16) & 0xff
            };
        }
    }
}