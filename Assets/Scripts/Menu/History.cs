using System.Collections.Generic;

namespace Reconnect.Menu
{
    public class History
    {
        private readonly Stack<(MenuState, CursorState)> _stack = new();

        public bool IsEmpty() => _stack.Count == 0;

        public (MenuState, CursorState) Pop() => _stack.Pop();
        
        public void Push(MenuState menu, CursorState cursorState) => _stack.Push((menu, cursorState));

        public void Clear() => _stack.Clear();

        public override string ToString()
            => string.Join("; ", _stack);
    }
}