using UnityEngine;
using System.Collections;

public class ButtonEffect2 : MonoBehaviour
{
    [Header("비활성화할 버튼 오브젝트")]
    public GameObject buttonObject;

    [Header("Animator")]
    public Animator targetAnimator;

    [Header("F 키 누를 때 재생할 애니메이션")]
    public string animationOnPress = "Press";

    [Header("F 키 뗄 때 재생할 애니메이션")]
    public string animationOnRelease = "Release";

    [Header("Release 애니메이션 길이 (초)")]
    public float releaseAnimationDuration = 0.5f;

    private bool isFPressed = false;

    void Start()
    {
        if (buttonObject == null)
            buttonObject = gameObject;
    }

    void Update()
    {
        // ---------- F 키 눌렀을 때 ----------
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFPressed = true;

            if (targetAnimator != null)
                targetAnimator.Play(animationOnPress);
        }

        // ---------- F 키 떼었을 때 ----------
        if (Input.GetKeyUp(KeyCode.F) && isFPressed)
        {
            if (targetAnimator != null)
                targetAnimator.Play(animationOnRelease);

            StartCoroutine(DeactivateAfterDelay());

            isFPressed = false;
        }
    }

    IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(releaseAnimationDuration);

        if (buttonObject != null)
            buttonObject.SetActive(false);
    }
}
