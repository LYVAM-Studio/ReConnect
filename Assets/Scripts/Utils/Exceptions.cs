using System;

namespace Reconnect.Utils
{
    public class ComponentNotFoundException: Exception
    {
        public ComponentNotFoundException() { }
        public ComponentNotFoundException(string message) : base(message) { }
        public ComponentNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class GameObjectNotFoundException: Exception
    {
        public GameObjectNotFoundException() { }
        public GameObjectNotFoundException(string message) : base(message) { }
        public GameObjectNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class UnreachableCaseException: Exception
    {
        public UnreachableCaseException() { }
        public UnreachableCaseException(string message) : base(message) { }
        public UnreachableCaseException(string message, Exception innerException) : base(message, innerException) { }
    }
}