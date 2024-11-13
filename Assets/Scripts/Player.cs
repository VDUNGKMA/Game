/*Lớp Player kế thừa từ NetworkBehaviour và IKitchenObjectParent, đại diện cho người chơi trong trò chơi nhiều người chơi. Nó xử lý:

Di chuyển nhân vật bằng joystick.
Tương tác với các đối tượng bếp (BaseCounter), như cắt, thái, hoặc nấu đồ ăn.
Đồng bộ hóa các hành động của người chơi trong môi trường mạng.*/
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class Player : NetworkBehaviour, IKitchenObjectParent {
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;
    public Joystick joystick;
    public GameObject btnE;
    public GameObject btnF;
    public Button buttonEComponent;
    public Button buttonFComponent;

    public static void ResetStaticData() {
        OnAnyPlayerSpawned = null;
    }


    public static Player LocalInstance { get; private set; }



    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }


    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;


    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;


    private void Start() {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
        joystick = GameObject.Find("Fixed Joystick").gameObject.GetComponent<Joystick>();
        btnE = GameObject.Find("E").gameObject;
        btnF = GameObject.Find("F").gameObject;
        buttonEComponent = btnE.GetComponent<Button>();
        buttonEComponent.onClick.AddListener(OnBtnEClicked);

        buttonFComponent = btnF.GetComponent<Button>();
        buttonFComponent.onClick.AddListener(OnBtnFClicked);
        Debug.Log(joystick != null ? "Tồn tại" : "Ko có");
    }
    private void OnBtnEClicked()
    {
        Debug.Log("Nút đc E nhấn");


        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }
    private void OnBtnFClicked()
    {
        Debug.Log("Nút đc F nhấn");


        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }
    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer) {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (clientId == OwnerClientId && HasKitchenObject()) {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        Debug.Log("Nút đc F nhấn click");
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    private void Update() {
        if (!IsOwner) {
            return;
        }

        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking() {
        return isWalking;
    }
    //Kiểm tra các đối tượng bếp gần nhất mà người chơi có thể tương tác.
    private void HandleInteractions() {
        // Lấy input từ joystick để tạo ra hướng di chuyển
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        inputVector = new Vector2(joystick.Horizontal, joystick.Vertical);
        // Tạo vector di chuyển dựa trên hướng input
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        // Nếu người chơi đang di chuyển, cập nhật hướng tương tác cuối cùng
        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }
        // Khoảng cách tối đa để tương tác
        float interactDistance = 2f;
         // Sử dụng raycast để kiểm tra nếu có một đối tượng bếp (quầy) ở phía trước
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            // Nếu raycast trúng một đối tượng bếp (BaseCounter)
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                // Nếu đối tượng bếp này khác với đối tượng bếp hiện tại
                if (baseCounter != selectedCounter) {
                    // Cập nhật đối tượng bếp được chọn
                    SetSelectedCounter(baseCounter);
                }
            } else {
                SetSelectedCounter(null);

            }
        } else {
            SetSelectedCounter(null);
        }
    }
/*Xử lý việc di chuyển nhân vật người chơi dựa trên đầu vào từ joystick. 
Nó kiểm tra xem có vật cản nào trên đường đi không và điều chỉnh hướng di chuyển 
nếu cần thiết. Phương thức này cũng cập nhật trạng thái (đang đi bộ hoặc đứng yên)
 và hướng quay của nhân vật.*/
    private void HandleMovement() {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        // Lấy giá trị từ joystick để biết hướng di chuyển (theo trục X và Z).
        inputVector = new Vector2(joystick.Horizontal, joystick.Vertical);
        // Kiểm tra nếu joystick không có đầu vào thì giữ hướng di chuyển hiện tại
        if (inputVector.magnitude < 0.1f)
        {
            // Nếu joystick không có đầu vào, thoát khỏi hàm mà không cập nhật hướng di chuyển
            isWalking = false;
            return;
        }
        //Tạo hướng di chuyển
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        //Tính toán khoảng cách di chuyển
        float moveDistance = moveSpeed * Time.deltaTime;
        // bán kính nhân vật
        float playerRadius = .6f;
        //kiểm tra xem có vật thể nào nằm trên đường di chuyển của nhân vật hay không
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionsLayerMask);

        if (!canMove) {
           
            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove) {
                // di chuyển tên truc X
                moveDir = moveDirX;
            } else {
              

                // di chuyển trên trục Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);

                if (canMove) {
                    // Can move only on the Z
                    moveDir = moveDirZ;
                } else {
                    // Cannot move in any direction
                }
            }
        }
        // cập nhập lại vị trí
        if (canMove) {
            transform.position += moveDir * moveDistance;
        }
        //kiểm tra xem nhân vật có đang di chuyển hay không.
        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        // cập nhập hướng xoay đầu nhân vật theo hướng di chuyển
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }


    public NetworkObject GetNetworkObject() {
        return NetworkObject;
    }

}
