using System;
using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;


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
    
    [SerializeField] private float maxSessionSeconds = 300f; // 5 min default
    private float _sessionStartTime;

    // ── Internals ──────────────────────────────────────────────────────────────
    private string logGameFileName;
    private string _logGameFilePath;
    private static StreamWriter _gameWriter;

    private string logExperimentFileName;
    private string _logExperimentFilePath;
    private static StreamWriter _ExperimentWriter;

    private GameObject player;
    private GameObject robot;

    
    // WIN CONDITIONS
    public bool agentWinning = false;
    public bool playerWinning = false;
    public List<GameObject> publicPositions = new();

    public static void AgentWin()
    {
        if (Instance == null)
            Debug.Log("Why is instance null?");
        
        Instance.agentWinning = true;
        Instance.Coordinated();
    }

    public static void PlayerWin()
    {   
        if (Instance == null)
            Debug.Log("Why is instance null?");
        
        Instance.playerWinning = true;
        Instance.Coordinated();
    }

    public bool Coordinated()
    {
        
        if (agentWinning && playerWinning)
        {
            Log("INTERACTION", "closed - both won");
            QuitGame();
            return true;
        }
        else
        {
            if (agentWinning)
                Log("INTERACTION", "robot reached goal");
            if (playerWinning)
                Log("INTERACTION", "player reached goal");    
            return false;   
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

        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Data"));
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Data", "Experiment"));


        _logGameFilePath = Path.Combine(Application.dataPath, "Data", logGameFileName);
        _gameWriter      = new StreamWriter(_logGameFilePath, append: true) { AutoFlush = true };
        if (new FileInfo(_logGameFilePath).Length == 0)
            _gameWriter.WriteLine("timestamp_unix,tag,message");
        _logExperimentFilePath = Path.Combine(Application.dataPath, "Data", "Experiment", logExperimentFileName);
        _ExperimentWriter      = new StreamWriter(_logExperimentFilePath, append: true) { AutoFlush = true };
        if (new FileInfo(_logExperimentFilePath).Length == 0)
            _ExperimentWriter.WriteLine("timestamp_unix,playerX,playerY,playerZ,robotX,robotY,robotZ");
    }

    public void StartGame()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        robot = GameObject.Find("Robot");
        // Main Menu -> Start
        string selectedPlayMode = playModeDropdown.options[playModeDropdown.value].text;
        logGameFileName = participantNameField.text + "_" + robotNameField.text + "_" + selectedPlayMode + ".csv";
        logExperimentFileName = participantNameField.text + "_" + robotNameField.text + ".csv";
        WriteHeaders();

        Log("INTERACTION", $"{participantNameField.text} and {robotNameField.text} started play mode {selectedPlayMode}");
        List<string> entries = new List<string>();

        foreach (GameObject obj in publicPositions)
        {
            if (obj != null)
            {
                Vector3 pos = obj.transform.position;
                entries.Add($"{obj.name}:({pos.x},{pos.y},{pos.z})");
            }
        }

        string dictLike = "{" + string.Join(";", entries) + "}";
        Log("STATIC_POSITIONS", $"{dictLike}");

        _sessionStartTime = Time.time;
        maxSessionSeconds = int.TryParse(maxSessionSecondsField.text, out int parsed) ? parsed : 120;
        Invoke(nameof(OutOfTime), maxSessionSeconds);
    }

    public void Update()
    {
        LogExperiment();
    }

    private void OutOfTime()
    {
        robotInterface.StopRobot();
        
        if (agentWinning)
            Log("INTERACTION", "only robot reached goal");
        if (playerWinning)
            Log("INTERACTION", "only player reached goal");    
        
        QuitGame();
        
    }

    private void OnApplicationQuit()
    {
        Log("INTERACTION", "closed");
        _gameWriter?.Close();
    }

    private void OnDestroy()
    {
        // Safety close in case the object is destroyed before quit
        if (Instance == this)
            _gameWriter?.Close();
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
        if (_gameWriter == null) return;

        double unixTime  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        // Escape commas so CSV stays clean
        string safeTag     = Escape(tag);
        string safeMessage = Escape(message);

        _gameWriter.WriteLine($"{unixTime},{safeTag},{safeMessage}");
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

    public void LogExperiment()
    {
        if (_ExperimentWriter == null) return;
        double unixTime  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        // Get the position of the player (tag Player)
        
        Vector3 playerPos = player.transform.position;
        string playerCoordinates = $"{playerPos.x},{playerPos.y},{playerPos.z}";

        // Get the position of the robot (object named Robot)
        
        Vector3 robotPos = robot.transform.position;
        string robotCoordinates = $"{robotPos.x},{robotPos.y},{robotPos.z}";

        // Write to file
        _ExperimentWriter.WriteLine($"{unixTime},{playerCoordinates},{robotCoordinates}");
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