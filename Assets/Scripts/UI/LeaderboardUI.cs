using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform leaderboardPanel;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Button playAgainButton; // Thêm nút Play Again
    private void Start()
    {
        if (!KitchenGameMultiplayer.playMultiplayer)
        {
            gameObject.SetActive(false);
        }
        else
        {
            KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
            Hide();
            // Gán sự kiện cho nút Play Again
            playAgainButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown();
                Loader.Load(Loader.Scene.MainMenuScene);
            });
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
            UpdateLeaderboard();
        }
        else
        {
            Hide();
        }
    }

    // private void UpdateLeaderboard()
    // {
    //     // Xóa các mục hiện có trong bảng xếp hạng
    //     foreach (Transform child in leaderboardPanel)
    //     {
    //         Destroy(child.gameObject);
    //     }

    //     // Lấy danh sách PlayerScore từ DeliveryManager
    //     List<PlayerScore> playerScores = DeliveryManager.Instance.GetPlayerScores();

    //     // Sắp xếp điểm số từ cao đến thấp
    //     playerScores.Sort((score1, score2) => score2.score.CompareTo(score1.score));

    //     // Tạo mục bảng xếp hạng cho từng người chơi
    //     foreach (PlayerScore scoreEntry in playerScores)
    //     {
    //         GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardPanel);
    //         RectTransform entryRect = entry.GetComponent<RectTransform>();

    //         // Đảm bảo entry được căn giữa
    //         entryRect.anchorMin = new Vector2(0.5f, 0.5f);
    //         entryRect.anchorMax = new Vector2(0.5f, 0.5f);
    //         entryRect.pivot = new Vector2(0.5f, 0.5f);
    //         entryRect.anchoredPosition = Vector2.zero;

    //         // Đặt văn bản cho mục bảng xếp hạng
    //         TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
    //         if (entryText != null)
    //         {
    //             // Hiển thị cả tên người chơi và số điểm
    //             entryText.text = $"Player:  {scoreEntry.playerName}: {scoreEntry.score} Recipes";
    //             entryText.alignment = TextAlignmentOptions.Center; // Căn giữa văn bản
    //             entryText.fontSize = 32;

    //             // Làm nổi bật người chơi đứng đầu
    //             if (scoreEntry.Equals(playerScores[0]))
    //             {
    //                 entryText.color = Color.yellow;
    //             }
    //         }
    //     }
    // }
    private void UpdateLeaderboard()
    {
        // Xóa các mục hiện có trong bảng xếp hạng
        foreach (Transform child in leaderboardPanel)
        {
            Destroy(child.gameObject);
        }

        // Lấy danh sách PlayerScore từ DeliveryManager
        List<PlayerScore> playerScores = DeliveryManager.Instance.GetPlayerScores();

        // Lấy tất cả ID người chơi từ KitchenGameMultiplayer
        List<ulong> allPlayerIds = KitchenGameMultiplayer.Instance.GetAllPlayerIds();

        // Đảm bảo rằng tất cả người chơi đều có mục trong playerScores
        foreach (ulong playerId in allPlayerIds)
        {
            // Kiểm tra xem người chơi đã có trong playerScores chưa
            if (!playerScores.Exists(score => score.playerId == playerId))
            {
                // Nếu chưa có, thêm người chơi với điểm 0
                string playerName = KitchenGameMultiplayer.Instance.GetPlayerNameFromClientId(playerId);
                playerScores.Add(new PlayerScore(playerId, playerName, 0));
            }
        }

        // Sắp xếp điểm số từ cao đến thấp
        playerScores.Sort((score1, score2) => score2.score.CompareTo(score1.score));

        // In ra thông tin của từng người chơi trong console
        foreach (PlayerScore scoreEntry in playerScores)
        {
            Debug.Log($"Player: {scoreEntry.playerName}, Score: {scoreEntry.score}");
        }

        // Tạo mục bảng xếp hạng cho từng người chơi
        foreach (PlayerScore scoreEntry in playerScores)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardPanel);
            RectTransform entryRect = entry.GetComponent<RectTransform>();

            // Đảm bảo entry được căn giữa
            entryRect.anchorMin = new Vector2(0.5f, 0.5f);
            entryRect.anchorMax = new Vector2(0.5f, 0.5f);
            entryRect.pivot = new Vector2(0.5f, 0.5f);
            entryRect.anchoredPosition = Vector2.zero;

            // Đặt văn bản cho mục bảng xếp hạng
            TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (entryText != null)
            {
                // Hiển thị cả tên người chơi và số điểm
                entryText.text = $"{scoreEntry.playerName}: {scoreEntry.score} Recipes";
                entryText.alignment = TextAlignmentOptions.Center; // Căn giữa văn bản
                entryText.fontSize = 32;

                // Làm nổi bật người chơi đứng đầu
                if (scoreEntry.Equals(playerScores[0]))
                {
                    entryText.color = Color.yellow;
                }
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        playAgainButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
