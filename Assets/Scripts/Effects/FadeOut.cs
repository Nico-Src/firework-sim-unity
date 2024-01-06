using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public float fadeSpeed;
    float fadeVal = 1.0f;
    bool active = false;
    public bool destroyAfterFade = true;
    public bool activateOnStart = true;
    public float delay = 1.0f;
    public float destroyDelay = 0.2f;
    Material[] mats;

    private void Start()
    {
        mats = GetComponent<Renderer>().materials;
        if (activateOnStart && delay > 0.0f) StartCoroutine("delayedStart");
        else if (activateOnStart) StartTransition();
    }

    IEnumerator delayedStart()
    {
        yield return new WaitForSeconds(delay);

        StartTransition();
    }

    IEnumerator delayedDestroy()
    {
        yield return new WaitForSeconds(destroyDelay);

        Destroy(transform.parent.gameObject);
    }

    void StartTransition()
    {
        active = true;
    }

    void Update()
    {
        if (active)
        {
            if (fadeVal > 0.0f) fadeVal -= (Time.deltaTime * fadeSpeed);

            foreach(Material mat in mats)
            {
                mat.color = new Vector4(mat.color.r, mat.color.g, mat.color.b, fadeVal);
            }

            if(fadeVal <= 0.0f)
            {
                active = false;
                StartCoroutine("delayedDestroy");
            }
        }
    }
}
