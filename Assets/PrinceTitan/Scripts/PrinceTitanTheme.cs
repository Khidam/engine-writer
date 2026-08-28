using UnityEngine;

namespace PrinceTitan
{
    /// <summary>
    /// One restrained visual language for every room. Font sizes are defined by
    /// the screens; the smallest interactive copy is deliberately kept readable.
    /// </summary>
    public static class PrinceTitanTheme
    {
        public static readonly Color Black = Hex("#090A09");
        public static readonly Color Ink = Hex("#151713");
        public static readonly Color InkSoft = Hex("#242720");
        public static readonly Color InkRaised = Hex("#34362F");
        public static readonly Color Olive = Hex("#42483A");
        public static readonly Color Magenta = Hex("#E72B86");
        public static readonly Color MagentaDark = Hex("#851746");
        public static readonly Color Ivory = Hex("#FFF8ED");
        public static readonly Color Paper = Hex("#D8C29C");
        public static readonly Color PaperLight = Hex("#F4E6C7");
        public static readonly Color PaperDark = Hex("#8B7656");
        public static readonly Color PaperInk = Hex("#28231D");
        public static readonly Color Brass = Hex("#CBA45D");
        public static readonly Color Muted = Hex("#B7B3A8");
        public static readonly Color Success = Hex("#82D6A7");
        public static readonly Color Government = Hex("#B8D8E7");
        public static readonly Color Clan = Hex("#E0A34D");
        public static readonly Color Contractor = Hex("#55C9C2");
        public static readonly Color Danger = Hex("#F06A76");

        private static Font cachedFont;
        private static Font cachedMonoFont;

        public static Font Font
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Resources.Load<Font>("PrinceTitan/Fonts/PrinceTitanCondensed");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cachedFont;
            }
        }

        public static Font MonoFont
        {
            get
            {
                if (cachedMonoFont != null) return cachedMonoFont;
                cachedMonoFont = Resources.Load<Font>("PrinceTitan/Fonts/PrinceTitanTypewriter");
                return cachedMonoFont != null ? cachedMonoFont : Font;
            }
        }

        public static Color OrganizationColor(OrganizationKind kind)
        {
            switch (kind)
            {
                case OrganizationKind.Empire: return Magenta;
                case OrganizationKind.Government: return Government;
                case OrganizationKind.Clan: return Clan;
                case OrganizationKind.Contractor: return Contractor;
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
