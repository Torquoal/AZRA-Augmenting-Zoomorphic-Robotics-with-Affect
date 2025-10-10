using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class FeedingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject robotBodyGameObject; // Robot body GameObject for proximity (primary target)
    [SerializeField] private AudioController audioController;
    [SerializeField] private FaceController faceController;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private SceneController sceneController;

    [Header("Donut System")]
    [SerializeField] private GameObject foodSphere; // Pre-existing sphere in scene called "Food"
    [SerializeField] private GameObject fullDonutOnSphere; // Full donut model placed on food sphere
    [SerializeField] private GameObject partialDonutOnSphere; // Partial donut model placed on food sphere  
    [SerializeField] private GameObject mostlyEatenDonutOnSphere; // Mostly eaten donut model placed on food sphere

    [Header("Feeding Settings")]
    [SerializeField] private float proximityThreshold = 0.15f; // Configurable proximity distance
    [SerializeField] private float biteInterval = 1.5f; // Time between bites
    [SerializeField] private string biteSoundName = "peep"; // Sound to play for each bite
    [SerializeField] private string eatingFaceExpression = "eating"; // Face expression during eating

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private GameObject currentDonut;
    private bool isEatingSequence = false;
    private Coroutine eatingCoroutine;
    private XRHandSubsystem handSubsystem;

    // Donut states
    private enum DonutState { Full, Partial, MostlyEaten, Gone }
    private DonutState currentDonutState = DonutState.Full;

    void Start()
    {
        // Get hand tracking subsystem for spawning only
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        if (handSubsystems.Count > 0)
        {
            handSubsystem = handSubsystems[0];
        }
        
        if (showDebugLogs) Debug.Log("FeedingController started - simplified version");
    }

    void Update()
    {
        // Check for spawn key press
        if (Keyboard.current != null && Keyboard.current[Key.D].wasPressedThisFrame)
        {
            if (showDebugLogs) Debug.Log("D key pressed - spawning donut");
            SpawnDonut();
        }

        // Always check proximity if donut exists (no need to check if being held)
        if (currentDonut != null)
        {
            CheckProximityToFace();
        }
    }

    public void SpawnDonut()
    {
        if (currentDonut != null)
        {
            Debug.LogWarning("Donut already exists! Remove current donut first.");
            return;
        }

        // Spawn donut above the robot (more visible)
        Vector3 spawnPosition;
        if (robotBodyGameObject != null)
        {
            // Spawn above the robot's body
            spawnPosition = robotBodyGameObject.transform.position + Vector3.up * 0.2f;
            if (showDebugLogs) Debug.Log($"Feed: Using robot body position: {spawnPosition}");
        }
        else
        {
            // Fallback: spawn in front of camera
            spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
            if (showDebugLogs) Debug.Log($"Feed: Using camera fallback position: {spawnPosition}");
        }
        
        if (foodSphere == null)
        {
            Debug.LogError("Food sphere not assigned! Please assign the 'Food' sphere in the inspector.");
            return;
        }

        if (showDebugLogs) Debug.Log($"Feed: Food sphere found: {foodSphere.name}, active: {foodSphere.activeInHierarchy}");

        // Try to spawn near right hand, fallback to robot position
        Vector3 rightHandPosition = spawnPosition; // Default fallback
        if (handSubsystem != null && handSubsystem.rightHand.isTracked)
        {
            XRHandJoint rightPalm = handSubsystem.rightHand.GetJoint(XRHandJointID.Palm);
            if (rightPalm.TryGetPose(out Pose palmPose))
            {
                rightHandPosition = palmPose.position + Vector3.up * 0.1f; // Slightly above palm
                if (showDebugLogs) Debug.Log($"Feed: Using right hand position: {rightHandPosition}");
            }
        }

        // Move existing sphere to right hand position
        foodSphere.transform.position = rightHandPosition;
        foodSphere.SetActive(true);
        
        // Activate the full donut model if available
        if (fullDonutOnSphere != null)
        {
            // Deactivate all donut models first
            if (partialDonutOnSphere != null) partialDonutOnSphere.SetActive(false);
            if (mostlyEatenDonutOnSphere != null) mostlyEatenDonutOnSphere.SetActive(false);
            
            // Activate full donut model
            fullDonutOnSphere.SetActive(true);
            if (showDebugLogs) Debug.Log("Feed: Activated full donut model on spawn");
        }
        
        currentDonut = foodSphere;
        currentDonutState = DonutState.Full;
        if (showDebugLogs) Debug.Log($"Feed: Food sphere moved to position: {rightHandPosition}");
    }

    public void RemoveDonut()
    {
        if (currentDonut != null)
        {
            // Just hide the sphere instead of destroying it
            foodSphere.SetActive(false);
            currentDonut = null;
            StopEatingSequence();
        }
    }

    // All pinching/holding logic removed - handled by inspector modules

    private void CheckProximityToFace()
    {
        // Use robot body GameObject as the target (face mesh is created at runtime)
        if (robotBodyGameObject == null) 
        {
            if (showDebugLogs) Debug.LogWarning("Feed: No robot body GameObject assigned for proximity check!");
            return;
        }

        float distance = Vector3.Distance(currentDonut.transform.position, robotBodyGameObject.transform.position);
        
        // Always show distance for debugging
        if (showDebugLogs)
        {
            Debug.Log($"Feed: Distance to robot body: {distance:F3}m (threshold: {proximityThreshold:F3}m)");
        }
        
        if (distance <= proximityThreshold && !isEatingSequence)
        {
            // Check if no other emotion is playing
            if (CanStartEating())
            {
                if (showDebugLogs) Debug.Log("Feed: Close enough to robot - starting eating sequence!");
                StartEatingSequence();
            }
        }
        else if (distance > proximityThreshold && isEatingSequence)
        {
            // Stop eating if moved away
            if (showDebugLogs) Debug.Log("Feed: Moved away from robot - stopping eating sequence");
            StopEatingSequence();
        }
    }

    private bool CanStartEating()
    {
        // Check if no other emotion is currently playing
        // This would need to be implemented based on your emotion system
        return true; // Placeholder - implement based on your emotion controller
    }

    private void StartEatingSequence()
    {
        if (isEatingSequence) return;

        isEatingSequence = true;
        eatingCoroutine = StartCoroutine(EatingSequence());

        if (showDebugLogs)
            Debug.Log("Feed: Started eating sequence");
    }

    private void StopEatingSequence()
    {
        if (!isEatingSequence) return;

        isEatingSequence = false;
        if (eatingCoroutine != null)
        {
            StopCoroutine(eatingCoroutine);
            eatingCoroutine = null;
        }

        // Emotional blocking temporarily removed for debugging


        if (showDebugLogs)
            Debug.Log("Feed: Stopped eating sequence");
    }

    private IEnumerator EatingSequence()
    {
        // Emotional blocking temporarily removed for debugging


        // Bite 1: Full → Partial
        if (showDebugLogs)
            Debug.Log("Feed: Starting bite 1 - waiting for bite interval");
        yield return new WaitForSeconds(biteInterval);

        if (showDebugLogs)
            Debug.Log("Feed: Playing bite sound and swapping to partial donut");
        PlayBiteSound();
        SwapDonutModel(DonutState.Partial);

        // Bite 2: Partial → Mostly Eaten
        if (showDebugLogs)
            Debug.Log("Feed: Starting bite 2 - waiting for bite interval");
        yield return new WaitForSeconds(biteInterval);

        if (showDebugLogs)
            Debug.Log("Feed: Playing bite sound and swapping to mostly eaten donut");
        PlayBiteSound();
        SwapDonutModel(DonutState.MostlyEaten);

        // Bite 3: Mostly Eaten → Gone
        if (showDebugLogs)
            Debug.Log("Feed: Starting bite 3 - waiting for bite interval");
        yield return new WaitForSeconds(biteInterval);

        if (showDebugLogs)
            Debug.Log("Feed: Playing bite sound and removing donut");
        PlayBiteSound();
        SwapDonutModel(DonutState.Gone);

        yield return new WaitForSeconds(3.0f);

        Debug.Log("Feed: Calculating emotional response");
        var response = emotionModel.CalculateEmotionalResponse("Feeding");
        emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
        
        // Now clean up the donut after the emotional response
        if (currentDonut != null)
        {
            // Just hide the sphere instead of destroying it
            foodSphere.SetActive(false);
            currentDonut = null;
        }
            
        isEatingSequence = false;
    }

    private void SwapDonutModel(DonutState newState)
    {
        if (currentDonut == null) return;

        // Deactivate all donut models first
        if (fullDonutOnSphere != null) fullDonutOnSphere.SetActive(false);
        if (partialDonutOnSphere != null) partialDonutOnSphere.SetActive(false);
        if (mostlyEatenDonutOnSphere != null) mostlyEatenDonutOnSphere.SetActive(false);
        
        // Activate the appropriate donut model
        switch (newState)
        {
            case DonutState.Full:
                if (fullDonutOnSphere != null)
                {
                    fullDonutOnSphere.SetActive(true);
                    if (showDebugLogs) Debug.Log("Feed: Activated full donut model");
                }
                break;
            case DonutState.Partial:
                if (partialDonutOnSphere != null)
                {
                    partialDonutOnSphere.SetActive(true);
                    if (showDebugLogs) Debug.Log("Feed: Activated partial donut model");
                }
                break;
            case DonutState.MostlyEaten:
                if (mostlyEatenDonutOnSphere != null)
                {
                    mostlyEatenDonutOnSphere.SetActive(true);
                    if (showDebugLogs) Debug.Log("Feed: Activated mostly eaten donut model");
                }
                break;
            case DonutState.Gone:
                // All models already deactivated above
                if (showDebugLogs) Debug.Log("Feed: All donut models deactivated");
                break;
        }
        
        currentDonutState = newState;
    }

    private void PlayBiteSound()
    {
        if (showDebugLogs)
            Debug.Log($"Feed: PlayBiteSound called - audioController: {audioController != null}, soundName: {biteSoundName}");
            
        if (audioController != null)
        {
            audioController.PlaySound(biteSoundName);
            if (showDebugLogs)
                Debug.Log($"Feed: Played sound: {biteSoundName}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("Feed: AudioController is null - cannot play bite sound!");
        }
    }

    // Public methods for UI buttons
    public void OnSpawnButtonClicked()
    {
        SpawnDonut();
    }

    public void OnRemoveButtonClicked()
    {
        RemoveDonut();
    }
}
