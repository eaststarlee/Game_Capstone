using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;              // 총구(로컬 Z+가 앞)
    public Bullet bulletPrefab;           // 발사체 프리팹

    [Header("Shooting")]
    public float fireRate = 10f;          // 초당 발사수
    public float hipSpread = 2.5f;        // 비조준 분산(도)
    public float aimSpread = 0.5f;        // 조준 분산(도)
    public float maxRayDistance = 500f;   // 조준 레이 최대 거리
    public float minAimDistance = 3f;     // 너무 가까운 히트 보정
    public LayerMask hitMask = ~0;        // 조준/피격 레이어

    [Header("VFX/SFX (optional)")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSfx;

    [Header("Debug")]
    public bool fireFromMuzzleForward = false; // ON: 총구 Z+ 기준, OFF: 카메라 십자선 기준

    float nextFireTime = 0f;
    public bool CanFire => Time.time >= nextFireTime;

    public void Fire(Transform cameraTf, bool isAiming)
    {
        if (!CanFire || !muzzle || !bulletPrefab) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);

        // 발사 방향 계산
        Vector3 dir;
        if (fireFromMuzzleForward)
        {
            // TPS 리얼: 총구 로컬 Z+로 발사
            dir = muzzle.forward;
        }
        else
        {
            // FPS식: 카메라 중앙 레이캐스트 → aimPoint 향해 발사
            Vector3 aimPoint = GetAimPointFromCamera(cameraTf);
            dir = (aimPoint - muzzle.position).normalized;
        }

        // 분산 적용
        float spread = Mathf.Max(0f, isAiming ? aimSpread : hipSpread);
        if (spread > 0.0001f)
        {
            dir = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.up)
                * Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.right)
                * dir;
        }

        // 탄 생성 + 초기 회전/속도
        Bullet b = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));
        b.Fire(dir);

        // 효과
        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && fireSfx) audioSource.PlayOneShot(fireSfx);

        // 디버그 레이(2초 유지)
        Debug.DrawRay(muzzle.position, dir * 5f, Color.cyan, 2f);
    }

    Vector3 GetAimPointFromCamera(Transform camTf)
    {
        Camera cam = Camera.main ? Camera.main : (camTf ? camTf.GetComponent<Camera>() : null);
        if (cam)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, hitMask, QueryTriggerInteraction.Ignore))
                return (hit.distance < minAimDistance) ? ray.GetPoint(minAimDistance) : hit.point;
            return ray.GetPoint(maxRayDistance);
        }

        // Camera.main이 없을 때 백업
        Ray r2 = new Ray(camTf.position, camTf.forward);
        if (Physics.Raycast(r2, out RaycastHit h2, maxRayDistance, hitMask, QueryTriggerInteraction.Ignore))
            return h2.point;
        return r2.GetPoint(maxRayDistance);
    }

    // ★ 요청한 Gizmo: 씬/플레이 중 선택했을 때 총구 Z+ 시각화
    void OnDrawGizmosSelected()
    {
        if (!muzzle) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(muzzle.position, muzzle.position + muzzle.forward * 0.6f);
    }
}
