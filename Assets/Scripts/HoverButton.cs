using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class HoverButton : MonoBehaviour
{
    [Header("Face Animation Controller")]
    [SerializeField] private FaceAnimationController faceAnimationController;


    [Header("Mood Trigger")]
    [SerializeField] private EmotionController emotionController;

    [Header("Hover Timing")]
    [SerializeField] private float hoverTime = 5f;
    [SerializeField] private float cooldown = 0.5f;

    [Header("Hand Path Matching")]
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";

    private bool isHovering = false;
    private float hoverStartTime = 0f;
    private bool moodChanged = false;
    private float lastChangeTime = -999f;

    [Header("Rating Manager")]
    [SerializeField] private RatingManager ratingManager;

    [Header("Stimuli Settings")]
    // Define your modalities explicitly
    [SerializeField]
    private List<string> modalities = new List<string> { "FacialExpression", "Sound" };

    // Define your categories explicitly
    [SerializeField]
    private List<string> categories = new List<string> 
    {
        "AnthroAbs",
        "AnthroManga",
        "MechaEmoji",
        "MechaScreenFace",
        "ZoomorphicAbs",
        "ZoomorphicReal"
    };

    // Define your emotions explicitly (use exactly these 5 for your experiment)
    [SerializeField]
    private List<string> emotions = new List<string> { "Happy", "Sad", "Angry", "Fear", "Surprise" };

    // Indices to track progress
    private int currentModalityIndex = 0;
    private int currentCategoryIndex = 0;
    private int currentEmotionIndex = 0;

    protected string extendedPath = "Modalities/FacialExpression/AnthroAbs"; // Default path for animations

    private bool presentationFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (IsHand(other))
        {
            Debug.Log("HoverButton: Hand entered area.");
            isHovering = true;
            hoverStartTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsHand(other))
        {
            Debug.Log("HoverButton: Hand exited area.");
            isHovering = false;
            hoverStartTime = 0f;
            moodChanged = false;
        }
    }

    private void Update()
    {
        if (isHovering && !moodChanged)
        {
            if (Time.time - hoverStartTime >= hoverTime && Time.time - lastChangeTime >= cooldown)
            {
                ChangeMoodSequentially();
            }
        }
    }

    private void ChangeMoodSequentially()
    {
        if (presentationFinished)
        {
            Debug.Log("All stimuli presented.");
            return;
        }

        if (emotionController == null || ratingManager == null)
        {
            Debug.LogWarning("EmotionController or RatingManager is missing.");
            return;
        }

        string modality = modalities[currentModalityIndex];
        string category = categories[currentCategoryIndex];
        string emotion = emotions[currentEmotionIndex];

        Debug.Log($"Presenting {modality} stimulus for {emotion} in {category}");

        // Pass category as the task label
        ratingManager.SetCurrentTask(emotion, category);

        // Pass emotion and modality to emotionController, so it can load the correct asset
        
        string emotionPath = $"Modalities/{modality}/{category}/{emotion}";

        emotionController.TryDisplayEmotion(emotion, emotionPath, true);

        AdvanceIndices();
    }

    private void AdvanceIndices()
    {

        currentEmotionIndex++;

        if (currentEmotionIndex >= emotions.Count)
        {
            currentEmotionIndex = 0;
            currentCategoryIndex++;
            SetExtendedPath($"Modalities/{modalities[currentModalityIndex]}/{categories[currentCategoryIndex]}");
            faceAnimationController.LoadNewAnimation(extendedPath);

            if (currentCategoryIndex >= categories.Count)
            {
                currentCategoryIndex = 0;
                currentModalityIndex++;
                SetExtendedPath($"Modalities/{modalities[currentModalityIndex]}/{categories[currentCategoryIndex]}");
                faceAnimationController.LoadNewAnimation(extendedPath);

                if (currentModalityIndex >= modalities.Count)
                {
                    presentationFinished = true;
                    Debug.Log("All categories and emotions for all modalities have been presented.");
                    return;
                }
            }
        }

        moodChanged = true;
        lastChangeTime = Time.time;
    }

    private bool IsHand(Collider col)
    {
        string fullPath = GetGameObjectPath(col.gameObject);
        return fullPath.Contains(leftHandPath) || fullPath.Contains(rightHandPath);
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }


    [ContextMenu("Get Extended Path")]
    public string GetExtendedPath()
    {
        return extendedPath;
    }

    public void SetExtendedPath(string path)
    {
        Debug.Log($"Setting extended path to: {path}");
        extendedPath = path;
    }

    public void ResetMood()
    {
        moodChanged = false;
        lastChangeTime = -999f;
    }
}
