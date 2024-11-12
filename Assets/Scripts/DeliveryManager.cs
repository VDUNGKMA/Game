using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{


    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;


    public static DeliveryManager Instance { get; private set; }


    [SerializeField] private RecipeListSO recipeListSO;


    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;

    // Thêm Dictionary để lưu số lượng món ăn hoàn thành của từng người chơi
    // private Dictionary<ulong, int> playerScores;
    private Dictionary<ulong, PlayerScore> playerScores;
    
    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
        // playerScores = new Dictionary<ulong, int>(); // Khởi tạo bảng xếp hạng
        playerScores = new Dictionary<ulong, PlayerScore>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    // public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    // {
    //     for (int i = 0; i < waitingRecipeSOList.Count; i++)
    //     {
    //         RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

    //         if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
    //         {
    //             // Has the same number of ingredients
    //             bool plateContentsMatchesRecipe = true;
    //             foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
    //             {
    //                 // Cycling through all ingredients in the Recipe
    //                 bool ingredientFound = false;
    //                 foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
    //                 {
    //                     // Cycling through all ingredients in the Plate
    //                     if (plateKitchenObjectSO == recipeKitchenObjectSO)
    //                     {
    //                         // Ingredient matches!
    //                         ingredientFound = true;
    //                         break;
    //                     }
    //                 }
    //                 if (!ingredientFound)
    //                 {
    //                     // This Recipe ingredient was not found on the Plate
    //                     plateContentsMatchesRecipe = false;
    //                 }
    //             }

    //             if (plateContentsMatchesRecipe)
    //             {
    //                 // Player delivered the correct recipe!
    //                 DeliverCorrectRecipeServerRpc(i);
    //                 return;
    //             }
    //         }
    //     }

    //     // No matches found!
    //     // Player did not deliver a correct recipe
    //     DeliverIncorrectRecipeServerRpc();
    // }
    public void DeliverRecipe(ulong playerId, PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(playerId, i);
                    return;
                }
            }
        }

        DeliverIncorrectRecipeServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }

   
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    // [ServerRpc(RequireOwnership = false)]
    // private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    // {
    //     DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    // }
    [ServerRpc(RequireOwnership = false)]
    // private void DeliverCorrectRecipeServerRpc(ulong playerId, int waitingRecipeSOListIndex)
    // {
    //     // Đảm bảo rằng ID người chơi hợp lệ
    //     if (!playerScores.ContainsKey(playerId))
    //     {
    //         playerScores[playerId] = 0;
    //     }

    //     // Tăng điểm số của người chơi trên server
    //     playerScores[playerId]++;

    //     // Xóa món ăn từ danh sách
    //     waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

    //     // Gửi cập nhật đến tất cả client
    //     DeliverCorrectRecipeClientRpc(playerId);
    // }
    private void DeliverCorrectRecipeServerRpc(ulong playerId, int waitingRecipeSOListIndex)
    {
        if (!playerScores.ContainsKey(playerId))
        {
            // Sử dụng phương thức ToString() để chuyển đổi playerName sang string
            string playerName = KitchenGameMultiplayer.Instance.GetPlayerNameFromClientId(playerId);
            playerScores[playerId] = new PlayerScore(playerId,playerName, 0);
        }
        // Lấy đối tượng PlayerScore ra, cập nhật và gán lại vào từ điển
        PlayerScore playerScore = playerScores[playerId];
        playerScore.score++;  // Tăng điểm số
        playerScores[playerId] = playerScore;  // Gán lại vào từ điển

        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
        successfulRecipesAmount++; // Tăng số lượng món ăn hoàn thành

        // In ra console để kiểm tra
        Debug.Log($"Successful Recipes: {successfulRecipesAmount}");
        DeliverCorrectRecipeClientRpc(playerId);
        // Gửi dữ liệu điểm số cập nhật tới các máy khách
        UpdateLeaderboardClientRpc(new List<PlayerScore>(playerScores.Values).ToArray());

    }
    [ClientRpc]
    private void UpdateLeaderboardClientRpc(PlayerScore[] updatedScores)
    {
        // Xử lý cập nhật điểm số trên máy khách
        playerScores.Clear();
        foreach (var score in updatedScores)
        {
            playerScores[score.playerId] = score;
        }

        // Cập nhật UI nếu cần thiết
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
    }
    // [ClientRpc]

    // private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    // {
    //     successfulRecipesAmount++;

    //     waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

    //     // Kiểm tra và lưu kỷ lục mới
    //     int highScore = PlayerPrefs.GetInt("HighScore", 0);
    //     if (successfulRecipesAmount > highScore)
    //     {
    //         PlayerPrefs.SetInt("HighScore", successfulRecipesAmount);
    //         PlayerPrefs.Save();
    //         Debug.Log("Kỷ lục mới đã được lưu: " + successfulRecipesAmount);

    //     }

    //     OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
    //     OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    // }
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(ulong playerId)
    {
        
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    private void SaveHighScore()
    {
        if (!KitchenGameMultiplayer.playMultiplayer)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (successfulRecipesAmount > highScore)
            {
                PlayerPrefs.SetInt("HighScore", successfulRecipesAmount);
                PlayerPrefs.Save();
                Debug.Log("Kỷ lục mới đã được lưu: " + successfulRecipesAmount);
            }
        }
    }
    // Thêm phương thức để lấy bảng xếp hạng
    // public Dictionary<ulong, int> GetPlayerScores()
    // {
    //     return playerScores;
    // }
    // Cập nhật phương thức để đảm bảo tất cả người chơi được thêm vào
    // public Dictionary<ulong, int> GetPlayerScores()
    // {
    //     // Lấy danh sách tất cả ID của người chơi từ KitchenGameManager
    //     List<ulong> allPlayerIds = KitchenGameManager.Instance.GetAllPlayerIds();

    //     // Đảm bảo tất cả người chơi đều có trong bảng xếp hạng
    //     foreach (var playerId in allPlayerIds)
    //     {
    //         if (!playerScores.ContainsKey(playerId))
    //         {
    //             playerScores[playerId] = 0; // Đặt điểm là 0 nếu chưa có trong bảng xếp hạng
    //         }
    //     }

    //     return playerScores;
    // }
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        Debug.Log($"GetSuccessfulRecipesAmount called. Current value: {successfulRecipesAmount}");
        return successfulRecipesAmount;
    }
    // public List<PlayerScore> GetPlayerScores()
    // {
    //     List<PlayerScore> scoresList = new List<PlayerScore>();
    //     foreach (var score in playerScores.Values)
    //     {
    //         scoresList.Add(score);
    //     }
    //     return scoresList;
    // }
    public List<PlayerScore> GetPlayerScores()
    {
        return new List<PlayerScore>(playerScores.Values);
    }

}
