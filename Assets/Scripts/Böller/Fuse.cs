using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;

public class Fuse : MonoBehaviour
{
    [Header("Fuse Settings")]
    [Range(0.1f, 50.0f)]
    public float timeToDetonate; // in seconds
    [Header("Detonation Settings")]
    public float breakForce = 1.0f;
    [Description("Radius in which other Firecrackers will detonate too.")]
    public float detonateRadius = 2.0f;
    public LayerMask detonateMask;
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip detonationSound;
    public float volume = 0.5f;
    public float soundDelay = -0.2f;
    System.Random rnd;

    Material mat;
    [ReadOnly(true)]
    public bool activated = false;
    [ReadOnly(true)]
    public bool detonated = false;
    float fuseProgress = 0.0f;

    public GameObject FracturedPrefab;

    void Start()
    {
        rnd = new System.Random(DateTime.Now.Millisecond);
        audioSource.volume = volume;
        audioSource.clip = detonationSound;
        mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (activated && !detonated)
        {
            mat.SetInt("_FuseActive", 1);
            // calc time-step with timeToDetonate
            float fuseStep = (1.0f / timeToDetonate) * Time.deltaTime;
            if(fuseProgress < 1.0f)
            {
                fuseProgress += fuseStep;
                mat.SetFloat("_Transparency", 1.0f - fuseProgress);
            } 
            else
            {
                activated = true;
                detonated = true;
                Transform parent = transform.parent.transform;

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, detonateRadius, detonateMask);
                foreach(Collider col in hitColliders)
                {
                    // not on self
                    if (col.gameObject.transform.parent == gameObject.transform.parent) continue;

                    Fuse fuse = col.gameObject.transform.parent.Find("Fuse").GetComponent<Fuse>();
                    if (fuse.activated == true) continue;
                    fuse.timeToDetonate = rnd.Next(100, 2000) / 1000.0f;
                    fuse.ActivateFuse();
                }

                GameObject fractured = Instantiate(FracturedPrefab, parent.transform.position, parent.transform.rotation);
                foreach(Rigidbody rb in fractured.GetComponentsInChildren<Rigidbody>())
                {
                    Vector3 force = (rb.transform.position - transform.position).normalized * breakForce;
                    rb.AddForce(force);
                }

                foreach(Renderer renderer in transform.parent.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
                StartCoroutine("DestroyDelayed");
            }
        }
    }

    IEnumerator DestroyDelayed()
    {
        yield return new WaitForSeconds(detonationSound.length + 0.2f);

        Destroy(transform.parent.gameObject);
    }

    public void ActivateFuse()
    {
        // deactivate outline
        transform.parent.transform.GetComponent<Outline>().enabled = false;
        activated = true;
        audioSource.PlayDelayed(timeToDetonate + soundDelay);
    }
}
