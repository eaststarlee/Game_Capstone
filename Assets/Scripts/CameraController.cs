using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target;
    public LayerMask wallLayer;

    [Header("카메라 설정")]
    public Vector3 offset = new Vector3(0, 2f, -5f);
    public float rotationSpeed = 5f;
    public float zoomSpeed = 10f;
    public float minZoom = -2f;
    public float maxZoom = -15f;
    public float collisionOffset = 0.3f;

    [Header("카메라 기울기 (Wall Run 전용)")]
    public float tiltSpeed = 7f; // 시야가 기울어지는 속도
    private float currentTilt = 0f;

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentZoom;

    void Start()
    {
        currentZoom = offset.z;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentY = Mathf.Clamp(currentY, -10f, 80f);

        currentZoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, maxZoom, minZoom);
        offset.z = currentZoom;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, wallLayer))
        {
            transform.position = hit.point + hit.normal * collisionOffset;
        }
        else
        {
            transform.position = desiredPosition;
        }

        // 1. 기본 타겟 쳐다보기
        transform.LookAt(target.position);

        // 2. 🔥 벽타기용 카메라 기울기 덧씌우기
        PlayerController pc = target.GetComponent<PlayerController>();
        float finalTilt = 0f;

        if (pc != null && pc.isWallRunning)
        {
            finalTilt = pc.targetTilt; // 플레이어가 전달한 기울기 각도 
        }

        // 부드럽게 목표 각도로 보간
        currentTilt = Mathf.Lerp(currentTilt, finalTilt, Time.deltaTime * tiltSpeed);

        // 카메라 Z축을 회전시켜 고개가 꺾이는 파쿠르 연출 완성
        transform.rotation *= Quaternion.Euler(0, 0, currentTilt);
    }
}