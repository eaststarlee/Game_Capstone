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
    public float spawnDistance = 1f;
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

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void FindInventory()
    {
        playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory != null)
        {
            Debug.Log("Inventory 참조 성공!");
        }
    }

    void Shoot()
    {
        if (playerInventory == null) return;
        var selectedSlot = playerInventory.GetSelectedSlot();
        if (selectedSlot == null || !selectedSlot.HasItem)
        {
            Debug.Log("선택된 슬롯이 비어있습니다! 발사 불가.");
            return;
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

        // 아이템별 머즐 & 총알 결정
        GameObject muzzleToUse = defaultMuzzlePrefab;
        GameObject bulletToUse = defaultBulletPrefab;

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

        // Bullet 생성
        GameObject bullet = Instantiate(bulletToUse, spawnPos, Quaternion.identity);
        bullet.transform.LookAt(targetPoint);

        // Muzzle 생성
        if (muzzleToUse != null && muzzleTransform != null)
        {
            GameObject flash = Instantiate(muzzleToUse, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
            Destroy(flash, muzzleDuration);
        }

        // 총소리 재생
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
}
