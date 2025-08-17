using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

// כלי עזר ליצירת דגים מהיר עם הגדרות מוכנות מראש
public class FishCreatorTool : MonoBehaviour
{
    [Header("🐟 Fish Creator Tool")]
    [Space(10)]
    
    [Header("Quick Setup")]
    public bool autoDetectSprites = true;
    public string spriteFolderPath = "Assets/Art/Fish/";
    
    [Header("Fish Presets")]
    [SerializeField] private List<FishPreset> fishPresets = new List<FishPreset>();
    
    [Header("Bulk Operations")]
    public bool createAllPresets = false;
    public FishSpawner targetSpawner;
    
    [System.Serializable]
    public class FishPreset
    {
        [Header("Basic Info")]
        public string presetName = "New Fish";
        public FishType fishType = FishType.Tropical;
        public FishSize fishSize = FishSize.Small;
        public FishRarity rarity = FishRarity.Common;
        
        [Header("Auto Settings")]
        public bool useAutoSettings = true;
        public string spritePrefix = "Fish_"; // Fish_ClownFish_, Fish_BlueTang_ etc.
        
        [Header("Manual Settings (if not auto)")]
        public Sprite[] customFrames;
        public float customAnimationSpeed = 8f;
        public Vector2 customSizeRange = new Vector2(0.8f, 1.2f);
        public Color customTint = Color.white;
        public float customMoveSpeed = 1f;
        
        [Header("Generated Result")]
        [SerializeField] private FishSpawner.FishData generatedFishData;
        
        public FishSpawner.FishData GenerateFishData()
        {
            FishSpawner.FishData fishData = new FishSpawner.FishData();
            
            // Basic setup
            fishData.fishName = presetName;
            
            if (useAutoSettings)
            {
                ApplyAutoSettings(fishData);
            }
            else
            {
                ApplyManualSettings(fishData);
            }
            
            generatedFishData = fishData;
            return fishData;
        }
        
        private void ApplyAutoSettings(FishSpawner.FishData fishData)
        {
            // Auto animation speed based on size
            switch (fishSize)
            {
                case FishSize.Tiny:
                    fishData.animationSpeed = Random.Range(12f, 16f);
                    fishData.sizeRange = new Vector2(0.4f, 0.6f);
                    fishData.moveSpeedMultiplier = 1.5f;
                    break;
                case FishSize.Small:
                    fishData.animationSpeed = Random.Range(8f, 12f);
                    fishData.sizeRange = new Vector2(0.7f, 1.0f);
                    fishData.moveSpeedMultiplier = 1.2f;
                    break;
                case FishSize.Medium:
                    fishData.animationSpeed = Random.Range(6f, 10f);
                    fishData.sizeRange = new Vector2(1.0f, 1.4f);
                    fishData.moveSpeedMultiplier = 1.0f;
                    break;
                case FishSize.Large:
                    fishData.animationSpeed = Random.Range(4f, 7f);
                    fishData.sizeRange = new Vector2(1.5f, 2.0f);
                    fishData.moveSpeedMultiplier = 0.8f;
                    break;
                case FishSize.Huge:
                    fishData.animationSpeed = Random.Range(2f, 5f);
                    fishData.sizeRange = new Vector2(2.0f, 3.0f);
                    fishData.moveSpeedMultiplier = 0.5f;
                    break;
            }
            
            // Auto spawn weight based on rarity
            switch (rarity)
            {
                case FishRarity.Common:
                    fishData.spawnWeight = Random.Range(0.8f, 1.0f);
                    break;
                case FishRarity.Uncommon:
                    fishData.spawnWeight = Random.Range(0.4f, 0.6f);
                    break;
                case FishRarity.Rare:
                    fishData.spawnWeight = Random.Range(0.1f, 0.3f);
                    break;
                case FishRarity.Epic:
                    fishData.spawnWeight = Random.Range(0.05f, 0.1f);
                    break;
                case FishRarity.Legendary:
                    fishData.spawnWeight = Random.Range(0.01f, 0.05f);
                    break;
            }
            
            // Auto tint based on type
            switch (fishType)
            {
                case FishType.Tropical:
                    fishData.tintColor = GetRandomTropicalColor();
                    break;
                case FishType.DeepSea:
                    fishData.tintColor = GetRandomDeepSeaColor();
                    break;
                case FishType.Shark:
                    fishData.tintColor = GetRandomSharkColor();
                    break;
                case FishType.Jellyfish:
                    fishData.tintColor = GetRandomJellyfishColor();
                    break;
                case FishType.Exotic:
                    fishData.tintColor = GetRandomExoticColor();
                    break;
            }
            
            // Auto sorting
            fishData.sortingLayerName = "Fish";
            fishData.sortingOrder = GetSortingOrderBySize(fishSize);
            
            // Try to auto-load sprites
            #if UNITY_EDITOR
            fishData.animationFrames = TryLoadSprites(spritePrefix);
            #endif
        }
        
