using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Core.Content;

using Google.Android.Material.Snackbar;

using Java.Interop;

using System;

namespace JSPad
{
    class Dialogue
    {
        /// <summary>
        /// Toast wrapper
        /// </summary>
        /// <param name="context">Base Context</param>
        /// <param name="message">String message</param>
        /// <param name="duration">Toast duration. Short by default</param>
        public static void WriteToast(Context context, string message, ToastLength duration = ToastLength.Short) =>
            Toast.MakeText(context, message, duration).Show();

        // Snackbar styles -> https://stackoverflow.com/a/34020962 => Thanks Dusan Dimitrijevic, Zubair Akber

        /// <summary>
        /// Snackbar with custom background and foreground.
        /// </summary>
        /// <param name="view">The main view that the snackbar is attached to</param>
        /// <param name="messageId">Get from string resource</param>
        /// <param name="background">Snackbar background color</param>
        /// <param name="foreground">Snackbar text color</param>
        /// <param name="duration">How long does snackbar will show</param>
        public static void ShowColoredSnack(View view, int messageId, Color background, Color foreground, int duration = Snackbar.LengthShort)
        {
            var snack = Snackbar.Make(view, messageId, duration);
            var sview = snack.View;
             
            sview.SetBackgroundColor(background);
            sview.FindViewById<TextView>(Resource.Id.snackbar_text).SetTextColor(foreground);

            snack.Show();
        }

        /// <summary>
        /// [Information] Alert Dialog Wrapper
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after positive button is clicked</param>
        /// <param name="cancelButton">Show/Hide cancel button?</param>
        /// <param name="onCancel">Callback after negative button is clicked</param>
        public static void Alert(Activity activity, string msg, Action onOK = null, bool cancelButton = false, Action onCancel = null)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(Resource.String.dialog_alert_title);
            alert.SetIcon(Resource.Drawable.icn_dialog_info);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => onOK?.Invoke());

            // Bind Cancel button if set to true
            if (cancelButton)
            {
                alert.SetNegativeButton(Resource.String.dialog_button_cancel, (e, v) => onCancel?.Invoke());
            }

            alert.Show();
        }
        /// <summary>
        /// [Warning] Alert Dialog Wrapper
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after positive button is clicked</param>
        /// <param name="cancelButton">Show/Hide cancel button?</param>
        /// <param name="onCancel">Callback after negative button is clicked</param>
        public static void Warn(Activity activity, string msg, Action onOK = null, bool cancelButton = false, Action onCancel = null)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(Resource.String.dialog_warn_title);
            alert.SetIcon(Resource.Drawable.icn_dialog_warn);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => onOK?.Invoke());

            // Bind Cancel button if set to true
            if (cancelButton)
            {
                alert.SetNegativeButton(Resource.String.dialog_button_cancel, (e, v) => onCancel?.Invoke());
            }

            alert.Show();
        }
        /// <summary>
        /// [Warning] Alert Dialog Wrapper
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after positive button is clicked</param> 
        public static void Error(Activity activity, string msg, Action onOK = null)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(Resource.String.dialog_error_title);
            alert.SetIcon(Resource.Drawable.icn_dialog_err);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => onOK?.Invoke());
              
            alert.Show();
        }
        /// <summary>
        /// [Confirm] Alert Dialog Wrapper
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after positive button is clicked</param> 
        /// <param name="onCancel">Callback after negative button is clicked</param>
        public static void Confirm(Activity activity, string msg, string title, Action onOK, Action onCancel = null, int icon = Resource.Drawable.icn_dialog_confirm)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(title);
            alert.SetIcon(icon);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => onOK?.Invoke());

            // Bind Cancel button 
            alert.SetNegativeButton(Resource.String.dialog_button_cancel, (e, v) => onCancel?.Invoke());

            alert.Show();
        }
        //
        // CONFIRM DIALOG WITH CUSTOM BUTTONS
        //
        public static void ConfirmOK(Activity activity, string msg, string title, int positive, Action onOK)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(title);
            alert.SetIcon(Resource.Drawable.icn_restart);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(positive, (e, v) => onOK?.Invoke());
             
            alert.Show();
        }
        /// <summary>
        /// [Confirm] Alert Dialog Wrapper
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after YES button is clicked</param> 
        /// <param name="onCancel">Callback after negative button is clicked</param>
        public static void ConfirmYesNo(Activity activity, string msg, string title, Action onOK, Action onCancel = null, int icon = Resource.Drawable.icn_dialog_confirm)
        {
            var alert = new AlertDialog.Builder(activity);
            alert.SetTitle(title);
            alert.SetIcon(icon);
            alert.SetMessage(msg);
            alert.SetCancelable(false);

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_yes, (e, v) => onOK?.Invoke());

            // Bind Cancel button 
            alert.SetNegativeButton(Resource.String.dialog_button_no, (e, v) => onCancel?.Invoke());

            alert.Show();
        }
        /// <summary>
        /// [Information] Alert Dialog Wrapper with 'Do not show again' option
        /// </summary>
        /// <param name="activity">The activity that invokes the dialog</param>
        /// <param name="msg">Alert message</param>
        /// <param name="onOK">Callback after positive button is clicked</param> 
        public static void AlertDontShowAgain (Activity activity, int layoutResourceID, int checkBoxResourceID, int textResourceID, string message, Action onOK = null, Action onChecked = null)
        { 
            var layout = LayoutInflater.From(activity);
            var view = layout.Inflate(layoutResourceID, null);  
            var checkBox = view.FindViewById<CheckBox>(checkBoxResourceID);
            var textView = view.FindViewById<TextView>(textResourceID);

            textView.Text = message;

            var alert = new AlertDialog.Builder(activity);

            alert.SetView(view);
            alert.SetTitle(Resource.String.dialog_alert_title);
            alert.SetIcon(Resource.Drawable.icn_dialog_info); 
            alert.SetCancelable(false);

            // Bind checkbox click
            checkBox.Click += (e, v) => onChecked?.Invoke();

            // Bind OK button click
            alert.SetPositiveButton(Resource.String.dialog_button_ok, (e, v) => onOK?.Invoke()); 
            alert.Show();
        }
         
        //
        // Dialog with progress bar to show the progress while waiting
        //
        public static AlertDialog.Builder IndefinitePreloaderDialog(Activity activity, string message)
        {
            var layout = LayoutInflater.From(activity);
            var view = layout.Inflate(Resource.Layout.dialog_indef_preloader, null);
            var textView = view.FindViewById<TextView>(Resource.Id.dialog_indefinite_preloader_text);

            textView.Text = message;

            var alert = new AlertDialog.Builder(activity);

            alert.SetView(view);
            alert.SetTitle(Resource.String.dialog_task_indefinite_title);
            alert.SetIcon(Resource.Drawable.icn_task);
            alert.SetCancelable(false);

            return alert;
        }

        public static AlertDialog.Builder RangeSliderDialog(Activity activity, string message)
        {
            var layout = LayoutInflater.From(activity);
            var view = layout.Inflate(Resource.Layout.dialog_indef_preloader, null);
            var textView = view.FindViewById<TextView>(Resource.Id.dialog_indefinite_preloader_text);

            textView.Text = message;

            var alert = new AlertDialog.Builder(activity);

            alert.SetView(view);
            alert.SetTitle(Resource.String.dialog_task_indefinite_title);
            alert.SetIcon(Resource.Drawable.icn_task);
            alert.SetCancelable(false);

            return alert;
        }
    } 
}