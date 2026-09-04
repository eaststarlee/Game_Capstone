// 스카이박스의 큐브맵 전환을 주기적으로 제어하는 스크립트
using UnityEngine;

public class SkyController : MonoBehaviour
{
    [Tooltip("전환 주기(초)")]
    public float cycleDuration = 10.0f;

    private string transitionPropertyName = "_CubemapTransition";

    private Material skyboxInstance;

    void Start()
    {
        if (RenderSettings.skybox == null)
        {
            Debug.LogError("RenderSettings.skybox가 설정되어 있지 않습니다. Skybox Material을 확인하세요.");
            return;
        }

        skyboxInstance = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skyboxInstance;
    }

    void Update()
    {
        if (skyboxInstance == null)
        {
            return;
        }

        float cosValue = Mathf.Cos(Time.time * 2.0f * Mathf.PI / cycleDuration);
        float transitionValue = (-cosValue + 1.0f) / 2.0f;
        skyboxInstance.SetFloat(transitionPropertyName, transitionValue);
    }
}