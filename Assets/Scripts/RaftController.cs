using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaftController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Animation")]
    [SerializeField] private Animator raftAnimator;
    
    [Header("Path Visualization")]
    [SerializeField] private Color pathColor = Color.cyan;
    [SerializeField] [Range(0.01f, 5.0f)] private float pathWidth = 0.5f; // ✅ עובי הקו - ניתן לעריכה באינספקטור
    [SerializeField] private int pathSortingOrder = 5;
    
    [Header("Path End Circles")]
    [SerializeField] private float circleSizeMultiplier = 4f; // ✅ עיגולים - פי כמה מעובי הקו
    
    // Private variables
    private LineRenderer pathRenderer;
    private GameObject startCircle;
    private GameObject endCircle;
    private Material pathMaterial;
    private Queue<Vector2> currentPath;
    private bool isMoving = false;
    private MapController mapController;
    private Vector2 targetPosition;
    
    void Start()
    {
        Debug.Log("🚀 RaftController starting with ADJUSTABLE line...");
        
        mapController = FindObjectOfType<MapController>();
        
        // ✅ אל תכפה פרמטרים - השתמש בערכי האינספקטור
        pathColor = Color.cyan; // רק צבע
        // pathWidth נשאר כמו שהמשתמש הגדיר באינספקטור
        Debug.Log($"📏 Using Inspector pathWidth: {pathWidth}");
        
        SafeSetupEverything();
        
        Debug.Log("✅ RaftController ready with adjustable line!");
    }
    
    void SafeSetupEverything()
    {
        Debug.Log("🔧 Setting up adjustable dashed line...");
        
        try
        {
            CleanupOldStuff();
            CreateSafeLineRenderer();
            CreateSafeCircles();
            
            Debug.Log("✅ Adjustable dashed line setup completed!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Setup failed: {e.Message}");
        }
    }
    
    void CleanupOldStuff()
    {
        Debug.Log("🧹 Cleaning up old objects...");
        
        Transform oldRaftPath = transform.Find("RaftPath");
        if (oldRaftPath != null)
        {
            Debug.Log("🗑️ Destroying old RaftPath");
            DestroyImmediate(oldRaftPath.gameObject);
        }
        
        LineRenderer oldLR = GetComponent<LineRenderer>();
        if (oldLR != null)
        {
            Debug.Log("🗑️ Destroying old LineRenderer");
            DestroyImmediate(oldLR);
        }
        
        if (startCircle != null)
        {
            DestroyImmediate(startCircle);
            startCircle = null;
        }
        
        if (endCircle != null)
        {
            DestroyImmediate(endCircle);
            endCircle = null;
        }
        
        Debug.Log("✅ Cleanup completed");
    }
    
    void CreateSafeLineRenderer()
    {
        Debug.Log("📏 Creating ADJUSTABLE LineRenderer...");
        
        try
        {
            pathRenderer = gameObject.AddComponent<LineRenderer>();
            
            if (pathRenderer == null)
            {
                Debug.LogError("❌ Failed to create LineRenderer!");
                return;
            }
            
            // ✅ צור material מקוקו מתכוונן
            pathMaterial = CreateDashedMaterial();
            
            // ✅ הגדרות עם קו מתכוונן
            pathRenderer.material = pathMaterial;
            pathRenderer.startColor = pathColor;
            pathRenderer.endColor = pathColor;
            
            // ✅ קו בעובי שהמשתמש בחר באינספקטור
            pathRenderer.startWidth = pathWidth;
            pathRenderer.endWidth = pathWidth;
            
            // ✅ הגדרות חשובות לטקסטורה מקוקו
            pathRenderer.textureMode = LineTextureMode.Tile; // חוזר על הטקסטורה
            pathRenderer.alignment = LineAlignment.TransformZ; // יישור עם ה-transform
            pathRenderer.useWorldSpace = true;
            
            // ✅ הגדרות נוספות לחדות
            pathRenderer.numCornerVertices = 0; // פינות חדות
            pathRenderer.numCapVertices = 0; // קצוות חדים
            
            pathRenderer.sortingLayerName = "Default";
            pathRenderer.sortingOrder = pathSortingOrder;
            
            Debug.Log($"📏 LineRenderer width set to: {pathWidth} (from Inspector)");
            
            pathRenderer.positionCount = 0;
            pathRenderer.enabled = false;
            
            Debug.Log($"✅ ADJUSTABLE LineRenderer created: Width={pathRenderer.startWidth}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ LineRenderer creation failed: {e.Message}");
        }
    }
    
    Material CreateDashedMaterial()
    {
        try
        {
            // ✅ Sprites/Default עובד עם צבעים
            Material material = new Material(Shader.Find("Sprites/Default"));
            
            if (material == null)
            {
                Debug.LogError("❌ No shader found!");
                return null;
            }
            
            // ✅ טקסטורה מקוקו פשוטה
            material.mainTexture = CreatePerfectDashTexture();
            material.color = pathColor; // צבע מהאינספקטור
            
            Debug.Log($"✅ Dashed material created with shader: {material.shader.name}, color: {pathColor}");
            return material;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Material creation failed: {e.Message}");
            return null;
        }
    }
    
    Texture2D CreatePerfectDashTexture()
    {
        try
        {
            // ✅ טקסטורה מקוקו קטנה יותר לביצועים טובים יותר
            int width = 40; // קטן יותר = מקוקו צפוף יותר
            int height = 8;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            Color[] colors = new Color[width * height];
            
            // ✅ מקוקו: 30 פיקסלים לבנים (75%), 10 שקופים (25%)
            for (int x = 0; x < width; x++)
            {
                // 75% נראה, 25% שקוף לניגודיות טובה יותר
                Color pixelColor = (x < 30) ? Color.white : Color.clear;
                
                for (int y = 0; y < height; y++)
                {
                    colors[y * width + x] = pixelColor;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point; // חד ללא טשטוש
            
            Debug.Log($"✅ Dash texture: {width}x{height}, 30px visible (75%), 10px transparent (25%)");
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Dash texture creation failed: {e.Message}");
            return null;
        }
    }
    
    void CreateSafeCircles()
    {
        Debug.Log("⭕ Creating adaptive circles...");
        
        try
        {
            if (startCircle == null)
            {
                startCircle = CreateSafeCircle("StartCircle");
            }
            
            if (endCircle == null)
            {
                endCircle = CreateSafeCircle("EndCircle");
            }
            
            HidePathCircles();
            
            Debug.Log("✅ Adaptive circles created");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Circle creation failed: {e.Message}");
        }
    }
    
    GameObject CreateSafeCircle(string name)
    {
        try
        {
            GameObject circle = new GameObject(name);
            SpriteRenderer spriteRenderer = circle.AddComponent<SpriteRenderer>();
            
            // ✅ צור sprite עיגול
            Sprite circleSprite = CreateCircleSprite();
            if (circleSprite != null)
            {
                spriteRenderer.sprite = circleSprite;
                Debug.Log($"✅ Circle sprite assigned to {name}");
            }
            else
            {
                Debug.LogError($"❌ Failed to create circle sprite for {name}");
                return null;
            }
            
            spriteRenderer.color = pathColor;
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 1; // ✅ נמוך מהרפסודה אבל עדיין נראה
            spriteRenderer.enabled = true; // ✅ וודא שהוא מופעל
            
            // ✅ עיגולים גדולים מספיק להיראות
            float diameter = Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f); // מינימום 0.5
            circle.transform.localScale = new Vector3(diameter, diameter, 1f);
            circle.SetActive(false); // נתחיל כבוי
            
            Debug.Log($"✅ Circle created: {name}");
            Debug.Log($"  Diameter: {diameter}");
            Debug.Log($"  Sorting Order: {spriteRenderer.sortingOrder}");
            Debug.Log($"  Color: {spriteRenderer.color}");
            Debug.Log($"  Sprite: {(spriteRenderer.sprite != null ? "EXISTS" : "NULL")}");
            
            return circle;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Circle creation failed for {name}: {e.Message}");
            return null;
        }
    }
    
    Sprite CreateCircleSprite()
    {
        try
        {
            int size = 64; // גודל יותר גדול לחדות טובה יותר
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f; // קצת יותר קטן מהגבול
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    // עיגול מלא בלבן
                    colors[y * size + x] = (distance <= radius) ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear; // חלק יותר
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            
            if (sprite != null)
            {
                Debug.Log($"✅ Circle sprite created: {size}x{size}, radius: {radius}");
            }
            else
            {
                Debug.LogError("❌ Failed to create sprite from texture");
            }
            
            return sprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Circle sprite creation failed: {e.Message}");
            return null;
        }
    }
    
    void ShowPathCircles(List<Vector2> path)
    {
        if (path == null || path.Count < 2) return;
        
        try
        {
            if (startCircle != null)
            {
                Vector3 startPos = new Vector3(path[0].x, path[0].y, 0f); // ✅ באותו מפלס כמו הקו
                startCircle.transform.position = startPos;
                startCircle.SetActive(true);
                
                // ✅ וודא שהעיגול גדול מספיק להיראות
                float diameter = Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f); // מינימום 0.5
                startCircle.transform.localScale = new Vector3(diameter, diameter, 1f);
                
                Debug.Log($"✅ Start circle: pos={startPos}, scale={diameter}, active={startCircle.activeInHierarchy}");
            }
            
            if (endCircle != null)
            {
                Vector3 endPos = new Vector3(path[path.Count - 1].x, path[path.Count - 1].y, 0f); // ✅ באותו מפלס כמו הקו
                endCircle.transform.position = endPos;
                endCircle.SetActive(true);
                
                // ✅ וודא שהעיגול גדול מספיק להיראות
                float diameter = Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f); // מינימום 0.5
                endCircle.transform.localScale = new Vector3(diameter, diameter, 1f);
                
                Debug.Log($"✅ End circle: pos={endPos}, scale={diameter}, active={endCircle.activeInHierarchy}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ShowPathCircles failed: {e.Message}");
        }
    }
    
    void HidePathCircles()
    {
        try
        {
            if (startCircle != null) startCircle.SetActive(false);
            if (endCircle != null) endCircle.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ HidePathCircles failed: {e.Message}");
        }
    }
    
    public void MoveToPosition(Vector2 targetPosition)
    {
        if (isMoving) return;
        
        if (mapController == null)
        {
            Debug.LogError("❌ MapController is null!");
            return;
        }
        
        if (!mapController.IsPositionNavigable(targetPosition))
        {
            Debug.Log("Cannot navigate to this position!");
            return;
        }
        
        List<Vector2> path = mapController.FindWaterPath(transform.position, targetPosition);
        
        if (path != null && path.Count > 0)
        {
            currentPath = new Queue<Vector2>(path);
            ShowPath(path);
            StartCoroutine(FollowPath());
        }
        else
        {
            Debug.LogWarning("No path found to target position!");
        }
    }
    
    void ShowPath(List<Vector2> path)
    {
        if (pathRenderer == null)
        {
            Debug.LogError("❌ PathRenderer is null! Attempting to recreate...");
            CreateSafeLineRenderer();
            
            if (pathRenderer == null)
            {
                Debug.LogError("❌ Failed to recreate PathRenderer!");
                return;
            }
        }
        
        if (path == null || path.Count == 0)
        {
            Debug.LogError("❌ Path is null or empty!");
            return;
        }
        
        try
        {
            pathRenderer.enabled = false;
            pathRenderer.positionCount = 0;
            
            // ✅ Z=0 לכל הנקודות - בחזית
            Vector3[] positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                positions[i] = new Vector3(path[i].x, path[i].y, 0f); // Z=0 תמיד
            }
            
            pathRenderer.positionCount = positions.Length;
            pathRenderer.SetPositions(positions);
            
            pathRenderer.enabled = true;
            
            ShowPathCircles(path);
            
            Debug.Log($"✅ ADJUSTABLE DASHED path shown: {path.Count} points, width={pathWidth}");
            Debug.Log($"📍 Start: {positions[0]}, End: {positions[positions.Length-1]}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ShowPath failed: {e.Message}");
        }
    }
    
    void HidePath()
    {
        try
        {
            if (pathRenderer != null)
            {
                pathRenderer.enabled = false;
                pathRenderer.positionCount = 0;
            }
            
            HidePathCircles();
            Debug.Log("✅ Path and circles hidden");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ HidePath failed: {e.Message}");
        }
    }
    
    IEnumerator FollowPath()
    {
        isMoving = true;
        
        while (currentPath != null && currentPath.Count > 0)
        {
            Vector2 nextWaypoint = currentPath.Dequeue();
            targetPosition = nextWaypoint;
            
            while (Vector2.Distance(transform.position, nextWaypoint) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, nextWaypoint, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        HidePath();
        isMoving = false;
        Debug.Log("✅ Path completed!");
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // ========= UPDATE & VALIDATION =========
    
    void OnValidate()
    {
        // ✅ עדכון רק אם יש שינוי אמיתי
        if (pathRenderer != null)
        {
            bool needsUpdate = false;
            
            // בדוק אם העובי השתנה
            if (Mathf.Abs(pathRenderer.startWidth - pathWidth) > 0.001f)
            {
                pathRenderer.startWidth = pathWidth;
                pathRenderer.endWidth = pathWidth;
                needsUpdate = true;
            }
            
            // בדוק אם הצבע השתנה
            if (pathRenderer.startColor != pathColor)
            {
                pathRenderer.startColor = pathColor;
                pathRenderer.endColor = pathColor;
                needsUpdate = true;
            }
            
            // עדכן טקסטורה רק אם צריך
            if (needsUpdate && pathRenderer.material != null)
            {
                Texture2D newDashTexture = CreatePerfectDashTexture();
                if (newDashTexture != null)
                {
                    pathRenderer.material.mainTexture = newDashTexture;
                    pathRenderer.material.color = pathColor; // ✅ וודא שהצבע מתעדכן
                    Debug.Log($"📏 Updated: pathWidth={pathWidth}, color={pathColor}, dash texture recreated");
                }
            }
        }
        
        // ✅ עדכן עיגולים - גודל וצבע
        float diameter = Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f);
        
        if (startCircle != null)
        {
            if (Mathf.Abs(startCircle.transform.localScale.x - diameter) > 0.001f)
            {
                startCircle.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
            
            // ✅ עדכן צבע העיגול
            SpriteRenderer sr = startCircle.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color != pathColor)
            {
                sr.color = pathColor;
            }
        }
        
        if (endCircle != null)
        {
            if (Mathf.Abs(endCircle.transform.localScale.x - diameter) > 0.001f)
            {
                endCircle.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
            
            // ✅ עדכן צבע העיגול
            SpriteRenderer sr = endCircle.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color != pathColor)
            {
                sr.color = pathColor;
            }
        }
    }
    
    // ========= DEBUGGING TOOLS =========
    
    [ContextMenu("⭕ Debug Circles Only")]
    void DebugCirclesOnly()
    {
        Debug.Log("⭕ CIRCLES DEBUG REPORT:");
        
        Debug.Log($"Start Circle: {(startCircle != null ? "EXISTS" : "NULL")}");
        if (startCircle != null)
        {
            Debug.Log($"  Active: {startCircle.activeInHierarchy}");
            Debug.Log($"  Position: {startCircle.transform.position}");
            Debug.Log($"  Scale: {startCircle.transform.localScale}");
            
            SpriteRenderer sr = startCircle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"  Sprite: {(sr.sprite != null ? "EXISTS" : "NULL")}");
                Debug.Log($"  Color: {sr.color}");
                Debug.Log($"  Sorting Order: {sr.sortingOrder}");
                Debug.Log($"  Enabled: {sr.enabled}");
                Debug.Log($"  Alpha: {sr.color.a}");
            }
        }
        
        Debug.Log($"End Circle: {(endCircle != null ? "EXISTS" : "NULL")}");
        if (endCircle != null)
        {
            Debug.Log($"  Active: {endCircle.activeInHierarchy}");
            Debug.Log($"  Position: {endCircle.transform.position}");
            Debug.Log($"  Scale: {endCircle.transform.localScale}");
            
            SpriteRenderer sr = endCircle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"  Sprite: {(sr.sprite != null ? "EXISTS" : "NULL")}");
                Debug.Log($"  Color: {sr.color}");
                Debug.Log($"  Sorting Order: {sr.sortingOrder}");
                Debug.Log($"  Enabled: {sr.enabled}");
                Debug.Log($"  Alpha: {sr.color.a}");
            }
        }
        
        // הצג רק עיגולים למשך 5 שניות
        if (startCircle != null && endCircle != null)
        {
            Vector3 testPos = transform.position;
            startCircle.transform.position = testPos + Vector3.left * 3f;
            endCircle.transform.position = testPos + Vector3.right * 3f;
            
            startCircle.SetActive(true);
            endCircle.SetActive(true);
            
            Debug.Log("👀 Showing ONLY circles for 5 seconds at test positions");
            StartCoroutine(HideCirclesAfterDelay(5f));
        }
    }
    
    System.Collections.IEnumerator HideCirclesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePathCircles();
        Debug.Log("⭕ Test circles hidden");
    }
    
    [ContextMenu("🎨 Test Color Changes")]
    void TestColorChanges()
    {
        Debug.Log("🎨 Testing color system...");
        Debug.Log($"Inspector Color: {pathColor}");
        
        if (pathRenderer != null)
        {
            Debug.Log($"LineRenderer Start Color: {pathRenderer.startColor}");
            Debug.Log($"LineRenderer End Color: {pathRenderer.endColor}");
            Debug.Log($"Material Color: {(pathRenderer.material != null ? pathRenderer.material.color : Color.black)}");
        }
        
        if (startCircle != null)
        {
            SpriteRenderer sr = startCircle.GetComponent<SpriteRenderer>();
            Debug.Log($"Start Circle Color: {(sr != null ? sr.color : Color.black)}");
        }
        
        if (endCircle != null)
        {
            SpriteRenderer sr = endCircle.GetComponent<SpriteRenderer>();
            Debug.Log($"End Circle Color: {(sr != null ? sr.color : Color.black)}");
        }
        
        // צור מסלול בדיקה כדי לראות את הצבעים
        List<Vector2> testPath = new List<Vector2>();
        Vector2 center = transform.position;
        testPath.Add(center + Vector2.left * 8);
        testPath.Add(center + Vector2.right * 8);
        
        ShowPath(testPath);
        Debug.Log("👀 Test path with current colors shown for 5 seconds");
        
        StartCoroutine(HideAfterDelay(5f));
    }
    
    [ContextMenu("🔍 Debug Everything")]
    void DebugEverything()
    {
        Debug.Log("🔍 COMPLETE DEBUG REPORT:");
        
        // בדוק LineRenderer
        Debug.Log($"📏 LineRenderer: {(pathRenderer != null ? "EXISTS" : "NULL")}");
        if (pathRenderer != null)
        {
            Debug.Log($"  Enabled: {pathRenderer.enabled}");
            Debug.Log($"  Width: {pathRenderer.startWidth}");
            Debug.Log($"  Points: {pathRenderer.positionCount}");
            Debug.Log($"  Material: {(pathRenderer.material != null ? "EXISTS" : "NULL")}");
            Debug.Log($"  Texture: {(pathRenderer.material?.mainTexture != null ? "EXISTS" : "NULL")}");
            Debug.Log($"  Shader: {pathRenderer.material?.shader?.name ?? "NULL"}");
            Debug.Log($"  Color: {pathRenderer.startColor}");
            Debug.Log($"  Sorting Order: {pathRenderer.sortingOrder}");
            Debug.Log($"  Texture Mode: {pathRenderer.textureMode}");
            Debug.Log($"  Alignment: {pathRenderer.alignment}");
            
            if (pathRenderer.positionCount > 0)
            {
                Vector3[] positions = new Vector3[pathRenderer.positionCount];
                pathRenderer.GetPositions(positions);
                Debug.Log($"  Start Position: {positions[0]}");
                Debug.Log($"  End Position: {positions[positions.Length - 1]}");
            }
        }
        
        // בדוק עיגולים
        Debug.Log($"⭕ Start Circle: {(startCircle != null ? "EXISTS" : "NULL")}");
        if (startCircle != null)
        {
            Debug.Log($"  Active: {startCircle.activeInHierarchy}");
            Debug.Log($"  Position: {startCircle.transform.position}");
            Debug.Log($"  Scale: {startCircle.transform.localScale}");
            SpriteRenderer sr = startCircle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"  Sprite: {(sr.sprite != null ? "EXISTS" : "NULL")}");
                Debug.Log($"  Color: {sr.color}");
                Debug.Log($"  Sorting Order: {sr.sortingOrder}");
                Debug.Log($"  Enabled: {sr.enabled}");
            }
        }
        
        Debug.Log($"⭕ End Circle: {(endCircle != null ? "EXISTS" : "NULL")}");
        if (endCircle != null)
        {
            Debug.Log($"  Active: {endCircle.activeInHierarchy}");
            Debug.Log($"  Position: {endCircle.transform.position}");
            Debug.Log($"  Scale: {endCircle.transform.localScale}");
        }
        
        Debug.Log("🎛️ PARAMETERS:");
        Debug.Log($"  Path Width: {pathWidth}");
        Debug.Log($"  Circle Multiplier: {circleSizeMultiplier}");
        Debug.Log($"  Path Color: {pathColor}");
        Debug.Log($"  Sorting Order: {pathSortingOrder}");
    }
    
    [ContextMenu("🔍 Debug Dash Texture")]
    void DebugDashTexture()
    {
        Debug.Log("🔍 Testing dash texture creation...");
        
        Texture2D testTexture = CreatePerfectDashTexture();
        
        if (testTexture != null)
        {
            Debug.Log($"✅ Texture created successfully:");
            Debug.Log($"  Size: {testTexture.width}x{testTexture.height}");
            Debug.Log($"  Format: {testTexture.format}");
            Debug.Log($"  Wrap Mode: {testTexture.wrapMode}");
            Debug.Log($"  Filter Mode: {testTexture.filterMode}");
            
            // בדוק כמה פיקסלים נראים וכמה שקופים
            Color[] pixels = testTexture.GetPixels();
            int visibleCount = 0;
            int transparentCount = 0;
            
            foreach (Color pixel in pixels)
            {
                if (pixel.a > 0.5f) visibleCount++;
                else transparentCount++;
            }
            
            Debug.Log($"  Visible pixels: {visibleCount}");
            Debug.Log($"  Transparent pixels: {transparentCount}");
            Debug.Log($"  Ratio: {(float)visibleCount / (visibleCount + transparentCount) * 100f:F1}% visible");
            
            if (pathRenderer != null && pathRenderer.material != null)
            {
                pathRenderer.material.mainTexture = testTexture;
                Debug.Log("✅ Applied new dash texture to LineRenderer");
            }
        }
        else
        {
            Debug.LogError("❌ Failed to create dash texture");
        }
    }
    
    [ContextMenu("🔧 Force Recreate Dash Texture")]
    void ForceRecreateDashTexture()
    {
        Debug.Log("🔧 Force recreating dash texture...");
        
        if (pathRenderer != null && pathRenderer.material != null)
        {
            Texture2D newTexture = CreatePerfectDashTexture();
            if (newTexture != null)
            {
                pathRenderer.material.mainTexture = newTexture;
                Debug.Log("✅ New dash texture applied");
                
                // הצג מסלול בדיקה כדי לראות את התוצאה
                List<Vector2> testPath = new List<Vector2>();
                Vector2 center = transform.position;
                testPath.Add(center + Vector2.left * 10);
                testPath.Add(center + Vector2.right * 10);
                
                ShowPath(testPath);
                Debug.Log("👀 Test path shown - you should see dashed line now!");
                
                StartCoroutine(HideAfterDelay(8f));
            }
        }
        else
        {
            Debug.LogError("❌ PathRenderer or material not found");
        }
    }
    
    [ContextMenu("🎯 Test Adjustable Dashed Path")]
    void TestAdjustableDashedPath()
    {
        Debug.Log("🎯 Testing ADJUSTABLE DASHED path...");
        
        // צור מסלול בדיקה ארוך
        List<Vector2> testPath = new List<Vector2>();
        Vector2 center = transform.position;
        
        testPath.Add(center + Vector2.left * 15);
        testPath.Add(center + Vector2.right * 15);
        
        ShowPath(testPath);
        
        Debug.Log($"🔥 ADJUSTABLE PATH CREATED:");
        Debug.Log($"📏 Line Width: {pathWidth} (Adjustable in Inspector)");
        Debug.Log($"⭕ Circle Diameter: {Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f)} (Sorting Order: 1)");
        Debug.Log($"🎨 Color: {pathColor}");
        Debug.Log($"🎲 Dash Pattern: 75% visible, 25% transparent");
        Debug.Log("👀 You should see: COLORED DASHED line + VISIBLE circles");
        Debug.Log("🎛️ Tip: Change Path Width and Path Color in Inspector!");
        
        StartCoroutine(HideAfterDelay(10f));
    }
    
    [ContextMenu("📊 Debug Adjustable Line Status")]
    void DebugAdjustableLineStatus()
    {
        Debug.Log("=== ADJUSTABLE DASHED LINE STATUS ===");
        Debug.Log($"📏 Current Path Width: {pathWidth} (Adjustable: 0.01f - 5.0f)");
        Debug.Log($"⭕ Circle Diameter: {Mathf.Max(pathWidth * circleSizeMultiplier, 0.5f)}");
        Debug.Log($"🎨 Path Color: {pathColor}");
        
        Debug.Log($"PathRenderer: {(pathRenderer != null ? "EXISTS" : "NULL")}");
        if (pathRenderer != null)
        {
            Debug.Log($"  Enabled: {pathRenderer.enabled}");
            Debug.Log($"  Width: {pathRenderer.startWidth} (Should match: {pathWidth})");
            Debug.Log($"  Material: {(pathRenderer.material != null ? "EXISTS" : "NULL")}");
            Debug.Log($"  Texture: {(pathRenderer.material?.mainTexture != null ? "ADAPTIVE DASH" : "NULL")}");
        }
        
        Debug.Log($"StartCircle: {(startCircle != null ? "EXISTS" : "NULL")}");
        Debug.Log($"EndCircle: {(endCircle != null ? "EXISTS" : "NULL")}");
        
        Debug.Log("💡 ADJUSTABLE: Change Path Width in Inspector (0.01f - 5.0f)");
        Debug.Log("🎲 FIXED DASH: 75% visible, 25% transparent pattern");
        Debug.Log("===============================");
    }
    
    [ContextMenu("🔧 Force Recreate Adjustable Line")]
    void ForceRecreateAdjustableLine()
    {
        Debug.Log("🔧 Force recreating ADJUSTABLE dashed line...");
        SafeSetupEverything();
        Debug.Log("✅ Adjustable line recreation completed!");
    }
    
    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePath();
        Debug.Log("🎯 Test path hidden");
    }
}