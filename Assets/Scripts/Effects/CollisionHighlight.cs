using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHighlight : MonoBehaviour
{
    bool isColliding;

    /// <summary>
    /// Unhighlighted Color
    /// </summary>
    Color originalColor;

    /// <summary>
    /// Highlight Color
    /// </summary>
    public Color highlightColor;

    // Outline reference
    Outline outline;

    private void Start()
    {
        outline = GetComponent<Outline>();
        originalColor = outline.OutlineColor;
    }

    /// <summary>
    /// Highlight whenever it enters another collider
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        outline.OutlineColor = highlightColor;
        isColliding = true;
    }

    /// <summary>
    /// Revert back to normal color after it exits other collider
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        outline.OutlineColor = originalColor;
        isColliding = false;
    }
}
