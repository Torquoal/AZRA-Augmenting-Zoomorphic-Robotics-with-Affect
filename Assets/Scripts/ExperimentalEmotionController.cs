using UnityEngine;
using TMPro;
using System.Collections;

using UnityEngine.InputSystem;



public class ExperimentalEmotionController : EmotionController
{
    [Header("Passive Expression Settings")]
    [SerializeField] private bool disablePassiveExpression = false;


    protected void Update()
    {
        // Only update passive expression if not showing an emotional display and not asleep
        if (!disablePassiveExpression && !isShowingEmotionalDisplay && !emotionModel.IsAsleep && Time.time - lastPassiveUpdateTime >= passiveUpdateInterval)
        {
            UpdatePassiveExpression();
            lastPassiveUpdateTime = Time.time;
        }

        if (Keyboard.current != null && Keyboard.current.ctrlKey.wasPressedThisFrame)
        {
            emotionModel.WakeUp();
            Debug.Log("SMILE space pressed ");
            TryDisplayEmotion("sad", "button", true); // Bypass cooldown for button triggers
        }

    }

    protected void TriggerEmotionFromButton(string emotion)
    {
        Debug.LogError("SMILE WORKING working .");
        TryDisplayEmotion(emotion, "button", true); // Bypass cooldown for button triggers
    }


    public bool TryDisplayEmotion(string displayString, string triggerEvent, bool bypassCooldown = false)
    {
        if (!sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return false;
        }

        // If asleep, only allow special wake-up events
        if (emotionModel.IsAsleep && displayString != "wake")
        {
            if (showDebugText)
                Debug.Log("Cannot display emotion - robot is asleep");
            return false;
        }

        // Always allow neutral emotions with bypass, and check cooldown for others
        if (!bypassCooldown && displayString.ToLower() != "neutral" && !CanDisplayEmotion())
        {
            float remainingCooldown = displayCooldown - (Time.time - lastDisplayTime);
            if (showDebugText && remainingCooldown > 0)
                Debug.Log($"Cannot display emotion - in cooldown. Cooldown remaining: {Mathf.Max(0, remainingCooldown):F2}s");
            else if (showDebugText)
                Debug.Log("Cannot display emotion - already showing an emotion");
            return false;
        }

        DisplayEmotionInternal(displayString, triggerEvent);
        return true;
    }
    public void TryDisplayFace(string displayString, string triggerEvent)
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return;
        }

        if (showDebugText)
            Debug.Log($"Emotion Controller: Displaying emotion: {displayString} from trigger: {triggerEvent}");

        currentDisplayString = displayString;
        currentTriggerEvent = triggerEvent;

        // Cancel any pending reset if we're going to sleep
        if (displayString.ToLower() == "sleep" || emotionModel.IsAsleep)
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }

        // Handle the emotion display
        switch (displayString.ToLower())
        {
            case "sleep":
                //sceneController.SetFaceExpression("sleepy");
                sceneController.ShowThought("sleep");
                sceneController.HideLightSphere();
                break;

            case "happy":
                sceneController.SetFaceExpression("happy");
                break;


            case "surprised":
                sceneController.SetFaceExpression("surprised");
                break;

            case "angry":
                sceneController.SetFaceExpression("angry");
                break;

            case "neutral":
                sceneController.HideLightSphere();
                sceneController.HideThought();
                sceneController.TailsEmotion("happy");
                break;

            case "sad":
                sceneController.SetFaceExpression("sad");
                break;

            case "scared":
                sceneController.SetFaceExpression("scared");
                break;

            default:
                Debug.LogWarning($"Unknown display string: {displayString}, defaulting to neutral");
                sceneController.HideLightSphere();
                sceneController.HideThought();
                sceneController.TailsEmotion("happy");
                break;
        }
    }

    public void TryDisplaySound(string displayString, string triggerEvent)
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return;
        }

        if (showDebugText)
            Debug.Log($"Emotion Controller: Displaying emotion: {displayString} from trigger: {triggerEvent}");

        currentDisplayString = displayString;
        currentTriggerEvent = triggerEvent;

        // Cancel any pending reset if we're going to sleep
        if (displayString.ToLower() == "sleep" || emotionModel.IsAsleep)
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }

        // Handle the emotion display
        switch (displayString.ToLower())
        {
            case "sleep":
                //sceneController.SetFaceExpression("sleepy");
                sceneController.ShowThought("sleep");
                sceneController.HideLightSphere();
                break;

            case "happy":
                sceneController.PlaySound("happy");
                break;


            case "surprised":
                sceneController.PlaySound("surprised");
                break;

            case "angry":
                sceneController.PlaySound("angry");
                break;

            case "neutral":
                sceneController.HideLightSphere();
                sceneController.HideThought();
                sceneController.TailsEmotion("happy");
                break;

            case "sad":
                sceneController.PlaySound("sad");
                break;

            case "scared":
                sceneController.PlaySound("scared");
                break;

            default:
                Debug.LogWarning($"Unknown display string: {displayString}, defaulting to neutral");
                sceneController.HideLightSphere();
                sceneController.HideThought();
                sceneController.TailsEmotion("happy");
                break;
        }
    }
}