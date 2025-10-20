using UnityEngine;
using System.Collections;
using System.Linq;

public class FaceAnimationController : MonoBehaviour
{
    [Header("Face Style Management")]
    [SerializeField] private FaceStyleManager faceStyleManager; // Reference to face style manager
    
    [Header("Legacy Animation Settings (Deprecated)")]
    [SerializeField] private string neutralLoopPath = "AbstractFaces/NeutralLoop";  // Legacy - kept for fallback
    [SerializeField] private string happyLoopPath = "AbstractFaces/HappyLoop";      // Legacy - kept for fallback
    [SerializeField] private string angryLoopPath = "AbstractFaces/AngryLoop";      // Legacy - kept for fallback
    [SerializeField] private string sadLoopPath = "AbstractFaces/SadLoop";        // Legacy - kept for fallback
    [SerializeField] private string scaredLoopPath = "AbstractFaces/ScaredLoop";  // Legacy - kept for fallback
    [SerializeField] private string surprisedLoopPath = "AbstractFaces/SurprisedLoop"; // Legacy - kept for fallback
    [SerializeField] private float frameRate = 24f;      // Animation frame rate
    [SerializeField] private bool useOptimizedFrames = true; // Use 100 frames instead of 200
    [SerializeField] private int optimizedFrameCount = 100; // Number of frames to use when optimized
    [SerializeField] private float frameDurationMultiplier = 2f; // Each frame lasts 2x longer

    [Header("Legacy Animation Behavior (Deprecated)")]
    [SerializeField] private bool loopNeutralAnimation = true;
    [SerializeField] private bool loopHappyAnimation = true;
    [SerializeField] private bool loopAngryAnimation = true;
    [SerializeField] private bool loopSadAnimation = true;
    [SerializeField] private bool loopScaredAnimation = true;
    [SerializeField] private bool loopSurprisedAnimation = true;
    
    private Material animatedMaterial;
    private float frameInterval;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private Coroutine animationCoroutine;
    private Texture2D[] neutralFrames;
    private Texture2D[] happyFrames;
    private Texture2D[] angryFrames;
    private Texture2D[] sadFrames;
    private Texture2D[] scaredFrames;
    private Texture2D[] surprisedFrames;

    private void Start()
    {
        frameInterval = 1f / frameRate;
        LoadAnimationFrames();
        
        // Create a new material instance
        animatedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        SetupMaterial();
    }

    private void LoadAnimationFrames()
    {
        Debug.Log($"FaceAnimationController: Starting frame loading. FaceStyleManager assigned: {faceStyleManager != null}");
        
        // Try to use FaceStyleManager first
        if (faceStyleManager != null)
        {
            Debug.Log("FaceAnimationController: Using FaceStyleManager for frame loading");
            LoadFramesFromStyleManager();
        }
        else
        {
            Debug.LogWarning("FaceAnimationController: FaceStyleManager not assigned, falling back to legacy loading");
            // Fallback to legacy loading
            LoadFramesLegacy();
        }
    }
    
    private void LoadFramesFromStyleManager()
    {
        if (faceStyleManager == null)
        {
            Debug.LogWarning("FaceAnimationController: FaceStyleManager not assigned, falling back to legacy loading");
            LoadFramesLegacy();
            return;
        }
        
        var styleData = faceStyleManager.GetCurrentStyleData();
        if (styleData == null)
        {
            Debug.LogWarning("FaceStyleManager: No style data available, falling back to legacy loading");
            LoadFramesLegacy();
            return;
        }
        
        Debug.Log($"FaceAnimationController: Loading frames from {faceStyleManager.GetCurrentStyleName()} style");
        Debug.Log($"Style data paths:");
        Debug.Log($"  Neutral: {styleData.neutralLoopPath}");
        Debug.Log($"  Happy: {styleData.happyLoopPath}");
        Debug.Log($"  Angry: {styleData.angryLoopPath}");
        Debug.Log($"  Sad: {styleData.sadLoopPath}");
        Debug.Log($"  Scared: {styleData.scaredLoopPath}");
        Debug.Log($"  Surprised: {styleData.surprisedLoopPath}");
        
        // Load frames using style manager data
        LoadFramesForEmotion(styleData.neutralLoopPath, ref neutralFrames, styleData.neutralFrameCount);
        LoadFramesForEmotion(styleData.happyLoopPath, ref happyFrames, styleData.happyFrameCount);
        LoadFramesForEmotion(styleData.angryLoopPath, ref angryFrames, styleData.angryFrameCount);
        LoadFramesForEmotion(styleData.sadLoopPath, ref sadFrames, styleData.sadFrameCount);
        LoadFramesForEmotion(styleData.scaredLoopPath, ref scaredFrames, styleData.scaredFrameCount);
        LoadFramesForEmotion(styleData.surprisedLoopPath, ref surprisedFrames, styleData.surprisedFrameCount);
    }
    
