using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    public CircuitComponentType ComponentType { get; protected set; }

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
    
    protected CircuitComponent(CircuitComponentType type)
    {
        ComponentType = type;

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

    public virtual void SetVoltage(double voltage)
    {
        Voltage = voltage;
    }

    public virtual void SetCurrent(double current)
    {
        Current = current;
    }

    public virtual void Toggle()
    {
    }
}

public enum CircuitComponentType { Wire, Battery, Bulb, Motor, Switch };
public enum Direction { North, South, East, West };


