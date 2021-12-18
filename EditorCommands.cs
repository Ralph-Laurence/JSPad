using Android.App;
using Android.Content;
using Android.Webkit;

using System;
using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    public class EditorCommands
    {
        //
        // Reference to the the webview
        //
        public WebView Editor { get; set; }
        //
        // Execute editor commands (javascript functions)
        //
        public void Exec(string command) => Editor.EvaluateJavascript(command, new JavaScriptResult());
        //
        // Read script from file then open the console Intent to run
        //
        public void RunScriptFromEditor(Activity invokeFrom, Context context, string scriptPath, string scriptName)
        {
            // Pass relevant data to Console View intent
            var console = new Intent(context, typeof(InterpreterActivity));
            console.PutExtra("ScriptName", scriptName);
            console.PutExtra("ScriptPath", scriptPath);
            console.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            // Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_NEW_TASK

            invokeFrom.StartActivity(console);
        }
        //
        // Save the code to file
        //
        public void Save(Context context, string contents, string path, string filename, Action onSuccess = null, Action onFail = null)
        {
            // Repository directory
            var projectsRootPath = Paths.RepositoryDirectory(context);

            // Create REPO directory if not exists
            if (!Directory.Exists(projectsRootPath))
            {
                // Create the repo directory and expect a result
                var result = Paths.CreateDirectory(projectsRootPath);

                // Write the project to file
                if (result)
                {
                    var write_task = Task.Run(async () => await EditorIO.WriteFileAsync(contents, path, filename));
                    var write_result = write_task.GetAwaiter().GetResult();

                    if (write_result)
                        onSuccess?.Invoke();
                    else
                        onFail?.Invoke();
                }
                else
                {
                    onFail?.Invoke();
                }
            }
        } 
    }  
}