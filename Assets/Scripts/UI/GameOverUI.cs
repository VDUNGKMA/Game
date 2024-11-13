

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button playAgainButton;
    public GameObject joystick;
    public GameObject gamePauseUIx;

    private void Awake()
    {
        playAgainButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        // Chỉ hiển thị GameOverUI trong chế độ chơi đơn
        if (KitchenGameMultiplayer.playMultiplayer)
        {
            gameObject.SetActive(false);
        }
        else
        {
            KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
            Hide();
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
            UpdateUI();
        }
        else
        {
            Hide();
        }
    }
    
    private void UpdateUI()
    {
        int successfulRecipes = DeliveryManager.Instance.GetSuccessfulRecipesAmount();
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (successfulRecipes > highScore)
        {
            PlayerPrefs.SetInt("HighScore", successfulRecipes);
            PlayerPrefs.Save();
            Debug.Log("Kỷ lục mới đã được lưu: " + successfulRecipes);
        }
        // In ra console để kiểm tra giá trị
        Debug.Log($"Updating UI. Successful Recipes: {successfulRecipes}, High Score: {PlayerPrefs.GetInt("HighScore", 0)}");
        recipesDeliveredText.text = successfulRecipes + "";
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0); 
    }
    private void Show() {
        gameObject.SetActive(true);
        joystick.SetActive(false);
        gamePauseUIx.SetActive(false);
        playAgainButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        joystick.SetActive(true);
        gamePauseUIx.SetActive(true);
    }
}
