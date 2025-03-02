using System;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class BreadboardUI : MonoBehaviour
    {
        public Breadboard Breadboard;

        public void Start()
        {
            if (Breadboard is null)
                throw new ArgumentException("No reference to the BreadBoard component");
        }
    }
}