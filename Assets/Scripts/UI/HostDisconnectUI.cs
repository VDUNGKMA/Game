using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour {


    [SerializeField] private Button playAgainButton;


    // private void Awake() {
    //     playAgainButton.onClick.AddListener(() => {
    //         Loader.Load(Loader.Scene.MainMenuScene);
    //     });
    // }
    private void Awake()
    {
        // Đảm bảo playAgainButton không null trước khi thêm sự kiện
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() =>
            {
                Loader.Load(Loader.Scene.MainMenuScene);
            });
        }
    }
    // private void Start() {
    //     NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

    //     Hide();
    // }
    private void Start()
    {
        // Kiểm tra null cho NetworkManager trước khi đăng ký sự kiện
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (clientId == NetworkManager.ServerClientId) {
            // Server is shutting down
            Show();
        }
    }

    // 
    private void Show()
    {
        if (this != null)
        {
            gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (this != null)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        // Gỡ bỏ đăng ký sự kiện khi đối tượng bị hủy
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }

}