        private void ApplyManualSettings(FishSpawner.FishData fishData)
        {
            fishData.animationFrames = customFrames;
            fishData.animationSpeed = customAnimationSpeed;
            fishData.sizeRange = customSizeRange;
            fishData.tintColor = customTint;
            fishData.moveSpeedMultiplier = customMoveSpeed;
            fishData.spawnWeight = GetWeightByRarity(rarity);
            fishData.sortingLayerName = "Fish";
            fishData.sortingOrder = 0;
        }
        
        private Color GetRandomTropicalColor()
        {
            Color[] tropicalColors = {
                new Color(1f, 0.5f, 0f),    // Orange
                new Color(1f, 1f, 0f),     // Yellow
                new Color(0f, 0.5f, 1f),   // Blue
                new Color(1f, 0f, 0.5f),   // Pink
                new Color(0.5f, 1f, 0f),   // Green
                Color.white
            };
            return tropicalColors[Random.Range(0, tropicalColors.Length)];
        }
        
        private Color GetRandomDeepSeaColor()
        {
            return new Color(
                Random.Range(0.2f, 0.6f),
                Random.Range(0.3f, 0.7f),
                Random.Range(0.6f, 1f),
                1f
            );
        }
        
        private Color GetRandomSharkColor()
        {
            return new Color(
                Random.Range(0.4f, 0.7f),
                Random.Range(0.4f, 0.7f),
                Random.Range(0.4f, 0.7f),
                1f
            );
        }
        
