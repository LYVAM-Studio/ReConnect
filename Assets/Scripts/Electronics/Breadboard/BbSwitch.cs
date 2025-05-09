using System;
using Reconnect.Electronics.Breadboards;
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        // TODO : Component not found
        _isOnHash = Animator.StringToHash("isON");
        _outline = GetComponent<Outline>();
        // TODO : Component not found
        _outline.enabled = false;
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
        => Math.Abs(target.GetTension(intensity) - expectedTension) < Breadboard.CircuitInfo.Tolerance; 
    
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
        //Debug.Log($"INSENTITY ::: {intensity} A");
        if (!CheckTension(Breadboard.Target, intensity, Breadboard.CircuitInfo.TargetTension))
        {
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
        ToggleAnimation();
        if (IsOn)
        {
            if (!ExecuteCircuit())
                ToggleAnimation();
        }
        else
        {
            Breadboard.Target.UndoAction();
        }
    }

}
