using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FishInfoUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject fishPanel;
    public Button closeButton;
    public Button collectButton;
    
    [Header("Fish Info Image")]
    public Image fishInfoImage; // תמונה סטטית של מידע הדג
    public Sprite fishInfoSprite; // התמונה שתוצג לכל הדגים
    
    [Header("Custom Graphics")]
    public Sprite customPanelSprite;
    public Vector2 panelSize = new Vector2(400f, 300f);
    
    [Header("Button Positions")]
    public Vector2 closeButtonPosition = new Vector2(-80f, -130f);
    public Vector2 collectButtonPosition = new Vector2(80f, -130f);
    
    [Header("Image Position")]
    public Vector2 fishInfoImagePosition = new Vector2(0f, 20f);
    public Vector2 fishInfoImageSize = new Vector2(350f, 200f);
    
    [Header("Auto Setup")]
    public bool autoCreateUI = true;
    
    [Header("Minigame Settings")]
    public string fishMinigameScene = "FishMinigame";
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private Canvas canvas;
    private FishSpawner.Fish currentFish;
    private RaftController raftController;
    private FishSpawner fishSpawner;
    private bool buttonsConnected = false;
    private bool isProcessingFishClick = false;
    
    void Start()
    {
        // Find required components
        #if UNITY_2023_1_OR_NEWER
        raftController = FindFirstObjectByType<RaftController>();
        fishSpawner = FindFirstObjectByType<FishSpawner>();
        canvas = FindFirstObjectByType<Canvas>();
        #else
        raftController = FindObjectOfType<RaftController>();
        fishSpawner = FindObjectOfType<FishSpawner>();
        canvas = FindObjectOfType<Canvas>();
        #endif
        
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found! Creating one...");
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create UI automatically if needed
        if (autoCreateUI && fishPanel == null)
        {
            CreateFishInfoUI();
        }
        
        // חיבור כפתורים רק פעם אחת
        ConnectButtons();
        
        // Hide panel initially
        if (fishPanel != null)
        {
            fishPanel.SetActive(false);
        }
        
        DebugLog("✅ FishInfoUI initialized successfully");
    }
    
    void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[FishInfoUI] {message}");
        }
    }
    
    void ConnectButtons()
    {
        if (buttonsConnected) 
        {
            DebugLog("⚠️ Buttons already connected, skipping...");
            return;
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseFishInfo);
            DebugLog("✅ Close button connected");
        }
        
        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(CollectCurrentFish);
            DebugLog("✅ Collect button connected");
        }
        
        buttonsConnected = true;
        DebugLog("✅ All buttons connected successfully");
    }
    
    void CreateFishInfoUI()
    {
        Debug.Log("Creating fish info UI...");
        
        // Create main panel
        GameObject panelGO = new GameObject("FishInfoPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        
        fishPanel = panelGO;
        Image panelImage = panelGO.AddComponent<Image>();
        
        // אם יש גרפיקה משלך - השתמש בה, אחרת ברירת מחדל
        if (customPanelSprite != null)
        {
            panelImage.sprite = customPanelSprite;
            panelImage.color = Color.white;
            Debug.Log("✅ Using custom panel sprite");
        }
        else
        {
            panelImage.color = new Color(0.1f, 0.2f, 0.4f, 0.95f);
            panelImage.sprite = CreateRoundedRectSprite();
            panelImage.type = Image.Type.Sliced;
        }
        
        // Setup panel rect
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;
        
        // Create fish info image instead of multiple text elements
        CreateFishInfoImage(panelGO.transform);
        
        // Create buttons
        CreateUIButton("CloseButton", "❌ Close", closeButtonPosition, panelGO.transform, out closeButton, null);
        CreateUIButton("CollectButton", "🎣 Collect Fish", collectButtonPosition, panelGO.transform, out collectButton, null);
        
        // Style collect button
        Image collectButtonImage = collectButton.GetComponent<Image>();
        if (collectButtonImage != null)
        {
            collectButtonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        }
        
        Debug.Log("✅ Fish info UI created successfully!");
    }
    
    void CreateFishInfoImage(Transform parent)
    {
        GameObject imageGO = new GameObject("FishInfoImage");
        imageGO.transform.SetParent(parent, false);
        
        fishInfoImage = imageGO.AddComponent<Image>();
        fishInfoImage.color = Color.white;
        
        // Set the static sprite if assigned
        if (fishInfoSprite != null)
        {
            fishInfoImage.sprite = fishInfoSprite;
        }
        else
        {
            // Create default placeholder if no sprite assigned
            fishInfoImage.sprite = CreateDefaultFishInfoSprite();
        }
        
        RectTransform imageRect = imageGO.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = fishInfoImagePosition;
        imageRect.sizeDelta = fishInfoImageSize;
        
        Debug.Log("✅ Fish info image created");
    }
    
    void CreateUIButton(string name, string text, Vector2 position, Transform parent, out Button buttonComponent, System.Action onClick)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.6f, 0.8f);
        buttonImage.sprite = CreateRoundedRectSprite();
        buttonImage.type = Image.Type.Sliced;
        
        buttonComponent = buttonGO.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage;
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(140f, 40f);
        
        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    Sprite CreateRoundedRectSprite()
    {
        // Create a simple rounded rectangle texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                // Create rounded corners
                bool isInCorner = (x < 4 || x >= size - 4) && (y < 4 || y >= size - 4);
                
                if (isInCorner)
                {
                    colors[y * size + x] = distance <= radius ? Color.white : Color.clear;
                }
                else
                {
                    colors[y * size + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
    }
    
    Sprite CreateDefaultFishInfoSprite()
    {
        // Create a simple placeholder fish info image
        int width = 256;
        int height = 128;
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        
        // Simple gradient background
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float gradientY = (float)y / height;
                Color bgColor = Color.Lerp(new Color(0.2f, 0.6f, 1f, 1f), new Color(0.1f, 0.4f, 0.8f, 1f), gradientY);
                colors[y * width + x] = bgColor;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    public void ShowFishInfo(FishSpawner.Fish fish)
    {
        if (fish == null || fishPanel == null) 
        {
            DebugLog("❌ Cannot show fish info - fish or panel is null");
            return;
        }
        
        // הגנה מפני לחיצות כפולות
        if (isProcessingFishClick)
        {
            DebugLog("⚠️ Already processing fish click, ignoring...");
            return;
        }
        
        isProcessingFishClick = true;
        DebugLog($"🐟 ShowFishInfo called for fish: {fish.GetInstanceID()}");
        
        // ניקוי יסודי של כל ה-state הקודם
        CompleteClearState();
        
        // קבע דג חדש
        currentFish = fish;
        DebugLog($"🎯 Current fish set to: {currentFish.GetInstanceID()}");
        
        // Update fish info image - always show the same static image
        if (fishInfoImage != null && fishInfoSprite != null)
        {
            fishInfoImage.sprite = fishInfoSprite;
        }
        
        // כפתור Collect תמיד זמין
        if (collectButton != null)
        {
            collectButton.interactable = true;
        }
        
        // וודא שהכפתורים מחוברים
        ConnectButtons();
        
        // Show panel
        fishPanel.SetActive(true);
        
        // שחרר את הנעילה לאחר זמן קצר
        StartCoroutine(ReleaseClickLock());
        
        DebugLog($"✅ Fish info panel shown - Static image displayed");
    }
    
    IEnumerator ReleaseClickLock()
    {
        yield return new WaitForSeconds(0.1f);
        isProcessingFishClick = false;
        DebugLog("🔓 Click lock released");
    }
    
    void CompleteClearState()
    {
        DebugLog("🧹 Starting complete state clear...");
        
        // נקה דג קודם
        if (currentFish != null)
        {
            currentFish.SetTargeted(false);
            DebugLog($"🎯 Cleared targeting from previous fish: {currentFish.GetInstanceID()}");
            currentFish = null;
        }
        
        // נקה כל הדגים מ-targeting
        if (fishSpawner != null)
        {
            fishSpawner.ClearAllTargets();
            DebugLog("🎯 Cleared all fish targets");
        }
        
        DebugLog("✅ Complete state clear finished");
    }
    
    float CalculateTravelTime(FishSpawner.Fish fish)
    {
        if (fish == null || raftController == null) return 0f;
        
        float distance = Vector2.Distance(raftController.transform.position, fish.transform.position);
        float raftSpeed = 3f;
        float travelTime = distance / raftSpeed;
        
        return travelTime;
    }
    
    public void CloseFishInfo()
    {
        DebugLog("🚪 CloseFishInfo called");
        
        if (fishPanel != null)
        {
            fishPanel.SetActive(false);
        }
        
        // ניקוי יסודי כשסוגרים
        CompleteClearState();
        
        DebugLog("✅ Fish info panel closed and state cleared");
    }
    
    public void CollectCurrentFish()
    {
        DebugLog("🎣 CollectCurrentFish called");
        
        if (currentFish == null)
        {
            DebugLog("❌ No fish selected for collection!");
            return;
        }
        
        DebugLog($"🎣 Starting collection for fish: {currentFish.GetInstanceID()}");
        
        // Clear any previous targets
        if (fishSpawner != null)
        {
            fishSpawner.ClearAllTargets();
        }
        
        // Set fish as target
        currentFish.SetTargeted(true);
        DebugLog($"🎯 Fish {currentFish.GetInstanceID()} set as target");
        
        // Move raft to fish
        if (raftController != null)
        {
            Vector2 fishPosition = currentFish.transform.position;
            raftController.MoveToPosition(fishPosition);
            
            // Start checking for collection
            StartCoroutine(CheckForFishCollection(currentFish));
            
            DebugLog($"🚢 Raft moving to collect fish at {fishPosition}");
        }
        else
        {
            DebugLog("❌ RaftController not found!");
        }
        
        // Close the info panel
        CloseFishInfo();
    }
    
    IEnumerator CheckForFishCollection(FishSpawner.Fish targetFish)
    {
        if (raftController == null || targetFish == null) yield break;
        
        float collectionDistance = 3f;
        float checkInterval = 0.2f;
        float maxWaitTime = 30f;
        float waitTime = 0f;
        
        DebugLog($"🎯 Starting collection check for fish: {targetFish.GetInstanceID()}");
        
        while (targetFish != null && !targetFish.isCollected && waitTime < maxWaitTime)
        {
            float distance = Vector2.Distance(raftController.transform.position, targetFish.transform.position);
            
            if (distance <= collectionDistance)
            {
                DebugLog($"🎉 Raft reached fish {targetFish.GetInstanceID()}! Loading minigame...");
                
                yield return StartCoroutine(LoadFishMinigame());
                
                targetFish.CollectFish();
                ShowCollectionEffect(targetFish.transform.position);
                
                DebugLog($"✅ Successfully collected fish {targetFish.GetInstanceID()}!");
                yield break;
            }
            
            waitTime += checkInterval;
            yield return new WaitForSeconds(checkInterval);
        }
        
        if (targetFish != null)
        {
            targetFish.SetTargeted(false);
            DebugLog($"⏰ Collection timeout for fish {targetFish.GetInstanceID()}");
        }
    }
    
    IEnumerator LoadFishMinigame()
    {
        DebugLog("🎮 Loading fish minigame...");
        
        if (string.IsNullOrEmpty(fishMinigameScene))
        {
            DebugLog("⚠️ Fish minigame scene name not set! Using default");
            fishMinigameScene = "FishMinigame";
        }
        
        DebugLog($"🎮 Attempting to load scene '{fishMinigameScene}'");
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(fishMinigameScene);
        
        if (asyncLoad == null)
        {
            DebugLog($"❌ Failed to start loading scene '{fishMinigameScene}'");
            yield return new WaitForSeconds(1f);
            yield break;
        }
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        DebugLog("✅ Minigame loaded successfully!");
    }
    
    void ShowCollectionEffect(Vector3 position)
    {
        DebugLog($"✨ Collection effect at {position}");
    }
    
    // Public methods for external control
    public bool IsShowing()
    {
        return fishPanel != null && fishPanel.activeInHierarchy;
    }
    
    public FishSpawner.Fish GetCurrentFish()
    {
        return currentFish;
    }
    
    public void SetMinigameScene(string sceneName)
    {
        fishMinigameScene = sceneName;
        DebugLog($"Fish minigame scene set to: {sceneName}");
    }
    
    // פונקציות לעיצוב מותאם אישית
    [ContextMenu("Update Panel Layout")]
    public void UpdatePanelLayout()
    {
        if (fishPanel != null)
        {
            // עדכן גודל פאנל
            RectTransform panelRect = fishPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = panelSize;
            }
            
            // עדכן מיקום תמונה
            if (fishInfoImage != null)
            {
                fishInfoImage.GetComponent<RectTransform>().anchoredPosition = fishInfoImagePosition;
                fishInfoImage.GetComponent<RectTransform>().sizeDelta = fishInfoImageSize;
            }
            
            // עדכן מיקומי כפתורים
            if (closeButton != null)
                closeButton.GetComponent<RectTransform>().anchoredPosition = closeButtonPosition;
            if (collectButton != null)
                collectButton.GetComponent<RectTransform>().anchoredPosition = collectButtonPosition;
            
            Debug.Log("✅ Panel layout updated with custom positions");
        }
    }
    
    [ContextMenu("Apply Custom Panel Sprite")]
    public void ApplyCustomPanelSprite()
    {
        if (fishPanel != null && customPanelSprite != null)
        {
            Image panelImage = fishPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = customPanelSprite;
                panelImage.color = Color.white;
                Debug.Log("✅ Custom panel sprite applied");
            }
        }
    }
    
    [ContextMenu("Test Fish Info UI")]
    void TestFishInfoUI()
    {
        #if UNITY_2023_1_OR_NEWER
        FishSpawner spawner = FindFirstObjectByType<FishSpawner>();
        #else
        FishSpawner spawner = FindObjectOfType<FishSpawner>();
        #endif
        
        if (spawner != null && spawner.spawnedFish.Count > 0)
        {
            foreach (var fish in spawner.spawnedFish)
            {
                if (fish != null && !fish.isCollected)
                {
                    ShowFishInfo(fish);
                    DebugLog("🧪 Testing fish info UI with static image");
                    return;
                }
            }
        }
        
        DebugLog("⚠️ No fish found for testing!");
    }
    
    [ContextMenu("Debug Current State")]
    void DebugCurrentState()
    {
        Debug.Log("=== FISH INFO UI DEBUG STATE ===");
        Debug.Log($"Current Fish: {(currentFish != null ? currentFish.GetInstanceID().ToString() : "NULL")}");
        Debug.Log($"Panel Active: {(fishPanel != null ? fishPanel.activeInHierarchy : false)}");
        Debug.Log($"Processing Click: {isProcessingFishClick}");
        Debug.Log($"Buttons Connected: {buttonsConnected}");
        Debug.Log($"Fish Info Sprite: {(fishInfoSprite != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"Fish Info Image: {(fishInfoImage != null ? "EXISTS" : "NULL")}");
        Debug.Log("===============================");
    }
}