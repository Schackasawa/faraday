using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour, ICircuitComponent
{
    bool isPlaced = false;
    bool isHeld = false;
    Point startingPeg;

    void Update()
    {

    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public void SetClone()
    {
    }

    public void Place(Point start)
    {
        isPlaced = true;
        startingPeg = start;
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

    public void SelectEntered()
    {
        isHeld = true;

        // Enable box and sphere colliders so this piece can be placed somewhere else on the board.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (isPlaced)
        {
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            script.RemoveComponent(this.gameObject, startingPeg);
            transform.parent = null;

            isPlaced = false;
        }
    }

    public void SelectExited()
    {
        isHeld = false;

        // Make sure gravity is enabled any time we release the object
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
    }
}
