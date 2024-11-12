// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PauseMultiplayerUI : MonoBehaviour {



//     private void Start() {
//         KitchenGameManager.Instance.OnMultiplayerGamePaused += KitchenGameManager_OnMultiplayerGamePaused;
//         KitchenGameManager.Instance.OnMultiplayerGameUnpaused += KitchenGameManager_OnMultiplayerGameUnpaused;

//         Hide();
//     }

//     private void KitchenGameManager_OnMultiplayerGameUnpaused(object sender, System.EventArgs e) {
//         Hide();
//     }

//     private void KitchenGameManager_OnMultiplayerGamePaused(object sender, System.EventArgs e) {
//         Show();
//     }

//     private void Show() {
//         gameObject.SetActive(true);
//     }

//     private void Hide() {
//         gameObject.SetActive(false);
//     }
// }
using System;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnMultiplayerGamePaused += KitchenGameManager_OnMultiplayerGamePaused;
            KitchenGameManager.Instance.OnMultiplayerGameUnpaused += KitchenGameManager_OnMultiplayerGameUnpaused;
        }

        Hide();
    }

    private void KitchenGameManager_OnMultiplayerGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void KitchenGameManager_OnMultiplayerGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("Multiplayer Paused - Showing UI");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("Multiplayer Unpaused - Hiding UI");
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnMultiplayerGamePaused -= KitchenGameManager_OnMultiplayerGamePaused;
            KitchenGameManager.Instance.OnMultiplayerGameUnpaused -= KitchenGameManager_OnMultiplayerGameUnpaused;
        }
    }
}
