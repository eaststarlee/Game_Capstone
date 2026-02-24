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

    [Header("Camera Ownership")]
    [Tooltip("별도 CameraController가 카메라+플레이어 회전을 담당하면 true")]
    public bool useExternalCameraController = true;

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

    // Yellow: 벽 부착(착붙)
    private bool wallRunEnabled = false;
    private float wallRunUpSpeed = 0f;     // 호환용 유지 (자동상승은 사용 안 함)
    private float wallRunGrav = -2.5f;     // 벽에 붙어있을 때 낙하 완화
    private float wallCheckDist = 0.6f;
    private LayerMask wallMask;

    [Header("Wall Run Advanced")]
    public float maxWallTilt = 15f;         // 벽타기 카메라 기울기 최대 각도
    public float wallStickForce = 0.03f;    // 너무 크면 밀려나는 느낌
    public float wallRunMoveMultiplier = 1.0f;
    public float wallJumpHorizontalForce = 4.0f;
    public float wallJumpVerticalBoost = 2.6f;
    public float wallRunExitLockTime = 0.12f;

    [Header("Wall Camera Tilt")]
    public float wallTiltSmooth = 10f;      // 기울기 보간 속도
    private float currentTilt = 0f;         // 실제 카메라에 적용되는 롤 값

    [Header("Yellow Ink Contact Smoothing")]
    public float yellowInkGraceTime = 0.14f;  // 데칼 여러 개 깜빡임 방지

    [HideInInspector] public float targetTilt = 0f;

    private Vector3 wallNormal = Vector3.zero;
    public bool isWallRunning { get; private set; } = false;

    private AudioSource walkAudioSource;
    private float wallRunReattachTimer = 0f;

    // 노란 잉크 접촉 스무딩
    private float lastYellowInkContactTime = -999f;
    private bool yellowInkContactActive = false;

    // 노란 잉크 파라미터 캐시
    private float cachedWallRunUpSpeed = 0f;
    private float cachedWallRunGrav = -2.5f;
    private float cachedWallCheckDist = 0.6f;
    private LayerMask cachedWallMask;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerBody == null) playerBody = transform;

        walkAudioSource = gameObject.AddComponent<AudioSource>();
        walkAudioSource.loop = true;
        walkAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (!inputEnabled) return;

        if (wallRunReattachTimer > 0f)
            wallRunReattachTimer -= Time.deltaTime;

        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        if (useExternalCameraController)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 벽타기 기울기 보간
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, wallTiltSmooth * Time.deltaTime);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            // ✅ 핵심 수정: 착지하면 벽점프의 가로 잔류 속도 제거
            velocity.x = 0f;
            velocity.z = 0f;

            if (velocity.y < 0f)
                velocity.y = -2f;
        }

        // 노란 잉크 접촉 grace-time 처리
        bool yellowStillValid = (Time.time - lastYellowInkContactTime) <= yellowInkGraceTime;

        if (!yellowStillValid && wallRunEnabled)
        {
            EnableWallRun(false, 0f, 0f, 0f, 0);
            yellowInkContactActive = false;
        }
        else if (yellowStillValid && yellowInkContactActive)
        {
            EnableWallRun(true, cachedWallRunUpSpeed, cachedWallRunGrav, cachedWallCheckDist, cachedWallMask);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 basisRight = (playerBody != null) ? playerBody.right : transform.right;
        Vector3 basisForward = (playerBody != null) ? playerBody.forward : transform.forward;

        Vector3 rawMove = (basisRight * x + basisForward * z);
        if (rawMove.sqrMagnitude > 1f) rawMove.Normalize();

        bool canWallStickNow = wallRunEnabled && !isGrounded && wallRunReattachTimer <= 0f && IsNearWall();

        if (canWallStickNow)
        {
            isWallRunning = true;
            float dt = Time.deltaTime;

            // 플레이어 입력만 사용 (벽면에 투영)
            Vector3 wallMoveDir = Vector3.ProjectOnPlane(rawMove, wallNormal);
            if (wallMoveDir.sqrMagnitude > 1f)
                wallMoveDir.Normalize();

            // 접착은 아주 약하게
            Vector3 antiDetach = -wallNormal * Mathf.Clamp(wallStickForce, 0f, 0.06f);

            // 자동 상승 제거, 낙하만 완화
            velocity.y += wallRunGrav * dt;
            velocity.y = Mathf.Max(velocity.y, -2.2f);

            // wall jump로 인해 들어갔던 xz 잔류속도는 벽주행 중엔 쓰지 않음
            velocity.x = 0f;
            velocity.z = 0f;

            Vector3 wallMove =
                wallMoveDir * (speed * surfaceSpeedMultiplier * wallRunMoveMultiplier)
                + Vector3.up * velocity.y
                + antiDetach;

            controller.Move(wallMove * dt);

            // 벽 점프
            if (Input.GetButtonDown("Jump"))
            {
                velocity = Vector3.zero;
                velocity += wallNormal * wallJumpHorizontalForce;
                velocity.y = wallJumpVerticalBoost;

                isWallRunning = false;
                targetTilt = 0f;
                wallRunReattachTimer = wallRunExitLockTime;
                PlayJumpSound();
            }
        }
        else
        {
            isWallRunning = false;
            targetTilt = 0f;

            // 일반 이동
            controller.Move(rawMove * speed * surfaceSpeedMultiplier * Time.deltaTime);

            // 일반/슈퍼점프
            if (Input.GetButtonDown("Jump"))
            {
                if (isGrounded)
                {
                    float effectiveJump = jumpHeight * surfaceJumpMultiplier;
                    velocity.y = Mathf.Sqrt(effectiveJump * -2f * gravity);
                    PlayJumpSound();
                }
                else if (superJumpEnabled)
                {
                    velocity.y = superJumpForce;
                    PlayJumpSound();
                }
            }

            float effectiveGravity = gravity * surfaceGravityMultiplier;
            velocity.y += effectiveGravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        // 발소리
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
            if (walkAudioSource.isPlaying) walkAudioSource.Stop();
        }
    }

    public void ForceJump(float force) { velocity.y = force; }
    public void EnableSuperJump(float force) { superJumpEnabled = true; superJumpForce = force; }
    public void DisableSuperJump() { superJumpEnabled = false; }

    public void EnableWallRun(bool enable, float upSpeed, float gravWhileRun, float checkDist, LayerMask mask)
    {
        wallRunEnabled = enable;
        wallRunUpSpeed = upSpeed;
        wallRunGrav = gravWhileRun;
        wallCheckDist = checkDist;
        wallMask = mask;
    }

    public void SetWallRunSurface(Vector3 normal)
    {
        if (normal.sqrMagnitude > 0.0001f)
            wallNormal = normal.normalized;
    }

    public void RegisterYellowInkContact(Vector3 normal, float upSpeed, float gravWhileRun, float checkDist, LayerMask mask)
    {
        yellowInkContactActive = true;
        lastYellowInkContactTime = Time.time;

        if (normal.sqrMagnitude > 0.0001f)
            wallNormal = normal.normalized;

        cachedWallRunUpSpeed = upSpeed;
        cachedWallRunGrav = gravWhileRun;
        cachedWallCheckDist = checkDist;
        cachedWallMask = mask;

        EnableWallRun(true, upSpeed, gravWhileRun, checkDist, mask);
    }

    public void NotifyYellowInkExit()
    {
        // 즉시 OFF 금지 (grace-time 만료 시 자동 해제)
    }

    private bool IsNearWall()
    {
        Transform t = playerBody != null ? playerBody : transform;
        Vector3 origin = t.position;

        bool hitRight = Physics.Raycast(origin, t.right, out RaycastHit hit, wallCheckDist, wallMask, QueryTriggerInteraction.Ignore);
        if (hitRight)
        {
            wallNormal = hit.normal.normalized;
            targetTilt = +maxWallTilt;
            return true;
        }

        bool hitLeft = Physics.Raycast(origin, -t.right, out hit, wallCheckDist, wallMask, QueryTriggerInteraction.Ignore);
        if (hitLeft)
        {
            wallNormal = hit.normal.normalized;
            targetTilt = -maxWallTilt;
            return true;
        }

        targetTilt = 0f;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);

        Transform t = playerBody != null ? playerBody : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(t.position, t.position + t.right * wallCheckDist);
        Gizmos.DrawLine(t.position, t.position - t.right * wallCheckDist);
    }

    public void ResetVelocity() { velocity = Vector3.zero; }

    private void PlayJumpSound()
    {
        if (jumpSfx != null && jumpSfx.Length > 0)
        {
            int index = Random.Range(0, jumpSfx.Length);
            AudioClip clip = jumpSfx[index];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position, jumpSfxVolume);
        }
    }
}