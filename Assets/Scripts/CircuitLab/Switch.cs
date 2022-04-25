using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour, ICircuitComponent {

    public GameObject pivot;
    public GameObject labelVoltage;
    public GameObject labelCurrent;

    private bool isHeld = false;
    private bool isPlaced = false;
    private Point startingPeg;
    bool isShortCircuit = false;
    bool isClosed = false;
    double voltage = 0f;
    double current = 0f;

    void Update () {

    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public bool IsPlaced()
    {
        return isPlaced;
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
        // Show/hide the labels
        labelVoltage.gameObject.SetActive(active);
        labelCurrent.gameObject.SetActive(active);

        // Make sure label is right side up
        var componentRotation = transform.localEulerAngles;
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        if (componentRotation.z == 0f)
        {
            rotationVoltage.z = rotationCurrent.z = -90f;
            positionVoltage.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 90f)
        {
            rotationVoltage.z = rotationCurrent.z = -90f;
            positionVoltage.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 180f)
        {
            rotationVoltage.z = rotationCurrent.z = 90f;
            positionVoltage.x = 0.01f;
            positionCurrent.x = -0.025f;
        }
        else
        {
            rotationVoltage.z = rotationCurrent.z = 90f;
            positionVoltage.x = 0.01f;
            positionCurrent.x = -0.025f;
        }

        // Apply label positioning
        labelVoltage.transform.localEulerAngles = rotationVoltage;
        labelVoltage.transform.localPosition = positionVoltage;
        labelCurrent.transform.localEulerAngles = rotationCurrent;
        labelCurrent.transform.localPosition = positionCurrent;
    }

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {
        isShortCircuit = shortCircuit;
        if (shortCircuit)
        {
            // Hide the labels
            labelVoltage.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
    }

    public bool IsClosed()
    {
        return isClosed;
    }

    public void Toggle()
    {
        isClosed = !isClosed;

        // Set the blade to the proper position by rotating the pivot
        var rotation = pivot.transform.localEulerAngles;
        rotation.z = isClosed ? 0 : -45f;
        pivot.transform.localEulerAngles = rotation;

        // Trigger a new simulation since we may have just closed or opened a circuit
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = lab.GetComponent<ICircuitLab>();
        if (script != null)
        {
            script.SimulateCircuit();
        }
    }

    public void SetVoltage(double newVoltage)
    {
        voltage = newVoltage;

        // Update label text
        labelVoltage.GetComponent<TextMesh>().text = voltage.ToString("0.##") + "V";
    }

    public void SetCurrent(double newCurrent)
    {
        current = newCurrent;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (newCurrent <= 0.0000001)
        {
            // Hide the labels
            labelVoltage.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrent.GetComponent<TextMesh>().text = (current * 1000f).ToString("0.##") + "mA";
        }
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
