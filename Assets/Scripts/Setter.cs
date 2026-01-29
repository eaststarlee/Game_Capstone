using UnityEngine;

public class Setter : MonoBehaviour
{
    [Header("Decal Settings")]
    public float insetDistance = 0.01f; // 면 안쪽으로 살짝 이동
    public float maxZRotationOffset = 45f; // 회전 보정 최대 각도

    // 데칼을 면에 맞춰 정렬 (법선 평균 + 회전/위치 보정)
    public void AlignDecalToSurface(GameObject decal, Vector3 hitPoint, Vector3 hitNormal, Vector3 referenceDir, Collider hitCollider = null)
    {
        if (decal == null) return;

        // 1) 기본 위치
        Vector3 positionOffset = hitNormal * insetDistance;
        decal.transform.position = hitPoint + positionOffset;

        // 2) Forward = 평균 법선
        Vector3 forward = -hitNormal;

        if (hitCollider != null)
        {
            // MeshFilter가 있으면 전체 법선 평균
            MeshFilter mf = hitCollider.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3 avgNormal = Vector3.zero;
                foreach (var n in mesh.normals)
                    avgNormal += n;
                avgNormal.Normalize();
                forward = -avgNormal; // 평균 법선 반대 방향
            }
        }

        // 3) Up 벡터 계산
        Vector3 up;
        if (referenceDir == Vector3.zero)
        {
            up = Vector3.up;
            if (Vector3.Dot(up, forward) > 0.99f)
                up = Vector3.Cross(forward, Vector3.right).normalized;
        }
        else
        {
            up = Vector3.Cross(Vector3.Cross(forward, referenceDir), forward).normalized;
        }

        // 4) 회전 적용
        Quaternion baseRotation = Quaternion.LookRotation(forward, up);

        // 5) 모서리/각진 면 보정: z축 ± 회전 추가
        float zOffset = Random.Range(-maxZRotationOffset, maxZRotationOffset);
        Quaternion zRotation = Quaternion.Euler(0f, 0f, zOffset);

        decal.transform.rotation = baseRotation * zRotation;

        // 6) 추가 위치 미세조정 (모서리 보정)
        if (referenceDir != Vector3.zero)
        {
            Vector3 sideOffset = Vector3.Cross(hitNormal, referenceDir).normalized * (insetDistance * 0.2f);
            decal.transform.position += sideOffset;
        }
    }
}
