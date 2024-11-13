// Lớp này cho phép người chơi tương tác với quầy bếp để cắt nguyên liệu, theo dõi tiến độ cắt, 
//và chuyển đổi nguyên liệu khi đã được cắt xong.
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress {


    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData() {
        OnAnyCut = null;
    }


    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;


    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;


    private int cuttingProgress;

    //Xử lý khi người chơi tương tác với quầy cắt.
    public override void Interact(Player player) {
        //Kiểm tra xem quầy cắt có đang chứa bất kỳ đối tượng nào không.
        if (!HasKitchenObject()) {
           // Kiểm tra xem người chơi có đang cầm một đối tượng hay không.
            if (player.HasKitchenObject()) {
                // nếu đối tượng mà người chơi cầm có thể được cắt
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                   // lấy đối tượng
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    //Đặt đối tượng lên quầy
                    kitchenObject.SetKitchenObjectParent(this);
                    //Gọi phương thức RPC để đồng bộ hóa
                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            } else {
                // Player not carrying anything
            }
        } else {
            // quầy đã có đối tượng
            if (player.HasKitchenObject()) {
                //Người chơi đang cầm một đĩa
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    //Thử thêm nguyên liệu trên quầy vào đĩa
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        //Nếu nguyên liệu được thêm thành công vào đĩa, nguyên liệu trên quầy sẽ bị hủy.
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
            } else {
                // Người chơi không cầm gì thì nhặt nguyên liệu 
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc() {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc() {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });

    }
    //Xử lý khi người chơi thực hiện hành động cắt/thái nguyên liệu.
    public override void InteractAlternate(Player player) {
        //Kiểm tra xem trên quầy cắt có nguyên liệu hay không và đối tg có công thức cắt không
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {

            //Yêu cầu cắt nguyên liệu và đồng bộ trên máy chủ
            CutObjectServerRpc();
            //Kiểm tra tiến trình cắt nguyên liệu để xem  đã hoàn tất chưa 
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc() {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
            // There is a KitchenObject here AND it can be cut
            CutObjectClientRpc();
        }
    }

    [ClientRpc]
    private void CutObjectClientRpc() {
        //Tăng tiến trình cắt lên 1
        cuttingProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
        //Lấy công thức cắt tương ứng với nguyên liệu hiện tại.
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc() {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
            // There is a KitchenObject here AND it can be cut
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }


    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        } else {
            return null;
        }
    }
    // Lấy công thức cắt/thái tương ứng với đối tượng
    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) {
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}