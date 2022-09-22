using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerFace : MonoBehaviour
{
    // Public members set in Unity Object Inspector
    public AudioSource timerAdjustmentSound;

    private bool cooldownActive = false;

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
            // Find our parent and adjust its timeout setting
            var script = transform.parent.GetComponent<CircuitComponent>();
            if (script != null)
            {
                script.Adjust();

                StartCoroutine(PlaySound(timerAdjustmentSound, 0f));

                cooldownActive = true;
                Invoke("Cooldown", 0.3f);
            }
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
