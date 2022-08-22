﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Balloon : CircuitComponent
{
    // Public members set in Unity Object Inspector
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject balloonParent;
    public float startScale = 0f;
    public float endScale = 4f;
    public AudioSource audioSourceInflate;
    public AudioSource audioSourcePop;

    float speed = 0f;
    float baseCurrent = 0.005f;
    float normalSpeed = 1f;
    bool inflating = false;

    public Balloon() : base(CircuitComponentType.Balloon) { }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    protected override void Update()
    {
        if (IsActive)
        {
            float step = speed * Time.deltaTime;
            
            // Inflate the balloon at a fixed speed until it pops
            float newScale = balloonParent.transform.localScale.x + step;

            // On Pop, play sound and reset scale
            if (newScale > endScale)
            {
                StartCoroutine(PlaySound(audioSourcePop, 0f));
                newScale = startScale;
            }

            balloonParent.transform.localScale = new Vector3(newScale, newScale, newScale);

            // Activate inflation sound if it's not already playing
            if (!inflating)
            {
                inflating = true;
                audioSourceInflate.Play();
            }
        }
        else
        {
            audioSourceInflate.Stop();
            inflating = false;
        }

        // Show/hide the labels
        labelResistance.gameObject.SetActive(IsActive && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        // Set resistance label text
        labelResistanceText.text = CircuitLab.BalloonResistance.ToString("0.##") + "Ω";

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

            // Update balloon inflation speed
            speed = normalSpeed * ((float)current / baseCurrent);
        }
    }

}
