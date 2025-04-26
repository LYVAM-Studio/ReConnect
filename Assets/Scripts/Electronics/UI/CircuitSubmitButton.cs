using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Graphs;
using Reconnect.Electronics.Components;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class CircuitSubmitButton : MonoBehaviour
    {
        public BreadboardUI BreadboardUI;

        public void Start()
        {
            BreadboardUI = GetComponentInParent<BreadboardUI>();
        }
        
        public void ExecuteCircuit()
        {
            Graph circuitGraph = GraphConverter.CreateGraph(BreadboardUI.Breadboard);
            //Debug.Log($"STATE :::\n"+string.Join('\n', from v in circuitGraph.Vertices select $"{v.GetType().Name[..3]} {v.Name}: [{string.Join(", ", v.AdjacentComponents)}]"));
            circuitGraph.DefineBranches();
            //Debug.Log($"VERTICES({circuitGraph.Vertices.Count}) :::\n"+string.Join('\n', circuitGraph.Vertices));
            string branchesDebug = "";
            foreach (Branch circuitGraphBranch in circuitGraph.Branches)
            {
                branchesDebug += circuitGraphBranch.Display() + '\n';
            }
            //Debug.Log($"BRANCHES({circuitGraph.Branches.Count}) :::\n"+branchesDebug);
            //Debug.Log($"BRANCHES({circuitGraph.Branches.Count}) :::\n"+string.Join('\n', circuitGraph.Branches));
            double intensity = circuitGraph.GetGlobalIntensity();
            //Debug.Log($"INSENTITY ::: {intensity} A");
            Lamp targetLamp = (Lamp) BreadboardUI.Breadboard.Target;
            targetLamp.Set(intensity);
            //Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
            //Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
        }
    }
}