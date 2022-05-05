using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Battery : MonoBehaviour, ICircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;

    bool isPlaced = false;
    bool isHeld = false;
    bool isActive = false;
    Point startingPeg;
    Direction direction;

    void Start()
    {
        // Set voltage label text
        labelVoltageText.text = CircuitLab.BatteryVoltage.ToString("0.##") + "V";
    }

    void Update()
    {
        // Show/hide the labels
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            labelVoltage.gameObject.SetActive(isActive && script.showLabels);
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
        switch (direction)
        {
            case Direction.North:
                rotationVoltage.z = 180f;
                break;
            case Direction.South:
                rotationVoltage.z = 0f;
                break;
            case Direction.East:
                rotationVoltage.z = -90f;
                break;
            case Direction.West:
                rotationVoltage.z = 90f;
                break;
            default:
                Debug.Log("Unrecognized direction!");
                break;
        }

        // Apply label positioning
        labelVoltage.transform.localEulerAngles = rotationVoltage;
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
