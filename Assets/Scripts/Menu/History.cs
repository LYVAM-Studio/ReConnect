using System.Collections.Generic;

namespace Reconnect.Menu
{
    public class History
    {
        private readonly Stack<(Menu, CursorState)> _stack = new();

        public bool IsEmpty() => _stack.Count == 0;

        public (Menu, CursorState) Pop() => _stack.Pop();
        
        public void Push(Menu menu, CursorState cursorState) => _stack.Push((menu, cursorState));

        public void Clear() => _stack.Clear();
    }
}