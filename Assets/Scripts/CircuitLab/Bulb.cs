using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulb : MonoBehaviour, ICircuitComponent {
    public GameObject labelResistance;
    public GameObject labelCurrent;
    public GameObject filament;

    bool isPlaced = false;
    bool isHeld = false;
    Point startingPeg;
    bool isActive = false;
    float intensity = 0f;
    double voltage = 0f;
    double current = 0f;
    double resistance = 1000f;

    void Update () {

    }

    public bool IsHeld()
    {
        return isHeld;
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

        // Show/hide the labels
        labelResistance.gameObject.SetActive(active);
        labelCurrent.gameObject.SetActive(active);

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
        Debug.Log("Intensity = " + intensity + ", current = " + current);

        // Set the filament emission color and intensity
        Color baseColor = Color.yellow;
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
}
