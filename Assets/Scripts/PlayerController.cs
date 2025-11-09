using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 12f;
    [Range(0.1f, 20f)] public float surfaceSpeedMultiplier = 1f;  // 잉크 등 효과 배율
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;

    [Header("References")]
    public Transform playerBody;
    public Camera playerCamera;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Weapon Link")]
    public WeaponSway weaponSway;

    [HideInInspector] public bool inputEnabled = true;

    private CharacterController controller;
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;

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

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (inputEnabled)
        {
            HandleRotation();
            HandleMovement();
        }

        // 중력
        velocity.y += gravity * Time.deltaTime;

        // ✅ 이동과 중력을 한 번의 Move로 처리 (핵심)
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // ✅ 여기서 multiplier 적용
        Vector3 horizontal = move * speed * surfaceSpeedMultiplier;

        // ✅ 중력 벡터에 더해 최종 이동벡터 계산
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
