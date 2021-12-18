using Android.App;
using Android.Content;
using Android.Webkit;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JSPad
{
    class SettingsReader 
    {
        private Activity BaseActivity = null;
        private Context BaseContext = null;

        public List<SettingsItem> SettingsItems = null;
        public static readonly string SettingsFileName = "settings.json";

        public string SettingsPath { get; private set; }

        public SettingsReader(Activity activity, Context baseContext)
        { 
            SettingsPath = Path.Combine(Paths.AppData(baseContext), SettingsFileName);

            SettingsItems = new List<SettingsItem>();
            BaseActivity = activity;
            BaseContext = baseContext;
        }
        //
        // Checks if a settings json file exists in appdata
        //
        public bool SettingsFileExists()
        {
            bool exists;

            try
            {
                var appData = Paths.AppData(BaseContext);
                var file = Path.Combine(appData, SettingsFileName);

                if (File.Exists(file))
                    exists = true;
                else
                    exists = false;
            }
            catch(IOException ioex)
            {
                exists = false;
            }

            return exists;
        }
        //
        // Read the original settings stored in apk
        //
        //public void LoadOriginalSettings(Action onLoaded)
        //{
        //    // Open settings json from asset
        //    var json = BaseContext.Assets.Open(SettingsFileName);

        //    // Read json file
        //    using var reader = new StreamReader(json);

        //    var jsonTask = Task.Run(async () => await reader.ReadToEndAsync());
        //    var jsonResult = jsonTask.GetAwaiter().GetResult();

        //    SettingsItems = JsonConvert.DeserializeObject<List<SettingsItem>>(jsonResult);

        //    onLoaded?.Invoke();
        //}
        //
        // Load the settings from file
        //
        public List<SettingsItem> LoadJsonSettings()
        {
            var values = new List<SettingsItem>();

            try
            {
                if (File.Exists(SettingsPath))
                {
                    using var reader = new StreamReader(SettingsPath);
                    var json = reader.ReadToEnd();

                    values = JsonConvert.DeserializeObject<List<SettingsItem>>(json);
                }
            }
            catch (IOException ioex)
            {
                values = default;
            }

            return values;
        }

        public async Task<List<SettingsItem>> LoadJsonSettingsAsync()
        {
            var values = new List<SettingsItem>();

            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        using var reader = new StreamReader(SettingsPath);
                        var json = reader.ReadToEnd();

                        values = JsonConvert.DeserializeObject<List<SettingsItem>>(json);
                    }
                }
                catch (IOException ioex)
                {
                    values = default;
                }
            });
           
            return values;
        }
        //
        // Write Original Settings To File
        //
        public async Task<bool> CopyOriginalSettingsAsync()
        {
            var success = false;

            await Task.Run(() =>
            { 
                // Lets asume that the app data directory
                // is already created upon install.
                // So let us just check for the existence
                // of the settings json file. Then
                // overwrite the settings
                var json = BaseContext.Assets.Open(SettingsFileName);

                try
                {
                    using var reader = new StreamReader(json);
                    var jsonTask = Task.Run(async () => await reader.ReadToEndAsync());
                    var jsonResult = jsonTask.GetAwaiter().GetResult();

                    if (!string.IsNullOrEmpty(jsonResult))
                    {
                        using var writer = new StreamWriter(SettingsPath);
                        writer.Write(jsonResult);
                    }

                    success = true;
                }
                catch (IOException ioex)
                {
                    success = false;
                }
            });

            return success;
        }

        public bool WriteSettings(string json)
        {
            var success = false;

            try
            {
                using var writer = new StreamWriter(SettingsPath);
                writer.Write(json);
                success = true;
            }
            catch (IOException ioex)
            {
                success = false;
            }

            return success;
        }
    }
}