using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Crutches.IO
{
    /// <summary>
    /// Internal cycle of all FolderObservers
    /// </summary>
    class FolderObserverCore
    {
        #region Comparers
        private class FileInfoEqualityComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo x, FileInfo y)
            {
                return x.FullName.Equals(y.FullName);
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }

        private class FolderObserverEqualityComparer : IEqualityComparer<FolderObserver>
        {
            public bool Equals(FolderObserver x, FolderObserver y)
            {
                return String.Equals(x.SourceFolder.FullName, y.SourceFolder.FullName, StringComparison.Ordinal);
            }

            public int GetHashCode(FolderObserver obj)
            {
                return obj.SourceFolder.GetHashCode();
            }
        }
        #endregion

        private const int PortionSize = 500;
        private const int ObserveInterval = 5000;
        
        private readonly FileInfoEqualityComparer _fileComparer = new FileInfoEqualityComparer();
        private readonly FolderObserverEqualityComparer _observerComparer = new FolderObserverEqualityComparer();


        private readonly ISet<FolderObserver> _sources;

        public void RegisterSource(FolderObserver source)
        {
            lock (_sources)
            {
                if (!_sources.Add(source))
                    throw new FolderObserverException("Source already exists");

            }
        }

        public void ForgetSource(FolderObserver source)
        {
            lock (_sources)
            {
                _sources.Remove(source);
                if(_sources.Count ==0)
                    StopObserving();
            }
        }

        private readonly Thread _observeThread;

        private void ObserveCycle()
        {
            while (_observe)
            {
                FindFilesInSources(_firstTime);
                _firstTime = false;
                
                Thread.Sleep(ObserveInterval);
            }
        }


        public FolderObserverCore()
        {
            _sources = new HashSet<FolderObserver>(_observerComparer);

            _observeThread = new Thread(ObserveCycle);
            _observeThread.IsBackground = true;
        }

        public void StartObserving()
        {
            lock (this)
            {
                if (!_observe)
                {
                    _observe = true;
                    _observeThread.Start();
                }
            }
            
        }

        private volatile bool _observe;
        private bool _firstTime = true;

        private void StopObserving()
        {
            _observe = false;
            _observeThread.Join();
        }


        

        private void FindFilesInSources(bool breakInPortions)
        {
            ISet<FolderObserver> tmpSource;
            lock (_sources){tmpSource = new HashSet<FolderObserver>(_sources);}
            foreach (var currentSource in tmpSource)
            {
                
                try
                {
                    var allNewLazyFiles = currentSource.SourceFolder.EnumerateFiles(currentSource.FilterRe,
                        currentSource.SearchOption);

                    var newFiles = new HashSet<FileInfo>(_fileComparer);
                    foreach (var file in allNewLazyFiles)
                    {
                        newFiles.Add(file);
                        if (breakInPortions && newFiles.Count == PortionSize)
                        {
                            RegisterFilesList(currentSource, newFiles, false);
                            newFiles.Clear();
                        }
                    }
                    RegisterFilesList(currentSource, newFiles, false);
                }
                catch (IOException ex)
                {
                    currentSource.InvokeErrorHandlers(ex);
                }
            }
        }


        private void RegisterFilesList(FolderObserver source, ICollection<FileInfo> paths, bool dealWithRemoved)
        {
            // check removed files
            if (dealWithRemoved)
            {
                ISet<string> deleted = new HashSet<string>();
                foreach (var registeredFile in source.RegisteredFiles)
                {
                    if (!paths.Any(x=>x.FullName.Equals(registeredFile)))
                        deleted.Add(registeredFile);
                }
                foreach (var deletedFile in deleted)
                {
                    source.RegisteredFiles.Remove(deletedFile);
                }
            }

            foreach (var file in paths)
            {
                if (!_observe) return; // if observing is stopped break the cycle
                if (source.RegisteredFiles.Contains(file.FullName)) continue;
                
                source.InvokeFileCreatedHandlers(WatcherChangeTypes.Created, source.SourceFolder.FullName, file.Name);
                
                source.RegisteredFiles.Add(file.FullName);
            }
        }
    }
}
