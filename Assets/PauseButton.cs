// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PauseButton : MonoBehaviour
// {
//     // Start is called before the first frame update

//     public static bool isPause = false;
//     public event EventHandler OnMultiplayerGamePaused;
//     public event EventHandler OnMultiplayerGameUnpaused;
//     public GameObject gamePauseUI;
//     public GameObject joystick;
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
//     public void OnPauseAc()
//     {
//         if (!isPause)
//         {
//             Time.timeScale = 1f;
//             isPause = true;
//             Hide();

//         }
//         else
//         {
//             Time.timeScale = 0f;
//             isPause = false;
//             Show();
//         }
//     }
//     private void Show()
//     {
//         gamePauseUI.SetActive(true);
//         joystick.SetActive(false);
//         Debug.Log("Hien");
//     }
//     private void Hide()
//     {
//         gamePauseUI.SetActive(false);
//         joystick.SetActive(true);
//         Debug.Log("An");
//     }
// }

using System;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    public GameObject gamePauseUI;
    public GameObject joystick;

    private void Start()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnMultiplayerGamePaused += KitchenGameManager_OnMultiplayerGamePaused;
            KitchenGameManager.Instance.OnMultiplayerGameUnpaused += KitchenGameManager_OnMultiplayerGameUnpaused;
        }
    }

    public void OnPauseButtonClicked()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.TogglePauseGame();
        }
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
        gamePauseUI.SetActive(true);
        joystick.SetActive(false);
        Debug.Log("Paused - Showing UI");
    }

    private void Hide()
    {
        gamePauseUI.SetActive(false);
        joystick.SetActive(true);
        Debug.Log("Unpaused - Hiding UI");
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
