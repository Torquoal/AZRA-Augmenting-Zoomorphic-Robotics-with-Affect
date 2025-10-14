using UnityEngine;

public class GameObjectToggle : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField] private GameObject targetObject; // The GameObject to toggle
    [SerializeField] private bool startActive = true; // Initial state
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Toggle Behavior")]
    [SerializeField] private bool toggleOnStart = false; // Toggle immediately when script starts
    [SerializeField] private bool invertToggle = false; // If true, toggles the opposite way
    
    private bool currentState;
    
    void Start()
    {
        // If no target assigned, use this GameObject
        if (targetObject == null)
        {
            targetObject = gameObject;
        }
        
        // Set initial state
        currentState = startActive;
        targetObject.SetActive(currentState);
        
        if (showDebugLogs)
        {
            Debug.Log($"GameObjectToggle: {targetObject.name} initialized as {(currentState ? "active" : "inactive")}");
        }
        
        // Toggle on start if requested
        if (toggleOnStart)
        {
            Toggle();
        }
    }
    
    [ContextMenu("Toggle GameObject")]
    public void Toggle()
    {
        if (targetObject == null)
        {
            Debug.LogError("GameObjectToggle: No target object assigned!");
            return;
        }
        
        // Toggle the state
        currentState = !currentState;
        
        // Apply inversion if enabled
        if (invertToggle)
        {
            currentState = !currentState;
        }
        
        // Set the active state
        targetObject.SetActive(currentState);
        
        if (showDebugLogs)
        {
            Debug.Log($"GameObjectToggle: {targetObject.name} toggled to {(currentState ? "active" : "inactive")}");
        }
    }
    
    [ContextMenu("Set Active")]
    public void SetActive()
    {
        if (targetObject == null) return;
        
        currentState = true;
        targetObject.SetActive(true);
        
        if (showDebugLogs)
        {
            Debug.Log($"GameObjectToggle: {targetObject.name} set to active");
        }
    }
    
    [ContextMenu("Set Inactive")]
    public void SetInactive()
    {
        if (targetObject == null) return;
        
        currentState = false;
        targetObject.SetActive(false);
        
        if (showDebugLogs)
        {
            Debug.Log($"GameObjectToggle: {targetObject.name} set to inactive");
        }
    }
    
    // Public method for external scripts to call
    public void ToggleFromButton()
    {
        Toggle();
    }
    
    // Public method to get current state
    public bool IsActive()
    {
        return currentState;
    }
    
    // Public method to set state from external scripts
    public void SetState(bool active)
    {
        if (targetObject == null) return;
        
        currentState = active;
        targetObject.SetActive(active);
        
        if (showDebugLogs)
        {
            Debug.Log($"GameObjectToggle: {targetObject.name} set to {(active ? "active" : "inactive")}");
        }
    }
}
