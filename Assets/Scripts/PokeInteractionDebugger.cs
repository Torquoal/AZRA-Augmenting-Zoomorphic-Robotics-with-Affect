using UnityEngine;
using UnityEngine.UI;
using Meta.XR.MRUtilityKit;

public class PokeInteractionDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private float debugInterval = 1.0f;
    
    private float lastDebugTime = 0f;
    
    void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("PokeInteractionDebugger started - monitoring poke interactions");
        }
    }
    
    void Update()
    {
        if (enableDebugLogs && Time.time - lastDebugTime >= debugInterval)
        {
            CheckPokeInteractionStatus();
            lastDebugTime = Time.time;
        }
    }
    
    void CheckPokeInteractionStatus()
    {
        // Check for poke interaction components using reflection
        MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
        int pokeCount = 0;
        
        foreach (var component in allComponents)
        {
            if (component.GetType().Name.Contains("Poke") || component.GetType().Name.Contains("ISDK"))
            {
                pokeCount++;
                Debug.Log($"PokeInteraction on: {component.gameObject.name} (Type: {component.GetType().Name})");
            }
        }
        
        Debug.Log($"Found {pokeCount} poke interaction components");
        
        // Check for PointableCanvas components using reflection
        MonoBehaviour[] allComponents2 = FindObjectsOfType<MonoBehaviour>();
        int pointableCount = 0;
        
        foreach (var component in allComponents2)
        {
            if (component.GetType().Name.Contains("Pointable") || component.GetType().Name.Contains("Canvas"))
            {
                pointableCount++;
                Debug.Log($"PointableCanvas on: {component.gameObject.name} (Type: {component.GetType().Name})");
            }
        }
        
        Debug.Log($"Found {pointableCount} PointableCanvas components");
        
        // Check for buttons
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log($"Found {buttons.Length} Button components");
        
        int buttonsWithPoke = 0;
        foreach (var button in buttons)
        {
            // Check for any poke interaction component
            Component[] components = button.GetComponents<Component>();
            bool hasPoke = false;
            foreach (Component comp in components)
            {
                if (comp.GetType().Name.Contains("Poke") || comp.GetType().Name.Contains("ISDK"))
                {
                    hasPoke = true;
                    break;
                }
            }
            if (hasPoke)
            {
                buttonsWithPoke++;
            }
        }
        
        Debug.Log($"Buttons with PokeInteraction: {buttonsWithPoke}/{buttons.Length}");
    }
    
    [ContextMenu("Force Test All Buttons")]
    public void ForceTestAllButtons()
    {
        Debug.Log("=== FORCE TESTING ALL BUTTONS ===");
        
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            Debug.Log($"Testing button: {button.name}");
            button.onClick.Invoke();
        }
        
        Debug.Log("=== FORCE TEST COMPLETE ===");
    }
}
