using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : CircuitComponent
{
    // Public members set in Unity Object Inspector
    public GameObject pivot;
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject labelTimeout;
    public TMP_Text labelTimeoutText;
    public int timeoutSeconds = 5;
    public int maxTimeoutSeconds = 10;
    public AudioSource switchSound;

    private bool ticking = false;
    private DateTime lastSwitch;
    private int lastTimeoutSeconds;

    public Timer() : base(CircuitComponentType.Timer)
    {
        // Timers start off in the closed position
        IsClosed = true;
        lastTimeoutSeconds = timeoutSeconds;
    }

    protected override void Update ()
    {
        // Show/hide the labels
        bool showLabels = Lab.showLabels && IsActive && !IsShortCircuit;
        labelVoltage.gameObject.SetActive(showLabels);
        labelCurrent.gameObject.SetActive(showLabels);

        // Calculate time remaining until next switch toggle
        int secondsRemaining = ticking ? lastTimeoutSeconds : timeoutSeconds;
        if (ticking)
        {
            TimeSpan elapsedTime = DateTime.Now - lastSwitch;
            secondsRemaining = Math.Clamp((lastTimeoutSeconds - (int)elapsedTime.TotalSeconds), 1, maxTimeoutSeconds);
        }

        // Update time remaining label
        labelTimeoutText.text = secondsRemaining.ToString();
    }

    protected override void Reset()
    {
        // Cancel any outstanding timeouts
        CancelInvoke();

        // Stop ticking and make sure we're in the closed position
        ticking = false;
        IsClosed = true;
        RotatePivot();
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // If we are becoming active for the first time, start our timer 
        // and set a callback to toggle the switch after 'timeoutSeconds'
        if (!ticking && isActive)
        {
            StartTimer();
        }

        // Make sure labels are right side up
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        var rotationTimeout = labelTimeout.transform.localEulerAngles;
        switch (Direction)
        {
            case Direction.North:
            case Direction.East:
                rotationVoltage.z = rotationCurrent.z = rotationTimeout.z = -90f;
                positionVoltage.x = -0.022f;
                positionCurrent.x = 0.022f;
                break;
            case Direction.South:
            case Direction.West:
                rotationVoltage.z = rotationCurrent.z = rotationTimeout.z = 90f;
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
        labelTimeout.transform.localEulerAngles = rotationTimeout;
    }

    public override void SetShortCircuit(bool isShortCircuit, bool isForward)
    {
        IsShortCircuit = isShortCircuit;
        if (isShortCircuit)
        {
            // Hide the labels
            labelVoltage.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    public override void Toggle()
    {
        // If the timer has been disabled, don't do anything
        if (!ticking)
            return;

        IsClosed = !IsClosed;

        // Set the blade to the proper position by rotating the pivot
        RotatePivot();

        // Play the switch sound
        StartCoroutine(PlaySound(switchSound, 0f));

        // Set a new callback to toggle again
        StartTimer();

        // Trigger a new simulation since we may have just closed or opened a circuit
        Lab.SimulateCircuit();
    }

    private void RotatePivot()
    {
        var rotation = pivot.transform.localEulerAngles;
        rotation.z = IsClosed ? 0 : 90f;
        pivot.transform.localEulerAngles = rotation;
    }

    private void StartTimer()
    {
        ticking = true;
        lastSwitch = DateTime.Now;
        lastTimeoutSeconds = timeoutSeconds;
        Invoke("Toggle", timeoutSeconds);
    }

    public override void Adjust()
    {
        // Decrement the timeout value, wrapping back to the maximum value after 1
        timeoutSeconds = (timeoutSeconds > 1) ? (timeoutSeconds - 1) : maxTimeoutSeconds;
    }

    public override void SetVoltage(double voltage)
    {
        Voltage = voltage;

        // Update label text
        labelVoltageText.text = voltage.ToString("0.##") + "V";
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (current <= 0.0000001)
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
}
