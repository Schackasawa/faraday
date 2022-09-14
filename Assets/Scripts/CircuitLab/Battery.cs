using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Battery : CircuitComponent, IBattery
{
    // Public members set in Unity Object Inspector
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;

    public float BatteryVoltage { get; private set; }

    public Battery()
    {
        BatteryVoltage = 10f;
    }

    protected override void Start()
    {
        // Set voltage label text
        labelVoltageText.text = BatteryVoltage.ToString("0.#") + "V";
    }

    protected override void Update()
    {
        // Show/hide the labels
        labelVoltage.gameObject.SetActive(IsActive && Lab.showLabels);
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
}
