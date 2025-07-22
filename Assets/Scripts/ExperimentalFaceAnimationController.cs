using UnityEngine;
using System.Collections;
using System.Linq;

public class ExperimentalFaceAnimationController : FaceAnimationController
{
    [Header("Hover Button")]
    [SerializeField] private HoverButton hoverButton; // Reference to the HoverButton script
    private string extendedPath = ""; // Default path for animations

    private void Start()
    {
        frameInterval = 1f / frameRate;

        extendedPath = hoverButton.GetExtendedPath();
        LoadAnimationFrames(extendedPath);

        // Create a new material instance
        animatedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        SetupMaterial();
    }

    private new void LoadAnimationFrames(string extendedPath)
    {
        // Load neutral, happy, angry, sad, scared, and surprised animation frames
        LoadFramesForEmotion(extendedPath, neutralLoopPath, ref neutralFrames);
        LoadFramesForEmotion(extendedPath, happyLoopPath, ref happyFrames);
        LoadFramesForEmotion(extendedPath, angryLoopPath, ref angryFrames);
        LoadFramesForEmotion(extendedPath, sadLoopPath, ref sadFrames);
        LoadFramesForEmotion(extendedPath, scaredLoopPath, ref scaredFrames);
        LoadFramesForEmotion(extendedPath, surprisedLoopPath, ref surprisedFrames);

        // Future emotion frame loading
        /*
        LoadFramesForEmotion(happyPath, ref happyFrames);
        LoadFramesForEmotion(sadPath, ref sadFrames);
        LoadFramesForEmotion(angryPath, ref angryFrames);
        LoadFramesForEmotion(scaredPath, ref scaredFrames);
        LoadFramesForEmotion(surprisedPath, ref surprisedFrames);
        */
    }

    private new void LoadFramesForEmotion(string extendedPath, string path, ref Texture2D[] frames)
    {
        // Load all textures from the Resources folder

        string fullPath = $"{extendedPath}/{path}";
        Debug.Log($"LIFE animation frames from: {fullPath}");
        Object[] loadedObjects = Resources.LoadAll(fullPath, typeof(Texture2D));

        // Convert to Texture2D array and sort by name to ensure correct order
        frames = loadedObjects
            .Cast<Texture2D>()
            .OrderBy(tex => tex.name)
            .ToArray();

        Debug.Log($"Loaded {frames.Length} animation frames from {fullPath}");

        if (frames.Length == 0)
        {
            Debug.LogWarning($"No frames found in Resources/{fullPath}");
        }
    }

    private new void SetupMaterial()
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

    public new void StartAnimation()
    {
        StartAnimation("neutral");
    }

    public new void StartAnimation(string emotion)
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
                shouldLoop = loopNeutralAnimation;
                break;
            case "happy":
                targetFrames = happyFrames;
                shouldLoop = loopHappyAnimation;
                break;
            case "angry":
                targetFrames = angryFrames;
                shouldLoop = loopAngryAnimation;
                break;
            case "sad":
                targetFrames = sadFrames;
                shouldLoop = loopSadAnimation;
                break;
            case "scared":
                targetFrames = scaredFrames;
                shouldLoop = loopScaredAnimation;
                break;
            case "surprised":
                targetFrames = surprisedFrames;
                shouldLoop = loopSurprisedAnimation;
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

    public new void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            Debug.Log("Stopped current animation");
        }
        isPlaying = false;
    }

    private new IEnumerator AnimateFrames(Texture2D[] frames, bool loop)
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
            yield return new WaitForSeconds(frameInterval);
        }
    }

    public new Material GetAnimatedMaterial()
    {
        return animatedMaterial;
    }

    public new void SetAlpha(float alpha)
    {
        if (animatedMaterial != null)
        {
            Color color = animatedMaterial.color;
            color.a = alpha;
            animatedMaterial.color = color;
        }
    }

    private new void OnDestroy()
    {
        if (animatedMaterial != null)
        {
            Destroy(animatedMaterial);
        }
    }
    
    public new void LoadNewAnimation(string newPath)
    {
        extendedPath = newPath;
        LoadAnimationFrames(extendedPath);
        Debug.Log($"Loaded new animation frames from: {newPath}");
    }



} 