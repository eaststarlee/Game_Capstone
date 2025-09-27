using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;    // 메인 카메라
    public Animator animator;            // 연결된 Animator (in-place 애니메이션: Apply Root Motion 끄기)

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
        // Animator에서 Apply Root Motion 꺼둘 것(코드로 이동 제어)
        if (animator != null) animator.applyRootMotion = false;
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
        isSprinting = allowSprint && Input.GetKey(sprintKey) && inputMag > 0.1f;
    }

    void HandleGroundCheck()
    {
        // CharacterController.isGrounded 사용 (간단하고 일반적)
        isGrounded = cc.isGrounded;
        // 바닥에 붙어있다면 작은 음수로 고정(중력 누적 방지)
        if (isGrounded && vertVelocityY < 0f) vertVelocityY = -2f;
    }

    void HandleMovement()
    {
        float targetSpeed = (isSprinting ? runSpeed : walkSpeed) * inputMag;
        float smoothTime = 1f / Mathf.Max(0.0001f, acceleration);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);

        if (inputMag >= 0.01f)
        {
            float cameraYaw = cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, cameraYaw, 0f) * inputDir;

            // 부드러운 회전 (전방 방향으로)
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 horizontalMove = moveDir.normalized * currentSpeed;
            Vector3 totalMove = horizontalMove + Vector3.up * vertVelocityY;
            cc.Move(totalMove * Time.deltaTime);
        }
        else
        {
            // 정지 상태: 수직 이동(중력)만 적용
            cc.Move(Vector3.up * vertVelocityY * Time.deltaTime);
            // 속도가 0에 수렴
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref speedVelocity, 0.08f);
        }
    }

    void ApplyGravityAndJump()
    {
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            // 점프 속도 계산: v = sqrt(2 * g * h)
            vertVelocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Animator에 Jump 트리거 전송
            animator?.SetTrigger(paramJump);
            // isGrounded는 다음 프레임에 false가 될 수 있음(Physics)
        }

        // 중력 누적
        vertVelocityY += gravity * Time.deltaTime;
    }

    void UpdateAnimatorParams()
    {
        if (animator == null) return;

        // Speed는 0..1로 정규화 (runSpeed 기준)
        float speedNormalized = runSpeed > 0f ? Mathf.Clamp01(currentSpeed / runSpeed) : 0f;
        animator.SetFloat(paramSpeed, speedNormalized);

        // isWalk: 이동 중이면서 달리는 상태가 아닌 경우
        bool walkState = inputMag > 0.01f && !isSprinting;
        animator.SetBool(paramIsWalk, walkState);

        // isRun: 스프린트 중
        animator.SetBool(paramIsRun, isSprinting);

        // Isgrounded: 현재 접지 여부
        animator.SetBool(paramIsGrounded, isGrounded);

        // *Optional debug landing detection* (Animator 쪽에서 FallingIdle->Landing를 Isgrounded==true 조건으로 설정하면 된다)
        if (!prevGrounded && isGrounded)
        {
            // 착지 감지: Animator의 조건(Isgrounded true)이 활성화되면 Landing 상태로 전환되게 세팅
            // (여기서는 별도의 트리거를 쓰지 않고 Isgrounded bool로 전환을 제어함)
        }
    }

    // 디버그용: 바닥 체크 시각화 (Inspector에서 보려면 플레이 중에 선택)
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Gizmos.DrawWireSphere(origin + Vector3.down * 0.1f, 0.35f);
    }
}
