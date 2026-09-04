using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossController2 : MonoBehaviour
{
    public enum BossPhase { Searching, BasicAttack, SpecialSkill, GroggyChance }

    [Header("현재 상태 및 단계")]
    public BossPhase currentPhase = BossPhase.Searching;
    private int setCounter = 0;
    private int basicAttackCounter = 0;
    private bool isCombatStarted = false;

    [Header("컴포넌트 연결")]
    public Transform playerTransform;
    public Transform mapCenter;
    private NavMeshAgent agent;
    public BossHealth2 health;
    private BossUIController2 uiController;
    private Animator anim;

    [Header("프리팹 및 효과 할당")]
    public GameObject cardMeleePrefab;
    public GameObject cardProjectilePrefab;
    public GameObject shockwavePrefab;
    public GameObject spinningPrefab;
    public GameObject flashEffectPrefab;
    public GameObject pillarObjectPrefab;
    public GameObject safeZonePrefab;
    public GameObject[] powerfulAttackPrefabs;
    public GameObject[] fallingObjectPrefabs;
    public GameObject vanishEffect;
    public GameObject redWarningPrefab;
    public GameObject bossClonePrefab;
    public GameObject Type3Shoot;
    public GameObject balloonGroup;
    public GameObject droneSpawnerPrefab;

    [Header("Special Type 4 (비행/풍선) 설정")]
    public float flySpeed = 6f;
    public Vector2 xzRange = new Vector2(-15f, 15f);
    public Vector2 yHeightRange = new Vector2(5f, 9f);
    private int currentBurstCount = 0;
    private bool isSpecial4Active = false;
    private GameObject currentSpawner;
    private bool isGroggy = false;

    [Header("콜라이더 설정")]
    public Collider normalCollider;  // 원래 쓰던 캡슐 콜라이더
    public Collider groggyCollider;  // 납작하게 만든 그로기 전용 콜라이더

    private List<Boss2Clone> activeClones = new List<Boss2Clone>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<BossHealth2>();
        uiController = Object.FindFirstObjectByType<BossUIController2>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 보스 체력 스크립트의 이벤트 연결
        health.OnGroggyStart += HandleGroggyStart;
        health.OnBossReset += ResetBossCombat;
    }

    void Update()
    {
        if (health.currentStatus == BossHealth2.BossState.Defeated) return;

        // 전투 시작 전 감지 로직
        if (!isCombatStarted)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= 45f) StartBossCombat(); // detectionRange 대신 20f 직접 기입 (조절 가능)
        }
    }
    // 애니메이션 함수
    private void PlayAnim(string target)
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isCasting", false);
        anim.SetBool("isGroggy", false);

        if (!string.IsNullOrEmpty(target))
            anim.SetBool(target, true);
    }

    private void StartBossCombat()
    {
        isCombatStarted = true;
        if (uiController != null) uiController.SetVisible(true);
        if (uiController != null) uiController.ShowPatternMessage("광대의 기이한 힘으로 체력 회복 능력이 비활성화되었습니다!");
        StartCoroutine(MainBossLoop());
    }

    // --- 메인 시퀀스 루프 ---
    IEnumerator MainBossLoop()
    {
        while (health.currentStatus != BossHealth2.BossState.Defeated)
        {
            // 3번의 전체 세트 반복
            for (setCounter = 0; setCounter < 3; setCounter++)
            {
                // 3번의 기본 공격 세트 (추격 -> 공격)
                for (basicAttackCounter = 0; basicAttackCounter < 3; basicAttackCounter++)
                {
                    yield return StartCoroutine(SearchPhase(3f)); // 3초간 추격
                    yield return StartCoroutine(ExecuteBasicAttack());
                }
                // 세트 종료 후 특수 스킬 1회 
                yield return StartCoroutine(ExecuteSpecialSkill(false));
            }

            // 3세트 모두 종료 시 강제 풍선 패턴(SpecialType4) 진입
            yield return StartCoroutine(SpecialType4());
        }
    }

    // --- 단계별 로직 ---

    IEnumerator SearchPhase(float duration)
    {
        currentPhase = BossPhase.Searching;
        PlayAnim("isWalking"); // 걷기 애니메이션 루프 시작
        if (health.currentStatus != BossHealth2.BossState.Normal) yield break;

        agent.enabled = true;
        agent.isStopped = false;
        float timer = 0f;
        while (timer < duration)
        {
            if (agent.isOnNavMesh) agent.SetDestination(playerTransform.position);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ExecuteBasicAttack()
    {
        currentPhase = BossPhase.BasicAttack;
        agent.isStopped = true;

        int attackType = Random.Range(1, 4);
        if (attackType == 1) yield return StartCoroutine(BasicAttackType1());
        else if (attackType == 2) yield return StartCoroutine(BasicAttackType2());
        else yield return StartCoroutine(BasicAttackType3());
    }

    private int specialSkillOrder = 1; // 스킬 순서를 저장할 변수 (상단 선언 권장)

    IEnumerator ExecuteSpecialSkill(bool includeType4)
    {
        currentPhase = BossPhase.SpecialSkill;

        // 현재 순서에 맞는 스킬 실행
        if (specialSkillOrder == 1) yield return StartCoroutine(SpecialType1());
        else if (specialSkillOrder == 2) yield return StartCoroutine(SpecialType2());
        else if (specialSkillOrder == 3) yield return StartCoroutine(SpecialType3());
        else if (specialSkillOrder == 4) yield return StartCoroutine(SpecialType4());

        // 다음 스킬 번호 증가
        specialSkillOrder++;

        // 범위 제한 (Type 4 포함 여부에 따라 리셋 지점 결정)
        int maxSkill = includeType4 ? 4 : 3;
        if (specialSkillOrder > maxSkill)
        {
            specialSkillOrder = 1;
        }
    }

    // --- BC2Test에서 이식된 공격 기술들 ---

    IEnumerator BasicAttackType1()
    {
        Debug.Log("패턴: 예측 도약 공격");
        float floorY = (mapCenter != null) ? mapCenter.position.y : 0f;
        Vector3 playerVelocity = playerTransform.GetComponent<CharacterController>()?.velocity ?? Vector3.zero;
        Vector3 predictedPos = playerTransform.position + Vector3.ClampMagnitude(playerVelocity * 0.5f, 2f);
        Vector3 playerFloorPos = new Vector3(predictedPos.x, floorY, predictedPos.z);
        Vector3 bossFloorPos = new Vector3(transform.position.x, floorY, transform.position.z);
        Vector3 jumpTarget = playerFloorPos + (playerFloorPos - bossFloorPos).normalized * 1.5f;

        agent.enabled = false;
        transform.LookAt(playerFloorPos);
        PlayAnim("isCasting");
        yield return StartCoroutine(MoveToPosition(transform.position + Vector3.up * 3f, 0.3f));
        yield return StartCoroutine(MoveToPosition(jumpTarget, 0.35f));

        if (cardMeleePrefab != null)
        {
            GameObject melee = Instantiate(cardMeleePrefab, transform.position + Vector3.up * 0.2f, transform.rotation);
            melee.transform.SetParent(this.transform);

            float swingDuration = 0.25f;
            float elapsed = 0f;
            Vector3 moveStartPos = transform.position;
            Vector3 moveEndPos = transform.position + transform.forward * 1.5f;

            while (elapsed < swingDuration)
            {
                float t = elapsed / swingDuration;
                transform.position = Vector3.Lerp(moveStartPos, moveEndPos, t);
                melee.transform.RotateAround(transform.position + Vector3.up * 0.5f, Vector3.up, (360 / swingDuration) * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            melee.transform.SetParent(null);
            Destroy(melee, 0.1f);
        }
        RecoverAgent();
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator BasicAttackType2()
    {
        Debug.Log("패턴: 유도 카드 발사");
        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        PlayAnim("isCasting");
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 3; i++)
        {
            if (cardProjectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
                GameObject card = Instantiate(cardProjectilePrefab, spawnPos, Quaternion.identity);
                Vector3 targetDir = (playerTransform.position + Vector3.up * 0.5f - spawnPos).normalized;
                StartCoroutine(HomingProjectile(card, targetDir));
            }
            yield return new WaitForSeconds(0.7f);
        }

        yield return new WaitForSeconds(3.0f);
    }

    IEnumerator HomingProjectile(GameObject proj, Vector3 initialDir)
    {
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        float speed = 15f;
        float elapsed = 0f;
        bool isHoming = false;
        if (rb != null) rb.linearVelocity = initialDir * speed;

        while (proj != null && elapsed < 3.0f)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= 1.0f)
            {
                if (!isHoming) { isHoming = true; speed *= 0.7f; }
                Vector3 homingDir = (playerTransform.position + Vector3.up * 0.5f - proj.transform.position).normalized;
                rb.linearVelocity = Vector3.Slerp(rb.linearVelocity.normalized, homingDir, Time.deltaTime * 5f) * speed;
                proj.transform.forward = rb.linearVelocity.normalized;
            }
            yield return null;
        }
        if (proj != null) Destroy(proj);
    }

    IEnumerator BasicAttackType3()
    {
        Debug.Log("패턴: 대각선 강하");
        agent.enabled = false;
        Vector3 airPos = transform.position + Vector3.up * 8f;
        yield return StartCoroutine(MoveToPosition(airPos, 0.5f));
        yield return new WaitForSeconds(0.4f);

        float floorY = (mapCenter != null) ? mapCenter.position.y : 0f;
        Vector3 targetLandPos = new Vector3(playerTransform.position.x, floorY, playerTransform.position.z);
        yield return StartCoroutine(MoveToPosition(targetLandPos, 0.35f));

        if (shockwavePrefab != null)
        {
            GameObject wave = Instantiate(shockwavePrefab, transform.position + Vector3.up * -0.2f, Quaternion.identity);
            Destroy(wave, 2.0f);
        }
        RecoverAgent();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator SpecialType1()
    {
        Debug.Log("특수: 강력한 내리꽂기");
        if (uiController != null) uiController.ShowPatternMessage("주의! 보스가 강력한 충격파를 준비합니다! 기둥 뒤로 대피하세요!");

        agent.enabled = false;
        PlayAnim("isCasting");

        // 1. 기준점 설정 (맵의 중심 좌표)
        Vector3 centerPos = mapCenter.position;
        float floorY = centerPos.y; // 맵 바닥의 Y값

        // 2. 보스 상승 (맵 중심 위 10m 지점으로 이동)
        yield return StartCoroutine(MoveToPosition(centerPos + Vector3.up * 10f, 0.8f));

        // 3. 회전 연출 및 기둥 소환
        if (spinningPrefab != null)
        {
            // [수정] 보스 자식으로 넣되, 위치는 보스 위치보다 5m 위로 고정
            GameObject spun = Instantiate(spinningPrefab, transform.position + Vector3.up * 5f, transform.rotation, transform);
            StartCoroutine(RotateEffect(spun));

            if (pillarObjectPrefab != null)
            {
                // [수정] 기둥 생성 위치를 맵 중심(centerPos) 기준으로 계산 (상댓값은 조절 가능)
                Vector3 pSpawnPos = centerPos + new Vector3(17f, 39, 5f);
                GameObject pillar = Instantiate(pillarObjectPrefab, pSpawnPos, Quaternion.identity);

                // [수정] 기둥이 내려올 최종 바닥 높이 설정 
                // pSpawnPos.y - 36.5f 대신 맵 바닥(floorY) + 약간의 여유값 사용
                float pillarTargetY = floorY + 15.0f;
                StartCoroutine(LowerPillar(pillar, pillarTargetY, 3f));

                Destroy(pillar, 15f);
            }

            // 패턴 대기 시간 (기둥 뒤로 숨을 시간)
            yield return new WaitForSeconds(10f);
            if (spun != null) Destroy(spun);
        }

        // 4. 충격파 발사 (Flash Effect)
        if (flashEffectPrefab != null)
            Destroy(Instantiate(flashEffectPrefab, transform.position, Quaternion.identity), 2f);

        // 5. 강력한 공격 프리팹들 생성 (바닥 이펙트 등)
        foreach (var p in powerfulAttackPrefabs)
        {
            if (p != null)
            {
                // [수정] 프리팹의 저장된 위치가 아닌, 현재 맵 바닥 중심 기준으로 생성되도록 보정
                // 만약 p 자체가 위치 정보가 포함된 오브젝트라면 아래와 같이 생성
                Vector3 attackPos = new Vector3(centerPos.x, floorY, centerPos.z);
                GameObject attackEffect = Instantiate(p, attackPos, p.transform.rotation);
                Destroy(attackEffect, 0.5f);
            }
            yield return new WaitForSeconds(0.3f);
        }

        // 6. 보스 복귀 (맵 중심으로 착지)
        yield return StartCoroutine(MoveToPosition(centerPos, 0.4f));

        RecoverAgent();
    }

    IEnumerator SpecialType2()
    {
        Debug.Log("특수: 은신 및 낙하");
        if (vanishEffect != null) { vanishEffect.SetActive(true); yield return new WaitForSeconds(1f); vanishEffect.SetActive(false); }

        agent.enabled = false;
        ToggleVisuals(false);

        float floorY = (mapCenter != null) ? mapCenter.position.y : 0f;

        for (int i = 0; i < 4; i++)
        {
            Vector3 target = new Vector3(playerTransform.position.x, floorY, playerTransform.position.z);
            GameObject rand = fallingObjectPrefabs[Random.Range(0, fallingObjectPrefabs.Length)];
            StartCoroutine(ExecuteMeteor(rand, target));
            yield return new WaitForSeconds(3f);
        }

        yield return new WaitForSeconds(2.2f);
        transform.position = mapCenter.position;
        ToggleVisuals(true); 
        if (vanishEffect != null) { vanishEffect.SetActive(true); yield return new WaitForSeconds(1f); vanishEffect.SetActive(false); }
        RecoverAgent();
    }

    IEnumerator SpecialType3()
    {
        Debug.Log("특수: 삼각 분신");
        agent.enabled = false;
        activeClones.Clear();
        for (int i = 0; i < 2; i++)
        {
            GameObject c = Instantiate(bossClonePrefab, transform.position, Quaternion.identity);
            Boss2Clone sc = c.GetComponent<Boss2Clone>();
            sc.projectilePrefab = Type3Shoot;
            sc.Init(playerTransform, 8f, 60f, (i + 1) * 120f);
            activeClones.Add(sc);
            sc.StartOrbit();
        }

        float timer = 0, fireTimer = 0;
        while (timer < 10f)
        {
            timer += Time.deltaTime; fireTimer += Time.deltaTime;
            float r = (timer * 60f) * Mathf.Deg2Rad;
            Vector3 t = playerTransform.position + new Vector3(Mathf.Cos(r), 0, Mathf.Sin(r)) * 8f;
            transform.position = Vector3.Lerp(transform.position, t, Time.deltaTime * 5f);
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

            if (fireTimer >= 1.5f) { Instantiate(Type3Shoot, transform.position + Vector3.up * 1.1f, transform.rotation); fireTimer = 0; }
            yield return null;
        }

        foreach (var cl in activeClones) if (cl != null) cl.FinalRush(playerTransform.position);
        yield return new WaitForSeconds(0.6f);
        transform.position = mapCenter.position;
        if (vanishEffect != null) { vanishEffect.SetActive(true); yield return new WaitForSeconds(1f); vanishEffect.SetActive(false); }
        RecoverAgent();
    }

    // --- SpecialType 4 (풍선/그로기 통합 패턴) ---

    IEnumerator SpecialType4()
    {
        Debug.Log("Skill 4: 풍선 비행 패턴 시작 (리셋 로직 포함)");
        if (uiController != null) uiController.ShowPatternMessage("각 풍선 색에 맞는 잉크 탄환으로 풍선을 전부 터트리세요!");

        // 1. 상태 변수 및 물리 초기화
        isGroggy = false;
        currentBurstCount = 0;
        if (agent != null) agent.enabled = false;

        // 2. 풍선 그룹 및 자식 풍선들 리셋 (재입장 대응)
        if (balloonGroup != null)
        {
            balloonGroup.SetActive(true);

            // 자식에 붙은 모든 BossBalloon을 가져옵니다.
            BossBalloon[] balloons = balloonGroup.GetComponentsInChildren<BossBalloon>(true);

            foreach (var b in balloons)
            {
                // [중요] 이전 패턴에서 터져서 꺼진 풍선을 다시 켭니다.
                b.gameObject.SetActive(true);

                // BossBalloon 내부에 있는 isBurst 변수를 false로 리셋하고 보스 참조를 연결합니다.
                b.Init(this);
            }
        }

        // 2-1. 드론 스포너 생성 및 자식화 (보낸 코드와 동일)
        if (droneSpawnerPrefab != null)
        {
            currentSpawner = Instantiate(droneSpawnerPrefab, transform.position, Quaternion.identity);
            currentSpawner.transform.SetParent(this.transform);
        }

        // 3. 비행 및 웨이브 호출 루프
        Vector3 targetPoint = GetNewRandomAirPoint();
        float waveTimer = 0f;
        float waveInterval = 4f;

        while (!isGroggy)
        {
            // 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, flySpeed * Time.deltaTime);

            // 드론 웨이브 호출 (현재 생성된 자식 스포너에서)
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                if (currentSpawner != null)
                {
                    currentSpawner.GetComponent<BossDroneSpawner>().SummonWave();
                }
                waveTimer = 0f;
            }

            // 도착 시 타겟 갱신
            if (Vector3.Distance(transform.position, targetPoint) < 1.5f)
                targetPoint = GetNewRandomAirPoint();

            yield return null;
        }

        // 4. 모든 풍선 파괴 (그로기)

        if (currentSpawner != null)
        {
            currentSpawner.GetComponent<BossDroneSpawner>().ClearAllDrones();
            Destroy(currentSpawner);
        }

        // 5. 추락 연출 (mapCenter의 Y좌표 기준)
        Debug.Log("추락 시작!");

        // 착지 목표 높이 설정: mapCenter가 바닥에 있다면 그 높이를 사용합니다.
        // 보스의 피벗(Pivot) 위치에 따라 약간의 보정값(+0.5f 등)을 더해줄 수 있습니다.
        float targetFloorY = (mapCenter != null) ? mapCenter.position.y : 0f;

        float gravity = 25f;
        float verticalVelocity = 0f;

        // 보스의 현재 Y좌표가 목표 Y좌표보다 높은 동안 계속 하강
        while (transform.position.y > targetFloorY)
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += Vector3.down * verticalVelocity * Time.deltaTime;

            // 추락 연출: 회전
            transform.Rotate(Vector3.forward * 400f * Time.deltaTime);

            yield return null;
        }

        // 지면에 정확히 고정 (X, Z는 유지하고 Y만 mapCenter 높이로)
        transform.position = new Vector3(transform.position.x, targetFloorY, transform.position.z);
        transform.rotation = Quaternion.identity;
        Debug.Log($"착지 완료: 목표 높이({targetFloorY})에 도달했습니다.");

        // 6. 그로기 대기
        yield return new WaitForSeconds(0.5f);
        PlayAnim("isGroggy");
        if (normalCollider != null) normalCollider.enabled = false;
        if (groggyCollider != null) groggyCollider.enabled = true;
        if (health != null) health.EnterGroggyState(8.0f);
        transform.position = new Vector3(transform.position.x, targetFloorY - 0.5f, transform.position.z);
        currentPhase = BossPhase.GroggyChance;
        yield return new WaitForSeconds(8.1f);

        // 7. 복구 로직
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
        if (balloonGroup != null) balloonGroup.SetActive(false);
        Debug.Log("Skill 4: 패턴 종료 및 복구 완료");
    }

    // 풍선에서 호출
    public void OnBalloonBurst()
    {
        currentBurstCount++;
        if (currentBurstCount >= 4) // 풍선 4개가 모두 터졌을 때
        {
            isGroggy = true;
        }
    }

    private void RecoverAgent()
    {
        agent.enabled = true;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    private void ToggleVisuals(bool show)
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = show;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = show;
    }

    private Vector3 GetNewRandomAirPoint()
    {
        Vector3 center = (mapCenter != null) ? mapCenter.position : Vector3.zero;
        return center + new Vector3(Random.Range(xzRange.x, xzRange.y), Random.Range(yHeightRange.x, yHeightRange.y), Random.Range(xzRange.x, xzRange.y));
    }

    IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        float t = 0; Vector3 s = transform.position;
        while (t < duration) { transform.position = Vector3.Lerp(s, target, t / duration); t += Time.deltaTime; yield return null; }
        transform.position = target;
    }

    IEnumerator RotateEffect(GameObject o) { while (o != null) { o.transform.Rotate(Vector3.up * 720f * Time.deltaTime); yield return null; } }
    IEnumerator LowerPillar(GameObject p, float tY, float d)
    {
        float e = 0; Vector3 s = p.transform.position; Vector3 en = new Vector3(s.x, tY, s.z);
        while (e < d && p != null) { p.transform.position = Vector3.Lerp(s, en, e / d); e += Time.deltaTime; yield return null; }
        if (p != null) { p.transform.position = en; if (safeZonePrefab != null) Instantiate(safeZonePrefab, en, p.transform.rotation, p.transform); }
    }

    IEnumerator ExecuteMeteor(GameObject p, Vector3 t)
    {
        GameObject w = Instantiate(redWarningPrefab, t, Quaternion.identity);
        GameObject m = Instantiate(p, t + Vector3.up * 50.5f, Quaternion.identity);
        float e = 0, d = 1.7f;
        while (e < d && m != null)
        {
            e += Time.deltaTime; m.transform.position += Vector3.down * (50f / d) * Time.deltaTime;
            if (w != null) { Color c = w.GetComponent<Renderer>().material.color; c.a = e / d; w.GetComponent<Renderer>().material.color = c; }
            yield return null;
        }
        if (m != null) { m.transform.Find("ImpactEffect")?.gameObject.SetActive(true); Destroy(w); Destroy(m, 0.5f); }
    }

    private void HandleGroggyStart()
    {
        // 1. 일단 현재 돌고 있는 모든 코루틴(MainBossLoop, SpecialType4 등)을 싹 다 밀어버립니다.
        StopAllCoroutines();

        // 2. 소환물이나 풍선 등 물리적인 것들 정리
        ClearAllPatterns();

        // 3. 8초 뒤에 메인 루프를 재시작하는 코루틴 딱 하나만 새로 실행합니다.
        StartCoroutine(RecoverAfterGroggy());

        Debug.Log("<color=yellow>모든 루프 중단 및 8초 후 재시작 예약</color>");
    }
    private IEnumerator RecoverAfterGroggy()
    {
        // 헬스 스크립트에서 설정한 그로기 시간만큼 대기 (8초)
        yield return new WaitForSeconds(8.0f);
        PlayAnim("");
        if (normalCollider != null) normalCollider.enabled = true;
        if (groggyCollider != null) groggyCollider.enabled = false;

        // 3. 물리/내비게이션 복구

        // 1. 상태 복구
        currentPhase = BossPhase.Searching;

        // 2. 물리/내비게이션 복구
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
        if (balloonGroup != null) balloonGroup.SetActive(false);

        Debug.Log("<color=green>그로기 종료: 메인 루프 재가동</color>");

        // 3. 멈췄던 메인 루프를 처음부터 다시 시작
        StartCoroutine(MainBossLoop());
    }
    private void ResetBossCombat() { isCombatStarted = false; StopAllCoroutines(); ClearAllPatterns(); RecoverAgent(); }

    public void ClearAllPatterns()
    {
        // 1. 모든 메인 루프 및 공격 코루틴 즉시 중단
        StopAllCoroutines();

        // 2. 비행 패턴 관련 상태 리셋
        isSpecial4Active = false;
        currentBurstCount = 0;

        // 3. 내비게이션 에이전트 복구 및 정지
        if (agent != null)
        {
            agent.enabled = true; // 꺼져있을 수 있는 에이전트 활성화

            // 공중에 있거나 NavMesh 밖일 경우를 대비해 가장 가까운 바닥으로 Warp
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        // 4. 물리/시각 효과 복구
        transform.rotation = Quaternion.identity; // 회전 연출 초기화
        ToggleVisuals(true); // 은신 중이었다면 다시 보이게

        // 5. 모든 소환물 및 패턴 오브젝트 제거

        // 드론 스포너 제거
        if (currentSpawner != null)
        {
            Destroy(currentSpawner);
            currentSpawner = null;
        }

        // 풍선 그룹 비활성화
        if (balloonGroup != null)
        {
            balloonGroup.SetActive(false);
        }

        // 생성된 분신들 제거
        if (activeClones != null && activeClones.Count > 0)
        {
            foreach (var clone in activeClones)
            {
                if (clone != null) Destroy(clone.gameObject);
            }
            activeClones.Clear();
        }

        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // 이름에 "Boss2Clone" 또는 "Type3Shoot" 등이 포함되어 있는지 확인
            // Contains를 사용하면 이름 뒤의 (Clone)을 신경 쓰지 않아도 됩니다.
            if (obj.name.Contains("pt1_pillar") || obj.name.Contains("SafeZone") || obj.name.Contains("T3S") || obj.name.Contains("Card_Attack") || obj.name.Contains("Card_Throw") || obj.name.Contains("WarningSign") || obj.name.Contains("Пешка_7") || obj.name.Contains("Пешка_5_2") )
            {
                // 보스 본체는 지워지면 안 되므로 체크 (보스 본체 이름이 "Boss2"라고 가정)
                if (obj.gameObject != this.gameObject)
                {
                    Destroy(obj);
                }
            }
        }

        // 남아있는 투사체나 임시 오브젝트가 있다면 태그 기반으로 추가 정리 가능 (선택 사항) 
        // GameObject[] projectiles = GameObject.FindGameObjectsWithTag("BossProjectile"); 태그보다 이름으로 찾아보기! 
        // foreach (var p in projectiles) Destroy(p);

        Debug.Log("<color=cyan>[BossController2]</color> 모든 패턴 요소 클린업 완료.");
    }
}