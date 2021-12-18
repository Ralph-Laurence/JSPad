using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using System;
using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "CreateNewProjectActivity")]
    public class CreateNewScriptActivity : Activity
    {
        private EditText InputScriptLocation = null;
        private EditText InputScriptName = null;

        private Button BtnCreate = null;
        private Button BtnCancel = null;

        private RadioGroup TemplateOptions = null;

        private bool CanGoBackToLast = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.create_script_activity);

            InitializeViews();
        }

        private void InitializeViews()
        {
            var canGoBack = Intent.GetStringExtra("CanGoBackToLast");
            CanGoBackToLast = Convert.ToBoolean(canGoBack);

            InputScriptName = FindViewById<EditText>(Resource.Id.edit_text_script_name);
            InputScriptLocation = FindViewById<EditText>(Resource.Id.edit_text_script_location);

            BtnCreate = FindViewById<Button>(Resource.Id.btn_create_script);
            BtnCancel = FindViewById<Button>(Resource.Id.btn_cancel_create_script);

            TemplateOptions = FindViewById<RadioGroup>(Resource.Id.template_radio_group);

            // Set input field for script location as the private directory
            InputScriptLocation.Text = Paths.RepositoryDirectory(BaseContext);

            // Bind click event into Create New button
            BtnCreate.Click += (s, v) => CreateNew();

            // Bind click event into Cancel Button
            BtnCancel.Click += (s, v) => GoBack();
        }
        //
        // Hardware back button
        //
        public override void OnBackPressed()
        {
            GoBack();
        }
        //
        // Go back to previous activity then clear current
        //
        private void GoBack()
        {
            if (!CanGoBackToLast)
                GoToHome();
            else
                Finish();
        }
        //
        // Go to home screen
        //
        private void GoToHome()
        {
            // Go back and clear current activity
            var intent = new Intent(this, typeof(LaunchScreenActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
            FinishAffinity();
        }
        //
        // Create New Project Async
        //
        private void CreateNew()
        { 
            // Get repo path
            var repo = Paths.RepositoryDirectory(BaseContext);

            // Get script name from input
            var scriptName = InputScriptName.Text;

            // Progressbar dialog for creating repository dir
            var createRepoDialog = Dialogue.IndefinitePreloaderDialog(this, "Creating repository").Create();

            // Progressbar dialog for creating new script
            var createScriptDialog = Dialogue.IndefinitePreloaderDialog(this, "Creating new project").Create();

            // Check if repo directory exists
            if (!Directory.Exists(repo))
            {
                RunOnUiThread(() => createRepoDialog.Show());

                var createDirTask = Task.Run(async () => await Paths.CreateDirectoryAsync(repo));
                var createDirRes = createDirTask.GetAwaiter().GetResult();

                if (createDirRes)
                {
                    RunOnUiThread(() => createRepoDialog.Hide());
                    
                    // Create script after successfully creating repo dir
                    CreateScript(repo, scriptName, createScriptDialog);
                }
                else
                {
                    RunOnUiThread(() => createRepoDialog.Hide());

                    // Error
                    Dialogue.Error(this, "Unable to create repository directory because of an error.");
                }
            }
            else
            {
                // Create script because repo dir already exists
                CreateScript(repo, scriptName, createScriptDialog);
            } 
        }
        //
        // Create Script File
        //
        private void CreateScript(string repo, string scriptName, Dialog dialog)
        {
            // Make sure that a script name is valid
            if (!string.IsNullOrEmpty(scriptName))
            {
                // Automatically add file extension
                scriptName = scriptName.EndsWith(".js") ? scriptName : $"{scriptName}.js";

                // Generate filename with path
                var path = Path.Combine(repo, scriptName);

                // Make sure script name does not exists yet
                if (!File.Exists(path))
                {
                    // Show the progress dialog while waiting
                    RunOnUiThread(() => dialog.Show());

                    // Get the ID of the selected checkbox
                    var idx = TemplateOptions.CheckedRadioButtonId;

                    // Tell what checkbox was selected
                    switch (idx)
                    {
                        // Create Empty script
                        case Resource.Id.chk_template_empty_script:

                            var task_CreateEmptyScript = Task.Run(async () => await CreateEmptyScriptAsync(path));
                            var result_CreateEmptyScriptTask = task_CreateEmptyScript.GetAwaiter().GetResult();

                            // Launch the editor after successfully creating the script file
                            if (result_CreateEmptyScriptTask)
                                LaunchEditor(scriptName);
                            else
                            {
                                Dialogue.Error(this, $"Unable to create script file because of an error");

                                // Show the progress dialog while waiting
                                RunOnUiThread(() => dialog.Hide());
                            }
                            break;

                        // Create hello world script
                        case Resource.Id.chk_template_hello_world:

                            var task_CreateHelloWorld = Task.Run(async () => await CreateHellowWorldAsync(path));
                            var result_CreateHelloWorld = task_CreateHelloWorld.GetAwaiter().GetResult();

                            // Launch the editor after successfully creating the script file
                            if (result_CreateHelloWorld)
                                LaunchEditor(scriptName);
                            else
                            {
                                Dialogue.Error(this, "Unable to create script file because of an error.");

                                // Show the progress dialog while waiting
                                RunOnUiThread(() => dialog.Hide());
                            }
                            break;
                    }
                }
                else
                {
                    Dialogue.Warn(this, $"\"{scriptName}\" already exists! Please choose a different script name.");
                }
            }
            else
            {
                Dialogue.Warn(this, "Please set a valid script name!");
            }
        }
        //
        // Empty Script
        //
        private async Task<bool> CreateEmptyScriptAsync(string path)
        {
            bool success = false;

            await Task.Run(() =>
            {
                try
                {
                    using var writer = new StreamWriter(path);
                    writer.WriteLine();

                    success = true;
                }
                catch (IOException ioex)
                {
                    success = false;
                    System.Diagnostics.Debug.WriteLine(ioex.Message);
                }
            });

            return success;
        }
        //
        // Hello World script template
        //
        private async Task<bool> CreateHellowWorldAsync(string path)
        {
            bool success = false;

            await Task.Run(() =>
            {
                try
                {
                    using var writer = new StreamWriter(path);

                    var helloWorld = 
@"/* 
Press the green 'Play' button
to run the script and see the output
on console
*/

// hello world console
console.log(""Hello, World!"");

// hello world alertbox
alert(""Hello World!"");
";
                    writer.Write(helloWorld);

                    success = true;
                }
                catch (IOException ioex)
                {
                    success = false;
                    System.Diagnostics.Debug.WriteLine(ioex.Message);
                }
            });

            return success;
        }
        //
        // Open the editor then add flags to prevent from going back
        //
        private void LaunchEditor(string scriptName)
        {
            var intent = new Intent(this, typeof(EditorActivity));
            intent.PutExtra("ScriptName", scriptName);
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            
            StartActivity(intent);
            FinishAffinity();
        }
    }
}