using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    private bool cooldownActive = false;

    void Start() { }
    void Update() { }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && 
            other.gameObject.name.Contains("Pinch"))
        {
            // Find our parent and toggle its state
            var script = transform.parent.GetComponent<CircuitComponent>();
            if (script != null)
            {
                script.Toggle();

                cooldownActive = true;
                Invoke("Cooldown", 0.1f);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Pinch"))
        {
            // Find our parent and toggle its state
            var script = transform.parent.GetComponent<CircuitComponent>();
            if (script != null)
            {
                script.Toggle();
            }
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
