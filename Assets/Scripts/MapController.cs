using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapController : MonoBehaviour
{
    [Header("Map Setup")]
    [SerializeField] private Texture2D navigationMask; // PNG עם שקיפות - איים שחורים, מים שקופים
    [SerializeField] private Texture2D originalMap; // המפה הצבעונית המקורית
    [SerializeField] private SpriteRenderer mapRenderer;
    [SerializeField] private Camera mapCamera;
    
    [Header("Navigation")]
    [SerializeField] private RaftController raftController;
    [SerializeField] private float pixelsPerUnit = 5f;
    [SerializeField] private int navigationResolution = 256; // רזולוציית Grid לניווט
    [SerializeField] private int islandMargin = 1; // מרווח מהאיים (תאי grid) - 0 = ללא מרווח
    [SerializeField] private bool smartMargin = true; // מרווח חכם - רק לאיים גדולים
    [SerializeField] private bool smoothPaths = true; // מסלולים מחוכמים
    [SerializeField] private int pathSmoothingSteps = 8; // רזולוציית החלקה
    
    [Header("Mobile Controls")]
    [SerializeField] private float dragSensitivity = 2f;
    [SerializeField] private float zoomSensitivity = 1f;
    [SerializeField] private float minZoom = 40f;
    [SerializeField] private float maxZoom = 150f;
    [SerializeField] private float initialZoom = 80f;
    
    [Header("Camera")]
    [SerializeField] private bool followRaft = true;
    [SerializeField] private float cameraFollowSpeed = 2f;
    [SerializeField] private float followStartDelay = 0.5f;
    
    [Header("Island Detection")]
    [SerializeField] private bool useColorBasedIslands = false; // כבוי למפות גדולות
    [SerializeField] private bool autoDetectIslands = true; // מופעל לזיהוי אוטומטי
    [SerializeField] private int minIslandSize = 25; // גודל מינימלי לאי (פיקסלים) - מותאם ל-100 איים
    [SerializeField] private bool optimizeForManyIslands = true; // אופטימיזציה לאיים רבים
    [SerializeField] private bool generateUniqueNames = true; // שמות ייחודיים אוטומטיים
    
    // Events
    [System.Serializable]
    public class IslandClickEvent : UnityEvent<int, IslandData> {}
    public IslandClickEvent OnIslandClicked;
    
    // Internal - Island data
    private Dictionary<int, IslandData> detectedIslands;
    private Dictionary<Color, int> colorToIslandId;
    private Color[] islandColors;
    
    [System.Serializable]
    public class IslandData
    {
        public int id;
        public string name;
        public Vector2 center;
        public Vector2 bounds; // רוחב וגובה
        public int pixelCount;
        public Color islandColor;
        public List<Vector2> boundary;
        
        // מידע משחק
        public string description;
        public bool isVisited;
        public List<string> resources;
        public int population;
        
        public IslandData(int id)
        {
            this.id = id;
            this.name = $"Island {id}";
            this.boundary = new List<Vector2>();
            this.resources = new List<string>();
            this.description = "A mysterious island waiting to be explored...";
        }
    }
    
    // Internal
    private bool[,] navigableGrid;
    private Vector3 lastTouchPosition;
    private bool isDragging = false;
    private Camera cam;
    private float raftMovementStartTime;
    
    // Map bounds
    public Vector2 mapSize { get; private set; }
    private Vector2 mapBounds;
    
    void Start()
    {
        cam = mapCamera != null ? mapCamera : Camera.main;
        SetupMap();
        CreateNavigationFromPNG();
        SetupInitialCameraView();
    }
    
    void SetupMap()
    {
        if (originalMap != null && mapRenderer != null)
        {
            Sprite mapSprite = Sprite.Create(originalMap, 
                new Rect(0, 0, originalMap.width, originalMap.height), 
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            mapRenderer.sprite = mapSprite;
            
            mapSize = new Vector2(originalMap.width / pixelsPerUnit, originalMap.height / pixelsPerUnit);
            mapBounds = mapSize * 0.45f;
            
            Debug.Log($"Map setup complete: {mapSize.x} x {mapSize.y} world units");
        }
    }
    
    void CreateNavigationFromPNG()
    {
        if (navigationMask == null)
        {
            Debug.LogError("Navigation Mask PNG is missing! Please assign a PNG with transparency.");
            CreateDefaultNavigation();
            return;
        }
        
        Debug.Log($"Loading navigation from PNG: {navigationMask.width}x{navigationMask.height}");
        Debug.Log($"PNG Format: {navigationMask.format}");
        Debug.Log($"PNG isReadable: {navigationMask.isReadable}");
        
        // ודא שהטקסטורה קריאה
        if (!navigationMask.isReadable)
        {
            Debug.LogError("Navigation mask must be readable! Fix: Select PNG → Inspector → Read/Write Enabled ✓ → Apply");
            CreateDefaultNavigation();
            return;
        }
        
        // אתחול נתוני איים
        detectedIslands = new Dictionary<int, IslandData>();
        colorToIslandId = new Dictionary<Color, int>();
        
        // זיהוי איים
        if (useColorBasedIslands)
        {
            DetectIslandsByColor();
        }
        else if (autoDetectIslands)
        {
            DetectIslandsByPixelClusters();
        }
        
        // יצירת Navigation Grid (כמו קודם)
        CreateNavigationGrid();
        
        Debug.Log($"Total islands detected: {detectedIslands.Count}");
    }
    
    void CreateNavigationGrid()
    {
        // יצירת Grid לניווט
        navigableGrid = new bool[navigationResolution, navigationResolution];
        bool[,] tempGrid = new bool[navigationResolution, navigationResolution];
        
        int blockedCells = 0;
        int totalCells = navigationResolution * navigationResolution;
        
        // שלב 1: קריאה ישירה מה-PNG
        for (int x = 0; x < navigationResolution; x++)
        {
            for (int y = 0; y < navigationResolution; y++)
            {
                // המרה מקואורדינטות Grid לקואורדינטות PNG
                float pngX = (float)x / navigationResolution * navigationMask.width;
                float pngY = (float)y / navigationResolution * navigationMask.height;
                
                // קריאת הפיקסל מה-PNG
                Color pixelColor = navigationMask.GetPixel(Mathf.FloorToInt(pngX), Mathf.FloorToInt(pngY));
                
                // אם האלפא גבוה (לא שקוף) = אי = לא ניתן לניווט
                tempGrid[x, y] = pixelColor.a < 0.5f; // סף שקיפות
            }
        }
        
        // שלב 2: הוספת מרווח מהאיים (אם נדרש)
        for (int x = 0; x < navigationResolution; x++)
        {
            for (int y = 0; y < navigationResolution; y++)
            {
                bool isNavigable = tempGrid[x, y];
                
                // אם התא עצמו חסום, בהחלט לא ניתן לניווט
                if (!isNavigable)
                {
                    navigableGrid[x, y] = false;
                    blockedCells++;
                    continue;
                }
                
                // בדוק אם להוסיף מרווח
                bool needsMargin = false;
                
                if (islandMargin > 0)
                {
                    // ספירת איים בקרבה
                    int nearbyIslands = 0;
                    int checkRadius = islandMargin + 2; // רדיוס בדיקה יותר גדול
                    
                    for (int dx = -checkRadius; dx <= checkRadius && !needsMargin; dx++)
                    {
                        for (int dy = -checkRadius; dy <= checkRadius && !needsMargin; dy++)
                        {
                            int checkX = x + dx;
                            int checkY = y + dy;
                            
                            if (checkX >= 0 && checkX < navigationResolution && 
                                checkY >= 0 && checkY < navigationResolution)
                            {
                                if (!tempGrid[checkX, checkY])
                                {
                                    nearbyIslands++;
                                }
                            }
                        }
                    }
                    
                    // אם זה מרווח חכם, בדוק אם האזור מספיק רחב למרווח
                    if (smartMargin)
                    {
                        // רק אם יש הרבה איים בקרבה ויש מקום למרווח
                        if (nearbyIslands > 5) // סף לאיים קרובים
                        {
                            // בדוק אם יש מספיק מים סביב כדי להצדיק מרווח
                            int waterCount = 0;
                            for (int dx = -islandMargin*2; dx <= islandMargin*2; dx++)
                            {
                                for (int dy = -islandMargin*2; dy <= islandMargin*2; dy++)
                                {
                                    int checkX = x + dx;
                                    int checkY = y + dy;
                                    
                                    if (checkX >= 0 && checkX < navigationResolution && 
                                        checkY >= 0 && checkY < navigationResolution)
                                    {
                                        if (tempGrid[checkX, checkY]) waterCount++;
                                    }
                                }
                            }
                            
                            // רק אם יש מספיק מים בסביבה (ערוץ רחב)
                            int totalChecked = (islandMargin*4+1) * (islandMargin*4+1);
                            if (waterCount < totalChecked * 0.3f) // אם פחות מ-30% מים
                            {
                                needsMargin = true;
                            }
                        }
                    }
                    else
                    {
                        // מרווח רגיל - בדוק קרבה פשוטה
                        for (int dx = -islandMargin; dx <= islandMargin && !needsMargin; dx++)
                        {
                            for (int dy = -islandMargin; dy <= islandMargin && !needsMargin; dy++)
                            {
                                int checkX = x + dx;
                                int checkY = y + dy;
                                
                                if (checkX >= 0 && checkX < navigationResolution && 
                                    checkY >= 0 && checkY < navigationResolution)
                                {
                                    if (!tempGrid[checkX, checkY])
                                    {
                                        needsMargin = true;
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (needsMargin)
                {
                    navigableGrid[x, y] = false;
                    blockedCells++;
                }
                else
                {
                    navigableGrid[x, y] = true;
                }
            }
        }
        
        float blockagePercentage = (float)blockedCells / totalCells * 100f;
        Debug.Log($"Blocked cells: {blockedCells}/{totalCells} ({blockagePercentage:F1}%)");
        
        if (blockagePercentage < 5f)
        {
            Debug.LogWarning("Very few blocked areas detected. Check if PNG has proper alpha channel.");
        }
        else if (blockagePercentage > 80f)
        {
            Debug.LogWarning("Most areas are blocked. Check if PNG transparency is inverted.");
        }
    }
    
    void DetectIslandsByColor()
    {
        Debug.Log("Detecting islands by unique colors...");
        
        Dictionary<Color, List<Vector2Int>> colorPixels = new Dictionary<Color, List<Vector2Int>>();
        
        // סרוק את כל הפיקסלים וקבץ לפי צבע
        for (int x = 0; x < navigationMask.width; x++)
        {
            for (int y = 0; y < navigationMask.height; y++)
            {
                Color pixelColor = navigationMask.GetPixel(x, y);
                
                // רק פיקסלים לא שקופים (איים)
                if (pixelColor.a >= 0.5f)
                {
                    // עיגול הצבע למניעת וריאציות קטנות
                    Color roundedColor = new Color(
                        Mathf.Round(pixelColor.r * 255f) / 255f,
                        Mathf.Round(pixelColor.g * 255f) / 255f,
                        Mathf.Round(pixelColor.b * 255f) / 255f,
                        1f
                    );
                    
                    if (!colorPixels.ContainsKey(roundedColor))
                    {
                        colorPixels[roundedColor] = new List<Vector2Int>();
                    }
                    
                    colorPixels[roundedColor].Add(new Vector2Int(x, y));
                }
            }
        }
        
        Debug.Log($"Found {colorPixels.Count} unique island colors");
        
        // צור נתוני אי לכל צבע
        int islandId = 1;
        foreach (var kvp in colorPixels)
        {
            if (kvp.Value.Count >= minIslandSize)
            {
                Color islandColor = kvp.Key;
                List<Vector2Int> pixels = kvp.Value;
                
                IslandData island = CreateIslandFromPixels(islandId, pixels, islandColor);
                detectedIslands[islandId] = island;
                colorToIslandId[islandColor] = islandId;
                
                Debug.Log($"Island {islandId}: {pixels.Count} pixels, color {islandColor}");
                islandId++;
            }
        }
    }
    
    void DetectIslandsByPixelClusters()
    {
        Debug.Log("Detecting islands by pixel clustering (optimized for many islands)...");
        
        bool[,] visited = new bool[navigationMask.width, navigationMask.height];
        int islandId = 1;
        int totalPixelsProcessed = 0;
        
        // אופטימיזציה: סרוק בקפיצות קטנות כדי למצוא איים מהר יותר
        int scanStep = optimizeForManyIslands ? 3 : 1;
        
        for (int x = 0; x < navigationMask.width; x += scanStep)
        {
            for (int y = 0; y < navigationMask.height; y += scanStep)
            {
                if (!visited[x, y])
                {
                    Color pixelColor = navigationMask.GetPixel(x, y);
                    
                    // אם זה פיקסל של אי (לא שקוף)
                    if (pixelColor.a >= 0.5f)
                    {
                        // הרץ flood fill למצוא את כל הפיקסלים הקשורים
                        List<Vector2Int> clusterPixels = FloodFillIslandOptimized(x, y, visited);
                        totalPixelsProcessed += clusterPixels.Count;
                        
                        if (clusterPixels.Count >= minIslandSize)
                        {
                            IslandData island = CreateIslandFromPixels(islandId, clusterPixels, pixelColor);
                            
                            // שמות ייחודיים אוטומטיים
                            if (generateUniqueNames)
                            {
                                island.name = GenerateIslandName(islandId, island);
                            }
                            
                            detectedIslands[islandId] = island;
                            
                            Debug.Log($"Island {islandId}: \"{island.name}\" - {clusterPixels.Count} pixels");
                            islandId++;
                            
                            // הגבלת מספר איים למניעת המתנה ארוכה
                            if (islandId > 150)
                            {
                                Debug.LogWarning("Reached maximum island limit (150). Consider increasing minIslandSize.");
                                break;
                            }
                        }
                        else if (clusterPixels.Count > 5) // אפילו איים קטנים - רק ללוג
                        {
                            Debug.Log($"Small landmass found: {clusterPixels.Count} pixels (below minimum of {minIslandSize})");
                        }
                    }
                }
            }
            
            // מעדכן progress כל 20%
            if (x % (navigationMask.width / 5) == 0)
            {
                float progress = (float)x / navigationMask.width * 100f;
                Debug.Log($"Island detection progress: {progress:F0}%");
            }
        }
        
        Debug.Log($"Island detection complete: Found {detectedIslands.Count} islands");
        Debug.Log($"Total landmass pixels: {totalPixelsProcessed}");
        Debug.Log($"Average island size: {(detectedIslands.Count > 0 ? totalPixelsProcessed / detectedIslands.Count : 0)} pixels");
    }
    
    List<Vector2Int> FloodFillIslandOptimized(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> pixels = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;
        
        // אופטימיזציה: הגבל את גודל הqueue למניעת memory issues
        int maxQueueSize = 10000;
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            pixels.Add(current);
            
            // בדוק 4 כיוונים (לא אלכסון ליותר טוב יותר)
            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(-1, 0)
            };
            
            foreach (Vector2Int dir in directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                
                if (newX >= 0 && newX < navigationMask.width &&
                    newY >= 0 && newY < navigationMask.height &&
                    !visited[newX, newY])
                {
                    Color neighborColor = navigationMask.GetPixel(newX, newY);
                    
                    // אם השכן גם הוא חלק מאי (לא שקוף)
                    if (neighborColor.a >= 0.5f)
                    {
                        visited[newX, newY] = true;
                        
                        // אופטימיזציה: הגבל גודל איים ענק
                        if (queue.Count < maxQueueSize)
                        {
                            queue.Enqueue(new Vector2Int(newX, newY));
                        }
                    }
                }
            }
            
            // בטיחות: הגבל גודל אי יחיד למניעת lag
            if (pixels.Count > 50000)
            {
                Debug.LogWarning($"Very large island detected ({pixels.Count} pixels), truncating...");
                break;
            }
        }
        
        return pixels;
    }
    
    string GenerateIslandName(int id, IslandData island)
    {
        // מערכים של שמות לקומבינציות צוריות
        string[] prefixes = {
            "Azure", "Coral", "Emerald", "Golden", "Hidden", "Mystic", "Pearl", "Ruby",
            "Sapphire", "Silver", "Sunset", "Sunrise", "Tropical", "Windward", "Crystal",
            "Shadow", "Moonlit", "Starlight", "Thunder", "Whisper", "Ancient", "Lost",
            "Sacred", "Forgotten", "Eternal", "Serene", "Wild", "Jade", "Amber", "Ivory"
        };
        
        string[] suffixes = {
            "Isle", "Island", "Atoll", "Cay", "Key", "Point", "Bay", "Cove", "Haven",
            "Refuge", "Sanctuary", "Paradise", "Oasis", "Retreat", "Shores", "Landing",
            "Harbor", "Port", "Rock", "Peak", "Crown", "Jewel", "Dream", "Spirit",
            "Heart", "Soul", "Echo", "Whisper", "Song", "Dance"
        };
        
        string[] middleWords = {
            "of the", "of", "", "", "", "" // רוב השמות בלי מילת יחס
        };
        
        // בחר רכיבים על בסיס ה-ID כדי שיהיה עקבי
        Random.InitState(id * 1337); // seed קבוע לכל אי
        
        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];
        string middle = middleWords[Random.Range(0, middleWords.Length)];
        
        string name;
        if (!string.IsNullOrEmpty(middle))
        {
            name = $"{prefix} {middle} {suffix}";
        }
        else
        {
            name = $"{prefix} {suffix}";
        }
        
        // אם השם ארוך מדי, קצר אותו
        if (name.Length > 25)
        {
            name = $"{prefix} {suffix}";
        }
        
        return name;
    }
    
    IslandData CreateIslandFromPixels(int id, List<Vector2Int> pixels, Color islandColor)
    {
        IslandData island = new IslandData(id);
        island.islandColor = islandColor;
        island.pixelCount = pixels.Count;
        
        // חשב מרכז ותיחום
        float centerX = 0, centerY = 0;
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        
        foreach (Vector2Int pixel in pixels)
        {
            centerX += pixel.x;
            centerY += pixel.y;
            
            minX = Mathf.Min(minX, pixel.x);
            maxX = Mathf.Max(maxX, pixel.x);
            minY = Mathf.Min(minY, pixel.y);
            maxY = Mathf.Max(maxY, pixel.y);
        }
        
        // המר לקואורדינטות עולם
        Vector2 pngCenter = new Vector2(centerX / pixels.Count, centerY / pixels.Count);
        island.center = PNGToWorldPosition(pngCenter);
        island.bounds = new Vector2(
            (maxX - minX) / pixelsPerUnit,
            (maxY - minY) / pixelsPerUnit
        );
        
        // צור boundary פשוט (ניתן לשפר)
        island.boundary = CreateIslandBoundary(pixels);
        
        // הגדר מידע בררת מחדל
        AssignDefaultIslandInfo(island);
        
        return island;
    }
    
    Vector2 PNGToWorldPosition(Vector2 pngPos)
    {
        Vector2 normalizedPos = new Vector2(
            pngPos.x / navigationMask.width - 0.5f,
            (navigationMask.height - pngPos.y) / navigationMask.height - 0.5f
        );
        
        return new Vector2(
            normalizedPos.x * mapSize.x,
            normalizedPos.y * mapSize.y
        );
    }
    
    List<Vector2> CreateIslandBoundary(List<Vector2Int> pixels)
    {
        // אלגוריתם פשוט למציאת boundary - ניתן לשפר
        HashSet<Vector2Int> pixelSet = new HashSet<Vector2Int>(pixels);
        List<Vector2> boundary = new List<Vector2>();
        
        foreach (Vector2Int pixel in pixels)
        {
            // בדוק אם זה פיקסל boundary (יש לו שכן שקוף)
            bool isBoundary = false;
            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(-1, 0)
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = pixel + dir;
                if (!pixelSet.Contains(neighbor))
                {
                    isBoundary = true;
                    break;
                }
            }
            
            if (isBoundary)
            {
                boundary.Add(PNGToWorldPosition(pixel));
            }
        }
        
        return boundary;
    }
    
    void AssignDefaultIslandInfo(IslandData island)
    {
        // השתמש ב-seed קבוע לכל אי לקבלת תוצאות עקביות
        Random.InitState(island.id * 2021);
        
        // הגדר אוכלוסיה על בסיס גודל האי
        if (island.pixelCount < 50)
        {
            island.population = Random.Range(0, 15); // איים קטנים - מעט תושבים
        }
        else if (island.pixelCount < 200)
        {
            island.population = Random.Range(10, 50); // איים בינוניים
        }
        else if (island.pixelCount < 1000)
        {
            island.population = Random.Range(40, 150); // איים גדולים
        }
        else
        {
            island.population = Random.Range(100, 500); // איים ענקיים
        }
        
        // משאבים מגוונים יותר
        string[] commonResources = {
            "Fresh Water", "Fish", "Coconuts", "Tropical Fruits", "Wood"
        };
        
        string[] rareResources = {
            "Pearls", "Gold", "Medicinal Plants", "Exotic Spices", "Precious Stones",
            "Ancient Artifacts", "Crystal Cave", "Hot Springs", "Sacred Groves"
        };
        
        string[] uniqueResources = {
            "Dragon Egg Fragment", "Moonstone Deposits", "Singing Crystals",
            "Time Flowers", "Memory Stones", "Phantom Ore", "Starlight Essence"
        };
        
        // כל אי מקבל 2-3 משאבים רגילים
        int commonCount = Random.Range(2, 4);
        for (int i = 0; i < commonCount; i++)
        {
            string resource = commonResources[Random.Range(0, commonResources.Length)];
            if (!island.resources.Contains(resource))
            {
                island.resources.Add(resource);
            }
        }
        
        // 30% סיכוי למשאב נדיר
        if (Random.Range(0f, 1f) < 0.3f)
        {
            string rareResource = rareResources[Random.Range(0, rareResources.Length)];
            island.resources.Add(rareResource);
        }
        
        // 5% סיכוי למשאב ייחודי (רק לאיים גדולים)
        if (island.pixelCount > 100 && Random.Range(0f, 1f) < 0.05f)
        {
            string uniqueResource = uniqueResources[Random.Range(0, uniqueResources.Length)];
            island.resources.Add(uniqueResource);
        }
        
        // תיאורים מגוונים לפי גודל ומשאבים
        string[] baseDescriptions = {
            "A peaceful island surrounded by crystal-clear waters",
            "An ancient landmass with mysterious origins",
            "A lush tropical paradise teeming with life",
            "A rugged island shaped by ocean winds",
            "A hidden gem in the vast ocean",
            "A serene retreat from the bustling world",
            "An island where time seems to stand still",
            "A mystical place shrouded in legend"
        };
        
        string baseDescription = baseDescriptions[Random.Range(0, baseDescriptions.Length)];
        
        // הוסף פרטים על בסיס האוכלוסיה והמשאבים
        string populationDesc = "";
        if (island.population == 0)
        {
            populationDesc = " This uninhabited island offers solitude and mystery.";
        }
        else if (island.population < 20)
        {
            populationDesc = $" A small community of {island.population} islanders calls this place home.";
        }
        else if (island.population < 100)
        {
            populationDesc = $" Home to {island.population} friendly inhabitants who live in harmony with nature.";
        }
        else
        {
            populationDesc = $" A thriving community of {island.population} people has built a prosperous settlement here.";
        }
        
        string resourceDesc = "";
        if (island.resources.Contains("Gold") || island.resources.Contains("Precious Stones"))
        {
            resourceDesc = " Rumors speak of hidden treasures waiting to be discovered.";
        }
        else if (island.resources.Contains("Ancient Artifacts"))
        {
            resourceDesc = " Ancient ruins hint at a civilization lost to time.";
        }
        else if (island.resources.Count > 4)
        {
            resourceDesc = " This bountiful island is rich in natural resources.";
        }
        
        island.description = baseDescription + populationDesc + resourceDesc;
        
        // מגבלת אורך התיאור
        if (island.description.Length > 200)
        {
            island.description = island.description.Substring(0, 197) + "...";
        }
    }
    
    void CreateDefaultNavigation()
    {
        Debug.LogWarning("Creating default navigation - all areas navigable");
        navigableGrid = new bool[navigationResolution, navigationResolution];
        
        for (int x = 0; x < navigationResolution; x++)
        {
            for (int y = 0; y < navigationResolution; y++)
            {
                navigableGrid[x, y] = true;
            }
        }
    }
    
    void SetupInitialCameraView()
    {
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
            cam.orthographicSize = initialZoom;
            
            Debug.Log($"Camera setup: position {cam.transform.position}, zoom {cam.orthographicSize}");
            Debug.Log($"Map world size: {mapSize.x} x {mapSize.y} units");
        }
    }
    
    void Update()
    {
        HandleInput();
        
        if (followRaft && raftController != null && raftController.IsMoving())
        {
            if (raftMovementStartTime == 0)
            {
                raftMovementStartTime = Time.time;
            }
            
            if (Time.time - raftMovementStartTime > followStartDelay)
            {
                FollowRaft();
            }
        }
        else
        {
            raftMovementStartTime = 0;
        }
    }
    
    void FollowRaft()
    {
        Vector3 targetPos = raftController.transform.position;
        targetPos.z = cam.transform.position.z;
        
        cam.transform.position = Vector3.Lerp(
            cam.transform.position, 
            targetPos, 
            cameraFollowSpeed * Time.deltaTime
        );
        
        ClampCameraToMapBounds();
    }
    
    void HandleInput()
    {
        #if UNITY_EDITOR
        HandleMouseInput();
        #else
        HandleTouchInput();
        #endif
    }
    
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastTouchPosition = Input.mousePosition;
            isDragging = false;
        }
        
        if (Input.GetMouseButton(0))
        {
            Vector3 deltaPosition = Input.mousePosition - lastTouchPosition;
            
            // בדירה חלקה יותר - סף נמוך יותר ותנועה מיידית
            if (deltaPosition.magnitude > 3f || isDragging)
            {
                isDragging = true;
                DragMap(deltaPosition);
            }
            
            lastTouchPosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButtonUp(0) && !isDragging)
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            HandleMapClick(worldPos);
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ZoomCamera(scroll > 0 ? 1 : -1);
        }
    }
    
    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 deltaPosition = (Vector3)touch.position - lastTouchPosition;
                
                // בדירה חלקה יותר במובייל
                if (deltaPosition.magnitude > 5f || isDragging)
                {
                    isDragging = true;
                    DragMap(deltaPosition);
                }
                
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended && !isDragging)
            {
                Vector2 worldPos = cam.ScreenToWorldPoint(touch.position);
                HandleMapClick(worldPos);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float prevDistance = Vector2.Distance(
                touch1.position - touch1.deltaPosition,
                touch2.position - touch2.deltaPosition);
            
            float deltaDistance = currentDistance - prevDistance;
            ZoomCamera(deltaDistance > 0 ? 1 : -1);
        }
    }
    
    void DragMap(Vector3 deltaPosition)
    {
        raftMovementStartTime = 0;
        
        Vector3 worldDelta = cam.ScreenToWorldPoint(deltaPosition) - cam.ScreenToWorldPoint(Vector3.zero);
        worldDelta *= dragSensitivity;
        
        Vector3 newPosition = cam.transform.position - worldDelta;
        
        float maxX = mapBounds.x - cam.orthographicSize * cam.aspect;
        float maxY = mapBounds.y - cam.orthographicSize;
        
        maxX = Mathf.Max(0, maxX);
        maxY = Mathf.Max(0, maxY);
        
        newPosition.x = Mathf.Clamp(newPosition.x, -maxX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, -maxY, maxY);
        newPosition.z = cam.transform.position.z;
        
        cam.transform.position = newPosition;
    }
    
    void ZoomCamera(int direction)
    {
        float zoomChange = zoomSensitivity * direction;
        zoomChange *= (cam.orthographicSize / initialZoom);
        
        float newSize = cam.orthographicSize - zoomChange;
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        
        ClampCameraToMapBounds();
    }
    
    void ClampCameraToMapBounds()
    {
        Vector3 pos = cam.transform.position;
        
        float maxX = mapBounds.x - cam.orthographicSize * cam.aspect;
        float maxY = mapBounds.y - cam.orthographicSize;
        
        maxX = Mathf.Max(0, maxX);
        maxY = Mathf.Max(0, maxY);
        
        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        
        cam.transform.position = pos;
    }
    
    // ✅ הפונקציה המתוקנת עם פתרון לשתי הבעיות
    void HandleMapClick(Vector2 worldPosition)
    {
        // ✅ תיקון בעיה 1: בדוק אם לחצו על UI לפני שמטפלים בלחיצה על המפה
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Clicked on UI element - ignoring map click");
            return; // אל תטפל בלחיצה אם זה על UI
        }
        
        // המשך הקוד הרגיל...
        if (IsPositionNavigable(worldPosition))
        {
            Debug.Log($"Moving to water position: {worldPosition}");
            if (raftController != null)
            {
                raftController.MoveToPosition(worldPosition);
            }
        }
        else
        {
            // נסה לזהות על איזה אי לחצו
            IslandData clickedIsland = GetIslandAtPosition(worldPosition);
            
            if (clickedIsland != null)
            {
                Debug.Log($"Clicked on {clickedIsland.name} (ID: {clickedIsland.id})");
                OnIslandClicked?.Invoke(clickedIsland.id, clickedIsland);
            }
            else
            {
                // ✅ תיקון בעיה 2: שיפור זיהוי איים גדולים
                // בדוק אם זה באמת אי או רק מרווח מהאיים
                Vector2Int gridPos = WorldToGrid(worldPosition);
                if (IsValidGridPosition(gridPos))
                {
                    Vector2 pngPos = WorldToPNGPosition(worldPosition);
                    
                    // וודא שאנחנו בתחום ה-PNG
                    if (pngPos.x >= 0 && pngPos.x < navigationMask.width &&
                        pngPos.y >= 0 && pngPos.y < navigationMask.height)
                    {
                        Color pixelColor = navigationMask.GetPixel(
                            Mathf.FloorToInt(pngPos.x), 
                            Mathf.FloorToInt(pngPos.y)
                        );
                        
                        if (pixelColor.a >= 0.5f)
                        {
                            // זה אי אמיתי! חפש את הקרוב ביותר ברדיוס יותר גדול
                            IslandData nearestIsland = FindNearestIslandInRadius(worldPosition, 5f);
                            
                            if (nearestIsland != null)
                            {
                                Debug.Log($"Found nearby island: {nearestIsland.name} (ID: {nearestIsland.id})");
                                OnIslandClicked?.Invoke(nearestIsland.id, nearestIsland);
                            }
                            else
                            {
                                Debug.Log($"Clicked on unidentified island at: {worldPosition}");
                                Debug.Log($"Pixel color: {pixelColor}");
                                OnIslandClicked?.Invoke(-1, null); // אי לא מזוהה
                            }
                        }
                        else
                        {
                            Debug.Log($"Clicked too close to island (margin zone): {worldPosition}");
                            Debug.Log("Try clicking further from islands or reduce Island Margin in settings");
                        }
                    }
                    else
                    {
                        Debug.Log($"Clicked outside PNG bounds: {worldPosition}");
                    }
                }
                else
                {
                    Debug.Log($"Clicked outside map bounds: {worldPosition}");
                }
            }
        }
    }
    
    // ✅ פונקציה חדשה לשיפור זיהוי איים גדולים
    IslandData FindNearestIslandInRadius(Vector2 worldPosition, float maxRadius)
    {
        IslandData nearestIsland = null;
        float minDistance = float.MaxValue;
        
        if (detectedIslands == null) return null;
        
        foreach (var island in detectedIslands.Values)
        {
            float distance = Vector2.Distance(worldPosition, island.center);
            
            // חשב רדיוס דינמי על בסיס גודל האי
            float islandRadius = Mathf.Max(island.bounds.x, island.bounds.y) * 0.6f; // 60% מהגודל
            islandRadius = Mathf.Max(islandRadius, 2f); // מינימום 2 יחידות
            islandRadius = Mathf.Min(islandRadius, maxRadius); // מקסימום הרדיוס שהוגדר
            
            if (distance <= islandRadius && distance < minDistance)
            {
                minDistance = distance;
                nearestIsland = island;
            }
        }
        
        if (nearestIsland != null)
        {
            Debug.Log($"Found island {nearestIsland.name} at distance {minDistance:F2} within radius {Mathf.Max(nearestIsland.bounds.x, nearestIsland.bounds.y) * 0.6f:F2}");
        }
        
        return nearestIsland;
    }
    
    IslandData GetIslandAtPosition(Vector2 worldPosition)
    {
        Vector2 pngPos = WorldToPNGPosition(worldPosition);
        
        if (pngPos.x < 0 || pngPos.x >= navigationMask.width ||
            pngPos.y < 0 || pngPos.y >= navigationMask.height)
        {
            return null;
        }
        
        Color pixelColor = navigationMask.GetPixel(
            Mathf.FloorToInt(pngPos.x), 
            Mathf.FloorToInt(pngPos.y)
        );
        
        // אם זה אי (לא שקוף)
        if (pixelColor.a >= 0.5f)
        {
            // אם משתמשים בזיהוי לפי צבע
            if (useColorBasedIslands && colorToIslandId != null)
            {
                Color roundedColor = new Color(
                    Mathf.Round(pixelColor.r * 255f) / 255f,
                    Mathf.Round(pixelColor.g * 255f) / 255f,
                    Mathf.Round(pixelColor.b * 255f) / 255f,
                    1f
                );
                
                if (colorToIslandId.ContainsKey(roundedColor))
                {
                    int islandId = colorToIslandId[roundedColor];
                    return detectedIslands.ContainsKey(islandId) ? detectedIslands[islandId] : null;
                }
            }
            
            // אחרת, חפש לפי מיקום (איטי יותר)
            foreach (var island in detectedIslands.Values)
            {
                if (Vector2.Distance(worldPosition, island.center) <= Mathf.Max(island.bounds.x, island.bounds.y))
                {
                    return island;
                }
            }
        }
        
        return null;
    }
    
    Vector2 WorldToPNGPosition(Vector2 worldPos)
    {
        Vector2 normalizedPos = new Vector2(
            (worldPos.x / mapSize.x) + 0.5f,
            (-worldPos.y / mapSize.y) + 0.5f
        );
        
        return new Vector2(
            normalizedPos.x * navigationMask.width,
            normalizedPos.y * navigationMask.height
        );
    }
    
    public bool IsPositionNavigable(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return IsValidGridPosition(gridPos) && navigableGrid[gridPos.x, gridPos.y];
    }
    
    public List<Vector2> FindWaterPath(Vector2 start, Vector2 end)
    {
        Vector2Int startGrid = WorldToGrid(start);
        Vector2Int endGrid = WorldToGrid(end);
        
        if (!IsPositionNavigable(start) || !IsPositionNavigable(end))
            return null;
        
        return AStar(startGrid, endGrid);
    }
    
    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 normalizedPos = new Vector2(
            (worldPos.x + mapSize.x * 0.5f) / mapSize.x,
            (worldPos.y + mapSize.y * 0.5f) / mapSize.y
        );
        
        normalizedPos.x = Mathf.Clamp01(normalizedPos.x);
        normalizedPos.y = Mathf.Clamp01(normalizedPos.y);
        
        return new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt(normalizedPos.x * navigationResolution), 0, navigationResolution - 1),
            Mathf.Clamp(Mathf.FloorToInt(normalizedPos.y * navigationResolution), 0, navigationResolution - 1)
        );
    }
    
    Vector2 GridToWorld(Vector2Int gridPos)
    {
        Vector2 normalizedPos = new Vector2(
            (float)gridPos.x / navigationResolution,
            (float)gridPos.y / navigationResolution
        );
        
        return new Vector2(
            (normalizedPos.x - 0.5f) * mapSize.x,
            (normalizedPos.y - 0.5f) * mapSize.y
        );
    }
    
    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < navigationResolution && 
               pos.y >= 0 && pos.y < navigationResolution;
    }
    
    List<Vector2> AStar(Vector2Int start, Vector2Int end)
    {
        // בדיקה מהירה - אם יש קו ישר פנוי
        if (IsDirectPathClear(GridToWorld(start), GridToWorld(end)))
        {
            List<Vector2> simplePath = new List<Vector2>();
            simplePath.Add(GridToWorld(start));
            simplePath.Add(GridToWorld(end));
            return simplePath;
        }
        
        // מימוש A* מלא
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        
        gScore[start] = 0;
        fScore[start] = GetDistance(start, end);
        openSet.Add(start);
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };
        
        int maxIterations = 1000;
        int iterations = 0;
        
        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            Vector2Int current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (GetFScore(openSet[i], fScore) < GetFScore(current, fScore))
                {
                    current = openSet[i];
                }
            }
            
            if (current.x == end.x && current.y == end.y)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;
                
                if (!IsValidGridPosition(neighbor) || 
                    !navigableGrid[neighbor.x, neighbor.y] || 
                    closedSet.Contains(neighbor))
                {
                    continue;
                }
                
                float tentativeGScore = GetGScore(current, gScore) + GetDistance(current, neighbor);
                
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= GetGScore(neighbor, gScore))
                {
                    continue;
                }
                
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + GetDistance(neighbor, end);
            }
        }
        
        Debug.LogWarning($"A* failed to find path after {iterations} iterations");
        return null;
    }
    
    float GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx + dy + (1.414f - 2) * Mathf.Min(dx, dy);
    }
    
    float GetGScore(Vector2Int pos, Dictionary<Vector2Int, float> gScore)
    {
        return gScore.ContainsKey(pos) ? gScore[pos] : float.MaxValue;
    }
    
    float GetFScore(Vector2Int pos, Dictionary<Vector2Int, float> fScore)
    {
        return fScore.ContainsKey(pos) ? fScore[pos] : float.MaxValue;
    }
    
    List<Vector2> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> pathGrid = new List<Vector2Int>();
        pathGrid.Add(current);
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            pathGrid.Add(current);
        }
        
        pathGrid.Reverse();
        
        List<Vector2> worldPath = new List<Vector2>();
        for (int i = 0; i < pathGrid.Count; i++)
        {
            worldPath.Add(GridToWorld(pathGrid[i]));
        }
        
        return SimplifyPath(worldPath);
    }
    
    List<Vector2> SimplifyPath(List<Vector2> path)
    {
        if (path.Count <= 2) return path;
        
        List<Vector2> simplified = new List<Vector2>();
        simplified.Add(path[0]);
        
        // שלב 1: פישוט קווי בסיסי
        for (int i = 1; i < path.Count - 1; i++)
        {
            if (!IsDirectPathClear(simplified[simplified.Count - 1], path[i + 1]))
            {
                simplified.Add(path[i]);
            }
        }
        
        simplified.Add(path[path.Count - 1]);
        
        Debug.Log($"Simplified path from {path.Count} to {simplified.Count} points");
        
        // שלב 2: החלקה עם Catmull-Rom spline
        if (smoothPaths && simplified.Count > 2)
        {
            return SmoothPathWithSpline(simplified);
        }
        
        return simplified;
    }
    
    List<Vector2> SmoothPathWithSpline(List<Vector2> originalPath)
    {
        if (originalPath.Count < 3) return originalPath;
        
        List<Vector2> smoothPath = new List<Vector2>();
        smoothPath.Add(originalPath[0]); // נקודת התחלה
        
        for (int i = 0; i < originalPath.Count - 1; i++)
        {
            Vector2 p0 = i > 0 ? originalPath[i - 1] : originalPath[i];
            Vector2 p1 = originalPath[i];
            Vector2 p2 = originalPath[i + 1];
            Vector2 p3 = i + 2 < originalPath.Count ? originalPath[i + 2] : originalPath[i + 1];
            
            // יצירת נקודות ביניים עם Catmull-Rom
            for (int step = 1; step <= pathSmoothingSteps; step++)
            {
                float t = (float)step / pathSmoothingSteps;
                Vector2 smoothPoint = CatmullRomInterpolate(p0, p1, p2, p3, t);
                
                // ודא שהנקודה המחוכמת עדיין ניתנת לניווט
                if (IsPositionNavigable(smoothPoint))
                {
                    smoothPath.Add(smoothPoint);
                }
                else
                {
                    // אם הנקודה המחוכמת חסומה, חזור לנקודה המקורית
                    smoothPath.Add(Vector2.Lerp(p1, p2, t));
                }
            }
        }
        
        Debug.Log($"Smoothed path to {smoothPath.Count} points");
        return smoothPath;
    }
    
    Vector2 CatmullRomInterpolate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        
        float x = 0.5f * (
            (2f * p1.x) +
            (-p0.x + p2.x) * t +
            (2f * p0.x - 5f * p1.x + 4f * p2.x - p3.x) * t2 +
            (-p0.x + 3f * p1.x - 3f * p2.x + p3.x) * t3
        );
        
        float y = 0.5f * (
            (2f * p1.y) +
            (-p0.y + p2.y) * t +
            (2f * p0.y - 5f * p1.y + 4f * p2.y - p3.y) * t2 +
            (-p0.y + 3f * p1.y - 3f * p2.y + p3.y) * t3
        );
        
        return new Vector2(x, y);
    }
    
    bool IsDirectPathClear(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.RoundToInt(distance * 5);
        steps = Mathf.Max(steps, 10);
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 checkPoint = Vector2.Lerp(start, end, t);
            
            if (!IsPositionNavigable(checkPoint))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // פונקציות ציבוריות לעבודה עם איים
    public IslandData GetIslandById(int id)
    {
        return detectedIslands != null && detectedIslands.ContainsKey(id) ? detectedIslands[id] : null;
    }
    
    public IslandData GetNearestIsland(Vector2 worldPosition)
    {
        if (detectedIslands == null) return null;
        
        IslandData nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var island in detectedIslands.Values)
        {
            float distance = Vector2.Distance(worldPosition, island.center);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = island;
            }
        }
        
        return nearest;
    }
    
    public List<IslandData> GetAllIslands()
    {
        return detectedIslands != null ? new List<IslandData>(detectedIslands.Values) : new List<IslandData>();
    }
    
    public void MarkIslandAsVisited(int islandId)
    {
        if (detectedIslands != null && detectedIslands.ContainsKey(islandId))
        {
            detectedIslands[islandId].isVisited = true;
            Debug.Log($"Island {detectedIslands[islandId].name} marked as visited!");
        }
    }
    
    public int GetVisitedIslandsCount()
    {
        if (detectedIslands == null) return 0;
        
        int count = 0;
        foreach (var island in detectedIslands.Values)
        {
            if (island.isVisited) count++;
        }
        return count;
    }
    
    // פונקציות היבוג
    [ContextMenu("Test Navigation at Position")]
    void TestNavigationAtPosition()
    {
        Vector2 testPos = Vector2.zero;
        bool navigable = IsPositionNavigable(testPos);
        Debug.Log($"Position {testPos} - Navigable: {navigable}");
        
        Vector2Int gridPos = WorldToGrid(testPos);
        Debug.Log($"Grid position: {gridPos}");
        
        if (navigationMask != null)
        {
            float pngX = (float)gridPos.x / navigationResolution * navigationMask.width;
            float pngY = (float)gridPos.y / navigationResolution * navigationMask.height;
            Color pixelColor = navigationMask.GetPixel(Mathf.FloorToInt(pngX), Mathf.FloorToInt(pngY));
            Debug.Log($"PNG pixel at ({pngX}, {pngY}): {pixelColor}");
        }
    }
    
    [ContextMenu("Debug Navigation Grid")]
    void DebugNavigationGrid()
    {
        if (navigableGrid == null)
        {
            Debug.Log("Navigation grid not initialized");
            return;
        }
        
        int navigableCells = 0;
        int totalCells = navigationResolution * navigationResolution;
        
        for (int x = 0; x < navigationResolution; x++)
        {
            for (int y = 0; y < navigationResolution; y++)
            {
                if (navigableGrid[x, y]) navigableCells++;
            }
        }
        
        float navigablePercentage = (float)navigableCells / totalCells * 100f;
        Debug.Log($"Navigation Grid: {navigableCells}/{totalCells} navigable ({navigablePercentage:F1}%)");
        Debug.Log($"Island margin: {islandMargin} grid cells (Smart: {(smartMargin ? "ON" : "OFF")})");
        Debug.Log($"Path smoothing: {(smoothPaths ? "Enabled" : "Disabled")} ({pathSmoothingSteps} steps)");
        
        if (navigablePercentage < 20f)
        {
            Debug.LogWarning("Very low navigable area! Try: Island Margin = 0 or Smart Margin = OFF");
        }
    }
    
    [ContextMenu("Debug Detected Islands")]
    void DebugDetectedIslands()
    {
        if (detectedIslands == null || detectedIslands.Count == 0)
        {
            Debug.Log("No islands detected. Run island detection first.");
            return;
        }
        
        Debug.Log("=== DETECTED ISLANDS ===");
        foreach (var island in detectedIslands.Values)
        {
            Debug.Log($"🏝️ {island.name} (ID: {island.id})");
            Debug.Log($"   Center: {island.center}");
            Debug.Log($"   Size: {island.bounds.x:F1} x {island.bounds.y:F1} units");
            Debug.Log($"   Pixels: {island.pixelCount}");
            Debug.Log($"   Color: {island.islandColor}");
            Debug.Log($"   Population: {island.population}");
            Debug.Log($"   Resources: {string.Join(", ", island.resources)}");
            Debug.Log($"   Description: {island.description}");
            Debug.Log("---");
        }
    }
    
    [ContextMenu("Regenerate Island Detection")]
    void RegenerateIslandDetection()
    {
        if (navigationMask == null)
        {
            Debug.LogError("No navigation mask assigned!");
            return;
        }
        
        Debug.Log("Regenerating island detection...");
        
        // נקה נתונים קיימים
        detectedIslands?.Clear();
        colorToIslandId?.Clear();
        
        // הרץ זיהוי מחדש
        if (useColorBasedIslands)
        {
            DetectIslandsByColor();
        }
        else if (autoDetectIslands)
        {
            DetectIslandsByPixelClusters();
        }
        
        Debug.Log($"Island detection complete. Found {detectedIslands.Count} islands.");
    }
    
    [ContextMenu("Reset Default Settings")]
    void ResetDefaultSettings()
    {
        islandMargin = 1;
        smartMargin = true;
        smoothPaths = true;
        pathSmoothingSteps = 8;
        Debug.Log("Settings reset to defaults. Recreating navigation...");
        CreateNavigationFromPNG();
        Debug.Log("Navigation recreated with default settings.");
    }
    
    [ContextMenu("Disable Island Margins")]
    void DisableIslandMargins()
    {
        islandMargin = 0;
        Debug.Log("Island margins disabled. Recreating navigation...");
        CreateNavigationFromPNG();
        Debug.Log("Navigation recreated without margins.");
    }
    
    [ContextMenu("Test Path Smoothing")]
    void TestPathSmoothing()
    {
        // יצירת מסלול דוגמה לבדיקה
        List<Vector2> testPath = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(10, 5),
            new Vector2(20, 0),
            new Vector2(30, -10),
            new Vector2(40, 0)
        };
        
        Debug.Log("Original path points:");
        for (int i = 0; i < testPath.Count; i++)
        {
            Debug.Log($"  Point {i}: {testPath[i]}");
        }
        
        List<Vector2> smoothed = SmoothPathWithSpline(testPath);
        
        Debug.Log($"Smoothed path ({smoothed.Count} points):");
        for (int i = 0; i < Mathf.Min(smoothed.Count, 10); i++)
        {
            Debug.Log($"  Point {i}: {smoothed[i]}");
        }
        
        if (smoothed.Count > 10)
        {
            Debug.Log($"... and {smoothed.Count - 10} more points");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // צייר grid דוגמה (כל 10 תאים)
        if (navigableGrid != null)
        {
            int step = Mathf.Max(1, navigationResolution / 20); // מקסימום 20x20 gizmos
            
            for (int x = 0; x < navigationResolution; x += step)
            {
                for (int y = 0; y < navigationResolution; y += step)
                {
                    Vector2 worldPos = GridToWorld(new Vector2Int(x, y));
                    
                    if (navigableGrid[x, y])
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(worldPos, Vector3.one * 2f);
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(worldPos, Vector3.one * 1.5f);
                    }
                }
            }
        }
        
        // הצגת מרווח האיים
        if (islandMargin > 0)
        {
            Gizmos.color = Color.yellow;
            float marginWorldSize = (float)islandMargin / navigationResolution * mapSize.x;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(marginWorldSize * 2, marginWorldSize * 2, 0));
        }
    }
#endif
}