using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;    // 메인 카메라
    public Animator animator;            // 연결된 Animator (in-place 애니메이션: Apply Root Motion 끄기)

    // ★ 추가: 조준 상태를 알기 위한 참조(인스펙터에 PlayerWeaponController 드래그)
    [Header("Combat / Aim")]
    public PlayerWeaponController weaponController;
    [Tooltip("조준(ADS) 중 이동 속도 배율")]
    public float aimSpeedMultiplier = 0.6f;

    [Header("Move Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float acceleration = 10f;     // 속도 보간 정도
    public float rotationSmoothTime = 0.12f;

    [Header("Jump & Gravity")]
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("Controls")]
    public bool allowSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Animator param names (정확히 일치시킬 것)")]
    public string paramIsWalk = "isWalk";
    public string paramIsRun = "isRun";
    public string paramSpeed = "Speed";
    public string paramIsGrounded = "Isgrounded"; // 네가 준 이름 그대로 (대소문자 주의)
    public string paramJump = "Jump";

    // 내부 상태
    private CharacterController cc;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;
    private float turnSmoothVelocity = 0f;
    private float vertVelocityY = 0f;

    private Vector3 inputDir = Vector3.zero;
    private float inputMag = 0f;
    private bool isSprinting = false;

    private bool isGrounded = true;
    private bool prevGrounded = true;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false; // 이동은 코드로 제어
    }

    void Update()
    {
        ReadInput();
        HandleGroundCheck();
        HandleMovement();
        ApplyGravityAndJump();
        UpdateAnimatorParams();
        prevGrounded = isGrounded;
    }

    void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(x, 0f, z).normalized;
        inputMag = inputDir.magnitude;

        // ★ 조준 중에는 스프린트 금지(원하면 허용해도 됨)
        bool aiming = weaponController && weaponController.IsAiming;
        isSprinting = !aiming && allowSprint && Input.GetKey(sprintKey) && inputMag > 0.1f;
    }

    void HandleGroundCheck()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded && vertVelocityY < 0f) vertVelocityY = -2f; // 중력 누적 방지
    }

    void HandleMovement()
    {
        bool aiming = weaponController && weaponController.IsAiming;

        float baseSpeed = (isSprinting ? runSpeed : walkSpeed);
        if (aiming) baseSpeed *= aimSpeedMultiplier;           // ★ 조준 중 속도 낮춤

        float targetSpeed = baseSpeed * inputMag;
        float smoothTime = 1f / Mathf.Max(0.0001f, acceleration);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);

        if (inputMag >= 0.01f)
        {
            float cameraYaw = cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, cameraYaw, 0f) * inputDir; // 카메라 기준 스트레이프

            // ★ 조준 중에는 이 스크립트가 회전을 덮어쓰지 않는다
            //    (플레이어 회전은 PlayerWeaponController.LateUpdate에서 카메라 yaw로 맞춤)
            if (!aiming)
            {
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            Vector3 horizontalMove = moveDir.normalized * currentSpeed;
            Vector3 totalMove = horizontalMove + Vector3.up * vertVelocityY;
            cc.Move(totalMove * Time.deltaTime);
        }
        else
        {
            // 정지 상태: 수직 이동(중력)만 적용
            cc.Move(Vector3.up * vertVelocityY * Time.deltaTime);
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref speedVelocity, 0.08f);
        }
    }

    void ApplyGravityAndJump()
    {
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            vertVelocityY = Mathf.Sqrt(jumpHeight * -2f * gravity); // v = sqrt(2*g*h)
            animator?.SetTrigger(paramJump);
        }
        vertVelocityY += gravity * Time.deltaTime;
    }

    void UpdateAnimatorParams()
    {
        if (animator == null) return;

        float speedNormalized = runSpeed > 0f ? Mathf.Clamp01(currentSpeed / runSpeed) : 0f;
        animator.SetFloat(paramSpeed, speedNormalized);

        bool walkState = inputMag > 0.01f && !isSprinting;
        animator.SetBool(paramIsWalk, walkState);
        animator.SetBool(paramIsRun, isSprinting);
        animator.SetBool(paramIsGrounded, isGrounded);
    }

    // 디버그용: 바닥 체크 시각화
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Gizmos.DrawWireSphere(origin + Vector3.down * 0.1f, 0.35f);
    }
}