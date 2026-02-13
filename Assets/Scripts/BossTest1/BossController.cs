using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public GameObject dashIndicator;   // 패턴 1용 Quad
    public GameObject wideAreaQuad;    // 패턴 2용 광역 Quad
    public GameObject platformGroup;   // 패턴 2 회피용 발판
    public GameObject explosionEffect; // 패턴 2 폭발 이펙트
    public Transform playerTransform;
    private CharacterController playerController;
    private NavMeshAgent agent;
    private BossHealth health;         // 보스 체력 스크립트 참조

    [Header("보호 시스템")]
    public GameObject weakPointShield; // 약점을 감싸고 있는 보호막 오브젝트

    [Header("거리 설정 (이격 시스템)")]
    public float detectionRange = 15f; // 처음 발견하는 거리 (작게)
    public float releaseRange = 35f;   // 전투를 포기하는 거리 (크게)

    [Header("위치 설정")]
    public Transform bossCenterAnchor; // 보스룸 중앙 기준점
    public float flyHeight = 15f;      // 패턴 2 높이
    public float pattern4Height = 20f; // 패턴 4 높이

    private bool isUIVisible = false;
    private Vector3 initialPosition; // 보스의 처음 위치 저장

    [Header("Default 상태 설정")]
    public float defaultDuration = 5.0f;
    private float defaultTimer = 0f;
    private bool isPatternRunning = false;
    private float normalSpeed;

    [Header("1번 패턴 설정")]
    public float dashSpeed = 35f;
    public float dashAcceleration = 500f;
    public float dashDistance = 15f;
    public float indicatorWidth = 2f;
    public GameObject firePrefab;
    public float fireInterval = 1.0f;

    [Header("3번 패턴 설정")]
    public float pullStrength = 8f;
    public float spinSpeed = 1000f;
    public float pattern3Duration = 15f;

    [Header("4번 패턴 설정 (타임어택)")]
    public GameObject jumpMapPlatforms;  // 패턴 4 타임어택용 점프맵 그룹
    public Image explosionTimerBar; // 패턴 4 타이머용 UI Image (Filled)
    public float pattern4Elapsed = 0f; // 현재 얼마나 시간이 흘렀는지 (외부 참조용)
    public GameObject bigExplosionEffect; // 패턴 4 전멸기 폭발
    public float timeLimit = 15f;        // 전멸기 제한 시간

    private BossUIController uiController;

    void Awake()
    {
        // 최신 함수 사용 (Awake나 Start에서 한 번만 호출)
        uiController = Object.FindFirstObjectByType<BossUIController>();
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<BossHealth>();
        normalSpeed = agent.speed;

        if (uiController != null)
        {
            uiController.SetVisible(false);
            isUIVisible = false; // 확실히 하기 위해 다시 한번 명시
        }

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform != null)
            playerController = playerTransform.GetComponent<CharacterController>();

        if (health != null)
        {
            health.OnGroggyStart += HandleGroggyStart;
            health.OnGroggyEnd += HandleGroggyEnd;
        }

        // 초기 비활성화
        if (dashIndicator != null) dashIndicator.SetActive(false);
        if (wideAreaQuad != null) wideAreaQuad.SetActive(false);
        if (platformGroup != null) platformGroup.SetActive(false);
        if (explosionEffect != null) explosionEffect.SetActive(false);
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);

        initialPosition = transform.position; // 시작 위치(지상) 저장
    }

    void Update()
    {
        if (health.currentStatus == BossHealth.BossState.Defeated || playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        // [핵심 로직] 상태에 따른 거리 체크
        if (!isUIVisible) // 아직 인식 전이라면
        {
            if (dist <= detectionRange) // 좁은 범위 안에 들어오면 인식!
            {
                StartCombat();
            }
        }
        else // 이미 전투 중이라면
        {
            if (dist > releaseRange) // 넓은 범위를 완전히 벗어나야 리셋!
            {
                ResetBossCombat();
            }
        }

        // 전투 중이고 패턴 중이 아닐 때만 추격
        if (isUIVisible && !isPatternRunning)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);

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

        // 패턴 1~3은 보호막 유지
        if (weakPointShield != null) weakPointShield.SetActive(true);

        yield return StartCoroutine(StartPattern1());
        yield return StartCoroutine(StartPattern2());
        yield return StartCoroutine(StartPattern3());

        // 패턴 4 진입 시 보호막 해제 (약점 노출!)
        if (weakPointShield != null) weakPointShield.SetActive(false);

        yield return StartCoroutine(StartPattern4());

        isPatternRunning = false;
    }


    // --- 패턴 1: 돌진 ---
    IEnumerator StartPattern1()
    {
        for (int i = 0; i < 4; i++)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            float timer = 0f;
            while (timer < 3f)
            {
                LookAtPlayer();
                timer += Time.deltaTime;
                yield return null;
            }

            if (playerController != null) playerController.enabled = false;
            if (dashIndicator != null)
            {
                dashIndicator.transform.localPosition = new Vector3(0, 0, dashDistance / 2f);
                dashIndicator.transform.localScale = new Vector3(indicatorWidth, dashDistance, 1f);
                dashIndicator.SetActive(true);
            }

            yield return new WaitForSeconds(0.8f);
            if (dashIndicator != null) dashIndicator.SetActive(false);
            if (playerController != null) playerController.enabled = true;

            Vector3 startPos = transform.position;
            Vector3 finalDashTarget = transform.position + transform.forward * dashDistance;

            agent.isStopped = false;
            agent.speed = dashSpeed;
            agent.acceleration = dashAcceleration;
            agent.SetDestination(finalDashTarget);

            float dashTimeout = 0f;
            while (agent.pathPending || agent.remainingDistance > 0.1f)
            {
                dashTimeout += Time.deltaTime;
                if (dashTimeout > 1.5f) break;
                yield return null;
            }

            SpawnFireTrail(startPos, transform.position);
            agent.speed = 0;
            yield return new WaitForSeconds(1.5f);

            agent.speed = normalSpeed;
            agent.acceleration = 8f;
        }
    }

    // --- 패턴 2: 광역 공중 공격 ---
    IEnumerator StartPattern2()
    {
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
            Vector3 startScale = new Vector3(0.1f, 0.1f, 5f);
            Vector3 endScale = new Vector3(1f, 1f, 5f);
            while (scaleTimer < 10f)
            {
                scaleTimer += Time.deltaTime;
                wideAreaQuad.transform.localScale = Vector3.Lerp(startScale, endScale, scaleTimer / 10f);
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

        moveTime = 0f;
        Vector3 airPos = transform.position;
        Vector3 groundPos = bossCenterAnchor.position;
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, moveTime / 2f);
            yield return null;
        }
        agent.enabled = true;
    }

    // --- 패턴 3: 블랙홀 회전 ---
    IEnumerator StartPattern3()
    {
        float timer = 0f;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        Quaternion initialRotation = transform.rotation;

        while (timer < pattern3Duration)
        {
            timer += Time.deltaTime;
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
            if (playerTransform != null && playerController != null)
            {
                Vector3 pullDirection = (transform.position - playerTransform.position).normalized;
                pullDirection.y = 0;
                playerController.Move(pullDirection * pullStrength * Time.deltaTime);
            }
            yield return null;
        }
        transform.rotation = initialRotation;
        yield return new WaitForSeconds(1f);
    }

    // --- [수정] 패턴 4: 타임어택 점프맵 ---
    IEnumerator StartPattern4()
    {
        Debug.Log("패턴 4 시작: 공중으로 상승");
        agent.enabled = false;

        Vector3 targetPos4 = bossCenterAnchor.position + Vector3.up * pattern4Height;

        // 1. 부드럽게 상승 (수정됨)
        float moveTime = 0f;
        Vector3 startPos = transform.position;
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos4, moveTime / 2f);
            yield return null;
        }
        transform.position = targetPos4;

        // 2. 타임어택 개시 (기존 15초 혹은 30초 대기 로직 대체)
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(true);
        if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(true);

        pattern4Elapsed = 0f; // ★ UI 참조용 변수 초기화

        float elapsed = 0f;
        while (elapsed < timeLimit)
        {
            elapsed += Time.deltaTime;
            pattern4Elapsed = elapsed; // ★ 매 프레임 업데이트하여 UI에 전달

            // 타이머 바 갱신
            if (explosionTimerBar != null)
            {
                explosionTimerBar.fillAmount = elapsed / timeLimit;

                // 80% 이상 경과 시 깜빡임 연출
                if (elapsed / timeLimit > 0.8f)
                {
                    explosionTimerBar.color = (Mathf.FloorToInt(Time.time * 10) % 2 == 0) ? Color.red : Color.white;
                }
            }
            yield return null;
        }

        // 3. 30초 생존 시 전멸기 발동 (그로기가 안 터졌을 때만 실행됨)
        if (bigExplosionEffect != null)
        {
            bigExplosionEffect.SetActive(true);
            Debug.Log("<color=red>전멸기 폭발!</color>");
            yield return new WaitForSeconds(2f);
            bigExplosionEffect.SetActive(false);

            // 전멸기가 시작되자마자 타이머 UI는 역할을 다했으므로 끕니다.
            if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(false);
            pattern4Elapsed = 0f;
        }

        // 4. [추가] 패턴 종료 후 지상으로 복귀
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);

        Debug.Log("패턴 4 종료: 지상으로 착지");
        moveTime = 0f;
        Vector3 airPos = transform.position;
        Vector3 groundPos = new Vector3(bossCenterAnchor.position.x, 0f, bossCenterAnchor.position.z);
        while (moveTime < 2f)
        {
            moveTime += Time.deltaTime;
            transform.position = Vector3.Lerp(airPos, groundPos, moveTime / 2f);
            yield return null;
        }
        transform.position = groundPos;

        agent.enabled = true;
    }

    void HandleGroggyStart()
    {
        if (dashIndicator != null) dashIndicator.SetActive(false);
        if (wideAreaQuad != null) wideAreaQuad.SetActive(false);
        if (platformGroup != null) platformGroup.SetActive(false);
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);
        if (explosionEffect != null) explosionEffect.SetActive(false);

        // [추가] 패턴 4 관련 UI 및 플랫폼 즉시 정리
        pattern4Elapsed = 0f;
        if (explosionTimerBar != null)
        {
            explosionTimerBar.fillAmount = 0f;
            explosionTimerBar.gameObject.SetActive(false);
        }
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);

        if (explosionTimerBar != null) explosionTimerBar.gameObject.SetActive(false);
        pattern4Elapsed = 0f;

        StopAllCoroutines();
        StartCoroutine(GroggyDownAnimation());
    }

    void HandleGroggyEnd()
    {
        agent.enabled = true;
        isPatternRunning = false;
        defaultTimer = 0f; // 초기화하여 추격부터 시작
    }

    IEnumerator GroggyDownAnimation()
    {
        // 현재 위치에서 수직으로 바닥까지 추락
        Vector3 groundPos = new Vector3(transform.position.x, 0f, transform.position.z);
        float fallSpeed = 0f;
        while (Vector3.Distance(transform.position, groundPos) > 0.1f)
        {
            fallSpeed += Time.deltaTime * 25f;
            transform.position = Vector3.MoveTowards(transform.position, groundPos, fallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = groundPos;
        Debug.Log("보스 추락 완료");
    }

    // --- 유틸리티 함수 ---
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

    void SpawnFireTrail(Vector3 start, Vector3 end)
    {
        if (firePrefab == null) return;
        float distance = Vector3.Distance(start, end);
        int fireCount = Mathf.Max(1, Mathf.FloorToInt(distance / fireInterval));
        for (int i = 0; i <= fireCount; i++)
        {
            Vector3 spawnPos = Vector3.Lerp(start, end, (float)i / fireCount);
            GameObject fire = Instantiate(firePrefab, spawnPos, Quaternion.identity);
            Destroy(fire, 10f);
        }
    }

    // 모든 상태를 초기화하고 중앙으로 복귀시키는 함수
    private void StartCombat()
    {
        isUIVisible = true;
        uiController?.SetVisible(true);
        Debug.Log("전투 개시!");
    }

    private void ResetBossCombat()
    {
        StopAllCoroutines();
        isPatternRunning = false;
        isUIVisible = false;
        uiController?.SetVisible(false);

        defaultTimer = 0f;
        health.ResetHP();

        // 1. 공중 위치 및 중력/에이전트 복구
        agent.enabled = true; // 에이전트 다시 활성화

        // 2. 강제 착지: Y축 값을 초기 시작 위치로 고정
        Vector3 resetPos = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
        transform.position = resetPos;

        // 3. 중앙 앵커로 복귀 명령
        if (bossCenterAnchor != null)
        {
            agent.SetDestination(bossCenterAnchor.position);
        }

        // 4. 모든 패턴 기믹 끄기
        DisableAllPatternObjects();

        Debug.Log("<color=blue>보스 리셋: 지상 복귀 및 앵커 이동</color>");
    }
    private void DisableAllPatternObjects()
    {
        if (dashIndicator != null) dashIndicator.SetActive(false);
        if (wideAreaQuad != null) wideAreaQuad.SetActive(false);
        if (platformGroup != null) platformGroup.SetActive(false);
        if (explosionEffect != null) explosionEffect.SetActive(false);
        if (jumpMapPlatforms != null) jumpMapPlatforms.SetActive(false);
    }
}