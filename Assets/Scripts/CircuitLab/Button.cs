﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : CircuitComponent, IConductor
{
    // Public members set in Unity Object Inspector
    public float normalCircuitSpeed = 0.16f;
    public float shortCircuitSpeed = 0.40f;
    public Vector3 startPosition = new Vector3(0, 0.04f, 0);
    public Vector3 endPosition = new Vector3(0, -0.04f, 0);
    public GameObject button;
    public Color normalEmissionColor;
    public Color shortEmissionColor;
    public float emissionIntensity;
    public AudioSource buttonPressAudio;

    Color32 normalBaseColor = new Color32(56, 206, 76, 255);
    Color32 shortBaseColor = new Color32(88, 18, 18, 255);
    float baseCurrent = 0.005f;
    float speed = 0f;

    public Button()
    {
        // Switches start off in the open position
        IsClosed = false;
    }

    protected override void Update ()
    {
        if (!IsClone)
        {
            // Figure out what direction the current is flowing and set the start/end positions accordingly
            Vector3 start = IsForward ? startPosition : endPosition;
            Vector3 end = IsForward ? endPosition : startPosition;

            // Move all electrons the same amount
            float step = speed * Time.deltaTime;
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Electron"))
                {
                    float remaining = Mathf.Abs(end.y - child.localPosition.y);
                    bool wrapping = (remaining <= step);
                    child.localPosition = Vector3.MoveTowards(child.localPosition, end, step);

                    // Wrap around to the beginning, adding any remainder
                    if (wrapping)
                    {
                        remaining = step - remaining;
                        if (end.y < start.y)
                        {
                            remaining = -remaining;
                        }
                        child.localPosition = new Vector3(start.x, start.y + remaining, start.z);

                        // Activate/Deactivate electrons when they hit the wrapping point
                        if (IsActive != child.gameObject.activeSelf)
                        {
                            child.gameObject.SetActive(IsActive);
                        }
                    }
                }
            }
        }
    }

    private void DeactivateElectrons()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Electron"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void SetElectronColor(Color emissionColor, Color32 baseColor)
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Electron"))
            {
                Renderer renderer = child.GetComponent<Renderer>();
                Material mat = renderer.material;
                mat.SetColor("_EmissionColor", emissionColor * Mathf.Pow(2, emissionIntensity));
                mat.SetColor("_BaseColor", baseColor);
            }
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
        if (!isActive)
            return;

        IsForward = isForward;
        speed = normalCircuitSpeed;

        // Change electrons to green
        SetElectronColor(normalEmissionColor, normalBaseColor);
    }

    public override void SetShortCircuit(bool isShortCircuit, bool isForward)
    {
        IsShortCircuit = isShortCircuit;
        if (isShortCircuit)
        {
            IsActive = true;
            IsForward = isForward;
            speed = shortCircuitSpeed;

            // Change electrons to red
            SetElectronColor(shortEmissionColor, shortBaseColor);
        }
    }

    public override void Toggle()
    {
        // The button shouldn't be functional until it is placed on the breadboard
        if (!IsPlaced)
            return;

        IsClosed = !IsClosed;

        // Lower the button when pressed
        var position = button.transform.localPosition;
        position.z = IsClosed ? 0.003f : 0.006f;
        button.transform.localPosition = position;

        // Click when the button is first pressed
        if (IsClosed)
            StartCoroutine(PlaySound(buttonPressAudio, 0f));

        // Trigger a new simulation since we may have just closed or opened a circuit
        Lab.SimulateCircuit();
    }

    public override void SetVoltage(double voltage)
    {
        Voltage = voltage;
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (!IsCurrentSignificant())
        {
            IsActive = false;
            DeactivateElectrons();
        }
        else
        {
            // Update electron speed
            speed = normalCircuitSpeed * ((float)current / baseCurrent);
        }
    }
}

