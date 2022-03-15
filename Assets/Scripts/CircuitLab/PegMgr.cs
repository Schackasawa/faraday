using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegMgr : MonoBehaviour {

    public AudioSource clickSound;
    public float clickStartTime = 0f;

    public float pegInterval = 0.1f;

    float wirePositionOffset = -1.64f;
    float longPositionOffset = -3.34f;

    bool isOccupied = false;
    GameObject clone = null;
    GameObject original = null;


	void Start () {
        clickSound.GetComponent<AudioSource>();
    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    void Update () {
        // If we have an original object referenced, and that object has been dropped, center it on this peg
        if (original)
        {
            var rigidBody = original.GetComponent<Rigidbody>();
            if (!rigidBody.isKinematic)
            {
                // Component has been dropped

                // Lock it to the board
                original.transform.parent = transform;
                original.GetComponent<Rigidbody>().isKinematic = true;
                original.GetComponent<Rigidbody>().useGravity = false;

                // Force this object to be removed from the grabbers' grabbable lists before we
                // disable the colliders, since they won't get an OnTriggerExit call to do it...
                /*
                var hand = GameObject.Find("AvatarGrabberRight").gameObject;
                var grabber = hand.GetComponent<OVRGrabber>();
                if (grabber != null)
                {
                    grabber.ForceRemoveGrabbable(original.GetComponent<OVRGrabbable>());
                }
                hand = GameObject.Find("AvatarGrabberLeft").gameObject;
                grabber = hand.GetComponent<OVRGrabber>();
                if (grabber != null)
                {
                    grabber.ForceRemoveGrabbable(original.GetComponent<OVRGrabbable>());
                }
                */

                // Disable box and sphere colliders so they don't interfere with the board and other components. 
                // We still have a small capsule collider on each component so they can be picked up from the middle.
                original.GetComponent<BoxCollider>().enabled = false;
                original.GetComponent<SphereCollider>().enabled = false;

                // Lock rotation to the nearest 90 degrees
                Point end = LockRotation(original, original);

                // Discard the placeholder
                DestroyClone();

                // Play the click sound
                StartCoroutine(PlaySound(clickSound, clickStartTime));

                var lab = GameObject.Find("CircuitLab").gameObject;
                var script = lab.GetComponent<ICircuitLab>();
                if (script != null)
                {
                    Point start = GetCoordinates();

                    // Add the component to our breadboard. This will also trigger a circuit simulation to
                    // activate any newly completed circuits.
                    script.AddComponent(original, start, end);
                }

                original = null;
            }
        }

        // If we have a clone, make sure the rotation changes when the user twists the component
        if (clone)
        {
            LockRotation(clone, original);
        }
    }

    Point GetCoordinates()
    {
        string name = transform.name.Substring(4);

        int row = int.Parse(name.Substring(0, name.IndexOf('_')));
        int col = int.Parse(name.Substring(name.IndexOf('_') + 1));

        return new Point(col, row);
    }

    Point LockRotation(GameObject target, GameObject reference)
    {
        float offset = wirePositionOffset;
        int pegOffset = 1;
        if (reference.transform.name.Contains("LongWire"))
        {
            offset = longPositionOffset;
            pegOffset = 2;
        }

        // Find the locations of all 4 neighboring pegs
        Point coords = GetCoordinates();
        Point end = new Point(coords.x, coords.y);
        //Debug.Log("row: " + coords.y + ", col: " + coords.x);

        string north = "Peg_" + (coords.y + pegOffset) + "_" + coords.x;
        Point ptNorth = new Point(coords.x, coords.y + pegOffset);
        string south = "Peg_" + (coords.y - pegOffset) + "_" + coords.x;
        Point ptSouth = new Point(coords.x, coords.y - pegOffset);
        string east = "Peg_" + coords.y + "_" + (coords.x + pegOffset);
        Point ptEast = new Point(coords.x + pegOffset, coords.y);
        string west = "Peg_" + coords.y + "_" + (coords.x - pegOffset);
        Point ptWest = new Point(coords.x - pegOffset, coords.y);

        // Find out if any of these are blocked and should be ignored
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = lab.GetComponent<ICircuitLab>();
        Point start = GetCoordinates();
        List<string> freeNeighbors = new List<string>();
        Point[] neighbors = { ptNorth, ptSouth, ptEast, ptWest };
        string[] neighborNames = { north, south, east, west };
        for (int i = 0; i < 4; i++)
        {
            if (script.IsSlotFree(start, neighbors[i], pegOffset))
            {
                freeNeighbors.Add(neighborNames[i]);
            }
        }

        // Highlight each neighbor
        //HighlightNeighbor(north, Color.red);
        //HighlightNeighbor(east, Color.yellow);
        //HighlightNeighbor(south, Color.green);
        //HighlightNeighbor(west, Color.blue);

        // Figure out which one is closest to the 2nd wire endpoint
        string closest = GetClosestNeighbor(reference, freeNeighbors);

        // Lock the clone so that it points to that endpoint
        var rotation = target.transform.localEulerAngles;
        var position = target.transform.localPosition;
        rotation.x = 0;
        rotation.y = 0;
        position.z = 0;
        if (closest == north)
        {
            rotation.z = 0;

            position.y = offset;
            position.x = 0;

            end.y += pegOffset;
        }
        else if (closest == east)
        {
            rotation.z = 90;

            position.y = 0;
            position.x = -offset;

            end.x += pegOffset;
        }
        else if (closest == south)
        {
            rotation.z = 180;

            position.y = -offset;
            position.x = 0;

            end.y -= pegOffset;
        }
        else
        {
            rotation.z = 270;

            position.y = 0;
            position.x = offset;

            end.x -= pegOffset;
        }
        target.transform.localEulerAngles = rotation;
        target.transform.localPosition = position;

        return end;
    }

    void HighlightNeighbor(string name, Color color)
    {
        var neighbor = GameObject.Find(name);
        if (neighbor)
        {
            DrawLine(transform.position, neighbor.transform.position, color);
        }
    }

    string GetClosestNeighbor(GameObject clone, List<string> names)
    {
        string closest = names[0];
        GameObject closestNeighbor = null;
        float min = 999;
        var endpoint = clone.transform.Find("WireEnd2");

        foreach (string name in names)
        {
            GameObject neighbor = GameObject.Find(name);
            if (neighbor)
            {
                float nextDistance = Vector3.Distance(neighbor.transform.position, endpoint.transform.position);
                if (nextDistance < min)
                {
                    min = nextDistance;
                    closest = name;
                    closestNeighbor = neighbor;
                }
            }
        }

        //DrawLine(endpoint.transform.position, closestNeighbor.transform.position, Color.red);
        return closest;
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Only deal with sphere colliders
        if (other.GetType() != typeof(SphereCollider))
        {
            return;
        }

        if (!isOccupied && other.name.StartsWith("Component") && (other.gameObject.transform.parent == null))
        {
            int componentLength = 1;
            if (other.transform.name.Contains("LongWire"))
            {
                componentLength = 2;
            }

            // Figure out if this peg has any free slots
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            if (script == null)
            {
                return;
            }
            Point start = GetCoordinates();
            int freeSlots = script.GetFreeComponentSlots(start, componentLength);

            // If not, ignore this trigger enter event
            if (freeSlots == 0)
            {
                return;
            }

            // Remember which object we are cloning
            original = other.gameObject;

            // Create a clone of the object
            clone = Instantiate(other.gameObject);
            clone.GetComponent<Rigidbody>().detectCollisions = false;

            // Make the clone transparent
            //var rend = clone.gameObject.GetComponent<Renderer>();
            //var color = rend.material.color;
            //rend.material.color = new Color(color.r, color.g, color.b, 0.5f);

            // Lock the clone in place
            clone.gameObject.GetComponent<Rigidbody>().useGravity = false;
            clone.gameObject.GetComponent<Rigidbody>().isKinematic = false;

            // Place the clone on the peg
            clone.transform.parent = transform;
            isOccupied = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Only deal with sphere colliders
        if (other.GetType() != typeof(SphereCollider))
        {
            return;
        }

        if (other.name.StartsWith("Component"))
        {
            // Forget about the original for now
            if (original) {
                original.transform.parent = null;
                original = null;
            }

            // Delete the clone on the peg
            DestroyClone();
        }
    }

    void DestroyClone()
    {
        if (isOccupied)
        {
            Destroy(clone);
            clone = null;
            isOccupied = false;
        }
    }
}
