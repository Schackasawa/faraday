using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Switch : MonoBehaviour, ICircuitComponent
{
    public GameObject circuitLab;
    public GameObject pivot;
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    bool isHeld = false;
    bool isPlaced = false;
    Point startingPeg;
    Direction direction;
    bool isActive = false;
    bool isShortCircuit = false;
    bool isClosed = false;
    double voltage = 0f;
    double current = 0f;

    void Update ()
    {
        // Show/hide the labels
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            bool showLabels = script.showLabels && isActive && !isShortCircuit;
            labelVoltage.gameObject.SetActive(showLabels);
            labelCurrent.gameObject.SetActive(showLabels);
        }
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

    public void Place(Point start, Direction dir)
    {
        isPlaced = true;
        startingPeg = start;
        direction = dir;
    }

    public void SetActive(bool active, bool forward)
    {
        isActive = active;

        // Make sure label is right side up
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        switch (direction)
        {
            case Direction.North:
            case Direction.East:
                rotationVoltage.z = rotationCurrent.z = -90f;
                positionVoltage.x = -0.022f;
                positionCurrent.x = 0.022f;
                break;
            case Direction.South:
            case Direction.West:
                rotationVoltage.z = rotationCurrent.z = 90f;
                positionVoltage.x = 0.022f;
                positionCurrent.x = -0.022f;
                break;
            default:
                Debug.Log("Unrecognized direction!");
                break;
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
        labelVoltageText.text = voltage.ToString("0.##") + "V";
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
            labelCurrentText.text = (current * 1000f).ToString("0.##") + "mA";
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
