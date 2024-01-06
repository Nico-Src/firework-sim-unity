using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement - Walking / Sprinting")]
    private float moveSpeed;
    [Range(0.0f, 100.0f)]
    public float walkSpeed;
    [Range(0.0f, 100.0f)]
    public float sprintSpeed;
    [Range(0.0f, 100.0f)]
    public float groundDrag;

    [Header("Movement - Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Zoom")]
    public float zoomSpeed;
    float defaultFOV;
    public float zoomedInFOVMultiplier = 20.0f;

    [Header("UI")]
    public GameObject crosshair;
    public GameObject toolSelectWindow;

    [Header("Place Settings")]
    public float placeDistance;
    public GameObject placePrefab;
    public GameObject placeHolderPrefab;
    GameObject placeholder;

    [Header("Selection")]
    public LayerMask selectMask;
    Outline selected;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundMask;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;
    MovementState state;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode zoomKey = KeyCode.C;
    public KeyCode switchToolKey = KeyCode.Tab;

    public MouseButton activateButton = MouseButton.LeftMouse;
    public MouseButton placeButton = MouseButton.RightMouse;

    [Header("Debug")]
    [ReadOnly(true)]
    public float curVel;
    [ReadOnly(true)]
    public bool readyToJump = false;
    [ReadOnly(true)]
    public bool grounded = false;
    [ReadOnly(true)]
    public Tool currentTool = Tool.Lighter;

    public enum MovementState
    {
        Walking,
        Sprinting,
        Air
    }

    public enum Tool
    {
        Lighter,
        Place
    }

    private void Start()
    {
        defaultFOV = Camera.main.fieldOfView;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        placeholder = Instantiate(placeHolderPrefab, Vector3.one * -100, Quaternion.identity);
        placeholder.gameObject.SetActive(false);
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        GetInput();
        SpeedLimiter();
        StateHandler();
        Raycast();

        // apply drag if player is grounded
        if (grounded)
        {
            rb.drag = groundDrag;
        } 
        else
        {
            rb.drag = 0;
        }

        if(currentTool == Tool.Place && placeholder != null)
        {
            placeholder.SetActive(true);
            placeholder.transform.position = transform.position + Camera.main.transform.forward * placeDistance;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Raycast()
    {
        // calculate cam center
        Vector3 camCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
        // check if there is an selectable object in the center (forward direction of the cam)
        if (Physics.Raycast(camCenter, Camera.main.transform.forward, out RaycastHit hit, 100, selectMask))
        {
            // if object has outline component enable it
            if (hit.transform.TryGetComponent<Outline>(out Outline outline))
            {
                Fuse fuse = outline.transform.Find("Fuse").GetComponent<Fuse>();
                // only if isnt already selected
                if (outline == selected) return;
                // if selected is not null (something is selected other than the current outline) disable outline of the selected
                if (selected != null) selected.enabled = false;
                // enable outline (only if fuse hasnt been activated or detonated)
                if(fuse.activated == false && fuse.detonated == false) outline.enabled = true;
                // set selected
                selected = outline;
            }
        }
        // if there isnt disable outline of the last selected
        else if(selected != null)
        {
            selected.enabled = false;
            selected = null;
        }

        if (Input.GetMouseButtonDown((int)placeButton) && currentTool == Tool.Place)
        {
            Instantiate(placePrefab, transform.position + Camera.main.transform.forward * placeDistance, Quaternion.identity);
        }
    }

    void StateHandler()
    {
        // sprinting
        if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        // walking
        else if (grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
        // in air
        else
        {
            state = MovementState.Air;
        }
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetMouseButtonDown((int)activateButton) && selected != null)
        {
            Fuse fuse = selected.transform.Find("Fuse").GetComponent<Fuse>();
            fuse.ActivateFuse();
        }

        if (Input.GetKey(zoomKey))
        {
            if (Camera.main.fieldOfView > (defaultFOV - zoomedInFOVMultiplier)) Camera.main.fieldOfView -= zoomSpeed * Time.deltaTime;
            else Camera.main.fieldOfView = defaultFOV - zoomedInFOVMultiplier;
        }
        else
        {
            if (Camera.main.fieldOfView < defaultFOV) Camera.main.fieldOfView += zoomSpeed * Time.deltaTime;
            else Camera.main.fieldOfView = defaultFOV;
        }

        if (Input.GetKeyDown(switchToolKey)) ToggleToolSelect();
    }

    private void ToggleToolSelect()
    {
        // toggle tool select window
        toolSelectWindow.SetActive(!toolSelectWindow.activeSelf);
        // hide crosshair if tool select window is active and show if not
        crosshair.SetActive(!toolSelectWindow.activeSelf);
        // set cursor state
        if (toolSelectWindow.activeSelf)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    public void SwitchTool(int tool)
    {
        currentTool = (Tool)tool;

        // if current tool is not the place tool hide placeholder
        if (currentTool != Tool.Place) placeholder.SetActive(false);
        // else placeholder is not set instantiate it
        else if (placeholder == null)
        {
             placeholder = Instantiate(placeholder);
        }

        ToggleToolSelect();
    }

    private void Move()
    {
        // calc move direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        // on ground
        if (grounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedLimiter()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        curVel = flatVel.magnitude;

        if(curVel > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset v velocity
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
