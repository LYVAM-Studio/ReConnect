using TMPro;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class CircuitInstructions : MonoBehaviour
    {
        [SerializeField] private string title;
        [TextArea(5, 15)] [SerializeField] private string instructions;
        [TextArea(3, 8)] [SerializeField] private string footer;

        [SerializeField] private TextMeshProUGUI titleMesh;
        [SerializeField] private TextMeshProUGUI instructionMesh;
        [SerializeField] private TextMeshProUGUI footerMesh;

        [SerializeField] private GameObject leftPanel;
        [SerializeField] private GameObject rightPanel;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            titleMesh.text = title;
            instructionMesh.text = instructions;
            footerMesh.text = footer;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
