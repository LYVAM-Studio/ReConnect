using System;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;
using Reconnect.MouseEvents;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class BbSwitch : MonoBehaviour, ICursorHandle
    {
        private Animator _animator;
        public Breadboard Breadboard;
        bool ICursorHandle.IsPointerDown { get; set; }

        // The component responsible for the outlines
        private Outline _outline;

        private int _isOnHash;

        private bool IsOn
        {
            get => _animator.GetBool(_isOnHash);
            set => _animator.SetBool(_isOnHash, value);
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator is null)
                throw new ComponentNotFoundException("No Animator component has been found on the switch");
            _isOnHash = Animator.StringToHash("isON");
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("No Outline component has been found on the switch.");
            _outline.enabled = false;
            if (!_animator.TryGetComponent(out BbSwitchAnimation childrenAnimationScript))
                throw new ComponentNotFoundException(
                    "no BbSwitchAnimation component has been found in the switch prefab children");
            childrenAnimationScript.bbSwitch = this;
        }

        void ICursorHandle.OnCursorEnter()
        {
            _outline.enabled = true;
        }

        void ICursorHandle.OnCursorExit()
        {
            _outline.enabled = false;
        }

        private void ToggleAnimation() => IsOn = !IsOn;

        private bool CheckTension(ElecComponent target, double intensity, double expectedTension)
            => Math.Abs(target.GetTension(intensity) - expectedTension) / expectedTension < Breadboard.CircuitInfo.TargetTolerance;
        
        private bool CheckIntensity(double intensity, double expectedIntensity)
            => Math.Abs(intensity - expectedIntensity) / expectedIntensity < Breadboard.CircuitInfo.TargetTolerance;

        private bool ExecuteCircuit()
        {
            Graph circuitGraph = GraphConverter.CreateGraph(Breadboard);
            circuitGraph.DefineBranches();
            double intensity = circuitGraph.GetGlobalIntensity();
            
            // WriteReport(circuitGraph, intensity);
            
            if (Breadboard.CircuitInfo.TargetQuantity is CircuitInfo.Quantity.Tension)
            {
                if (!CheckTension(Breadboard.Target, intensity, Breadboard.CircuitInfo.TargetValue))
                    return false;
            }
            else
            {
                if (!CheckIntensity(intensity, Breadboard.CircuitInfo.TargetValue))
                    return false;
            }
            
            Breadboard.Target.DoAction();
            return true;
        }
        void ICursorHandle.OnCursorClick()
        {
            ToggleAnimation();
        }

        public void OnSwitchStartUp()
        {
            Breadboard.Target.UndoAction();
        }

        private void OnFailedExercise()
        {
            ToggleAnimation(); // automatic shutdown of the switch
            // TODO: handle KO of the player
        }
        
        public void OnSwitchIdleDown()
        {
            if (!ExecuteCircuit())
                OnFailedExercise();
        }
        
        // Debug function
        private void WriteReport(Graph circuitGraph, double intensity, string path = "CircuitReport.md")
        {
            using System.IO.StreamWriter writer = new System.IO.StreamWriter(path);
            writer.WriteLine("## State");
            writer.WriteLine();
            foreach (Vertex v in circuitGraph.Vertices)
            {
                writer.WriteLine($"- {v.GetType().Name} {v.Name}:");
                foreach (Vertex adj in v.AdjacentComponents)
                    writer.WriteLine($"  - {adj}");
            }

            writer.WriteLine();
            writer.WriteLine("## Vertices");
            writer.WriteLine();
            writer.WriteLine($"**Number: {circuitGraph.Vertices.Count}**");
            writer.WriteLine();
            foreach (Vertex v in circuitGraph.Vertices)
                writer.WriteLine($"- {v}");
            writer.WriteLine();
            writer.WriteLine("## Branches");
            writer.WriteLine();
            writer.WriteLine($"**Number: {circuitGraph.Branches.Count}**");
            writer.WriteLine();
            foreach (Branch b in circuitGraph.Branches)
            {
                writer.WriteLine(
                    $"- {b.Resistance:0.##} Ohms from {b.StartNode} to {b.EndNode}");
                foreach (var c in b.Components)
                    writer.WriteLine($"  - {c}");
            }

            writer.WriteLine();
            writer.WriteLine("## Result");
            writer.WriteLine();
            writer.WriteLine($"- Actual intensity: {intensity:0.##} A");
            writer.WriteLine($"- Actual tension: {Breadboard.Target.GetTension(intensity):0.##} V");
            var tVal = Breadboard.CircuitInfo.TargetValue;
            writer.WriteLine($"- Expected tension: {tVal:0.##} V");
            writer.WriteLine($"- Actual percentage: {Math.Abs(Breadboard.Target.GetTension(intensity) - tVal) / tVal * 100:0.##}%");
            writer.WriteLine($"- Tolerance: {Breadboard.CircuitInfo.TargetTolerance * 100:0.##}%");
        }
    }
}