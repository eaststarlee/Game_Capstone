using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BC2Test : MonoBehaviour
{
    [Header("테스트 설정 (숫자키 1~7 이용)")]
    public Transform playerTransform;
    public Transform mapCenter;

    private NavMeshAgent agent;
    public BossHealth2 health;
    private BossUIController2 uiController;
    private bool isTestingPattern = false; // 패턴 실행 중 중복 실행 방지

    [Header("프리팹 연결 (BossController2와 동일하게 설정)")]
    public GameObject cardMeleePrefab;
    public GameObject cardProjectilePrefab;
    public GameObject shockwavePrefab;
    public GameObject spinningPrefab;      // 충전 중 회전 연출 오브젝트
    public GameObject flashEffectPrefab; // 시각적 이펙트 프리팹
    public GameObject pillarObjectPrefab;  // 회피용 기둥
    public GameObject safeZonePrefab; // 기둥 안착 후 활성화될 안전 구역 표시
    public GameObject[] powerfulAttackPrefabs; // 5개의 폭발 프리팹 (순차 활성화용)
    public GameObject[] fallingObjectPrefabs;
    public GameObject vanishEffect;
    public GameObject redWarningPrefab;
    public GameObject bossClonePrefab;
    public GameObject Type3Shoot;
    private List<Boss2Clone> activeClones = new List<Boss2Clone>();
    public GameObject balloonGroup;
    public GameObject droneSpawnerPrefab; // 드론 스포너 프리팹 (오브젝트 아님)
    private GameObject currentSpawner;    // 생성된 스포너 저장용

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // 1. 보스 헬스 스크립트 자동 찾기
        if (health == null)
            health = GetComponent<BossHealth2>();

        // 2. 플레이어 태그 확인
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        // [중요] health가 할당되지 않았을 경우를 대비한 Null 체크
        if (health == null) return;

        // 보스가 'Normal' 상태일 때만 로직 실행
        if (health.currentStatus == BossHealth2.BossState.Normal)
        {
            if (isTestingPattern) return;

            // 에이전트 유효성 확인
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                if (playerTransform != null)
                    agent.SetDestination(playerTransform.position);

                HandleTestInput();
            }
        }
        else
        {
            // Idle 등 상태일 때 멈춤
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }
    }

    // 키 입력 부분은 Update를 깔끔하게 유지하기 위해 분리
    private void HandleTestInput()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) StartTest(BasicAttackType1());
        if (Input.GetKeyDown(KeyCode.Keypad2)) StartTest(BasicAttackType2());
        if (Input.GetKeyDown(KeyCode.Keypad3)) StartTest(BasicAttackType3());
        if (Input.GetKeyDown(KeyCode.Keypad4)) StartTest(SpecialType1());
        if (Input.GetKeyDown(KeyCode.Keypad5)) StartTest(SpecialType2());
        if (Input.GetKeyDown(KeyCode.Keypad6)) StartTest(SpecialType3());
        if (Input.GetKeyDown(KeyCode.Keypad7)) StartTest(SpecialType4());
    }

    public void ClearCurrentPatternElements()
    {
        StopAllCoroutines();
        isTestingPattern = false; // [중요] 패턴 실행 중 변수 강제 리셋

        if (agent != null)
        {
            agent.enabled = true; // [중요] 꺼져있던 에이전트 강제 활성화

            // 에이전트 활성화 직후 Warp를 써야 안전합니다.
            // 현재 위치가 NavMesh 위가 아닐 수 있으므로 가장 가까운 점으로 복구
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
        // 1. 실행 중인 모든 코루틴 중단 (패턴 및 이동 멈춤)
        StopAllCoroutines();
        isTestingPattern = false;

        // 2. 물리 및 내비게이션 초기화
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 3. 소환물 제거 (드론 스포너 등)
        if (currentSpawner != null)
        {
            Destroy(currentSpawner);
        }

        // 4. 비행 패턴용 풍선 그룹 비활성화
        if (balloonGroup != null)
        {
            balloonGroup.SetActive(false);
        }

        // 5. 분신 제거
        foreach (var clone in activeClones)
        {
            if (clone != null) Destroy(clone.gameObject);
        }
        activeClones.Clear();

        Debug.Log("<color=red>[BC2Test]</color> 모든 패턴 요소 클린업 완료");
    }

    // 테스트용 코루틴 실행 래퍼
    void StartTest(IEnumerator pattern)
    {
        isTestingPattern = true;
        agent.isStopped = true;
        StartCoroutine(RunPattern(pattern));
    }

    IEnumerator RunPattern(IEnumerator pattern)
    {
        yield return StartCoroutine(pattern);
        isTestingPattern = false;
        Debug.Log("테스트 패턴 종료 - 다시 탐색 상태로 돌아갑니다.");
    }

    // --- 아래에 구현할 스킬들을 그대로 복사/작성 합니다 ---

    IEnumerator BasicAttackType1()
    {
        Debug.Log("Test: 기본 공격 1 실행 (예측 도약 + 추격 전진)");

        // 1. 예측 위치 계산 (Anticipation)
        // 플레이어의 현재 속도를 기반으로 0.5초 뒤의 위치를 예측 (간단한 선형 예측)
        // CharacterController나 Rigidbody의 velocity를 활용하면 더 정확합니다.
        Vector3 playerVelocity = playerTransform.GetComponent<CharacterController>()?.velocity ?? Vector3.zero;

        // 너무 멀리 예측하지 않도록 최대치 제한 (예: 2m)
        Vector3 predictedPos = playerTransform.position + Vector3.ClampMagnitude(playerVelocity * 0.5f, 2f);
        Vector3 playerFloorPos = new Vector3(predictedPos.x, transform.position.y, predictedPos.z);

        // 도약 타겟: 예측 지점보다 1.5m 뒤를 찍어서 플레이어를 관통하거나 압박
        Vector3 jumpTarget = playerFloorPos + (playerFloorPos - transform.position).normalized * 1.5f;

        // 2. 도약 및 준비
        transform.LookAt(playerFloorPos);
        yield return StartCoroutine(MoveToPosition(transform.position + Vector3.up * 3f, 0.3f));
        yield return StartCoroutine(MoveToPosition(jumpTarget, 0.35f)); // 더 빨라진 도약 속도

        // 3. 휘두르기 실행 (공격 중 전진 포함)
        if (cardMeleePrefab != null)
        {
            // 소환 위치 설정 (보스 정면)
            Vector3 centerPos = transform.position + Vector3.up * 0.2f;
            Vector3 startDir = Quaternion.Euler(0, -90f, 0) * transform.forward; // 왼쪽 90도 시작
            Vector3 spawnPos = centerPos + startDir * 0f; // 중심점에서 생성

            GameObject melee = Instantiate(cardMeleePrefab, spawnPos, transform.rotation);
            // 카드가 보스를 따라다니도록 부모 설정 (공격 중 전진 시 궤적 유지)
            melee.transform.SetParent(this.transform);

            float swingDuration = 0.25f;
            float totalAngle = 360;
            float moveDistance = 1.5f; // 휘두르는 동안 전진할 거리
            float elapsed = 0f;

            Vector3 moveStartPos = transform.position;
            Vector3 moveEndPos = transform.position + transform.forward * moveDistance;

            while (elapsed < swingDuration)
            {
                float t = elapsed / swingDuration;
                float step = (totalAngle / swingDuration) * Time.deltaTime;

                // [추격] 보스 자체가 전방으로 미끄러짐
                transform.position = Vector3.Lerp(moveStartPos, moveEndPos, t);

                // [공전] 카드는 미끄러지는 보스(부모)를 기준으로 회전
                // 갱신된 보스 위치를 기준으로 RotateAround 수행
                Vector3 currentCenter = transform.position + Vector3.up * 0.5f;
                melee.transform.RotateAround(currentCenter, Vector3.up, step);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 상위 오브젝트 해제 후 제거
            melee.transform.SetParent(null);
            Destroy(melee, 0.1f);
        }

        yield return new WaitForSeconds(0.6f); // 후딜레이 약간 감소로 긴박함 유지
    }
    IEnumerator BasicAttackType2()
    {
        Debug.Log("Test: 기본 공격 2 실행 (3D 조준 + 2초 후 유도)");

        // 플레이어를 향해 몸을 돌림
        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 3; i++)
        {
            if (cardProjectilePrefab != null)
            {
                // 1. 소환 (보스 머리 위쪽에서 생성)
                Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
                GameObject card = Instantiate(cardProjectilePrefab, spawnPos, Quaternion.identity);

                // 2. 초기 3D 발사 방향 설정 (XYZ 모두 고려)
                Vector3 targetDir = (playerTransform.position + Vector3.up * 0.5f - spawnPos).normalized;
                card.transform.forward = targetDir; // 카드가 날아가는 방향을 바라보게 함

                // 3. 유도 로직 실행 (별도의 코루틴이나 스크립트로 분리)
                StartCoroutine(HomingProjectile(card, targetDir));
            }
            yield return new WaitForSeconds(0.7f); // 발사 간격
        }
        yield return new WaitForSeconds(0.5f);
    }

    // 투사체별 유도 로직
    IEnumerator HomingProjectile(GameObject proj, Vector3 initialDir)
    {
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        float speed = 20f;
        float elapsed = 0f;
        bool isHoming = false;

        // 초기 발사 (2초 동안 직진)
        if (rb != null) rb.linearVelocity = initialDir * speed;

        while (proj != null && elapsed < 3.0f) // 최대 3초 생존
        {
            elapsed += Time.deltaTime;

            // 발사 1초 후부터 유도 시작
            if (elapsed >= 1.0f)
            {
                if (!isHoming)
                {
                    Debug.Log("카드 유도 시작!");
                    isHoming = true;
                    speed *= 0.7f; // 유도 시에는 너무 빠르면 못 피하므로 속도를 약간 조절
                }

                if (playerTransform != null)
                {
                    // 플레이어를 향한 방향 계산
                    Vector3 homingDir = (playerTransform.position + Vector3.up * 0.5f - proj.transform.position).normalized;

                    // 급격한 회전보다는 부드럽게 회전 (Slerp)
                    rb.linearVelocity = Vector3.Slerp(rb.linearVelocity.normalized, homingDir, Time.deltaTime * 5f) * speed;

                    // 투사체 모델의 방향도 진행 방향으로 회전
                    proj.transform.forward = rb.linearVelocity.normalized;
                }
            }
            yield return null;
        }

        if (proj != null) Destroy(proj);
    }

    IEnumerator BasicAttackType3()
    {
        Debug.Log("Test: 기본 공격 3 실행 (3D 대각선 강하)");

        // 1. 에이전트 비활성화 (물리 법칙 무시하고 3D 이동을 하기 위함)
        if (agent != null) agent.enabled = false;

        // 2. 수직 상승
        Vector3 airPos = transform.position + Vector3.up * 8f;
        yield return StartCoroutine(MoveToPosition(airPos, 0.5f));

        // 3. 공중 대기 및 조준
        yield return new WaitForSeconds(0.4f);

        // 4. 대각선 강하 돌진
        // 착지할 높이를 0(또는 지면 높이)으로 고정하여 정확한 대각선 벡터 생성
        Vector3 targetLandPos = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

        float elapsed = 0;
        float duration = 0.35f;
        Vector3 currentAirPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // NavMesh를 타지 않고 Transform 좌표를 직접 선형 보간(Lerp)하여 대각선 이동
            transform.position = Vector3.Lerp(currentAirPos, targetLandPos, elapsed / duration);
            yield return null;
        }
        transform.position = targetLandPos;

        // 5. 착지 판정 및 쇼크웨이브 생성
        if (shockwavePrefab != null)
        {
            // Y축 높이 보정 (필요 시)
            float yOffset = -0.2f;
            Vector3 shockwavePos = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);

            // 변수에 할당하여 생성
            GameObject wave = Instantiate(shockwavePrefab, shockwavePos, Quaternion.identity);

            // [중요] 생성과 동시에 2초 뒤 삭제 예약
            // 이펙트 길이에 맞춰 시간을 조절하세요 (예: 1.5f, 2.0f)
            Destroy(wave, 2.0f);
        }

        // 6. 에이전트 재활성화 (다시 추격을 시작하기 위함)
        if (agent != null)
        {
            agent.enabled = true;
            // 다시 켰을 때 튕기는 현상을 막기 위해 현재 위치를 즉시 베이크
            agent.Warp(transform.position);
        }

        yield return new WaitForSeconds(1f);
    }

    // --- 특수 스킬 뼈대 (앞으로 채워나갈 곳) ---
    IEnumerator SpecialType1()
    {
        Debug.Log("Skill 1: 강력한 내리꽂기 패턴 시작");

        if (agent != null) agent.enabled = false;

        // 1. 공중 상승 및 중앙(0, 0, 10) 이동
        float groundY = transform.position.y;
        Vector3 airCenterPos = new Vector3(0f, groundY + 10f, 0f);
        yield return StartCoroutine(MoveToPosition(airCenterPos, 0.8f));

        // 2. 연출용 spinningPrefab 소환
        GameObject spawnedSpinning = null;
        if (spinningPrefab != null)
        {
            float yOffset = 5.0f;
            Vector3 spinningPos = transform.position + Vector3.up * yOffset;
            spawnedSpinning = Instantiate(spinningPrefab, spinningPos, transform.rotation);
            spawnedSpinning.transform.SetParent(this.transform);

            // 움찔거림 방지를 위해 코루틴으로 직접 회전 제어
            StartCoroutine(RotateSpinningEffect(spawnedSpinning));
        }

        // 3. 회피용 기둥 소환 및 정해진 거리 하강
        if (pillarObjectPrefab != null)
        {
            // 보스(중앙 공중 위치)를 기준으로 오프셋 적용
            Vector3 pillarSpawnPos = transform.position + new Vector3(17f, 41f, 5f);

            GameObject spawnedPillar = Instantiate(pillarObjectPrefab, pillarSpawnPos, Quaternion.identity);

            // y축으로 총 36.5만큼 아래로 이동 (목표 Y값 = 시작 Y - 27.5)
            float targetY = pillarSpawnPos.y - 36.5f;
            StartCoroutine(LowerPillarToGround(spawnedPillar, targetY, 3.0f));

            Destroy(spawnedPillar, 15f);
        }

        // 4. 10초 충전 대기
        yield return new WaitForSeconds(10.0f);

        // 5. 충전 완료 및 연출 오브젝트 파괴
        if (spawnedSpinning != null) Destroy(spawnedSpinning);

        if (flashEffectPrefab != null)
        {
            // 보스 위치(공중)에서 화려한 폭발/섬광 이펙트 생성
            GameObject flash = Instantiate(flashEffectPrefab, transform.position, Quaternion.identity);

            // 이펙트의 수명에 따라 적절히 파괴 (보통 2~3초)
            Destroy(flash, 2.0f);
        }

        // 6. 폭발 오브젝트 순차 실행 (최종 단순화 버전)
        for (int i = 0; i < powerfulAttackPrefabs.Length; i++)
        {
            if (powerfulAttackPrefabs[i] != null)
            {
                // 1. 프리팹 생성 (인스펙터에 설정된 위치/회전값 기반)
                GameObject explosion = Instantiate(
                    powerfulAttackPrefabs[i],
                    powerfulAttackPrefabs[i].transform.position,
                    powerfulAttackPrefabs[i].transform.rotation
                );

                // 2. 활성화 (프리팹이 꺼져있을 경우를 대비)
                explosion.SetActive(true);

                // 3. 설정한 시간 뒤에 자동 파괴
                Destroy(explosion, 0.5f);
            }

            // 4. 다음 폭발까지의 간격
            yield return new WaitForSeconds(0.3f);
        }

        // 7. 보스 하강 및 복구
        Vector3 landPos = new Vector3(0f, groundY, 0f);
        yield return StartCoroutine(MoveToPosition(landPos, 0.4f));

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        Debug.Log("Skill 1: 패턴 종료");
    }

    // --- 추가된 기둥 하강 코루틴 ---
    IEnumerator LowerPillarToGround(GameObject pillar, float targetY, float duration)
    {
        float elapsed = 0;
        Vector3 startPos = pillar.transform.position;
        // X, Z는 유지하고 계산된 targetY로만 이동
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        while (elapsed < duration)
        {
            if (pillar == null) yield break;
            elapsed += Time.deltaTime;
            pillar.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }
        pillar.transform.position = endPos;
        
        // --- 추가된 부분: 기둥 이동 완료 후 안전 구역 생성 ---
        if (safeZonePrefab != null && pillar != null)
        {
            // 기둥의 자식으로 넣거나, 기둥 위치에 맞춰 생성합니다.
            // 기둥이 파괴될 때 같이 사라지도록 기둥을 부모로 설정하는 것을 추천합니다.
            GameObject safeZone = Instantiate(safeZonePrefab, endPos, pillar.transform.rotation);
            safeZone.transform.SetParent(pillar.transform);

            Debug.Log("기둥 안착: 안전 구역 활성화");
        }
    }

    //회전 연출 코루틴
    IEnumerator RotateSpinningEffect(GameObject obj)
    {
        while (obj != null)
        {
            // Y축 기준으로 매 프레임 빠르게 회전 (속도 720은 초당 2바퀴)
            obj.transform.Rotate(Vector3.up * 720f * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator SpecialType2()
    {
        Debug.Log("Skill 2: 연출 후 은신 및 낙하 패턴 시작");

        // 0. 사라지기 전 연출
        if (vanishEffect != null)
        {
            vanishEffect.SetActive(true);
            // 이펙트가 보여질 시간을 줍니다.
            yield return new WaitForSeconds(1.0f);
            vanishEffect.SetActive(false);
        }

        // 1. 보스 은신 및 에이전트 정지
        if (agent != null) agent.enabled = false;

        // 모든 시각적 요소 및 콜라이더 비활성화
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach (var r in allRenderers) r.enabled = false;
        foreach (var c in allColliders) c.enabled = false;

        // --- 스킬 내부 설정 변수 ---
        int totalAttacks = 7;
        float spawnHeight = 50f;
        float fallDuration = 1.7f;
        float actualGroundY = -1.5f;
        float attackInterval = 3f;

        // 2. 낙하 공격 루프
        for (int i = 0; i < totalAttacks; i++)
        {
            if (playerTransform != null && fallingObjectPrefabs != null && fallingObjectPrefabs.Length > 0)
            {
                Vector3 targetPos = new Vector3(playerTransform.position.x, actualGroundY, playerTransform.position.z);
                GameObject randomPrefab = fallingObjectPrefabs[Random.Range(0, fallingObjectPrefabs.Length)];
                StartCoroutine(ExecuteMeteorDrop(randomPrefab, targetPos, spawnHeight, fallDuration));
            }
            yield return new WaitForSeconds(attackInterval);
        }

        // 3. 패턴 종료 대기
        yield return new WaitForSeconds(fallDuration + 0.5f);

        // 4. 보스 복귀 및 재등장 연출
        transform.position = new Vector3(0f, 0f, 0f);

        // 다시 나타날 때도 이펙트를 써주면 더 멋집니다.
        if (vanishEffect != null)
        {
            vanishEffect.SetActive(true);
            // 이펙트와 함께 보스가 나타나도록 0.1~0.2초 정도만 살짝 대기
            yield return new WaitForSeconds(0.2f);
        }

        // 모든 시각적 요소 및 콜라이더 다시 활성화
        foreach (var r in allRenderers) if (r != null) r.enabled = true;
        foreach (var c in allColliders) if (c != null) c.enabled = true;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        // 나타난 후 이펙트 끄기
        if (vanishEffect != null)
        {
            yield return new WaitForSeconds(0.8f); // 이펙트가 유지되다 사라지도록
            vanishEffect.SetActive(false);
        }

        Debug.Log("Skill 2: 패턴 종료 및 복귀");
    }

    IEnumerator ExecuteMeteorDrop(GameObject selectedPrefab, Vector3 targetPos, float height, float duration)
    {
        if (redWarningPrefab == null || selectedPrefab == null) yield break;

        // 1. 경고판 소환
        GameObject warning = Instantiate(redWarningPrefab, targetPos, Quaternion.identity);
        Renderer warningRenderer = warning.GetComponent<Renderer>();

        // 2. 오프셋 계산 (중앙 피벗 보정)
        Collider meteorCollider = selectedPrefab.GetComponentInChildren<Collider>();
        float yOffset = (meteorCollider != null) ? meteorCollider.bounds.extents.y : 0.5f;

        // 3. 낙하 물체 소환 (높이 50m 지점)
        Vector3 finalSpawnPos = targetPos + Vector3.up * (height + yOffset);
        GameObject meteor = Instantiate(selectedPrefab, finalSpawnPos, Quaternion.identity);

        float elapsed = 0f;
        // [자동 계산] 속력 = 거리(50) / 시간(1.7) -> 약 29.4m/s의 빠른 속도로 낙하
        float speed = height / duration;

        // 4. 하강 루프
        while (elapsed < duration && meteor != null)
        {
            elapsed += Time.deltaTime;
            meteor.transform.position += Vector3.down * speed * Time.deltaTime;

            if (warningRenderer != null)
            {
                // 바닥에 가까워질수록 경고판이 선명해짐
                Color c = warningRenderer.material.color;
                c.a = Mathf.Clamp01(elapsed / duration);
                warningRenderer.material.color = c;
            }

            yield return null;
        }

        // 5. 착지 및 정리
        // 5. [수정] 착지 및 자식 이펙트 활성화
        if (meteor != null)
        {
            Vector3 finalLandPos = new Vector3(targetPos.x, targetPos.y + yOffset, targetPos.z);
            meteor.transform.position = finalLandPos;

            // --- 자식 오브젝트인 ImpactEffect 찾아서 활성화 ---
            // 프리팹 내부에 "ImpactEffect"라는 이름의 자식이 있다고 가정합니다.
            Transform impactEffect = meteor.transform.Find("ImpactEffect");
            if (impactEffect != null)
            {
                impactEffect.gameObject.SetActive(true);
            }

            if (warning != null) Destroy(warning);

            // 이펙트가 보여질 시간을 위해 0.5초 뒤에 물체 전체 파괴
            Destroy(meteor, 0.5f);
        }
        else if (warning != null)
        {
            Destroy(warning);
        }
    }
    IEnumerator SpecialType3()
    {
        Debug.Log("Skill 3: 삼각 편대 분신 패턴 시작");

        if (agent != null) agent.enabled = false;

        float orbitRadius = 8f;   // 회전 반경
        float orbitSpeed = 60f;   // 회전 속도
        float patternDuration = 10f;

        // 1. 분신 2체 생성
        activeClones.Clear();
        for (int i = 0; i < 2; i++)
        {
            GameObject cloneObj = Instantiate(bossClonePrefab, transform.position, Quaternion.identity);
            Boss2Clone cloneScript = cloneObj.GetComponent<Boss2Clone>();

            // 분신 초기화: 분신 1은 120도, 분신 2는 240도 오프셋 부여
            cloneScript.projectilePrefab = Type3Shoot;
            cloneScript.Init(playerTransform, orbitRadius, orbitSpeed, (i + 1) * 120f);
            activeClones.Add(cloneScript);
        }

        // 2. 본체 설정 (본체는 0도 오프셋)
        float currentAngle = 0f;
        //bool isPatternActive = true;
        float timer = 0f;

        // 분신들에게 공격 시작 명령
        foreach (var clone in activeClones) clone.StartOrbit();

        // 3. 10초간 공전 및 발사 루프
        float fireTimer = 0f;
        while (timer < patternDuration)
        {
            timer += Time.deltaTime;
            fireTimer += Time.deltaTime;

            // 본체 원형 이동 로직
            currentAngle += orbitSpeed * Time.deltaTime;
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector3 targetPos = playerTransform.position + new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian)) * orbitRadius;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

            // 본체 투사체 발사 (분신과 동일한 주기)
            if (fireTimer >= 1.5f)
            {
                // 분신과 동일하게 1.5m 높이 보정
                Vector3 firePos = transform.position + (Vector3.up * 1.1f) + (transform.forward * 1.0f);
                Instantiate(Type3Shoot, firePos, transform.rotation);
                fireTimer = 0f;
            }

            yield return null;
        }

        // 4. 패턴 종료: 살아남은 분신들 돌진
        Vector3 playerCurrentPos = playerTransform.position;
        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                clone.FinalRush(playerCurrentPos);
            }
        }

        // 본체 복귀
        yield return new WaitForSeconds(0.6f);
        transform.position = new Vector3(0f, 0f, 0f);
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        Debug.Log("Skill 3: 패턴 종료");
    }
    // BC2Test 클래스 상단에 health 참조가 이미 있으므로 이를 활용합니다.
    // public BossHealth2 health; // 기존에 선언되어 있음

    [Header("Special Type 4 설정")]
    public float flySpeed = 6f; // 비행 속도 약간 상향
    public float rotationSpeed = 3f;
    public Vector2 xzRange = new Vector2(-15f, 15f);
    public Vector2 yHeightRange = new Vector2(5f, 9f);
    private int currentBurstCount = 0;
    private bool isGroggy = false;

    IEnumerator SpecialType4()
    {
        Debug.Log("Skill 4: 풍선 비행 패턴 시작 (리셋 로직 포함)");

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
                //b.Init(this);
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
        if (health != null) health.EnterGroggyState(8.0f);

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

        // 6. 그로기 대기 (5초)
        yield return new WaitForSeconds(5.0f);

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

    private Vector3 GetNewRandomAirPoint()
    {
        Vector3 center = (mapCenter != null) ? mapCenter.position : Vector3.zero;
        float rx = Random.Range(xzRange.x, xzRange.y);
        float rz = Random.Range(xzRange.x, xzRange.y);
        float ry = Random.Range(yHeightRange.x, yHeightRange.y);
        return center + new Vector3(rx, ry, rz);
    }

    // 이동 헬퍼
    IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        float time = 0;
        Vector3 start = transform.position;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(start, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }
}