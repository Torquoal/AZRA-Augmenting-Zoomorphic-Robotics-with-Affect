using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class HoverButton : MonoBehaviour
{
    [Header("Face Animation Controller")]
    [SerializeField] private ExperimentalFaceAnimationController faceAnimationController;


    [Header("Mood Trigger")]
    [SerializeField] private ExperimentalEmotionController emotionController;

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
    private int currentCategoryIndex = 0; // Start with a different category for each participant
    private int currentEmotionIndex = 0;

    protected string extendedPath = ""; // Default path for animations

    private bool presentationFinished = false;

    // Add participant number field
    [Header("Experimental Design")]
    [SerializeField] private int participantNumber = 1;

    // Latin square orders for each modality
    private List<int> facialExpressionOrder;
    private List<int> soundOrder;
    // Track current position in Latin square for each modality
    private int[] currentLatinSquarePositions;

    // Initialize Latin square orders based on participant number
    [ContextMenu("Initialize Latin Square Orders")]
    private void InitializeLatinSquareOrders()
    {
        // Generate balanced Latin square for 6 categories (0-5)
        // This creates a balanced design where each category appears in each position equally
        facialExpressionOrder = GenerateBalancedLatinSquare(participantNumber, 6);
        soundOrder = GenerateBalancedLatinSquare(participantNumber + 6, 6); // Offset for different order
        
        Debug.Log($"Participant {participantNumber} - Facial Expression Order: {string.Join(", ", facialExpressionOrder)}");
        Debug.Log($"Participant {participantNumber} - Sound Order: {string.Join(", ", soundOrder)}");

        // Initialize tracking positions for each modality
        currentLatinSquarePositions = new int[modalities.Count];
        for (int i = 0; i < modalities.Count; i++) currentLatinSquarePositions[i] = 0;
    }

    // Generate a balanced Latin square order for given participant and size
    private List<int> GenerateBalancedLatinSquare(int participant, int size)
    {
        List<int> order = new List<int>();
        
        // Base Latin square pattern
        for (int i = 0; i < size; i++)
        {
            int category = (participant + i) % size;
            order.Add(category);
        }
        
        return order;
    }

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

        if (modality == "FacialExpression")
        {
            emotionController.TryDisplayFace(emotion, emotionPath);
        }
        else if (modality == "Sound")
        {
            emotionController.TryDisplaySound(emotion, emotionPath);
        }

        AdvanceIndices();
    }

    private void AdvanceIndices()
    {
        if (facialExpressionOrder == null || soundOrder == null || currentLatinSquarePositions == null)
        {
            InitializeLatinSquareOrders();
        }

        // Get the current order and position for this modality
        List<int> currentOrder = (modalities[currentModalityIndex] == "FacialExpression")
            ? facialExpressionOrder
            : soundOrder;
        int position = currentLatinSquarePositions[currentModalityIndex];
        int categoryIndex = currentOrder[position];
        string category = categories[categoryIndex];
        string modality = modalities[currentModalityIndex];
        string emotion = emotions[currentEmotionIndex];

        string emotionPath = $"Modalities/{modality}/{category}";

        SetExtendedPath(emotionPath);
        faceAnimationController.LoadNewAnimation(extendedPath);

        Debug.Log($"WANTING TO Presenting {modality} stimulus for {emotion} in {category}");

        // Pass category as the task label
        ratingManager.SetCurrentTask(emotion, category);

        // Pass emotion and modality to emotionController, so it can load the correct asset
        // Now advance indices for the next call
        currentEmotionIndex++;

        if (currentEmotionIndex >= emotions.Count)
        {
            currentEmotionIndex = 0;
            currentLatinSquarePositions[currentModalityIndex]++;
            SetExtendedPath(emotionPath);
            faceAnimationController.LoadNewAnimation(extendedPath);

            if (currentLatinSquarePositions[currentModalityIndex] >= categories.Count)
            {
                currentLatinSquarePositions[currentModalityIndex] = 0;
                currentModalityIndex++;

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

    // Demonstration method to show Latin square outputs for different participants
    [ContextMenu("Demonstrate Latin Square")]
    public void DemonstrateLatinSquare()
    {
        Debug.Log("=== Latin Square Demonstration ===");
        
        for (int participant = 1; participant <= 6; participant++)
        {
            List<int> facialOrder = GenerateBalancedLatinSquare(participant, 6);
            List<int> soundOrder = GenerateBalancedLatinSquare(participant + 6, 6);
            
            Debug.Log($"Participant {participant}:");
            Debug.Log($"  Facial Expression Order: {string.Join(", ", facialOrder)}");
            Debug.Log($"  Sound Order: {string.Join(", ", soundOrder)}");
            Debug.Log("---");
        }
    }
}
