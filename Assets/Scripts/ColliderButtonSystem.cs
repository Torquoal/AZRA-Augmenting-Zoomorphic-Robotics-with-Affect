using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class ColliderButtonSystem : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private float buttonActivationDistance = 0.05f; // Distance to activate button
    [SerializeField] private float buttonCooldown = 0.5f; // Cooldown between activations
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.green;
    
    private Button[] buttons;
    private float[] buttonCooldowns;
    private bool[] buttonStates; // true = pressed, false = not pressed
    private Color[] originalColors;
    
    void Start()
    {
        SetupColliderButtons();
    }
    
    void SetupColliderButtons()
    {
        // Find all buttons in the scene
        buttons = FindObjectsOfType<Button>();
        buttonCooldowns = new float[buttons.Length];
        buttonStates = new bool[buttons.Length];
        originalColors = new Color[buttons.Length];
        
        Debug.Log($"ColliderButtonSystem: Found {buttons.Length} buttons to setup");
        
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            
            // Add collider if not present
            Collider buttonCollider = button.GetComponent<Collider>();
            if (buttonCollider == null)
            {
                // Add a box collider to the button
                buttonCollider = button.gameObject.AddComponent<BoxCollider>();
                buttonCollider.isTrigger = true;
                Debug.Log($"Added collider to button: {button.name}");
            }
            
            // Store original color
            if (button.image != null)
            {
                originalColors[i] = button.image.color;
            }
            
            // Set up button state
            buttonStates[i] = false;
            buttonCooldowns[i] = 0f;
            
            Debug.Log($"Setup collider button: {button.name}");
        }
    }
    
    void Update()
    {
        // Update cooldowns
        for (int i = 0; i < buttonCooldowns.Length; i++)
        {
            if (buttonCooldowns[i] > 0)
            {
                buttonCooldowns[i] -= Time.deltaTime;
            }
        }
        
        // Check for hand interactions with buttons
        CheckHandButtonInteractions();
    }
    
    void CheckHandButtonInteractions()
    {
        // Get hand positions (using the same approach as stroke detection)
        Vector3 leftHandPos = GetHandPosition(true);
        Vector3 rightHandPos = GetHandPosition(false);
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            
            Button button = buttons[i];
            Collider buttonCollider = button.GetComponent<Collider>();
            if (buttonCollider == null) continue;
            
            // Check distance to both hands
            float leftDistance = Vector3.Distance(leftHandPos, buttonCollider.bounds.center);
            float rightDistance = Vector3.Distance(rightHandPos, buttonCollider.bounds.center);
            float minDistance = Mathf.Min(leftDistance, rightDistance);
            
            // Check if hand is close enough to activate button
            if (minDistance <= buttonActivationDistance && buttonCooldowns[i] <= 0)
            {
                if (!buttonStates[i])
                {
                    // Button just activated
                    buttonStates[i] = true;
                    ActivateButton(button, i);
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"ColliderButton: {button.name} activated by hand proximity!");
                    }
                }
                
                // Visual feedback - hover state
                SetButtonColor(button, i, hoverColor);
            }
            else
            {
                if (buttonStates[i])
                {
                    // Button just deactivated
                    buttonStates[i] = false;
                    SetButtonColor(button, i, normalColor);
                }
            }
        }
    }
    
    void ActivateButton(Button button, int buttonIndex)
    {
        // Trigger the button's onClick event
        button.onClick.Invoke();
        
        // Set cooldown
        buttonCooldowns[buttonIndex] = buttonCooldown;
        
        // Visual feedback - pressed state
        SetButtonColor(button, buttonIndex, pressedColor);
        
        // Reset to normal color after a short delay
        StartCoroutine(ResetButtonColor(button, buttonIndex, 0.1f));
    }
    
    System.Collections.IEnumerator ResetButtonColor(Button button, int buttonIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetButtonColor(button, buttonIndex, normalColor);
    }
    
    void SetButtonColor(Button button, int buttonIndex, Color color)
    {
        if (button.image != null)
        {
            button.image.color = color;
        }
        
        // Also try to set color on any child images (for more complex button setups)
        Image[] childImages = button.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img != button.image) // Don't double-set the main image
            {
                img.color = color;
            }
        }
    }
    
    Vector3 GetHandPosition(bool isLeftHand)
    {
        // Use the same hand tracking approach as your existing stroke detection
        // This should match your StrokeDetector implementation
        
        // Try to get hand position from XRHandSubsystem (like your existing code)
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        
        if (handSubsystems.Count > 0)
        {
            var handSubsystem = handSubsystems[0];
            var hand = isLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
            
            if (hand.isTracked)
            {
                var palmJoint = hand.GetJoint(XRHandJointID.Palm);
                if (palmJoint.TryGetPose(out Pose palmPose))
                {
                    return palmPose.position;
                }
            }
        }
        
        // Fallback: return a position far away so buttons won't activate
        return Vector3.one * 1000f;
    }
    
    [ContextMenu("Test All Buttons")]
    public void TestAllButtons()
    {
        Debug.Log("=== TESTING ALL COLLIDER BUTTONS ===");
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Debug.Log($"Testing button: {buttons[i].name}");
                buttons[i].onClick.Invoke();
            }
        }
        
        Debug.Log("=== BUTTON TEST COMPLETE ===");
    }
    
    [ContextMenu("Reset All Button Colors")]
    public void ResetAllButtonColors()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                SetButtonColor(buttons[i], i, originalColors[i]);
            }
        }
    }
}
