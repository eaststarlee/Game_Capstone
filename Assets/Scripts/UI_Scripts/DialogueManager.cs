using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class Dialogue
{
    public string speaker;
    public string[] lines;
}

[System.Serializable]
public class DialogueList
{
    public Dialogue[] dialogues;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text dialogueText;      // Canvas Text 오브젝트
    public GameObject dialogueUI;  // 대화창 전체 오브젝트

    [Header("Optional: Objects to Disable When Dialogue Ends")]
    public List<GameObject> objectsToDisable;

    private DialogueList dialogueList;

    private int currentDialogue = 0;
    private int currentLine = 0;

    private bool isDialogueActive = false;  // 대화창 활성 상태

    public PlayerController playerController;

    private bool playerInTrigger = false;   // ← 트리거 안에 있는지 체크

    void Awake()
    {
        LoadDialogueJSON();
    }

    void Update()
    {
        // 플레이어가 트리거 안에 있어야만 F 키 반응
        if (!playerInTrigger) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else
            {
                NextLine();
            }
        }
    }

    void LoadDialogueJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "dialogues.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            dialogueList = JsonUtility.FromJson<DialogueList>(json);
        }
        else
        {
            Debug.LogError("dialogues.json 파일이 없습니다! 경로: " + path);
        }
    }

    void ShowLine()
    {
        if (dialogueList == null || dialogueList.dialogues.Length == 0) return;
        dialogueText.text = dialogueList.dialogues[currentDialogue].lines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= dialogueList.dialogues[currentDialogue].lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialogueUI.SetActive(true);

        currentDialogue = 0;
        currentLine = 0;

        if (playerController != null)
        {
            var cc = playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;  // 이동 비활성화
        }

        ShowLine();
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.SetActive(false);

        if (playerController != null)
        {
            var cc = playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;  // 이동 다시 허용
        }
    }

    // --- 여기가 트리거 핵심 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            // 트리거 벗어나면 대화 강제 종료
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }
}
