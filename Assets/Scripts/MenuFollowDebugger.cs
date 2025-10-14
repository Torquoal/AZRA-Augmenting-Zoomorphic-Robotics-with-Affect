using UnityEngine;

public class MenuFollowDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float debugInterval = 1.0f;
    
    [Header("References")]
    [SerializeField] private Transform userTransform;
    [SerializeField] private Transform menuTransform;
    [SerializeField] private MenuFollowSystem menuFollowSystem;
    
    private float lastDebugTime = 0f;
    
    void Start()
    {
        // Find references if not assigned
        if (userTransform == null)
        {
            userTransform = Camera.main.transform;
        }
        
        if (menuTransform == null)
        {
            menuTransform = transform;
        }
        
        if (menuFollowSystem == null)
        {
            menuFollowSystem = FindObjectOfType<MenuFollowSystem>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("MenuFollowDebugger: Started debugging menu follow system");
        }
    }
    
    void Update()
    {
        if (showDebugLogs && Time.time - lastDebugTime >= debugInterval)
        {
            DebugMenuFollowStatus();
            lastDebugTime = Time.time;
        }
    }
    
    void DebugMenuFollowStatus()
    {
        if (userTransform == null || menuTransform == null)
        {
            Debug.LogError("MenuFollowDebugger: Missing references!");
            return;
        }
        
        float distance = Vector3.Distance(userTransform.position, menuTransform.position);
        
        Debug.Log($"=== MENU FOLLOW DEBUG ===");
        Debug.Log($"User position: {userTransform.position}");
        Debug.Log($"Menu position: {menuTransform.position}");
        Debug.Log($"Distance between user and menu: {distance:F2}m");
        Debug.Log($"User rotation: {userTransform.rotation.eulerAngles}");
        Debug.Log($"Menu rotation: {menuTransform.rotation.eulerAngles}");
        
        if (menuFollowSystem != null)
        {
            Debug.Log($"MenuFollowSystem enabled: {menuFollowSystem.enabled}");
        }
        else
        {
            Debug.LogWarning("MenuFollowSystem: No MenuFollowSystem found!");
        }
    }
    
    [ContextMenu("Reset User Position")]
    public void ResetUserPosition()
    {
        if (userTransform == null) return;
        
        // Reset user to origin
        userTransform.position = Vector3.zero;
        userTransform.rotation = Quaternion.identity;
        
        Debug.Log("MenuFollowDebugger: Reset user position to origin");
    }
    
    [ContextMenu("Reset Menu Position")]
    public void ResetMenuPosition()
    {
        if (menuTransform == null) return;
        
        // Reset menu to a safe position
        menuTransform.position = new Vector3(0, 1.5f, 1);
        menuTransform.rotation = Quaternion.identity;
        
        Debug.Log("MenuFollowDebugger: Reset menu position");
    }
    
    [ContextMenu("Disable Menu Follow")]
    public void DisableMenuFollow()
    {
        if (menuFollowSystem != null)
        {
            menuFollowSystem.enabled = false;
            Debug.Log("MenuFollowDebugger: Disabled MenuFollowSystem");
        }
    }
    
    [ContextMenu("Enable Menu Follow")]
    public void EnableMenuFollow()
    {
        if (menuFollowSystem != null)
        {
            menuFollowSystem.enabled = true;
            Debug.Log("MenuFollowDebugger: Enabled MenuFollowSystem");
        }
    }
}
