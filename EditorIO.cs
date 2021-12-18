using Android.Content;

using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    public class EditorIO
    {  
        /// <summary>
        /// Write contents to file in a separate thread.
        /// </summary>
        /// <param name="content">The contents to write</param>
        /// <param name="path">The output location</param>
        /// <param name="filename">The filename</param>
        /// <returns>Task | bool</returns>
        public static async Task<bool> WriteFileAsync(string content, string path, string filename)
        {
            bool success = false;

            await Task.Run(() =>
            {
                try
                {  
                    var file = Path.Combine(path, filename);

                    using var sw = new StreamWriter(file);
                    sw.WriteLine(content);

                    success = true;
                }
                catch (IOException ioex)
                {
                    success = false;
                }
            });

            return success;
        }
        /// <summary>
        /// Reads a file from the given path
        /// </summary>
        /// <param name="path">The path to file</param>
        /// <returns>Task | string</returns>
        public static async Task<string> ReadFromFileAsync(string path)
        {
            var result = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    using var reader = new StreamReader(path);
                    result = reader.ReadToEnd();
                }
                catch (IOException ioex)
                {
                    result = default;
                }
            });

            return result;
        }

        public static async Task<bool> DeleteFileAsync(Context context, string filename)
        {
            var success = false;

            await Task.Run(() =>
            {
                try
                {
                    var repo = Paths.RepositoryDirectory(context);
                    var file = Path.Combine(repo, filename);

                    // Check if repo directory exists
                    if (Directory.Exists(repo))
                    {
                        // Delete the file inside that repo
                        File.Delete(file);

                        success = true;
                    }
                }
                catch(IOException ioex)
                {
                    success = false;
                }
            });

            return success;
        }
    } 
}