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

    [Header("Break (Black Ink)")]
    public GameObject breakEffectPrefab;
    public AudioClip breakSfx;
    [Range(0f, 1f)] public float breakSfxVolume = 1f;
    public float destroyDelayAfterBreak = 0f;

    [Header("Optional")]
    public bool freezeAfterHit = true;

    private enum State { LaunchMove, Pause, Chase }
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

    private void TickChase()
    {
        if (pathToFollow == null || pathToFollow.waypoints.Length == 0) return;

        TryDamagePlayerByDistance();
        if (isDead) return;

        if (currentWaypointIndex >= pathToFollow.waypoints.Length)
        {
            transform.position += transform.forward * chaseSpeed * Time.deltaTime;
            return;
        }

        Transform targetWP = pathToFollow.waypoints[currentWaypointIndex];
        if (targetWP == null) return;

        Vector3 aimPoint = targetWP.position + Vector3.up * aimHeightOffset;
        Vector3 toAim = aimPoint - transform.position;

        if (!chaseIn3D) toAim.y = 0f;

        if (toAim.sqrMagnitude < waypointThreshold * waypointThreshold)
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
        Destroy(gameObject);
    }
}