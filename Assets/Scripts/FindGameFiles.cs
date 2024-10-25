using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FindGameFiles : MonoBehaviour
{
    [SerializeField] private string gamesDirectory;
    private PlayerInput input;
    private Process process;
    private bool isGameRunning = false;

    public GameObject buttonPrefab; // Assign a Button prefab in the Unity Inspector
    public Transform buttonContainer; // Assign a UI container to hold the buttons (like a ScrollView)

    // UI elements for displaying selected game's details
    public Text infoTitle;
    public Text infoDescription;
    public Text infoAuthors;
    public Image infoCoverArt;

    private List<GameInfo> gamesList = new List<GameInfo>();

    void Start()
    {
        FindGameBuilds(gamesDirectory);
        CreateGameButtons();
        input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Check for the controller button combination while a game is running
        if (isGameRunning && process != null && !process.HasExited)
        {
            if (CheckExitCombination())
            {
                OnExitGame();
            }
        }
    }

    bool CheckExitCombination()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            // Checking Start + Select button combo
            return gamepad.startButton.isPressed && gamepad.selectButton.isPressed;
        }
        return false;
    }

    void FindGameBuilds(string directory)
    {
        if (Directory.Exists(directory))
        {
            string[] gameFolders = Directory.GetDirectories(directory);

            foreach (string gameFolder in gameFolders)
            {
                UnityEngine.Debug.Log($"Searching in: {gameFolder}");

                string buildsFolderPath = FindBuildsFolder(gameFolder);

                if (!string.IsNullOrEmpty(buildsFolderPath))
                {
                    UnityEngine.Debug.Log($"Builds folder found: {buildsFolderPath}");
                    GameInfo gameInfo = LoadGameInfo(buildsFolderPath);
                    if (gameInfo != null)
                    {
                        gamesList.Add(gameInfo); // Add the game info to the list
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"No builds folder found in: {gameFolder}");
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"Directory does not exist: {directory}");
        }
    }

    string FindBuildsFolder(string directory)
    {
        string[] subDirectories = Directory.GetDirectories(directory);

        foreach (string subDir in subDirectories)
        {
            if (Path.GetFileName(subDir).ToLower() == "builds")
            {
                return subDir;
            }

            string foundBuildsFolder = FindBuildsFolder(subDir);
            if (!string.IsNullOrEmpty(foundBuildsFolder))
            {
                return foundBuildsFolder;
            }
        }

        return null;
    }

    GameInfo LoadGameInfo(string buildsFolderPath)
    {
        string title = "Unknown Title";
        string description = "No Description";
        string authors = "Unknown Authors";
        Texture2D coverArt = null;
        string executablePath = null;

        // Check for title.txt, description.txt, and authors.txt
        string titleFile = Path.Combine(buildsFolderPath, "title.txt");
        string descriptionFile = Path.Combine(buildsFolderPath, "description.txt");
        string authorsFile = Path.Combine(buildsFolderPath, "authors.txt");

        if (File.Exists(titleFile))
        {
            title = File.ReadAllText(titleFile);
        }

        if (File.Exists(descriptionFile))
        {
            description = File.ReadAllText(descriptionFile);
        }

        if (File.Exists(authorsFile))
        {
            authors = File.ReadAllText(authorsFile);
        }

        // Check for an optional cover art (e.g., cover.png)
        string[] coverArtExtensions = { ".png", ".jpg" };
        foreach (string extension in coverArtExtensions)
        {
            string coverArtPath = Path.Combine(buildsFolderPath, "cover" + extension);
            if (File.Exists(coverArtPath))
            {
                coverArt = LoadImage(coverArtPath); // Load the cover art into a texture
                break;
            }
        }

        // Find the valid executable file (exclude UnityCrashHandler64.exe)
        executablePath = FindExecutableFile(buildsFolderPath);

        if (string.IsNullOrEmpty(executablePath))
        {
            UnityEngine.Debug.LogWarning($"No valid executable found in: {buildsFolderPath}");
        }

        return new GameInfo(title, description, authors, coverArt, buildsFolderPath, executablePath);
    }

    string FindExecutableFile(string buildsFolderPath)
    {
        // Get all .exe files in the builds folder and its subdirectories
        string[] exeFiles = Directory.GetFiles(buildsFolderPath, "*.exe", SearchOption.AllDirectories);

        foreach (string exeFile in exeFiles)
        {
            string fileName = Path.GetFileName(exeFile);

            // Ignore UnityCrashHandler64.exe
            if (!fileName.Equals("UnityCrashHandler64.exe", StringComparison.OrdinalIgnoreCase))
            {
                return exeFile; // Return the path to the first valid executable found
            }
        }

        return null; // No valid executable found
    }

    Texture2D LoadImage(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    void CreateGameButtons()
    {
        foreach (GameInfo game in gamesList)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);

            // Set the button's text to display the game title
            Text titleText = newButton.transform.Find("Text").GetComponent<Text>();
            titleText.text = game.title;

            // Add listener to handle button selection (controller or mouse hover)
            AddEventTrigger(newButton, EventTriggerType.Select, () => DisplayGameInfo(game));

            // Optional: Handle mouse hover
            AddEventTrigger(newButton, EventTriggerType.PointerEnter, () => DisplayGameInfo(game));

            // Add listener to launch the game when the button is clicked
            newButton.GetComponent<Button>().onClick.AddListener(() => LaunchGame(game));
        }
    }

    void AddEventTrigger(GameObject button, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }

    public void LaunchGame(GameInfo game)
    {
        if (string.IsNullOrEmpty(game.executablePath))
        {
            UnityEngine.Debug.LogError($"No executable found for the game: {game.title}");
            return;
        }

        try
        {
            // Launch the game's executable
            process = Process.Start(game.executablePath);
            UnityEngine.Debug.Log($"Launching game: {game.title} at {game.executablePath}");

            isGameRunning = true; // Set the game running flag
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to launch game: {game.title}. Error: {e.Message}");
        }
    }

    public void OnExitGame()
    {
        if (process != null && !process.HasExited)
        {
            try
            {
                UnityEngine.Debug.Log("Exiting game...");
                process.Kill(); // Kill the process directly
                process = null; // Clear the process reference after killing
                isGameRunning = false;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to exit the game. Error: {e.Message}");
            }
        }
        else
        {
            UnityEngine.Debug.Log("No game process is currently running.");
        }
    }

    void DisplayGameInfo(GameInfo game)
    {
        UnityEngine.Debug.Log($"Displaying info for game: {game?.title}");

        if (game == null)
        {
            UnityEngine.Debug.LogError("Game object is null.");
            return;
        }

        infoTitle.text = game.title;
        infoDescription.text = game.description;
        infoAuthors.text = game.authors;

        if (game.coverArt != null)
        {
            infoCoverArt.sprite = Sprite.Create(game.coverArt, new Rect(0, 0, game.coverArt.width, game.coverArt.height), new Vector2(0.5f, 0.5f));
            infoCoverArt.gameObject.SetActive(true);
        }
        else
        {
            infoCoverArt.gameObject.SetActive(false);
        }
    }

    void OnApplicationQuit()
    {
        // Make sure the game process is killed if the Unity app quits
        OnExitGame();
    }
}
