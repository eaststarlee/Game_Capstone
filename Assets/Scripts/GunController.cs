using UnityEngine;

[System.Serializable]
public class ItemGunMapping
{
    public string itemName;
    public GameObject muzzlePrefab;
    public GameObject bulletPrefab; // 아이템별 총알 추가
}

public class GunController : MonoBehaviour
{
    [Header("Default Bullet Settings")]
    public GameObject defaultBulletPrefab; // 기존 bulletPrefab 대신 기본 총알

    [Header("Camera & Fire Settings")]
    public Camera playerCamera;
    public float fireRate = 5f;
    private float nextFireTime = 0f;
    public float spawnDistance = 0.3f;
    public float minTargetDistance = 2f;

    [Header("Muzzle Flash Settings")]
    public GameObject defaultMuzzlePrefab;
    public Transform muzzleTransform;
    public float muzzleDuration = 0.5f;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip fireSound;

    [Header("Item -> Gun Mapping")]
    public ItemGunMapping[] itemGunMappings;

    private Inventory playerInventory;

    void Update()
    {
        if (playerInventory == null)
            FindInventory();

        // 좌클릭: 기존 발사
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            // **우클릭 누른 상태**에서 좌클릭이면 Hitscan 실행
            if (Input.GetMouseButton(1))
            {
                if (Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                    Hitscan();
                }
            }
            else // 일반 좌클릭이면 기존
            {
                if (Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                    Hitscan();
                }
            }
        }
    }

    void FindInventory()
    {
        // 새로운 방식 (Unity 2023+)
        playerInventory = FindFirstObjectByType<Inventory>();

        if (playerInventory != null)
        {
            Debug.Log("Inventory 참조 성공!");
        }
    }
    /*
    // 기존 Fire1용 발사
    void Shoot()
    {
        if (playerInventory == null) return;
        var selectedSlot = playerInventory.GetSelectedSlot();

        GameObject muzzleToUse = defaultMuzzlePrefab;
        GameObject bulletToUse = defaultBulletPrefab;

        if (selectedSlot != null && selectedSlot.HasItem)
        {
            foreach (var mapping in itemGunMappings)
            {
                if (mapping.itemName == selectedSlot.itemName)
                {
                    if (mapping.muzzlePrefab != null)
                        muzzleToUse = mapping.muzzlePrefab;
                    if (mapping.bulletPrefab != null)
                        bulletToUse = mapping.bulletPrefab;
                    break;
                }
            }
        }

        Vector3 spawnPos = playerCamera.transform.position + playerCamera.transform.forward * spawnDistance;

        Vector3 targetPoint;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1000f))
        {
            targetPoint = hit.point;
            if ((targetPoint - spawnPos).magnitude < minTargetDistance)
                targetPoint = spawnPos + playerCamera.transform.forward * minTargetDistance;
        }
        else
        {
            targetPoint = spawnPos + playerCamera.transform.forward * 1000f;
        }

        GameObject bullet = Instantiate(bulletToUse, spawnPos, Quaternion.identity);
        bullet.transform.LookAt(targetPoint);

        if (muzzleToUse != null && muzzleTransform != null)
        {
            GameObject flash = Instantiate(muzzleToUse, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
            Destroy(flash, muzzleDuration);
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
    */
    // 우클릭 전용 Fire2: 레이캐스트 충돌 뒤 3 거리 위치에서 총알 생성
    void Hitscan()
    {
        if (playerInventory == null) return;
        var selectedSlot = playerInventory.GetSelectedSlot();

        GameObject muzzleToUse = defaultMuzzlePrefab;
        GameObject bulletToUse = defaultBulletPrefab;

        if (selectedSlot != null && selectedSlot.HasItem)
        {
            foreach (var mapping in itemGunMappings)
            {
                if (mapping.itemName == selectedSlot.itemName)
                {
                    if (mapping.muzzlePrefab != null)
                        muzzleToUse = mapping.muzzlePrefab;
                    if (mapping.bulletPrefab != null)
                        bulletToUse = mapping.bulletPrefab;
                    break;
                }
            }
        }

        // 레이캐스트 충돌 지점 뒤 3 거리 계산
        Vector3 spawnPos;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1000f))
        {
            spawnPos = hit.point - playerCamera.transform.forward.normalized * 1f;
        }
        else
        {
            spawnPos = playerCamera.transform.position + playerCamera.transform.forward * (spawnDistance + 1f);
        }

        // 타겟 포인트 계산
        Vector3 targetPoint;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, 1000f))
        {
            targetPoint = hitInfo.point;
            if ((targetPoint - spawnPos).magnitude < minTargetDistance)
                targetPoint = spawnPos + playerCamera.transform.forward * minTargetDistance;
        }
        else
        {
            targetPoint = spawnPos + playerCamera.transform.forward * 1000f;
        }

        GameObject bullet = Instantiate(bulletToUse, spawnPos, Quaternion.identity);
        bullet.transform.LookAt(targetPoint);

        if (muzzleToUse != null && muzzleTransform != null)
        {
            GameObject flash = Instantiate(muzzleToUse, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
            Destroy(flash, muzzleDuration);
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
}
