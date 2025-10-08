
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target; // 카메라가 따라다닐 대상 (플레이어)
    public LayerMask wallLayer; // 벽으로 인식할 레이어

    [Header("카메라 설정")]
    public Vector3 offset = new Vector3(0, 2f, -5f); // 타겟으로부터의 기본 거리
    public float rotationSpeed = 5f; // 마우스로 카메라 회전 속도
    public float zoomSpeed = 10f; // 마우스 휠로 줌 속도
    public float minZoom = -2f; // 최소 줌 거리 (타겟보다 앞으로 감)
    public float maxZoom = -15f; // 최대 줌 거리
    public float collisionOffset = 0.3f; // 카메라가 벽에 충돌 시 떨어질 거리

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentZoom;

    void Start()
    {
        currentZoom = offset.z;
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 마우스 입력 받기
        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, -10f, 80f); // 상하 각도 제한

        // 줌 처리
        currentZoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, maxZoom, minZoom);
        offset.z = currentZoom;

        // 카메라 회전 계산
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 카메라 위치 계산 (회전 * 오프셋 + 타겟 위치)
        Vector3 desiredPosition = target.position + rotation * offset;

        // --- 카메라 충돌 처리 ---
        // 타겟에서 카메라가 있어야 할 위치까지 레이를 쏴서, 그 사이에 벽(wallLayer)이 있는지 확인
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, wallLayer))
        {
            // 벽이 감지되면, 카메라 위치를 충돌 지점에서 약간 앞으로 당김
            transform.position = hit.point + hit.normal * collisionOffset;
        }
        else
        {
            // 벽이 없으면 원하는 위치로 이동
            transform.position = desiredPosition;
        }

        // 항상 타겟을 바라보도록 설정
        transform.LookAt(target.position);
    }
}
