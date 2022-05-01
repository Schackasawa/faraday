using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulb : MonoBehaviour, ICircuitComponent
{
    public GameObject circuitLab;
    public GameObject labelResistance;
    public GameObject labelCurrent;
    public GameObject filament;
    public AudioSource colorChangeAudio;

    bool isPlaced = false;
    bool isHeld = false;
    Point startingPeg;
    bool isActive = false;
    float intensity = 0f;
    double voltage = 0f;
    double current = 0f;
    double resistance = 1000f;

    bool cooldownActive = false;
    Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
    int emissionColorIdx = 1;

    void Update ()
    {
        // Show/hide the labels
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = circuitLab.GetComponent<CircuitLab>();
        if (script != null)
        {
            labelResistance.gameObject.SetActive(isActive && script.showLabels);
            labelCurrent.gameObject.SetActive(isActive && script.showLabels);
        }
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public bool IsPlaced()
    {
        return isPlaced;
    }

    public void SetClone()
    {
    }

    public void Place(Point start)
    {
        isPlaced = true;
        startingPeg = start;
    }

    public void SetActive(bool active, bool forward)
    {
        if (!isActive && active)
        {
            ActivateLight();
        }

        if (isActive && !active)
        {
            DeactivateLight();
        }

        isActive = active;

        // Set resistance label text
        labelResistance.GetComponent<TextMesh>().text = resistance.ToString("0.##") + "Ω";

        // Make sure label is right side up
        var componentRotation = transform.localEulerAngles;
        var rotationResistance = labelResistance.transform.localEulerAngles;
        var positionResistance = labelResistance.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        if (componentRotation.z == 0f)
        {
            rotationResistance.z = rotationCurrent.z = -90f;
            positionResistance.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 90f)
        {
            rotationResistance.z = rotationCurrent.z = -90f;
            positionResistance.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 180f)
        {
            rotationResistance.z = rotationCurrent.z = 90f;
            positionResistance.x = 0.01f;
            positionCurrent.x = -0.025f;
        }
        else
        {
            rotationResistance.z = rotationCurrent.z = 90f;
            positionResistance.x = 0.01f;
            positionCurrent.x = -0.025f;
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

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {
        // Not possible, since bulbs have resistance
    }

    public bool IsClosed()
    {
        return true;
    }

    public void Toggle()
    {

    }

    public void SetVoltage(double newVoltage)
    {
        voltage = newVoltage;
    }

    public void SetCurrent(double newCurrent)
    {
        current = newCurrent;

        // Update label text
        labelCurrent.GetComponent<TextMesh>().text = (current * 1000f).ToString("0.##") + "mA";

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
        isHeld = true;

        // Enable box and sphere colliders so this piece can be placed somewhere else on the board.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (isPlaced)
        {
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            script.RemoveComponent(this.gameObject, startingPeg);

            isPlaced = false;
        }
    }

    public void SelectExited()
    {
        isHeld = false;

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
        if (!cooldownActive && isActive &&
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
