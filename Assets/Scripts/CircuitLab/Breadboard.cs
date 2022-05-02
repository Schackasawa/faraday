using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

public enum CircuitComponentType { Wire, LongWire, Battery, Bulb, Motor, Switch };

public class CircuitComponent
{
    public GameObject GameObject { get; set; }
    public CircuitComponentType ComponentType { get; set; }
    public Point Start { get; set; }
    public Point End { get; set; }
    public int Generation { get; set; }
    public int ShortCircuitGeneration { get; set; }
    public bool ShortCircuitForward { get; set; }
    public Export<double> VoltageExport { get; set; }
    public Export<double> CurrentExport { get; set; }

    public bool ActiveCircuit { get; set; }
    public bool ActiveShort { get; set; }

    public CircuitComponent(GameObject gameObject, CircuitComponentType type, Point start, Point end)
    {
        GameObject = gameObject;
        ComponentType = type;
        Start = start;
        End = end;
    }
}

public class Peg
{
    public GameObject GameObject { get; set; }
    public List<CircuitComponent> Components { get; set; }
    public bool IsBlocked { get; set; }

    public Peg()
    {
        Components = new List<CircuitComponent>();
        IsBlocked = false;
    }
}

public class Board
{
    public List<CircuitComponent> Components { get; set; }
    private Peg[,] Pegs { get; set; }
    private int Rows { get; set; }
    private int Cols { get; set; }
    public int Generation { get; set; }

    public Board(int rows, int cols)
    {
        Components = new List<CircuitComponent>();
        Generation = 0;

        Rows = rows;
        Cols = cols;

        Pegs = new Peg[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Pegs[i, j] = new Peg();
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                Pegs[i, j].GameObject.SetActive(true);
                Pegs[i, j].IsBlocked = false;
                Pegs[i, j].Components.Clear();
            }
        }

        Components.Clear();
    }

    public void AddComponent(CircuitComponent component)
    {
        // Add it to our master list of components
        Components.Add(component);

        // Link the new component to both the starting and ending pegs
        Pegs[component.Start.y, component.Start.x].Components.Add(component);
        Pegs[component.End.y, component.End.x].Components.Add(component);
    }

    public Peg GetPeg(Point coords)
    {
        if (coords.x < 0 || coords.x >= Cols || coords.y < 0 || coords.y >= Rows)
        {
            return null;
        }
        return Pegs[coords.y, coords.x];
    }

    public void SetPegGameObject(Point coords, GameObject gameObject)
    {
        if (coords.x < 0 || coords.x >= Cols || coords.y < 0 || coords.y >= Rows)
        {
            return;
        }
        Pegs[coords.y, coords.x].GameObject = gameObject;
    }

    public void BlockPeg(Point coords, bool block)
    {
        Peg peg = GetPeg(coords);
        if (peg != null)
        {
            peg.GameObject.SetActive(!block);
            peg.IsBlocked = block;
        }
    }
}


