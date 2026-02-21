using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float surfaceSpeedMultiplier = 1f;

    public float gravity = -9.81f;
    public float jumpHeight = 1f;

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

    [Header("Sound Effects")]
    public AudioClip[] jumpSfx;
    [Range(0f, 1f)] public float jumpSfxVolume = 1f;
    public AudioClip walkSfx;
    [Range(0f, 1f)] public float walkSfxVolume = 1f;

    [HideInInspector] public bool inputEnabled = true;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    [HideInInspector] public float surfaceJumpMultiplier = 1f;
    [HideInInspector] public float surfaceGravityMultiplier = 1f;

    // Blue: 슈퍼점프 
    private bool superJumpEnabled = false;
    private float superJumpForce = 40f;

    // Yellow: 벽달리기
    private bool wallRunEnabled = false;
    private float wallRunUpSpeed = 4f;
    private float wallRunGrav = -3f;
    private float wallCheckDist = 0.6f;
    private LayerMask wallMask;

    public bool isWallRunning { get; private set; } = false;

    private AudioSource walkAudioSource;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerBody == null)
            playerBody = transform;

        // 걷기 전용 오디오 소스 동적 생성 (무한 반복 세팅)
        walkAudioSource = gameObject.AddComponent<AudioSource>();
        walkAudioSource.loop = true;
        walkAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (!inputEnabled) return;

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move =
            playerBody.right * x +
            playerBody.forward * z;

        controller.Move(move * speed * surfaceSpeedMultiplier * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            // 🔸 기본 점프 (이제 파란 잉크는 여기 안 타고, InkArea에서 바로 ForceJump로 처리)
            if (isGrounded)
            {
                float effectiveJump = jumpHeight * surfaceJumpMultiplier;
                velocity.y = Mathf.Sqrt(effectiveJump * -2f * gravity);
                PlayJumpSound();
            }
            else if (superJumpEnabled)
            {
                // 혹시 다른 용도로 쓸 수도 있으니 남겨둔 로직
                velocity.y = superJumpForce;
                PlayJumpSound();
            }
        }

        if (wallRunEnabled && !isGrounded && IsNearWall())
        {
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = Mathf.Max(velocity.y, wallRunUpSpeed);
            }

            velocity.y += wallRunGrav * Time.deltaTime;
        }
        else
        {
            float effectiveGravity = gravity * surfaceGravityMultiplier;
            velocity.y += effectiveGravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);

        // --- 발소리 처리 (무한 반복 모드) ---
        bool isMoving = (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f);
        if (isGrounded && isMoving)
        {
            if (!walkAudioSource.isPlaying && walkSfx != null)
            {
                walkAudioSource.clip = walkSfx;
                walkAudioSource.volume = walkSfxVolume;
                walkAudioSource.Play();
            }
        }
        else
        {
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
        }
    }

    // === 🔵 파란 잉크 전용: 강제 점프 메서드 ===
    public void ForceJump(float force)
    {
        // 언제든 Y속도를 이 값으로 갈아끼워 바로 점프
        velocity.y = force;
    }

    // === Blue: 예전 슈퍼점프용 API(혹시 다른 데서 쓸 수도 있으니 유지) ===
    public void EnableSuperJump(float force)
    {
        superJumpEnabled = true;
        superJumpForce = force;
    }

    public void DisableSuperJump()
    {
        superJumpEnabled = false;
    }

    // === Yellow: 벽달리기 제어 ===
    public void EnableWallRun(bool enable, float upSpeed, float gravWhileRun, float checkDist, LayerMask mask)
    {
        wallRunEnabled = enable;
        wallRunUpSpeed = upSpeed;
        wallRunGrav = gravWhileRun;
        wallCheckDist = checkDist;
        wallMask = mask;
        isWallRunning = enable; //비네트 효과 적용 관련 받을 내용
    }

    private bool IsNearWall()
    {
        Transform t = playerBody != null ? playerBody : transform;

        return Physics.Raycast(t.position, t.right, wallCheckDist, wallMask)
            || Physics.Raycast(t.position, -t.right, wallCheckDist, wallMask);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    private void PlayJumpSound()
    {
        if (jumpSfx != null && jumpSfx.Length > 0)
        {
            int index = Random.Range(0, jumpSfx.Length);
            AudioClip clip = jumpSfx[index];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, jumpSfxVolume);
            }
        }
    }
}
