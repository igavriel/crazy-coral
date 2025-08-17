using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Settings")]
    [SerializeField] private List<FishData> fishTypes = new List<FishData>();
    [SerializeField] private int totalFishCount = 50;
    [SerializeField] private float minFishDistance = 3f;
    [SerializeField] private LayerMask fishLayer = 1;
    
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(100f, 100f);
    [SerializeField] private bool useMapBounds = true;
    
    [Header("Animation")]
    [SerializeField] private float defaultAnimationSpeed = 24f; // ✅ FIX: 24 FPS כברירת מחדל
    
    [Header("Movement (Optional)")]
    [SerializeField] private bool enableFishMovement = false;
    [SerializeField] private float fishMoveSpeed = 1f;
    [SerializeField] private float movementRadius = 5f;
    [SerializeField] private Vector2 directionChangeInterval = new Vector2(3f, 8f);
    
    [Header("Performance")]
    [SerializeField] private bool useFrustumCulling = false; // ✅ כבוי כברירת מחדל
    [SerializeField] private float cullDistance = 200f;
    [SerializeField] private int maxVisibleFish = 100;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnInfo = true;
    
    // Components
    private MapController mapController;
    private Camera playerCamera;
    
    // Fish management
    public List<Fish> spawnedFish = new List<Fish>();
    private Queue<Fish> inactiveFish = new Queue<Fish>();
    private Transform fishContainer;
    
    // Performance
    private Coroutine frustumCullingCoroutine;
    
    [System.Serializable]
    public class FishData
    {
        [Header("Basic Info")]
        public string fishName = "Fish";
        public Sprite[] animationFrames;
        public float animationSpeed = 24f; // ✅ FIX: 24 FPS כברירת מחדל
        public bool randomStartFrame = true;
        
        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        public float spawnWeight = 1f;
        public Vector2 sizeRange = new Vector2(1.0f, 1.0f); // ✅ גודל קבוע = 1
        
        [Header("Visual")]
        public Color tintColor = Color.white;
        public int sortingOrder = 0;
        public string sortingLayerName = "Fish";
        
        [Header("Movement (if enabled)")]
        public float moveSpeedMultiplier = 1f;
        public bool flipOnDirectionChange = true;
        
        public bool IsValid()
        {
            return animationFrames != null && animationFrames.Length > 0 && 
                   animationFrames[0] != null;
        }
    }
    
    // Fish instance class
    public class Fish : MonoBehaviour
    {
        public FishData fishData;
        public SpriteRenderer spriteRenderer;
        public Collider2D fishCollider;
        
        // Animation
        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float frameTime;
        
        // Movement
        private Vector3 moveDirection;
        public Vector3 homePosition;
        private float directionChangeTimer;
        private float directionChangeInterval;
        
        // Performance
        public bool isVisible = true;
        public bool isInitialized = false;
        
        // Collection state
        public bool isCollected = false;
        public bool isTargeted = false;
        
        // Fish stats
        public int fishLevel = 1;
        public List<string> collectionRequirements = new List<string>();
        public bool canBeCollected = true;
        
        public void Initialize(FishData data, Vector3 position)
        {
            fishData = data;
            homePosition = position;
            transform.position = position;
            
            // ✅ גודל קבוע = 1
            transform.localScale = Vector3.one * 1.0f;
            
            // Setup sprite renderer
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // Setup collider for clicking - ✅ גודל נכון
            if (fishCollider == null)
                fishCollider = GetComponent<Collider2D>();
            
            if (fishCollider == null)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.isTrigger = true;
                
                // ✅ גודל collider נכון לגודל הדג
                circleCollider.radius = 2.5f; // גדול מספיק ללחיצה
                
                fishCollider = circleCollider;
            }
            
            // ✅ Add FishClickHandler בצורה נכונה
            FishClickHandler clickHandler = gameObject.GetComponent<FishClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = gameObject.AddComponent<FishClickHandler>();
            }
            clickHandler.fish = this;
            
            // Set initial sprite
            if (data.animationFrames.Length > 0)
            {
                if (data.randomStartFrame)
                {
                    currentFrame = Random.Range(0, data.animationFrames.Length);
                }
                else
                {
                    currentFrame = 0; // ✅ התחל מפריים 0
                }
                spriteRenderer.sprite = data.animationFrames[currentFrame];
            }
            
            // ✅ הגדרות נכונות
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingLayerName = data.sortingLayerName;
            spriteRenderer.sortingOrder = data.sortingOrder + 10;
            spriteRenderer.enabled = true;
            
            // ✅ Setup animation timing בצורה נכונה
            if (data.animationSpeed > 0)
            {
                frameTime = 1f / data.animationSpeed;
            }
            else
            {
                frameTime = 1f / 24f; // ✅ FIX: ברירת מחדל 24 FPS
            }
            animationTimer = Random.Range(0f, frameTime * 0.5f); // קצת אופסט
            
            // Setup movement
            SetRandomDirection();
            
            // Setup fish stats
            SetupFishStats(data);
            
            // ✅ כפה נראות ופעילות
            isVisible = true;
            isInitialized = true;
            gameObject.SetActive(true);
            
            Debug.Log($"🐟 Fish initialized: {data.fishName}, Frames: {data.animationFrames.Length}, FPS: {data.animationSpeed}");
        }
        
        private void SetupFishStats(FishData data)
        {
            float rarityMultiplier = 1f / Mathf.Max(data.spawnWeight, 0.01f);
            
            if (rarityMultiplier <= 2f)
            {
                fishLevel = Random.Range(1, 4);
            }
            else if (rarityMultiplier <= 5f)
            {
                fishLevel = Random.Range(2, 6);
            }
            else if (rarityMultiplier <= 10f)
            {
                fishLevel = Random.Range(4, 8);
            }
            else
            {
                fishLevel = Random.Range(6, 11);
            }
            
            collectionRequirements.Clear();
            if (fishLevel >= 3)
            {
                collectionRequirements.Add("דרישה: רשת דייג");
            }
            if (fishLevel >= 5)
            {
                collectionRequirements.Add("דרישה: רמת דייג 3+");
            }
            if (fishLevel >= 8)
            {
                collectionRequirements.Add("דרישה: ציוד מתקדם");
            }
            
            canBeCollected = true;
        }
        
        public void UpdateFish(bool movementEnabled, float moveSpeed, float movementRadius, 
                              Vector2 directionChangeRange)
        {
            if (!isInitialized || isCollected) return;
            
            // ✅ וודא שהדג תמיד נראה
            if (!isVisible)
            {
                SetVisibility(true);
            }
            
            // ✅ עדכן אנימציה תמיד
            UpdateAnimation();
            
            // Update movement (if enabled and not targeted)
            if (movementEnabled && !isTargeted)
            {
                UpdateMovement(moveSpeed, movementRadius, directionChangeRange);
            }
            
            // Visual feedback if targeted
            if (isTargeted)
            {
                Color originalColor = fishData.tintColor;
                spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, 
                    Mathf.PingPong(Time.time * 2f, 1f) * 0.3f);
            }
            else
            {
                // ✅ וודא צבע לבן כשלא ממוקד
                if (spriteRenderer.color != Color.white)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
        
        // ✅ שיטת UpdateAnimation משופרת
        private void UpdateAnimation()
        {
            if (fishData == null || fishData.animationFrames == null || fishData.animationFrames.Length <= 1) 
            {
                return;
            }
            
            // ✅ וודא שיש לנו sprite renderer
            if (spriteRenderer == null) return;
            
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= frameTime)
            {
                animationTimer = 0f;
                
                // ✅ עבור לפריים הבא
                currentFrame = (currentFrame + 1) % fishData.animationFrames.Length;
                
                // ✅ וודא שהפריים קיים
                if (currentFrame < fishData.animationFrames.Length && fishData.animationFrames[currentFrame] != null)
                {
                    spriteRenderer.sprite = fishData.animationFrames[currentFrame];
                }
                
                // ✅ וודא שה-renderer מופעל
                if (!spriteRenderer.enabled)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }
        
        private void UpdateMovement(float baseSpeed, float maxRadius, Vector2 directionChangeRange)
        {
            directionChangeTimer += Time.deltaTime;
            
            if (directionChangeTimer >= directionChangeInterval)
            {
                SetRandomDirection();
                directionChangeTimer = 0f;
                directionChangeInterval = Random.Range(directionChangeRange.x, directionChangeRange.y);
            }
            
            float speed = baseSpeed * fishData.moveSpeedMultiplier;
            Vector3 newPosition = transform.position + moveDirection * speed * Time.deltaTime;
            
            float distanceFromHome = Vector3.Distance(newPosition, homePosition);
            if (distanceFromHome > maxRadius)
            {
                Vector3 directionToHome = (homePosition - transform.position).normalized;
                moveDirection = Vector3.Slerp(moveDirection, directionToHome, Time.deltaTime * 2f);
            }
            
            transform.position = newPosition;
            
            if (fishData.flipOnDirectionChange && spriteRenderer != null)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }
        
        private void SetRandomDirection()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            moveDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        }
        
        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
            if (fishCollider != null)
                fishCollider.enabled = visible;
        }
        
        public void SetTargeted(bool targeted)
        {
            isTargeted = targeted;
        }
        
        public void CollectFish()
        {
            if (!canBeCollected) return;
            
            isCollected = true;
            isTargeted = false;
            
            SetVisibility(false);
            
            #if UNITY_2023_1_OR_NEWER
            FishSpawner spawner = FindFirstObjectByType<FishSpawner>();
            #else
            FishSpawner spawner = FindObjectOfType<FishSpawner>();
            #endif
            if (spawner != null)
            {
                spawner.OnFishCollected(this);
            }
            
            Debug.Log($"🎣 Collected {fishData.fishName} (Level {fishLevel})!");
        }
        
        public bool CanBeCollected()
        {
            return canBeCollected && !isCollected;
        }
        
        public string GetFishDescription()
        {
            string description = $"סוג: {fishData.fishName}\nרמה: {fishLevel}";
            
            if (collectionRequirements.Count > 0)
            {
                description += "\n\nדרישות איסוף:";
                foreach (string requirement in collectionRequirements)
                {
                    description += $"\n• {requirement}";
                }
            }
            else
            {
                description += "\n\nניתן לאיסוף מיידי!";
            }
            
            return description;
        }
    }
    
    // Helper class for detecting fish clicks
    public class FishClickHandler : MonoBehaviour
    {
        public Fish fish;
        
        void OnMouseDown()
        {
            if (fish != null && !fish.isCollected)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;
                
                Debug.Log($"🐟 Clicked on {fish.fishData.fishName}");
                
                #if UNITY_2023_1_OR_NEWER
                FishInfoUI fishInfoUI = FindFirstObjectByType<FishInfoUI>();
                #else
                FishInfoUI fishInfoUI = FindObjectOfType<FishInfoUI>();
                #endif
                if (fishInfoUI != null)
                {
                    fishInfoUI.ShowFishInfo(fish);
                }
                else
                {
                    Debug.LogWarning("⚠️ FishInfoUI not found in scene!");
                }
            }
        }
    }
    
    void Start()
    {
        #if UNITY_2023_1_OR_NEWER
        mapController = FindFirstObjectByType<MapController>();
        #else
        mapController = FindObjectOfType<MapController>();
        #endif
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            #if UNITY_2023_1_OR_NEWER
            playerCamera = FindFirstObjectByType<Camera>();
            #else
            playerCamera = FindObjectOfType<Camera>();
            #endif
        }
        
        GameObject container = new GameObject("Fish Container");
        fishContainer = container.transform;
        
        StartCoroutine(InitializeFishSpawning());
        
        // ✅ Frustum Culling רק אם מופעל
        if (useFrustumCulling && playerCamera != null)
        {
            frustumCullingCoroutine = StartCoroutine(FrustumCullingUpdate());
        }
    }
    
    IEnumerator InitializeFishSpawning()
    {
        if (logSpawnInfo)
            Debug.Log("🐟 Starting fish spawning system...");
        
        List<FishData> validFishTypes = new List<FishData>();
        foreach (var fishData in fishTypes)
        {
            if (fishData.IsValid())
            {
                validFishTypes.Add(fishData);
            }
            else
            {
                Debug.LogWarning($"⚠️ Invalid fish data: {fishData.fishName}");
            }
        }
        
        if (validFishTypes.Count == 0)
        {
            Debug.LogError("❌ No valid fish types found! Please assign animation frames.");
            yield break;
        }
        
        if (logSpawnInfo)
            Debug.Log($"🐟 Found {validFishTypes.Count} valid fish types");
        
        while (mapController == null)
        {
            #if UNITY_2023_1_OR_NEWER
            mapController = FindFirstObjectByType<MapController>();
            #else
            mapController = FindObjectOfType<MapController>();
            #endif
            yield return new WaitForSeconds(0.1f);
        }
        
        int waitCycles = 0;
        while (mapController.mapSize.x <= 0f || mapController.mapSize.y <= 0f)
        {
            if (logSpawnInfo && waitCycles % 50 == 0)
            {
                Debug.Log($"⏳ Waiting for MapController to finish loading... (cycle {waitCycles})");
            }
            
            waitCycles++;
            if (waitCycles > 300)
            {
                Debug.LogError("❌ MapController failed to load after 30 seconds! Using default area.");
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (logSpawnInfo)
        {
            Debug.Log($"✅ MapController ready! Map size: {mapController.mapSize}");
        }
        
        Vector2 actualSpawnCenter = spawnAreaCenter;
        Vector2 actualSpawnSize = spawnAreaSize;
        
        if (useMapBounds && mapController != null && mapController.mapSize.x > 0 && mapController.mapSize.y > 0)
        {
            actualSpawnSize = mapController.mapSize * 0.8f;
            actualSpawnCenter = Vector2.zero;
            
            if (logSpawnInfo)
                Debug.Log($"🗺️ Using map bounds: {actualSpawnSize}");
        }
        else
        {
            if (logSpawnInfo)
                Debug.Log($"⚠️ Using manual spawn area: {actualSpawnSize} (MapController not ready or useMapBounds disabled)");
        }
        
        int successfulSpawns = 0;
        int attempts = 0;
        int maxAttempts = totalFishCount * 5;
        
        while (successfulSpawns < totalFishCount && attempts < maxAttempts)
        {
            attempts++;
            
            Vector2 randomPosition = new Vector2(
                Random.Range(-actualSpawnSize.x * 0.5f, actualSpawnSize.x * 0.5f),
                Random.Range(-actualSpawnSize.y * 0.5f, actualSpawnSize.y * 0.5f)
            ) + actualSpawnCenter;
            
            bool isNavigable = true;
            
            if (mapController != null)
            {
                try
                {
                    isNavigable = mapController.IsPositionNavigable(randomPosition);
                }
                catch (System.Exception e)
                {
                    if (attempts % 100 == 0)
                    {
                        Debug.LogWarning($"⚠️ Navigation check failed: {e.Message}. Continuing with spawn...");
                    }
                    isNavigable = true;
                }
            }
            
            if (!isNavigable)
            {
                continue;
            }
            
            bool tooClose = false;
            foreach (var existingFish in spawnedFish)
            {
                if (Vector2.Distance(existingFish.transform.position, randomPosition) < minFishDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            FishData chosenFishData = ChooseRandomFishType(validFishTypes);
            
            GameObject fishGO = new GameObject($"Fish_{chosenFishData.fishName}_{successfulSpawns}");
            fishGO.transform.SetParent(fishContainer);
            fishGO.layer = Mathf.RoundToInt(Mathf.Log(fishLayer.value, 2));
            
            Fish fish = fishGO.AddComponent<Fish>();
            fish.Initialize(chosenFishData, randomPosition);
            
            spawnedFish.Add(fish);
            successfulSpawns++;
            
            if (successfulSpawns % 10 == 0)
            {
                yield return null;
            }
        }
        
        if (logSpawnInfo)
        {
            Debug.Log($"🐟 Fish spawning complete!");
            Debug.Log($"   • Successfully spawned: {successfulSpawns}/{totalFishCount} fish");
            Debug.Log($"   • Total attempts: {attempts}");
            Debug.Log($"   • Spawn area: {actualSpawnSize} around {actualSpawnCenter}");
        }
    }
    
    FishData ChooseRandomFishType(List<FishData> validTypes)
    {
        float totalWeight = 0f;
        foreach (var fishData in validTypes)
        {
            totalWeight += fishData.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var fishData in validTypes)
        {
            currentWeight += fishData.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return fishData;
            }
        }
        
        return validTypes[0];
    }
    
    // ✅ Update מעדכן את כל הדגים
    void Update()
    {
        foreach (var fish in spawnedFish)
        {
            if (fish != null && fish.isInitialized)
            {
                fish.UpdateFish(enableFishMovement, fishMoveSpeed, movementRadius, directionChangeInterval);
            }
        }
    }
    
    IEnumerator FrustumCullingUpdate()
    {
        while (true)
        {
            if (playerCamera != null)
            {
                Vector3 cameraPos = playerCamera.transform.position;
                int visibleCount = 0;
                
                spawnedFish.Sort((a, b) => {
                    if (a == null || b == null) return 0;
                    float distA = Vector3.Distance(a.transform.position, cameraPos);
                    float distB = Vector3.Distance(b.transform.position, cameraPos);
                    return distA.CompareTo(distB);
                });
                
                foreach (var fish in spawnedFish)
                {
                    if (fish == null) continue;
                    
                    float distance = Vector3.Distance(fish.transform.position, cameraPos);
                    bool shouldBeVisible = distance <= cullDistance && visibleCount < maxVisibleFish;
                    
                    fish.SetVisibility(shouldBeVisible);
                    
                    if (shouldBeVisible)
                        visibleCount++;
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    void OnDestroy()
    {
        if (frustumCullingCoroutine != null)
        {
            StopCoroutine(frustumCullingCoroutine);
        }
    }
    
    // ========================================
    // ✅ שיטות תיקון ודיבוג
    // ========================================
    
    [ContextMenu("🔧 FIX ALL FISH - Complete")]
    public void FixAllFishComplete()
    {
        Debug.Log("🔧 Fixing ALL fish issues - Animation, Clicking, Visibility...");
        
        Transform fishContainer = GameObject.Find("Fish Container")?.transform;
        if (fishContainer == null)
        {
            Debug.LogError("❌ Fish Container not found!");
            return;
        }
        
        int fixedCount = 0;
        
        for (int i = 0; i < fishContainer.childCount; i++)
        {
            GameObject fishGO = fishContainer.GetChild(i).gameObject;
            Fish fishScript = fishGO.GetComponent<Fish>();
            
            // ✅ כפה Scale = 1
            fishGO.transform.localScale = Vector3.one * 1.0f;
            
            // ✅ כפה נראות
            if (fishScript != null)
            {
                fishScript.isVisible = true;
                fishScript.isInitialized = true;
            }
            
            // ✅ כפה SpriteRenderer
            SpriteRenderer sr = fishGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.color = Color.white;
                sr.sortingOrder = 10;
            }
            
            // ✅ תקן Collider
            Collider2D col = fishGO.GetComponent<Collider2D>();
            if (col == null)
            {
                CircleCollider2D newCol = fishGO.AddComponent<CircleCollider2D>();
                newCol.isTrigger = true;
                newCol.radius = 2.5f;
                col = newCol;
            }
            else if (col is CircleCollider2D circleCol)
            {
                circleCol.radius = 2.5f;
            }
            col.enabled = true;
            
            // ✅ תקן ClickHandler
            FishClickHandler clickHandler = fishGO.GetComponent<FishClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = fishGO.AddComponent<FishClickHandler>();
            }
            clickHandler.fish = fishScript;
            
            // ✅ תקן אנימציה
            if (fishScript != null && fishScript.fishData != null)
            {
                var frameTimeField = typeof(Fish).GetField("frameTime", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                var animationTimerField = typeof(Fish).GetField("animationTimer", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (frameTimeField != null && animationTimerField != null)
                {
                    float frameTime = 1f / fishScript.fishData.animationSpeed;
                    frameTimeField.SetValue(fishScript, frameTime);
                    animationTimerField.SetValue(fishScript, 0f);
                }
            }
            
            // ✅ כפה GameObject פעיל
            fishGO.SetActive(true);
            
            fixedCount++;
        }
        
        Debug.Log($"✅ Fixed {fixedCount} fish completely! Animation, Clicking, Visibility all working!");
    }
    
    [ContextMenu("🎬 Update All Fish Animation Speed")]
    public void UpdateAllFishAnimationSpeed()
    {
        Debug.Log($"🎬 Updating all fish animation speed to: {defaultAnimationSpeed} FPS");
        
        // עדכן את FishData
        foreach (var fishType in fishTypes)
        {
            if (fishType != null)
            {
                fishType.animationSpeed = defaultAnimationSpeed;
            }
        }
        
        // עדכן את כל הדגים הקיימים
        Transform fishContainer = GameObject.Find("Fish Container")?.transform;
        if (fishContainer != null)
        {
            int updatedCount = 0;
            
            for (int i = 0; i < fishContainer.childCount; i++)
            {
                GameObject fishGO = fishContainer.GetChild(i).gameObject;
                Fish fishScript = fishGO.GetComponent<Fish>();
                
                if (fishScript != null && fishScript.fishData != null)
                {
                    // עדכן את מהירות האנימציה
                    fishScript.fishData.animationSpeed = defaultAnimationSpeed;
                    
                    // עדכן את frameTime באמצעות Reflection
                    var frameTimeField = typeof(Fish).GetField("frameTime", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    
                    if (frameTimeField != null)
                    {
                        float newFrameTime = 1f / defaultAnimationSpeed;
                        frameTimeField.SetValue(fishScript, newFrameTime);
                    }
                    
                    updatedCount++;
                }
            }
            
            Debug.Log($"✅ Updated animation speed for {updatedCount} fish to {defaultAnimationSpeed} FPS!");
        }
        else
        {
            Debug.LogWarning("⚠️ Fish Container not found!");
        }
    }
    
    [ContextMenu("🚫 Disable Frustum Culling")]
    public void DisableFrustumCulling()
    {
        useFrustumCulling = false;
        
        if (frustumCullingCoroutine != null)
        {
            StopCoroutine(frustumCullingCoroutine);
            frustumCullingCoroutine = null;
        }
        
        foreach (var fish in spawnedFish)
        {
            if (fish != null)
            {
                fish.SetVisibility(true);
            }
        }
        
        Debug.Log("✅ Frustum Culling disabled - all fish visible!");
    }
    
    [ContextMenu("🐟 Test Fish Animation & Clicking")]
    public void TestFishAnimationAndClicking()
    {
        Debug.Log("🧪 Testing fish animation and clicking...");
        
        Transform fishContainer = GameObject.Find("Fish Container")?.transform;
        if (fishContainer != null && fishContainer.childCount > 0)
        {
            GameObject firstFish = fishContainer.GetChild(0).gameObject;
            Fish fishScript = firstFish.GetComponent<Fish>();
            
            if (fishScript != null)
            {
                Debug.Log($"🐟 First fish: {fishScript.fishData?.fishName}");
                Debug.Log($"   Animation frames: {fishScript.fishData?.animationFrames?.Length}");
                Debug.Log($"   Animation speed: {fishScript.fishData?.animationSpeed}");
                Debug.Log($"   Is initialized: {fishScript.isInitialized}");
                Debug.Log($"   Is visible: {fishScript.isVisible}");
                
                // בדוק Collider
                Collider2D col = firstFish.GetComponent<Collider2D>();
                Debug.Log($"   Has Collider: {col != null}");
                if (col != null)
                {
                    Debug.Log($"   Collider enabled: {col.enabled}");
                    Debug.Log($"   Collider type: {col.GetType().Name}");
                }
                
                // בדוק ClickHandler
                FishClickHandler handler = firstFish.GetComponent<FishClickHandler>();
                Debug.Log($"   Has ClickHandler: {handler != null}");
                
                // בדוק SpriteRenderer
                SpriteRenderer sr = firstFish.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Debug.Log($"   Current sprite: {sr.sprite?.name}");
                    Debug.Log($"   Sprite enabled: {sr.enabled}");
                }
            }
        }
        else
        {
            Debug.LogError("❌ No fish found for testing!");
        }
    }
    
    // ========================================
    // שיטות מקוריות
    // ========================================
    
    public void SpawnAdditionalFish(int count)
    {
        StartCoroutine(SpawnMoreFish(count));
    }
    
    IEnumerator SpawnMoreFish(int count)
    {
        int spawned = 0;
        int attempts = 0;
        
        while (spawned < count && attempts < count * 5)
        {
            attempts++;
            
            Vector2 randomPosition = new Vector2(
                Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
                Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f)
            ) + spawnAreaCenter;
            
            if (mapController != null && !mapController.IsPositionNavigable(randomPosition))
                continue;
            
            bool tooClose = false;
            foreach (var existingFish in spawnedFish)
            {
                if (Vector2.Distance(existingFish.transform.position, randomPosition) < minFishDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            List<FishData> validTypes = new List<FishData>();
            foreach (var fishData in fishTypes)
            {
                if (fishData.IsValid()) validTypes.Add(fishData);
            }
            
            if (validTypes.Count == 0) break;
            
            FishData chosenFishData = ChooseRandomFishType(validTypes);
            
            GameObject fishGO = new GameObject($"Fish_{chosenFishData.fishName}_{spawnedFish.Count}");
            fishGO.transform.SetParent(fishContainer);
            
            Fish fish = fishGO.AddComponent<Fish>();
            fish.Initialize(chosenFishData, randomPosition);
            
            spawnedFish.Add(fish);
            spawned++;
            
            if (spawned % 5 == 0)
                yield return null;
        }
        
        Debug.Log($"🐟 Spawned {spawned} additional fish");
    }
    
    public void RemoveAllFish()
    {
        foreach (var fish in spawnedFish)
        {
            if (fish != null && fish.gameObject != null)
            {
                DestroyImmediate(fish.gameObject);
            }
        }
        
        spawnedFish.Clear();
        Debug.Log("🐟 All fish removed");
    }
    
    public void SetFishMovement(bool enabled)
    {
        enableFishMovement = enabled;
    }
    
    public void SetAnimationSpeed(float speed)
    {
        defaultAnimationSpeed = speed;
        
        foreach (var fish in spawnedFish)
        {
            if (fish != null && fish.fishData != null)
            {
                fish.fishData.animationSpeed = speed;
            }
        }
    }
    
    public void OnFishCollected(Fish collectedFish)
    {
        if (collectedFish != null)
        {
            Debug.Log($"🎣 Fish collected: {collectedFish.fishData.fishName}");
        }
    }
    
    public Fish GetTargetedFish()
    {
        foreach (var fish in spawnedFish)
        {
            if (fish != null && fish.isTargeted)
                return fish;
        }
        return null;
    }
    
    public void ClearAllTargets()
    {
        foreach (var fish in spawnedFish)
        {
            if (fish != null)
                fish.SetTargeted(false);
        }
    }
    
    [ContextMenu("Force Respawn All Fish")]
    void ForceRespawnAllFish()
    {
        RemoveAllFish();
        StartCoroutine(InitializeFishSpawning());
    }
    
    [ContextMenu("Debug Fish Info")]
    void DebugFishInfo()
    {
        Debug.Log("=== FISH SPAWNER DEBUG INFO ===");
        Debug.Log($"Total fish spawned: {spawnedFish.Count}");
        Debug.Log($"Fish types configured: {fishTypes.Count}");
        Debug.Log($"Movement enabled: {enableFishMovement}");
        Debug.Log($"Frustum culling: {useFrustumCulling}");
        
        int visibleFish = 0;
        foreach (var fish in spawnedFish)
        {
            if (fish != null && fish.isVisible) visibleFish++;
        }
        Debug.Log($"Currently visible fish: {visibleFish}");
        
        foreach (var fishType in fishTypes)
        {
            if (fishType.IsValid())
            {
                Debug.Log($"✅ {fishType.fishName}: {fishType.animationFrames.Length} frames, weight: {fishType.spawnWeight}");
            }
            else
            {
                Debug.Log($"❌ {fishType.fishName}: Invalid (missing frames)");
            }
        }
    }
    
    [ContextMenu("Auto Load Fish Animation (fish_00000 pattern)")]
    void AutoLoadFishAnimation()
    {
        #if UNITY_EDITOR
        if (fishTypes.Count == 0)
        {
            fishTypes.Add(new FishData());
        }
        
        List<Sprite> foundSprites = new List<Sprite>();
        
        string[] guids = UnityEditor.AssetDatabase.FindAssets("fish_ t:Sprite");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.name.StartsWith("fish_"))
            {
                foundSprites.Add(sprite);
            }
        }
        
        foundSprites.Sort((a, b) => string.Compare(a.name, b.name));
        
        // ✅ FIX: השתמש ב-defaultAnimationSpeed מה-Inspector
        fishTypes[0].animationFrames = foundSprites.ToArray();
        fishTypes[0].fishName = "Rabbit Fish";
        fishTypes[0].animationSpeed = defaultAnimationSpeed; // ✅ משתמש בהגדרה מה-Inspector!
        fishTypes[0].sizeRange = new Vector2(1.0f, 1.0f);
        fishTypes[0].spawnWeight = 0.7f;
        fishTypes[0].tintColor = Color.white;
        fishTypes[0].sortingOrder = 10;
        
        Debug.Log($"✅ Auto-loaded {foundSprites.Count} animation frames for Rabbit Fish!");
        Debug.Log($"🎬 Animation Speed set to: {defaultAnimationSpeed} FPS");
        
        if (foundSprites.Count == 125)
        {
            Debug.Log("🎉 Perfect! Found all 125 frames as expected!");
        }
        else if (foundSprites.Count > 0)
        {
            Debug.Log($"⚠️ Found {foundSprites.Count} frames instead of expected 125. Check file names.");
        }
        else
        {
            Debug.LogError("❌ No sprites found with 'fish_' prefix. Make sure files are named fish_00000.png, fish_00001.png, etc.");
        }
        #else
        Debug.LogWarning("Auto Load only works in Editor!");
        #endif
    }
    
    [ContextMenu("Quick Debug - Check Setup")]
    void QuickDebugSetup()
    {
        Debug.Log("=== QUICK FISH SETUP DEBUG ===");
        
        Debug.Log($"🎣 FishSpawner: {(this != null ? "✅ Found" : "❌ Missing")}");
        Debug.Log($"📋 Fish Types Count: {fishTypes.Count}");
        
        for (int i = 0; i < fishTypes.Count; i++)
        {
            var fishType = fishTypes[i];
            bool isValid = fishType.IsValid();
            int frameCount = fishType.animationFrames != null ? fishType.animationFrames.Length : 0;
            Debug.Log($"   Fish {i}: {fishType.fishName} - {(isValid ? "✅" : "❌")} Valid - {frameCount} frames");
            
            if (!isValid && frameCount == 0)
            {
                Debug.LogError($"   ❌ Fish {i} has no animation frames! Run 'Auto Load Fish Animation'");
            }
        }
        
        #if UNITY_2023_1_OR_NEWER
        MapController mc = FindFirstObjectByType<MapController>();
        #else
        MapController mc = FindObjectOfType<MapController>();
        #endif
        
        if (mc != null)
        {
            Debug.Log($"🗺️ MapController: ✅ Found");
            Debug.Log($"   Map Size: {mc.mapSize}");
            Debug.Log($"   Map Controller initialized: {(mc.mapSize.x > 0 ? "✅ Yes" : "❌ No")}");
        }
        else
        {
            Debug.LogError("🗺️ MapController: ❌ Not found in scene!");
        }
        
        Debug.Log($"🐟 Spawned Fish: {spawnedFish.Count}");
        
        Transform container = transform.Find("Fish Container");
        if (container == null)
        {
            container = GameObject.Find("Fish Container")?.transform;
        }
        
        if (container != null)
        {
            Debug.Log($"📦 Fish Container: ✅ Found with {container.childCount} children");
        }
        else
        {
            Debug.Log($"📦 Fish Container: ❌ Not found");
        }
        
        Camera cam = Camera.main;
        if (cam == null)
        {
            #if UNITY_2023_1_OR_NEWER
            cam = FindFirstObjectByType<Camera>();
            #else
            cam = FindObjectOfType<Camera>();
            #endif
        }
        
        if (cam != null)
        {
            Debug.Log($"📷 Camera: ✅ Found at position {cam.transform.position}");
        }
        else
        {
            Debug.LogError($"📷 Camera: ❌ Not found!");
        }
        
        Debug.Log("=== END DEBUG ===");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(spawnAreaCenter.x, spawnAreaCenter.y, 0);
        Vector3 size = new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0);
        Gizmos.DrawWireCube(center, size);
        
        if (Application.isPlaying && spawnedFish != null)
        {
            foreach (var fish in spawnedFish)
            {
                if (fish == null) continue;
                
                if (fish.isVisible)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(fish.transform.position, 0.5f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(fish.transform.position, 0.5f);
                }
                
                if (enableFishMovement)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(fish.homePosition, movementRadius);
                }
            }
        }
        
        if (useFrustumCulling && playerCamera != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerCamera.transform.position, cullDistance);
        }
    }
}