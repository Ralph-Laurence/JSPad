using Android.Content;

using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    class Paths
    {
        #region SYSTEM_DIRS
          
        /// <summary>
        /// [Private Internal Storage]
        /// Shareable directory
        /// </summary>
        /// <param name="context">Base Context</param>
        /// <returns>/storage/emulated/0/Android/data/com.xxx/files</returns>
        public static string AppData(Context context) =>
            context.GetExternalFilesDir(null).AbsolutePath.ToString();

        /// <summary>
        /// [OS Internal Storage]
        /// Only accessible to OS or the app itself
        /// </summary>
        /// <param name="context">Base Context</param>
        /// <returns>/data/user/0</returns>
        public static string Internal(Context context) =>
            context.FilesDir.ToString();

        #endregion

        #region PROJECT DIRECTORIES

        /// <summary>
        /// The default folder name for projects created with JSPad
        /// </summary>
        public static readonly string RootProjectFolderName = "JSPad Projects";

        /// <summary>
        /// The default directory for projects created with JSPad
        /// </summary>
        public static string RepositoryDirectory(Context context) => 
            Path.Combine(AppData(context), RootProjectFolderName);

        /// <summary>
        /// The folder where JSPad settings will be stored
        /// </summary>
        public static readonly string RootSettingsDirectory = "settings";

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="directory">The path to directory</param>
        /// <returns>bool</returns>
        public static bool DirectoryExists(string directory) => Directory.Exists(directory);

        //
        // Async call to create directory
        //
        public static async Task<bool> CreateDirectoryAsync(string directory)
        {
            bool success = false;
            
            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(directory);

                    success = true;
                }
                catch (IOException ioex)
                {
                    success = false;
                }
            });

            return success;
        }
        //
        // Create Directory and expect a result
        //
        public static bool CreateDirectory(string path)
        {
            var task = Task.Run(async () => await CreateDirectoryAsync(path));
            var result = task.GetAwaiter().GetResult();

            return result;
        }
        #endregion

        #region PATH METHODS

        /// <summary>
        /// Creates a directory if not exists
        /// </summary>
        /// <param name="directory">The path to directory</param>
        /// <returns>Task</returns>
        //public static async Task CreateDirectoryAsync(string directory)
        //{
        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            if (!DirectoryExists(directory))
        //                Directory.CreateDirectory(directory);
        //        }
        //        catch (IOException ioex)
        //        {

        //        }
        //    });
        //}

        #endregion
    }
}