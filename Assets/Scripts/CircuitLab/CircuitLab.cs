using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using UnityEngine.XR.Interaction.Toolkit;

public class CircuitLab : MonoBehaviour, ICircuitLab
{
    public AudioSource circuitSound;
    public AudioSource shortSound1;
    public AudioSource shortSound2;
    public float circuitSoundStartTime = 0f;
    public float shortSound1StartTime = 0f;
    public float shortSound2StartTime = 0f;
    public float fadeDelay = 2f;
    public float fadeTime = 2;
    public GameObject handle;
    public bool showLabels = false;

    public GameObject pegTemplate = null;
    public float pegInterval = 0.1f;
    public float pegHeight = 0.45f;
    public Vector3 pegScale;

    Board board;
    const int numRows = 9;
    const int numCols = 9;

    float yHandleStart = 0f;
    float yTableStart = 0f;

    void Start()
    {
        // Record the initial height of the handle so we can move the whole board when the handle moves
        yHandleStart = handle.transform.position.y;
        yTableStart = transform.localPosition.y;

        // Create a new board object to hold all breadboard state
        board = new Board(numRows, numCols);

        // Create peg objects at regular intervals across the board
        CreatePegs();
    }

    void Update()
    {
        // Get the handle's current y position and move the entire circuit lab to match
        float yHandleCurrent = handle.transform.position.y;
        transform.localPosition = new Vector3(transform.localPosition.x, yTableStart + (yHandleCurrent - yHandleStart), transform.localPosition.z);
    }

    public void ToggleLabels()
    {
        showLabels = !showLabels;
    }

    public void Reset()
    {
        // Find all dispensers and tell them to clean up their components
        GameObject[] dispensers;
        dispensers = GameObject.FindGameObjectsWithTag("Dispenser");
        foreach (GameObject dispenser in dispensers)
        {
            dispenser.GetComponent<IDispenser>().Reset();
        }

        // Find all pegs and tell them to reset their state
        GameObject[] pegs;
        pegs = GameObject.FindGameObjectsWithTag("Peg");
        foreach (GameObject peg in pegs)
        {
            peg.GetComponent<IPeg>().Reset();
        }

        // Reset all circuit lab state
        board.Reset();
    }

