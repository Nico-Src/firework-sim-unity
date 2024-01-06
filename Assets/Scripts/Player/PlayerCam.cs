using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.None) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        // apply mouse movement
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        // rotate cam 
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        // player rotation (only on y-axis)
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
