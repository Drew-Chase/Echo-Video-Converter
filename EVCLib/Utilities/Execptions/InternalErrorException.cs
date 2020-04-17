using System;

namespace ChaseLabs.Echo.Video_Converter.Utilities.Exceptions
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException(string message) : base(message)
        {
        }
    }
}