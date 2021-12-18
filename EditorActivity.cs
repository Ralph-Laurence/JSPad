using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;

using AndroidX.AppCompat.App;

using Google.Android.Material.Snackbar;

using System;
using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class EditorActivity : AppCompatActivity
    {
        private PreferenceManager Prefs = null;
        private PermissionAuthorization PermAuth = null;
        private EditorCommands Commands = null;
         
        private TextView FilenameText = null;
        private WebView CodeEditor = null;

        // MENUBAR BUTTONS
        private Button UndoButton = null;
        private Button RedoButton = null;
        private Button CutButton = null;
        private Button CopyButton = null;
        private Button PasteButton = null;
        private Button FindButton = null;
        private Button SelectAllButton = null;
        private Button ClearButton = null;
        private Button SaveButton = null;
        private Button RunButton = null;

        // Snackbar
        private View SnackbarView = null;

        // FOOTER BUTTONS
        private Button TabButton = null;
        private Button OpenRepoButton = null;
        private Button NewProjectButton = null;
        private Button SettingsButton = null;

        private string RepoDirectory = string.Empty;
        private string FileTitle = string.Empty;    // Filename 

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Reference to Repository directory
            RepoDirectory = Paths.RepositoryDirectory(BaseContext);

            Prefs = new PreferenceManager(BaseContext);

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
                InitializeViews();
            }
        }
        protected override void OnRestart()
        {
            // Toast.MakeText(this, "Welcome back", ToastLength.Short).Show();

            // Re-Apply editor settings
            LoadEditorSettings();

            var repoDir = Paths.RepositoryDirectory(BaseContext);
            var file = System.IO.Path.Combine(repoDir, FileTitle);

            // Check if the current script still exists
            if (Directory.Exists(RepoDirectory))
            {
                if (!File.Exists(file))
                {
                    Dialogue.Error(this, "This script no longer exists.", onOK: () =>
                    {
                        var home = new Intent(this, typeof(LaunchScreenActivity));
                        home.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        StartActivity(home);
                    });
                }
            }

            base.OnRestart();
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
                    InitializeViews();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        //
        // INITIALIZATION
        //
        public void InitializeViews()
        {
            CodeEditor = FindViewById<WebView>(Resource.Id.code_editor);
             
            FilenameText = FindViewById<TextView>(Resource.Id.filename_text);

            UndoButton = FindViewById<Button>(Resource.Id.undo_button);
            RedoButton = FindViewById<Button>(Resource.Id.redo_button);
            CutButton = FindViewById<Button>(Resource.Id.cut_button);
            CopyButton = FindViewById<Button>(Resource.Id.copy_button);
            PasteButton = FindViewById<Button>(Resource.Id.paste_button);
            FindButton = FindViewById<Button>(Resource.Id.find_button);
            SelectAllButton = FindViewById<Button>(Resource.Id.select_all);
            ClearButton = FindViewById<Button>(Resource.Id.clear_all);
            SaveButton = FindViewById<Button>(Resource.Id.save_button);
            TabButton = FindViewById<Button>(Resource.Id.tab_space_button);
            RunButton = FindViewById<Button>(Resource.Id.run_button);
            NewProjectButton = FindViewById<Button>(Resource.Id.new_button);
            SettingsButton = FindViewById<Button>(Resource.Id.settings_button);

            SnackbarView = FindViewById<View>(Resource.Id.footer_divider);

            OpenRepoButton = FindViewById<Button>(Resource.Id.open_button);

            // Create reference to editor commands
            Commands = new EditorCommands() { Editor = CodeEditor };
               
            // Setup the code editor webview
            InitializeEditor();

            // Bind click events for menu buttons
            BindMenubarToolClick();
        }
        //
        // Read stored settings from Shared Prefs
        //
        private void LoadEditorSettings()
        {
            var theme = Prefs.GetString(PreferenceKeys.EditorTheme);
            var fontSize = Prefs.GetInt(PreferenceKeys.EditorFontSize);
            var tabSize = Prefs.GetInt(PreferenceKeys.EditorTabSize);
            var enableIntellisense = Prefs.GetBool(PreferenceKeys.EnableIntellisense).ToString().ToLower();
            var enableIndent = Prefs.GetBool(PreferenceKeys.EnableIndent).ToString().ToLower();
            var enableSnippet = Prefs.GetBool(PreferenceKeys.EnableSnippets).ToString().ToLower();
            var showFold = Prefs.GetBool(PreferenceKeys.ShowFolding).ToString().ToLower();
            var showLineNum = Prefs.GetBool(PreferenceKeys.ShowLineNumbering).ToString().ToLower();
            
            Commands.Exec(@"
aceEditor.setOptions
({
    enableBasicAutocompletion: " + enableIntellisense +
    ", enableLiveAutocompletion: " + enableIntellisense +
    ", enableSnippets:" + enableSnippet +
    ", showLineNumbers:" + showLineNum +
    ", showFoldWidgets:" + showFold +
    ", displayIndentGuides:" + enableIndent +
    ", fontSize: " + fontSize +
    ", tabSize: " + tabSize +
"});" +
@"
aceEditor.setTheme(""ace/theme/" + theme + @""");
");
        }
        // Initialize WebView Editor
        public void InitializeEditor()
        { 
            // Retrieve Filename from intent data
            FileTitle = Intent.GetStringExtra("ScriptName");

            // Set project title text
            FilenameText.Text = FileTitle;

            // Setup the editor webview
            CodeEditor.Settings.JavaScriptEnabled = true;
            
            var dialog = Dialogue.IndefinitePreloaderDialog(this, "Loading editor...").Create();
            
            // Show the progress dialog while waiting for the editor to load
            dialog.Show();
            
            // Load the editor
            CodeEditor.LoadUrl("file:///android_asset/www/editor/editor.html");
            
            //
            // Load the script after the editor has been fully loaded
            //
            var webviewClient = new EditorWebViewClient()
            {
                PageLoaded = () =>
                {
                    // Bind editor settings
                    LoadEditorSettings();
                    
                    var scriptPath = System.IO.Path.Combine(RepoDirectory, FileTitle);

                    // Load script into the editor
                    if (File.Exists(scriptPath))
                    {  
                        try
                        {
                            var readScriptTask = Task.Run(async () => await ReadScriptAsync(scriptPath));
                            var readScriptResult = readScriptTask.GetAwaiter().GetResult();

                            // Append script to editor 
                            Commands.Exec($"SetValue(`{readScriptResult}`)");
                            Commands.Exec($"SetEditorUnChangedState()");

                            // Hide the progress dialog 
                            dialog.Hide();
                        }
                        catch (IOException ioex)
                        {
                            // Hide the progress dialog 
                            dialog.Hide();

                            Dialogue.Error(this, $"The editor is unable to load the script \"{FileTitle}\".", 
                                () => ForceExitToMain());
                        }
                    }
                    else
                    {
                        // Hide the progress dialog 
                        dialog.Hide();

                        Dialogue.Error(this, $"Unable to load script \"{FileTitle}\" into the editor. The file does not exist.",
                            () => ForceExitToMain());
                    }
                }
            };

            CodeEditor.SetWebViewClient(webviewClient);
        }  
        //
        // Button Click Events
        //
        public void BindMenubarToolClick()
        {
            UndoButton.Click += (e, v) => Commands.Exec("Undo()");
            RedoButton.Click += (e, v) => Commands.Exec("Redo()");
            CopyButton.Click += (e, v) => Commands.Exec("Copy()");
            CutButton.Click += (e, v) => Commands.Exec("Cut()");
            PasteButton.Click += (e, v) => Commands.Exec("Paste()");
            FindButton.Click += (e, v) => Commands.Exec("Find()");
            SelectAllButton.Click += (e, v) => Commands.Exec("SelectAll()");
            
            // Insert tabs
            TabButton.Click += (e, v) => Commands.Exec("InsertTab()");

            // Clear the editor including undo stack
            ClearButton.Click += (e, v) =>
            {
                Dialogue.Confirm
                (
                    activity: this, 
                    msg: "Clearing the editor also resets the undo history. Continue?", 
                    title: "Clear Editor",
                    onOK: () =>
                    {
                        Commands.Exec("ClearEditor()");
                    }
                ); 
            };

            // Open the Settings activity referencing the editor
            SettingsButton.Click += (s, v) =>
            {
                var settings = new Intent(this, typeof(SettingsActivity));
                settings.PutExtra("IsActiveInEditor", true);
                // settings.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(settings);
                // FinishAffinity();
            };

            // Create a bew project. ASk for confirmation before continuing
            NewProjectButton.Click += (e, v) =>
            {
                CheckEditorChangedValue
                (
                    onChanged: () =>
                    {
                        Dialogue.ConfirmYesNo
                        (
                            this,
                            $"Save changes to \"{FileTitle}\"?",
                            "Create New Script",
                            onOK: () =>
                            {
                                // Save first before creating new script
                                Save
                                (
                                    onSuccess: () => LaunchNewCreateScriptPage(true),
                                    onFail: () => Dialogue.Error(this, "Unable to save script")
                                );
                            },
                            onCancel: () => LaunchNewCreateScriptPage(true) // Open repo without saving
                        );
                    },
                    onNotChanged: () => LaunchNewCreateScriptPage(true)
                ); ;
            };

            SaveButton.Click += (e, v) => Save(() => Toast.MakeText(BaseContext, "Saved", ToastLength.Short).Show());

            // Run the script. Save if not yet saved
            RunButton.Click += (e, v) =>
            {
                // Save first before running
                Save(onSuccess: () =>
                {
                    try
                    {
                        var scriptPath = System.IO.Path.Combine(RepoDirectory, FileTitle);

                        if (File.Exists(scriptPath))
                        {
                            Commands.RunScriptFromEditor(this, BaseContext, scriptPath, FileTitle);
                        }
                        else
                        {
                            Dialogue.ShowColoredSnack
                            (
                                view: SnackbarView,
                                messageId: Resource.String.snack_unable_to_load_script,
                                background: UIStyles.MaterialRed(BaseContext),
                                foreground: UIStyles.StandardWhite(BaseContext),
                                duration: Snackbar.LengthLong
                            );
                        }
                    }
                    catch (IOException ioex)
                    {
                        Dialogue.Error(this, "Script is either missing or corrupt.");
                    }
                });
            };

            // Open repo button clicked
            OpenRepoButton.Click += (e, v) =>
            {
                // Confirm before opening repo
                CheckEditorChangedValue
                (
                    onChanged: () =>
                    {
                        Dialogue.ConfirmYesNo
                        (
                            this, 
                            $"Save changes to \"{FileTitle}\" before loading another script?", 
                            "Unsaved Changes",
                            onOK: () =>
                            {
                                // Save first before opening repo
                                Save
                                (
                                    onSuccess: () => LaunchScriptsLibrary(true),
                                    onFail:() => Dialogue.Error(this, "Unable to save script")
                                );
                            },
                            onCancel: () => LaunchScriptsLibrary() // Open repo without saving
                        );
                    },
                    onNotChanged: () => LaunchScriptsLibrary()
                );
            };
        }
        //
        // Exit to main menu directly
        //
        private void ForceExitToMain()
        {
            var main = new Intent(this, typeof(LaunchScreenActivity));
            main.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(main);
            FinishAffinity();
        }
        //
        // Save the code to file
        //
        public void Save(Action onSuccess = null, Action onFail = null)
        {
            // Get the result from webview
            var jsResult = new JavaScriptResult();

            // Process the results
            jsResult.OnResponse += (s, v) =>
            {
                var task = Task.Run(async () => await EditorIO.WriteFileAsync(v.Result, RepoDirectory, FileTitle));
                var write = task.GetAwaiter().GetResult();

                if (write)
                {
                    // mark editor as unchanged
                    Commands.Exec("SetEditorUnChangedState()");
                    onSuccess?.Invoke();
                }
                else
                    onFail?.Invoke();
            };

            // Retrieve the codes from the editor
            CodeEditor.EvaluateJavascript("GetValue()", jsResult);
        }
        //
        // Load the script and return as string
        //
        private async Task<string> ReadScriptAsync(string path)
        {
            var result = string.Empty;

            await Task.Run(() =>
            {
                if (File.Exists(path))
                {
                    using var reader = new StreamReader(path);
                    result = reader.ReadToEnd();
                }
            });

            return result;
        }
        //
        // Hardware back button pressed
        //
        public override void OnBackPressed()
        {
            CheckEditorChangedValue
            (
                onChanged: () =>
                {
                    // Exit Confirmation; With editor changes
                    RunOnUiThread(() =>
                    {
                        Dialogue.Confirm(this,
                            title: GetString(Resource.String.dialog_confirm_exit_has_changes_title),
                            msg: GetString(Resource.String.dialog_confirm_exit_has_changes),
                            onOK: () => FinishAndRemoveTask(),
                            onCancel: () => Dialogue.WriteToast(BaseContext, GetString(Resource.String.toast_save_changes)));
                    });
                },
                onNotChanged: () =>
                {
                    // Exit Confirmation; No editor changes
                    RunOnUiThread(() =>
                    {
                        Dialogue.Confirm(this,
                        title: GetString(Resource.String.dialog_confirm_exit_title),
                        msg: GetString(Resource.String.dialog_confirm_exit),
                        onOK: () => FinishAndRemoveTask(),
                        onCancel: () => { });
                    }); 
                }
            );
        }
        //
        // Check for editor value changes then call specific actions
        //
        public void CheckEditorChangedValue(Action onChanged, Action onNotChanged)
        {
            // Get editor changed state from IValueCallback
            var jsResult = new JavaScriptResult();
            
            // A value has been recieved from webview
            jsResult.OnResponse += (s, v) =>
            {
                // Flag to tell if editor is changed
                var hasChanges = Convert.ToBoolean(v.Result);

                if (hasChanges)
                    onChanged?.Invoke();
                else
                    onNotChanged?.Invoke(); 
            };

            // Execute command
            CodeEditor.EvaluateJavascript("GetEditorChanged()", jsResult); 
        }
         
        //
        // Launch the Scripts Library viewer
        //
        public void LaunchScriptsLibrary(bool editorChanged = false)
        {
            var lib = new Intent(this, typeof(ScriptsRepoActivity));
            lib.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask); 
            lib.PutExtra("ScriptName", FileTitle);

            lib.PutExtra("EditorChanged", editorChanged ? "True" : "False");

            StartActivity(lib);
        }
        //
        //
        //
        public void LaunchNewCreateScriptPage(bool canGoBackToLast = false)
        {
            var home = new Intent(this, typeof(CreateNewScriptActivity));
             
            if (canGoBackToLast)
            {
                home.PutExtra("CanGoBackToLast", "True");
                StartActivity(home);
            }
                
            else
            {
                home.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(home);
                FinishAffinity();
            }
        }
    } 
}