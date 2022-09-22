using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Flute : CircuitComponent, IResistor
{
    // Public members set in Unity Object Inspector
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject key;
    public GameObject flute;
    public GameObject labelNote;
    public TMP_Text labelNoteText;
    public AudioSource noteChangeAudio;
    public AudioSource bellAudio;

    private bool cooldownActive = false;
    private float scale = Mathf.Pow(2f, 1.0f / 12f);
    private int noteIndex = 0;
    private string[] notes = new string[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public float Resistance { get; private set; }

    public Flute()
    {
        Resistance = 2500f;
    }

    protected override void Update ()
    {
        // Show/hide the labels
        labelResistance.gameObject.SetActive(IsActive && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        if (!IsActive && isActive)
        {
            // Play the note once when we are first activated
            PlayFlute();
        }

        IsActive = isActive;

        // Set resistance label text
        labelResistanceText.text = Resistance.ToString("0.#") + "Ω";

        // Make sure labels are right side up
        RotateLabel(labelResistance, LabelAlignment.Top);
        RotateLabel(labelCurrent, LabelAlignment.Bottom);
        RotateLabel(labelNote, LabelAlignment.Center);
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        // Update label text
        labelCurrentText.text = (current * 1000f).ToString("0.#") + "mA";
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive &&
            other.gameObject.name.Contains("Pinch"))
        {
            // Switch the note to the next one in the list
            noteIndex = ++noteIndex % 12;
            bellAudio.pitch = Mathf.Pow(scale, noteIndex);
            labelNoteText.text = notes[noteIndex];

            // Play the note so the user knows what note they've selected
            PlayFlute();

            // Make the note label visible for a few seconds
            labelNote.gameObject.SetActive(true);
            CancelInvoke("HideNoteLabel");
            Invoke("HideNoteLabel", 3f);

            // Rotate the key to the appropriate orientation
            var rotation = key.transform.localEulerAngles;
            rotation.z = ((noteIndex + 1) * (360f / 12f));
            key.transform.localEulerAngles = rotation;

            cooldownActive = true;
            Invoke("Cooldown", 0.5f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }

    void HideNoteLabel()
    {
        labelNote.gameObject.SetActive(false);
    }

    void PlayFlute()
    {
        // Play the appropriate note
        StartCoroutine(PlaySound(bellAudio, 0f));

        // Expand the flute body slightly to give some visual feedback
        flute.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        // Set a callback to shrink it
        Invoke("ShrinkFlute", 0.1f);
    }

    void ShrinkFlute()
    {
        // Restore the original size of the flute body
        flute.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
