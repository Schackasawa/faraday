using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public GameObject audioGameObject;

    AudioSource clip;

    void Start()
    {
        clip = audioGameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (clip)
        {
            clip.Play();
        }
    }
}