    private void LoadFramesLegacy()
    {
        Debug.Log("FaceAnimationController: Using LEGACY loading method with these paths:");
        Debug.Log($"  Neutral: {neutralLoopPath}");
        Debug.Log($"  Happy: {happyLoopPath}");
        Debug.Log($"  Angry: {angryLoopPath}");
        Debug.Log($"  Sad: {sadLoopPath}");
        Debug.Log($"  Scared: {scaredLoopPath}");
        Debug.Log($"  Surprised: {surprisedLoopPath}");
        
        // Legacy loading method
        LoadFramesForEmotion(neutralLoopPath, ref neutralFrames);
        LoadFramesForEmotion(happyLoopPath, ref happyFrames);
        LoadFramesForEmotion(angryLoopPath, ref angryFrames);
        LoadFramesForEmotion(sadLoopPath, ref sadFrames);
        LoadFramesForEmotion(scaredLoopPath, ref scaredFrames);
        LoadFramesForEmotion(surprisedLoopPath, ref surprisedFrames);
    }

    private void LoadFramesForEmotion(string path, ref Texture2D[] frames)
    {
        LoadFramesForEmotion(path, ref frames, optimizedFrameCount);
    }
    
    private void LoadFramesForEmotion(string path, ref Texture2D[] frames, int targetFrameCount)
    {
        Debug.Log($"FaceAnimationController: Attempting to load frames from Resources/{path}");
        
        // Load all textures from the Resources folder
        Object[] loadedObjects = Resources.LoadAll(path, typeof(Texture2D));
        
        Debug.Log($"FaceAnimationController: Found {loadedObjects.Length} objects in Resources/{path}");
        
        // Convert to Texture2D array and sort by name to ensure correct order
        var allFrames = loadedObjects
            .Cast<Texture2D>()
            .OrderBy(tex => tex.name)
            .ToArray();

        if (useOptimizedFrames && allFrames.Length > targetFrameCount)
        {
            // Optimization: Load only every Nth frame to reduce to target frame count
            // This reduces memory usage while maintaining smooth animation quality
            frames = new Texture2D[targetFrameCount];
            int step = allFrames.Length / targetFrameCount; // Calculate step size
            
            for (int i = 0; i < targetFrameCount; i++)
            {
                int sourceIndex = i * step;
                if (sourceIndex < allFrames.Length)
                {
                    frames[i] = allFrames[sourceIndex];
                }
            }
            
            Debug.Log($"Optimized: Loaded {frames.Length} frames (every {step}th frame) from {path} (original: {allFrames.Length})");
        }
        else
        {
            // Use all frames (original behavior)
            frames = allFrames;
            Debug.Log($"Loaded {frames.Length} animation frames from {path}");
        }
        
        if (frames.Length == 0)
        {
            Debug.LogError($"No frames found in Resources/{path} - Check if the folder exists and contains texture files");
        }
    }

