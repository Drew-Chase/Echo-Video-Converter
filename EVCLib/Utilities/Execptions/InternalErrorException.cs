using System;

namespace ChaseLabs.Echo.Video_Converter.Util.Exceptions
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException(string message) : base(message)
        {
        }
    }
}