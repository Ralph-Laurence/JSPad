using Android.Webkit;

using Newtonsoft.Json;

using System;

namespace JSPad
{
    class JavaScriptResult : Java.Lang.Object, IValueCallback
    {
        // Event handler with JSResponse as event parameter
        public event EventHandler<JSResponseEventArgs> OnResponse;

        public void OnReceiveValue(Java.Lang.Object value)
        {
            var v = JsonConvert.DeserializeObject(value.ToString());
            // Fire (raise) this event after a command executes
            OnResponse?.Invoke(this, new JSResponseEventArgs(v.ToString()));
        }
    }
}