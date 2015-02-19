using System;
using System.IO;

namespace Crutches.IO
{
    public class ObservingEventArgs : EventArgs
    {
        public string FullPath { get; private set; }

        public ObservingEventArgs(string fullpath)
            : base()
        {
            FullPath = fullpath;
        }

    }

    public class ObservingErrorEventArgs : ErrorEventArgs
    {
        public ObservingErrorEventArgs(Exception exception) : base(exception) { }

        public bool RestartObserving { get; set; }
    }


    public delegate void ObservingEventHandler(object sender, ObservingEventArgs e);

    public delegate void ObservingErrorEventHandler(object sender, ObservingErrorEventArgs e);
}
