using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class HoverButton : MonoBehaviour
{
    [Header("Face Animation Controller")]
    [SerializeField] private ExperimentalFaceAnimationController faceAnimationController;

    [Header("Mood Trigger")]
    [SerializeField] private ExperimentalEmotionController emotionController;
    [SerializeField] private FaceController faceController;

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

    [Header("Audio Controller")]
    [SerializeField] private ExperimentalAudioController audiocontroller;

    [Header("Scene Controller")]
    [SerializeField] private SceneController sceneController;

    [Header("Experiment Settings")]
    [SerializeField] private string selectedModality = "FacialExpression"; // Choose: "FacialExpression" or "Sound"
    
    // Define your categories explicitly - different for each modality
    [SerializeField]
    private List<string> faceCategories = new List<string> 
    {
        "AnthroAbs",
        "AnthroManga",
        "MechaEmoji",
        "MechaScreenFace",
        "ZoomorphicAbs",
        "ZoomorphicReal"
    };

    [SerializeField]
    private List<string> soundCategories = new List<string> 
    {
        "HumanNoises",
        "Animalese",
        "Beeps",
        "CatNoises",
        "Musical"
    };

    // Define your emotions explicitly (use exactly these 5 for your experiment)
    [SerializeField]
    private List<string> emotions = new List<string> { "Happy", "Sad", "Angry", "Scared", "Surprised" };

    // Indices to track progress
    private int currentCategoryIndex = 0; // Start with a different category for each participant
    private int currentEmotionIndex = 0;

    protected string extendedPath = ""; // Default path for animations

    private bool presentationFinished = false;
    private bool isAnimationLoaded = false; // Track if animation is loaded initially

    private int RatingDelay = 7; 

    private List<int> shuffledEmotionOrder = null;
    private int emotionPosition = 0;

    // Add participant number field
    [Header("Experimental Design")]
    [SerializeField] private int participantNumber = 1;

    // Latin square order for the selected modality
    private List<int> categoryOrder;
    // Track current position in Latin square
    private int currentLatinSquarePosition;

    // Initialize Latin square order based on participant number
    [ContextMenu("Initialize Latin Square Order")]
    private void InitializeLatinSquareOrder()
    {
        // Get the appropriate category count for the selected modality
        int categoryCount = GetCurrentCategoryCount();
        
        // Generate balanced Latin square for the appropriate number of categories
        categoryOrder = GenerateBalancedLatinSquare(Mathf.Max(0, participantNumber - 1), categoryCount);
        
        Debug.Log($"Participant {participantNumber} - {selectedModality} Category Order: {string.Join(", ", categoryOrder)}");

        // Initialize tracking position
        currentLatinSquarePosition = 0;
    }

    // Get the current category count based on selected modality
    private int GetCurrentCategoryCount()
    {
        return (selectedModality == "FacialExpression") ? faceCategories.Count : soundCategories.Count;
    }

    // Get the current category list based on selected modality
    private List<string> GetCurrentCategoryList()
    {
        return (selectedModality == "FacialExpression") ? faceCategories : soundCategories;
    }

    // Generate a balanced Latin square order for given participant and size
    private List<int> GenerateBalancedLatinSquare(int participant, int size)
    {
        List<int> order = new List<int>(size);
        int j = 0;
        int h = 0;
        for (int i = 0; i < size; ++i)
        {
            int val;
            if (i < 2 || i % 2 != 0)
            {
                val = j++;
            }
            else
            {
                val = size - h - 1;
                ++h;
            }

            int idx = (val + participant) % size;
            order.Add(idx);
        }

        if (size % 2 != 0 && participant % 2 != 0)
        {
            order.Reverse();
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
        if (isHovering && !moodChanged && (ratingManager == null || !ratingManager.taskRunning) && sceneController.IsWakeUpComplete())
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

        if (ratingManager.taskRunning)
        {
            Debug.Log("Rating UI active; deferring mood change.");
            return;
        }

        AdvanceIndicesAndDisplayStimulus();
    }

    private IEnumerator DelayedRating(string emotion, string category, string modality)
    {
        yield return new WaitForSeconds(RatingDelay);
        ratingManager.SetCurrentTask(emotion, category, modality);
    }

    private void InitializeIfNeeded()
    {
        if (categoryOrder == null)
        {
            InitializeLatinSquareOrder();
        }
    }

    private string GetCurrentCategory()
    {
        int categoryIndex = categoryOrder[currentLatinSquarePosition];
        return GetCurrentCategoryList()[categoryIndex];
    }

    private string GetNextEmotion()
    {
        if (shuffledEmotionOrder == null || emotionPosition >= emotions.Count)
        {
            shuffledEmotionOrder = Enumerable.Range(0, emotions.Count).ToList();
            ShuffleList(shuffledEmotionOrder);
            emotionPosition = 0;
            isAnimationLoaded = false;
        }

        int emotionIndex = shuffledEmotionOrder[emotionPosition];
        return emotions[emotionIndex];
    }

    private void HandleStimulusPresentation(string emotion, string category)
    {
        if (!isAnimationLoaded && selectedModality == "FacialExpression")
        {
            string emotionPath = $"Modalities/{selectedModality}/{category}";
            SetExtendedPath(emotionPath);
            faceAnimationController.LoadNewAnimation(extendedPath);
            isAnimationLoaded = true;
        }

        if (selectedModality == "FacialExpression")
        {

            Debug.Log($"Displaying facial expression for emotion: {emotion} in category: {category} selectedModality: {selectedModality}");

            // Make face visible for facial expression modality
            if (faceController != null)
            {
                faceController.SetFaceVisibility(1f);
            }
            emotionController.TryDisplayFace(emotion, "");
        }
        else if (selectedModality == "Sound")
        {
            // Make face invisible for sound modality
            if (faceController != null)
            {
                faceController.SetFaceVisibility(0f);
            }
            Debug.Log($"Playing sound for emotion: {emotion} in category: {category}");
            audiocontroller.PlaySound(emotion, category);
        }
    }

    private void AdvanceStateIfNeeded()
    {
        emotionPosition++;

        if (emotionPosition >= emotions.Count)
        {
            emotionPosition = 0;
            shuffledEmotionOrder = null;
            currentLatinSquarePosition++;
            isAnimationLoaded = false;

            if (currentLatinSquarePosition >= GetCurrentCategoryCount())
            {
                presentationFinished = true;
                Debug.Log($"All categories and emotions for {selectedModality} modality have been presented.");
            }
        }
    }

    private void AdvanceIndicesAndDisplayStimulus()
    {
        InitializeIfNeeded();

        string category = GetCurrentCategory();
        string emotion = GetNextEmotion();

        Debug.Log($"Presenting {selectedModality} stimulus for {emotion} in {category}");

        HandleStimulusPresentation(emotion, category);
        StartCoroutine(DelayedRating(emotion, category, selectedModality));

        AdvanceStateIfNeeded();

        moodChanged = true;
        lastChangeTime = Time.time;
    }

    void Start()
    {
        ratingManager.SetParticipantID(participantNumber);
        ratingManager.SetEmotionCount(emotions.Count);
        
        // Make face invisible after wakeup to avoid white screen
        if (faceController != null)
        {
            faceController.SetFaceVisibility(0f);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
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
        
        // Show for both modalities
        string[] modalities = { "FacialExpression", "Sound" };
        
        for (int participant = 1; participant <= 6; participant++)
        {
            Debug.Log($"Participant {participant}:");
            
            foreach (string modality in modalities)
            {
                int categoryCount = (modality == "FacialExpression") ? faceCategories.Count : soundCategories.Count;
                List<int> categoryOrder = GenerateBalancedLatinSquare(participant - 1, categoryCount);
                
                Debug.Log($"  {modality} Category Order ({categoryCount} categories): {string.Join(", ", categoryOrder)}");
            }
            Debug.Log("---");
        }
    }
}
