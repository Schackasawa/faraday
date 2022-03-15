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
}

public interface ICircuitComponent
{
    void SetActive(bool active, bool forward);
    void SetShortCircuit(bool shortCircuit, bool forward);
    void SetVoltage(double newVoltage);
    void SetCurrent(double newCurrent);
    bool IsClosed();
    void Toggle();
}

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

