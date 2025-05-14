using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Reconnect.Menu
{
    public class KnockOutMenuManager : MonoBehaviour
    {
        public static KnockOutMenuManager Instance;
        [SerializeField] private TMP_Text timerText;
        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("A KnockOutMenuManager has already been instantiated.");
            Instance = this;
        }

        public IEnumerator KnockOutForSeconds(uint seconds, Action endAction)
        {
            for (uint i = seconds; i > 0; i--)
            {
                timerText.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            endAction();
        }
    }
}
