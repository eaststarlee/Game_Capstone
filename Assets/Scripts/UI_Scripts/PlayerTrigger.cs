using UnityEngine;
using System.Collections;

public class PlayerTrigger : MonoBehaviour
{
    [Header("Targets")]
    public GameObject redTarget;
    public GameObject blueTarget;
    public GameObject yellowTarget;

    [Header("Red/Blue Duration")]
    public float duration = 1f;

    [Header("Player Tag")]
    public string playerTag = "Player"; // Player 오브젝트에 Tag 설정 필요

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.name)
        {
            case "RedInkDecalObject(Clone)":
                if (redTarget != null)
                    StartCoroutine(ActivateTemporary(redTarget, duration));
                break;

            case "BlueInkDecalObject(Clone)":
                if (blueTarget != null)
                    StartCoroutine(ActivateTemporary(blueTarget, duration));
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Yellow는 접촉 중 WallRun 상태일 때만 활성화
        if (other.gameObject.name == "YellowInkDecalObject(Clone)" && yellowTarget != null)
        {
            GameObject playerObj = GameObject.FindWithTag(playerTag);
            if (playerObj != null)
            {
                PlayerController pc = playerObj.GetComponent<PlayerController>();
                if (pc != null)
                {
                    yellowTarget.SetActive(pc.isWallRunning);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "YellowInkDecalObject(Clone)" && yellowTarget != null)
        {
            yellowTarget.SetActive(false);
        }
    }

    private IEnumerator ActivateTemporary(GameObject obj, float duration)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }
}
