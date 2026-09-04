using UnityEngine;

public class GhostPath : MonoBehaviour
{
    [Tooltip("경로를 구성하는 웨이포인트(Transform) 배열입니다.")]
    public Transform[] waypoints;

    [Tooltip("에디터에서 보일 선의 색상")]
    public Color pathColor = Color.red;

    // OnDrawGizmos는 에디터의 Scene 뷰에서만 그려지며 실제 게임 화면에선 렌더링되지 않습니다.
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                // 웨이포인트끼리 선으로 연결 (선형 경로)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                // 웨이포인트 위치에 보기 편하게 작은 구체 표시
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            }
        }

        // 마지막 웨이포인트 구체 표시
        if (waypoints.Length > 0 && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.2f);
        }
    }
}