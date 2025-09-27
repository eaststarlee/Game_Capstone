using UnityEngine;
using UnityEngine.UI;

public class AimUIController : MonoBehaviour
{
    [Header("Aim UI Image 오브젝트")]
    public GameObject aimUIImage; // 인스펙터에 Aim UI 넣기

    [Header("토글할 키")]
    public KeyCode toggleKey = KeyCode.Tab; // 원하는 키로 변경 가능

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (aimUIImage != null)
            {
                bool isActive = aimUIImage.activeSelf;
                aimUIImage.SetActive(!isActive);
            }
        }
    }
}
