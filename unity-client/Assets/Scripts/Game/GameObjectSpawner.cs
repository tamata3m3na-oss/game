using UnityEngine;

/// <summary>
/// GameObjectSpawner: ضمان ظهور الطائرات والـ UI فوراً في GameScene
/// يتفادى الاعتماد على بيانات السيرفر لإنشاء العناصر الأساسية
/// </summary>
public class GameObjectSpawner : MonoBehaviour
{
    [Header("Ship Prefabs")]
    public GameObject playerShipPrefab;
    public GameObject opponentShipPrefab;
    
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform opponentSpawnPoint;
    
    [Header("Ship References")]
    public ShipController playerShip;
    public ShipController opponentShip;
    
    [Header("UI References")]
    public GameSceneUI gameSceneUI;
    
    private bool isInitialized = false;

    private void Start()
    {
        // تأخير صغير للتأكد من تحميل كل المكونات
        Invoke(nameof(InitializeGameObjects), 0.1f);
    }

    /// <summary>
    /// إنشاء الطائرات والـ UI فوراً
    /// </summary>
    private void InitializeGameObjects()
    {
        if (isInitialized) return;
        
        try
        {
            Debug.Log("[GameObjectSpawner] Initializing game objects...");
            
            // إنشاء الطائرات إذا لم تكن موجودة
            InitializeShips();
            
            // تفعيل الـ UI
            InitializeUI();
            
            isInitialized = true;
            
            Debug.Log("[GameObjectSpawner] Game objects initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameObjectSpawner] Error initializing game objects: {e.Message}");
        }
    }

    /// <summary>
    /// إنشاء الطائرات
    /// </summary>
    private void InitializeShips()
    {
        // إنشاء الطائرة المحلية
        if (playerShip == null && playerShipPrefab != null && playerSpawnPoint != null)
        {
            GameObject playerObj = Instantiate(playerShipPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerShip = playerObj.GetComponent<ShipController>();
            
            if (playerShip != null)
            {
                // بيانات وهمية للاختبار
                playerShip.Initialize(1, true);
                Debug.Log("[GameObjectSpawner] Player ship created");
            }
        }

        // إنشاء طائرة الخصم
        if (opponentShip == null && opponentShipPrefab != null && opponentSpawnPoint != null)
        {
            GameObject opponentObj = Instantiate(opponentShipPrefab, opponentSpawnPoint.position, opponentSpawnPoint.rotation);
            opponentShip = opponentObj.GetComponent<ShipController>();
            
            if (opponentShip != null)
            {
                // بيانات وهمية للاختبار
                opponentShip.Initialize(2, false);
                Debug.Log("[GameObjectSpawner] Opponent ship created");
            }
        }
    }

    /// <summary>
    /// تفعيل الـ UI
    /// </summary>
    private void InitializeUI()
    {
        // البحث عن GameSceneUI وتفعيله
        if (gameSceneUI == null)
        {
            gameSceneUI = FindObjectOfType<GameSceneUI>();
        }
        
        if (gameSceneUI != null)
        {
            // تفعيل الـ UI
            Canvas canvas = gameSceneUI.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                Debug.Log("[GameObjectSpawner] GameSceneUI activated");
            }
        }
        else
        {
            Debug.LogWarning("[GameObjectSpawner] GameSceneUI not found");
        }
    }

    /// <summary>
    /// الحصول على الطائرة المحلية
    /// </summary>
    public ShipController GetPlayerShip()
    {
        return playerShip;
    }

    /// <summary>
    /// الحصول على طائرة الخصم
    /// </summary>
    public ShipController GetOpponentShip()
    {
        return opponentShip;
    }

    /// <summary>
    /// تحديث بيانات الطائرات من snapshot
    /// </summary>
    public void UpdateShipsFromSnapshot(NetworkGameState snapshotData)
    {
        if (snapshotData == null || snapshotData.player1 == null || snapshotData.player2 == null)
            return;

        try
        {
            // تحديد الطائرة المحلية والخصم
            if (playerShip != null && opponentShip != null)
            {
                // البحث عن اللاعب المحلي
                var authManager = FindObjectOfType<AuthManager>();
                int localPlayerId = authManager != null ? authManager.GetUserId() : 1;

                if (snapshotData.player1.id == localPlayerId)
                {
                    playerShip.UpdateFromSnapshot(snapshotData.player1);
                    opponentShip.UpdateFromSnapshot(snapshotData.player2);
                }
                else
                {
                    playerShip.UpdateFromSnapshot(snapshotData.player2);
                    opponentShip.UpdateFromSnapshot(snapshotData.player1);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameObjectSpawner] Error updating ships from snapshot: {e.Message}");
        }
    }
}