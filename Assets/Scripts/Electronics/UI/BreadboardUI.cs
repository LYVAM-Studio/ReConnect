using System;
using Reconnect.Electronics.Breadboards;
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