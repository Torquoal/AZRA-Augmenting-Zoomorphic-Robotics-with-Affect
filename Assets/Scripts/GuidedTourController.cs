using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuidedTourController : MonoBehaviour
{
    [Header("Tour Panel References")]
    [SerializeField] private GameObject tourPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private UnityEngine.UI.Button nextButton;
    
    [Header("Tour Settings")]
    // Tour settings removed - not currently used
    
    [Header("References")]
    [SerializeField] private StudyLogger studyLogger;
    [SerializeField] private WelcomeSequenceController welcomeController;
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private MetricsMenuController metricsMenuController;
    
    // Tour state
    private bool isTourActive = false;
    private int currentStep = 0;
    private List<TourStep> tourSteps = new List<TourStep>();
    private List<TourStep> randomizedSteps = new List<TourStep>();
    
    // Button debouncing to prevent double-clicks
    private float lastButtonClickTime = 0f;
    private const float BUTTON_DEBOUNCE_TIME = 0.5f;
    
    // Store original interaction states to restore after tour
    private bool originalGazeToggle = true;
    private bool originalDistanceToggle = true;
    private bool originalSpeechToggle = true;
    
    // Tour step data
    [System.Serializable]
    public class TourStep
    {
        public string title;
        public string description;
        public string interactionType; // For detection and logging
    }
    
    void Start()
    {
        InitializeTourSteps();
        SetupTourPanel();
        
        // Subscribe to welcome sequence completion
        if (welcomeController != null)
        {
            // We'll need to add a completion event to WelcomeSequenceController
            // For now, we'll start the tour manually
        }
    }
    
    void InitializeTourSteps()
    {
        // Create the 6 main interaction steps
        tourSteps.Add(new TourStep
        {
            title = "Stroking",
            description = "Try stroking Qoobo gently from the front and back and vice versa. See how it responds to your touch.",
            interactionType = "stroking"
        });
        
        tourSteps.Add(new TourStep
        {
            title = "Distance",
            description = "Move far away from Qoobo. Notice how the robot reacts when you get too far away.",
            interactionType = "distance"
        });
        
        tourSteps.Add(new TourStep
        {
            title = "Gaze",
            description = "Look directly at Qoobo's face and wait for it to respond, then try looking in the opposite direction and see how it reacts.",
            interactionType = "gaze"
        });
        
        tourSteps.Add(new TourStep
        {
            title = "Speech",
            description = "Try saying some words to Qoobo, such as Hello, Sad, Happy, Food, etc. The robot can hear and respond to your voice commands.",
            interactionType = "speech"
        });
        
        tourSteps.Add(new TourStep
        {
            title = "Feeding",
            description = "Use the menu to spawn donut in the scene and try to feed it to Qoobo.",
            interactionType = "feeding"
        });
        
        tourSteps.Add(new TourStep
        {
            title = "Ghost Mode",
            description = "Use the menu to toggle 'ghost mode' to make Qoobo become transparent and follow you around. Press it again to return to normal.",
            interactionType = "ghost_mode"
        });
        
        // Randomize the order
        RandomizeTourSteps();
    }
    
    void RandomizeTourSteps()
    {
        randomizedSteps.Clear();
        List<TourStep> tempList = new List<TourStep>(tourSteps);
        
        Debug.Log($"GuidedTour: Starting with {tempList.Count} original steps");
        
        while (tempList.Count > 0)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            randomizedSteps.Add(tempList[randomIndex]);
            Debug.Log($"GuidedTour: Added step {randomizedSteps.Count}: {tempList[randomIndex].title}");
            tempList.RemoveAt(randomIndex);
        }
        
        Debug.Log($"GuidedTour: Randomized {randomizedSteps.Count} tour steps");
    }
    
    void SetupTourPanel()
    {
        if (tourPanel != null)
        {
            tourPanel.SetActive(true); // Start with panel visible
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }
        
        // Show initial welcome message
        ShowWelcomeMessage();
    }
    
    void ShowWelcomeMessage()
    {
        if (titleText != null) titleText.text = "Welcome to AZRA!";
        if (descriptionText != null) descriptionText.text = "Welcome! Take a moment to get familiar with Qoobo. When you're ready, we'll guide you through the different ways you can interact with Qoobo.";
        if (nextButton != null) nextButton.gameObject.SetActive(true);
    }
    
    public void StartTour()
    {
        if (isTourActive) return;
        
        Debug.Log("GuidedTour: Tour system initialized - panel is already visible");
        LogTourEvent("Tour System Ready", "Panel visible, waiting for user to start");
        
        // Store original interaction states and disable interfering interactions
        StoreOriginalInteractionStates();
        DisableInterferingInteractions();
        
        // Tour is ready but not active until user presses Next
        isTourActive = false;
        currentStep = -1; // Will start with general familiarization when Next is pressed
    }
    
    
    void ShowCurrentStep()
    {
        if (!isTourActive) return;
        
        Debug.Log($"GuidedTour: ShowCurrentStep - currentStep: {currentStep}, randomizedSteps.Count: {randomizedSteps.Count}");
        
        if (currentStep == -1)
        {
            // General familiarization step
            ShowGeneralFamiliarization();
        }
        else if (currentStep < randomizedSteps.Count)
        {
            // Show current interaction step
            ShowInteractionStep(randomizedSteps[currentStep]);
        }
        else
        {
            // Tour completion
            Debug.Log("GuidedTour: Tour completion reached");
            ShowTourCompletion();
        }
    }
    
    void ShowGeneralFamiliarization()
    {
        Debug.Log("GuidedTour: Showing General Familiarisation step");
        if (tourPanel != null) 
        {
            tourPanel.SetActive(true);
            Debug.Log("GuidedTour: Tour panel activated");
        }
        else
        {
            Debug.LogError("GuidedTour: Tour panel is null!");
        }
        
        if (titleText != null) titleText.text = "General Familiarisation";
        if (descriptionText != null) descriptionText.text = "Welcome! Take a moment to get familiar with Qoobo. When you're ready, we'll guide you through the different ways you can interact with Qoobo.";
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        
        LogTourEvent("General Familiarisation", "Step shown");
    }
    
    void ShowInteractionStep(TourStep step)
    {
        if (tourPanel != null) tourPanel.SetActive(true);
        if (titleText != null) titleText.text = step.title;
        if (descriptionText != null) descriptionText.text = step.description;
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        
        // Enable the specific interaction for this step
        EnableCurrentStepInteraction();
        
        LogTourEvent($"Tour Step: {step.title}", step.interactionType);
    }
    
    void ShowTourCompletion()
    {
        if (tourPanel != null) tourPanel.SetActive(true);
        if (titleText != null) titleText.text = "Tour Complete!";
        if (descriptionText != null) descriptionText.text = "Congratulations! You've experienced all the different ways to interact with Qoobo. You can now explore freely and use any of the features you've learned about. Take your time to experiment and enjoy your time with the robot.";
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        
        // Restore original interaction states when tour is complete
        RestoreOriginalInteractionStates();
        
        LogTourEvent("Tour Complete", "All steps completed");
        isTourActive = false;
    }
    
    [ContextMenu("Next Step")]
    public void NextStep()
    {
        // Check debouncing to prevent double-clicks
        if (Time.time - lastButtonClickTime < BUTTON_DEBOUNCE_TIME)
        {
            Debug.Log($"GuidedTour: Next button on cooldown, please wait {BUTTON_DEBOUNCE_TIME - (Time.time - lastButtonClickTime):F1}s");
            return;
        }
        
        lastButtonClickTime = Time.time;
        
        // If tour is not active yet, start it now
        if (!isTourActive)
        {
            Debug.Log("GuidedTour: User pressed Next - starting tour now");
            isTourActive = true;
            LogTourEvent("Tour Started", "User initiated with Next button");
        }
        
        currentStep++;
        LogTourEvent("Next Button", $"Advanced to step {currentStep + 1}");
        ShowCurrentStep();
    }
    
    public void CompleteCurrentStep()
    {
        if (!isTourActive || currentStep < 0 || currentStep >= randomizedSteps.Count) return;
        
        TourStep currentStepData = randomizedSteps[currentStep];
        LogTourEvent($"Interaction Completed: {currentStepData.title}", currentStepData.interactionType);
        
        // Auto-advance after a short delay
        StartCoroutine(AutoAdvanceAfterDelay());
    }
    
    IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        NextStep();
    }
    
    void LogTourEvent(string eventType, string details)
    {
        if (studyLogger != null)
        {
            studyLogger.LogEmotionalResponse("guided_tour", $"guided_tour {eventType}: {details}");
        }
        Debug.Log($"GuidedTour: {eventType} - {details}");
    }
    
    // Public methods for interaction detection
    public void OnStrokingDetected()
    {
        if (IsCurrentStep("stroking"))
        {
            CompleteCurrentStep();
        }
    }
    
    public void OnDistanceDetected()
    {
        if (IsCurrentStep("distance"))
        {
            CompleteCurrentStep();
        }
    }
    
    public void OnGazeDetected()
    {
        if (IsCurrentStep("gaze"))
        {
            CompleteCurrentStep();
        }
    }
    
    public void OnSpeechDetected()
    {
        if (IsCurrentStep("speech"))
        {
            CompleteCurrentStep();
        }
    }
    
    public void OnFeedingDetected()
    {
        if (IsCurrentStep("feeding"))
        {
            CompleteCurrentStep();
        }
    }
    
    public void OnGhostModeDetected()
    {
        if (IsCurrentStep("ghost_mode"))
        {
            CompleteCurrentStep();
        }
    }
    
    bool IsCurrentStep(string interactionType)
    {
        if (currentStep < 0 || currentStep >= randomizedSteps.Count) return false;
        return randomizedSteps[currentStep].interactionType == interactionType;
    }
    
    // Interaction management methods using MetricsMenuController
    void StoreOriginalInteractionStates()
    {
        if (metricsMenuController != null)
        {
            originalGazeToggle = metricsMenuController.IsGazeToggleOn();
            originalDistanceToggle = metricsMenuController.IsDistanceToggleOn();
            originalSpeechToggle = metricsMenuController.IsSpeechToggleOn();
        }
    }
    
    void RestoreOriginalInteractionStates()
    {
        if (metricsMenuController != null)
        {
            // Restore original toggle states
            metricsMenuController.SetGazeToggle(originalGazeToggle);
            metricsMenuController.SetDistanceToggle(originalDistanceToggle);
            metricsMenuController.SetSpeechToggle(originalSpeechToggle);
        }
        
        // Also restore the EmotionController states using programmatic methods
        if (emotionController != null)
        {
            emotionController.SetGazeInteractionEnabledProgrammatic(originalGazeToggle);
            emotionController.SetDistanceInteractionEnabledProgrammatic(originalDistanceToggle);
            emotionController.SetSpeechInteractionEnabledProgrammatic(originalSpeechToggle);
        }
    }
    
    void DisableInterferingInteractions()
    {
        if (metricsMenuController != null)
        {
            // Disable gaze, distance, and speech toggles during tour
            metricsMenuController.SetGazeToggle(false);
            metricsMenuController.SetDistanceToggle(false);
            metricsMenuController.SetSpeechToggle(false);
        }
        
        // Also directly disable the EmotionController states using programmatic methods
        if (emotionController != null)
        {
            emotionController.SetGazeInteractionEnabledProgrammatic(false);
            emotionController.SetDistanceInteractionEnabledProgrammatic(false);
            emotionController.SetSpeechInteractionEnabledProgrammatic(false);
        }
    }
    
    void EnableCurrentStepInteraction()
    {
        if (metricsMenuController != null && currentStep < randomizedSteps.Count)
        {
            string currentInteractionType = randomizedSteps[currentStep].interactionType;
            
            // Enable only the interaction type for the current step
            switch (currentInteractionType)
            {
                case "gaze":
                    metricsMenuController.SetGazeToggle(true);
                    // Also enable in EmotionController using programmatic method
                    emotionController.SetGazeInteractionEnabledProgrammatic(true);
                    break;
                case "distance":
                    metricsMenuController.SetDistanceToggle(true);
                    // Also enable in EmotionController using programmatic method
                    emotionController.SetDistanceInteractionEnabledProgrammatic(true);
                    break;
                case "speech":
                    metricsMenuController.SetSpeechToggle(true);
                    // Also enable in EmotionController using programmatic method
                    emotionController.SetSpeechInteractionEnabledProgrammatic(true);
                    break;
                // Stroking, feeding, and ghost_mode don't need special interaction toggles
            }
        }
    }

    // Context menu for testing
    [ContextMenu("Start Tour")]
    public void TestStartTour()
    {
        StartTour();
    }
    
    [ContextMenu("Start Tour Immediately")]
    public void TestStartTourImmediately()
    {
        // Tour panel is already visible, just start the tour
        StartTour();
    }
    
    [ContextMenu("Next Step")]
    public void TestNextStep()
    {
        NextStep();
    }
}
