using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using UnityEngine.XR.Interaction.Toolkit;

public class CircuitLab : MonoBehaviour, ICircuitLab
{
    // Public members set in Unity Object Inspector
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

    // Dynamic components that may change state on their own. Keeping a list of these allows us to
    // avoid redundant Simulate() calls when many components are changing state around the same time.
    List<IDynamic> dynamicComponents = new List<IDynamic>();
    int numActiveCircuits = 0;

    void Start()
    {
        // Record the initial height of the handle so we can move the whole board when the handle moves
        yHandleStart = handle.transform.position.y;
        yTableStart = transform.localPosition.y;

        // Create a new board object to hold all breadboard state
        board = new Board(numRows, numCols);

        // Create peg objects at regular intervals across the board
        CreatePegs();

        PreloadSimulator();
    }

    void PreloadSimulator()
    {
        // Build a simple circuit and run an analysis to preload the SpiceSharp simulator. 
        // This avoids a multi-second lag when connecting our first circuit on the breadboard.
        var ckt = new Circuit(
            new VoltageSource("V1", "in", "0", 1.0),
            new Resistor("R1", "in", "out", 1.0e4),
            new Resistor("R2", "out", "0", 2.0e4)
            );
        var dc = new OP("DC 1");
        dc.Run(ckt);
    }

