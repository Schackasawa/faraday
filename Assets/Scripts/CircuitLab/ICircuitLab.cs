using UnityEngine;
using System.Collections;

public interface ICircuitLab
{
    void AddComponent(GameObject component, Point start, Point end);
    void RemoveComponent(GameObject component, Point start);
    int GetFreeComponentSlots(Point start, int length);
    bool IsSlotFree(Point start, Point end, int length);
    void BlockPegs(Point start, Point end, bool block);
    void SimulateCircuit();
    void ToggleLabels();
    void Reset();
}

public interface ICircuitComponent
{
    void SetActive(bool active, bool forward);
    void SetShortCircuit(bool shortCircuit, bool forward);
    void SetVoltage(double newVoltage);
    void SetCurrent(double newCurrent);
    void SetClone();
    void Place(Point start, Direction dir);
    bool IsHeld();
    bool IsPlaced();
    bool IsClosed();
    void Toggle();
}

public interface IDispenser
{
    void Reset();
}

public interface IPeg
{
    void Reset();
}

public enum Direction { North, South, East, West };

public struct Point
{
    public int x, y;
    public Point(int px, int py)
    {
        x = px;
        y = py;
    }

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }
}

