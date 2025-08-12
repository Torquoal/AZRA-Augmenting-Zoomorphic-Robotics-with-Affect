using UnityEngine;





public class ExperimentalAudioController : AudioController
{
    [Header("AnthroManga Sounds")]
    [SerializeField] private AudioClip[] anthroMangaBeepSounds;  // Drag your AnthroManga beep sound files here
    [Header("MechaEmoji Sounds")]
    [SerializeField] private AudioClip[] mechaEmojiBeepSounds;  // Drag your MechaEmoji beep sound files here
    [Header("MechaScreenFace Sounds")]
    [SerializeField] private AudioClip[] mechaScreenFaceBeepSounds;
    [Header("ZoomorphicAbs Sounds")]
    [SerializeField] private AudioClip[] zoomorphicAbsBeepSounds;
    [Header("ZoomorphicReal Sounds")]
    [SerializeField] private AudioClip[] zoomorphicRealBeepSounds;


    public void PlaySound(string emotion, string category)
    {
        int index = 0;
        string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry", "peep"};

        emotion = emotion.ToLower();
        for (int i = 0; i < emotionArray.Length; i++)
        {
            if (emotionArray[i] == emotion)
            {
                Debug.Log($"Found matching emotion: {emotion} at index {i}");
                index = i;
            }
        }

        if (qooboSpeaker != null && beepSounds != null && index < beepSounds.Length && beepSounds[index] != null)
        {
            switch (category)
            {
                case "AnthroManga":
                    if (anthroMangaBeepSounds != null && index < anthroMangaBeepSounds.Length)
                    {
                        qooboSpeaker.clip = anthroMangaBeepSounds[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "MechaEmoji":
                    if (mechaEmojiBeepSounds != null && index < mechaEmojiBeepSounds.Length)
                    {
                        qooboSpeaker.clip = mechaEmojiBeepSounds[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "MechaScreenFace":
                    if (mechaScreenFaceBeepSounds != null && index < mechaScreenFaceBeepSounds.Length)
                    {
                        qooboSpeaker.clip = mechaScreenFaceBeepSounds[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "ZoomorphicAbs":
                    if (zoomorphicAbsBeepSounds != null && index < zoomorphicAbsBeepSounds.Length)
                    {
                        qooboSpeaker.clip = zoomorphicAbsBeepSounds[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "ZoomorphicReal":
                    if (zoomorphicRealBeepSounds != null && index < zoomorphicRealBeepSounds.Length)
                    {
                        qooboSpeaker.clip = zoomorphicRealBeepSounds[index];
                        qooboSpeaker.Play();
                    }
                    break;
            }

            // qooboSpeaker.clip = beepSounds[index];
            // qooboSpeaker.Play();
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