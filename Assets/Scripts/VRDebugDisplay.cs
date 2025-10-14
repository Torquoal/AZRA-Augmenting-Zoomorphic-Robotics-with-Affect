using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class VRDebugDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Display Settings")]
    [SerializeField] private int maxLogLines = 50;
    [SerializeField] private bool autoScroll = true;
    [SerializeField] private float fontSize = 24f;
    [SerializeField] private bool showTimestamp = true;
    [SerializeField] private bool showLogType = true;
    
    [Header("Filtering")]
    [SerializeField] private bool showLogs = true;
    [SerializeField] private bool showWarnings = true;
    [SerializeField] private bool showErrors = true;
    [SerializeField] private string filterText = "";
    
    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.1f; // Update every 100ms
    [SerializeField] private bool enableLogCapture = true;
    
    private List<string> logEntries = new List<string>();
    private StringBuilder logBuilder = new StringBuilder();
    private float lastUpdateTime;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeDebugDisplay();
    }
    
    void InitializeDebugDisplay()
    {
        if (debugText == null)
        {
            Debug.LogError("VRDebugDisplay: No TextMeshProUGUI assigned!");
            return;
        }
        
        // Set up text properties
        debugText.fontSize = fontSize;
        debugText.text = "VR Debug Log Initialized...\n";
        debugText.color = Color.white;
        
        // Set up scroll rect (optional - can be null)
        if (scrollRect != null)
        {
            if (scrollRect.verticalScrollbar != null)
            {
                scrollRect.verticalScrollbar.value = 1f; // Start at top
                Debug.Log("VRDebugDisplay: ScrollRect found and configured");
            }
            else
            {
                Debug.LogWarning("VRDebugDisplay: ScrollRect found but no vertical scrollbar");
            }
        }
        else
        {
            Debug.LogWarning("VRDebugDisplay: No ScrollRect assigned - display will work without scrolling");
        }
        
        // Subscribe to Unity's log events
        if (enableLogCapture)
        {
            Application.logMessageReceived += OnLogMessageReceived;
            Debug.Log("VRDebugDisplay: Log capture enabled and subscribed");
        }
        else
        {
            Debug.LogWarning("VRDebugDisplay: Log capture is disabled!");
        }
        
        isInitialized = true;
        Debug.Log("VRDebugDisplay: Debug display initialized successfully");
        
    }
    
    
    void OnDestroy()
    {
        // Unsubscribe from log events
        Application.logMessageReceived -= OnLogMessageReceived;
    }
    
    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (!isInitialized) return;
        
        // Filter by log type
        if (!ShouldShowLogType(type)) return;
        
        // Filter by text content
        if (!string.IsNullOrEmpty(filterText) && !logString.ToLower().Contains(filterText.ToLower()))
            return;
        
        // Create formatted log entry
        string timestamp = showTimestamp ? System.DateTime.Now.ToString("HH:mm:ss") : "";
        string logType = showLogType ? $"[{type}]" : "";
        string entry = $"{timestamp} {logType} {logString}";
        
        // Add to log entries
        logEntries.Add(entry);
        
        // Limit log entries
        if (logEntries.Count > maxLogLines)
        {
            logEntries.RemoveAt(0);
        }
        
        // Force immediate update
        UpdateDisplay();
        
        // Debug to console to verify it's working
        Debug.Log($"VRDebugDisplay: Captured log - {entry}");
    }
    
    bool ShouldShowLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Log: return showLogs;
            case LogType.Warning: return showWarnings;
            case LogType.Error: return showErrors;
            case LogType.Exception: return showErrors;
            default: return true;
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Throttled update for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    void UpdateDisplay()
    {
        if (debugText == null) return;
        
        // Build log text
        logBuilder.Clear();
        foreach (string entry in logEntries)
        {
            logBuilder.AppendLine(entry);
        }
        
        debugText.text = logBuilder.ToString();
        
        // Auto scroll to bottom (only if scroll rect exists)
        if (autoScroll && scrollRect != null)
        {
            StartCoroutine(ScrollToBottom());
        }
    }
    
    System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null && scrollRect.verticalScrollbar != null)
        {
            scrollRect.verticalScrollbar.value = 0f;
        }
    }
    
    // Public methods for external control
    public void ClearLogs()
    {
        logEntries.Clear();
        if (debugText != null)
        {
            debugText.text = "Logs cleared.\n";
        }
        Debug.Log("VRDebugDisplay: Logs cleared");
    }
    
    public void SetFilterText(string filter)
    {
        filterText = filter;
        Debug.Log($"VRDebugDisplay: Filter set to '{filter}'");
    }
    
    public void ToggleLogType(LogType type, bool enabled)
    {
        switch (type)
        {
            case LogType.Log: showLogs = enabled; break;
            case LogType.Warning: showWarnings = enabled; break;
            case LogType.Error: showErrors = enabled; break;
        }
        Debug.Log($"VRDebugDisplay: {type} logs {(enabled ? "enabled" : "disabled")}");
    }
    
    public void SetMaxLines(int lines)
    {
        maxLogLines = Mathf.Max(10, lines);
        Debug.Log($"VRDebugDisplay: Max lines set to {maxLogLines}");
    }
    
    public void SetFontSize(float size)
    {
        fontSize = size;
        if (debugText != null)
        {
            debugText.fontSize = fontSize;
        }
        Debug.Log($"VRDebugDisplay: Font size set to {fontSize}");
    }
    
    // Context menu methods for easy testing
    [ContextMenu("Test Log Message")]
    public void TestLogMessage()
    {
        Debug.Log("VRDebugDisplay: Test log message");
        Debug.LogWarning("VRDebugDisplay: Test warning message");
        Debug.LogError("VRDebugDisplay: Test error message");
    }
    
    [ContextMenu("Manual Test Display")]
    public void ManualTestDisplay()
    {
        // Manually add test messages to bypass log capture
        logEntries.Add("Manual Test: This is a manual test message");
        logEntries.Add("Manual Test: This should appear in the debug panel");
        logEntries.Add("Manual Test: If you see this, the display is working");
        
        UpdateDisplay();
        Debug.Log("VRDebugDisplay: Manual test messages added");
    }
    
    [ContextMenu("Clear All Logs")]
    public void ClearAllLogs()
    {
        ClearLogs();
    }
    
    [ContextMenu("Show Current Settings")]
    public void ShowCurrentSettings()
    {
        Debug.Log($"VRDebugDisplay Settings - Max Lines: {maxLogLines}, Font Size: {fontSize}, Auto Scroll: {autoScroll}");
        Debug.Log($"VRDebugDisplay Settings - Show Logs: {showLogs}, Show Warnings: {showWarnings}, Show Errors: {showErrors}");
        Debug.Log($"VRDebugDisplay Settings - Filter: '{filterText}', Update Interval: {updateInterval}s");
    }
}
