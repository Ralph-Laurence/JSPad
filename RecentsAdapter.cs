using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSPad
{
    class RecentsAdapter : BaseAdapter<RecentFiles>
    {
        public Activity BaseActivity { get; set; }

        public List<RecentFiles> m_RecentFiles { get; set; }

        public RecentsAdapter(Activity activity, List<RecentFiles> recentFiles) : base()
        {
            BaseActivity = activity;
            m_RecentFiles = recentFiles;
        }

        public override RecentFiles this[int position] => m_RecentFiles[position];

        public override int Count => m_RecentFiles.Count;

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = m_RecentFiles[position];
            var row = convertView;

            var inflater = BaseActivity.LayoutInflater;

            if (convertView == null)
                row = inflater.Inflate(Resource.Layout.list_view_row_items, null, true);

            var titleTxView = row.FindViewById<TextView>(Resource.Id.row_text_recents_title);
            var dateTxView = row.FindViewById<TextView>(Resource.Id.row_text_recents_date);

            titleTxView.Text = item.Filename;
            dateTxView.Text = item.Filedate;

            return row;
        }
    }
}