    public void CreatePegs()
    {
        // Create a matrix of pegs
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                CreatePeg(i, j);
            }
        }
    }

    private void CreatePeg(int row, int col)
    {
        string name = "Peg_" + row.ToString() + "_" + col.ToString();

        // Get bounds of breadboard
        var boardObject = GameObject.Find("Breadboard").gameObject;
        var mesh = boardObject.GetComponent<MeshFilter>().mesh;
        var size = mesh.bounds.size;
        var boardWidth = size.x * boardObject.transform.localScale.x;
        var boardHeight = size.z * boardObject.transform.localScale.z;
        //Debug.Log("Board Dimensions = " + boardWidth.ToString() + " x " + boardHeight.ToString());

        // Create a new peg
        var position = new Vector3(-(boardWidth / 2.0f) + ((col + 1) * pegInterval), pegHeight, -(boardHeight / 2.0f) + ((row + 1) * pegInterval));
        var rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        var peg = Instantiate(pegTemplate, position, rotation) as GameObject;
        peg.transform.parent = boardObject.transform;
        peg.transform.localPosition = position;
        peg.transform.localRotation = rotation;
        peg.transform.localScale = pegScale;

        peg.name = name;

        Point coords = new Point(col, row);
        board.SetPegGameObject(coords, peg);
    }

    public void AddComponent(GameObject component, Point start, Point end)
    {
        // Figure out what type of component this is
        string name = component.name;
        //Debug.Log("Adding component: " + name + ", Start(" + start.x + "," + start.y + "), End(" + end.x + "," + end.y + ")");
        CircuitComponentType type = CircuitComponentType.Wire;
        if (name.Contains("Battery"))
        {
            type = CircuitComponentType.Battery;
        }
        else if (name.Contains("Bulb"))
        {
            type = CircuitComponentType.Bulb;
        }
        else if (name.Contains("Wire"))
        {
            type = CircuitComponentType.Wire;
        }
        else if (name.Contains("LongWire"))
        {
            type = CircuitComponentType.LongWire;
        }
        else if (name.Contains("Motor"))
        {
            type = CircuitComponentType.Motor;
        }
        else if (name.Contains("Switch"))
        {
            type = CircuitComponentType.Switch;
        }
        else
        {
            Debug.Log("Unrecognized component type: " + name + "!");
        }

        // Create a component object and link it to the starting and ending pegs
        CircuitComponent newComponent = new CircuitComponent(component, type, start, end);
        board.AddComponent(newComponent);

        // Hide any pegs that we are blocking with the new component
        BlockPegs(start, end, true);

        // Run a new circuit simulation so that any circuits we've just created get activated
        SimulateCircuit();
    }

    public void RemoveComponent(GameObject component, Point start)
    {
        Peg pegA = board.GetPeg(start);
        if (pegA != null)
        {
            CircuitComponent found = pegA.Components.Find(x => x.GameObject == component);
            if (found != null)
            {
                Peg pegB = board.GetPeg(found.End);
                if (pegB != null)
                {
                    // Remove it from each of the pegs it's attached to
                    if (!pegA.Components.Remove(found))
                        Debug.Log("Failed to remove component from Peg A!");
                    if (!pegB.Components.Remove(found))
                        Debug.Log("Failed to remove component from Peg B!");

                    // Remove it from the master list as well
                    board.Components.Remove(found);

                    // Unblock/unhide intermediate pegs
                    BlockPegs(found.Start, found.End, false);
                }
            }
        }

        // Deactivate the component
        var script = component.GetComponent<ICircuitComponent>();
        if (script != null)
        {
            script.SetActive(false, false);
        }

        // Run a new circuit simulation so that any circuits we've just broken get deactivated
        SimulateCircuit();
    }

    public int GetFreeComponentSlots(Point start, int length)
    {
        int freeSlots = 0;

        // Check north
        if ((start.y + length < numRows) && (IsSlotFree(start, new Point(start.x, start.y + length), length)))
        {
            freeSlots++;
        }

        // Check south
        if ((start.y + length >= 0) && (IsSlotFree(start, new Point(start.x, start.y - length), length)))
        {
            freeSlots++;
        }

        // Check east
        if ((start.x + length < numCols) && (IsSlotFree(start, new Point(start.x + length, start.y), length)))
        {
            freeSlots++;
        }

        // Check west
        if ((start.x - length >= 0) && (IsSlotFree(start, new Point(start.x - length, start.y), length)))
        {
            freeSlots++;
        }

        return freeSlots;
    }

    protected bool LinesOverlap(Point startA, Point endA, Point startB, Point endB)
    {
        Point startC = startB;
        Point endC = endB;

        // Reverse second segment if necessary
        if (startA.x != startB.x || startA.y != startB.y)
        {
            startC = endB;
            endC = startB;
        }

        // Figure out if the two lines overlap
        if ((endC.x > startC.x && endA.x > startA.x) ||
            (endC.x < startC.x && endA.x < startA.x) ||
            (endC.y > startC.y && endA.y > startA.y) ||
            (endC.y < startC.y && endA.y < startA.y))
        {
            return true;
        }

        return false;
    }

    public bool IsSlotFree(Point start, Point end, int length)
    {
        // If there are any components at start that are connected in the direction
        // of end, then we already know this slot is blocked, regardless of length.
        Peg pegStart = board.GetPeg(start);
        foreach (CircuitComponent component in pegStart.Components)
        {
            if (LinesOverlap(start, end, component.Start, component.End))
            {
                return false;
            }
        }

        // Make a list of all pegs between start and end
        List<Point> points = new List<Point>();
        if (start.x != end.x)
        {
            int xStart = (start.x < end.x ? start.x : end.x);
            int xEnd = (start.x < end.x ? end.x : start.x);
            for (int x = xStart; x <= xEnd; x++)
            {
                points.Add(new Point(x, start.y));
            }
        }
        if (start.y != end.y)
        {
            int yStart = (start.y < end.y ? start.y : end.y);
            int yEnd = (start.y < end.y ? end.y : start.y);
            for (int y = yStart; y <= yEnd; y++)
            {
                points.Add(new Point(start.x, y));
            }
        }

        // If the component we are trying to place is longer than one segment, make sure
        // none of the intermediate pegs have *any* components attached to them at all.
        if (length > 1)
        {
            for (int i = 1; i < length; i++)
            {
                // Make sure we aren't out of bounds. If we go out of bounds, then this slot
                // isn't big enough for the desired component, so we can return false immediately.
                Peg peg = board.GetPeg(points[i]);
                if (peg == null)
                {
                    return false;
                }

                if (peg.Components.Count > 0)
                {
                    return false;
                }
            }
        }

        // Search each peg to find out if there are components linking any two of them
        foreach (Point pointA in points)
        {
            // If any of the points are blocked, then the entire slot is blocked
            Peg pegA = board.GetPeg(pointA);
            if (pegA != null && pegA.IsBlocked)
            {
                return false;
            }

            foreach (Point pointB in points)
            {
                // Skip identity comparison
                if (pointB.x == pointA.x && pointB.y == pointA.y)
                {
                    continue;
                }

                // Make sure we aren't out of bounds. If we go out of bounds, then this slot
                // isn't big enough for the desired component, so we can return false immediately.
                Peg pegB = board.GetPeg(pointB);
                if (pegB == null)
                {
                    return false;
                }

                // Check all of the components attached to pegB to see if any of them
                // are also attached to pegA. If so, it is blocking the slot.
                foreach (CircuitComponent component in pegB.Components)
                {
                    if (((component.Start.x == pointA.x) && (component.Start.y == pointA.y)) ||
                        ((component.End.x == pointA.x) && (component.End.y == pointA.y)))
                    {
                        // The two pegs are connected, so this slot isn't free!
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void BlockPegs(Point start, Point end, bool block)
    {
        // Hide all pegs between start and end
        List<Point> points = new List<Point>();
        if (start.x != end.x)
        {
            int xStart = (start.x < end.x ? start.x : end.x);
            int xEnd = (start.x < end.x ? end.x : start.x);
            for (int x = xStart + 1; x < xEnd; x++)
            {
                Point coords = new Point(x, start.y);
                board.BlockPeg(coords, block);
            }
        }
        if (start.y != end.y)
        {
            int yStart = (start.y < end.y ? start.y : end.y);
            int yEnd = (start.y < end.y ? end.y : start.y);
            for (int y = yStart + 1; y < yEnd; y++)
            {
                Point coords = new Point(start.x, y);
                board.BlockPeg(coords, block);
            }
        }
    }

    private void DeactivateComponent(CircuitComponent component)
    {
        var script = component.GameObject.GetComponent<ICircuitComponent>();
        if (script != null)
        {
            script.SetActive(false, false);
        }
    }

    private void SetShortComponent(CircuitComponent component, bool shortCircuit, bool forward)
    {
        var script = component.GameObject.GetComponent<ICircuitComponent>();
        if (script != null)
        {
            script.SetShortCircuit(shortCircuit, forward);
        }
    }

    private void SetVoltage(CircuitComponent component, double voltage)
    {
        var script = component.GameObject.GetComponent<ICircuitComponent>();
        if (script != null)
        {
            script.SetVoltage(voltage);
        }
    }

    private void SetCurrent(CircuitComponent component, double current)
    {
        var script = component.GameObject.GetComponent<ICircuitComponent>();
        if (script != null)
        {
            script.SetCurrent(current);
        }
    }

    public void SimulateCircuit()
    {
        //Debug.Log("SIMULATE START");

        // Bump the generation number for this circuit simulation
        int gen = ++board.Generation;

        // First, search for all batteries so we know where to start
        List<CircuitComponent> batteries = new List<CircuitComponent>();
        foreach (CircuitComponent component in board.Components)
        {
            if (component.ComponentType == CircuitComponentType.Battery)
            {
                batteries.Add(component);
            }
        }

        foreach (CircuitComponent battery in batteries)
        {
            // Skip batteries that have already been included in a circuit
            if (battery.Generation == gen)
            {
                continue;
            }

            List<CircuitComponent> circuit = new List<CircuitComponent>();
            List<CircuitComponent> components = new List<CircuitComponent>();
            List<SpiceSharp.Circuits.Entity> entities = new List<SpiceSharp.Circuits.Entity>();

            // Add the battery as the first component
            circuit.Add(battery);
            components.Add(battery);

            int resistors = 0;

            // Leaving the positive terminal of the battery (to follow current flow), do a depth-first 
            // search to find each circuit that eventually leads to the negative terminal of the battery
            Point currPosition = battery.End;
            Peg peg = board.GetPeg(currPosition);

            foreach (CircuitComponent component in peg.Components)
            {
                if (component != battery)
                {
                    FindCircuit(circuit, entities, components, component, currPosition, resistors, gen);
                }
            }

            // Case 1: There's a short circuit on this battery
            if (battery.ShortCircuitGeneration == gen)
            {
                foreach (CircuitComponent component in components)
                {
                    // Highlight each component involved in the short circuit, and deactivate all 
                    // other connected components.
                    if (component.ShortCircuitGeneration == gen)
                    {
                        SetShortComponent(component, true, component.ShortCircuitForward);
                    }
                    else
                    {
                        DeactivateComponent(component);
                    }
                }

                if (!battery.ActiveShort)
                {
                    battery.ActiveShort = true;
                    battery.ActiveCircuit = false;
                    StartCoroutine(PlaySound(shortSound1, shortSound1StartTime));
                    StartCoroutine(PlaySound(shortSound2, shortSound2StartTime));
                }
            }
            // Case 2: A normal circuit has been completed on this battery
            else if (battery.Generation == gen)
            {
                var ssCircuit = new Circuit(entities);

                // Create an Operating Point Analysis for the circuit
                var op = new OP("DC 1");

                // Create exports so we can access component properties
                foreach (CircuitComponent component in components)
                {
                    bool isBattery = (component.ComponentType == CircuitComponentType.Battery);

                    if (component.Generation == gen)
                    {
                        component.VoltageExport = new RealVoltageExport(op, isBattery ? component.End.ToString() : component.Start.ToString());
                        component.CurrentExport = new RealPropertyExport(op, "V" + component.GameObject.name, "i");
                    }
                }

                // Catch exported data
                op.ExportSimulationData += (sender, args) =>
                {
                    var input = args.GetVoltage(battery.End.ToString());
                    var output = args.GetVoltage(battery.Start.ToString());
                    //Debug.Log("      ### IN: " + input + ", OUT: " + output);

                    foreach (CircuitComponent component in components)
                    {
                        if (component.Generation == gen)
                        {
                            // Update the voltage value
                            var voltage = component.VoltageExport.Value - output;
                            //Debug.Log("      ### " + component.GameObject.name + " Voltage at " + component.Start.ToString() + "-" + component.End.ToString() + ": " + voltage);
                            SetVoltage(component, voltage);

                            // Update the current value
                            var current = component.CurrentExport.Value;
                            //Debug.Log("      ### " + component.GameObject.name + " Current at " + component.Start.ToString() + "-" + component.End.ToString() + ": " + current);
                            SetCurrent(component, current);
                        }
                    }
                };

                // Run the simulation
                try
                {
                    op.Run(ssCircuit);

                    if (!battery.ActiveCircuit)
                    {
                        battery.ActiveCircuit = true;
                        battery.ActiveShort = false;
                        StartCoroutine(PlaySound(circuitSound, circuitSoundStartTime));
                    }
                }
                catch
                {
                    Debug.Log("Simulation Error! Caught exception...");

                    // Treat a simulation error as a short circuit
                    battery.ActiveShort = true;
                    battery.ActiveCircuit = false;
                    StartCoroutine(PlaySound(shortSound1, shortSound1StartTime));
                    StartCoroutine(PlaySound(shortSound2, shortSound2StartTime));

                    // Deactivate all components in this circuit
                    foreach (CircuitComponent component in components)
                    {
                        DeactivateComponent(component);
                    }
                }
            }
            // Case 3: No complete circuit
            else
            {
                battery.ActiveShort = false;
                battery.ActiveCircuit = false;
            }
        }

        // Deactivate any components that didn't get included in any complete circuits this time
        foreach (CircuitComponent component in board.Components)
        {
            if (component.Generation != gen)
            {
                DeactivateComponent(component);
            }
        }

        // Clear any components that didn't get included in any short circuits this time
        foreach (CircuitComponent component in board.Components)
        {
            if (component.ShortCircuitGeneration != gen)
            {
                SetShortComponent(component, false, false);
            }
        }

        //Debug.Log("SIMULATE END");
    }

    private void FindCircuit(List<CircuitComponent> circuit, List<SpiceSharp.Circuits.Entity> entities, List<CircuitComponent> components,
        CircuitComponent component, Point currPosition, int resistors, int gen)
    {
        // If the component is not closed (for example, an open switch), the circuit is broken
        if (component.ComponentType == CircuitComponentType.Switch)
        {
            var script = component.GameObject.GetComponent<ICircuitComponent>();
            if (script != null)
            {
                if (!script.IsClosed())
                {
                    return;
                }
            }
        }

        // Add the component to the circuit we are building
        circuit.Add(component);
        components.Add(component);

        // Keep track of the number of resistors, so we'll know if we have a short circuit
        if ((component.ComponentType == CircuitComponentType.Bulb) ||
            (component.ComponentType == CircuitComponentType.Motor))
        {
            resistors++;
        }

        // Find out the end point of this component. Note that many components allow current to flow in
        // both directions, so we usually need to just find out which is the *other* end.
        Point position = circuit[0].Start;
        Point nextPosition = component.End;
        if (nextPosition.y == currPosition.y && nextPosition.x == currPosition.x)
        {
            nextPosition = component.Start;
        }

        // Get all components at the new end point
        Peg peg = board.GetPeg(nextPosition);
        //Debug.Log("Got all components at the next position. (" + peg.Components.Count + ")");

        foreach (CircuitComponent nextComponent in peg.Components)
        {
            // Ignore the component we just added
            if (nextComponent == component)
            {
                continue;
            }

            //Debug.Log("Examining next component...");

            // Check if we found the battery again. If so, make sure it's the negative terminal.
            //Debug.Log("--- NEXT COMPONENT (" + nextComponent.GameObject.name + ") TYPE = " + nextComponent.ComponentType);
            //Debug.Log("--- NEXT POSITION = " + nextPosition.x + "," + nextPosition.y);
            //Debug.Log("--- NEXT COMPONENT START = " + nextComponent.Start.x + "," + nextComponent.Start.y);
            //Debug.Log("--- circuit[0] (" + circuit[0].GameObject.name + ") type = " + circuit[0].ComponentType);
            if ((nextComponent == circuit[0]) &&
                (nextPosition.x == nextComponent.Start.x) &&
                (nextPosition.y == nextComponent.Start.y))
            {
                // A circuit has been completed! 

                // If this is a short circuit (a circuit with no resistors), flag the components in the short and bail
                if (resistors == 0)
                {
                    // Mark each of the components involved in the short circuit
                    foreach (CircuitComponent shortComponent in circuit)
                    {
                        // Figure out what direction the current is flowing in for this component
                        bool forward = (position.y == shortComponent.Start.y) && (position.x == shortComponent.Start.x);

                        shortComponent.Generation = gen;
                        shortComponent.ShortCircuitGeneration = gen;
                        shortComponent.ShortCircuitForward = forward;

                        position = forward ? shortComponent.End : shortComponent.Start;
                    }
                    break;
                }

                // Activate all components in this circuit
                foreach (CircuitComponent activeComponent in circuit)
                {
                    // Figure out what direction the current is flowing in for this component
                    bool forward = (position.y == activeComponent.Start.y) && (position.x == activeComponent.Start.x);

                    // Add this component to our circuit diagram, taking care to note which direction it's facing
                    if (activeComponent.Generation != gen)
                    {
                        AddSpiceSharpEntity(entities, activeComponent, forward);
                    }

                    // Set the generation number of this component so we can tell that it's been activated
                    // in this generation of circuit simulation
                    activeComponent.Generation = gen;

                    var script = activeComponent.GameObject.GetComponent<ICircuitComponent>();
                    if (script != null)
                    {
                        // Activate this component
                        script.SetActive(true, forward);
                    }

                    position = forward ? activeComponent.End : activeComponent.Start;
                }
            }
            else
            {
                // Check if we are connecting to a previously connected component
                foreach (CircuitComponent previousComponent in circuit)
                {
                    if (previousComponent == nextComponent)
                    {
                        // Loop detected! Deactivate all components and bail out.
                        Debug.Log("LOOP detected! Deactivating components...");
                        foreach (CircuitComponent loopComponent in circuit)
                        {
                            var script = loopComponent.GameObject.GetComponent<ICircuitComponent>();
                            if (script != null)
                            {
                                // XXXDPS - How to we keep from deactivating valid paths?
                                //script.SetActive(false, true);
                            }
                        }

                        circuit.Remove(component);
                        return;
                    }
                }

                // A circuit has not yet been found, so go ahead and add the next component to our 
                // circuit and continue searching...
                //Debug.Log("Looks good. Adding to circuit and recursing...");
                FindCircuit(circuit, entities, components, nextComponent, nextPosition, resistors, gen);
            }
        }

        // Pop off the last component and return
        //Debug.Log("Recursion done. Leaving...");
        circuit.Remove(component);
    }

    void AddSpiceSharpEntity(List<SpiceSharp.Circuits.Entity> entities, CircuitComponent component, bool forward)
    {
        string name = component.GameObject.name;
        string start = forward ? component.Start.ToString() : component.End.ToString();
        string end = forward ? component.End.ToString() : component.Start.ToString();
        string mid = name;

        // Add the appropriate SpiceSharp component. Each component will also have a 0 voltage source
        // added next to it to act as an ammeter.
        switch (component.ComponentType)
        {
            case CircuitComponentType.Battery:
                entities.Add(new VoltageSource("V" + name, mid, end, 0f));
                entities.Add(new VoltageSource(name, mid, start, 10f));

                // Add a lossless wire to the battery. SpiceSharp isn't happy if the circuit doesn't have at least
                // one line, so this makes sure that even circuits built entirely of resistors simulate properly.
                entities.Add(new LosslessTransmissionLine("L" + name, end, name + "a", name + "a", end));
                break;
            case CircuitComponentType.Bulb:
                // Treat bulbs as simple resistors
                entities.Add(new VoltageSource("V" + name, start, mid, 0f));
                entities.Add(new Resistor(name, mid, end, 1000f));
                break;
            case CircuitComponentType.Motor:
                // Treat motors as simple resistors
                entities.Add(new VoltageSource("V" + name, start, mid, 0f));
                entities.Add(new Resistor(name, mid, end, 2000f));
                break;
            case CircuitComponentType.Wire:
            case CircuitComponentType.LongWire:
            case CircuitComponentType.Switch:
            default:
                // Treat wires and switches as lossless, but insert a 0 voltage source in the middle to
                // act as an ammeter.
                entities.Add(new LosslessTransmissionLine(name, start, mid + "a", mid + "a", start));
                entities.Add(new VoltageSource("V" + name, mid + "a", mid + "b", 0f));
                entities.Add(new LosslessTransmissionLine(name + "b", mid + "b", end, end, mid + "b"));
                break;
        }
    }

    SpiceSharp.Circuits.Entity CreateSpiceSharpMeter(CircuitComponent component, Point start, Point end)
    {
        string name = component.GameObject.name + end.ToString();

        return new VoltageSource(name, end.ToString(), start.ToString(), 1f);
    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        StartCoroutine(FadeOut(source, fadeDelay, fadeTime));

        source.Stop();
        source.Play();
    }

    public static IEnumerator FadeOut(AudioSource source, float delay, float time)
    {
        yield return new WaitForSeconds(delay);

        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / time;

            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

}