    void Update()
    {
        // Get the handle's current y position and move the entire circuit lab to match
        float yHandleCurrent = handle.transform.position.y;
        transform.localPosition = new Vector3(transform.localPosition.x, yTableStart + (yHandleCurrent - yHandleStart), transform.localPosition.z);

        // Update all dynamic components
        if (dynamicComponents.Count > 0)
        {
            bool simulate = false;
            foreach (IDynamic component in dynamicComponents)
            {
                if (component.UpdateState(numActiveCircuits))
                    simulate = true;
            }

            // If any of the dynamic components requested a new simulation, trigger it once
            if (simulate)
                SimulateCircuit();
        }
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

        CircuitComponent cp = component.GetComponent<CircuitComponent>();
        if (cp == null)
        {
            Debug.Log($"ERROR: Component Type not found in {name}!");
            return;
        }

        // Create a component object and link it to the starting and ending pegs
        PlacedComponent newComponent = new PlacedComponent(component, start, end);
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
            PlacedComponent found = pegA.Components.Find(x => x.GameObject == component);
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
        var script = component.GetComponent<CircuitComponent>();
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
        foreach (PlacedComponent component in pegStart.Components)
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
                foreach (PlacedComponent component in pegB.Components)
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

    public void RegisterDynamicComponent(IDynamic component)
    {
        dynamicComponents.Add(component);
    }

    public void UnregisterDynamicComponent(IDynamic component)
    {
        dynamicComponents.Remove(component);
    }

    public void SimulateCircuit()
    {
        //Debug.Log("SIMULATE START");
        numActiveCircuits = 0;

        // Bump the generation number for this circuit simulation
        int gen = ++board.Generation;

        // First, search for all batteries (and solar panels) so we know where to start
        List<PlacedComponent> batteries = new List<PlacedComponent>();
        foreach (PlacedComponent component in board.Components)
        {
            if (component.Component is IBattery || component.Component is ISolar)
            {
                batteries.Add(component);
            }
        }

        foreach (PlacedComponent battery in batteries)
        {
            // Skip batteries that have already been included in a circuit
            if (battery.Generation == gen)
            {
                continue;
            }

            List<PlacedComponent> circuit = new List<PlacedComponent>();
            List<PlacedComponent> components = new List<PlacedComponent>();
            List<SpiceSharp.Entities.Entity> entities = new List<SpiceSharp.Entities.Entity>();

            // Add the battery as the first component
            circuit.Add(battery);
            components.Add(battery);

            int resistors = 0;

            // Leaving the positive terminal of the battery (to follow current flow), do a depth-first 
            // search to find each circuit that eventually leads to the negative terminal of the battery
            Point currPosition = battery.End;
            Peg peg = board.GetPeg(currPosition);

            foreach (PlacedComponent component in peg.Components)
            {
                if (component != battery)
                {
                    FindCircuit(circuit, entities, components, component, currPosition, resistors, gen);
                }
            }

            // Case 1: There's a short circuit on this battery
            if (battery.ShortCircuitGeneration == gen)
            {
                foreach (PlacedComponent component in components)
                {
                    // Highlight each component involved in the short circuit, and deactivate all 
                    // other connected components.
                    if (component.ShortCircuitGeneration == gen)
                    {
                        component.Component.SetShortCircuit(true, component.ShortCircuitForward);
                    }
                    else
                    {
                        component.Component.SetActive(false, false);
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
                numActiveCircuits++;
                var ssCircuit = new Circuit(entities);

                // Create an Operating Point Analysis for the circuit
                var op = new OP("DC 1");

                // Create exports so we can access component properties
                foreach (PlacedComponent component in components)
                {
                    bool isBattery = (component.Component is IBattery);

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

                    // Loop through the components and find the lowest voltage, so we can normalize the entire
                    // circuit to start at 0V.
                    double minVoltage = 0f;
                    foreach (PlacedComponent component in components)
                    {
                        if (component.Generation == gen)
                        {
                            if (component.VoltageExport.Value < minVoltage)
                                minVoltage = component.VoltageExport.Value;
                        }
                    }

                    // Now loop through again and tell each component what its voltage and current values are
                    foreach (PlacedComponent component in components)
                    {
                        if (component.Generation == gen)
                        {
                            // Update the voltage value
                            var voltage = component.VoltageExport.Value - minVoltage;
                            component.Component.SetVoltage(voltage);

                            // Update the current value
                            var current = component.CurrentExport.Value;
                            component.Component.SetCurrent(current);
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
                catch (ValidationFailedException exception)
                {
                    Debug.Log("Simulation Error! Caught exception: " + exception);
                    foreach (var rule in exception.Rules)
                    {
                        Debug.Log("  Rule: " + rule);
                        Debug.Log("  ViolationCount: " + rule.ViolationCount);
                        foreach (var violation in rule.Violations)
                        {
                            Debug.Log("    Violation: " + violation);
                            Debug.Log("    Subject: " + violation.Subject);
                        }
                    }
                    Debug.Log("Inner exception: " + exception.InnerException);

                    // Treat a simulation error as a short circuit
                    battery.ActiveShort = true;
                    battery.ActiveCircuit = false;
                    StartCoroutine(PlaySound(shortSound1, shortSound1StartTime));
                    StartCoroutine(PlaySound(shortSound2, shortSound2StartTime));

                    // Deactivate all components in this circuit
                    foreach (PlacedComponent component in components)
                    {
                        component.Component.SetActive(false, false);
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
        foreach (PlacedComponent component in board.Components)
        {
            if (component.Generation != gen)
            {
                component.Component.SetActive(false, false);
            }
        }

        // Clear any components that didn't get included in any short circuits this time
        foreach (PlacedComponent component in board.Components)
        {
            if (component.ShortCircuitGeneration != gen)
            {
                component.Component.SetShortCircuit(false, false);
            }
        }

        //Debug.Log("SIMULATE END");
    }

    private void FindCircuit(List<PlacedComponent> circuit, List<SpiceSharp.Entities.Entity> entities, List<PlacedComponent> components,
        PlacedComponent component, Point currPosition, int resistors, int gen)
    {
        // If the component is not closed (for example, an open switch), the circuit is broken
        var script = component.GameObject.GetComponent<CircuitComponent>();
        if (script == null || !script.IsClosed)
        {
            return;
        }

        // Add the component to the circuit we are building
        circuit.Add(component);
        components.Add(component);

        // Keep track of the number of resistors, so we'll know if we have a short circuit
        if (component.Component is IResistor)
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

        foreach (PlacedComponent nextComponent in peg.Components)
        {
            // Ignore the component we just added
            if (nextComponent == component)
            {
                continue;
            }

            // Check if we found the battery again. If so, make sure it's the negative terminal.
            //Debug.Log("--- NEXT COMPONENT (" + nextComponent.GameObject.name + ")" );
            //Debug.Log("--- NEXT POSITION = " + nextPosition.x + "," + nextPosition.y);
            //Debug.Log("--- NEXT COMPONENT START = " + nextComponent.Start.x + "," + nextComponent.Start.y);
            //Debug.Log("--- circuit[0] (" + circuit[0].GameObject.name + ")" );
            if ((nextComponent == circuit[0]) &&
                (nextPosition.x == nextComponent.Start.x) &&
                (nextPosition.y == nextComponent.Start.y))
            {
                // A circuit has been completed! 

                // If this is a short circuit (a circuit with no resistors), flag the components in the short and bail
                if (resistors == 0)
                {
                    // Mark each of the components involved in the short circuit
                    foreach (PlacedComponent shortComponent in circuit)
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
                foreach (PlacedComponent activeComponent in circuit)
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

                    // Activate this component
                    script = activeComponent.GameObject.GetComponent<CircuitComponent>();
                    if (script != null)
                    {
                        script.SetActive(true, forward);
                    }

                    position = forward ? activeComponent.End : activeComponent.Start;
                }
            }
            else
            {
                // Check if we are connecting to a previously connected component
                foreach (PlacedComponent previousComponent in circuit)
                {
                    if (previousComponent == nextComponent)
                    {
                        // Loop detected! Deactivate all components and bail out.
                        foreach (PlacedComponent loopComponent in circuit)
                        {
                            // XXX - Ideally, we would deactivate any components in the loop, but how do
                            // we keep from deactivating components that are also part of a valid circuit?
                            script = loopComponent.GameObject.GetComponent<CircuitComponent>();
                            if (script != null)
                            {
                                //script.SetActive(false, true);
                            }
                        }
                        circuit.Remove(component);
                        return;
                    }
                }

                // A circuit has not yet been found, so go ahead and add the next component to our 
                // circuit and continue searching...
                FindCircuit(circuit, entities, components, nextComponent, nextPosition, resistors, gen);
            }
        }

        // Pop off the last component and return
        circuit.Remove(component);
    }

    void AddSpiceSharpEntity(List<SpiceSharp.Entities.Entity> entities, PlacedComponent placedComponent, bool forward)
    {
        string name = placedComponent.GameObject.name;
        string start = forward ? placedComponent.Start.ToString() : placedComponent.End.ToString();
        string end = forward ? placedComponent.End.ToString() : placedComponent.Start.ToString();
        string mid = name;
        CircuitComponent component = placedComponent.Component;

        // Add the appropriate SpiceSharp component. Each component will
        // also have a 0 voltage source added next to it to act as an ammeter.
        if (component is IBattery battery)
        {
            // If this is the first voltage source we are adding, make sure one of 
            // the ends is specified as ground, or "0" Volt point of reference.
            if (entities.Count == 0)
            {
                entities.Add(new VoltageSource("V" + name, start, "0", 0f));
                entities.Add(new VoltageSource(name, "0", end, battery.BatteryVoltage));
            }
            else
            {
                entities.Add(new VoltageSource("V" + name, start, mid, 0f));
                entities.Add(new VoltageSource(name, mid, end, battery.BatteryVoltage));
            }
        }
        else if (component is ISolar solar)
        {
            string mid2 = name + "2";

            // If this is the first voltage source we are adding, make sure one of 
            // the ends is specified as ground, or "0" Volt point of reference.
            if (entities.Count == 0)
            {
                entities.Add(new VoltageSource("V" + name, start, "0", 0f));
                entities.Add(new VoltageSource(name, "0", mid2, solar.SolarVoltage));
            }
            else
            {
                entities.Add(new VoltageSource("V" + name, start, mid, 0f));
                entities.Add(new VoltageSource(name, mid, mid2, solar.SolarVoltage));
            }

            // Also add a resistor to simulate the way current changes with the
            // angle to the sun even though voltage may be the same.
            entities.Add(new Resistor("R" + name, mid2, end, solar.SolarResistance));
        }
        else if (component is IResistor resistor)
        {
            entities.Add(new VoltageSource("V" + name, mid, start, 0f));
            entities.Add(new Resistor(name, mid, end, resistor.Resistance));
        }
        else if (component is IConductor)
        {
            // All normal conductors (wires, switches, etc.) are considered lossless
            entities.Add(new VoltageSource("V" + name, mid, start, 0f));
            entities.Add(new LosslessTransmissionLine(name, mid, end, end, mid));
        }
        else
        {
            Debug.Log("Unrecognized component: " + name);
        }
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