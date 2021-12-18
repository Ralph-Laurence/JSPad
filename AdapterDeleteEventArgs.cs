
using System;

namespace JSPad
{
    class AdapterDeletedEventArgs
    {
        public bool Deleted { get; set; }
        public AdapterDeletedEventArgs(bool deleted) => Deleted = deleted;
    }

    class AdapterDeletingEventArgs
    {
        public bool Deleting { get; set; }
        public AdapterDeletingEventArgs(bool deleting) => Deleting = deleting;
    }
}