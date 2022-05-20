using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Motor : CircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    float speed = 0f;
    float baseCurrent = 0.005f;
    float normalSpeed = 600f;

    public Motor() : base(CircuitComponentType.Motor) { }

    protected override void Update () {

        if (IsActive)
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
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            labelResistance.gameObject.SetActive(IsActive && script.showLabels);
            labelCurrent.gameObject.SetActive(IsActive && script.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // Set resistance label text
        labelResistanceText.text = CircuitLab.MotorResistance.ToString("0.##") + "Ω";

        // Make sure label is right side up
        var rotationResistance = labelResistance.transform.localEulerAngles;
        var positionResistance = labelResistance.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        switch (Direction)
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

    public override void SetCurrent(double current)
    {
        Current = current;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (current <= 0.0000001)
        {
            IsActive = false;

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
        IsHeld = true;

        // Enable box and sphere colliders so this piece can be placed somewhere else on the board.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (IsPlaced)
        {
            var script = circuitLab.GetComponent<ICircuitLab>();
            script.RemoveComponent(this.gameObject, StartingPeg);

            IsPlaced = false;
        }
    }

    public void SelectExited()
    {
        IsHeld = false;

        // Make sure gravity is enabled any time we release the object
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
    }
}
