using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target & Collision")]
    public Transform target;                  // 보통 Player의 어깨 근처(Empty: CameraPivot)
    public LayerMask wallLayer = ~0;          // 충돌 방지용
    public float collisionOffset = 0.3f;      // 벽에 닿았을 때 띄우는 거리
    public float collisionRadius = 0.2f;      // 스피어캐스트 반경

    [Header("Orbit")]
    public float rotationSpeed = 3f;          // 마우스 감도
    public float minPitch = -30f;             // 위/아래 각 제한
    public float maxPitch = 60f;
    public float rotationSmoothTime = 0.05f;  // 회전 스무딩(지터 감소)

    [Header("Distance/Offsets")]
    public Vector3 defaultOffset = new Vector3(0f, 0.15f, -4.5f); // 평소
    public Vector3 aimOffset = new Vector3(0.6f, 0.15f, -2.0f); // 조준 시(오른쪽 어깨)
    public float followLerp = 15f;            // 위치 보간
    public float aimLerp = 12f;            // 오프셋/FOV 보간

    [Header("FOV")]
    public float defaultFOV = 60f;
    public float aimFOV = 40f;

    // 내부 상태
    Camera _cam;
    float _targetYaw, _targetPitch;
    float _yaw, _pitch;            // 스무딩된 값
    float _yawVel, _pitchVel;      // SmoothDampAngle용
    Vector3 _offset;               // 현재 오프셋
    bool _isAiming;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _offset = defaultOffset;
        if (_cam) _cam.fieldOfView = defaultFOV;

        Vector3 e = transform.eulerAngles;
        _targetYaw = _yaw = e.y;
        _targetPitch = _pitch = Mathf.Clamp(e.x, minPitch, maxPitch);
    }

    public void SetTarget(Transform t) => target = t;
    public void SetAiming(bool aiming) => _isAiming = aiming;

    void Update()
    {
        if (!target) return;

        // 입력은 Update에서 받아두고
        _targetYaw += Input.GetAxisRaw("Mouse X") * rotationSpeed;
        _targetPitch -= Input.GetAxisRaw("Mouse Y") * rotationSpeed;
        _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);

        // 오프셋/FOV 전환(조준↔일반) 스무딩
        Vector3 targetOffset = _isAiming ? aimOffset : defaultOffset;
        _offset = Vector3.Lerp(_offset, targetOffset, aimLerp * Time.deltaTime);

        float targetFov = _isAiming ? aimFOV : defaultFOV;
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, aimLerp * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (!target) return;

        // 회전은 LateUpdate에서 스무딩 적용(지터 감소)
        _yaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVel, rotationSmoothTime);
        _pitch = Mathf.SmoothDampAngle(_pitch, _targetPitch, ref _pitchVel, rotationSmoothTime);
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);

        // 충돌 보정
        Vector3 desiredPos = target.position + rot * _offset;
        Vector3 finalPos = desiredPos;

        Vector3 dir = desiredPos - target.position;
        float dist = dir.magnitude;
        if (dist > 0.0001f)
        {
            dir /= dist;
            if (Physics.SphereCast(target.position, collisionRadius, dir, out RaycastHit hit, dist, wallLayer, QueryTriggerInteraction.Ignore))
                finalPos = hit.point + hit.normal * collisionOffset;
        }

        // 위치/회전 적용 (LookAt 사용 X)
        transform.position = Vector3.Lerp(transform.position, finalPos, followLerp * Time.deltaTime);
        transform.rotation = rot;
    }
}