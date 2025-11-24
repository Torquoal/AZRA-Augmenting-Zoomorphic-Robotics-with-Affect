using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FaceStyleManager : MonoBehaviour
{
    [Header("Face Style Settings")]
    [SerializeField] private FaceStyle currentStyle = FaceStyle.Current;
    
    [Header("Face Style Collections")]
    [SerializeField] private FaceStyleData currentStyleData;
    [SerializeField] private FaceStyleData animeStyleData;
    [SerializeField] private FaceStyleData catStyleData;
    
    [Header("Animation Settings")]
    // Animation settings removed - not currently used
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Current loaded style data
    private FaceStyleData activeStyleData;
    
    public enum FaceStyle
    {
        Current,
        Anime,
        Cat
    }
    
    [System.Serializable]
    public class FaceStyleData
    {
        [Header("Animation Paths")]
        public string neutralLoopPath = "AbstractFaces/NeutralLoop";
        public string happyLoopPath = "AbstractFaces/HappyLoop";
        public string angryLoopPath = "AbstractFaces/AngryLoop";
        public string sadLoopPath = "AbstractFaces/SadLoop";
        public string scaredLoopPath = "AbstractFaces/ScaredLoop";
        public string surprisedLoopPath = "AbstractFaces/SurprisedLoop";
        
        [Header("Static Neutral Face")]
        public Texture2D staticNeutralFace; // For styles that don't have neutral animation
        
        [Header("Frame Counts (for different styles)")]
        public int neutralFrameCount = 100;
        public int happyFrameCount = 100;
        public int angryFrameCount = 100;
        public int sadFrameCount = 100;
        public int scaredFrameCount = 100;
        public int surprisedFrameCount = 100;
        
        [Header("Loop Settings")]
        public bool loopNeutralAnimation = true;
        public bool loopHappyAnimation = true;
        public bool loopAngryAnimation = true;
        public bool loopSadAnimation = true;
        public bool loopScaredAnimation = true;
        public bool loopSurprisedAnimation = true;
    }
    
    void Start()
    {
        // Set up default paths for each style if not already configured
        SetupDefaultPaths();
        LoadFaceStyle(currentStyle);
    }
    
    private void SetupDefaultPaths()
    {
        // Set up default paths for Current style (AbstractFaces)
        if (currentStyleData != null)
        {
            if (string.IsNullOrEmpty(currentStyleData.neutralLoopPath))
                currentStyleData.neutralLoopPath = "AbstractFaces/NeutralLoop";
            if (string.IsNullOrEmpty(currentStyleData.happyLoopPath))
                currentStyleData.happyLoopPath = "AbstractFaces/HappyLoop";
            if (string.IsNullOrEmpty(currentStyleData.angryLoopPath))
                currentStyleData.angryLoopPath = "AbstractFaces/AngryLoop";
            if (string.IsNullOrEmpty(currentStyleData.sadLoopPath))
                currentStyleData.sadLoopPath = "AbstractFaces/SadLoop";
            if (string.IsNullOrEmpty(currentStyleData.scaredLoopPath))
                currentStyleData.scaredLoopPath = "AbstractFaces/ScaredLoop";
            if (string.IsNullOrEmpty(currentStyleData.surprisedLoopPath))
                currentStyleData.surprisedLoopPath = "AbstractFaces/SurprisedLoop";
        }
        
        // Set up default paths for Anime style (MangaFaces)
        // Neutral loop stays the same for all styles - use AbstractFaces
        if (animeStyleData != null)
        {
            if (string.IsNullOrEmpty(animeStyleData.neutralLoopPath))
                animeStyleData.neutralLoopPath = "AbstractFaces/NeutralLoop"; // Same as current style
            if (string.IsNullOrEmpty(animeStyleData.happyLoopPath))
                animeStyleData.happyLoopPath = "MangaFaces/HappyLoop";
            if (string.IsNullOrEmpty(animeStyleData.angryLoopPath))
                animeStyleData.angryLoopPath = "MangaFaces/AngryLoop";
            if (string.IsNullOrEmpty(animeStyleData.sadLoopPath))
                animeStyleData.sadLoopPath = "MangaFaces/SadLoop";
            if (string.IsNullOrEmpty(animeStyleData.scaredLoopPath))
                animeStyleData.scaredLoopPath = "MangaFaces/ScaredLoop";
            if (string.IsNullOrEmpty(animeStyleData.surprisedLoopPath))
                animeStyleData.surprisedLoopPath = "MangaFaces/SurprisedLoop";
            
            // Manga faces should loop continuously
            animeStyleData.loopNeutralAnimation = true;
            animeStyleData.loopHappyAnimation = true;
            animeStyleData.loopAngryAnimation = true;
            animeStyleData.loopSadAnimation = true;
            animeStyleData.loopScaredAnimation = true;
            animeStyleData.loopSurprisedAnimation = true;
        }
        
        // Set up default paths for Cat style (CatFaces)
        // For cat style, use happy frames as neutral frames
        if (catStyleData != null)
        {
            if (string.IsNullOrEmpty(catStyleData.neutralLoopPath))
                catStyleData.neutralLoopPath = "CatFaces/HappyLoop"; // Use happy frames as neutral for cat style
            if (string.IsNullOrEmpty(catStyleData.happyLoopPath))
                catStyleData.happyLoopPath = "CatFaces/HappyLoop";
            if (string.IsNullOrEmpty(catStyleData.angryLoopPath))
                catStyleData.angryLoopPath = "CatFaces/AngryLoop";
            if (string.IsNullOrEmpty(catStyleData.sadLoopPath))
                catStyleData.sadLoopPath = "CatFaces/SadLoop";
            if (string.IsNullOrEmpty(catStyleData.scaredLoopPath))
                catStyleData.scaredLoopPath = "CatFaces/ScaredLoop";
            if (string.IsNullOrEmpty(catStyleData.surprisedLoopPath))
                catStyleData.surprisedLoopPath = "CatFaces/SurprisedLoop";
            
            // Cat faces should NOT loop - play once and hold last frame
            catStyleData.loopNeutralAnimation = false;
            catStyleData.loopHappyAnimation = false;
            catStyleData.loopAngryAnimation = false;
            catStyleData.loopSadAnimation = false;
            catStyleData.loopScaredAnimation = false;
            catStyleData.loopSurprisedAnimation = false;
        }
    }
    
    public void SetFaceStyle(FaceStyle newStyle)
    {
        if (newStyle == currentStyle) return;
        
        if (showDebugLogs)
            Debug.Log($"FaceStyleManager: Switching from {currentStyle} to {newStyle}");
        
        currentStyle = newStyle;
        LoadFaceStyle(currentStyle);
    }
    
    public void SetFaceStyle(int styleIndex)
    {
        if (styleIndex >= 0 && styleIndex < 3)
        {
            SetFaceStyle((FaceStyle)styleIndex);
        }
    }
    
    private void LoadFaceStyle(FaceStyle style)
    {
        switch (style)
        {
            case FaceStyle.Current:
                activeStyleData = currentStyleData;
                break;
            case FaceStyle.Anime:
                activeStyleData = animeStyleData;
                break;
            case FaceStyle.Cat:
                activeStyleData = catStyleData;
                break;
        }
        
        if (showDebugLogs)
            Debug.Log($"FaceStyleManager: Loaded {style} style data");
    }
    
    public FaceStyleData GetCurrentStyleData()
    {
        return activeStyleData;
    }
    
    public FaceStyle GetCurrentStyle()
    {
        return currentStyle;
    }
    
    public string GetCurrentStyleName()
    {
        return currentStyle.ToString();
    }
    
    public int GetFrameCountForEmotion(string emotion)
    {
        if (activeStyleData == null) return 100; // Default fallback
        
        switch (emotion.ToLower())
        {
            case "neutral":
                return activeStyleData.neutralFrameCount;
            case "happy":
                return activeStyleData.happyFrameCount;
            case "angry":
                return activeStyleData.angryFrameCount;
            case "sad":
                return activeStyleData.sadFrameCount;
            case "scared":
                return activeStyleData.scaredFrameCount;
            case "surprised":
                return activeStyleData.surprisedFrameCount;
            default:
                return 100; // Default fallback
        }
    }
    
    public bool ShouldLoopAnimation(string emotion)
    {
        if (activeStyleData == null) return true; // Default fallback
        
        switch (emotion.ToLower())
        {
            case "neutral":
                return activeStyleData.loopNeutralAnimation;
            case "happy":
                return activeStyleData.loopHappyAnimation;
            case "angry":
                return activeStyleData.loopAngryAnimation;
            case "sad":
                return activeStyleData.loopSadAnimation;
            case "scared":
                return activeStyleData.loopScaredAnimation;
            case "surprised":
                return activeStyleData.loopSurprisedAnimation;
            default:
                return true; // Default fallback
        }
    }
    
    public string GetAnimationPathForEmotion(string emotion)
    {
        if (activeStyleData == null) return "";
        
        switch (emotion.ToLower())
        {
            case "neutral":
                return activeStyleData.neutralLoopPath;
            case "happy":
                return activeStyleData.happyLoopPath;
            case "angry":
                return activeStyleData.angryLoopPath;
            case "sad":
                return activeStyleData.sadLoopPath;
            case "scared":
                return activeStyleData.scaredLoopPath;
            case "surprised":
                return activeStyleData.surprisedLoopPath;
            default:
                return "";
        }
    }
    
    public Texture2D GetStaticNeutralFace()
    {
        return activeStyleData?.staticNeutralFace;
    }
    
    // Context menu for testing
    [ContextMenu("Test Switch to Anime")]
    private void TestSwitchToAnime()
    {
        SetFaceStyle(FaceStyle.Anime);
    }
    
    [ContextMenu("Test Switch to Cat")]
    private void TestSwitchToCat()
    {
        SetFaceStyle(FaceStyle.Cat);
    }
    
    [ContextMenu("Test Switch to Current")]
    private void TestSwitchToCurrent()
    {
        SetFaceStyle(FaceStyle.Current);
    }
}
