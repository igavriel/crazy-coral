using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandInfoUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject islandPanel;
    public Button closeButton;
    public Button visitButton;
    
    [Header("Island Info Image")]
    public Image islandInfoImage; // תמונה סטטית של מידע האי
    public Sprite islandInfoSprite; // התמונה שתוצג לכל האיים
    
    [Header("Custom Graphics")]
    public Sprite customPanelSprite;
    public Vector2 panelSize = new Vector2(400f, 300f);
    
    [Header("Button Positions")]
    public Vector2 closeButtonPosition = new Vector2(-80f, -130f);
    public Vector2 visitButtonPosition = new Vector2(80f, -130f);
    
    [Header("Image Position")]
    public Vector2 islandInfoImagePosition = new Vector2(0f, 20f);
    public Vector2 islandInfoImageSize = new Vector2(350f, 200f);
    
    [Header("Auto Setup")]
    public bool autoCreateUI = true;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private MapController mapController;
    private MapController.IslandData currentIsland;
    private Canvas canvas;
    private bool buttonsConnected = false;
    
    void Start()
    {
        // מצא את MapController - תיקון ל-Unity 2023+
        #if UNITY_2023_1_OR_NEWER
        mapController = FindFirstObjectByType<MapController>();
        canvas = FindFirstObjectByType<Canvas>();
        #else
        mapController = FindObjectOfType<MapController>();
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
        
        // יצירת UI אוטומטית אם צריך
        if (autoCreateUI && islandPanel == null)
        {
            CreateSimpleUI();
        }
        
        // חיבור כפתורים
        ConnectButtons();
        
        // התחברות לEvent
        if (mapController != null)
        {
            mapController.OnIslandClicked.AddListener(OnIslandClicked);
            DebugLog("✅ Connected to MapController events");
        }
        else
        {
            Debug.LogError("❌ MapController not found!");
        }
        
        // הסתרת הפאנל בהתחלה
        if (islandPanel != null)
        {
            islandPanel.SetActive(false);
        }
    }
    
    void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[IslandInfoUI] {message}");
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
            closeButton.onClick.AddListener(ClosePanel);
            DebugLog("✅ Close button connected");
        }
        
        if (visitButton != null)
        {
            visitButton.onClick.RemoveAllListeners();
            visitButton.onClick.AddListener(VisitIsland);
            DebugLog("✅ Visit button connected");
        }
        
        buttonsConnected = true;
        DebugLog("✅ All buttons connected successfully");
    }
    
    void CreateSimpleUI()
    {
        Debug.Log("Creating simple UI for island info...");
        
        // יצירת פאנל הראשי
        GameObject panelGO = new GameObject("IslandPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        
        islandPanel = panelGO;
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
            panelImage.color = new Color(0f, 0f, 0f, 0.8f); // רקע שחור שקוף
        }
        
        // הגדרת גודל ומיקום הפאנל
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;
        
        // Create island info image instead of multiple text elements
        CreateIslandInfoImage(panelGO.transform);
        
        // כפתור סגירה
        CreateUIButton("CloseButton", "❌ Close", closeButtonPosition, panelGO.transform, out closeButton, ClosePanel);
        
        // כפתור ביקור
        CreateUIButton("VisitButton", "🚢 Visit Island", visitButtonPosition, panelGO.transform, out visitButton, VisitIsland);
        
        Debug.Log("✅ Simple UI created successfully!");
    }
    
    void CreateIslandInfoImage(Transform parent)
    {
        GameObject imageGO = new GameObject("IslandInfoImage");
        imageGO.transform.SetParent(parent, false);
        
        islandInfoImage = imageGO.AddComponent<Image>();
        islandInfoImage.color = Color.white;
        
        // Set the static sprite if assigned
        if (islandInfoSprite != null)
        {
            islandInfoImage.sprite = islandInfoSprite;
        }
        else
        {
            // Create default placeholder if no sprite assigned
            islandInfoImage.sprite = CreateDefaultIslandInfoSprite();
        }
        
        RectTransform imageRect = imageGO.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = islandInfoImagePosition;
        imageRect.sizeDelta = islandInfoImageSize;
        
        Debug.Log("✅ Island info image created");
    }
    
    void CreateUIButton(string name, string text, Vector2 position, Transform parent, out Button buttonComponent, System.Action onClick)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);
        
        buttonComponent = buttonGO.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage;
        
        if (onClick != null)
        {
            buttonComponent.onClick.AddListener(() => onClick());
        }
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(140f, 40f);
        
        // טקסט על הכפתור
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    Sprite CreateDefaultIslandInfoSprite()
    {
        // Create a simple placeholder island info image
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
                Color bgColor = Color.Lerp(new Color(1f, 0.8f, 0.4f, 1f), new Color(0.8f, 0.6f, 0.2f, 1f), gradientY);
                colors[y * width + x] = bgColor;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    void OnIslandClicked(int islandId, MapController.IslandData islandData)
    {
        DebugLog($"🏝️ UI received island click: {islandId}");
        
        if (islandData != null)
        {
            ShowIslandInfo(islandData);
        }
        else
        {
            ShowUnknownIslandInfo(islandId);
        }
    }
    
    void ShowIslandInfo(MapController.IslandData island)
    {
        currentIsland = island;
        
        if (islandPanel != null)
        {
            islandPanel.SetActive(true);
        }
        
        // Update island info image - always show the same static image
        if (islandInfoImage != null && islandInfoSprite != null)
        {
            islandInfoImage.sprite = islandInfoSprite;
        }
        
        // Update visit button state based on visited status
        if (visitButton != null)
        {
            visitButton.interactable = !island.isVisited;
            Text buttonText = visitButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = island.isVisited ? "✅ Visited" : "🚢 Visit Island";
        }
        
        DebugLog($"✅ Showing static info for {island.name}");
    }
    
    void ShowUnknownIslandInfo(int islandId)
    {
        currentIsland = null;
        
        if (islandPanel != null)
        {
            islandPanel.SetActive(true);
        }
        
        // Update island info image - show the same static image even for unknown islands
        if (islandInfoImage != null && islandInfoSprite != null)
        {
            islandInfoImage.sprite = islandInfoSprite;
        }
        
        if (visitButton != null)
        {
            visitButton.interactable = false;
            Text buttonText = visitButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = "❌ Cannot Visit";
        }
        
        DebugLog("✅ Showing static info for unknown island");
    }
    
    public void ClosePanel()
    {
        if (islandPanel != null)
        {
            islandPanel.SetActive(false);
        }
        
        currentIsland = null;
        DebugLog("🚪 Island info panel closed");
    }
    
    public void VisitIsland()
    {
        if (currentIsland != null && mapController != null)
        {
            // סמן כמבוקר
            mapController.MarkIslandAsVisited(currentIsland.id);
            
            // עדכן UI
            currentIsland.isVisited = true;
            
            if (visitButton != null)
            {
                visitButton.interactable = false;
                Text buttonText = visitButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = "✅ Visited";
            }
            
            DebugLog($"🎉 Visited {currentIsland.name}!");
        }
    }
    
    // פונקציות לעיצוב מותאם אישית
    [ContextMenu("Update Panel Layout")]
    public void UpdatePanelLayout()
    {
        if (islandPanel != null)
        {
            // עדכן גודל פאנל
            RectTransform panelRect = islandPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = panelSize;
            }
            
            // עדכן מיקום תמונה
            if (islandInfoImage != null)
            {
                islandInfoImage.GetComponent<RectTransform>().anchoredPosition = islandInfoImagePosition;
                islandInfoImage.GetComponent<RectTransform>().sizeDelta = islandInfoImageSize;
            }
            
            // עדכן מיקומי כפתורים
            if (closeButton != null)
                closeButton.GetComponent<RectTransform>().anchoredPosition = closeButtonPosition;
            if (visitButton != null)
                visitButton.GetComponent<RectTransform>().anchoredPosition = visitButtonPosition;
            
            Debug.Log("✅ Panel layout updated with custom positions");
        }
    }
    
    [ContextMenu("Apply Custom Panel Sprite")]
    public void ApplyCustomPanelSprite()
    {
        if (islandPanel != null && customPanelSprite != null)
        {
            Image panelImage = islandPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = customPanelSprite;
                panelImage.color = Color.white;
                Debug.Log("✅ Custom panel sprite applied");
            }
        }
    }
    
    [ContextMenu("Test Island Info UI")]
    void TestIslandInfoUI()
    {
        if (mapController != null)
        {
            var islands = mapController.GetAllIslands();
            if (islands.Count > 0)
            {
                ShowIslandInfo(islands[0]);
                DebugLog("🧪 Testing island info UI with static image");
            }
            else
            {
                DebugLog("⚠️ No islands found for testing!");
            }
        }
        else
        {
            DebugLog("⚠️ MapController not found for testing!");
        }
    }
    
    [ContextMenu("Debug Current State")]
    void DebugCurrentState()
    {
        Debug.Log("=== ISLAND INFO UI DEBUG STATE ===");
        Debug.Log($"Current Island: {(currentIsland != null ? currentIsland.name : "NULL")}");
        Debug.Log($"Panel Active: {(islandPanel != null ? islandPanel.activeInHierarchy : false)}");
        Debug.Log($"Buttons Connected: {buttonsConnected}");
        Debug.Log($"Island Info Sprite: {(islandInfoSprite != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"Island Info Image: {(islandInfoImage != null ? "EXISTS" : "NULL")}");
        Debug.Log($"MapController: {(mapController != null ? "EXISTS" : "NULL")}");
        Debug.Log("===============================");
    }
}