
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public Camera playerCamera;

    [Header("Settings")]
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || muzzlePoint == null || playerCamera == null)
        {
            Debug.LogError("GunController is not set up correctly!");
            return;
        }

        // 1. Find the target point with a raycast from the center of the screen.
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = playerCamera.transform.position + playerCamera.transform.forward * 1000; // A point far away
        }

        // 2. Instantiate the bullet at the muzzle point.
        GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);

        // 3. Make the bullet look at the target point.
        bullet.transform.LookAt(targetPoint);
    }
}
