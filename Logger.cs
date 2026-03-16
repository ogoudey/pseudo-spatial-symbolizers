using System;
using System.IO;
using UnityEngine;
using TMPro;


/// <summary>
/// Singleton logger. Access from any script via Logger.Instance.Log(...)
/// Log format is CSV-friendly and easily plottable with pyplot:
///   timestamp_unix,timestamp_iso,tag,message
/// </summary>
public class Logger : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static Logger Instance { get; private set; }

    // ── Config ─────────────────────────────────────────────────────────────────
    [Tooltip("Log filename (placed in Application.persistentDataPath)")]


    public WakeUpRobot robotInterface;
    public TMP_InputField participantNameField;
    public TMP_InputField robotNameField;
    public TMP_InputField maxSessionSecondsField;
    public TMP_Dropdown playModeDropdown;
    private string logFileName;
    [SerializeField] private float maxSessionSeconds = 300f; // 5 min default
    private float _sessionStartTime;

    // ── Internals ──────────────────────────────────────────────────────────────
    private string _logFilePath;
    private static StreamWriter _writer;
    
    // WIN CONDITIONS
    public bool agentWinning = false;
    public bool playerWinning = false;
    public static void AgentWin()
    {
        if (Instance == null)
            Debug.Log("Why is instance null?");
        Log("INTERACTION", "robot reached goal");
        Instance.agentWinning = true;
        Instance.Coordinated();
    }

    public static void PlayerWin()
    {   
        if (Instance == null)
            Debug.Log("Why is instance null?");
        Log("INTERACTION", "player reached goal");
        Instance.agentWinning = true;
        Instance.Coordinated();
    }

    public void Coordinated()
    {
        if (agentWinning && playerWinning)
        {
            Log("INTERACTION", "closed - both won");
            QuitGame();
        }
            
    }



    private void WriteHeaders()
    {
        Debug.Log("Writing headers...");
        // Enforce singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Open log file (append so multiple sessions are preserved)
        _logFilePath = Path.Combine(Application.dataPath, "Data", logFileName);
        _writer      = new StreamWriter(_logFilePath, append: true) { AutoFlush = true };

        // Write CSV header if the file is brand-new
        if (new FileInfo(_logFilePath).Length == 0)
            _writer.WriteLine("timestamp_unix,tag,message");
    }

    public void StartGame()
    {
        // Main Menu -> Start
        string selectedPlayMode = playModeDropdown.options[playModeDropdown.value].text;
        logFileName = selectedPlayMode + "_" + participantNameField.text + "_" + robotNameField.text + ".csv";
        WriteHeaders();
        Log("INTERACTION", $"{participantNameField.text} and {robotNameField.text} started play mode {selectedPlayMode}");
        _sessionStartTime = Time.time;
        maxSessionSeconds = int.TryParse(maxSessionSecondsField.text, out int parsed) ? parsed : 120;
        Invoke(nameof(OutOfTime), maxSessionSeconds);
    }

    private void OutOfTime()
    {
        robotInterface.StopRobot();
        Log("INTERACTION", "closed - out of time");
        QuitGame();
        
    }

    private void OnApplicationQuit()
    {
        Log("INTERACTION", "closed");
        _writer?.Close();
    }

    private void OnDestroy()
    {
        // Safety close in case the object is destroyed before quit
        if (Instance == this)
            _writer?.Close();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Public API
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Write a timestamped entry to the log file.
    /// </summary>
    /// <param name="tag">Short category label (e.g. "PLAYER", "AI", "SESSION").</param>
    /// <param name="message">Free-form message. Commas are escaped automatically.</param>
    public static void Log(string tag, string message)
    {
        if (_writer == null) return;

        double unixTime  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        // Escape commas so CSV stays clean
        string safeTag     = Escape(tag);
        string safeMessage = Escape(message);

        _writer.WriteLine($"{unixTime},{safeTag},{safeMessage}");
    }

    /// <summary>Convenience overload — logs with tag "INFO".</summary>
    public static void Log(string message) => Log("INFO", message);

    // ══════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════════

    private static string Escape(string s)
    {
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            return $"\"{s.Replace("\"", "\"\"")}\"";
        return s;
    }

    

    public void QuitGame()
    {
        // another Quit(), same as Pause > Quit
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ── LogTimePercent ─────────────────────────────────────────────────────────
    /// <summary>
    /// Logs how far through the permitted session time we are.
    /// e.g. "50%permittedtime=150 seconds"
    /// Call this whenever you want a time-progress breadcrumb.
    /// </summary>
    public void LogTimePercent()
    {
        float elapsed  = Time.time - _sessionStartTime;
        int   percent  = Mathf.RoundToInt((elapsed / maxSessionSeconds) * 100f);
        int   elapsedS = Mathf.FloorToInt(elapsed);
        Log("INFO", $"{percent}%permittedtime={elapsedS} seconds");
    }
}