using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource qooboSpeaker;
    [SerializeField] private SoundStyleManager soundStyleManager; // Reference to sound style manager
    
    [Header("Legacy Settings (Deprecated)")]
    [SerializeField] private AudioClip[] beepSounds;  // Legacy - kept for fallback

    public void PlaySound(string emotion)
    {
        AudioClip soundToPlay = null;
        
        // Try to get sound from SoundStyleManager first
        if (soundStyleManager != null)
        {
            soundToPlay = soundStyleManager.GetSoundForEmotion(emotion);
        }
        
        // Fallback to legacy beepSounds if SoundStyleManager fails
        if (soundToPlay == null && beepSounds != null)
        {
            int index = 0;
            string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry", "peep"};

            for (int i = 0; i < emotionArray.Length; i++)
            {
                if (emotionArray[i] == emotion)
                {
                    index = i;
                    break;
                }
            }   

            if (index < beepSounds.Length && beepSounds[index] != null)
            {
                soundToPlay = beepSounds[index];
            }
        }
        
        // Play the sound
        if (qooboSpeaker != null && soundToPlay != null)
        {
            qooboSpeaker.clip = soundToPlay;
            qooboSpeaker.Play();
        }
        else
        {
            Debug.LogWarning($"AudioController: No sound found for emotion '{emotion}'");
        }
    }

    // Convenience methods
    public void PlayHappySound() => PlaySound("happy");
    public void PlaySadSound() => PlaySound("sad");
    public void PlayScaredSound() => PlaySound("scared");
    public void PlaySurprisedSound() => PlaySound("surprised");
    public void PlayAngrySound() => PlaySound("angry");
    public void PlayPeepSound() => PlaySound("peep");
} 