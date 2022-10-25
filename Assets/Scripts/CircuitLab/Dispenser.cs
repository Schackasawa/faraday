using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour, IDispenser
{
    // Public members set in Unity Object Inspector
    public enum ComponentTag { Wire, LongWire, Battery, Switch, Motor, Bulb, Balloon, Timer, Flute, Button, Solar };
    public ComponentTag componentTag;

    float xSpeed = 90f;
    float growDelay = 0.1f;
    float growTime = 0.5f;
    int numDispensed = 1;

    float arcHeight = 0.2f;
    float animationSpeed = 0.5f;

    protected class AnimatedComponent
    {
        public GameObject gameObject;
        public Vector3 startPosition;
        public Vector3 currentPosition;
        public Vector3 targetPosition;
        public Vector3 startScale;
        public Vector3 currentScale;
        public Vector3 targetScale;

        public AnimatedComponent(GameObject component, Vector3 target)
        {
            gameObject = component;
            startPosition = component.transform.position;
            currentPosition = startPosition;
            targetPosition = target;

            startScale = component.transform.localScale;
            currentScale = component.transform.localScale;
            targetScale = component.transform.localScale / 3;
        }
    };
    List<AnimatedComponent> animatedComponents;

    void Start()
    {
        animatedComponents = new List<AnimatedComponent>();
    }

    void Update()
    {
        // Rotate the object floating in this dispenser
        transform.Rotate(Vector3.up, xSpeed * Time.deltaTime);

        // Animate any objects we are currently recalling
        AnimateComponents();
    }

    private void AnimateComponents()
    {
        foreach (var component in animatedComponents)
        {
            // First, animate the total position so that the x and z values are calculated properly
            component.currentPosition = Vector3.MoveTowards(component.gameObject.transform.position, component.targetPosition, animationSpeed * Time.deltaTime);

            // Handle the y component of the position specially, to give it the desired arc throughout the animation
            float x0 = component.startPosition.x;
            float x1 = component.targetPosition.x;
            float dist = x1 - x0;
            if (dist != 0f)
            {
                float nextX = Mathf.MoveTowards(component.gameObject.transform.position.x, x1, animationSpeed * Time.deltaTime);
                float baseY = Mathf.Lerp(component.startPosition.y, component.targetPosition.y, (nextX - x0) / dist);
                float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                component.currentPosition.y = baseY + arc;

                // Also shrink as we approach the destination
                component.currentScale = Vector3.Lerp(component.startScale, component.targetScale, (nextX - x0) / dist);
                component.gameObject.transform.localScale = component.currentScale;
            }

            // Move to the calculated position
            component.gameObject.transform.position = component.currentPosition;

            // Destroy the object when we reach the target
            if (component.currentPosition == component.targetPosition)
            {
                Destroy(component.gameObject);
                component.gameObject = null;
            }
        }

        // Remove completed animations from the list
        animatedComponents.RemoveAll(item => item.gameObject == null);
    }

    public void Reset()
    {
        GameObject child = (gameObject.transform.childCount > 0) ? gameObject.transform.GetChild(0).gameObject : null;

        // Find all objects created by this dispenser
        GameObject[] components;
        components = GameObject.FindGameObjectsWithTag(componentTag.ToString());
        foreach (GameObject component in components)
        {
            // Avoid the component that is currently in this dispenser
            if (component != child)
            {
                // Pull each object back to the dispenser and destroy it when it arrives
                RecallComponent(component);
            }
        }
    }

    protected void RecallComponent(GameObject component)
    {
        // Disable gravity and any other forces from affecting the component while in flight
        component.GetComponent<Rigidbody>().useGravity = false;
        component.GetComponent<Rigidbody>().isKinematic = true;

        // Disable colliders so it won't snap to any breadboard pegs
        component.GetComponent<BoxCollider>().enabled = false;
        component.GetComponent<SphereCollider>().enabled = false;

        // Add it to a list of currently animating components
        animatedComponents.Add(new AnimatedComponent(component, transform.position));
    }

    void OnTriggerExit(Collider other)
    {
        // Find out if we lost our child
        if (transform.childCount == 0 && other.transform.name.Contains("Component") && 
            (other.transform.tag == componentTag.ToString()))
        {
            // Turn back on gravity
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;

            // Create another object
            var clone = Instantiate(other);
            clone.transform.name = "Component" + other.tag + numDispensed++;
            clone.transform.parent = transform;
            clone.transform.localPosition = new Vector3(0, 0, 0);
            clone.transform.localRotation = new Quaternion();
            clone.gameObject.GetComponent<Rigidbody>().useGravity = false;
            clone.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            clone.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // Start it out tiny and scale it up slowly over a few seconds
            StartCoroutine(ScaleUp(clone.gameObject.transform, growDelay, growTime));
        }
    }

    public static IEnumerator ScaleUp(Transform transform, float delay, float time)
    {
        yield return new WaitForSeconds(delay);

        while (transform.localScale.x < 1f)
        {
            float newScale = transform.localScale.x + (Time.deltaTime / time);
            if (newScale > 1f)
            {
                newScale = 1f;
            }
            transform.localScale = new Vector3(newScale, newScale, newScale);

            yield return null;
        }
    }
}
