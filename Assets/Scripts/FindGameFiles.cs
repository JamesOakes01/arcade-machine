using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FindGameFiles : MonoBehaviour
{
    [SerializeField] private string gamesDirectory;
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
    }

    void FindGameBuilds(string directory)
    {
        if (Directory.Exists(directory))
        {
            string[] gameFolders = Directory.GetDirectories(directory);

            foreach (string gameFolder in gameFolders)
            {
                Debug.Log($"Searching in: {gameFolder}");

                string buildsFolderPath = FindBuildsFolder(gameFolder);

                if (!string.IsNullOrEmpty(buildsFolderPath))
                {
                    Debug.Log($"Builds folder found: {buildsFolderPath}");
                    GameInfo gameInfo = LoadGameInfo(buildsFolderPath);
                    if (gameInfo != null)
                    {
                        gamesList.Add(gameInfo); // Add the game info to the list
                    }
                }
                else
                {
                    Debug.Log($"No builds folder found in: {gameFolder}");
                }
            }
        }
        else
        {
            Debug.LogError($"Directory does not exist: {directory}");
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
        Debug.LogWarning($"No valid executable found in: {buildsFolderPath}");
    }

    return new GameInfo(title, description, authors, coverArt, buildsFolderPath, executablePath);
}

// Method to find the game's executable file in the builds folder
string FindExecutableFile(string buildsFolderPath)
{
    // Get all .exe files in the builds folder and its subdirectories
    string[] exeFiles = Directory.GetFiles(buildsFolderPath, "*.exe", SearchOption.AllDirectories);

    foreach (string exeFile in exeFiles)
    {
        string fileName = Path.GetFileName(exeFile);

        // Ignore UnityCrashHandler64.exe
        if (!fileName.Equals("UnityCrashHandler64.exe", System.StringComparison.OrdinalIgnoreCase))
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

    // Create buttons for each game, only showing the game's title on the button
    void CreateGameButtons()
    {
        foreach (GameInfo game in gamesList)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);

            // Set the button's title (just the game's name)
            Text titleText = newButton.transform.Find("Text").GetComponent<Text>();
            titleText.text = game.title;

            // Add a listener to update the info panel when the button is selected
            newButton.GetComponent<Button>().onClick.AddListener(() => DisplayGameInfo(game));
        }
    }

    // Update the UI panel with the selected game's details
    void DisplayGameInfo(GameInfo game)
    {
        infoTitle.text = game.title;
        infoDescription.text = game.description;
        infoAuthors.text = game.authors;

        // Update the cover art if it exists
        if (game.coverArt != null)
        {
            infoCoverArt.sprite = Sprite.Create(game.coverArt, new Rect(0, 0, game.coverArt.width, game.coverArt.height), new Vector2(0.5f, 0.5f));
            infoCoverArt.gameObject.SetActive(true); // Show cover art if available
        }
        else
        {
            infoCoverArt.gameObject.SetActive(false); // Hide cover art if not available
        }
    }
}
