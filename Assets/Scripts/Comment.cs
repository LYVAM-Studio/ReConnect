using UnityEngine;

namespace Reconnect
{
    // This script does nothing. It is used to display a comment on the unity inspector.
    public class Comment : MonoBehaviour
    {
        [TextArea(3, 10)]
        public string comment = "Write your comment here!";
    }
}