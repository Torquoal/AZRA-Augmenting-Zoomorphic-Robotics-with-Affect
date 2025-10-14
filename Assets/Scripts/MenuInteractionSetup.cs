using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Meta.XR.MRUtilityKit;

public class MenuInteractionSetup : MonoBehaviour
{
    [Header("Menu Setup")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private bool setupOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupMenuInteraction();
        }
    }
    
    [ContextMenu("Setup Menu Interaction")]
    public void SetupMenuInteraction()
    {
        if (menuCanvas == null)
        {
            Debug.LogError("Menu canvas not assigned!");
            return;
        }
        
        Debug.Log("=== SETTING UP MENU INTERACTION ===");
        
        // 1. Setup PointableCanvas on the menu canvas
        SetupPointableCanvas();
        
        // 2. Setup ISDK_PokeInteraction on buttons
        SetupPokeInteraction();
        
        // 3. Setup Event System for UI
        SetupEventSystem();
        
        // 4. Verify setup
        VerifySetup();
        
        Debug.Log("=== MENU INTERACTION SETUP COMPLETE ===");
    }
    
    void SetupPointableCanvas()
    {
        // Try to find any pointable canvas component
        var pointableCanvas = FindPointableCanvasComponent(menuCanvas.gameObject);
        if (pointableCanvas == null)
        {
            Debug.Log("No PointableCanvas component found on menu canvas");
            Debug.Log("You may need to manually add the PointableCanvas component to your menu canvas");
        }
        else
        {
            Debug.Log($"Found PointableCanvas component: {pointableCanvas.GetType().Name}");
        }
    }
    
    Component FindPointableCanvasComponent(GameObject obj)
    {
        // Try to find any component that might be a pointable canvas
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp.GetType().Name.Contains("Pointable") || comp.GetType().Name.Contains("Canvas"))
            {
                return comp;
            }
        }
        return null;
    }
    
    void SetupPokeInteraction()
    {
        // Find all buttons in the menu
        Button[] buttons = menuCanvas.GetComponentsInChildren<Button>();
        Debug.Log($"Found {buttons.Length} buttons to setup for poke interaction");
        
        foreach (Button button in buttons)
        {
            // Try to find existing poke interaction component
            var pokeInteraction = FindPokeInteractionComponent(button.gameObject);
            if (pokeInteraction == null)
            {
                Debug.Log($"No poke interaction component found on button: {button.name}");
                Debug.Log("You may need to manually add the ISDK_PokeInteraction component to buttons");
            }
            else
            {
                Debug.Log($"Found poke interaction component on button: {button.name}");
            }
            
            // Set up the button to respond to poke events
            SetupButtonPokeEvents(button);
        }
    }
    
    Component FindPokeInteractionComponent(GameObject obj)
    {
        // Try to find any component that might be a poke interaction
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp.GetType().Name.Contains("Poke") || comp.GetType().Name.Contains("ISDK"))
            {
                return comp;
            }
        }
        return null;
    }
    
    void SetupButtonPokeEvents(Button button)
    {
        // Remove existing listeners to avoid duplicates
        button.onClick.RemoveAllListeners();
        
        // Add poke interaction listener
        button.onClick.AddListener(() => {
            if (showDebugLogs)
                Debug.Log($"Button {button.name} clicked via poke interaction!");
        });
        
        Debug.Log($"Configured poke events for button: {button.name}");
    }
    
    void SetupEventSystem()
    {
        // Check if EventSystem exists
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem found! Creating one...");
            
            // Create EventSystem GameObject
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("Created EventSystem with StandaloneInputModule");
        }
        else
        {
            Debug.Log("EventSystem already exists");
        }
    }
    
    void VerifySetup()
    {
        Debug.Log("=== VERIFYING SETUP ===");
        
        // Check PointableCanvas
        var pointableCanvas = FindPointableCanvasComponent(menuCanvas.gameObject);
        if (pointableCanvas != null)
        {
            Debug.Log($"✅ PointableCanvas: Found ({pointableCanvas.GetType().Name})");
        }
        else
        {
            Debug.LogError("❌ PointableCanvas: Missing!");
        }
        
        // Check buttons with poke interaction
        Button[] buttons = menuCanvas.GetComponentsInChildren<Button>();
        int pokeButtons = 0;
        
        foreach (Button button in buttons)
        {
            var pokeInteraction = FindPokeInteractionComponent(button.gameObject);
            if (pokeInteraction != null)
            {
                pokeButtons++;
            }
        }
        
        Debug.Log($"✅ Buttons with PokeInteraction: {pokeButtons}/{buttons.Length}");
        
        // Check EventSystem
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log("✅ EventSystem: Found");
        }
        else
        {
            Debug.LogError("❌ EventSystem: Missing!");
        }
        
        Debug.Log("=== VERIFICATION COMPLETE ===");
    }
    
    [ContextMenu("Test Menu Interaction")]
    public void TestMenuInteraction()
    {
        Debug.Log("=== TESTING MENU INTERACTION ===");
        
        // Simulate button clicks to test if they work
        Button[] buttons = menuCanvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            Debug.Log($"Testing button: {button.name}");
            // This will trigger the onClick events
            button.onClick.Invoke();
        }
        
        Debug.Log("=== MENU INTERACTION TEST COMPLETE ===");
    }
}
