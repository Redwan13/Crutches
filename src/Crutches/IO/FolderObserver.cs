using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Crutches.IO
{

    /// <summary>
    /// Observes forlders 
    /// </summary>
    public class FolderObserver : IEquatable<FolderObserver>
    {
        private static readonly FolderObserverCore Core;
        
        static FolderObserver()
        {
            Core = new FolderObserverCore();
        }
        
        public DirectoryInfo SourceFolder { get; private set; }

        private readonly Regex _filterRe;

        public string Filter { get { return _filterRe.ToString(); } }

        internal Regex FilterRe { get { return _filterRe; } }

        public SearchOption SearchOption { get; private set; }

        /// <summary>
        /// Occures when folder starts observing
        /// </summary>
        public event ObservingEventHandler OnStartObserving;
        /// <summary>
        /// Occures when folder stops observing
        /// </summary>
        public event ObservingEventHandler OnStopObserving;
        /// <summary>
        /// Occures when exception is thrown during observing
        /// </summary>
        public event ObservingErrorEventHandler OnErrorObserving;
        /// <summary>
        /// Occures when file matching filter appears in observed folder
        /// </summary>
        public event FileSystemEventHandler OnFileCreated;


        internal readonly ISet<string> RegisteredFiles = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Folder to observe</param>
        /// <param name="searchPattern">Regex pattern to filter files in folder</param>
        /// <param name="searchOption">Search option</param>
        public FolderObserver(string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (searchPattern == null) throw new ArgumentNullException("searchPattern");

            SourceFolder = new DirectoryInfo(path);
            _filterRe = new Regex(searchPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            SearchOption = searchOption;
        }
        
        /// <summary>
        /// Start observing folder
        /// </summary>
        public void StartObserving()
        {
            Core.RegisterSource(this);
            Core.StartObserving();
            if (OnStartObserving != null) OnStartObserving(this, new ObservingEventArgs(SourceFolder.FullName));
        }


        /// <summary>
        /// Stop observing folder
        /// </summary>
        public void StopObserving()
        {
            Core.ForgetSource(this);
            if (OnStopObserving != null)
                OnStopObserving(this, new ObservingEventArgs(SourceFolder.FullName));
        }

        internal void InvokeErrorHandlers(IOException ex)
        {
            if (OnErrorObserving != null)
                OnErrorObserving(this, new ObservingErrorEventArgs(ex));
        }

        internal void InvokeFileCreatedHandlers(WatcherChangeTypes changeType, string dirPath, string fileName)
        {
            if (OnFileCreated != null)
                OnFileCreated(this, new FileSystemEventArgs(changeType, dirPath, fileName));
        }


        public bool Equals(FolderObserver other)
        {
            return String.Equals(this.SourceFolder.FullName, other.SourceFolder.FullName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FolderObserver;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return SourceFolder.GetHashCode();
        }

        
    }
}
