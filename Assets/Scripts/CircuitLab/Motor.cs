using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : /*OVRGrabbable, */MonoBehaviour, ICircuitComponent
{
    float normalSpeed = 600f;
    public GameObject labelResistance;
    public GameObject labelCurrent;

    float speed = 0f;
    float baseCurrent = 0.005f;
    bool isActive = false;
    double voltage = 0f;
    double current = 0f;
    double resistance = 2000f;

	void Update () {

        if (isActive)
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
    }

    public void SetActive(bool active, bool forward)
    {
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

    public void SetShortCircuit(bool shortCircuit, bool forward)
    {
        // Not possible, since motors have resistance
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

            // Hide the labels
            labelResistance.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            // Update label text
            labelCurrent.GetComponent<TextMesh>().text = (current * 1000f).ToString("0.##") + "mA";

            // Update motor speed
            speed = normalSpeed * ((float)current / baseCurrent);
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
