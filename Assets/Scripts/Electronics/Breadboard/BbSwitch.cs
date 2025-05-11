using System;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.CircuitLoading;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graphs;
using Reconnect.MouseEvents;
using UnityEngine;

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
    
    private bool IsUp
    {
        get
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("IdleUp");
        }
    }
    
    private bool IsDown
    {
        get
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("IdleDown");
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        // TODO : Component not found
        _isOnHash = Animator.StringToHash("isON");
        _outline = GetComponent<Outline>();
        // TODO : Component not found
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
        => Math.Abs(target.GetTension(intensity) - expectedTension) < Breadboard.CircuitInfo.TargetTolerance;
    
    private bool CheckIntensity(ElecComponent target, double tension, double expectedIntensity)
        => Math.Abs(target.GetIntensity(tension) - expectedIntensity) < Breadboard.CircuitInfo.TargetTolerance;
    
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
        // Debug.Log($"INSENTITY ::: {intensity} A");
        // Debug.Log($"{Breadboard.Target.GetTension(intensity)} {Breadboard.CircuitInfo.TargetValue}");

        if (Breadboard.CircuitInfo.TargetQuantity is CircuitInfo.Quantity.Tension)
        {
            if (!CheckTension(Breadboard.Target, intensity, Breadboard.CircuitInfo.TargetValue))
                return false;
        }
        else
        {
            if (!CheckIntensity(Breadboard.Target, intensity, Breadboard.CircuitInfo.TargetValue))
                // TODO: what to put there? ______ ^^^^^^^^^
                return false;
        }
        
        Breadboard.Target.Action();
        return true;
        
        //Debug.Log(targetLamp.isLampOn(intensity) ? "The lamp is ON ! Success" : "The lamp is OFF ! You failed");
        //Debug.Log($"tension of the target : {targetLamp.GetVoltage(intensity)} Volts");
    }
    void ICursorHandle.OnCursorClick()
    {
        Debug.Log("clicked");
        //_animator.Play("LeverDown");
        ToggleAnimation();
    }

    public void OnSwitchIdleUp()
    {
        Breadboard.Target.UndoAction();
    }
    
    public void OnSwitchIdleDown()
    {
        if (!ExecuteCircuit())
            ToggleAnimation();
    }
    
}
