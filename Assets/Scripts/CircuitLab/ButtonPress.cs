using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    public CapsuleCollider buttonCollider;

    private bool isPressed = false;
    private Collider otherCollider = null;
    private bool cooldownActive = false;

    void Update()
    {
        if (isPressed)
        {
            // Check if we are still colliding. If not, release the button.
            if (!otherCollider.bounds.Intersects(buttonCollider.bounds))
            {
                // Find our parent and toggle its state
                ToggleParent();
                isPressed = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && !isPressed && 
            other.gameObject.name.Contains("Pinch"))
        {
            isPressed = true;
            otherCollider = other;

            // Find our parent and toggle its state
            ToggleParent();

            // Don't allow another press for a little while to prevent flickering
            cooldownActive = true;
            Invoke("Cooldown", 0.1f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }

    void ToggleParent()
    {
        var script = transform.parent.GetComponent<CircuitComponent>();
        if (script != null)
        {
            script.Toggle();
        }
    }
}
