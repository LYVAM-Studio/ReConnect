using System.Collections.Generic;
using System.Linq;

namespace Reconnect.Menu
{
    public class History
    {
        private Stack<(MenuState, CursorState)> _stack = new();

        public bool IsEmpty() => _stack.Count == 0;

        public (MenuState, CursorState) Pop() => _stack.Pop();
        
        public void Push(MenuState menu, CursorState cursorState) => _stack.Push((menu, cursorState));

        public void Clear() => _stack.Clear();

        public override string ToString() => string.Join("; ", _stack);

        public void CorruptHistory()
        {
            if (_stack.Any(t => t.Item1 == MenuState.BreadBoard))
                _stack = new Stack<(MenuState, CursorState)>(_stack.Where(t => t.Item1 != MenuState.BreadBoard).Reverse());
        }
    }
}