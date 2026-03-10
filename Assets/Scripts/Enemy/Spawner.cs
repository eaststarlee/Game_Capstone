using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour, IBreakable
{
    [Header("Prefab / Target")]
    public GameObject dronePrefab;

    [Tooltip("비워두면 Tag=Player 자동 검색")]
    public Transform playerTarget;

    [Header("Activation")]
    [Tooltip("체크 시, 플레이어가 감지 범위 내에 있을 때만 스폰 타이머가 돕니다.")]
    public bool requirePlayerInRange = true;
    [Tooltip("플레이어 감지 반경")]
    public float activationRange = 15f;

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

    [Header("Break (Black Ink Interaction)")]
    public GameObject breakEffectPrefab;
    public AudioClip breakSfx;
    [Range(0f, 1f)] public float breakSfxVolume = 1f;

    [Header("Sound Effects (Looping)")]
    public AudioClip idleLoopSfx;
    [Range(0f, 1f)] public float idleLoopSfxVolume = 1f;
    private AudioSource idleAudioSource;

    [Header("Health")]
    [Tooltip("파괴되기 위해 필요한 피격 횟수")]
    public int maxHP = 5;
    private int currentHP;

    [Header("UI")]
    public WorldSpaceHealthBar healthBar;

    [Header("Health Recovery & Grace Period")]
    [Tooltip("사정거리를 벗어난 후 체력이 초기화되기까지의 유예 시간")]
    public float gracePeriod = 3f;
    private float regenTimer = 0f;
    private bool isPlayerInside = false; // 현재 플레이어가 범위 안에 있는지 여부

    private readonly List<GameObject> alive = new List<GameObject>();
    private float timer = 0f;
    private bool spawning = false;
    private bool isBroken = false;

    private void Start()
    {
        PlayerHealth.OnPlayerRespawn += ResetSpawner;
        currentHP = maxHP;

        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        timer = -startDelay;

        // 대기음(Loop) 오디오 소스 초기화
        idleAudioSource = gameObject.AddComponent<AudioSource>();
        idleAudioSource.spatialBlend = 0f; // 2D 음향 (거리에 상관없이 볼륨 고정)
        idleAudioSource.loop = true;
        idleAudioSource.playOnAwake = false;

        if (healthBar != null) healthBar.UpdateHealthBar(currentHP, maxHP);
    }

    private void Update()
    {
        if (isBroken)
        {
            if (healthBar != null) healthBar.gameObject.SetActive(false);
            if (idleAudioSource != null && idleAudioSource.isPlaying)
                idleAudioSource.Stop();
            return;
        }
        if (dronePrefab == null) return;

        CleanupNulls();

        bool isPlayerInRange = true;

        // HP 활성화 로직

        // 1. 거리 체크
        isPlayerInside = CheckPlayerInRange();

        // 2. 상태별 UI 및 회복 로직
        if (isPlayerInside)
        {
            // [범위 안]
            regenTimer = 0f; // 타이머 리셋
            if (healthBar != null) healthBar.SetStatusColor(true); // 활성화 색상(초록)

            // 사운드 재생
            if (idleLoopSfx != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        }
        else
        {
            // [범위 밖]
            if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();

            if (currentHP < maxHP)
            {
                // 체력이 깎인 상태로 밖에 나갔다면
                regenTimer += Time.deltaTime;

                if (regenTimer < gracePeriod)
                {
                    // 유예 시간 동안: 깜빡거림 연출
                    if (healthBar != null) healthBar.FlashUpdate();
                }
                else
                {
                    // 유예 시간 종료: 체력 초기화 및 회색 고정
                    currentHP = maxHP;
                    regenTimer = 0f;
                    if (healthBar != null)
                    {
                        healthBar.UpdateHealthBar(currentHP, maxHP);
                        healthBar.SetStatusColor(false);
                    }
                }
            }
            else
            {
                // 체력이 가득 찬 상태로 밖에 있다면 그냥 회색
                if (healthBar != null) healthBar.SetStatusColor(false);
            }

            return; // 범위 밖이면 스폰 로직 실행 안 함
        }

        // 3. 스폰 타이머 (범위 안일 때만 실행)
        timer += Time.deltaTime;
        if (!spawning && timer >= waveInterval)
        {
            timer = 0f;
            StartCoroutine(SpawnWave());
        }

        if (requirePlayerInRange && playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist > activationRange)
            {
                isPlayerInRange = false;
            }
        }

        // 범위 내 대기음(Loop) 처리
        if (isPlayerInRange && idleLoopSfx != null)
        {
            if (!idleAudioSource.isPlaying)
            {
                idleAudioSource.clip = idleLoopSfx;
                idleAudioSource.volume = idleLoopSfxVolume;
                idleAudioSource.Play();
            }
        }
        else
        {
            if (idleAudioSource != null && idleAudioSource.isPlaying)
            {
                idleAudioSource.Stop();
            }
        }

        if (!isPlayerInRange) return; // 타이머 정지 처리

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
            if (isBroken) yield break;

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
        if (isBroken) return;

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
        if (isBroken) return;
        if (dronePrefab == null) return;
        if (!spawning) StartCoroutine(SpawnWave());
    }

    public void ForceSpawnOne()
    {
        if (isBroken) return;
        if (dronePrefab == null) return;
        CleanupNulls();
        if (alive.Count >= maxAlive) return;
        SpawnOne();
    }

    // IBreakable 인터페이스 구현
    public void Break()
    {
        if (isBroken) return;

        // [핵심 추가] 범위 밖에 있을 때는 데미지를 입지 않음
        if (!isPlayerInside)
        {
            if (debugLog) Debug.Log("[Spawner] Out of range: Attack Ignored.");
            return;
        }

        // 체력 감소
        currentHP--;

        // HP UI 업데이트 추가
        if (healthBar != null) healthBar.UpdateHealthBar(currentHP, maxHP);

        if (currentHP > 0)
        {
            // 피격 피드백 (필요 시 추가)
            return;
        }

        isBroken = true;

        if (idleAudioSource != null && idleAudioSource.isPlaying)
        {
            idleAudioSource.Stop();
        }

        // 1. 이펙트 재생
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 사운드 재생
        if (breakSfx != null)
        {
            // PlayClipAtPoint는 기본적으로 3D(spatialBlend=1)로 만들어지므로,
            // 2D로 거리에 상관없이 들리게 하려면 직접 AudioSource를 만들어 재생합니다.
            GameObject sfxObj = new GameObject("SpawnerBreakSfx");
            sfxObj.transform.position = transform.position;
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.clip = breakSfx;
            source.spatialBlend = 0f; // 2D (거리 무관)
            source.volume = breakSfxVolume;
            source.Play();
            Destroy(sfxObj, breakSfx.length + 0.1f);
        }

        // 3. 소환된 드론들 모두 제거 부분: 스폰된 드론들은 게임 내에 남도록 유지
        // ClearAllDrones();

        // 4. 스포너 파괴가 아닌 비활성화 상태로 전환 (부활 시 켜지게)
        gameObject.SetActive(false);
    }

    private void ClearAllDrones()
    {
        CleanupNulls();
        foreach (var drone in alive)
        {
            if (drone != null)
            {
                Destroy(drone);
            }
        }
        alive.Clear();
    }

    private void OnDestroy()
    {
        PlayerHealth.OnPlayerRespawn -= ResetSpawner;
    }

    private void ResetSpawner()
    {
        currentHP = maxHP;
        isBroken = false;
        timer = -startDelay;
        ClearAllDrones();
        gameObject.SetActive(true);
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHP, maxHP);
            healthBar.gameObject.SetActive(true); // 리스폰 시 UI 다시 켜기
        }
    }

    // 안전장치: 투사체의 이름으로 충돌 감지
    private void OnTriggerEnter(Collider other)
    {
        if (isBroken) return;

        // Projectile 이름에 "BlackInkProjectile(Clone)" 등이 포함되어 있는지 확인
        if (other.name.Contains("BlackInkProjectile"))
        {
             Break();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (requirePlayerInRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activationRange);
        }
    }

    private bool CheckPlayerInRange()
    {
        // 1. 타겟이 없거나 거리 체크가 필요 없는 설정이라면 항상 true 반환
        if (!requirePlayerInRange || playerTarget == null)
            return true;

        // 2. 실제 거리 계산
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // 3. 설정한 activationRange 이내에 있으면 true
        return dist <= activationRange;
    }
}
