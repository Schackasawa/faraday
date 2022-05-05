using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Motor : MonoBehaviour, ICircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    bool isPlaced = false;
    bool isHeld = false;
    Point startingPeg;
    Direction direction;
    float speed = 0f;
    float baseCurrent = 0.005f;
    bool isActive = false;
    double voltage = 0f;
    double current = 0f;
    float normalSpeed = 600f;

    void Update () {

        if (isActive)
        {
            float step = speed * Time.deltaTime;
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Motor"))
                {
                    // Rotate the motor at a fixed speed
                    child.transform.Rotate(Vector3.forward, step);
                }
            }
        }

        // Show/hide the labels
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            labelResistance.gameObject.SetActive(isActive && script.showLabels);
            labelCurrent.gameObject.SetActive(isActive && script.showLabels);
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

        // Set resistance label text
        labelResistanceText.text = CircuitLab.MotorResistance.ToString("0.##") + "Ω";

        // Make sure label is right side up
        var rotationResistance = labelResistance.transform.localEulerAngles;
        var positionResistance = labelResistance.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        switch (direction)
        {
            case Direction.North:
            case Direction.East:
                rotationResistance.z = rotationCurrent.z = -90f;
                positionResistance.x = -0.022f;
                positionCurrent.x = 0.022f;
                break;
            case Direction.South:
            case Direction.West:
                rotationResistance.z = rotationCurrent.z = 90f;
                positionResistance.x = 0.022f;
                positionCurrent.x = -0.022f;
                break;
            default:
                Debug.Log("Unrecognized direction!");
                break;
        }

        // Apply label positioning
        labelResistance.transform.localEulerAngles = rotationResistance;
        labelResistance.transform.localPosition = positionResistance;
        labelCurrent.transform.localEulerAngles = rotationCurrent;
        labelCurrent.transform.localPosition = positionCurrent;
    }

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {
        // Not possible, since motors have resistance
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
        voltage = newVoltage;
    }

    public void SetCurrent(double newCurrent)
    {
        current = newCurrent;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (newCurrent <= 0.0000001)
        {
            isActive = false;

            // Hide the labels
            labelResistance.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrentText.text = (current * 1000f).ToString("0.##") + "mA";

            // Update motor speed
            speed = normalSpeed * ((float)current / baseCurrent);
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
