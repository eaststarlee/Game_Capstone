using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 12f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100f;
    public float jumpHeight = 3f;

    public Transform playerBody;
    public Camera playerCamera;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Weapon Link")]
    public WeaponSway weaponSway;

    [HideInInspector]
    public bool inputEnabled = true; // 입력 허용 여부

    private CharacterController controller;
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) controller = gameObject.AddComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCamera != null && playerBody != null)
        {
            playerCamera.transform.SetParent(playerBody);
            playerCamera.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (!wasGrounded && isGrounded)
            weaponSway?.ApplyLandingBob();

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (inputEnabled)
        {
            // --- 이동 ---
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);

            // --- 점프 ---
            if (Input.GetButtonDown("Jump") && isGrounded)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // --- 마우스 회전 ---
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (playerBody != null)
                playerBody.Rotate(Vector3.up * mouseX);
        }

        // --- 중력 ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
