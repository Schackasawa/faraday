using UnityEngine;
using System.Collections;

public interface ICircuitLab
{
    void AddComponent(GameObject component, Point start, Point end);
    void RemoveComponent(GameObject component, Point start);
    int GetFreeComponentSlots(Point start, int length);
    bool IsSlotFree(Point start, Point end, int length);
    void BlockPegs(Point start, Point end, bool block);
    void RegisterDynamicComponent(IDynamic component);
    void UnregisterDynamicComponent(IDynamic component);
    void SimulateCircuit();
    void ToggleLabels();
    void Reset();
}

public interface IDispenser
{
    void Reset();
}

public interface IPeg
{
    void Reset();
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


