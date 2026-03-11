using UnityEngine;
using TMPro;
public class UI : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject restartMenu;
    bool pauseMenuOpen = false;
    bool restartMenuOpen;


    public PlayerMovement mvmtScript;

    public TMP_InputField robotNameField;
    public WakeUpRobot interactionConfig;

    void Start()
    {
        OpenStartMenu();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!restartMenuOpen)
            {
                if (!pauseMenuOpen)
                    OpenPauseMenu();
                else
                    CloseMenus();
            }
        }
    }

    void OpenStartMenu()
    {
        // On Start()
        pauseMenu.SetActive(false);
        restartMenu.SetActive(true);
        restartMenuOpen = true;
        mvmtScript.movementEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        interactionConfig.robotName = robotNameField.text;
        Time.timeScale = 0f;
    }

    void OpenPauseMenu()
    {
        // On Esc
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        pauseMenuOpen = true;
        mvmtScript.movementEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        // Pause > Restart
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void CloseMenus()
    {
        // Pause > Continue; Restart > Start
        pauseMenu.SetActive(false);
        restartMenu.SetActive(false);

        Time.timeScale = 1f;
        pauseMenuOpen = false;
        restartMenuOpen = false;
        mvmtScript.movementEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ContinueGame()
    {
        // Pause > Continue
        CloseMenus();
        mvmtScript.Go();
    }

    public void QuitGame()
    {
        // Pause > Quit
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}