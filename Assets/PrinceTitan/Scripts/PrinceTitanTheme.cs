using UnityEngine;

namespace PrinceTitan
{
    public static class PrinceTitanTheme
    {
        public static readonly Color Ink = Hex("#19171C");
        public static readonly Color InkSoft = Hex("#29252C");
        public static readonly Color InkRaised = Hex("#36313A");
        public static readonly Color Magenta = Hex("#D82B78");
        public static readonly Color MagentaDark = Hex("#8E1F55");
        public static readonly Color Ivory = Hex("#F7F0E8");
        public static readonly Color Paper = Hex("#E8D2A3");
        public static readonly Color PaperLight = Hex("#F3E4C0");
        public static readonly Color PaperInk = Hex("#4A3329");
        public static readonly Color Brass = Hex("#C8A15D");
        public static readonly Color Muted = Hex("#A49AA6");
        public static readonly Color Success = Hex("#76B99D");

        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cachedFont;
            }
        }

        public static Color Hex(string value)
        {
            Color color;
            return ColorUtility.TryParseHtmlString(value, out color) ? color : Color.white;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
