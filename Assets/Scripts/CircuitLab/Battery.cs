using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Battery : CircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;

    public Battery() : base(CircuitComponentType.Battery) { }

    protected override void Start()
    {
        // Set voltage label text
        labelVoltageText.text = CircuitLab.BatteryVoltage.ToString("0.##") + "V";
    }

    protected override void Update()
    {
        // Show/hide the labels
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            labelVoltage.gameObject.SetActive(IsActive && script.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // Make sure label is right side up
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        switch (Direction)
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

    public void SelectEntered()
    {
        IsHeld = true;

        // Enable box and sphere colliders so this piece can be placed somewhere else on the board.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (IsPlaced)
        {
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
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
