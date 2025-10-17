using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class VoskRecognizer
{
    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_model_new(string model_path);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_new(IntPtr model, float sample_rate);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern void vosk_recognizer_free(IntPtr recognizer);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern void vosk_model_free(IntPtr model);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool vosk_recognizer_accept_waveform(IntPtr recognizer, short[] data, int length);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_result(IntPtr recognizer);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_partial_result(IntPtr recognizer);

    private IntPtr model;
    private IntPtr recognizer;
    private bool isInitialized;
    private bool showDebugLogs = false;  // Debug log control

    public void SetDebugLogging(bool enabled)
    {
        showDebugLogs = enabled;
    }

    public bool Initialize(int sampleRate = 16000)
    {
        try
        {
            // IMPORTANT: Vosk Model Setup for Android/Quest
            // On Android platforms (including Quest), StreamingAssets files are inside the APK and cannot be accessed
            // by native libraries like Vosk. The model files must be manually copied to the persistent data path.
            // 
            // Setup Instructions:
            // 1. Build and deploy your app to Quest
            // 2. Run the app and check the debug log for the persistent data path
            // 3. Manually copy the VoskModel folder from Assets/StreamingAssets/VoskModel/ to the persistent data path
            // 4. The path will be something like: /storage/emulated/0/Android/data/com.yourcompany.yourapp/files/VoskModel
            // 5. Copy the entire folder structure including all subdirectories (am, conf, graph, ivector)
            
            string targetModelPath = System.IO.Path.Combine(Application.persistentDataPath, "VoskModel");
            
            if (showDebugLogs)
            {
                Debug.Log($"VoskRecognizer: Loading model from {targetModelPath}");
            }

            model = vosk_model_new(targetModelPath);
            if (model == IntPtr.Zero)
            {
                Debug.LogError($"VoskRecognizer: Failed to create model from path: {targetModelPath}");
                Debug.LogError("VoskRecognizer: Make sure the VoskModel folder has been manually copied to the persistent data path.");
                return false;
            }

            recognizer = vosk_recognizer_new(model, sampleRate);
            if (recognizer == IntPtr.Zero)
            {
                Debug.LogError("VoskRecognizer: Failed to create recognizer");
                vosk_model_free(model);
                return false;
            }

            isInitialized = true;
            if (showDebugLogs)
                Debug.Log("VoskRecognizer: Initialized successfully");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"VoskRecognizer: Initialization error - {e.Message}");
            Debug.LogException(e);
            return false;
        }
    }

    public string ProcessAudio(float[] audioData)
    {
        if (!isInitialized || audioData == null || audioData.Length == 0)
            return null;

        try
        {
            // Convert float audio to 16-bit PCM
            short[] pcmData = new short[audioData.Length];
            
            // Calculate some stats about the audio
            float maxAmp = 0f;
            float minAmp = 0f;
            for (int i = 0; i < audioData.Length; i++)
            {
                maxAmp = Mathf.Max(maxAmp, audioData[i]);
                minAmp = Mathf.Min(minAmp, audioData[i]);
                pcmData[i] = (short)(audioData[i] * 32768f);
            }

            // Process the audio
            if (vosk_recognizer_accept_waveform(recognizer, pcmData, pcmData.Length))
            {
                IntPtr resultPtr = vosk_recognizer_result(recognizer);
                if (resultPtr != IntPtr.Zero)
                {
                    string result = Marshal.PtrToStringAnsi(resultPtr);
                    if (showDebugLogs)
                        Debug.Log($"VoskRecognizer: Raw result: {result}");
                    return result;
                }
            }
            else
            {
                // Check for partial results
                IntPtr partialPtr = vosk_recognizer_partial_result(recognizer);
                if (partialPtr != IntPtr.Zero)
                {
                    string partial = Marshal.PtrToStringAnsi(partialPtr);
                    if (showDebugLogs)
                        Debug.Log($"VoskRecognizer: Raw partial: {partial}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"VoskRecognizer: Processing error - {e.Message}");
            Debug.LogException(e);
        }

        return null;
    }

    public void Cleanup()
    {
        if (isInitialized)
        {
            if (recognizer != IntPtr.Zero)
            {
                vosk_recognizer_free(recognizer);
                recognizer = IntPtr.Zero;
            }
            if (model != IntPtr.Zero)
            {
                vosk_model_free(model);
                model = IntPtr.Zero;
            }
            isInitialized = false;
        }
    }
}