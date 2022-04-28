using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public GameObject audioGameObject;

    AudioSource clip;

    void Start()
    {
        Debug.Log("Start Collider");
        clip = audioGameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter!");
        if (clip)
        {
            clip.Play();
        }
    }
}
