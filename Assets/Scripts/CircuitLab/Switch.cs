using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Switch : CircuitComponent
{
    // Public members set in Unity Object Inspector
    public GameObject pivot;
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public AudioSource switchSound;

    public Switch() : base(CircuitComponentType.Switch)
    {
        // Switches start off in the open position
        IsClosed = false;
    }

    protected override void Update ()
    {
        bool sufficientCurrent = (Current > 0.0000001);

        // Show/hide the labels
        bool showLabels = Lab.showLabels && IsActive && sufficientCurrent && !IsShortCircuit;
        labelVoltage.gameObject.SetActive(showLabels);
        labelCurrent.gameObject.SetActive(showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // Make sure label is right side up
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        switch (Direction)
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

    public override void SetShortCircuit(bool isShortCircuit, bool isForward)
    {
        IsShortCircuit = isShortCircuit;
    }

    public override void Toggle()
    {
        IsClosed = !IsClosed;

        // Set the blade to the proper position by rotating the pivot
        var rotation = pivot.transform.localEulerAngles;
        rotation.z = IsClosed ? 0 : -45f;
        pivot.transform.localEulerAngles = rotation;

        StartCoroutine(PlaySound(switchSound, 0f));

        // Trigger a new simulation since we may have just closed or opened a circuit
        Lab.SimulateCircuit();
    }

    public override void SetVoltage(double voltage)
    {
        Voltage = voltage;

        // Update label text
        labelVoltageText.text = voltage.ToString("0.#") + "V";
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // Update label text
        labelCurrentText.text = (current * 1000f).ToString("0.#") + "mA";
    }
}
