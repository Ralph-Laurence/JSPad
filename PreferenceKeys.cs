namespace JSPad
{
    // DEFAULT SETTINGS 
    public struct PreferenceDefaults
    {
        public static readonly int FontSize = 12;
        public static readonly bool PrefsChanged = false;
    }

    class PreferenceKeys
    { 
        // GENERAL PREFS
        public static readonly string PreferenceName = "JSPAD_PREFS";
        public static readonly string PrivacyAgreementDontShowAgain = "DONT_SHOW_AGAIN";
        public static readonly string UseDefaults = "USE_DEFAULTS";
        public static readonly string PrefsChanged = "PREFS_CHANGED";

        // EDITOR PREFS
        public static readonly string EditorTheme = "EDITOR_THEME";
        public static readonly string EditorFontSize = "EDITOR_FONT_SIZE";
        public static readonly string EditorTabSize = "EDITOR_TAB_SIZE";
        public static readonly string EnableIntellisense = "ENABLE_INTELLISENSE";
        public static readonly string EnableSnippets = "ENABLE_SNIPPETS";
        public static readonly string EnableIndent = "ENABLE_INDENT";
        public static readonly string ShowFolding = "SHOW_FOLD";
        public static readonly string ShowLineNumbering = "SHOW_LINE_NUM";
    }
}