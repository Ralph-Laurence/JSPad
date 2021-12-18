namespace JSPad 
{
    class SettingsItem
    {
        public string Theme { get; set; }
        public int FontSize { get; set; }
        public int TabSize { get; set; }

        public bool EnableAutocomplete {get; set;}
        public bool EnableSnippets { get; set; }
        public bool EnableIndentGuide { get; set; }
        public bool ShowFold { get; set; }
        public bool ShowLineNums { get; set; }
    }
}