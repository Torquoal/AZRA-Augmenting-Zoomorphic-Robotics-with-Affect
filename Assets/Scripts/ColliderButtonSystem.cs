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
    
    [Header("Performance")]
    [SerializeField] private float handCheckInterval = 0.1f; // Check hands every 100ms (10 FPS)
    [SerializeField] private float meaningfulDistance = 0.2f; // Only check if hand is within this distance
    
    [Header("Button Assignment")]
    [SerializeField] private bool useManualButtonAssignment = false;
    [SerializeField] private Button[] manualButtons; // Manually assign buttons here
    
    // Editor testing removed to avoid Input System conflicts
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.green;
    
    private Button[] buttons;
    private float[] buttonCooldowns;
    private bool[] buttonStates; // true = pressed, false = not pressed
    private Color[] originalColors;
    
    // Performance tracking
    private float lastHandCheckTime = 0f;
    private Vector3 lastLeftHandPos = Vector3.zero;
    private Vector3 lastRightHandPos = Vector3.zero;
    
    void Start()
    {
        Debug.Log("ColliderButtonSystem: Starting...");
        SetupColliderButtons();
        Debug.Log("ColliderButtonSystem: Setup complete");
        
        // Test hand positions immediately
        Vector3 leftHand = GetHandPosition(true);
        Vector3 rightHand = GetHandPosition(false);
        Debug.Log($"ColliderButtonSystem: Initial hand positions - Left: {leftHand}, Right: {rightHand}");
    }
    
    void SetupColliderButtons()
    {
        // Get buttons based on assignment method
        if (useManualButtonAssignment && manualButtons != null && manualButtons.Length > 0)
        {
            buttons = manualButtons;
            Debug.Log($"ColliderButtonSystem: Using {buttons.Length} manually assigned buttons");
        }
        else
        {
            buttons = FindObjectsOfType<Button>();
            Debug.Log($"ColliderButtonSystem: Found {buttons.Length} buttons automatically");
        }
        
        buttonCooldowns = new float[buttons.Length];
        buttonStates = new bool[buttons.Length];
        originalColors = new Color[buttons.Length];
        
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
            
            // Make collider visible for debugging
            if (buttonCollider != null)
            {
                // Enable the collider's gizmo to be visible in scene view
                buttonCollider.enabled = true;
                
                // Add a visible material to the collider
                CreateVisibleCollider(button, buttonCollider);
                
                Debug.Log($"Collider enabled for button: {button.name}");
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
        
        // Throttled hand checking for performance
        if (Time.time - lastHandCheckTime >= handCheckInterval)
        {
            if (showDebugLogs && Time.time % 5f < 0.1f) // Log every 5 seconds
            {
                Debug.Log("ColliderButtonSystem: Update running, checking hand interactions...");
            }
            CheckHandButtonInteractions();
            lastHandCheckTime = Time.time;
        }
    }
    
    void CheckHandButtonInteractions()
    {
        // Get hand positions (using the same approach as stroke detection)
        Vector3 leftHandPos = GetHandPosition(true);
        Vector3 rightHandPos = GetHandPosition(false);
        
        if (showDebugLogs && Time.time % 3f < 0.1f) // Log every 3 seconds
        {
            Debug.Log($"ColliderButtonSystem: Checking interactions - Left: {leftHandPos}, Right: {rightHandPos}");
        }
        
        // Check if hands have moved significantly
        float leftHandMovement = Vector3.Distance(leftHandPos, lastLeftHandPos);
        float rightHandMovement = Vector3.Distance(rightHandPos, lastRightHandPos);
        
        // Only proceed if hands are within meaningful distance of any button
        bool shouldCheck = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Collider buttonCollider = buttons[i].GetComponent<Collider>();
                if (buttonCollider != null)
                {
                    float leftDist = Vector3.Distance(leftHandPos, buttonCollider.bounds.center);
                    float rightDist = Vector3.Distance(rightHandPos, buttonCollider.bounds.center);
                    
                    if (leftDist <= meaningfulDistance || rightDist <= meaningfulDistance)
                    {
                        shouldCheck = true;
                        break;
                    }
                }
            }
        }
        
        if (!shouldCheck)
        {
            // Update last positions and return early
            lastLeftHandPos = leftHandPos;
            lastRightHandPos = rightHandPos;
            return;
        }
        
        // Debug hand positions and distances every few seconds
        if (showDebugLogs && Time.time % 2f < 0.1f)
        {
            Debug.Log($"ColliderButtonSystem: Left hand at {leftHandPos}, Right hand at {rightHandPos}");
            
            // Log distances to each button
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    Collider buttonCollider = buttons[i].GetComponent<Collider>();
                    if (buttonCollider != null)
                    {
                        float leftDist = Vector3.Distance(leftHandPos, buttonCollider.bounds.center);
                        float rightDist = Vector3.Distance(rightHandPos, buttonCollider.bounds.center);
                        Debug.Log($"ColliderButtonSystem: Button {buttons[i].name} - Left hand distance: {leftDist:F3}m, Right hand distance: {rightDist:F3}m (Activation: {buttonActivationDistance:F3}m)");
                    }
                }
            }
        }
        
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
                    if (showDebugLogs && Time.time % 3f < 0.1f)
                    {
                        string handName = isLeftHand ? "Left" : "Right";
                        Debug.Log($"ColliderButtonSystem: {handName} hand tracked at {palmPose.position}");
                    }
                    return palmPose.position;
                }
                else if (showDebugLogs && Time.time % 3f < 0.1f)
                {
                    string handName = isLeftHand ? "Left" : "Right";
                    Debug.LogWarning($"ColliderButtonSystem: {handName} hand tracked but palm joint pose failed");
                }
            }
            else if (showDebugLogs && Time.time % 3f < 0.1f)
            {
                string handName = isLeftHand ? "Left" : "Right";
                Debug.LogWarning($"ColliderButtonSystem: {handName} hand not tracked");
            }
        }
        else if (showDebugLogs && Time.time % 3f < 0.1f)
        {
            Debug.LogWarning("ColliderButtonSystem: No hand subsystems found");
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
    
    void CreateVisibleCollider(Button button, Collider collider)
    {
        // Create a separate GameObject next to the button for visibility
        GameObject colliderVisual = new GameObject(button.name + "_ColliderVisual");
        colliderVisual.transform.position = button.transform.position + Vector3.right * 0.5f; // Spawn to the right
        colliderVisual.transform.rotation = button.transform.rotation;
        colliderVisual.transform.localScale = Vector3.one * 0.1f; // Small cube
        
        // Add a cube mesh to make it visible
        MeshRenderer renderer = colliderVisual.AddComponent<MeshRenderer>();
        MeshFilter filter = colliderVisual.AddComponent<MeshFilter>();
        filter.mesh = CreateCubeMesh();
        
        // Create a bright material that's easy to see
        Material colliderMaterial = new Material(Shader.Find("Standard"));
        colliderMaterial.color = Color.red; // Bright red, no transparency
        renderer.material = colliderMaterial;
        
        // Add a collider to the visual cube so it's easier to see
        BoxCollider visualCollider = colliderVisual.AddComponent<BoxCollider>();
        visualCollider.isTrigger = true;
        
        Debug.Log($"Created visible collider cube next to {button.name} at {colliderVisual.transform.position}");
    }
    
    Mesh CreateCubeMesh()
    {
        // Create a simple cube mesh
        Mesh mesh = new Mesh();
        mesh.name = "ColliderCube";
        
        // Simple cube vertices
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };
        
        int[] triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            2, 3, 4, 2, 4, 5,
            1, 2, 5, 5, 2, 6,
            0, 7, 4, 0, 4, 3,
            5, 6, 7, 5, 7, 4,
            0, 1, 5, 0, 5, 4
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    [ContextMenu("Show Hand Distances")]
    public void ShowHandDistances()
    {
        Vector3 leftHandPos = GetHandPosition(true);
        Vector3 rightHandPos = GetHandPosition(false);
        
        Debug.Log($"ColliderButtonSystem: Current hand positions - Left: {leftHandPos}, Right: {rightHandPos}");
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Collider buttonCollider = buttons[i].GetComponent<Collider>();
                if (buttonCollider != null)
                {
                    float leftDist = Vector3.Distance(leftHandPos, buttonCollider.bounds.center);
                    float rightDist = Vector3.Distance(rightHandPos, buttonCollider.bounds.center);
                    Debug.Log($"Button {buttons[i].name} at {buttonCollider.bounds.center} - Left: {leftDist:F3}m, Right: {rightDist:F3}m (Need: {buttonActivationDistance:F3}m)");
                }
            }
        }
    }
}
