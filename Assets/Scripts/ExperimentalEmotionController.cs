using UnityEngine;
using TMPro;
using System.Collections;

using UnityEngine.InputSystem;

public class ExperimentalEmotionController : EmotionController
{
    protected void Update()
    {
        // Only update passive expression if not showing an emotional display and not asleep
        if (!isShowingEmotionalDisplay && !emotionModel.IsAsleep && Time.time - lastPassiveUpdateTime >= passiveUpdateInterval)
        {
            UpdatePassiveExpression();
            lastPassiveUpdateTime = Time.time;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
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
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
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

    
} 