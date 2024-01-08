using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundParticleSystem : MonoBehaviour
{
    public AudioClip Clip;
    [HideInInspector]
    public AudioSource audioSource;
    public float volume;
    public bool emit = true;
    [Tooltip("Interval to play sounds. (in seconds)")]
    public float interval = 0.1f;

    void Start()
    {
        // setup audio source
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Clip;
    }

    /// <summary>
    /// Start "emitting sound"
    /// </summary>
    public void Begin()
    {
        StartCoroutine("PlaySound");
    }

    /// <summary>
    /// Play Sound "loop"
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(interval);

        // if emitting is enabled play audio and schedule next sound
        if (emit)
        {
            audioSource.PlayOneShot(Clip, volume);
            StartCoroutine("PlaySound");
        }
        // else stop audio source
        else audioSource.Stop();
    }
}
