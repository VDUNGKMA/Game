using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Chỉ chấp nhận đĩa món ăn

                // Nếu ở chế độ nhiều người chơi, gửi ID người chơi
                if (KitchenGameMultiplayer.playMultiplayer)
                {
                    ulong playerId = player.GetNetworkObject().OwnerClientId;
                    DeliveryManager.Instance.DeliverRecipe(playerId, plateKitchenObject);
                }
                else
                {
                    // Chế độ một người chơi
                    DeliveryManager.Instance.DeliverRecipe(0, plateKitchenObject);
                }

                // Hủy đối tượng món ăn sau khi giao
                KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
            }
        }
    }
}
