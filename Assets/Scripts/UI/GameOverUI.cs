// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.UI;

// public class GameOverUI : MonoBehaviour
// {


//     [SerializeField] private TextMeshProUGUI recipesDeliveredText;
//     [SerializeField] private TextMeshProUGUI highScoreText;
//     [SerializeField] private Transform leaderboardPanel;
//     [SerializeField] private GameObject leaderboardEntryPrefab; // Prefab cho từng dòng bảng xếp hạng
//     [SerializeField] private Button playAgainButton;
//     public GameObject joystick;
//     public GameObject gamePauseUIx;

//     private void Awake()
//     {
//         playAgainButton.onClick.AddListener(() =>
//         {
//             NetworkManager.Singleton.Shutdown();
//             Loader.Load(Loader.Scene.MainMenuScene);
//         });
//     }

//     private void Start()
//     {
//         KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

//         Hide();
//     }

//     // private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
//     // {
//     //     if (KitchenGameManager.Instance.IsGameOver())
//     //     {
//     //         Show();

//     //         recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
//     //     }
//     //     else
//     //     {
//     //         Hide();
//     //     }
//     // }
//     // private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
//     // {
//     //     if (KitchenGameManager.Instance == null || DeliveryManager.Instance == null)
//     //     {
//     //         Debug.LogError("KitchenGameManager or DeliveryManager is null.");
//     //         return;
//     //     }

//     //     if (KitchenGameManager.Instance.IsGameOver())
//     //     {
//     //         Show();

//     //         int currentScore = DeliveryManager.Instance.GetSuccessfulRecipesAmount();

//     //         if (recipesDeliveredText != null)
//     //         {
//     //             recipesDeliveredText.text = "Số món ăn hoàn thành: " + currentScore;
//     //         }

//     //         /// Chỉ hiển thị kỷ lục trong chế độ chơi đơn
//     //         if (!KitchenGameMultiplayer.playMultiplayer)
//     //         {
//     //             int highScore = PlayerPrefs.GetInt("HighScore", 0);
//     //             highScoreText.text = "Kỷ lục: " + highScore;
//     //         }
//     //         else
//     //         {
//     //             highScoreText.text = ""; // Ẩn kỷ lục khi chơi nhiều người
//     //         }
//     //     }
//     //     else
//     //     {
//     //         Hide();
//     //     }
//     // }
//     private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
//     {
//         if (KitchenGameManager.Instance == null || DeliveryManager.Instance == null)
//         {
//             Debug.LogError("KitchenGameManager hoặc DeliveryManager là null.");
//             return;
//         }

//         if (KitchenGameManager.Instance.IsGameOver())
//         {
//             Show();
//             UpdateUI();
//         }
//         else
//         {
//             Hide();
//         }
//     }

//     private void UpdateUI()
//     {

//         if (KitchenGameMultiplayer.playMultiplayer)
//         {
//             // Hiển thị bảng xếp hạng cho chế độ nhiều người chơi
//             UpdateLeaderboard();
//         }
//         else
//         {
//             // Hiển thị kỷ lục cho chế độ 1 người chơi
//             int successfulRecipes = DeliveryManager.Instance.GetSuccessfulRecipesAmount();
//             int highScore = PlayerPrefs.GetInt("HighScore", 0);
//             recipesDeliveredText.text = "Recipes Delivered: " + successfulRecipes;
//             highScoreText.text = "High Score: " + highScore;
//         }
//     }

//     // private void UpdateLeaderboard()
//     // {
//     //     // Xóa các mục hiện có trong bảng xếp hạng
//     //     foreach (Transform child in leaderboardPanel)
//     //     {
//     //         Destroy(child.gameObject);
//     //     }

//     //     // Lấy điểm số của các người chơi từ DeliveryManager
//     //     Dictionary<ulong, int> playerScores = DeliveryManager.Instance.GetPlayerScores();

//     //     // Sắp xếp điểm số từ cao đến thấp
//     //     List<KeyValuePair<ulong, int>> sortedScores = new List<KeyValuePair<ulong, int>>(playerScores);
//     //     sortedScores.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

//     //     // Tạo mục bảng xếp hạng cho từng người chơi
//     //     foreach (KeyValuePair<ulong, int> scoreEntry in sortedScores)
//     //     {
//     //         GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardPanel);
//     //         TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
//     //         entryText.text = $"Player {scoreEntry.Key}: {scoreEntry.Value} Recipes";
//     //     }
//     // }
//     private void UpdateLeaderboard()
//     {
//         // Lấy điểm số của các người chơi từ DeliveryManager
//         Dictionary<ulong, int> playerScores = DeliveryManager.Instance.GetPlayerScores();

//         if ( playerScores.Count == 0)
//         {
//             Debug.Log("Không có điểm số nào được ghi nhận!");
//             // return;
//         }
//         // In danh sách điểm số để kiểm tra
//         foreach (var score in playerScores)
//         {

//             Debug.Log($"Player {score.Key}: {score.Value} Recipes");
//         }
//         // Xóa các mục hiện có trong bảng xếp hạng
//         foreach (Transform child in leaderboardPanel)
//         {
//             Destroy(child.gameObject);
//         }

//         // Sắp xếp điểm số từ cao đến thấp
//         List<KeyValuePair<ulong, int>> sortedScores = new List<KeyValuePair<ulong, int>>(playerScores);
//         sortedScores.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

//         // Tạo mục bảng xếp hạng cho từng người chơi
//         foreach (KeyValuePair<ulong, int> scoreEntry in sortedScores)
//         {
//             GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardPanel);
//             TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
//             if (entryText != null)
//             {
//                 entryText.text = $"Player {scoreEntry.Key}: {scoreEntry.Value} Recipes";
//                 entryText.fontSize = 32; // Tăng kích thước font chữ
//                                          // Nếu là người chơi có điểm cao nhất, thay đổi màu để làm nổi bật
//                 if (scoreEntry.Equals(sortedScores[0]))
//                 {
//                     entryText.color = Color.yellow; // Màu vàng cho người chơi dẫn đầu
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Không tìm thấy TextMeshProUGUI trong leaderboardEntryPrefab!");
//             }
//             // Căn giữa các mục trong leaderboardPanel
//             entry.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
//             entry.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
//         }
//     }

//     private void Show()
//     {
//         gameObject.SetActive(true);
//         joystick.SetActive(false);
//         gamePauseUIx.SetActive(false);
//         playAgainButton.Select();

//     }

//     private void Hide()
//     {
//         gameObject.SetActive(false);
//         joystick.SetActive(true);
//         gamePauseUIx.SetActive(true);

//     }


// }

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
