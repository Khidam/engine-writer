using UnityEngine;

namespace PrinceTitan
{
    /// <summary>
    /// One restrained visual language for every room. Font sizes are defined by
    /// the screens; the smallest interactive copy is deliberately kept readable.
    /// </summary>
    public static class PrinceTitanTheme
    {
        public static readonly Color Black = Hex("#0A090C");
        public static readonly Color Ink = Hex("#151319");
        public static readonly Color InkSoft = Hex("#231F27");
        public static readonly Color InkRaised = Hex("#332D36");
        public static readonly Color Magenta = Hex("#E22A82");
        public static readonly Color MagentaDark = Hex("#8D174E");
        public static readonly Color Ivory = Hex("#FFF8ED");
        public static readonly Color Paper = Hex("#EAD8B7");
        public static readonly Color PaperLight = Hex("#FFF3D7");
        public static readonly Color PaperInk = Hex("#33251F");
        public static readonly Color Brass = Hex("#D4AD64");
        public static readonly Color Muted = Hex("#B9AFB9");
        public static readonly Color Success = Hex("#79D2AA");
        public static readonly Color Government = Hex("#B8D8E7");
        public static readonly Color Clan = Hex("#E0A34D");
        public static readonly Color Contractor = Hex("#55C9C2");
        public static readonly Color Danger = Hex("#F06A76");

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

        public static Color FactionColor(PowerKind kind)
        {
            switch (kind)
            {
                case PowerKind.Empire: return Magenta;
                case PowerKind.Government: return Government;
                case PowerKind.Clan: return Clan;
                case PowerKind.Contractor: return Contractor;
                default: return Ivory;
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
