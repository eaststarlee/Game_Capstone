using UnityEngine;

public class Pathchaser : MonoBehaviour, IBreakable
{
    [Header("Path Following")]
    [Tooltip("드론이 따라갈 경로 오브젝트를 연결해주세요.")]
    public GhostPath pathToFollow;

    [Tooltip("해당 거리(반경) 안에 들어오면 다음 웨이포인트로 넘어갑니다.")]
    public float waypointThreshold = 0.5f;

    private int currentWaypointIndex = 0;

    [Header("Launch (set by spawner)")]
    public Vector3 launchDirection = Vector3.forward;
    public float launchSpeed = 12f;
    public float launchMoveDuration = 0.35f;
    public float pauseAfterLaunch = 0.8f;

    [Header("Chase (3D)")]
    public float chaseSpeed = 6f;
    public float rotateSpeed = 720f;

    [Tooltip("경로를 향할 때 목표 높이 오프셋")]
    public float aimHeightOffset = 0.75f;

    [Tooltip("true면 위아래 포함 3D로 추적, false면 수평만 추적")]
    public bool chaseIn3D = true;

    [Header("Separation (Anti-clump)")]
    public float separationRadius = 1.2f;
    public float separationStrength = 1.5f;
    public int separationEveryNFrames = 2;
    public LayerMask droneMask = ~0;
    public float separationMinDist = 0.01f;

    [Header("Damage")]
    public float damageRange = 1.2f;
    public int damageToPlayer = 1;
    public float hitCooldown = 0.25f;
    public bool selfDestructOnHit = true;
    public float selfDestructDelay = 0f;

    [Header("Sound Effects (Contact)")]
    public AudioClip contactSfx1;
    public AudioClip contactSfx2;
    [Range(0f, 1f)] public float contactSfxVolume = 1f;

    [Header("Heartbeat (Proximity Audio)")]
    [Tooltip("해당 보스(객체)의 심장 박동 소리")]
    public AudioClip heartbeatSfx;
    [Tooltip("소리가 들리기 시작하는 최대 거리")]
    public float heartbeatMaxDistance = 40f;
    [Tooltip("소리가 최대로 커지는 최소 거리 (이보다 가까우면 최대 볼륨)")]
    public float heartbeatMinDistance = 10f;
    [Range(0f, 1f)] public float heartbeatMaxVolume = 1f;
    
    private AudioSource heartbeatSource;

    [Header("Break (Black Ink)")]
    public GameObject breakEffectPrefab;
    public AudioClip breakSfx;
    [Range(0f, 1f)] public float breakSfxVolume = 1f;
    public float destroyDelayAfterBreak = 0f;

    [Header("Optional")]
    public bool freezeAfterHit = true;

    private enum State { LaunchMove, Pause, Chase, WaitBeforeBreak }
    private State state = State.LaunchMove;

    private float launchTimer = 0f;
    private float pauseTimer = 0f;

    private bool isDead = false;

    private Vector3 cachedSeparation = Vector3.zero;
    private int frameCounter = 0;

    private void Start()
    {
        PlayerHealth.OnPlayerRespawn += OnPlayerRespawnEvent;

        if (launchDirection.sqrMagnitude < 0.0001f)
            launchDirection = transform.forward;

        launchDirection.Normalize();

        state = State.LaunchMove;
        launchTimer = 0f;
        pauseTimer = 0f;
        currentWaypointIndex = 0;

        cachedSeparation = Vector3.zero;
        frameCounter = 0;

        // 심장 박동 오디오 초기화
        if (heartbeatSfx != null)
        {
            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.clip = heartbeatSfx;
            heartbeatSource.loop = true;
            heartbeatSource.spatialBlend = 0f; // 볼륨을 스크립트로 직접 조절하기 위해 2D(0)로 세팅
            heartbeatSource.volume = 0f;       // 시작 시 볼륨 0
            heartbeatSource.Play();
        }
    }

