using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocker : MonoBehaviour
{
    public AudioSource audioSource1;

    private bool cooldownActive = false;

    void Start()
    {

    }

    void Update()
    {

    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && 
            other.gameObject.name.Contains("Pinch"))
        {
            // Find our parent and toggle its state
            var script = transform.parent.GetComponent<ICircuitComponent>();
            if (script != null)
            {
                script.Toggle();

                // Set the blade to the proper position by rotating the pivot
                var rotation = transform.localEulerAngles;
                rotation.y = -rotation.y;
                transform.localEulerAngles = rotation;

                StartCoroutine(PlaySound(audioSource1, 0f));

                cooldownActive = true;
                Invoke("Cooldown", 1);
            }
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
