using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;  // Required for launching the exe process
using System.IO;          // Required for file path operation
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{

    private PlayerInput input;

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    // This function will be called to launch the exe
    public void LoadGame()
    {
            string exePath = "C:\\Users\\James\\Downloads\\games\\swap-out-main\\swap-out-main\\builds\\swap-out.exe";
            // Create the process
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.EnableRaisingEvents = true;
            process.Exited += new System.EventHandler(handler);

            input.DeactivateInput();

            process.Start();
            UnityEngine.Debug.Log("Application launched successfully!");
    }

    public void PlayGame(string path)
    {
            string exePath = path;
            // Create the process
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.EnableRaisingEvents = true;
            process.Exited += new System.EventHandler(handler);

            input.DeactivateInput();

            process.Start();
            UnityEngine.Debug.Log("Application launched successfully!");
    }

    public void handler(object sender, System.EventArgs e){
        UnityEngine.Debug.Log("exited process");
        input.ActivateInput();

        if (sender is Process process)
        {
            process.Dispose();
        }
    }
}