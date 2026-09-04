using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCheater : MonoBehaviour
{
    [Header("테스트할 씬 이름")]
    public string scene1Name = "Scene1";
    public string scene2Name = "Scene2";

    void Update()
    {
        // 숫자 1키를 누르면 Scene1으로 이동
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log($"{scene1Name}으로 전환합니다.");
            SceneManager.LoadScene(scene1Name);
        }

        // 숫자 2키를 누르면 Scene2으로 이동
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Debug.Log($"{scene2Name}으로 전환합니다.");
            SceneManager.LoadScene(scene2Name);
        }
    }
}