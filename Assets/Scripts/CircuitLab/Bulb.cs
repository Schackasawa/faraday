using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bulb : CircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject filament;
    public AudioSource colorChangeAudio;

    float intensity = 0f;

    bool cooldownActive = false;
    Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
    int emissionColorIdx = 1;

    public Bulb() : base(CircuitComponentType.Bulb) { }

    protected override void Update ()
    {
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
        if (!IsActive && isActive)
        {
            ActivateLight();
        }

        if (IsActive && !isActive)
        {
            DeactivateLight();
        }

        IsActive = isActive;

        // Set resistance label text
        labelResistanceText.text = CircuitLab.BulbResistance.ToString("0.##") + "Ω";

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

    private void DeactivateLight()
    {
        // Cool down the filament by deactivating the emission
        filament.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
    }

    private void ActivateLight()
    {
        // Heat up the filament by activating the emission
        filament.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // Update label text
        labelCurrentText.text = (current * 1000f).ToString("0.##") + "mA";

        // Calculate light intensity based on current
        float maxCurrent = 0.01f;
        float maxIntensity = 5.0f;
        float minIntensity = 3.0f;
        float pctCurrent = ((float)current > maxCurrent ? maxCurrent : (float)current) / maxCurrent;
        intensity = (pctCurrent * (maxIntensity - minIntensity)) + minIntensity;

        ActivateFilament();
    }

    public void ActivateFilament()
    {
        // Set the filament emission color and intensity
        Color baseColor = colors[emissionColorIdx];
        Color finalColor = baseColor * Mathf.Pow(2, intensity);
        filament.GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);
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

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && IsActive &&
            other.gameObject.name.Contains("Pinch"))
        {
            // Switch the emission color to the next one in the list
            emissionColorIdx = ++emissionColorIdx % colors.Length;
            ActivateFilament();

            StartCoroutine(PlaySound(colorChangeAudio, 0f));

            cooldownActive = true;
            Invoke("Cooldown", 0.5f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
