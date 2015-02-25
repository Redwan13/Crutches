using System;
using System.Runtime.Serialization;

namespace Crutches
{
    /// <summary>
    /// Base exception type for Crutches
    /// </summary>
    public class CrutchException :Exception
    {
        public CrutchException() { }
        
        public CrutchException(string message) : base(message) { }
        
        public CrutchException(string message, Exception inner) : base(message, inner) { }
        
        protected CrutchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