        private Color GetRandomJellyfishColor()
        {
            return new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.9f, 1f),
                0.8f // שקוף יותר
            );
        }
        
        private Color GetRandomExoticColor()
        {
            return new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0.5f, 1f),
                1f
            );
        }
        
        private int GetSortingOrderBySize(FishSize size)
        {
            switch (size)
            {
                case FishSize.Tiny: return 3;
                case FishSize.Small: return 2;
                case FishSize.Medium: return 1;
                case FishSize.Large: return 0;
                case FishSize.Huge: return -1;
                default: return 0;
            }
        }
        
        private float GetWeightByRarity(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Common: return 1.0f;
                case FishRarity.Uncommon: return 0.5f;
                case FishRarity.Rare: return 0.2f;
                case FishRarity.Epic: return 0.08f;
                case FishRarity.Legendary: return 0.02f;
                default: return 1.0f;
            }
        }
        
        #if UNITY_EDITOR
        public Sprite[] TryLoadSprites(string prefix)
        {
            List<Sprite> foundSprites = new List<Sprite>();
            
            string[] guids = AssetDatabase.FindAssets($"{prefix} t:Sprite");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null && sprite.name.Contains(prefix))
                {
                    foundSprites.Add(sprite);
                }
            }
            
            // Sort by name for proper animation order
            foundSprites.Sort((a, b) => string.Compare(a.name, b.name));
            
            return foundSprites.ToArray();
        }
        #else
        public Sprite[] TryLoadSprites(string prefix)
        {
            return new Sprite[0];
        }
        #endif
    }
    
    public enum FishType
    {
        Tropical,
        DeepSea,
        Shark,
        Jellyfish,
        Exotic
    }
    
    public enum FishSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge
    }
    
    public enum FishRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    void Start()
    {
        // Find spawner if not assigned
        if (targetSpawner == null)
        {
            #if UNITY_2023_1_OR_NEWER
            targetSpawner = FindFirstObjectByType<FishSpawner>();
            #else
            targetSpawner = FindObjectOfType<FishSpawner>();
            #endif
        }
    }
    
    // Public methods for runtime use
    public void CreateFishFromPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= fishPresets.Count)
        {
            Debug.LogError($"❌ Invalid preset index: {presetIndex}");
            return;
        }
        
        FishPreset preset = fishPresets[presetIndex];
        FishSpawner.FishData newFishData = preset.GenerateFishData();
        
        if (targetSpawner != null)
        {
            // Add to spawner's fish types (requires making fishTypes public or adding method)
            Debug.Log($"🐟 Created fish: {newFishData.fishName}");
        }
    }
    
    public void CreateAllPresetFish()
    {
        if (targetSpawner == null)
        {
            Debug.LogError("❌ No FishSpawner assigned!");
            return;
        }
        
        int created = 0;
        foreach (var preset in fishPresets)
        {
            FishSpawner.FishData fishData = preset.GenerateFishData();
            if (fishData.IsValid())
            {
                created++;
                Debug.Log($"✅ Created: {fishData.fishName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Invalid fish data for: {preset.presetName}");
            }
        }
        
        Debug.Log($"🐟 Created {created}/{fishPresets.Count} fish from presets");
    }
    
    // Context menu methods for easy access
    [ContextMenu("Create Default Fish Presets")]
    public void CreateDefaultPresets()
    {
        fishPresets.Clear();
        
        // Special preset for user's large animation (125 frames, 487x500)
        fishPresets.Add(new FishPreset {
            presetName = "Large Ocean Fish",
            fishType = FishType.Tropical,
            fishSize = FishSize.Large,
            rarity = FishRarity.Uncommon,
            spritePrefix = "fish_", // matches fish_00000.png pattern
            useAutoSettings = true
        });
        
        // Additional presets for future fish
        fishPresets.Add(new FishPreset {
            presetName = "Clown Fish",
            fishType = FishType.Tropical,
            fishSize = FishSize.Small,
            rarity = FishRarity.Common,
            spritePrefix = "Fish_ClownFish"
        });
        
        fishPresets.Add(new FishPreset {
            presetName = "Blue Tang",
            fishType = FishType.Tropical,
            fishSize = FishSize.Small,
            rarity = FishRarity.Common,
            spritePrefix = "Fish_BlueTang"
        });
        
        fishPresets.Add(new FishPreset {
            presetName = "Angel Fish",
            fishType = FishType.Tropical,
            fishSize = FishSize.Medium,
            rarity = FishRarity.Uncommon,
            spritePrefix = "Fish_AngelFish"
        });
        
        // Deep sea fish
        fishPresets.Add(new FishPreset {
            presetName = "Deep Sea Lantern",
            fishType = FishType.DeepSea,
            fishSize = FishSize.Small,
            rarity = FishRarity.Rare,
            spritePrefix = "Fish_Lantern"
        });
        
        // Sharks
        fishPresets.Add(new FishPreset {
            presetName = "Baby Shark",
            fishType = FishType.Shark,
            fishSize = FishSize.Medium,
            rarity = FishRarity.Uncommon,
            spritePrefix = "Fish_BabyShark"
        });
        
        fishPresets.Add(new FishPreset {
            presetName = "Great White",
            fishType = FishType.Shark,
            fishSize = FishSize.Huge,
            rarity = FishRarity.Legendary,
            spritePrefix = "Fish_GreatWhite"
        });
        
        // Jellyfish
        fishPresets.Add(new FishPreset {
            presetName = "Moon Jellyfish",
            fishType = FishType.Jellyfish,
            fishSize = FishSize.Medium,
            rarity = FishRarity.Uncommon,
            spritePrefix = "Fish_MoonJelly"
        });
        
        // Exotic
        fishPresets.Add(new FishPreset {
            presetName = "Rainbow Fish",
            fishType = FishType.Exotic,
            fishSize = FishSize.Small,
            rarity = FishRarity.Epic,
            spritePrefix = "Fish_Rainbow"
        });
        
        fishPresets.Add(new FishPreset {
            presetName = "Golden Fish",
            fishType = FishType.Exotic,
            fishSize = FishSize.Medium,
            rarity = FishRarity.Legendary,
            spritePrefix = "Fish_Golden"
        });
        
        Debug.Log($"✅ Created {fishPresets.Count} default fish presets (including Large Ocean Fish for 125-frame animation)");
    }
    
    [ContextMenu("Generate All Fish Data")]
    public void GenerateAllFishData()
    {
        foreach (var preset in fishPresets)
        {
            preset.GenerateFishData();
        }
        
        Debug.Log("🎨 Generated all fish data from presets");
    }
    
    [ContextMenu("Auto-Find Sprites")]
    public void AutoFindSprites()
    {
        #if UNITY_EDITOR
        foreach (var preset in fishPresets)
        {
            if (preset.useAutoSettings)
            {
                Sprite[] foundSprites = preset.TryLoadSprites(preset.spritePrefix);
                if (foundSprites.Length > 0)
                {
                    Debug.Log($"🔍 Found {foundSprites.Length} sprites for {preset.presetName}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No sprites found for {preset.presetName} with prefix '{preset.spritePrefix}'");
                }
            }
        }
        #endif
    }
    
    [ContextMenu("Debug Preset Info")]
    public void DebugPresetInfo()
    {
        Debug.Log("=== FISH CREATOR TOOL DEBUG ===");
        Debug.Log($"Total presets: {fishPresets.Count}");
        Debug.Log($"Target spawner: {(targetSpawner != null ? "Found" : "Missing")}");
        Debug.Log($"Auto detect sprites: {autoDetectSprites}");
        Debug.Log($"Sprite folder: {spriteFolderPath}");
        
        foreach (var preset in fishPresets)
        {
            var fishData = preset.GenerateFishData();
            string status = fishData.IsValid() ? "✅" : "❌";
            Debug.Log($"{status} {preset.presetName}: {preset.fishType} {preset.fishSize} {preset.rarity}");
            if (fishData.animationFrames != null)
                Debug.Log($"    Frames: {fishData.animationFrames.Length}, Speed: {fishData.animationSpeed:F1}, Weight: {fishData.spawnWeight:F2}");
        }
    }
    
    // Helper method to add generated fish to spawner
    public void AddPresetsToSpawner()
    {
        if (targetSpawner == null)
        {
            Debug.LogError("❌ No target spawner assigned!");
            return;
        }
        
        // This would require the FishSpawner to have a public method to add fish types
        // For now, log the creation - user can manually copy the data
        foreach (var preset in fishPresets)
        {
            var fishData = preset.GenerateFishData();
            if (fishData.IsValid())
            {
                Debug.Log($"🐟 Ready to add: {fishData.fishName} ({fishData.animationFrames.Length} frames, weight: {fishData.spawnWeight:F2})");
            }
        }
        
        Debug.Log("💡 Manually copy these FishData objects to your FishSpawner component");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FishCreatorTool))]
public class FishCreatorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FishCreatorTool tool = (FishCreatorTool)target;
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("🛠️ Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🐟 Create Default Presets"))
        {
            tool.CreateDefaultPresets();
        }
        
        if (GUILayout.Button("🎨 Generate All Fish Data"))
        {
            tool.GenerateAllFishData();
        }
        
        if (GUILayout.Button("🔍 Auto-Find Sprites"))
        {
            tool.AutoFindSprites();
        }
        
        if (GUILayout.Button("📋 Debug Info"))
        {
            tool.DebugPresetInfo();
        }
        
        EditorGUILayout.Space(10);
        
        if (tool.targetSpawner != null)
        {
            if (GUILayout.Button("➕ Add Presets to Spawner"))
            {
                tool.AddPresetsToSpawner();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a FishSpawner to use quick addition features", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("💡 Tip: Use sprite prefixes like 'Fish_ClownFish_' for auto-detection", EditorStyles.helpBox);
    }
}
#endif