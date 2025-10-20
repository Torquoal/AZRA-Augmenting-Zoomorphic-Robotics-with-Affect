using UnityEngine;
using System.Collections.Generic;

public class SoundStyleManager : MonoBehaviour
{
    [Header("Sound Style Settings")]
    [SerializeField] private SoundStyle currentStyle = SoundStyle.Current;
    
    [Header("Sound Collections")]
    [SerializeField] private AudioClip[] currentSounds; // Original beep sounds
    [SerializeField] private AudioClip[] animaleseSounds; // Animalese sounds
    [SerializeField] private AudioClip[] catSounds; // Cat sounds
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Current loaded sounds
    private AudioClip[] activeSounds;
    
    public enum SoundStyle
    {
        Current,
        Animalese,
        Cat
    }
    
    void Start()
    {
        LoadSoundStyle(currentStyle);
    }
    
    public void SetSoundStyle(SoundStyle newStyle)
    {
        if (newStyle == currentStyle) return;
        
        if (showDebugLogs)
            Debug.Log($"SoundStyleManager: Switching from {currentStyle} to {newStyle}");
        
        currentStyle = newStyle;
        LoadSoundStyle(currentStyle);
    }
    
    public void SetSoundStyle(int styleIndex)
    {
        if (styleIndex >= 0 && styleIndex < 3)
        {
            SetSoundStyle((SoundStyle)styleIndex);
        }
    }
    
    private void LoadSoundStyle(SoundStyle style)
    {
        switch (style)
        {
            case SoundStyle.Current:
                activeSounds = currentSounds;
                break;
            case SoundStyle.Animalese:
                activeSounds = animaleseSounds;
                break;
            case SoundStyle.Cat:
                activeSounds = catSounds;
                break;
        }
        
        if (showDebugLogs)
            Debug.Log($"SoundStyleManager: Loaded {style} style with {activeSounds?.Length ?? 0} sounds");
    }
    
    public AudioClip GetSoundForEmotion(string emotion)
    {
        if (activeSounds == null || activeSounds.Length == 0)
        {
            Debug.LogWarning($"SoundStyleManager: No sounds loaded for style {currentStyle}");
            return null;
        }
        
        // Map emotions to sound indices (same as AudioController)
        string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry", "peep"};
        
        for (int i = 0; i < emotionArray.Length; i++)
        {
            if (emotionArray[i] == emotion)
            {
                if (i < activeSounds.Length && activeSounds[i] != null)
                {
                    return activeSounds[i];
                }
                else
                {
                    Debug.LogWarning($"SoundStyleManager: No sound found for emotion '{emotion}' in {currentStyle} style");
                    return null;
                }
            }
        }
        
        Debug.LogWarning($"SoundStyleManager: Unknown emotion '{emotion}'");
        return null;
    }
    
    public SoundStyle GetCurrentStyle()
    {
        return currentStyle;
    }
    
    public string GetCurrentStyleName()
    {
        return currentStyle.ToString();
    }
    
    // Context menu for testing
    [ContextMenu("Test Switch to Animalese")]
    private void TestSwitchToAnimalese()
    {
        SetSoundStyle(SoundStyle.Animalese);
    }
    
    [ContextMenu("Test Switch to Cat")]
    private void TestSwitchToCat()
    {
        SetSoundStyle(SoundStyle.Cat);
    }
    
    [ContextMenu("Test Switch to Current")]
    private void TestSwitchToCurrent()
    {
        SetSoundStyle(SoundStyle.Current);
    }
}
