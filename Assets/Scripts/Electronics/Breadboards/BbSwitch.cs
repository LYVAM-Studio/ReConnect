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
        void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            // TODO : Component not found
            _isOnHash = Animator.StringToHash("isON");
            if (!TryGetComponent(out _outline))
                throw new ComponentNotFoundException("No Outline component has been found on the switch.");
            _outline.enabled = false;
            BbSwitchAnimation childrenAnimationScript = _animator.GetComponent<BbSwitchAnimation>();
            if (childrenAnimationScript != null)
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
        
        public bool ExecuteCircuit()
        {
            Graph circuitGraph = GraphConverter.CreateGraph(Breadboard);
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
            // Debug.Log($"INTENSITY ::: {intensity} A");
            // Debug.Log($"{Breadboard.Target.GetTension(intensity)} {Breadboard.CircuitInfo.TargetValue}");

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
            
            Breadboard.Target.Action();
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
        
    }
}