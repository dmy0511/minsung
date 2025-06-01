using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteMessage : MonoBehaviour
{
    [Header("메시지 설정")]
    public GameObject messageCanvas;
    public GameObject noteMessage;
    public float messageDisplayTime = 3.0f;

    private bool isShowingMessage = false;
    private Door_Stage3 doorStage3;

    void Start()
    {
        if (messageCanvas != null)
        {
            messageCanvas.SetActive(false);
        }

        if (noteMessage != null)
        {
            noteMessage.SetActive(false);
        }

        doorStage3 = FindObjectOfType<Door_Stage3>();
    }

    void OnMouseDown()
    {
        ShowNoteMessage();
    }

    public void OnNoteClicked()
    {
        ShowNoteMessage();
    }

    void ShowNoteMessage()
    {
        if (isShowingMessage) return;

        if (doorStage3 != null && doorStage3.IsCardsIncompleteMessageActive())
        {
            doorStage3.HideCardsIncompleteMessage();
        }

        if (messageCanvas != null && noteMessage != null)
        {
            messageCanvas.SetActive(true);
            noteMessage.SetActive(true);

            if (doorStage3 != null)
            {
                doorStage3.SetNoteMessageActive(true);
            }

            StartCoroutine(HideMessageAfterDelay());
        }
    }

    IEnumerator HideMessageAfterDelay()
    {
        isShowingMessage = true;

        yield return new WaitForSeconds(messageDisplayTime);

        HideNoteMessage();
    }

    public void HideNoteMessage()
    {
        if (messageCanvas != null)
        {
            messageCanvas.SetActive(false);
        }

        if (noteMessage != null)
        {
            noteMessage.SetActive(false);
        }

        if (doorStage3 != null)
        {
            doorStage3.SetNoteMessageActive(false);
        }

        isShowingMessage = false;
    }

    public bool IsNoteMessageActive()
    {
        return noteMessage != null && noteMessage.activeInHierarchy;
    }
}
