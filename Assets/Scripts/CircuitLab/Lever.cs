using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public enum Action { ResetComponents, ToggleLabels };

    // Public members set in Unity Object Inspector
    public Action action;
    public HingeJoint hinge;
    public AudioSource activationSound;
    public GameObject circuitLab;

    JointLimits limits; 
    bool triggered = false;
    ICircuitLab lab;

    void Start()
    {
        limits = hinge.limits;
        lab = circuitLab.GetComponent<ICircuitLab>();
    }

    void Update()
    {
        if (triggered)
        {
            if (hinge.angle <= (limits.min + 0.1f))
            {
                triggered = false;
            }
        }
        else
        {
            if (hinge.angle >= (limits.max - 0.1f))
            {
                triggered = true;
                ActivateTrigger();
            }
        }
    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    void ActivateTrigger()
    {
        StartCoroutine(PlaySound(activationSound, 0f));

        switch (action)
        {
            case Action.ResetComponents:
                // Reinitialize all circuit lab state
                lab.Reset();
                break;

            case Action.ToggleLabels:
                // Toggle circuit labels for current, resistance, etc.
                lab.ToggleLabels();
                break;

            default:
                break;
        }
    }
}
