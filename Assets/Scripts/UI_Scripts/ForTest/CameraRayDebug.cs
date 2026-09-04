using UnityEngine;

public class CameraRayDebug : MonoBehaviour
{
    public float rayDistance = 100f;

    void Update()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        // 화면 중앙 기준 레이
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 노란색 선으로 Scene View에 표시
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow);

        // 실제 Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }
}