using System;

namespace CompassMaui
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
