using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulb : MonoBehaviour, /*OVRGrabbable, */ICircuitComponent {
    public GameObject labelResistance;
    public GameObject labelCurrent;
    public GameObject filament;
    public GameObject glow;

    bool isActive = false;
    float intensity = 0f;
    double voltage = 0f;
    double current = 0f;
    double resistance = 1000f;

    void Update () {

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
        Light light = glow.GetComponent<Light>();

        // Cool down the filament and deactivate the light object
        filament.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        light.enabled = false;
    }

    private void ActivateLight()
    {
        Light light = glow.GetComponent<Light>();

        // Heat up the filament and activate the light object
        filament.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        light.enabled = true;
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

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (newCurrent <= 0.0000001)
        {
            isActive = false;
            DeactivateLight();

            // Hide the labels
            labelResistance.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrent.GetComponent<TextMesh>().text = (current * 1000f).ToString("0.##") + "mA";

            // Update light intensity
            intensity = (((float)current * 1000f) / 10.0f);

            // Set the filament emission color
            Color baseColor = Color.yellow;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace((float)intensity * 0.5f);
            filament.GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);

            // Set the light object's intensity
            Light light = glow.GetComponent<Light>();
            light.intensity = (float)intensity;
        }
    }

    /*
    Point GetParentCoordinates()
    {
        string name = transform.parent.name.Substring(4);

        int row = int.Parse(name.Substring(0, name.IndexOf('_')));
        int col = int.Parse(name.Substring(name.IndexOf('_') + 1));

        return new Point(col, row);
    }

    override public void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        if (transform.parent && transform.parent.name.Contains("Peg"))
        {
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<SphereCollider>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = true;

            // Remove this object from the board state
            Point start = GetParentCoordinates();
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            script.RemoveComponent(this.gameObject, start);
            transform.parent = null;
        }

        base.GrabBegin(hand, grabPoint);
    }
    */
}
