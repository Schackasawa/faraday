using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    // Public members set in Unity Object Inspector
    public CircuitLab Lab;

    public bool IsPlaced { get; protected set; }
    public bool IsHeld { get; protected set; }
    public bool IsClone { get; set; }
    public bool IsClosed { get; protected set; }

    protected Point StartingPeg { get; set; }
    protected Direction Direction { get; set; }
    protected bool IsActive { get; set; }
    protected bool IsForward { get; set; }
    protected bool IsShortCircuit { get; set; }
    protected double Voltage { get; set; }
    protected double Current { get; set; }
    
    protected CircuitComponent()
    {
        IsPlaced = false;
        IsHeld = false;
        IsClone = false;
        IsClosed = true;
        IsActive = false;
        IsForward = true;
        IsShortCircuit = false;
        Voltage = 0f;
        Current = 0f;
    }

    protected virtual void Start() { }
    protected virtual void Update() { }

    public void Place(Point start, Direction dir)
    {
        IsPlaced = true;
        StartingPeg = start;
        Direction = dir;
    }

    public virtual void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
        IsForward = isForward;
    }

    public virtual void SetShortCircuit(bool isShortCircuit, bool isForward)
    {
        IsShortCircuit = isShortCircuit;
        IsForward = isForward;
    }

    public double GetVoltage()
    {
        return Voltage;
    }

    public virtual void SetVoltage(double voltage)
    {
        Voltage = voltage;
    }

    public virtual void SetCurrent(double current)
    {
        Current = current;
    }

    // Toggle the state of a binary component (for example, a switch)
    public virtual void Toggle()
    {
    }

    // Adjust the behavior of a component with various modes
    public virtual void Adjust()
    {
    }

    // Reset any component-specific state that might need resetting
    protected virtual void Reset()
    {
    }

    public virtual void SelectEntered()
    {
        IsHeld = true;

        // Enable box and sphere colliders so this piece can be placed somewhere else on the board.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (IsPlaced)
        {
            Lab.RemoveComponent(this.gameObject, StartingPeg);

            IsPlaced = false;
        }

        // Reinitialize component state
        Reset();
    }

    public virtual void SelectExited()
    {
        IsHeld = false;

        // Make sure gravity is enabled any time we release the object
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
    }

    protected IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }
}

public enum Direction { North, South, East, West };


