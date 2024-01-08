using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;

public class Fuse : MonoBehaviour
{
    [Header("Settings")]
    public FirecrackerData FirecrackerData;
    private float timeToDetonate;
    private float explosionForce;
    private float totalDetonationTime;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Header("Effects")]
    public GameObject[] Sparks;
    public GameObject[] Smokes;

    Material mat;
    [HideInInspector]
    public bool activated = false;
    [HideInInspector]
    public bool detonated = false;
    float fuseProgress = 0.0f;
    bool fadeOutFuse = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        InitValues();
        SpawnEffects();
    }

    private void InitValues()
    {
        timeToDetonate = UnityEngine.Random.Range(FirecrackerData.DetonationRange.Min, FirecrackerData.DetonationRange.Max);
        explosionForce = UnityEngine.Random.Range(FirecrackerData.ExplosionForceRange.Min, FirecrackerData.ExplosionForceRange.Max);

        Sparks = new GameObject[FirecrackerData.SparksPrefabs.Length];
        Smokes = new GameObject[FirecrackerData.SmokePrefabs.Length];

        // calculate total detonation time (if there are multiple detonations add offsets)
        totalDetonationTime = timeToDetonate;
        if(FirecrackerData.DetonationCount > 1) for(int i = 0; i < FirecrackerData.DetonationCount; i++) totalDetonationTime += FirecrackerData.DetonationOffsets[i];
    }

    private void SpawnEffects()
    {
        // Spark Effects
        for (int i = 0; i < FirecrackerData.SparksPrefabs.Length; i++)
        {
            Sparks[i] = Instantiate(FirecrackerData.SparksPrefabs[i], transform);
            Sparks[i].transform.localRotation = FirecrackerData.SparksRotations[i];
            Sparks[i].SetActive(false);
        }

        // Smoke Effects
        for (int i = 0; i < FirecrackerData.SmokePrefabs.Length; i++)
        {
            Smokes[i] = Instantiate(FirecrackerData.SmokePrefabs[i], transform);
            Smokes[i].transform.localRotation = FirecrackerData.SmokeRotations[i];
            Smokes[i].SetActive(false);
        }
    }

    private void Update()
    {
        // fade out fuse sound in the last fifth of the detonation time
        if (fadeOutFuse && audioSource.volume > 0) audioSource.volume -= FirecrackerData.FuseVolume * Time.deltaTime * 5;

        if (activated && !detonated)
        {
            mat.SetInt("_FuseActive", 1);
            if(fuseProgress < 1.0f)
            {
                fuseProgress += (1.0f / timeToDetonate) * Time.deltaTime;

                // stop fuse sparkles after the fuse progress is greater than the sparks duration
                for(int i = 0; i < Sparks.Length; i++)
                {
                    if ((fuseProgress * timeToDetonate) > (FirecrackerData.SparksDurations[i] * timeToDetonate) + (FirecrackerData.SparkOffsets[i] * timeToDetonate))
                    {
                        Sparks[i].GetComponent<ParticleSystem>().Stop();
                    }
                }

                // stop smoke after the fuse progress is greater than the smoke duration
                for (int i = 0; i < Smokes.Length; i++)
                {
                    if ((fuseProgress * timeToDetonate) > (FirecrackerData.SmokeDurations[i] * timeToDetonate) + (FirecrackerData.SmokeOffsets[i] * timeToDetonate))
                    {
                        Smokes[i].GetComponent<ParticleSystem>().Stop();
                    }
                }

                // particle effects should move with the fuse progress (between two given points
                Vector3 curPos = Vector3.Lerp(FirecrackerData.FusePath.From, FirecrackerData.FusePath.To, fuseProgress);
                Sparks[0].transform.localPosition = curPos;
                Smokes[0].transform.localPosition = curPos;

                // set transparency of fuse shader (how much of the fuse is gone basically)
                mat.SetFloat("_Transparency", 1.0f - fuseProgress);
            } 
            else
            {
                activated = true; detonated = true;

                Transform parent = transform.parent.transform;

                // check if there are other fuses in a given radius (and activate them too with a random detonation time)
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, FirecrackerData.DetonationRadius, FirecrackerData.DetonationMask);
                foreach(Collider col in hitColliders)
                {
                    // not on self
                    if (col.gameObject.transform.parent == gameObject.transform.parent) continue;
                    // get fuse
                    Fuse fuse = col.gameObject.transform.parent.Find("Fuse").GetComponent<Fuse>();
                    // if already activated skip
                    if (fuse.activated == true) continue;
                    // else activate
                    fuse.timeToDetonate = UnityEngine.Random.Range(100,2000) / 1000.0f;
                    fuse.ActivateFuse();
                }

                // spawn fractured prefab for explosion effect
                GameObject fractured = Instantiate(FirecrackerData.FracturedPrefab, parent.transform.position, parent.transform.rotation);
                // add random force to each piece of the fractured prefab
                foreach(Rigidbody rb in fractured.GetComponentsInChildren<Rigidbody>())
                {
                    Vector3 force = (rb.transform.position - transform.position).normalized * explosionForce;
                    rb.AddForce(force);
                }

                // disable the renderers on the original prefabs children
                foreach(Renderer renderer in transform.parent.GetComponentsInChildren<Renderer>()) renderer.enabled = false;

                // destroy after delay
                StartCoroutine("DestroyDelayed");
            }
        }
    }

    /// <summary>
    /// Destroy parent with a delay (after the detonation sound + 0.2 seconds)
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyDelayed()
    {
        yield return new WaitForSeconds(totalDetonationTime + 0.2f);

        // destroy parent
        Destroy(transform.parent.gameObject);
    }

    /// <summary>
    /// Activates the Fuse
    /// </summary>
    public void ActivateFuse()
    {
        // deactivate outline
        transform.parent.transform.GetComponent<Outline>().enabled = false;

        activated = true;

        // enable particle effects
        Sparks[0].SetActive(true);
        Smokes[0].SetActive(true);
        StartCoroutine("StartSparks");
        StartCoroutine("StartSmokes");

        // play fuse sound
        audioSource.volume = FirecrackerData.FuseVolume;
        audioSource.PlayOneShot(FirecrackerData.FuseSound);
        StartCoroutine("Detonate");
    }

    IEnumerator StartSparks()
    {
        for(int i = 0; i < Sparks.Length; i++)
        {
            yield return new WaitForSeconds(FirecrackerData.SparkOffsets[i] * timeToDetonate);
            Sparks[i].SetActive(true);
            foreach (SoundParticleSystem system in Sparks[i].GetComponentsInChildren<SoundParticleSystem>()) system.Begin();
        }
    }

    IEnumerator StartSmokes()
    {
        for (int i = 0; i < Smokes.Length; i++)
        {
            yield return new WaitForSeconds(FirecrackerData.SmokeOffsets[i] * timeToDetonate);
            Smokes[i].SetActive(true);
        }
    }

    /// <summary>
    /// Coroutine to time detonation sounds and effects
    /// </summary>
    /// <returns></returns>
    IEnumerator Detonate()
    {
        float fifth = (timeToDetonate / 5.0f);

        yield return new WaitForSeconds((timeToDetonate + FirecrackerData.DetonationOffsets[0]) - fifth);

        // start fuse fade out
        fadeOutFuse = true;

        yield return new WaitForSeconds(fifth);

        // play detonate sound
        audioSource.Stop();
        audioSource.volume = FirecrackerData.DetonationVolumes[0];
        audioSource.PlayOneShot(FirecrackerData.DetonationSounds[0]);

        // deactivate any effects using a sound particle system
        foreach(SoundParticleSystem system in transform.parent.GetComponentsInChildren<SoundParticleSystem>())
        {
            system.emit = false;
            system.audioSource.Stop();
        }
    }
}
