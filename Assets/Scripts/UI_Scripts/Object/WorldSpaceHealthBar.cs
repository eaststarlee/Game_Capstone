using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    public Image fillImage;
    private Transform cam;

    public Color activeColor = Color.green;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam != null)
            transform.LookAt(transform.position + cam.forward);
    }

    public void UpdateHealthBar(float currentHP, float maxHP)
    {
        fillImage.fillAmount = currentHP / maxHP;
    }

    // 기본 색상 설정
    public void SetStatusColor(bool isActive)
    {
        if (fillImage != null)
        {
            fillImage.color = isActive ? activeColor : inactiveColor;
        }
    }

    // 깜빡임 효과 (유예 시간 동안 실행)
    public void FlashUpdate()
    {
        if (fillImage != null)
        {
            // 초당 약 5번 깜빡임
            float lerp = Mathf.PingPong(Time.time * 10f, 1f);
            fillImage.color = Color.Lerp(inactiveColor, activeColor, lerp);
        }
    }
}