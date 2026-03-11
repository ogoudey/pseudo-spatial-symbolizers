using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float lookSpeed = 2f;

    [Header("References")]
    private Transform playerCamera;

    private Rigidbody rb;

    private float yaw = 0f;
    private float pitch = 0f;

    private Vector3 moveInput;
    private bool jumpRequested = false;
    public bool movementEnabled = true;


    void Start()
    {
        
    }

    public void Go()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Camera cam = GetComponentInChildren<Camera>();

        if (cam != null)
        {
            playerCamera = cam.transform;
        }
        else
        {
            Debug.LogError("No Camera found as a child of this GameObject!");
        }
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Let physics handle movement, not rotation
    }

    void Update()
    {
        if (movementEnabled){
            // --- Mouse Look ---
            yaw += lookSpeed * Input.GetAxis("Mouse X");
            pitch -= lookSpeed * Input.GetAxis("Mouse Y");
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // --- Movement input ---
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            moveInput = transform.forward * v + transform.right * h;

            // --- Jump input ---
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            {
                jumpRequested = true;
            }
        }
    }

    private void FixedUpdate()
    {
        // --- Move the player ---
        Vector3 velocity = moveInput * moveSpeed;
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(velocity.x, currentVel.y, velocity.z);

        // --- Handle jump ---
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpRequested = false;
        }
    }

    private bool IsGrounded()
    {
        // Simple ground check using a raycast
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}

