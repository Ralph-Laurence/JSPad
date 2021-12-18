using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;

using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    [Activity(Label = "Interpreter", Theme = "@style/AppTheme")]
    public class InterpreterActivity : Activity
    {
        private WebView     InterpreterView = null;
        private TextView    ConsoleTitle    = null;
        private Button      ConsoleBack     = null;
        private Button      ClearButton     = null;

        private EditorCommands Commands     = null;

        private string ScriptName = string.Empty;
        private string ScriptPath = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
             
            // Set layout
            SetContentView(Resource.Layout.activity_interpreter);

            // Initialize Views and other objects
            InitializeViews();
        }
        //
        // INITIALIZATION
        //
        private void InitializeViews()
        {
            // Get script information from intent data
            ScriptPath = Intent.GetStringExtra("ScriptPath");
            ScriptName = Intent.GetStringExtra("ScriptName");

            InterpreterView = FindViewById<WebView>(Resource.Id.interpreter_view);
            ConsoleTitle = FindViewById<TextView>(Resource.Id.console_title_text);
            ConsoleBack = FindViewById<Button>(Resource.Id.console_back_button);
            ClearButton = FindViewById<Button>(Resource.Id.btn_clear_console);

            // Set console title as script name
            ConsoleTitle.Text = ScriptName;

            // Button for clearing the console
            ConsoleBack.Click += (s, e) => Finish();

            // Editor commands for running js functions
            Commands = new EditorCommands() { Editor = InterpreterView };

            InterpreterView.Settings.JavaScriptEnabled = true;
            InterpreterView.SetWebChromeClient(new WebChromeClient());

            // Clear Console
            ClearButton.Click += (s, e) => Commands.Exec("clear()");

            // Runnable template data
            var runnableTask = Task.Run(async () => await CreateRunnableAsync());
            var runnableData = runnableTask.GetAwaiter().GetResult();

            if (runnableData != string.Empty) 
                InterpreterView.LoadDataWithBaseURL("file:///android_asset/www/interpreter", 
                                                    runnableData, "text/html", "UTF-8", null);
            else
                Dialogue.Error(this, "Unable to load interpreter");
        }
        //
        // Create Runnable HTML
        //
        public async Task<string> CreateRunnableAsync()
        { 
            var output = string.Empty;

            await Task.Run(() =>
            {
                // Get path to template data
                var runnableTemplate = Assets.Open("www/interpreter/interpreter.html");

                // Read template data from HTML
                using var templateReader = new StreamReader(runnableTemplate);
                var templateResult = templateReader.ReadToEnd();

                // Read script from file
                using var scriptReader = new StreamReader(ScriptPath);
                var scriptResult = scriptReader.ReadToEnd();

                // Inject script data into HTML
                output = templateResult.Replace("{content}", scriptResult); 
            });

            return output;
        }
    }
}