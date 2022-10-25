using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class Solar : CircuitComponent, IBattery
{
    // Public members set in Unity Object Inspector
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;
    public GameObject panel;
    public int rotationIncrement = 90;
    bool cooldownActive = false;
    public AudioSource rotatePanelAudio;

    public float BatteryVoltage { get; private set; }

    public Solar() 
    {
        // XXX - Voltage of Solar Panel will be variable based on light intensity and direction
        BatteryVoltage = 10f;
    }

    protected override void Start()
    {
        // Set voltage label text
        labelVoltageText.text = BatteryVoltage.ToString("0.#") + "V";
    }

    protected override void Update ()
    {
        // Show/hide the labels
        labelVoltage.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && other.gameObject.name.Contains("Pinch"))
        {
            // Rotate the panel by one increment
            var rotation = panel.transform.localEulerAngles;
            rotation.z += rotationIncrement;
            panel.transform.localEulerAngles = rotation;

            StartCoroutine(PlaySound(rotatePanelAudio, 0f));

            cooldownActive = true;
            Invoke("Cooldown", 0.5f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}

