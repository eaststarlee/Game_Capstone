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
    public LayerMask hitMask = ~0;        // 조준/피격 레이어(플레이어/무기/UI 제외)

    [Header("VFX/SFX (optional)")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSfx;

    float nextFireTime = 0f;
    public bool CanFire => Time.time >= nextFireTime;

    public void Fire(Transform cameraTf, bool isAiming)
    {
        if (!CanFire || !muzzle || !bulletPrefab) return;
        nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);

        // 1) 화면 중앙 조준점(카메라 레이 기준)
        Vector3 aimPoint = GetAimPointFromCamera(cameraTf);

        // 2) 총구 -> 조준점 방향
        Vector3 dir = (aimPoint - muzzle.position).normalized;

        // 3) 분산(원뿔)
        float spread = Mathf.Max(0f, isAiming ? aimSpread : hipSpread);
        if (spread > 0.0001f)
        {
            dir = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.up)
                * Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.right)
                * dir;
        }

        // 4) 탄 생성 + 속도 부여
        Bullet b = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));
        b.Fire(dir); // ← 반드시 호출

        // 5) 효과
        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && fireSfx) audioSource.PlayOneShot(fireSfx);
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
        // 백업
        Ray r2 = new Ray(camTf.position, camTf.forward);
        if (Physics.Raycast(r2, out RaycastHit h2, maxRayDistance, hitMask, QueryTriggerInteraction.Ignore))
            return h2.point;
        return r2.GetPoint(maxRayDistance);
    }
}
