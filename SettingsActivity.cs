using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.AppCompat.Widget;

using Google.Android.Material.Slider;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "SettingsActivity")] // , MainLauncher = true
    public class SettingsActivity : Activity 
    {
        private PreferenceManager Prefs = null;
        private TextView FontSizeText = null;
        private TextView TabSizeText = null;
        private TextView ThemeText = null;

        private SwitchCompat IntellisenseSwitch = null;
        private SwitchCompat SnippetSwitch = null;
        private SwitchCompat IndentSwitch = null;
        private SwitchCompat FoldSwitch = null;
        private SwitchCompat LineNumSwitch = null;

        private SettingsReader Settings = null;

        private List<SettingsItem> SettingsResult = null;
        private Dialog WaitDialog = null;

        private ImageButton ChangeThemeButton = null;
        private ImageButton ChangeFontSizeButton = null;
        private ImageButton ChangeTabSizeButton = null;

        private TextView VersionText = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.settings_layout);
              
            InitializeViews();
        } 

        private void InitializeViews()
        { 
            WaitDialog = Dialogue.IndefinitePreloaderDialog(this, GetString(Resource.String.dialog_applying_settings)).Create();
            Prefs = new PreferenceManager(BaseContext);
            Settings = new SettingsReader(this, BaseContext);

            ThemeText = FindViewById<TextView>(Resource.Id.textview_theme_indicator);
            FontSizeText = FindViewById<TextView>(Resource.Id.textview_font_size_indicator);
            TabSizeText = FindViewById<TextView>(Resource.Id.textview_tab_indicator);

            IntellisenseSwitch = FindViewById<SwitchCompat>(Resource.Id.intellisense_switch);
            SnippetSwitch = FindViewById<SwitchCompat>(Resource.Id.snippet_switch);
            IndentSwitch = FindViewById<SwitchCompat>(Resource.Id.indent_switch);
            FoldSwitch = FindViewById<SwitchCompat>(Resource.Id.code_folding_switch);
            LineNumSwitch = FindViewById<SwitchCompat>(Resource.Id.line_num_switch);

            ChangeThemeButton = FindViewById<ImageButton>(Resource.Id.setting_button_change_theme);
            ChangeFontSizeButton = FindViewById<ImageButton>(Resource.Id.btn_change_font_size);
            ChangeTabSizeButton = FindViewById<ImageButton>(Resource.Id.btn_change_tabsize);

            VersionText = FindViewById<TextView>(Resource.Id.about_version_text);

            ThemeText.Text = UIStyles.ToTitleCase(Prefs.GetString(PreferenceKeys.EditorTheme));
            FontSizeText.Text = Prefs.GetInt(PreferenceKeys.EditorFontSize).ToString();
            TabSizeText.Text = Prefs.GetInt(PreferenceKeys.EditorTabSize).ToString();

            IntellisenseSwitch.Checked = Prefs.GetBool(PreferenceKeys.EnableIntellisense);  
            SnippetSwitch.Checked = Prefs.GetBool(PreferenceKeys.EnableSnippets);  
            IndentSwitch.Checked = Prefs.GetBool(PreferenceKeys.EnableIndent);  
            FoldSwitch.Checked = Prefs.GetBool(PreferenceKeys.ShowFolding); 
            LineNumSwitch.Checked = Prefs.GetBool(PreferenceKeys.ShowLineNumbering);

            VersionText.Text = PackageManager.GetPackageInfo(PackageName, 0).VersionName.ToString();

            var settingsTask = Task.Run(async () => await Settings.LoadJsonSettingsAsync());
            SettingsResult = settingsTask.GetAwaiter().GetResult();

            // Add click handlers for each options
            BindOptionEvents();
        } 
        //
        // Check for Switch Changed or Textview clicked
        //
        private void BindOptionEvents()
        {
            //
            // Create an alert dialog for changin themes
            //
            ChangeThemeButton.Click += (s, v) =>
            { 
                var layout = LayoutInflater.From(this);
                var view = layout.Inflate(Resource.Layout.dialog_theme_picker_layout, null);
                
                // View that groups together radio buttons
                var themeSelectorGroup = view.FindViewById<RadioGroup>(Resource.Id.theme_selector_group);

                // Get the initial theme
                var initialTheme = Prefs.GetString(PreferenceKeys.EditorTheme);
                  
                // Set the initial theme's radio button checked
                switch (initialTheme)
                {
                    case "dark":
                        view.FindViewById<RadioButton>(Resource.Id.radio_theme_dark).Checked = true;
                        break;
                    case "gruvbox":
                        view.FindViewById<RadioButton>(Resource.Id.radio_theme_gruvbox).Checked = true;
                        break;
                    case "twilight":
                        view.FindViewById<RadioButton>(Resource.Id.radio_theme_twilight).Checked = true;
                        break;
                    case "clouds":
                        view.FindViewById<RadioButton>(Resource.Id.radio_theme_clouds).Checked = true;
                        break;
                    case "eclipse":
                        view.FindViewById<RadioButton>(Resource.Id.radio_theme_eclipse).Checked = true;
                        break;
                }

                // Store the selected theme's name here
                var selectedTheme = string.Empty;

                var alert = new AlertDialog.Builder(this);

                alert.SetView(view);

                alert.SetTitle(Resource.String.theme_select_title);
                alert.SetIcon(Resource.Drawable.icn_theme);
                alert.SetCancelable(true);

                var dialog = alert.Show();

                themeSelectorGroup.CheckedChange += (s, v) =>
                {
                    var selectedRadio = view.FindViewById<RadioButton>(v.CheckedId);
                    // var idString = Resources.GetResourceEntryName(v.CheckedId); // Int ID to String Entry

                    selectedTheme = selectedRadio.Text.ToLower();

                    Prefs.SetString(PreferenceKeys.EditorTheme, selectedTheme);

                    SettingsResult.ForEach(s => s.Theme = selectedTheme);

                    ThemeText.Text = UIStyles.ToTitleCase(selectedTheme);

                    dialog.Cancel();

                    Toast.MakeText(this, GetString(Resource.String.dialog_theme_applied), ToastLength.Short).Show();
                }; 
            };
            //
            // Change Font Size
            //
            ChangeFontSizeButton.Click += (s, v) =>
            {
                var seekStep = 2;
                var seekMin = 10;
                var seekMax = 20;
                 
                var layout = LayoutInflater.From(this);
                var view = layout.Inflate(Resource.Layout.dialog_slider_layout, null);
                var seekbar = view.FindViewById<SeekBar>(Resource.Id.dialog_seekbar);
                var seekIndicator = view.FindViewById<TextView>(Resource.Id.dialog_seekbar_indicator);

                // Thanks to WannaGetHigh -> https://stackoverflow.com/a/25118544
                seekbar.Max = (seekMax - seekMin) / seekStep;

                // Set initial value 
                var initialFontSize = Prefs.GetInt(PreferenceKeys.EditorFontSize);

                var fontSizes = new Dictionary<int, int>()
                {
                    // Key      -> Actual Font Size
                    // Value    -> Seekbar Equivalent 
                    { 10, 0},
                    { 12, 1},
                    { 14, 2},
                    { 16, 3},
                    { 18, 4},
                    { 20, 5}
                };
                var seekValue = fontSizes[initialFontSize];
                seekbar.Progress = seekValue;
                seekIndicator.Text = initialFontSize.ToString();

                seekbar.ProgressChanged += (s, v) =>
                { 
                    seekValue = seekMin + (v.Progress * seekStep);
                    seekIndicator.Text = seekValue.ToString();
                };

                var alert = new AlertDialog.Builder(this);

                alert.SetView(view);
                alert.SetTitle(Resource.String.dialog_change_font_size_title);
                alert.SetIcon(Resource.Drawable.icn_font_size);
                alert.SetCancelable(false);
                  
                // Bind OK button click
                alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => 
                {
                    Prefs.SetInt(PreferenceKeys.EditorFontSize, seekValue);
                    SettingsResult.ForEach(s => s.FontSize = seekValue);
                    FontSizeText.Text = seekValue.ToString();
                });
                alert.SetNegativeButton(Resource.String.dialog_button_cancel, (e, v) => { });
                alert.Show();
            };
            // 
            // Change Tab Size
            //
            ChangeTabSizeButton.Click += (s, v) =>
            {
                var seekStep = 2;
                var seekMin = 2;
                var seekMax = 12;

                var layout = LayoutInflater.From(this);
                var view = layout.Inflate(Resource.Layout.dialog_slider_layout, null);
                var seekbar = view.FindViewById<SeekBar>(Resource.Id.dialog_seekbar);
                var seekIndicator = view.FindViewById<TextView>(Resource.Id.dialog_seekbar_indicator);

                // Thanks to WannaGetHigh -> https://stackoverflow.com/a/25118544
                seekbar.Max = (seekMax - seekMin) / seekStep;

                // Set initial tab value 
                var initialTabSize = Prefs.GetInt(PreferenceKeys.EditorTabSize);

                var tabSizes = new Dictionary<int, int>()
                {
                    // Key      -> Actual Tab Size
                    // Value    -> Seekbar Equivalent 
                    { 2, 0},
                    { 4, 1},
                    { 6, 2},
                    { 8, 3},
                    { 10, 4},
                    { 12, 5}
                };
                var seekValue = tabSizes[initialTabSize];
                seekbar.Progress = seekValue;
                seekIndicator.Text = initialTabSize.ToString();

                seekbar.ProgressChanged += (s, v) =>
                {
                    seekValue = seekMin + (v.Progress * seekStep);
                    seekIndicator.Text = seekValue.ToString();
                };

                var alert = new AlertDialog.Builder(this);

                alert.SetView(view);
                alert.SetTitle(Resource.String.dialog_change_tab_size_title);
                alert.SetIcon(Resource.Drawable.icn_tab_width);
                alert.SetCancelable(false);

                // Bind OK button click
                alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) =>
                {
                    Prefs.SetInt(PreferenceKeys.EditorTabSize, seekValue);
                    SettingsResult.ForEach(s => s.TabSize = seekValue);
                    TabSizeText.Text = seekValue.ToString();
                });
                alert.SetNegativeButton(Resource.String.dialog_button_cancel, (e, v) => { });
                alert.Show();
            };
            //
            // Allow Intellisense?
            //
            IntellisenseSwitch.CheckedChange += (s, v) =>
            {
                var enable = IntellisenseSwitch.Checked; 

                SettingsResult.ForEach(s => s.EnableAutocomplete = enable);
                 
                Prefs.SetBool(PreferenceKeys.EnableIntellisense, enable);
            };
            //
            // Allow Snippets?
            //
            SnippetSwitch.CheckedChange += (s, v) =>
            {
                var enable = SnippetSwitch.Checked;
                 
                SettingsResult.ForEach(s => s.EnableSnippets = enable);
                 
                Prefs.SetBool(PreferenceKeys.EnableSnippets, enable);
            };
            //
            // Allow indent guide?
            //
            IndentSwitch.CheckedChange += (s, v) =>
            {
                var enable = IndentSwitch.Checked;

                SettingsResult.ForEach(s => s.EnableIndentGuide = enable);
                 
                Prefs.SetBool(PreferenceKeys.EnableIndent, enable);
            };
            //
            // Allow Code Folding?
            //
            FoldSwitch.CheckedChange += (s, v) =>
            {
                var enable = FoldSwitch.Checked;

                SettingsResult.ForEach(s => s.ShowFold = enable);
                 
                Prefs.SetBool(PreferenceKeys.ShowFolding, enable);
            };
            //
            // Allow line numbering
            //
            LineNumSwitch.CheckedChange += (s, v) =>
            {
                var enable = LineNumSwitch.Checked;

                SettingsResult.ForEach(s => s.ShowLineNums = enable);
                 
                Prefs.SetBool(PreferenceKeys.ShowLineNumbering, enable);
            };
        }
        //
        // Check for changes if back key is pressed
        //
        public override void OnBackPressed()
        {
            RunOnUiThread(() => WaitDialog.Show());

            Task.Run(async () => await ApplySettings(() =>
            {
                RunOnUiThread(() =>
                {
                    RunOnUiThread(() => WaitDialog.Dismiss());

                    Finish();
 
                });
            }, null)); 
        }
        //
        // Write the settings to file
        //
        private async Task ApplySettings(Action onSuccess, Action onFail)
        {
            await Task.Run(() =>
            {
                //Serialize to json
                var updatedJson = JsonConvert.SerializeObject(SettingsResult);

                var writeSettings = Settings.WriteSettings(updatedJson);
                if (writeSettings)
                    onSuccess?.Invoke();
                else
                    onFail?.Invoke();
            });
        }
         
    }
}