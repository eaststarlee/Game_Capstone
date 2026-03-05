using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public GameObject dashIndicator;
    public GameObject wideAreaQuad;
    public GameObject platformGroup;
    public GameObject explosionEffect;
    public Transform playerTransform;
    private CharacterController playerController;
    private NavMeshAgent agent;
    private BossHealth health;

    [Header("보호 시스템")]
    public GameObject weakPointShield;
    public GameObject initialInvisibleBarrier;
    public GameObject pathBlocker;

    [Header("거리 설정")]
    public float detectionRange = 15f;
    public float releaseRange = 35f;

    [Header("위치 설정")]
    public Transform bossCenterAnchor;
    public float flyHeight = 15f;
    public float pattern4Height = 20f;

    [Header("패턴 추가: 소환")]
    public BossDroneSpawner ghostSpawner;

    private bool isUIVisible = false;
    private Vector3 initialPosition;

    [Header("Default 상태 설정")]
    public float defaultDuration = 5.0f;
    private float defaultTimer = 0f;
    private bool isPatternRunning = false;
    private float normalSpeed;

    [Header("1번 패턴 설정 (돌진)")]
    public float dashSpeed = 35f;
    public float dashAcceleration = 500f;
    public float dashDistance = 15f;
    public float indicatorWidth = 2f;
    public GameObject firePrefab;
    public float fireInterval = 1.0f;

    [Header("3번 패턴 설정 (블랙홀)")]
    public float pullStrength = 8f;
    public float spinSpeed = 1000f;
    public float pattern3Duration = 15f;

    [Header("4번 패턴 설정 (타임어택)")]
    public GameObject jumpMapPlatforms;
    public Image explosionTimerBar;
    public float pattern4Elapsed = 0f;
    public GameObject bigExplosionEffect;
    public float timeLimit = 15f;

    private BossUIController uiController;

    void Awake()
    {
        uiController = Object.FindFirstObjectByType<BossUIController>();
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<BossHealth>();
        normalSpeed = agent.speed;

        if (uiController != null) uiController.SetVisible(false);

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform != null)
            playerController = playerTransform.GetComponent<CharacterController>();

        if (initialInvisibleBarrier != null)
            initialInvisibleBarrier.SetActive(true);

        if (health != null)
        {
            health.OnGroggyStart += HandleGroggyStart;
            health.OnGroggyEnd += HandleGroggyEnd;
        }

        initialPosition = transform.position;
        DisableAllPatternObjects();
    }

    void Update()
    {
        if (health.currentStatus == BossHealth.BossState.Defeated || playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (!isUIVisible)
        {
            if (dist <= detectionRange) StartCombat();
        }
        else
        {
            if (dist > releaseRange) ResetBossCombat();
        }

        if (isUIVisible && !isPatternRunning && health.currentStatus == BossHealth.BossState.Normal)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }

            defaultTimer += Time.deltaTime;
            if (defaultTimer >= defaultDuration)
            {
                defaultTimer = 0f;
                StartCoroutine(StartFullPatternSequence());
            }
        }
    }

    IEnumerator StartFullPatternSequence()
    {
        isPatternRunning = true;

        if (weakPointShield != null) weakPointShield.SetActive(true);

        // 1. 돌진 (플레이어 경직 포함)
        yield return StartCoroutine(StartPattern1());

        // 2. 공중 광역기
        yield return StartCoroutine(StartPattern2());

        // 3. 소환 패턴 (패턴 2가 확실히 끝난 후 실행)
        yield return StartCoroutine(StartPatternSummon());

        // 4. 블랙홀
        yield return StartCoroutine(StartPattern3());

        if (weakPointShield != null) weakPointShield.SetActive(false);

        // 5. 타임어택 전멸기
        yield return StartCoroutine(StartPattern4());

        isPatternRunning = false;
    }

    // --- 수정된 패턴 1: 플레이어 경직(Stun) 복구 ---
    IEnumerator StartPattern1()
    {
        for (int i = 0; i < 4; i++)
        {
            if (health.currentStatus != BossHealth.BossState.Normal) yield break;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            float timer = 0f;
            while (timer < 2f) { LookAtPlayer(); timer += Time.deltaTime; yield return null; }

            // [경직 시작] 플레이어 움직임 봉쇄
            if (playerController != null) playerController.enabled = false;

            if (dashIndicator != null)
            {
                dashIndicator.transform.localPosition = new Vector3(0, 0, dashDistance / 2f);
                dashIndicator.transform.localScale = new Vector3(indicatorWidth, dashDistance, 1f);
                dashIndicator.SetActive(true);
            }

            yield return new WaitForSeconds(0.8f);

            if (dashIndicator != null) dashIndicator.SetActive(false);

            // [경직 해제] 보스가 돌진하기 직전에 다시 풀어줌
            if (playerController != null) playerController.enabled = true;

            Vector3 startPos = transform.position;
            Vector3 finalDashTarget = transform.position + transform.forward * dashDistance;

            agent.isStopped = false;
            agent.speed = dashSpeed;
            agent.acceleration = dashAcceleration;
            agent.SetDestination(finalDashTarget);

            float dashTimeout = 0f;
            while (agent.pathPending || agent.remainingDistance > 0.5f)
            {
                dashTimeout += Time.deltaTime;
                if (dashTimeout > 1.5f) break;
                yield return null;
            }

            SpawnFireTrail(startPos, transform.position);
            agent.velocity = Vector3.zero;
            yield return new WaitForSeconds(1.0f);

            agent.speed = normalSpeed;
            agent.acceleration = 8f;
        }
    }

    // --- 수정된 패턴 2: 착지 안전장치 강화 ---
    IEnumerator StartPattern2()
    {
        uiController?.ShowPatternMessage("보스가 지면 폭격을 준비 중입니다! 가장 높은 발판 위로 대피하세요!");
        agent.enabled = false;
        Vector3 targetAirPos = bossCenterAnchor.position + Vector3.up * flyHeight;
        float moveTime = 0f;
        Vector3 startPos = transform.position;

        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetAirPos, moveTime / 2f);
            yield return null;
        }

        if (platformGroup != null) platformGroup.SetActive(true);
        if (wideAreaQuad != null)
        {
            wideAreaQuad.SetActive(true);
            float scaleTimer = 0f;
            while (scaleTimer < 7f)
            {
                scaleTimer += Time.deltaTime;
                wideAreaQuad.transform.localScale = Vector3.Lerp(new Vector3(0.1f, 0.1f, 5f), new Vector3(1f, 1f, 5f), scaleTimer / 7f);
                yield return null;
            }
        }

        if (explosionEffect != null)
        {
            explosionEffect.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            explosionEffect.SetActive(false);
        }

        if (wideAreaQuad != null) wideAreaQuad.SetActive(false);
        if (platformGroup != null) platformGroup.SetActive(false);
        ClearBlueInkDecals();

        // 착지
        moveTime = 0f;
        Vector3 airPos = transform.position;
        Vector3 groundPos = new Vector3(bossCenterAnchor.position.x, initialPosition.y, bossCenterAnchor.position.z);
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, moveTime / 2f);
            yield return null;
        }

        agent.enabled = true;
        // 핵심: 에이전트가 완벽하게 NavMesh에 안착할 때까지 "한 프레임 더" 대기
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => agent.isOnNavMesh);
    }

    IEnumerator StartPatternSummon()
    {
        Debug.Log("소환 패턴 실행됨!");
        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        yield return new WaitForSeconds(1.0f);

        if (ghostSpawner != null)
        {
            ghostSpawner.SummonWave();
        }
        else
        {
            Debug.LogError("GhostSpawner가 연결되지 않았습니다!");
        }

        yield return new WaitForSeconds(2.0f);

        if (agent.isActiveAndEnabled) agent.isStopped = false;
    }

    // --- 나머지 패턴 및 함수는 이전과 동일하지만 흐름 끊김 방지를 위해 포함 ---
    IEnumerator StartPattern3()
    {
        float timer = 0f;
        if (agent.isActiveAndEnabled) { agent.isStopped = true; agent.velocity = Vector3.zero; }
        Quaternion initRot = transform.rotation;
        while (timer < pattern3Duration)
        {
            if (health.currentStatus != BossHealth.BossState.Normal) yield break;
            timer += Time.deltaTime;
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
            if (playerTransform != null && playerController != null)
            {
                Vector3 pullDir = (transform.position - playerTransform.position).normalized;
                pullDir.y = 0;
                playerController.Move(pullDir * pullStrength * Time.deltaTime);
            }
            yield return null;
        }
        transform.rotation = initRot;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator StartPattern4()
    {
        uiController?.ShowPatternMessage("보스가 피할 수 없는 폭발을 충전 중입니다! 보스 머리 위 약점을 공격해 멈추세요!");
        agent.enabled = false;
        Vector3 targetPos4 = bossCenterAnchor.position + Vector3.up * pattern4Height;
        float moveTime = 0f;
        Vector3 startPos = transform.position;
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos4, moveTime / 2f);
            yield return null;
        }
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(true);
        if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(true);

        pattern4Elapsed = 0f;
        while (pattern4Elapsed < timeLimit)
        {
            // 그로기 상태가 되면 루프 탈출
            if (health.currentStatus != BossHealth.BossState.Normal) break;

            pattern4Elapsed += Time.deltaTime;
            if (explosionTimerBar != null) explosionTimerBar.fillAmount = pattern4Elapsed / timeLimit;
            yield return null;
        }
        // [중요 추가] 루프를 빠져나오자마자 변수를 0으로 리셋하여 UI가 꺼지게 함
        pattern4Elapsed = 0f;

        if (health.currentStatus == BossHealth.BossState.Normal && bigExplosionEffect != null)
        {
            bigExplosionEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            bigExplosionEffect.SetActive(false);
        }

        if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(false);
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);

        moveTime = 0f;
        Vector3 airPos = transform.position;
        Vector3 groundPos = new Vector3(bossCenterAnchor.position.x, initialPosition.y, bossCenterAnchor.position.z);
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, moveTime / 2f);
            yield return null;
        }
        agent.enabled = true;
        yield return new WaitUntil(() => agent.isOnNavMesh);
    }

    void HandleGroggyStart()
    {
        if (playerController != null) playerController.enabled = true; // 그로기 시 플레이어 경직 강제 해제
        pattern4Elapsed = 0f;
        DisableAllPatternObjects();
        StopAllCoroutines();
        StartCoroutine(GroggyDownAnimation());
    }

    void HandleGroggyEnd() { agent.enabled = true; isPatternRunning = false; defaultTimer = 0f; }

    IEnumerator GroggyDownAnimation()
    {
        Vector3 groundPos = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
        float fallSpeed = 0f;
        while (Vector3.Distance(transform.position, groundPos) > 0.1f)
        {
            fallSpeed += Time.deltaTime * 25f;
            transform.position = Vector3.MoveTowards(transform.position, groundPos, fallSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    // 블루 잉크 데칼을 모두 찾아 지우는 함수
    private void ClearBlueInkDecals()
    {
        // 씬에 있는 모든 게임 오브젝트를 탐색 (성능을 위해 태그를 쓰는 것이 좋지만, 이름으로도 가능합니다)
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // 이름에 "BlueInkDecalObject"가 포함되어 있는지 확인
            if (obj.name.Contains("BlueInkDecalObject"))
            {
                Destroy(obj);
            }
        }
        Debug.Log("패턴 2 종료: 모든 BlueInkDecalObject를 제거했습니다.");
    }

    void SpawnFireTrail(Vector3 start, Vector3 end)
    {
        if (firePrefab == null) return;
        float distance = Vector3.Distance(start, end);
        int fireCount = Mathf.Max(1, Mathf.FloorToInt(distance / fireInterval));
        for (int i = 0; i <= fireCount; i++)
        {
            Vector3 spawnPos = Vector3.Lerp(start, end, (float)i / fireCount);
            GameObject fire = Instantiate(firePrefab, spawnPos, Quaternion.identity);
            Destroy(fire, 7f);
        }
    }

    private void StartCombat()
    {
        isUIVisible = true;
        uiController?.SetVisible(true);

        // 전투 시작 시 투명 배리어 해제 (이제부터 타격 가능)
        if (initialInvisibleBarrier != null)
        {
            initialInvisibleBarrier.SetActive(false);
            Debug.Log("보스 전투 시작: 투명 배리어가 해제되었습니다.");
        }

        if (pathBlocker != null)
        {
            pathBlocker.SetActive(false);
            Debug.Log("보스전 시작: 진입로가 차단/비활성화되었습니다.");
        }
    }

    private void ResetBossCombat()
    {
        StopAllCoroutines();
        if (playerController != null) playerController.enabled = true;

        isPatternRunning = false;
        isUIVisible = false;
        uiController?.SetVisible(false);
        defaultTimer = 0f;
        health.ResetHP();
        if (initialInvisibleBarrier != null)
        {
            initialInvisibleBarrier.SetActive(true);
        }
        if (pathBlocker != null)
        {
            pathBlocker.SetActive(true);
            Debug.Log("전투 리셋: 진입로가 다시 활성화되었습니다.");
        }
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = normalSpeed;
            // initialPosition 대신 Boss Center Anchor의 위치로 이동 명령
            if (bossCenterAnchor != null)
            {
                agent.SetDestination(bossCenterAnchor.position);
            }
            else
            {
                agent.SetDestination(initialPosition);
            }
        }

        DisableAllPatternObjects();
    }

    private void DisableAllPatternObjects()
    {
        if (dashIndicator != null) dashIndicator.SetActive(false);
        if (wideAreaQuad != null) wideAreaQuad.SetActive(false);
        if (platformGroup != null) platformGroup.SetActive(false);
        if (explosionEffect != null) explosionEffect.SetActive(false);
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);
        if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(false);
    }
}