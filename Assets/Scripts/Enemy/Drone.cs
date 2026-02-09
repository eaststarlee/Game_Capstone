using UnityEngine;

public class Drone : MonoBehaviour, IBreakable
{
    [Header("Target")]
    public Transform target;

    [Header("Launch (set by spawner)")]
    public Vector3 launchDirection = Vector3.forward;
    public float launchSpeed = 12f;
    public float launchMoveDuration = 0.35f;
    public float pauseAfterLaunch = 0.8f;

    [Header("Chase (3D)")]
    public float chaseSpeed = 6f;
    public float rotateSpeed = 720f;

    [Tooltip("플레이어를 향할 때 목표 높이 오프셋")]
    public float aimHeightOffset = 0.75f;

    [Tooltip("true면 위아래 포함 3D로 추적, false면 수평만 추적")]
    public bool chaseIn3D = true;

    [Header("Separation (Anti-clump)")]
    [Tooltip("드론끼리 뭉치지 않게 밀어내는 반경")]
    public float separationRadius = 1.2f;

    [Tooltip("밀어내는 힘의 세기 (0이면 꺼짐). 0.8~2.5 추천")]
    public float separationStrength = 1.5f;

    [Tooltip("Separation 계산을 몇 프레임마다 할지(가벼워짐). 1이면 매 프레임")]
    public int separationEveryNFrames = 2;

    [Tooltip("Separation에서 체크할 레이어(Drone을 이 레이어에 넣으면 성능/정확도 좋아짐)")]
    public LayerMask droneMask = ~0;

    [Tooltip("자기 자신을 제외할 최소 거리(너무 붙으면 폭발 방지)")]
    public float separationMinDist = 0.01f;

    [Header("Damage")]
    public float damageRange = 1.2f;
    public int damageToPlayer = 10;
    public float hitCooldown = 0.25f;
    public bool selfDestructOnHit = true;
    public float selfDestructDelay = 0f;

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
    private bool hasDealtDamage = false;
    private float lastHitTime = -999f;

    private Vector3 cachedSeparation = Vector3.zero;
    private int frameCounter = 0;

    private void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (launchDirection.sqrMagnitude < 0.0001f)
            launchDirection = transform.forward;

        launchDirection.Normalize();

        state = State.LaunchMove;
        launchTimer = 0f;
        pauseTimer = 0f;

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

        isDead = false;
        hasDealtDamage = false;
        lastHitTime = -999f;

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
        if (target == null) return;

        TryDamagePlayerByDistance();
        if (isDead) return;

        // ✅ 기본 추적 방향
        Vector3 aimPoint = target.position + Vector3.up * aimHeightOffset;
        Vector3 toAim = aimPoint - transform.position;
        if (!chaseIn3D) toAim.y = 0f;

        if (toAim.sqrMagnitude < 0.0001f) return;

        Vector3 chaseDir = toAim.normalized;

        // ✅ 드론끼리 분산 방향(가짜 반발력)
        Vector3 sep = GetSeparationVector();
        Vector3 finalDir = chaseDir;

        if (separationStrength > 0f && sep.sqrMagnitude > 0.000001f)
        {
            // chaseDir + sep 섞기
            // separationStrength가 클수록 뭉침 방지가 강해짐
            finalDir = (chaseDir + sep * separationStrength).normalized;
        }

        // 회전/이동
        Quaternion desired = Quaternion.LookRotation(finalDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotateSpeed * Time.deltaTime);

        transform.position += finalDir * chaseSpeed * Time.deltaTime;
    }

    private Vector3 GetSeparationVector()
    {
        // 끄기
        if (separationStrength <= 0f || separationRadius <= 0f) return Vector3.zero;

        frameCounter++;
        if (separationEveryNFrames < 1) separationEveryNFrames = 1;

        // 매 프레임 계산하지 말고 캐시로 가볍게
        if (frameCounter % separationEveryNFrames != 0)
            return cachedSeparation;

        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, droneMask, QueryTriggerInteraction.Ignore);

        Vector3 push = Vector3.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (c == null) continue;

            // 자기 자신 제외 (같은 transform이거나 같은 GameObject)
            if (c.transform == transform) continue;

            // “드론끼리만” 대상으로 제한: Drone 컴포넌트가 있는 애만
            Drone other = c.GetComponentInParent<Drone>();
            if (other == null) continue;

            Vector3 away = transform.position - other.transform.position;

            // 수평 추적이면 separation도 수평으로
            if (!chaseIn3D) away.y = 0f;

            float d2 = away.sqrMagnitude;
            if (d2 < separationMinDist * separationMinDist) continue;

            // 가까울수록 더 강하게 밀기(1/d)
            float d = Mathf.Sqrt(d2);
            float weight = 1f / d;

            push += away.normalized * weight;
            count++;
        }

        if (count > 0)
            push /= count;

        // 너무 강하게 흔들리지 않게 약간 clamp
        if (push.sqrMagnitude > 1f)
            push.Normalize();

        cachedSeparation = push;
        return cachedSeparation;
    }

    private void TryDamagePlayerByDistance()
    {
        if (hasDealtDamage) return;

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > damageRange) return;

        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph == null) ph = target.GetComponentInParent<PlayerHealth>();

        if (ph != null)
            ph.TakeDamage(damageToPlayer);

        hasDealtDamage = true;

        if (selfDestructOnHit)
        {
            if (freezeAfterHit)
            {
                isDead = true;
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                if (selfDestructDelay <= 0f) Destroy(gameObject);
                else Destroy(gameObject, selfDestructDelay);
            }
            else
            {
                isDead = true;
                if (selfDestructDelay <= 0f) Destroy(gameObject);
                else Destroy(gameObject, selfDestructDelay);
            }
        }
    }

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
}