    public void InitializeLaunch(Vector3 dir, float speed, float moveDuration, float pauseDuration)
    {
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        dir.Normalize();

        launchDirection = dir;
        launchSpeed = Mathf.Max(0f, speed);
        launchMoveDuration = Mathf.Max(0f, moveDuration);
        pauseAfterLaunch = Mathf.Max(0f, pauseDuration);

        state = State.LaunchMove;
        launchTimer = 0f;
        pauseTimer = 0f;
        currentWaypointIndex = 0;

        isDead = false;

        cachedSeparation = Vector3.zero;
        frameCounter = 0;
    }

    private void Update()
    {
        UpdateHeartbeat();

        if (isDead) return;

        switch (state)
        {
            case State.LaunchMove:
                TickLaunchMove();
                break;
            case State.Pause:
                TickPause();
                break;
            case State.Chase:
                TickChase();
                break;
            case State.WaitBeforeBreak:
                TickWaitBeforeBreak();
                break;
        }
    }

    private void TickLaunchMove()
    {
        float dt = Time.deltaTime;
        transform.position += launchDirection * launchSpeed * dt;

        launchTimer += dt;
        if (launchTimer >= launchMoveDuration)
        {
            state = State.Pause;
            pauseTimer = 0f;
        }
    }

    private void TickPause()
    {
        pauseTimer += Time.deltaTime;
        if (pauseTimer >= pauseAfterLaunch)
        {
            state = State.Chase;
        }
    }

    private void UpdateHeartbeat()
    {
        // 심장 소리가 할당되지 않았거나 재생 중이 아니면 무시
        if (heartbeatSource == null || !heartbeatSource.isPlaying) return;

        // 죽었거나 플레이어가 없으면 소리를 끈다
        if (isDead || PlayerHealth.Instance == null)
        {
            heartbeatSource.volume = 0f;
            return;
        }

        // 플레이어와의 거리 계산
        float dist = Vector3.Distance(transform.position, PlayerHealth.Instance.transform.position);

        if (dist >= heartbeatMaxDistance)
        {
            heartbeatSource.volume = 0f; // 범위를 벗어나면 안 들림
        }
        else if (dist <= heartbeatMinDistance)
        {
            heartbeatSource.volume = heartbeatMaxVolume; // 아주 가까우면 최대 볼륨
        }
        else
        {
            // 거리 비례 계산 (Min과 Max 사이의 퍼센티지로 볼륨 결정)
            float t = 1f - ((dist - heartbeatMinDistance) / (heartbeatMaxDistance - heartbeatMinDistance));
            heartbeatSource.volume = Mathf.Lerp(0f, heartbeatMaxVolume, t);
        }
    }

    private void TickChase() 
    {
        if (pathToFollow == null || pathToFollow.waypoints.Length == 0) return;

        TryDamagePlayerByDistance();
        if (isDead) return;

        if (currentWaypointIndex >= pathToFollow.waypoints.Length)
        {
            state = State.WaitBeforeBreak;
            pauseTimer = 0f;
            return;
        }

        Transform targetWP = pathToFollow.waypoints[currentWaypointIndex];
        if (targetWP == null) 
        {
            currentWaypointIndex++;
            return;
        }

        Vector3 aimPoint = targetWP.position + Vector3.up * aimHeightOffset;
        Vector3 toAim = aimPoint - transform.position;

        if (!chaseIn3D) toAim.y = 0f;

        // 오브젝트의 크기(Collider) 등을 고려해 임계값을 넉넉하게 보정
        float currentThreshold = waypointThreshold;
        Collider myCol = GetComponentInChildren<Collider>();
        if (myCol != null)
        {
            currentThreshold += myCol.bounds.extents.magnitude; // 콜라이더의 크기만큼 도달 판정 반경 확장
        }

        if (toAim.sqrMagnitude < currentThreshold * currentThreshold)
        {
            currentWaypointIndex++;
            return;
        }

        Vector3 chaseDir = toAim.normalized;

        Vector3 sep = GetSeparationVector();
        Vector3 finalDir = chaseDir;

        if (separationStrength > 0f && sep.sqrMagnitude > 0.000001f)
        {
            finalDir = (chaseDir + sep * separationStrength).normalized;
        }

        Quaternion desired = Quaternion.LookRotation(finalDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotateSpeed * Time.deltaTime);
        transform.position += finalDir * chaseSpeed * Time.deltaTime;
    }

    private void TickWaitBeforeBreak()
    {
        pauseTimer += Time.deltaTime;
        if (pauseTimer >= 5f)
        {
            Break();
        }
    }

