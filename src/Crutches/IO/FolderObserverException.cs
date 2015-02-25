using System;
using System.Runtime.Serialization;

namespace Crutches.IO
{
    /// <summary>
    /// Exception, that occures during folder observation
    /// </summary>
    [Serializable]
    public class FolderObserverException : CrutchException
    {
        /// <summary>
        /// 
        /// </summary>
        public FolderObserverException() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public FolderObserverException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public FolderObserverException(string message, Exception inner) : base(message, inner) { }
        protected FolderObserverException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
