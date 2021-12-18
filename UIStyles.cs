using Android.Content;
using Android.Graphics;

using AndroidX.Core.Content;

using System.Globalization;

namespace JSPad
{
    class UIStyles
    {
        // Color conversion -> https://stackoverflow.com/a/38267564 -> Thanks FetFrumos!

        // Primary Color
        public static Color GetPrimaryColor(Context context) => 
            new Color(ContextCompat.GetColor(context, Resource.Color.colorPrimary));

        // Primary Dark
        public static Color GetPrimaryDarkColor(Context context) =>
            new Color(ContextCompat.GetColor(context, Resource.Color.colorPrimaryDark));

        // Accent Color
        public static Color GetAccentColor(Context context) =>
            new Color(ContextCompat.GetColor(context, Resource.Color.colorAccent));

        public static Color StandardWhite(Context context) =>
            new Color(ContextCompat.GetColor(context, Resource.Color.standard_white));

        public static Color MaterialRed(Context context) =>
            new Color(ContextCompat.GetColor(context, Resource.Color.material_red));

        public static string ToTitleCase(string original) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(original);
    }
}