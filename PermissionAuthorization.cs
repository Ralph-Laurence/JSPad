using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Core.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSPad 
{
    class PermissionAuthorization
    {
        public readonly int REQUEST_ID_READ_WRITE = 0;
        public readonly string[] READ_WRITE_REQUEST_PERMISSIONS =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        public Activity MainActivity { get; set; }
        public Context BaseContext { get; set; }

        public bool VerifyPermissions(Permission[] permissions, int totalPermissions)
        {
            // Make sure that atleast one permission has been granted
            if (permissions.Length != totalPermissions)
                return false;

            foreach (var perms in permissions)
            {
                if (perms != Permission.Granted)
                    return false;
            }

            return true;
        }
        //
        // CHECK PERMISSION
        //
        public bool IsReadWritePermissionGranted()
        {
            bool granted;

            if (ActivityCompat.CheckSelfPermission(BaseContext, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                ActivityCompat.CheckSelfPermission(BaseContext, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                granted = false;
            }
            else
            {
                granted = true;
            }

            return granted;
        }
        //
        // REQUEST READ AND WRITE PERMISSION
        //
        public void RequestReadWritePermission(Action onRequest)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(MainActivity, Manifest.Permission.ReadExternalStorage) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(MainActivity, Manifest.Permission.WriteExternalStorage))
            {
                onRequest();                
            }
            else
            {
                DirectReadWritePermission();
            }
        }
        //
        // DIRECTLY REQUEST PERMISSION
        //
        public void DirectReadWritePermission()
        {
            ActivityCompat.RequestPermissions(MainActivity, READ_WRITE_REQUEST_PERMISSIONS, REQUEST_ID_READ_WRITE);
        }
    }
}