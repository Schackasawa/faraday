using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Wire : MonoBehaviour, ICircuitComponent
{
    public GameObject circuitLab;
    public float normalCircuitSpeed = 0.16f;
    public float shortCircuitSpeed = 0.40f;
    public Vector3 startPosition = new Vector3(0, 0.04f, 0);
    public Vector3 endPosition = new Vector3(0, -0.04f, 0);
    public GameObject labelVoltage;
    public GameObject labelCurrent;
    public Color normalEmissionColor;
    public Color shortEmissionColor;
    public float emissionIntensity;

    Color32 normalBaseColor = new Color32(56, 206, 76, 255);
    Color32 shortBaseColor = new Color32(88, 18, 18, 255);
    bool isPlaced = false;
    bool isHeld = false;
    bool isClone = false;
    Point startingPeg;
    float baseCurrent = 0.005f;
    float speed = 0f;
    bool isForward = true;
    bool isActive = false;
    bool isShortCircuit = false;
    double voltage = 0f;
    double current = 0f;

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
        isClone = true;
    }

    public void Place(Point start)
    {
        isPlaced = true;
        startingPeg = start;
    }

    void Update () {
        if (!isClone)
        {
            // Figure out what direction the current is flowing and set the start/end positions accordingly
            Vector3 start = isForward ? startPosition : endPosition;
            Vector3 end = isForward ? endPosition : startPosition;

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
                        if (isActive != child.gameObject.activeSelf)
                        {
                            child.gameObject.SetActive(isActive);
                        }
                    }
                }
            }

            // Show/hide the labels
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = circuitLab.GetComponent<CircuitLab>();
            if (script != null)
            {
                bool showLabels = script.showLabels && isActive && !isShortCircuit;
                labelVoltage.gameObject.SetActive(showLabels);
                labelCurrent.gameObject.SetActive(showLabels);
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

    public void SetActive(bool active, bool forward)
    {
        isActive = active;
        if (active)
        {
            isForward = forward;
            speed = normalCircuitSpeed;

            // Change electrons to green
            SetElectronColor(normalEmissionColor, normalBaseColor);
        }

        // Make sure label is right side up
        var componentRotation = transform.localEulerAngles;
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        var rotationCurrent = labelCurrent.transform.localEulerAngles;
        var positionCurrent = labelCurrent.transform.localPosition;
        if (componentRotation.z == 0f)
        {
            rotationVoltage.z = rotationCurrent.z = - 90f;
            positionVoltage.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 90f)
        {
            rotationVoltage.z = rotationCurrent.z = -90f;
            positionVoltage.x = -0.01f;
            positionCurrent.x = 0.025f;
        }
        else if (componentRotation.z == 180f)
        {
            rotationVoltage.z = rotationCurrent.z = 90f;
            positionVoltage.x = 0.01f;
            positionCurrent.x = -0.025f;
        }
        else
        {
            rotationVoltage.z = rotationCurrent.z = 90f;
            positionVoltage.x = 0.01f;
            positionCurrent.x = -0.025f;
        }

        // Apply label positioning
        labelVoltage.transform.localEulerAngles = rotationVoltage;
        labelVoltage.transform.localPosition = positionVoltage;
        labelCurrent.transform.localEulerAngles = rotationCurrent;
        labelCurrent.transform.localPosition = positionCurrent;
    }

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {
        isShortCircuit = shortCircuit;
        if (shortCircuit)
        {
            isActive = true;
            isForward = forward;
            speed = shortCircuitSpeed;

            // Change electrons to red
            SetElectronColor(shortEmissionColor, shortBaseColor);

            // Hide the labels
            labelVoltage.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
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

        // Update label text
        labelVoltage.GetComponent<TextMesh>().text = voltage.ToString("0.##") + "V";
    }

    public void SetCurrent(double newCurrent)
    {
        current = newCurrent;

        // If we don't have a significant positive current, then we are inactive, even if
        // we are technically part of an active circuit
        if (newCurrent <= 0.0000001)
        {
            isActive = false;
            DeactivateElectrons();

            // Hide the labels
            labelVoltage.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrent.GetComponent<TextMesh>().text = (current * 1000f).ToString("0.##") + "mA";

            // Update electron speed
            speed = normalCircuitSpeed * ((float)current / baseCurrent);
        }
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

