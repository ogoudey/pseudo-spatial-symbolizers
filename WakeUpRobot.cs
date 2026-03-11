using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


public class WakeUpRobot : MonoBehaviour
{
    public static WakeUpRobot Instance;
    public float interactRadius = 2f;
    
    public string robotName = "defaultRobot";
    private Process robotProcess;
    int pidGroup;
    public Light robotLight;

    void Start()
    {

    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
        bool playerNearby = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerNearby = true;
                break;
            }
        }

        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (robotProcess == null || robotProcess.HasExited)
            {
                StartRobot();
            }
            else
            {
                StopRobot();
            }
        }
    }



    public void StartRobot()
    {
        string scriptPath = Application.dataPath + "/Scripts/unity_robot_run.sh";
        string logPath = Application.dataPath + "/unity_robot_log.txt";

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "/bin/bash";
        psi.Arguments = $"-c \"export VLA_STAR_PATH=Robotics/Projects/VLA_Star; setsid {scriptPath} {robotName} > {logPath} 2>&1\"";
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        robotProcess = Process.Start(psi);

        [DllImport("libc")]
        static extern int setpgid(int pid, int pgid);
        setpgid(robotProcess.Id, robotProcess.Id);
        UnityEngine.Debug.Log("Started robot subprocess");

        
    }

    public void StopRobot()
    {
        if (robotProcess != null && !robotProcess.HasExited)
        {
            KillProcessGroup();
        }

        robotProcess = null;
        robotLight.enabled = false; // Python wnt get a chance to "say goodbye"
        UnityEngine.Debug.Log("Robot subprocess stopped");
    }


    void KillProcessGroup()
    {
        Process.Start("/bin/bash", $"-c \"kill -TERM -{robotProcess.Id}\"");
        Process.Start("/bin/bash", $"-c \"pkill -f vla_chat\"");
        Process.Start("/bin/bash", $"-c \"pkill -f vla_unity\"");
    }


    void OnDestroy()
    {
        StopRobot();
    }

    void OnApplicationQuit()
    {
        StopRobot();
    }
}