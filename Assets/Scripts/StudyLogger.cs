using UnityEngine;
using System.IO;
using System.Text;

public class StudyLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private string fileName = "participant_log.csv";
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Participant Info")]
    [SerializeField] private string participantID = "P001";
    
    [Header("References")]
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private EmotionController emotionController;
    
    private string filePath;
    private bool isInitialized = false;
    private StringBuilder logBuilder;
    
    void Start()
    {
        InitializeLogger();
    }
    
    void InitializeLogger()
    {
        if (!enableLogging)
        {
            if (showDebugLogs)
            {
                Debug.Log("StudyLogger: Logging disabled");
            }
            return;
        }
        
        // Create log file path
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        
        // Initialize string builder
        logBuilder = new StringBuilder();
        
        // Write CSV header
        WriteCSVHeader();
        
        isInitialized = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Initialized - Log file: {filePath}");
        }
    }
    
    void WriteCSVHeader()
    {
        string header = "ParticipantID,Timestamp,EventType,EmotionDisplayed,Valence,Arousal,TouchGauge,RestGauge,SocialGauge,HungerGauge,TriggerEvent";
        WriteToFile(header);
    }
    
    public void LogEmotionalResponse(string triggerEvent, string emotionDisplayed)
    {
        if (!isInitialized || !enableLogging)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("StudyLogger: Cannot log - logger not initialized or disabled");
            }
            return;
        }
        
        // Get current mood values from EmotionModel
        float valence = emotionModel.CurrentValence;
        float arousal = emotionModel.CurrentArousal;
        float touchGauge = emotionModel.TouchGauge;
        float restGauge = emotionModel.RestGauge;
        float socialGauge = emotionModel.SocialGauge;
        float hungerGauge = emotionModel.HungerGauge;
        
        // Create log entry
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"{participantID},{timestamp},EmotionalResponse,{emotionDisplayed},{valence:F3},{arousal:F3},{touchGauge:F3},{restGauge:F3},{socialGauge:F3},{hungerGauge:F3},{triggerEvent}";
        
        WriteToFile(logEntry);
        
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Logged emotional response - {emotionDisplayed} (Valence: {valence:F3}, Arousal: {arousal:F3})");
        }
    }
    
    public void LogMoodChange(string triggerEvent)
    {
        if (!isInitialized || !enableLogging)
        {
            return;
        }
        
        // Get current mood values from EmotionModel
        float valence = emotionModel.CurrentValence;
        float arousal = emotionModel.CurrentArousal;
        float touchGauge = emotionModel.TouchGauge;
        float restGauge = emotionModel.RestGauge;
        float socialGauge = emotionModel.SocialGauge;
        float hungerGauge = emotionModel.HungerGauge;
        
        // Create log entry
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"{participantID},{timestamp},MoodChange,None,{valence:F3},{arousal:F3},{touchGauge:F3},{restGauge:F3},{socialGauge:F3},{hungerGauge:F3},{triggerEvent}";
        
        WriteToFile(logEntry);
        
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Logged mood change - {triggerEvent} (Valence: {valence:F3}, Arousal: {arousal:F3})");
        }
    }
    
    public void LogCustomEvent(string eventType, string description, float customValue = 0f)
    {
        if (!isInitialized || !enableLogging)
        {
            return;
        }
        
        // Get current mood values from EmotionModel
        float valence = emotionModel.CurrentValence;
        float arousal = emotionModel.CurrentArousal;
        float touchGauge = emotionModel.TouchGauge;
        float restGauge = emotionModel.RestGauge;
        float socialGauge = emotionModel.SocialGauge;
        float hungerGauge = emotionModel.HungerGauge;
        
        // Create log entry
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"{participantID},{timestamp},{eventType},{description},{valence:F3},{arousal:F3},{touchGauge:F3},{restGauge:F3},{socialGauge:F3},{hungerGauge:F3},{customValue:F3}";
        
        WriteToFile(logEntry);
        
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Logged custom event - {eventType}: {description}");
        }
    }
    
    void WriteToFile(string logEntry)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"StudyLogger: Failed to write to file - {e.Message}");
        }
    }
    
    // Public methods for external control
    public void SetLoggingEnabled(bool enabled)
    {
        enableLogging = enabled;
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Logging {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    public void SetFileName(string newFileName)
    {
        fileName = newFileName;
        if (isInitialized)
        {
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            if (showDebugLogs)
            {
                Debug.Log($"StudyLogger: File name changed to {fileName}");
            }
        }
    }
    
    public void SetParticipantID(string newParticipantID)
    {
        participantID = newParticipantID;
        if (showDebugLogs)
        {
            Debug.Log($"StudyLogger: Participant ID changed to {participantID}");
        }
    }
    
    public string GetLogFilePath()
    {
        return filePath;
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Log Emotional Response")]
    public void TestLogEmotionalResponse()
    {
        LogEmotionalResponse("TestTrigger", "TestEmotion");
    }
    
    [ContextMenu("Test Log Mood Change")]
    public void TestLogMoodChange()
    {
        LogMoodChange("TestMoodChange");
    }
    
    [ContextMenu("Show Log File Path")]
    public void ShowLogFilePath()
    {
        Debug.Log($"StudyLogger: Log file path: {filePath}");
    }
    
    [ContextMenu("Open Log File Location")]
    public void OpenLogFileLocation()
    {
        if (File.Exists(filePath))
        {
            Application.OpenURL("file://" + Path.GetDirectoryName(filePath));
        }
        else
        {
            Debug.LogWarning("StudyLogger: Log file does not exist yet");
        }
    }
}
