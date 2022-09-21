using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Motor : CircuitComponent, IResistor
{
    // Public members set in Unity Object Inspector
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    float speed = 0f;
    float baseCurrent = 0.005f;
    float normalSpeed = 600f;

    public float Resistance { get; private set; }

    public Motor()
    { 
        Resistance = 2000f;
    }

    protected override void Update () 
    {
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
        labelResistance.gameObject.SetActive(IsActive && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // Set resistance label text
        labelResistanceText.text = Resistance.ToString("0.#") + "Ω";

        // Make sure labels are right side up
        RotateLabel(labelResistance, LabelAlignment.Top);
        RotateLabel(labelCurrent, LabelAlignment.Bottom);
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (!IsCurrentSignificant())
        {
            IsActive = false;

            // Hide the labels
            labelResistance.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrentText.text = (current * 1000f).ToString("0.#") + "mA";

            // Update motor speed
            speed = normalSpeed * ((float)current / baseCurrent);
        }
    }


}
