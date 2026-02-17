using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDroneSpawner : MonoBehaviour
{
    [Header("Prefab / Target")]
    public GameObject dronePrefab;
    public Transform playerTarget;

    [Header("Spawn Settings")]
    public int dronesPerWave = 8;
    public float intraWaveDelay = 0.1f;
    public int maxAlive = 20;
    public float spawnRadius = 5f;

    [Header("Launch Settings")]
    public float launchSpeed = 10f;
    public float launchMoveDuration = 0.4f;

    private List<GameObject> aliveDrones = new List<GameObject>();

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }
    }

    // 보스 컨트롤러에서 이 함수를 호출합니다.
    public void SummonWave()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        int spawned = 0;
        while (spawned < dronesPerWave)
        {
            CleanupNulls();
            if (aliveDrones.Count >= maxAlive) break;

            SpawnOne();
            spawned++;
            yield return new WaitForSeconds(intraWaveDelay);
        }
    }

    private void SpawnOne()
    {
        Vector3 spawnPos = transform.position + (Random.insideUnitSphere * spawnRadius);
        spawnPos.y = transform.position.y; // 높이 고정

        GameObject obj = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        aliveDrones.Add(obj);

        // 유령(Drone) 스크립트 초기화
        Drone d = obj.GetComponent<Drone>();
        if (d != null)
        {
            d.target = playerTarget;
            Vector3 launchDir = (spawnPos - transform.position).normalized;
            d.InitializeLaunch(launchDir, launchSpeed, launchMoveDuration, 0.5f);
        }
    }

    private void CleanupNulls()
    {
        aliveDrones.RemoveAll(item => item == null);
    }

    // 보스 리셋이나 사망 시 모든 쫄 제거
    public void ClearAllDrones()
    {
        StopAllCoroutines();
        foreach (var drone in aliveDrones)
        {
            if (drone != null) Destroy(drone);
        }
        aliveDrones.Clear();
    }
}