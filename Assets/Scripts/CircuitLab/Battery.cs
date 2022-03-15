using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour, /*OVRGrabbable, */ICircuitComponent
{
    void Update()
    {

    }

    public void SetActive(bool active, bool forward)
    {

    }

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {

    }

    public bool IsClosed()
    {
        return true;
    }

    public void Toggle()
    {

    }

    public void SetVoltage(double newVoltage)
    {

    }

    public void SetCurrent(double newCurrent)
    {

    }

    Point GetParentCoordinates()
    {
        string name = transform.parent.name.Substring(4);

        int row = int.Parse(name.Substring(0, name.IndexOf('_')));
        int col = int.Parse(name.Substring(name.IndexOf('_') + 1));

        return new Point(col, row);
    }
    /*
    override public void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        if (transform.parent && transform.parent.name.Contains("Peg"))
        {
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<SphereCollider>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = true;

            // Remove this object from the board state
            Point start = GetParentCoordinates();
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            script.RemoveComponent(this.gameObject, start);
            transform.parent = null;
        }

        base.GrabBegin(hand, grabPoint);
    }
    */
}
