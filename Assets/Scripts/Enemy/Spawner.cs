using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    [Header("Prefab / Target")]
    public GameObject dronePrefab;

    [Tooltip("비워두면 Tag=Player 자동 검색")]
    public Transform playerTarget;

    [Header("Wave Settings")]
    [Tooltip("웨이브 시작 간격(초)")]
    public float waveInterval = 30f;

    [Tooltip("한 웨이브 스폰 수")]
    public int dronesPerWave = 10;

    [Tooltip("웨이브 내 연사 간격(초)")]
    public float intraWaveDelay = 0.12f;

    [Tooltip("게임 시작 후 첫 웨이브 시작 지연(초)")]
    public float startDelay = 0f;

    [Header("Limits")]
    public int maxAlive = 30;

    [Header("Spawn Position")]
    [Tooltip("스폰 반경(0이면 스포너 위치)")]
    public float spawnRadius = 0f;

    [Tooltip("스폰 높이 오프셋")]
    public float spawnHeightOffset = 0.5f;

    [Header("Launch (사출)")]
    [Tooltip("사출 속도(유닛/초)")]
    public float launchSpeed = 12f;

    [Tooltip("사출 이동 시간(초)")]
    public float launchMoveDuration = 0.35f;

    [Tooltip("사출 후 멈칫 시간(초)")]
    public Vector2 pauseAfterLaunchRange = new Vector2(0.6f, 1.0f);

    [Tooltip("사출 방향의 위쪽 성분(0이면 수평, 너무 크면 위로만 감)")]
    public Vector2 launchUpRange = new Vector2(0f, 0.12f);

    [Header("Debug")]
    public bool debugLog = false;

    private readonly List<GameObject> alive = new List<GameObject>();
    private float timer = 0f;
    private bool spawning = false;

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        timer = -startDelay;
    }

    private void Update()
    {
        if (dronePrefab == null) return;

        CleanupNulls();

        timer += Time.deltaTime;
        if (!spawning && timer >= waveInterval)
        {
            timer = 0f;
            StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        spawning = true;

        int spawned = 0;
        while (spawned < dronesPerWave)
        {
            CleanupNulls();
            if (alive.Count >= maxAlive) break;

            SpawnOne();
            spawned++;

            if (intraWaveDelay > 0f) yield return new WaitForSeconds(intraWaveDelay);
            else yield return null;
        }

        spawning = false;
    }

    private void SpawnOne()
    {
        Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;

        if (spawnRadius > 0f)
        {
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            spawnPos += new Vector3(r.x, 0f, r.y);
        }

        GameObject obj = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        alive.Add(obj);

        Drone d = obj.GetComponent<Drone>();
        if (d != null)
        {
            if (playerTarget != null) d.target = playerTarget;

            // 랜덤 사출 방향 만들기
            Vector3 dir = Random.onUnitSphere;
            dir.y = Random.Range(launchUpRange.x, launchUpRange.y); // 위로 과도하게 튀는 거 방지
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            float pause = Random.Range(pauseAfterLaunchRange.x, pauseAfterLaunchRange.y);
            if (pause < 0f) pause = 0f;

            d.InitializeLaunch(dir, launchSpeed, launchMoveDuration, pause);
        }

        if (debugLog)
            Debug.Log($"[Spawner] Spawned {obj.name} at {spawnPos}", this);
    }

    private void CleanupNulls()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null) alive.RemoveAt(i);
        }
    }

    public void ForceWave()
    {
        if (dronePrefab == null) return;
        if (!spawning) StartCoroutine(SpawnWave());
    }

    public void ForceSpawnOne()
    {
        if (dronePrefab == null) return;
        CleanupNulls();
        if (alive.Count >= maxAlive) return;
        SpawnOne();
    }
}
