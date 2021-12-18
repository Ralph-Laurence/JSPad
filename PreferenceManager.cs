using Android.App;
using Android.Content;

namespace JSPad
{
    public class PreferenceManager
    {
        public ISharedPreferences Preferences = null;
        private Context BaseContext = null;

        public PreferenceManager(Context context)
        {
            BaseContext = context;

            Preferences = BaseContext.GetSharedPreferences
            (
                name: PreferenceKeys.PreferenceName,
                mode: 0
            );
        }
        //
        // PREFERENCE SETTERS
        //
        public void SetInt(string key, int value) 
        {
            var editor = Preferences.Edit();

            editor.PutInt(key, value);
            editor.Commit();
        }

        public void SetBool(string key, bool value)
        {
            var editor = Preferences.Edit();

            editor.PutBoolean(key, value);
            editor.Commit();
        }

        public void SetString(string key, string value)
        {
            var editor = Preferences.Edit();

            editor.PutString(key, value);
            editor.Commit();
        }

        //
        // PREFERENCE GETTERS
        //
        public bool GetBool(string key) => Preferences.GetBoolean(key, false);

        public int GetInt(string key) => Preferences.GetInt(key, 0);

        public string GetString(string key) => Preferences.GetString(key, string.Empty);

        public void UseDefaults()
        {
            // Clear prefs
            Preferences.Edit().Clear().Commit(); 

            // Use default font size
            SetInt(PreferenceKeys.EditorFontSize, PreferenceDefaults.FontSize);

            SetBool(PreferenceKeys.PrefsChanged, PreferenceDefaults.PrefsChanged);
        } 
    }
}