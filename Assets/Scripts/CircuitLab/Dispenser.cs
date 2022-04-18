using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour
{
    float xSpeed = 90f;
    float growDelay = 0.1f;
    float growTime = 0.5f;
    int numDispensed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the object
        transform.Rotate(Vector3.up, xSpeed * Time.deltaTime);
    }

    void OnTriggerExit(Collider other)
    {
        // Find out if we lost our child
        if (transform.childCount == 0 && other.transform.name.Contains("Component"))
        {
            // Turn back on gravity
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;

            // Create another object
            var clone = Instantiate(other);
            clone.transform.name = other.name + numDispensed++;
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
