using UnityEngine;

public class AimUIController : MonoBehaviour
{
    [Header("Aim UI Image 오브젝트")]
    public GameObject aimUIImage; // 인스펙터에 Aim UI 넣기

    private void Update()
    {
        if (aimUIImage != null)
        {
            // 마우스 우클릭을 누르고 있는 동안만 활성화
            aimUIImage.SetActive(Input.GetMouseButton(1));
        }
    }
}
