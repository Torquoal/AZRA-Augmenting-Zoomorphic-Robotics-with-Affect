using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class HoverButton : MonoBehaviour
{
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
    private HashSet<string> usedEmotions = new HashSet<string>();

    [Header("Rating Manager")]
    [SerializeField] private RatingManager ratingManager;

    [Header("Stimuli Settings")]
    [SerializeField] private int totalCategories = 6;
    [SerializeField] private List<string> emotions = new List<string> { "Excited", "Happy", "Relaxed", "Energetic", "Tired", "Annoyed", "Sad", "Gloomy" };
    private int currentCategory = 0;
    private int currentEmotionIndex = 0;
    private enum StimulusType { Face, Sound }
    private StimulusType currentStimulusType = StimulusType.Face;
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

    // private void ChangeMoodRandomly()
    // {
    //     if (emotionController != null && emotions.Count > 0)
    //     {
    //         List<string> availableEmotions = emotions.FindAll(e => !usedEmotions.Contains(e));
    //         if (availableEmotions.Count == 0)
    //         {
    //             Debug.Log("HoverButton: All emotions have been used.");
    //             return;
    //         }

    //         string selectedEmotion = availableEmotions[Random.Range(0, availableEmotions.Count)];
    //         Debug.Log($"HoverButton: Hover time reached. Changing mood to '{selectedEmotion}'.");
    //         ratingManager.SetCurrentTask(selectedEmotion, "button");
    //         emotionController.TryDisplayEmotion(selectedEmotion, "", true);
    //         usedEmotions.Add(selectedEmotion);
    //         moodChanged = true;
    //         lastChangeTime = Time.time;
    //         Debug.Log($"HoverButton: '{usedEmotions}' NOW.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("HoverButton: EmotionController is not assigned or emotion list is empty.");
    //     }
    // }

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

        string emotion = emotions[currentEmotionIndex];
        string categoryLabel = $"Category_{currentCategory + 1}";
        string stimulusLabel = currentStimulusType.ToString();

        Debug.Log($"Presenting {stimulusLabel} stimulus for {emotion} in {categoryLabel}");

        ratingManager.SetCurrentTask(emotion, categoryLabel);
        emotionController.TryDisplayEmotion(emotion, stimulusLabel, true);

        AdvanceIndex();
    }




private void AdvanceIndex()
{
    currentEmotionIndex++;

    if (currentEmotionIndex >= emotions.Count)
    {
        currentEmotionIndex = 0;
        currentCategory++;

        if (currentCategory >= totalCategories)
        {
            currentCategory = 0;

            // Switch to next stimulus type
            if (currentStimulusType == StimulusType.Face)
            {
                currentStimulusType = StimulusType.Sound;
            }
            else
            {
                presentationFinished = true;
                Debug.Log("All categories and emotions for both Face and Sound have been presented.");
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

    // Optional reset, in case you want to allow changing mood again later
    public void ResetMood()
    {
        moodChanged = false;
        lastChangeTime = -999f;
    }
}
