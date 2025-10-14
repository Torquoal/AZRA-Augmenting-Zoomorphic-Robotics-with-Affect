using UnityEngine;
using UnityEngine.UI;

public class StrokeBasedButtonSystem : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private float buttonActivationDistance = 0.05f;
    [SerializeField] private float buttonCooldown = 0.5f;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("References")]
    [SerializeField] private StrokeDetector strokeDetector; // Reference to your existing stroke detector
    
    private Button[] buttons;
    private float[] buttonCooldowns;
    private bool[] buttonStates;
    
    void Start()
    {
        SetupButtons();
    }
    
    void SetupButtons()
    {
        buttons = FindObjectsOfType<Button>();
        buttonCooldowns = new float[buttons.Length];
        buttonStates = new bool[buttons.Length];
        
        Debug.Log($"StrokeBasedButtonSystem: Found {buttons.Length} buttons");
        
        // Add colliders to buttons if they don't have them
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            Collider buttonCollider = button.GetComponent<Collider>();
            
            if (buttonCollider == null)
            {
                buttonCollider = button.gameObject.AddComponent<BoxCollider>();
                buttonCollider.isTrigger = true;
                Debug.Log($"Added collider to button: {button.name}");
            }
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
        
        // Check for hand interactions
        CheckHandButtonInteractions();
    }
    
    void CheckHandButtonInteractions()
    {
        // Get hand positions using the same method as your stroke detector
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
            
            // Check if hand is close enough and not on cooldown
            if (minDistance <= buttonActivationDistance && buttonCooldowns[i] <= 0)
            {
                if (!buttonStates[i])
                {
                    buttonStates[i] = true;
                    ActivateButton(button, i);
                }
            }
            else
            {
                buttonStates[i] = false;
            }
        }
    }
    
    void ActivateButton(Button button, int buttonIndex)
    {
        // Trigger the button
        button.onClick.Invoke();
        
        // Set cooldown
        buttonCooldowns[buttonIndex] = buttonCooldown;
        
        if (showDebugLogs)
        {
            Debug.Log($"StrokeBasedButton: {button.name} activated by hand proximity!");
        }
    }
    
    Vector3 GetHandPosition(bool isLeftHand)
    {
        // Use the same hand tracking approach as your StrokeDetector
        // You can copy the hand position logic from your existing StrokeDetector script
        
        // For now, return a position that won't trigger buttons
        // You'll need to implement this based on your existing stroke detection code
        return Vector3.one * 1000f;
    }
    
    [ContextMenu("Test All Buttons")]
    public void TestAllButtons()
    {
        Debug.Log("=== TESTING ALL STROKE-BASED BUTTONS ===");
        
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
}
