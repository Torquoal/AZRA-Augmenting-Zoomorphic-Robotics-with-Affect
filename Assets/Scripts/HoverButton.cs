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

    [Header("Emotions")]
    [SerializeField] private List<string> emotions = new List<string> { "Excited", "Happy", "Relaxed", "Energetic", "Tired", "Annoyed", "Sad", "Gloomy" };

    private HashSet<string> usedEmotions = new HashSet<string>();


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
                ChangeMoodRandomly();
            }
        }
    }

    private void ChangeMoodRandomly()
    {
        if (emotionController != null && emotions.Count > 0)
        {
            List<string> availableEmotions = emotions.FindAll(e => !usedEmotions.Contains(e));
            if (availableEmotions.Count == 0)
            {
                Debug.Log("HoverButton: All emotions have been used.");
                return;
            }

            string selectedEmotion = availableEmotions[Random.Range(0, availableEmotions.Count)];
            Debug.Log($"HoverButton: Hover time reached. Changing mood to '{selectedEmotion}'.");
            emotionController.TryDisplayEmotion(selectedEmotion, "", true);
            usedEmotions.Add(selectedEmotion);
            moodChanged = true;
            lastChangeTime = Time.time;
            Debug.Log($"HoverButton: '{usedEmotions}' NOW.");
        }
        else
        {
            Debug.LogWarning("HoverButton: EmotionController is not assigned or emotion list is empty.");
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

    // Optional reset, in case you want to allow changing mood again later
    public void ResetMood()
    {
        moodChanged = false;
        lastChangeTime = -999f;
    }
}
