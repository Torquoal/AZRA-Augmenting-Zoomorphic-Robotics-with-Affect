using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetricsMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider responseCooldownSlider;
    [SerializeField] private TextMeshProUGUI responseCooldownValueText;
    [SerializeField] private TextMeshProUGUI responseCooldownLabelText;
    
    [Header("Mood vs Event Weighting")]
    [SerializeField] private Slider moodEventBalanceSlider;
    [SerializeField] private TextMeshProUGUI moodEventBalanceText;
    
    [Header("Stochastic Variability")]
    [SerializeField] private Slider stochasticVariabilitySlider;
    [SerializeField] private TextMeshProUGUI stochasticVariabilityText;
    
    [Header("Response Toggles")]
    [SerializeField] private Toggle faceToggle;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle lightToggle;
    [SerializeField] private Toggle thoughtToggle;
    
    [Header("Interaction Toggles")]
    [SerializeField] private Toggle strokeToggle;
    [SerializeField] private Toggle distanceToggle;
    [SerializeField] private Toggle gazeToggle;
    [SerializeField] private Toggle speechToggle;
    
    [Header("Emotional State Buttons")]
    [SerializeField] private Button happyButton;
    [SerializeField] private Button neutralButton;
    [SerializeField] private Button annoyedButton;
    [SerializeField] private Button sadButton;
    
    [Header("Sound Style Buttons")]
    [SerializeField] private Button currentSoundButton;
    [SerializeField] private Button animaleseSoundButton;
    [SerializeField] private Button catSoundButton;
    [SerializeField] private TextMeshProUGUI currentSoundStyleText;
    
    [Header("Face Style Buttons")]
    [SerializeField] private Button currentFaceButton;
    [SerializeField] private Button animeFaceButton;
    [SerializeField] private Button catFaceButton;
    [SerializeField] private TextMeshProUGUI currentFaceStyleText;
    
    [Header("Settings")]
    [SerializeField] private float minCooldown = 1.0f;
    [SerializeField] private float maxCooldown = 10.0f;
    [SerializeField] private float defaultCooldown = 2.0f;
    
    [Header("Target Scripts")]
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private SoundStyleManager soundStyleManager;
    [SerializeField] private FaceStyleManager faceStyleManager;
    [SerializeField] private FaceAnimationController faceAnimationController;
    [SerializeField] private StudyLogger studyLogger;
    
    private float currentCooldown;
    
    // Tracking for slider interactions to log only once per interaction
    private bool isSliderBeingDragged = false;
    private bool isMoodEventSliderBeingDragged = false;
    private bool isStochasticSliderBeingDragged = false;
    
    void Start()
    {
        // Setup slider
        if (responseCooldownSlider != null)
        {
            responseCooldownSlider.minValue = minCooldown;
            responseCooldownSlider.maxValue = maxCooldown;
            responseCooldownSlider.onValueChanged.AddListener(OnCooldownSliderChanged);
        }
        
        // Get initial value from EmotionController or use default
        if (emotionController != null)
        {
            currentCooldown = GetEmotionControllerCooldown();
        }
        else
        {
            currentCooldown = defaultCooldown;
        }
        
        if (responseCooldownSlider != null)
        {
            responseCooldownSlider.value = currentCooldown;
        }
        
        UpdateDisplay();
        
        // Setup emotional state buttons
        SetupEmotionalStateButtons();
        
        // Setup sound style buttons
        SetupSoundStyleButtons();
        
        // Setup face style buttons
        SetupFaceStyleButtons();
        
        // Setup mood vs event weighting sliders
        SetupWeightingSliders();
        
        // Setup stochastic variability slider
        SetupStochasticVariabilitySlider();
        
        // Setup response toggles
        SetupResponseToggles();
        
        // Setup interaction toggles
        SetupInteractionToggles();
    }
    
    void SetupEmotionalStateButtons()
    {
        if (happyButton != null)
        {
            happyButton.onClick.AddListener(() => SetEmotionalState("Happy"));
        }
        
        if (neutralButton != null)
        {
            neutralButton.onClick.AddListener(() => SetEmotionalState("Neutral"));
        }
        
        if (annoyedButton != null)
        {
            annoyedButton.onClick.AddListener(() => SetEmotionalState("Annoyed"));
        }
        
        if (sadButton != null)
        {
            sadButton.onClick.AddListener(() => SetEmotionalState("Sad"));
        }
    }
    
    void SetEmotionalState(string state)
    {
        if (emotionModel != null)
        {
            switch (state)
            {
                case "Happy":
                    emotionModel.SetHappyState();
                    Debug.Log("MetricsMenu: Set robot to Happy state (V: 6, A: 0)");
                    break;
                case "Neutral":
                    emotionModel.SetNeutralState();
                    Debug.Log("MetricsMenu: Set robot to Neutral state (V: 0, A: 0)");
                    break;
                case "Annoyed":
                    emotionModel.SetAnnoyedState();
                    Debug.Log("MetricsMenu: Set robot to Annoyed state (V: -6, A: 6)");
                    break;
                case "Sad":
                    emotionModel.SetSadState();
                    Debug.Log("MetricsMenu: Set robot to Sad state (V: -6, A: 0)");
                    break;
            }
            
            // Log the button press
            LogPrototypingEvent($"{state} State Button", "pressed");
        }
        else
        {
            Debug.LogWarning("MetricsMenu: EmotionModel not assigned - cannot set emotional state");
        }
    }
    
    void SetupSoundStyleButtons()
    {
        if (currentSoundButton != null)
        {
            currentSoundButton.onClick.AddListener(() => SetSoundStyle(SoundStyleManager.SoundStyle.Current));
        }
        
        if (animaleseSoundButton != null)
        {
            animaleseSoundButton.onClick.AddListener(() => SetSoundStyle(SoundStyleManager.SoundStyle.Animalese));
        }
        
        if (catSoundButton != null)
        {
            catSoundButton.onClick.AddListener(() => SetSoundStyle(SoundStyleManager.SoundStyle.Cat));
        }
        
        // Update display with current style
        UpdateSoundStyleDisplay();
    }
    
    void SetSoundStyle(SoundStyleManager.SoundStyle style)
    {
        if (soundStyleManager != null)
        {
            soundStyleManager.SetSoundStyle(style);
            UpdateSoundStyleDisplay();
            Debug.Log($"MetricsMenu: Sound style changed to {style}");
            
            // Log the button press
            LogPrototypingEvent($"{style} Sound Style Button", "pressed");
        }
        else
        {
            Debug.LogWarning("MetricsMenu: SoundStyleManager not assigned - cannot change sound style");
        }
    }
    
    void UpdateSoundStyleDisplay()
    {
        if (currentSoundStyleText != null && soundStyleManager != null)
        {
            currentSoundStyleText.text = $"Current: {soundStyleManager.GetCurrentStyleName()}";
        }
    }
    
    void SetupFaceStyleButtons()
    {
        if (currentFaceButton != null)
        {
            currentFaceButton.onClick.AddListener(() => SetFaceStyle(FaceStyleManager.FaceStyle.Current));
        }
        
        if (animeFaceButton != null)
        {
            animeFaceButton.onClick.AddListener(() => SetFaceStyle(FaceStyleManager.FaceStyle.Anime));
        }
        
        if (catFaceButton != null)
        {
            catFaceButton.onClick.AddListener(() => SetFaceStyle(FaceStyleManager.FaceStyle.Cat));
        }
        
        // Update display with current style
        UpdateFaceStyleDisplay();
    }
    
    void SetFaceStyle(FaceStyleManager.FaceStyle style)
    {
        if (faceStyleManager != null)
        {
            faceStyleManager.SetFaceStyle(style);
            UpdateFaceStyleDisplay();
            
            // Reload frames in the animation controller
            if (faceAnimationController != null)
            {
                faceAnimationController.ReloadFramesForCurrentStyle();
            }
            
            Debug.Log($"MetricsMenu: Face style changed to {style}");
            
            // Log the button press
            LogPrototypingEvent($"{style} Face Style Button", "pressed");
        }
        else
        {
            Debug.LogWarning("MetricsMenu: FaceStyleManager not assigned - cannot change face style");
        }
    }
    
    void UpdateFaceStyleDisplay()
    {
        if (currentFaceStyleText != null && faceStyleManager != null)
        {
            currentFaceStyleText.text = $"Current: {faceStyleManager.GetCurrentStyleName()}";
        }
    }
    
    void SetupWeightingSliders()
    {
        // Setup mood vs event balance slider
        if (moodEventBalanceSlider != null)
        {
            moodEventBalanceSlider.minValue = 0f;  // 0 = 100% Event, 0% Mood
            moodEventBalanceSlider.maxValue = 1f;  // 1 = 0% Event, 100% Mood
            moodEventBalanceSlider.onValueChanged.AddListener(OnMoodEventBalanceChanged);
            
            // Get initial value from EmotionModel (convert mood weight to slider position)
            if (emotionModel != null)
            {
                moodEventBalanceSlider.value = emotionModel.GetMoodWeight();
            }
        }
        
        // Update display
        UpdateWeightingDisplay();
    }
    
    void SetupStochasticVariabilitySlider()
    {
        if (stochasticVariabilitySlider != null)
        {
            stochasticVariabilitySlider.minValue = 0f;
            stochasticVariabilitySlider.maxValue = 10f;
            stochasticVariabilitySlider.onValueChanged.AddListener(OnStochasticVariabilityChanged);
            
            // Get initial value from EmotionModel
            if (emotionModel != null)
            {
                float variability = emotionModel.GetStochasticVariability();
                stochasticVariabilitySlider.value = variability;
            }
            
            UpdateStochasticVariabilityDisplay();
        }
    }
    
    void OnMoodEventBalanceChanged(float value)
    {
        if (emotionModel != null)
        {
            // value: 0 = 100% Event, 1 = 100% Mood
            emotionModel.SetMoodWeight(value);
            UpdateWeightingDisplay();
            Debug.Log($"MetricsMenu: Mood/Event balance changed to {value:F2} (Mood: {value:F2}, Event: {1f-value:F2})");
        }
        
        // Only log when dragging ends, not during dragging
        if (!isMoodEventSliderBeingDragged)
        {
            LogPrototypingEvent("Mood Event Balance Slider", value.ToString("F2"));
        }
    }
    
    void OnStochasticVariabilityChanged(float value)
    {
        if (emotionModel != null)
        {
            emotionModel.SetStochasticVariability(value);
            UpdateStochasticVariabilityDisplay();
            float percentage = (value / 20f) * 100f;
            Debug.Log($"MetricsMenu: Stochastic variability changed to {value:F1} ({percentage:F0}% of emotional range)");
        }
        
        // Only log when dragging ends, not during dragging
        if (!isStochasticSliderBeingDragged)
        {
            LogPrototypingEvent("Stochastic Variability Slider", value.ToString("F1"));
        }
    }
    
    void UpdateWeightingDisplay()
    {
        if (emotionModel != null)
        {
            if (moodEventBalanceText != null)
            {
                float moodWeight = emotionModel.GetMoodWeight();
                float eventWeight = emotionModel.GetEventWeight();
                moodEventBalanceText.text = $"Mood: {moodWeight:P0} | Event: {eventWeight:P0}";
            }
        }
    }
    
    void UpdateStochasticVariabilityDisplay()
    {
        if (emotionModel != null)
        {
            if (stochasticVariabilityText != null)
            {
                float variability = emotionModel.GetStochasticVariability();
                float percentage = (variability / 20f) * 100f;
                stochasticVariabilityText.text = $"Stochastic Variability: {percentage:F0}%";
            }
        }
    }
    

    public 

    void SetupResponseToggles()
    {
        // Setup face toggle
        if (faceToggle != null)
        {
            faceToggle.onValueChanged.AddListener(OnFaceToggleChanged);
            // Get initial value from EmotionController
            if (emotionController != null)
            {
                // Set value without triggering event
                faceToggle.SetIsOnWithoutNotify(emotionController.IsFaceResponsesEnabled());
            }
        }
        
        // Setup sound toggle
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                soundToggle.SetIsOnWithoutNotify(emotionController.IsSoundResponsesEnabled());
            }
        }
        
        // Setup light toggle
        if (lightToggle != null)
        {
            lightToggle.onValueChanged.AddListener(OnLightToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                lightToggle.SetIsOnWithoutNotify(emotionController.IsLightResponsesEnabled());
            }
        }
        
        // Setup thought toggle
        if (thoughtToggle != null)
        {
            thoughtToggle.onValueChanged.AddListener(OnThoughtToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                thoughtToggle.SetIsOnWithoutNotify(emotionController.IsThoughtResponsesEnabled());
            }
        }
    }
    
    // Timer lock for face toggle to prevent rapid clicking
    private float lastFaceToggleTime = 0f;
    private const float FACE_TOGGLE_COOLDOWN = 0.5f;
    
    void OnFaceToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Face toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (faceToggle != null)
            {
                faceToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetFaceResponsesEnabled();
            Debug.Log($"MetricsMenu: Face toggle clicked");
        }
    }
    
    void OnSoundToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Sound toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (soundToggle != null)
            {
                soundToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetSoundResponsesEnabled();
            Debug.Log($"MetricsMenu: Sound toggle clicked");
        }
    }
    
    void OnLightToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Light toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (lightToggle != null)
            {
                lightToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetLightResponsesEnabled();
            Debug.Log($"MetricsMenu: Light toggle clicked");
        }
    }
    
    void OnThoughtToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Thought toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (thoughtToggle != null)
            {
                thoughtToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetThoughtResponsesEnabled();
            Debug.Log($"MetricsMenu: Thought toggle clicked");
        }
    }
    
    void SetupInteractionToggles()
    {
        // Setup stroke toggle
        if (strokeToggle != null)
        {
            strokeToggle.onValueChanged.AddListener(OnStrokeToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                strokeToggle.SetIsOnWithoutNotify(emotionController.IsStrokeInteractionEnabled());
            }
        }
        
        // Setup distance toggle
        if (distanceToggle != null)
        {
            distanceToggle.onValueChanged.AddListener(OnDistanceToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                distanceToggle.SetIsOnWithoutNotify(emotionController.IsDistanceInteractionEnabled());
            }
        }
        
        // Setup gaze toggle
        if (gazeToggle != null)
        {
            gazeToggle.onValueChanged.AddListener(OnGazeToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                gazeToggle.SetIsOnWithoutNotify(emotionController.IsGazeInteractionEnabled());
            }
        }
        
        // Setup speech toggle
        if (speechToggle != null)
        {
            speechToggle.onValueChanged.AddListener(OnSpeechToggleChanged);
            if (emotionController != null)
            {
                // Set value without triggering event
                speechToggle.SetIsOnWithoutNotify(emotionController.IsSpeechInteractionEnabled());
            }
        }
    }
    
    void OnStrokeToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Stroke toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (strokeToggle != null)
            {
                strokeToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetStrokeInteractionEnabled();
            Debug.Log($"MetricsMenu: Stroke toggle clicked");
        }
    }
    
    void OnDistanceToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Distance toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (distanceToggle != null)
            {
                distanceToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetDistanceInteractionEnabled();
            Debug.Log($"MetricsMenu: Distance toggle clicked");
        }
    }
    
    void OnGazeToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Gaze toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (gazeToggle != null)
            {
                gazeToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetGazeInteractionEnabled();
            Debug.Log($"MetricsMenu: Gaze toggle clicked");
        }
    }
    
    void OnSpeechToggleChanged(bool enabled)
    {
        // Check cooldown to prevent rapid clicking
        if (Time.time - lastFaceToggleTime < FACE_TOGGLE_COOLDOWN)
        {
            Debug.Log($"MetricsMenu: Speech toggle on cooldown, please wait {FACE_TOGGLE_COOLDOWN - (Time.time - lastFaceToggleTime):F1}s");
            // Reset the toggle to its previous state
            if (speechToggle != null)
            {
                speechToggle.SetIsOnWithoutNotify(!enabled);
            }
            return;
        }
        
        lastFaceToggleTime = Time.time;
        
        if (emotionController != null)
        {
            emotionController.SetSpeechInteractionEnabled();
            Debug.Log($"MetricsMenu: Speech toggle clicked");
        }
    }
    
    float GetEmotionControllerCooldown()
    {
        if (emotionController != null)
        {
            // Use reflection to get the private field
            var field = emotionController.GetType().GetField("displayCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(emotionController);
            }
        }
        return defaultCooldown;
    }
    
    void OnCooldownSliderChanged(float value)
    {
        Debug.Log($"OnCooldownSliderChanged called with value: {value}");
        currentCooldown = value;
        UpdateDisplay();
        
        // Apply to emotion controller if available
        if (emotionController != null)
        {
            // Set the displayCooldown directly using reflection or a public setter
            SetEmotionControllerCooldown(currentCooldown);
            Debug.Log($"MetricsMenu: Response cooldown changed to {currentCooldown:F1}s");
        }
        
        // Only log when dragging ends, not during dragging
        if (!isSliderBeingDragged)
        {
            LogPrototypingEvent("Response Cooldown Slider", currentCooldown.ToString("F1"));
        }
    }
    
    void SetEmotionControllerCooldown(float cooldown)
    {
        if (emotionController != null)
        {
            // Use reflection to set the private field
            var field = emotionController.GetType().GetField("displayCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(emotionController, cooldown);
            }
        }
    }
    
    // Log prototyping events to StudyLogger
    void LogPrototypingEvent(string item, string value)
    {
        if (studyLogger != null)
        {
            studyLogger.LogEmotionalResponse("prototyping", $"prototyping {item} changed to {value}");
        }
    }
    
    void UpdateDisplay()
    {
        if (responseCooldownValueText != null)
        {
            responseCooldownValueText.text = $"{currentCooldown:F1}s";
        }
        
        if (responseCooldownLabelText != null)
        {
            responseCooldownLabelText.text = "Response Cooldown:";
        }
    }
    
    // Public method to get current cooldown value
    public float GetResponseCooldown()
    {
        return currentCooldown;
    }
    
    // Public method to set cooldown programmatically
    public void SetResponseCooldown(float cooldown)
    {
        cooldown = Mathf.Clamp(cooldown, minCooldown, maxCooldown);
        currentCooldown = cooldown;
        
        if (responseCooldownSlider != null)
        {
            responseCooldownSlider.value = cooldown;
        }
        
        UpdateDisplay();
    }
    
    // Public methods to control toggles programmatically for GuidedTour
    public void SetGazeToggle(bool enabled)
    {
        if (gazeToggle != null)
        {
            gazeToggle.SetIsOnWithoutNotify(enabled);
        }
    }
    
    public void SetDistanceToggle(bool enabled)
    {
        if (distanceToggle != null)
        {
            distanceToggle.SetIsOnWithoutNotify(enabled);
        }
    }
    
    public void SetSpeechToggle(bool enabled)
    {
        if (speechToggle != null)
        {
            speechToggle.SetIsOnWithoutNotify(enabled);
        }
    }
    
    public bool IsGazeToggleOn()
    {
        return gazeToggle != null ? gazeToggle.isOn : false;
    }
    
    public bool IsDistanceToggleOn()
    {
        return distanceToggle != null ? distanceToggle.isOn : false;
    }
    
    public bool IsSpeechToggleOn()
    {
        return speechToggle != null ? speechToggle.isOn : false;
    }
}
