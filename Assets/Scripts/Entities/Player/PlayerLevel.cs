using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Player
{
    public class PlayerLevel : MonoBehaviour
    {
        public uint value;

        public void LevelUp()
        {
            value++;
            // TODO : display lesson of the new level
            // TODO : display indication on the new level
        }
    }
}