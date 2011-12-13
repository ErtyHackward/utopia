using System;

namespace LostIsland.Shared.Web
{
    public class WebEventArgs<T> : EventArgs
    {
        public T Responce { get; set; }
        public Exception Exception { get; set; }
    }
}