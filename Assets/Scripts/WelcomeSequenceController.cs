using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WelcomeSequenceController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas welcomeCanvas;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI countdownText;
    
    [Header("Sequence Settings")]
    [SerializeField] private float qooboCountdownDuration = 10f; // Countdown before Qoobo placement
    [SerializeField] private float recenterDetectionTimeout = 10f; // How long to wait for user to recenter
    [SerializeField] private bool waitForManualRecenter = true; // Wait for user to recenter manually
    
    [Header("References")]
    [SerializeField] private QooboPositioner qooboPositioner; // Reference to QooboPositioner
    [SerializeField] private MenuFollowSystem menuFollowSystem; // Reference to MenuFollowSystem
    [SerializeField] private GuidedTourController guidedTourController; // Reference to GuidedTourController
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool sequenceCompleted = false;
    private bool recenterDetected = false;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float recenterDetectionStartTime;
    private Transform userTransform;
    
    void Start()
    {
        // Find the main camera (user's head)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            userTransform = mainCamera.transform;
            if (showDebugLogs) Debug.Log("WelcomeSequence: Found user camera");
        }
        else
        {
            Debug.LogError("WelcomeSequence: No main camera found!");
        }
        
        // Start the welcome sequence
        StartCoroutine(WelcomeSequence());
    }
    
    IEnumerator WelcomeSequence()
    {
        if (showDebugLogs) Debug.Log("WelcomeSequence: Starting welcome sequence");
        
        // Show the welcome UI
        if (welcomeCanvas != null)
        {
            welcomeCanvas.gameObject.SetActive(true);
        }
        
        // Step 1: Show recenter instruction immediately
        if (instructionText != null)
        {
            instructionText.text = "Please Recenter Your View";
        }
        if (countdownText != null)
        {
            countdownText.text = "Use Quest Menu → Recenter";
        }
        
        if (showDebugLogs) Debug.Log("WelcomeSequence: Waiting for user to recenter manually");
        
        // Step 2: Wait for user to recenter manually
        if (waitForManualRecenter)
        {
            // Wait a moment for app to stabilize before starting recenter detection
            if (showDebugLogs) Debug.Log("WelcomeSequence: Waiting for app to stabilize before recenter detection");
            yield return new WaitForSeconds(2f);
            
            yield return StartCoroutine(WaitForRecenter());
        }
        
        // Step 3: Show Qoobo instruction
        if (instructionText != null)
        {
            instructionText.text = "Place hand on top of Qoobo with your middle finger in line with the tail";
        }
        if (countdownText != null)
        {
            countdownText.text = "";
        }
        
        if (showDebugLogs) Debug.Log("WelcomeSequence: Showing Qoobo placement instruction");
        
        // Step 4: Countdown for Qoobo placement
        if (showDebugLogs) Debug.Log("WelcomeSequence: Starting Qoobo countdown");
        
        for (int i = (int)qooboCountdownDuration; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            
            if (showDebugLogs) Debug.Log($"WelcomeSequence: Qoobo countdown {i}");
            
            yield return new WaitForSeconds(1f);
        }
        
        // Step 5: Trigger Qoobo placement and menu positioning
        if (showDebugLogs) Debug.Log("WelcomeSequence: Triggering Qoobo placement and menu positioning");
        
        // Call QooboPositioner UpdateQooboPosition function
        if (qooboPositioner != null)
        {
            qooboPositioner.UpdateQooboPosition();
            if (showDebugLogs) Debug.Log("WelcomeSequence: QooboPositioner.UpdateQooboPosition() called");
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("WelcomeSequence: QooboPositioner not assigned");
        }
        
        // Call MenuFollowSystem positioning
        if (menuFollowSystem != null)
        {
            menuFollowSystem.TeleportToUser();
            if (showDebugLogs) Debug.Log("WelcomeSequence: MenuFollowSystem.TeleportToUser() called");
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("WelcomeSequence: MenuFollowSystem not assigned");
        }
        
        // Step 6: Complete sequence
        if (showDebugLogs) Debug.Log("WelcomeSequence: Sequence completed");
        
        // Hide UI
        if (welcomeCanvas != null)
        {
            welcomeCanvas.gameObject.SetActive(false);
        }
        
        sequenceCompleted = true;
        
        // Notify other systems that welcome sequence is done
        OnWelcomeSequenceCompleted();
    }
    
    IEnumerator WaitForRecenter()
    {
        if (showDebugLogs) Debug.Log("WelcomeSequence: Starting recenter detection");
        
        // Store initial camera position and rotation
        if (userTransform != null)
        {
            lastCameraPosition = userTransform.position;
            lastCameraRotation = userTransform.rotation;
        }
        
        recenterDetectionStartTime = Time.time;
        recenterDetected = false;
        
        // Wait for recenter detection or timeout
        while (!recenterDetected && (Time.time - recenterDetectionStartTime) < recenterDetectionTimeout)
        {
            // Check for recenter by detecting camera position or rotation change
            if (userTransform != null)
            {
                Vector3 currentPosition = userTransform.position;
                Quaternion currentRotation = userTransform.rotation;
                
                float distanceMoved = Vector3.Distance(currentPosition, lastCameraPosition);
                float rotationDifference = Quaternion.Angle(currentRotation, lastCameraRotation);
                
                // Require significant positional movement for recenter detection
                bool positionChanged = distanceMoved > 0.2f; // 20cm threshold - requires actual movement
                bool rotationChanged = rotationDifference > 15f; // 15 degree threshold - only large head turns
                
                // Only trigger if BOTH position AND rotation change (more reliable)
                if (positionChanged && rotationChanged)
                {
                    if (showDebugLogs) Debug.Log($"WelcomeSequence: Recenter detected! Position: {distanceMoved:F2}m, Rotation: {rotationDifference:F1}°");
                    recenterDetected = true;
                    break;
                }
                
                // Alternative: Just position change (if rotation is too sensitive)
                if (positionChanged && distanceMoved > 0.3f) // 30cm threshold for position-only detection
                {
                    if (showDebugLogs) Debug.Log($"WelcomeSequence: Recenter detected by position only! Position: {distanceMoved:F2}m");
                    recenterDetected = true;
                    break;
                }
                
                // Also check for any movement at all (even tiny movements)
                if (distanceMoved > 0.01f || rotationDifference > 0.5f)
                {
                    if (showDebugLogs) Debug.Log($"WelcomeSequence: Small movement detected - Position: {distanceMoved:F3}m, Rotation: {rotationDifference:F2}°");
                }
            }
            
            yield return new WaitForSeconds(0.1f); // Check every 100ms
        }
        
        if (recenterDetected)
        {
            if (showDebugLogs) Debug.Log("WelcomeSequence: Recenter detected successfully");
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("WelcomeSequence: Recenter detection timed out");
        }
    }
    
    void OnWelcomeSequenceCompleted()
    {
        // This can be called by other systems to check if welcome sequence is done
        if (showDebugLogs) Debug.Log("WelcomeSequence: Welcome sequence completed - app ready");
        
        // Start the guided tour after welcome sequence
        if (guidedTourController != null)
        {
            guidedTourController.StartTour();
        }
        else
        {
            Debug.LogWarning("WelcomeSequence: GuidedTourController not assigned - tour will not start");
        }
    }
    
    // Public methods for other systems to check status
    public bool IsSequenceCompleted()
    {
        return sequenceCompleted;
    }
    
    public void StartSequence()
    {
        if (!sequenceCompleted)
        {
            StartCoroutine(WelcomeSequence());
        }
    }
    
    public void SkipSequence()
    {
        if (showDebugLogs) Debug.Log("WelcomeSequence: Sequence skipped");
        
        if (welcomeCanvas != null)
        {
            welcomeCanvas.gameObject.SetActive(false);
        }
        
        sequenceCompleted = true;
        OnWelcomeSequenceCompleted();
    }
}