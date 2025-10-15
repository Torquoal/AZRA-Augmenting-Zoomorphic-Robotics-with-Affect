using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetricsMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider responseCooldownSlider;
    [SerializeField] private TextMeshProUGUI responseCooldownValueText;
    [SerializeField] private TextMeshProUGUI responseCooldownLabelText;
    
    [Header("Settings")]
    [SerializeField] private float minCooldown = 0.5f;
    [SerializeField] private float maxCooldown = 5.0f;
    [SerializeField] private float defaultCooldown = 2.0f;
    
    [Header("Target Script")]
    [SerializeField] private EmotionController emotionController;
    
    private float currentCooldown;
    
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
        currentCooldown = value;
        UpdateDisplay();
        
        // Apply to emotion controller if available
        if (emotionController != null)
        {
            // Set the displayCooldown directly using reflection or a public setter
            SetEmotionControllerCooldown(currentCooldown);
            Debug.Log($"MetricsMenu: Response cooldown changed to {currentCooldown:F1}s");
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
}