    private void SetupMaterial()
    {
        if (animatedMaterial != null)
        {
            // Configure the material for transparency
            animatedMaterial.EnableKeyword("_ALPHABLEND_ON");
            animatedMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            
            // Set the material to be transparent
            animatedMaterial.SetFloat("_Surface", 1f); // 1 = Transparent
            animatedMaterial.SetFloat("_Mode", 3); // Transparent mode
            
            // Set up blending
            animatedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            animatedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            animatedMaterial.SetInt("_ZWrite", 0);
            animatedMaterial.renderQueue = 3000;
            
            // Ensure alpha channel is respected
            animatedMaterial.SetOverrideTag("RenderType", "Transparent");
            animatedMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back); // Enable back-face culling
            
            // Set color to white with full alpha to not tint the texture
            animatedMaterial.color = new Color(1, 1, 1, 1);
            
            // Set initial frame
            if (neutralFrames != null && neutralFrames.Length > 0)
            {
                animatedMaterial.mainTexture = neutralFrames[0];
            }
        }
    }

    public void StartAnimation()
    {
        StartAnimation("neutral");
    }

    public void StartAnimation(string emotion)
    {
        // Always stop any current animation first
        StopAnimation();

        Texture2D[] targetFrames = null;
        bool shouldLoop = false;

        // Select the appropriate frames and loop setting based on emotion
        switch (emotion.ToLower())
        {
            case "neutral":
                targetFrames = neutralFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            case "happy":
                targetFrames = happyFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            case "angry":
                targetFrames = angryFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            case "sad":
                targetFrames = sadFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            case "scared":
                targetFrames = scaredFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            case "surprised":
                targetFrames = surprisedFrames;
                shouldLoop = GetLoopSetting(emotion);
                break;
            default:
                Debug.LogWarning($"Unknown emotion for animation: {emotion}");
                return;
        }

        if (targetFrames != null && targetFrames.Length > 0)
        {
            currentFrame = 0;
            isPlaying = true;
            animationCoroutine = StartCoroutine(AnimateFrames(targetFrames, shouldLoop));
            Debug.Log($"Started {emotion} animation with {targetFrames.Length} frames, loop={shouldLoop}");
        }
        else
        {
            Debug.LogError($"No frames available for {emotion} animation");
        }
    }
    
    private bool GetLoopSetting(string emotion)
    {
        // Try to get loop setting from FaceStyleManager first
        if (faceStyleManager != null)
        {
            return faceStyleManager.ShouldLoopAnimation(emotion);
        }
        
        // Fallback to legacy settings
        switch (emotion.ToLower())
        {
            case "neutral":
                return loopNeutralAnimation;
            case "happy":
                return loopHappyAnimation;
            case "angry":
                return loopAngryAnimation;
            case "sad":
                return loopSadAnimation;
            case "scared":
                return loopScaredAnimation;
            case "surprised":
                return loopSurprisedAnimation;
            default:
                return true; // Default fallback
        }
    }

    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            Debug.Log("Stopped current animation");
        }
        isPlaying = false;
    }

    private IEnumerator AnimateFrames(Texture2D[] frames, bool loop)
    {
        while (isPlaying)
        {
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                    Debug.Log("Animation loop restarting");
                }
                else
                {
                    isPlaying = false;
                    Debug.Log("Animation completed (not looping)");
                    break;
                }
            }

            if (frames[currentFrame] != null)
            {
                animatedMaterial.mainTexture = frames[currentFrame];
            }
            
            currentFrame++;
            
            // Use optimized frame duration if enabled (4x longer per frame)
            float actualFrameInterval = useOptimizedFrames ? 
                frameInterval * frameDurationMultiplier : 
                frameInterval;
                
            yield return new WaitForSeconds(actualFrameInterval);
        }
    }

    public Material GetAnimatedMaterial()
    {
        return animatedMaterial;
    }
    
    public void ReloadFramesForCurrentStyle()
    {
        if (faceStyleManager != null)
        {
            Debug.Log("FaceAnimationController: Reloading frames for current style");
            LoadAnimationFrames();
        }
    }

    public void SetAlpha(float alpha)
    {
        if (animatedMaterial != null)
        {
            Color color = animatedMaterial.color;
            color.a = alpha;
            animatedMaterial.color = color;
        }
    }

    private void OnDestroy()
    {
        if (animatedMaterial != null)
        {
            Destroy(animatedMaterial);
        }
    }
} 