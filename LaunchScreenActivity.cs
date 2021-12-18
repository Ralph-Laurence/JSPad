using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "JSPad", Theme = "@style/AppTheme", MainLauncher = true)]
  
    public class LaunchScreenActivity : Activity
    {
        private PermissionAuthorization PermAuth = null;
         
        private TextView RecentsHeader = null;

        private LinearLayout CreateNewScriptButton = null;
        private LinearLayout OpenScriptButton = null;
        private LinearLayout SettingsButton = null;

        private ListView RecentsListView = null;
        private List<RecentFiles> m_RecentFiles = null;

        private PreferenceManager Prefs = null;
        private SettingsReader Settings = null;

        private string RepoDirectory = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        { 
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.launch_screen);

            // Initialize Preference Manager
            Prefs = new PreferenceManager(BaseContext);

            // Reference to Repository directory
            RepoDirectory = Paths.RepositoryDirectory(BaseContext);

            // Initialize Permission Manager
            PermAuth = new PermissionAuthorization()
            {
                MainActivity = this,
                BaseContext = BaseContext
            };

            // Check if READ WRITE permissions are not granted yet.
            if (!PermAuth.IsReadWritePermissionGranted())
            {
                // Request Permissions
                PermAuth.RequestReadWritePermission(() =>
                {
                    // Display a dialogue with a valid reason and a button to trigger the request
                    Dialogue.Confirm(this, GetString(Resource.String.dialog_request_perms_rw),
                    GetString(Resource.String.dialog_request_perms_title),
                    onOK: () =>
                    {
                        PermAuth.DirectReadWritePermission();
                    },
                    onCancel: () =>
                    {
                        // Exit
                        Dialogue.Warn(this, "Permission not granted. The application will exit...",
                            () => FinishAndRemoveTask());
                    },
                    Resource.Drawable.icn_perm_request);
                });
            }
            else
            {
                // Initialize views if perms are granted
                // InitializeViews();
                LoadSettings();
            } 
        }
        //
        // Invoked by Android to inform the Activity of the user's choices
        //
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            // Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
             
            if (requestCode == PermAuth.REQUEST_ID_READ_WRITE)
            {
                if (!PermAuth.VerifyPermissions(grantResults, 2))
                {
                    // Exit if permissions are not granted
                    Dialogue.Warn(this, "Permission not granted. The application will exit...",
                        () => FinishAndRemoveTask());
                }
                else
                {
                    // Initialize views after perms were granted
                    // InitializeViews();
                    LoadSettings();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        //
        // LOAD SETTINGS AND SAVE THEM TO PREFERENCES
        //
        private void LoadSettings()
        {
            // Show dialog while waiting
            var waitDialog = Dialogue.IndefinitePreloaderDialog(this, "Setting up, please wait...").Create();

            waitDialog.Show();

            // Write the original json settings file to app data path
            Settings = new SettingsReader(this, BaseContext);

            // Check if settings file exists.
            // If not, copy the original settings and write it in there
            if (!Settings.SettingsFileExists())
            {
                var writeSettingsTask = Task.Run(async () => await Settings.CopyOriginalSettingsAsync());
                var writeSettingsRes = writeSettingsTask.GetAwaiter().GetResult();

                // Force exit if failed
                if (!writeSettingsRes)
                {
                    Dialogue.Error(this, "Unable to write settings to file.", () => FinishAndRemoveTask());
                }
            }

            // Read settings and apply preferences 
            Task.Run(async () => await SetPreferencesAsync
            (
                onSuccess: () => 
                {
                    RunOnUiThread(() =>
                    {
                        waitDialog.Dismiss();

                        // After settings have loaded, begin initialization of views
                        InitializeViews();
                    });
                },
                onFailed: () =>
                {
                    RunOnUiThread(() =>
                    {
                        // Force close
                        Dialogue.Error(this, "Unable to load settings from file.", () => FinishAndRemoveTask());
                    });
                }
            ));
              
        }
        private async Task SetPreferencesAsync(Action onSuccess, Action onFailed)
        {
            await Task.Run(() =>
            {
                // Read settings and apply preferences 
                var readSettingsRes = Settings.LoadJsonSettings();

                // Make sure that settings are loaded
                if (readSettingsRes.Count > 0)
                {
                    var theme = string.Empty;
                    var tabSize = 0;
                    var fontSize = 0;
                    var enableIntellisense = false;
                    var enableSnippet = false;
                    var enableIndent = false;
                    var showFold = false;
                    var showLineNumbers = false;

                    // Set preferences
                    readSettingsRes.ForEach(settings =>
                    {
                        theme = settings.Theme;
                        tabSize = settings.TabSize;
                        fontSize = settings.FontSize;
                        enableIntellisense = settings.EnableAutocomplete;
                        enableSnippet = settings.EnableSnippets;
                        enableIndent = settings.EnableIndentGuide;
                        showFold = settings.ShowFold;
                        showLineNumbers = settings.ShowLineNums;
                    });

                    Prefs.SetString(PreferenceKeys.EditorTheme, theme);
                    Prefs.SetInt(PreferenceKeys.EditorFontSize, fontSize);
                    Prefs.SetInt(PreferenceKeys.EditorTabSize, tabSize);
                    Prefs.SetBool(PreferenceKeys.EnableIntellisense, enableIntellisense);
                    Prefs.SetBool(PreferenceKeys.EnableSnippets, enableSnippet);
                    Prefs.SetBool(PreferenceKeys.EnableIndent, enableIndent);
                    Prefs.SetBool(PreferenceKeys.ShowFolding, showFold);
                    Prefs.SetBool(PreferenceKeys.ShowLineNumbering, showLineNumbers);

                    onSuccess?.Invoke();
                }
                else
                {
                    onFailed?.Invoke();
                    // Force exit if settings cannot be raed
                    // Dialogue.Error(this, "Unable to read settings from file.", () => FinishAndRemoveTask());
                }
            }); 
        }
        // 
        // Initialize views and other objects
        //
        private void InitializeViews()
        {
            // Show dialog while waiting
            var waitDialog = Dialogue.IndefinitePreloaderDialog(this, "Almost there...").Create();
            waitDialog.Show();

            // Create a directory where we will store all projects
            CreateRepositoryDirectory();

            m_RecentFiles = new List<RecentFiles>(); 

            RecentsHeader = FindViewById<TextView>(Resource.Id.textview_recents_count);
            RecentsListView = FindViewById<ListView>(Resource.Id.recents_list);

            CreateNewScriptButton = FindViewById<LinearLayout>(Resource.Id.big_button_create_new);
            OpenScriptButton = FindViewById<LinearLayout>(Resource.Id.big_button_open_script);
            SettingsButton = FindViewById<LinearLayout>(Resource.Id.launch_screen_settings_button);
              
            // Show a List of recent Files
            var taskListRecents = Task.Run(async () => await ListRecentFilesAsync());
            var taskListRecentsRes = taskListRecents.GetAwaiter().GetResult();

            // Count how many recent files are there
            RecentsHeader.Text = $"Recents ({taskListRecentsRes})";

            // Bind dataset to listview
            var recentsDataAdapter = new RecentsAdapter(this, m_RecentFiles);
            RecentsListView.Adapter = recentsDataAdapter;

            waitDialog.Dismiss();

            // Bind click event on listview items
            RecentsListView.ItemClick += (s, e) =>
            {
                // Get click position
                var itemPos = e.Position;

                // Get the selected recent file
                var item = m_RecentFiles[itemPos];

                // Get the filename
                var fileName = item.Filename;

                // Show a dialog before loading the editor
                Dialogue.IndefinitePreloaderDialog(this, $"Opening script...").Create().Show();

                // Edit the selected recent file
                LaunchActivity(typeof(EditorActivity), new Dictionary<string, string>()
                {
                    { "ScriptName", fileName }
                });
                // Toast.MakeText(this, fileName, ToastLength.Short).Show();
            };

            // Bind click events onto big buttons
            CreateNewScriptButton.Click += (s, o) => LaunchActivity(typeof(CreateNewScriptActivity));

            // Bind click event on Open Button
            OpenScriptButton.Click += (s, e) => LaunchActivity(typeof(ScriptsRepoActivity), null, true);

            // Launch the settings window
            SettingsButton.Click += (s, e) => LaunchActivity(typeof(SettingsActivity), null, true);

            // Force user to accept the agreement
            ShowPrivacyAgreement();
        }
        //
        // Open the main screen for creating new projects
        //
        public void LaunchActivity(Type intent, Dictionary<string, string> extras = null, bool canGoBackToLast = false)  
        {
            var activity = new Intent(this, intent); 
             
            if (intent == typeof(ScriptsRepoActivity) || intent == typeof(SettingsActivity))
            { 
                if (canGoBackToLast)
                {
                    activity.PutExtra("CanGoBackToLast", "True");
                    StartActivity(activity);
                }
                else
                {
                    activity.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    StartActivity(activity);
                    FinishAffinity();
                }
            }
            else
            {
                activity.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

                if (extras != null)
                    foreach (KeyValuePair<string, string> kvp in extras)
                        activity.PutExtra(kvp.Key, kvp.Value);

                StartActivity(activity);
                FinishAffinity();
            }
        }
        //
        // Show recently opened files
        //
        private async Task<int> ListRecentFilesAsync()
        {
            var fileCount = 0;

            await Task.Run(() =>
            {
                // Read all filenames in a driectory
                var files = Directory.GetFiles(Paths.RepositoryDirectory(BaseContext));

                // Get recet files and show at Listview
                try
                {
                    // Check if there were files in the said directory
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            // Get file date
                            var date = File.GetLastWriteTime(file);

                            // Check if file is within 3 days ago
                            var today = DateTime.Now;

                            // Get the number of days between today and file date
                            var diff = (today - date).Days;

                            if (diff <= 3)
                            {
                                // Get file title
                                var title = new DirectoryInfo(file).Name;

                                var recents = new RecentFiles()
                                {
                                    Filedate = date.ToString("MMM. dd, yyyy - h:mm tt"),
                                    Filename = title,
                                    SortableDate = date
                                };

                                m_RecentFiles.Add(recents);

                            }
                        }

                        // Sort files by date (Descending)
                        m_RecentFiles.Sort((c1, c2) => DateTime.Compare(c2.SortableDate, c1.SortableDate));

                        fileCount = m_RecentFiles.Count;
                    }
                }
                catch (IOException ioex)
                {
                    Dialogue.WriteToast(BaseContext, GetString(Resource.String.toast_failed_to_list_files));
                }
            });

            return fileCount;
        }
        //
        // Show privacy agreement
        //
        private void ShowPrivacyAgreement()
        { 
            var dontShowAgain = Prefs.GetBool(PreferenceKeys.PrivacyAgreementDontShowAgain);

            if (!dontShowAgain)
            {
                var dialogLayoutID = Resource.Layout.dialog_dontshow_again;
                var checkBoxID = Resource.Id.dialog_dont_show_again_checkbox;
                var textViewID = Resource.Id.dialog_dont_show_text;
                var privacyMsg = GetString(Resource.String.dialog_privacy_change);

                Dialogue.AlertDontShowAgain(this, dialogLayoutID, checkBoxID, textViewID, privacyMsg, null,
                onChecked: () =>
                {
                    // Dont show this dialog in the future
                    Prefs.SetBool(PreferenceKeys.PrivacyAgreementDontShowAgain, true);
                });
            }
        }
        //
        // Create Repositories Directory If Not Exists
        //
        public void CreateRepositoryDirectory()
        {
            // Create project folder if not exists

            if (!Directory.Exists(RepoDirectory))
            {
                var result = Paths.CreateDirectory(RepoDirectory);

                var snackbarMsg = result ?
                    Resource.String.snack_success_create_rootdir :
                    Resource.String.snack_fail_create_rootdir;

                var snackbarBg = result ?
                    UIStyles.GetPrimaryColor(BaseContext) :
                    UIStyles.MaterialRed(BaseContext);

                var snackbarFg = result ?
                    UIStyles.GetPrimaryDarkColor(BaseContext) :
                    UIStyles.StandardWhite(BaseContext);
                 
            }
        }

    }
}