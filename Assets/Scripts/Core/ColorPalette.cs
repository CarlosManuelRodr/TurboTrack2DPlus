using UnityEngine;

namespace Core
{
    /// <summary>
    /// DB32 Color palette.
    /// </summary>
    public enum DB32Color
    {
        Black, Valhalla, Loulou, OiledCedar, Rope, TahitiGold, Twine, Pancho,
        GoldenFizz, Atlantis, Christi, ElfGreen, Dell, Verdigris, Opal, DeepKoamaru,
        VeniceBlue, RoyalBlue, Cornflower, Viking, LightSteelBlue, White, Heather, 
        Topaz, DimGray, SmokeyAsh, Clairvoyant, Brown, Mandy, Plum, Rainforest, Stinger
    }
    
    /// <summary>
    /// Get color from color name.
    /// </summary>
    public static class ColorPaletteExtensions
    {
        public static Color GetColor(this DB32Color db32Color)
        {
            return db32Color switch
            {
                DB32Color.Black => new Color(0f, 0f, 0f),
                DB32Color.Valhalla => new Color(0.133333f, 0.12549f, 0.203922f),
                DB32Color.Loulou => new Color(0.270588f, 0.156863f, 0.235294f),
                DB32Color.OiledCedar => new Color(0.4f, 0.223529f, 0.192157f),
                DB32Color.Rope => new Color(0.560784f, 0.337255f, 0.231373f),
                DB32Color.TahitiGold => new Color(0.87451f, 0.443137f, 0.14902f),
                DB32Color.Twine => new Color(0.85098f, 0.627451f, 0.4f),
                DB32Color.Pancho => new Color(0.933333f, 0.764706f, 0.603922f),
                DB32Color.GoldenFizz => new Color(0.984314f, 0.94902f, 0.211765f),
                DB32Color.Atlantis => new Color(0.6f, 0.898039f, 0.313725f),
                DB32Color.Christi => new Color(0.415686f, 0.745098f, 0.188235f),
                DB32Color.ElfGreen => new Color(0.215686f, 0.580392f, 0.431373f),
                DB32Color.Dell => new Color(0.294118f, 0.411765f, 0.184314f),
                DB32Color.Verdigris => new Color(0.321569f, 0.294118f, 0.141176f),
                DB32Color.Opal => new Color(0.196078f, 0.235294f, 0.223529f),
                DB32Color.DeepKoamaru => new Color(0.247059f, 0.247059f, 0.454902f),
                DB32Color.VeniceBlue => new Color(0.188235f, 0.376471f, 0.509804f),
                DB32Color.RoyalBlue => new Color(0.356863f, 0.431373f, 0.882353f),
                DB32Color.Cornflower => new Color(0.388235f, 0.607843f, 1f),
                DB32Color.Viking => new Color(0.372549f, 0.803922f, 0.894118f),
                DB32Color.LightSteelBlue => new Color(0.796078f, 0.858824f, 0.988235f),
                DB32Color.White => new Color(1f, 1f, 1f),
                DB32Color.Heather => new Color(0.607843f, 0.678431f, 0.717647f),
                DB32Color.Topaz => new Color(0.517647f, 0.494118f, 0.529412f),
                DB32Color.DimGray => new Color(0.411765f, 0.415686f, 0.415686f),
                DB32Color.SmokeyAsh => new Color(0.34902f, 0.337255f, 0.321569f),
                DB32Color.Clairvoyant => new Color(0.462745f, 0.258824f, 0.541176f),
                DB32Color.Brown => new Color(0.67451f, 0.196078f, 0.196078f),
                DB32Color.Mandy => new Color(0.85098f, 0.341176f, 0.388235f),
                DB32Color.Plum => new Color(0.843137f, 0.482353f, 0.729412f),
                DB32Color.Rainforest => new Color(0.560784f, 0.592157f, 0.290196f),
                DB32Color.Stinger => new Color(0.541176f, 0.435294f, 0.188235f),
                _ => Color.white
            };
        }
    }
}