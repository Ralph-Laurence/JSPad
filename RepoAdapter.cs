using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSPad
{
    class RepoAdapter : BaseAdapter<RepoFiles>
    {
        private Activity BaseActivity { get; set; }
        private List<RepoFiles> m_RepoFiles { get; set; }
        private Context BaseContext { get; set; }

        private ImageButton EditButton = null;
        private ImageButton DeleteButton = null;

        public event EventHandler<AdapterDeletedEventArgs> OnAdapterDataDeleted;
        public event EventHandler<AdapterDeletingEventArgs> OnAdapterDeletingData;
         

        public RepoAdapter(Activity activity, Context context, List<RepoFiles> repoFiles) : base()
        {
            BaseActivity = activity;
            m_RepoFiles = repoFiles;
            BaseContext = context;
        }

        public override RepoFiles this[int position] => m_RepoFiles[position];
        public override int Count => m_RepoFiles.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        { 
            var item = m_RepoFiles[position];
            var row = convertView;

            var inflater = BaseActivity.LayoutInflater;

            if (convertView == null)
                row = inflater.Inflate(Resource.Layout.repo_list_layout, null, true);

            var titleTxView = row.FindViewById<TextView>(Resource.Id.row_text_repo_title);
            var dateTxView = row.FindViewById<TextView>(Resource.Id.row_text_repo_date);

            EditButton = row.FindViewById<ImageButton>(Resource.Id.btn_edit_repo);
            DeleteButton = row.FindViewById<ImageButton>(Resource.Id.btn_delete_repo);

            // Thanks to sunita -> https://stackoverflow.com/a/45518262
            if (!DeleteButton.HasOnClickListeners)
            {
                DeleteButton.Click += (s, v) =>
                {
                    Dialogue.Confirm(BaseActivity, $"\"{item.Filename}\" will be deleted permanently. Continue?", "Delete Script", 
                    onOK: () => 
                    {
                        OnAdapterDeletingData?.Invoke(this, new AdapterDeletingEventArgs(true));

                        var taskDelScript = Task.Run(async () => await EditorIO.DeleteFileAsync(BaseContext, item.Filename));
                        var delScripResult = taskDelScript.GetAwaiter().GetResult();

                        if (delScripResult)
                        {
                            Dialogue.WriteToast(BaseContext, "Script successfully deleted", ToastLength.Short);

                            OnAdapterDataDeleted?.Invoke(this, new AdapterDeletedEventArgs(true)); 
                        }
                        else
                        { 
                            Dialogue.Error(BaseActivity, $"Unable to remove \"{item.Filename}\" from the repository.");
                        }
                    });
                };
            }

            if (!EditButton.HasOnClickListeners)
            {
                EditButton.Click += (s, e) =>
                {
                    LaunchActivity(typeof(EditorActivity), new Dictionary<string, string>()
                    {
                        { "ScriptName", item.Filename }
                    });
                };
            }

            titleTxView.Text = item.Filename;
            dateTxView.Text = item.Filedate; 
             
            return row;
        }

        //
        // Open the main screen for creating new projects
        //
        private void LaunchActivity(Type intent, Dictionary<string, string> extras = null)
        {
            var activity = new Intent(BaseContext, intent);
            activity.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            if (extras != null)
                foreach (KeyValuePair<string, string> kvp in extras)
                    activity.PutExtra(kvp.Key, kvp.Value);

            BaseActivity.StartActivity(activity);
            BaseActivity.FinishAffinity();
        }
    } 
}