    private Vector3 GetSeparationVector()
    {
        if (separationStrength <= 0f || separationRadius <= 0f) return Vector3.zero;

        frameCounter++;
        if (separationEveryNFrames < 1) separationEveryNFrames = 1;

        if (frameCounter % separationEveryNFrames != 0)
            return cachedSeparation;

        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, droneMask, QueryTriggerInteraction.Ignore);

        Vector3 push = Vector3.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (c == null || c.transform == transform) continue;

            Pathchaser other = c.GetComponentInParent<Pathchaser>();
            if (other == null) continue;

            Vector3 away = transform.position - other.transform.position;
            if (!chaseIn3D) away.y = 0f;

            float d2 = away.sqrMagnitude;
            if (d2 < separationMinDist * separationMinDist) continue;

            float d = Mathf.Sqrt(d2);
            float weight = 1f / d;

            push += away.normalized * weight;
            count++;
        }

        if (count > 0) push /= count;

        if (push.sqrMagnitude > 1f) push.Normalize();

        cachedSeparation = push;
        return cachedSeparation;
    }

    private void TryDamagePlayerByDistance() { }

    public void Break()
    {
        if (isDead) return;
        isDead = true;

        if (heartbeatSource != null) heartbeatSource.Stop();

        Vector3 pos = transform.position;

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, pos, Quaternion.identity);

        if (breakSfx != null)
            AudioSource.PlayClipAtPoint(breakSfx, pos, breakSfxVolume);

        if (destroyDelayAfterBreak <= 0f) Destroy(gameObject);
        else Destroy(gameObject, destroyDelayAfterBreak);
    }

    private void PlayContactSfx(AudioClip clip)
    {
        if (clip == null) return;

        GameObject sfxObj = new GameObject("DroneContactSfx");
        sfxObj.transform.position = transform.position;
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.volume = contactSfxVolume;
        source.Play();
        Destroy(sfxObj, clip.length + 0.1f);
    }

    private void OnDestroy()
    {
        PlayerHealth.OnPlayerRespawn -= OnPlayerRespawnEvent;
    }

    private void OnPlayerRespawnEvent()
    {
        // 이벤트를 받자마자 즉시 콜라이더 비활성화
        Collider myCol = GetComponentInChildren<Collider>();
        if (myCol != null)
        {
            myCol.enabled = false;
        }

        // 비활성화된 콜라이더를 넘겨서 처리
        StartCoroutine(HandleRespawn(myCol));
    }

    private System.Collections.IEnumerator HandleRespawn(Collider myCol)
    {
        // 플레이어의 위치 갱신 완료까지 대기
        yield return null;
        yield return new WaitForFixedUpdate();

        // 실제 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Vector3 targetPos = player.transform.position;

            // 기존 오프셋 유지
            targetPos.x -= 150f;
            targetPos.y = -50f;

            transform.position = targetPos;

            // 상태 초기화
            currentWaypointIndex = 0;
            state = State.LaunchMove;
            launchTimer = 0f;
            pauseTimer = 0f;

            Debug.Log($"<color=cyan>[Pathchaser]</color> 리스폰 후 재배치 완료: {targetPos}");
        }
        else
        {
            Debug.LogWarning("[Pathchaser] Player 태그를 찾지 못했습니다.");
        }

        // 1초 후 콜라이더 재활성화
        if (myCol != null)
        {
            StartCoroutine(ReenableColliderAfterDelay(myCol, 1.0f));
        }
    }

    // 💡 지정된 시간(초)만큼 기다린 후 콜라이더를 안전하게 다시 켜는 코루틴
    private System.Collections.IEnumerator ReenableColliderAfterDelay(Collider col, float delay)
    {
        // 지정된 시간(1초) 동안 대기합니다.
        yield return new WaitForSeconds(delay);

        // 괴물이 그 사이에 죽지 않았다면 콜라이더를 다시 활성화합니다.
        if (col != null && !isDead)
        {
            col.enabled = true;
            Debug.Log($"<color=cyan>[Pathchaser]</color> 리스폰 1초 경과: 콜라이더가 다시 활성화되었습니다.");
        }
    }
}