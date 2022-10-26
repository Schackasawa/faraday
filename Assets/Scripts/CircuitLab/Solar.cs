using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class Solar : CircuitComponent, IBattery, IDynamic
{
    // Public members set in Unity Object Inspector
    public GameObject labelWattage;
    public TMP_Text labelWattageText;
    public GameObject panel;
    public GameObject cell;
    public int rotationIncrement = 90;
    bool cooldownActive = false;
    public AudioSource rotatePanelAudio;
    public GameObject sun;
    public AudioSource sunActivationAudio;

    public float BatteryVoltage { get; private set; }
    public float SolarResistance { get; private set; }
    public float SolarWattage { get; private set; }

    private float previousWattage = 0;
    private bool registered = false;

    const float MaxWattage = 10f;

    public Solar() 
    {
        // Wattage of Solar Panel is variable based on sun direction
        BatteryVoltage = 10f;
        SolarWattage = 0;
    }

    ~Solar()
    {
        if (registered)
        {
            // Remove ourselves from the circuit lab's list of dynamic objects
            Lab.UnregisterDynamicComponent(this);
        }
    }

    protected override void Start()
    {
        // Set wattage label text
        labelWattageText.text = SolarWattage.ToString("0.0") + "W";
    }

    protected override void Update ()
    {
        if (!registered)
        {
            // Register as a dynamic component so we'll get coordinated UpdateState calls
            registered = true;
            Lab.RegisterDynamicComponent(this);
        }

        // Make sure the sun is active when any solar panel is placed on the board
        if (IsPlaced && !sun.activeInHierarchy)
        {
            sun.SetActive(true);
            StartCoroutine(PlaySound(sunActivationAudio, 0f));
        }

        // Show/hide the labels
        labelWattage.gameObject.SetActive(IsActive && Lab.showLabels);

        // Cast a ray from the center of the panel so we can compute the relative angle of the sun
        Vector3 forwardPosition = cell.transform.position + cell.transform.forward * 0.5f;

        // Draw visible lines for the sun and normal vectors to aid in debugging
        if (IsPlaced)
        {
            //DrawLine(cell.transform.position, sun.transform.position, Color.green);
            //DrawLine(cell.transform.position, forwardPosition, Color.red);
        }

        // Find the angle between the sun and the panel normal vector
        float angle = Vector3.Angle(cell.transform.forward, sun.transform.position - cell.transform.position);

        // Use a basic linear function for now to compute wattage based on angle to the sun
        SolarWattage = 0;
        if (angle < 90f)
        {
            float pctWattage = 1f - (angle / 90f);
            SolarWattage = MaxWattage * pctWattage;
        }

        // Compute voltage and resistance based on wattage to simulate the variable
        // current that a solar panel produces in different lighting conditions.
        BatteryVoltage = SolarWattage; // XXX - Just set voltage to wattage for initial prototype

        // Update label text
        labelWattageText.text = SolarWattage.ToString("0.0") + "W";
    }

    public bool UpdateState(int numActiveCircuits)
    {
        // If the panel output has changed since our last UpdateState call, return true
        // to let the circuit lab know that it needs to run an updated simulation.
        bool simulate = (previousWattage != SolarWattage);
        previousWattage = SolarWattage;

        return simulate;
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.001f;
        lr.endWidth = 0.001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, Time.deltaTime);
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

