using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/**
 * Handles the on-screen debug console within the Unity UI.
 * Supports scrolling and automatic focus on the latest log entries.
 */
public class LogManager : MonoBehaviour
{
    [SerializeField] private Text uiText; 
    [SerializeField] private ScrollRect scrollRect; 

    private List<string> logs = new List<string>();

    /**
     * Adds a new log entry with a timestamp and colored tags.
     * Automatically scrolls to the bottom of the list.
     * @param message - The string content to display in the log.
     */
    public void WriteLog(string message)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");
        logs.Add($"<color=yellow>[{time}]</color> {message}");
        
        // Keep a reasonable history (e.g., 50 lines)
        if (logs.Count > 50) 
        {
            logs.RemoveAt(0);
        }

        uiText.text = string.Join("\n", logs);

        Canvas.ForceUpdateCanvases();
        
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /**
     * Resets the log history and clears the on-screen text.
     */
    public void ClearLog()
    {
        logs.Clear();
        uiText.text = "";
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}