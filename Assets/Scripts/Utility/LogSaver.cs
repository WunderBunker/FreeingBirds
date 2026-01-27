using System;
using System.IO;
using UnityEngine;

public class LogSaver : MonoBehaviour
{
    private string _logFilePath;

    void Awake()
    {
        _logFilePath = Path.Combine(Application.persistentDataPath, "freeing_birds_logs.txt");
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string pLogString, string pStackTrace, LogType pType)
    {
        string vLogEntry = $"{DateTime.Now} [{pType}] {pLogString}\n";

        if (pType == LogType.Error || pType == LogType.Exception)
            vLogEntry += pStackTrace + "\n";

        File.AppendAllText(_logFilePath, vLogEntry);
    }
}
