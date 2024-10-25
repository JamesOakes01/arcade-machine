using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo
{
    public string title;
    public string description;
    public string authors;
    public Texture2D coverArt;
    public string buildsFolderPath;
    public string executablePath; // Add a field to store the executable path

    public GameInfo(string title, string description, string authors, Texture2D coverArt, string buildsFolderPath, string executablePath)
    {
        this.title = title;
        this.description = description;
        this.authors = authors;
        this.coverArt = coverArt;
        this.buildsFolderPath = buildsFolderPath;
        this.executablePath = executablePath; // Store the executable path
    }
}

