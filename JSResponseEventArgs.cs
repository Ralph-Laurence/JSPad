
using System;
using System.Text.RegularExpressions;

namespace JSPad
{
    class JSResponseEventArgs : EventArgs
    {
        public string Result { get; set; }

        // This will recieve the value that was passed from the raised event
        // Unescape characters
        public JSResponseEventArgs(string result) => Result = Regex.Unescape(result);
    }
}