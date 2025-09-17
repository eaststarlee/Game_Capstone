using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Controls")]
    public KeyCode unarmedKey = KeyCode.Alpha1;   // 맨손
    public KeyCode rifleKey = KeyCode.Alpha2;     // 라이플

    [Header("Weapon & Layers")]
    public string rifleLayerName = "Rifle Layer"; // 라이플 애니메이터 레이어 이름
    public float layerChangeSpeed = 10f;          // 레이어 전환 속도

    [Header("Animator param names")]
    public string paramIsFiring = "isFiring";         // 사격 트리거 이름

    // --- Private 변수 ---
    private int rifleLayerIndex;
    private float rifleLayerTargetWeight = 0f;    // 목표 레이어 가중치
    private float rifleLayerCurrentWeight = 0f;   // 현재 레이어 가중치

    void Awake()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            // 라이플 레이어 인덱스를 가져오기
            rifleLayerIndex = animator.GetLayerIndex(rifleLayerName);
        }
    }

    void Update()
    {
        ReadInput();             // 기존 입력 처리
        HandleWeaponSwitch();    // 무기 전환
        HandleShooting();        // 사격 처리
    }

    void ReadInput()
    {
        // 기존 입력 처리 함수 내용
        // 예: 이동, 점프 등
    }

    /// <summary>
    /// 1,2번 키 입력에 따른 무기 전환 처리
    /// </summary>
    void HandleWeaponSwitch()
    {
        // 1번 키: 맨손
        if (Input.GetKeyDown(unarmedKey))
            rifleLayerTargetWeight = 0f;

        // 2번 키: 라이플
        if (Input.GetKeyDown(rifleKey))
            rifleLayerTargetWeight = 1f;

        // 현재 Weight에서 목표 Weight까지 부드럽게 전환
        rifleLayerCurrentWeight = Mathf.Lerp(rifleLayerCurrentWeight, rifleLayerTargetWeight, Time.deltaTime * layerChangeSpeed);

        // 애니메이터 레이어에 적용
        animator.SetLayerWeight(rifleLayerIndex, rifleLayerCurrentWeight);
    }

    /// <summary>
    /// 마우스 클릭 시 사격 처리
    /// </summary>
    void HandleShooting()
    {
        // 마우스 왼쪽 버튼을 누르고 있고, 라이플을 장착했을 때
        if (Input.GetMouseButton(0) && rifleLayerCurrentWeight > 0.9f)
        {
            // isFiring 파라미터를 true로 설정
            animator.SetBool(paramIsFiring, true);
        }
        else
        {
            // 마우스 버튼을 떼거나, 맨손 상태일 때는 isFiring 파라미터를 false로 설정
            animator.SetBool(paramIsFiring, false);
        }
    }
}
