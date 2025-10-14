using UnityEngine;
using System.Collections.Generic;

public class DebugLogToggle : MonoBehaviour
{
    [Header("Debug Toggle Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool toggleOnStart = false;
    
    [Header("Scripts to Toggle")]
    [SerializeField] private List<MonoBehaviour> scriptsToToggle = new List<MonoBehaviour>();
    
    [Header("GameObjects to Toggle")]
    [SerializeField] private List<GameObject> objectsToToggle = new List<GameObject>();
    
    private bool currentDebugState;
    
    void Start()
    {
        currentDebugState = showDebugLogs;
        
        if (showDebugLogs)
        {
            Debug.Log("DebugLogToggle: Started - Debug logs are ON");
        }
        
        if (toggleOnStart)
        {
            ToggleDebugLogs();
        }
    }
    
    [ContextMenu("Toggle Debug Logs")]
    public void ToggleDebugLogs()
    {
        currentDebugState = !currentDebugState;
        
        // Toggle debug logs in scripts
        foreach (MonoBehaviour script in scriptsToToggle)
        {
            if (script != null)
            {
                // Try to find and toggle showDebugLogs field
                var field = script.GetType().GetField("showDebugLogs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(script, currentDebugState);
                    Debug.Log($"DebugLogToggle: Set {script.GetType().Name}.showDebugLogs to {currentDebugState}");
                }
            }
        }
        
        // Toggle GameObjects
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(currentDebugState);
            }
        }
        
        Debug.Log($"DebugLogToggle: Debug logs {(currentDebugState ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Enable Debug Logs")]
    public void EnableDebugLogs()
    {
        currentDebugState = true;
        ToggleDebugLogs();
    }
    
    [ContextMenu("Disable Debug Logs")]
    public void DisableDebugLogs()
    {
        currentDebugState = false;
        ToggleDebugLogs();
    }
    
    // Public method for button calls
    public void ToggleFromButton()
    {
        ToggleDebugLogs();
    }
    
    // Public method to get current state
    public bool IsDebugEnabled()
    {
        return currentDebugState;
    }
}
