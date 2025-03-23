using UnityEngine;

public class PhysicsScript : MonoBehaviour
{
    // parameters that can be edited in Editor
    [Header("In Base gravity force constant")]
    public readonly float InBaseGravity = -9.81f; // gravity strength (default: -9.81, earth gravity)
    [Header("Outer Base gravity force constant")]
    public readonly float OuterBaseGravity = -6f; // gravity strength outside of the base (lower to have the moon effect)

    private bool _is_in_base = false;
    public float Gravity
    {
        get => _is_in_base ? InBaseGravity : OuterBaseGravity;
    }

    public void ToggleGravity() => _is_in_base = !_is_in_base;
    public void SetInBase(bool value) => _is_in_base = value;
}
