using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Color to transition from")]
    public Color originalColor;
    [Tooltip("Color to transition to")]
    public Color targetColor;
    private Image img;
    // transition progress
    private float progress;
    // boolean to check if we are entering or exiting the transition
    private bool enter;
    private bool exit;
    [Tooltip("Transition Speed")]
    public float speed;

    void Start()
    {
        img = GetComponentInChildren<Image>();
    }

    void Update()
    {
        // enter transition
        if (enter)
        {
            // increase till progress reaches 1
            if (progress < 1.0f)
            {
                progress += speed * Time.deltaTime;
                // lerp to target color
                img.color = Color.Lerp(originalColor, targetColor, progress);
            }
            // else set progress to 1, enter to false and color to target color
            else
            {
                progress = 1;
                enter = false;
                img.color = targetColor;
            }
        }

        // exit transition
        if (exit)
        {
            // increase till progress reaches 1
            if (progress < 1.0f)
            {
                progress += speed * Time.deltaTime;
                // lerp to original color
                img.color = Color.Lerp(targetColor, originalColor, progress);
            }
            // else set progress to 1, enter to false and color to original color
            else
            {
                progress = 1;
                exit = false;
                img.color = originalColor;
            }
        }

        if (!enter && !exit) img.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        enter = true;
        progress = 0;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exit = true;
        progress = 0;
    }
}
