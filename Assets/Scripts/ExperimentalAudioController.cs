using UnityEngine;





public class ExperimentalAudioController : AudioController
{
    [Header("Musical Sounds")]
    [SerializeField] private AudioClip[] musical;  
    [Header("Human Sounds")]
    [SerializeField] private AudioClip[] humanNoises; 
    [Header("Beeps Sounds")]
    [SerializeField] private AudioClip[] beeps;
    [Header("Animalese Sounds")]
    [SerializeField] private AudioClip[] animalese;
    [Header("Cat Sounds")]
    [SerializeField] private AudioClip[] catNoises;


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
                case "Musical":
                    if (musical != null && index < musical.Length)
                    {
                        qooboSpeaker.clip = musical[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "HumanNoises":
                    if (humanNoises != null && index < humanNoises.Length)
                    {
                        qooboSpeaker.clip = humanNoises[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "Beeps":
                    if (beeps != null && index < beeps.Length)
                    {
                        qooboSpeaker.clip = beeps[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "Animalese":
                    if (animalese != null && index < animalese.Length)
                    {
                        qooboSpeaker.clip = animalese[index];
                        qooboSpeaker.Play();
                    }
                    break;
                case "CatNoises":
                    if (catNoises != null && index < catNoises.Length)
                    {
                        qooboSpeaker.clip = catNoises[index];
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