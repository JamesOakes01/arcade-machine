using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FindGameFiles : MonoBehaviour
{
    [SerializeField] private string pathToGamesFolder;
    // Start is called before the first frame update
    void Start()
    {
        SearchForBuilds(pathToGamesFolder);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Main function to call from Unity to search for .exe files
    public void SearchForBuilds(string rootFolderPath)
    {
        Debug.Log("Starting Search...");
        // Get all subfolders within the root folder
        string[] subFolders = Directory.GetDirectories(rootFolderPath);

        foreach (string folder in subFolders)
        {
            Debug.Log("loop");
            // Search for a "Builds" folder within each subfolder
            string buildsPath = Path.Combine(folder, "builds");
            //Debug.Log(buildsPath);
            
            if (Directory.Exists(buildsPath))
            {
                // Find .exe files inside the "Builds" folder
                //FindExeFiles(buildsPath);
                Debug.Log(buildsPath);
            }
            else
            {
                Debug.Log("this path does not exist: "+ buildsPath);
            }
        }
    }
}
