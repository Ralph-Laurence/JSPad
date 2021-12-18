using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "ScriptsLibraryActivity")]
    
    public class ScriptsRepoActivity : Activity
    {
        private List<RepoFiles> ScriptFiles = null;

        private ListView ScriptsListView = null;
        private PermissionAuthorization PermAuth = null;
        private RepoAdapter RepoDataAdapter = null;
        private Dialog DeleteProgressDialog = null;
        private Dialog RetrieveProcessDialog = null;

        private ImageButton HomeButton = null;
        private ImageButton SortAscButton = null;
        private ImageButton SortDescButton = null;
        private ImageButton SortDateAscButton = null;
        private ImageButton SortDateDescButton = null;

        // Snackbar
        private View SnackbarView = null;

        private string ScriptName = string.Empty;
        private bool EditorChanged = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.repositories_layout);
            // Create your application here

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
        //
        //
        public void InitializeViews()
        {  
            ScriptName = Intent.GetStringExtra("ScriptName");

            var editorChanged = Intent.GetStringExtra("EditorChanged");
            EditorChanged = Convert.ToBoolean(editorChanged);

            DeleteProgressDialog = Dialogue.IndefinitePreloaderDialog(this, "Deleting Script...").Create();
            RetrieveProcessDialog = Dialogue.IndefinitePreloaderDialog(this, "Retrieving Scripts...").Create();

            ScriptFiles = new List<RepoFiles>();
            ScriptsListView = FindViewById<ListView>(Resource.Id.repos_listview);

            HomeButton = FindViewById<ImageButton>(Resource.Id.btn_home);
            SortAscButton = FindViewById<ImageButton>(Resource.Id.btn_sort_repo_asc);
            SortDescButton = FindViewById<ImageButton>(Resource.Id.btn_sort_repo_desc);
            SortDateAscButton = FindViewById<ImageButton>(Resource.Id.btn_sort_repo_date_asc);
            SortDateDescButton = FindViewById<ImageButton>(Resource.Id.btn_sort_repo_date_desc);

            SnackbarView = FindViewById<View>(Resource.Id.repo_snackview);

            HomeButton.Click += (s, v) =>
            {
                if (!EditorChanged)
                    LaunchActivity(typeof(LaunchScreenActivity));
                else
                {
                    Dialogue.ConfirmYesNo(this, 
                        $"Your changes to \"{ScriptName}\" will not be saved if you continue. Exit to main?", 
                        "Exit",
                        onOK: () =>
                        {
                            LaunchActivity(typeof(LaunchScreenActivity));
                        });
                }
            };
            SortAscButton.Click += (s, v) =>
            {
                ScanFilesInRepo("ASC");
                ShowSnack(Resource.String.repo_menu_sort_asc);
            };
            SortDescButton.Click += (s, v) =>
            {
                ScanFilesInRepo("DESC");
                ShowSnack(Resource.String.repo_menu_sort_desc);
            };
            SortDateAscButton.Click += (s, v) =>
            {
                ScanFilesInRepo("DATE DESC");
                ShowSnack(Resource.String.repo_menu_sort_date_desc);
            };
            SortDateDescButton.Click += (s, v) =>
            {
                ScanFilesInRepo("DATE ASC");
                ShowSnack(Resource.String.repo_menu_sort_date_asc);
            };
             
            // Find all files inside repo
            ScanFilesInRepo();
        }
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            menu.SetHeaderTitle("Select Action");
            menu.Add(0, v.Id, 0, "Run");
            menu.Add(0, v.Id, 0, "Edit");
            menu.Add(0, v.Id, 0, "Delete");
            // base.OnCreateContextMenu(menu, v, menuInfo);
        }
        //
        //
        //
        private void ScanFilesInRepo(string sort = null)
        {
            ScriptFiles.Clear();

            RunOnUiThread(() => RetrieveProcessDialog.Show());

            // Show a List of repo Files
            var taskListRepo = Task.Run(async () => await ListFilesInRepoAsync(sort));
            var taskListRepoRes = taskListRepo.GetAwaiter().GetResult();

            RunOnUiThread(() => RetrieveProcessDialog.Hide());

            // Bind dataset to listview
            RepoDataAdapter = new RepoAdapter(this, BaseContext, ScriptFiles);
            RepoDataAdapter.OnAdapterDeletingData += (s, e) =>
            {
                if (e.Deleting)
                    DeleteProgressDialog.Show();
            };
            RepoDataAdapter.OnAdapterDataDeleted += (s, e) =>
            { 
                if (e.Deleted)
                {
                    DeleteProgressDialog.Hide();
                    ScanFilesInRepo();
                }
            };

            ScriptsListView.Adapter = RepoDataAdapter;
        }
        //
        // Get all files inside the repo
        //
        public async Task<int> ListFilesInRepoAsync(string sortMode = null)
        {
            var total = 0;

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

                            // Get file title
                            var title = new DirectoryInfo(file).Name;

                            var repo = new RepoFiles()
                            {
                                Filedate = date.ToString("MMM. dd, yyyy - h:mm tt"),
                                Filename = title,
                                SortableDate = date
                            };

                            ScriptFiles.Add(repo);
                        }

                        // Sort Files
                        switch (sortMode)
                        {
                            case "ASC":
                                // Sort files by filename (Ascending)
                                ScriptFiles = ScriptFiles.OrderBy(r => r.Filename).ToList();
                                break;
                            case "DESC":
                                // Sort files by filename (Descending)
                                ScriptFiles = ScriptFiles.OrderByDescending(r => r.Filename).ToList();
                                break;
                            case "DATE ASC":
                                ScriptFiles.Sort((d1, d2) => DateTime.Compare(d1.SortableDate, d2.SortableDate));
                                break;
                            case "DATE DESC":
                                ScriptFiles.Sort((d1, d2) => DateTime.Compare(d2.SortableDate, d1.SortableDate));
                                break;
                            default:
                                // Sort files by filename (Ascending)
                                ScriptFiles = ScriptFiles.OrderBy(r => r.Filename).ToList();
                                break;
                        }

                        total = ScriptFiles.Count;
                    }
                }
                catch (IOException ioex)
                {
                    Dialogue.WriteToast(BaseContext, GetString(Resource.String.toast_failed_to_list_files));
                }
            });

            return total;
        }
        //
        // Open the main screen for creating new projects
        //
        public void LaunchActivity(Type intent, Dictionary<string, string> extras = null)
        {
            var activity = new Intent(this, intent);
            activity.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            if (extras != null)
                foreach (KeyValuePair<string, string> kvp in extras)
                    activity.PutExtra(kvp.Key, kvp.Value);

            StartActivity(activity);
            FinishAffinity();
        }
        //
        //
        //
        private void ShowSnack(int messageId)
        {
            Dialogue.ShowColoredSnack
            (
                view: SnackbarView,
                messageId: messageId,
                background: UIStyles.GetAccentColor(BaseContext),
                foreground: UIStyles.StandardWhite(BaseContext),
                duration: Snackbar.LengthLong
            );
        }
    }
}