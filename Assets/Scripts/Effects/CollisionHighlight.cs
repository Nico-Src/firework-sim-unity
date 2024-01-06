using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHighlight : MonoBehaviour
{
    public bool isColliding;
    Color originalColor;
    public Color highlightColor;
    Outline outline;

    private void Start()
    {
        outline = GetComponent<Outline>();
        originalColor = outline.OutlineColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        outline.OutlineColor = highlightColor;
        isColliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        outline.OutlineColor = originalColor;
        isColliding = false;
    }
}
