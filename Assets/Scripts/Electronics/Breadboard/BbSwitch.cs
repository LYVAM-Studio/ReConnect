using Reconnect.MouseEvents;
using UnityEngine;

public class BbSwitch : MonoBehaviour, ICursorHandle
{
    public Animator Animator;
    bool ICursorHandle.IsPointerDown { get; set; }

    // The component responsible for the outlines
    private Outline _outline;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void ICursorHandle.OnCursorEnter()
    {
        _outline.enabled = true;
    }

    void ICursorHandle.OnCursorExit()
    {
        _outline.enabled = false;
    }
        
    void ICursorHandle.OnCursorClick()
    {
        
    }

}
