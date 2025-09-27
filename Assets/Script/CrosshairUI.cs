using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] float fadeSpeed = 18f;
    [SerializeField] float aimSize = 16f;
    [SerializeField] float hipSize = 28f;

    float targetAlpha = 0f;
    RectTransform rt;
    Color col;

    void Awake()
    {
        if (!img) img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        col = img.color;
        col.a = 0f;
        img.color = col;
        rt.sizeDelta = new Vector2(aimSize, aimSize);
    }

    public void SetAiming(bool aiming)
    {
        // 조준 중엔 보이고(작게), 평소엔 커지거나 숨기기
        targetAlpha = aiming ? 1f : 0.0f;           // 평소에 항상 보이고 싶으면 0.5f 정도
        float size = aiming ? aimSize : hipSize;
        rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, new Vector2(size, size), 0.8f);
    }

    void Update()
    {
        col.a = Mathf.Lerp(col.a, targetAlpha, Time.deltaTime * fadeSpeed);
        img.color = col;
    }
}
