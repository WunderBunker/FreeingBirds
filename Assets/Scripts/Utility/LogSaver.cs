using System;
using System.IO;
using UnityEngine;

public class LogSaver : MonoBehaviour
{
    private string _logFilePath;
    private string _logWaiting;

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
            _logWaiting = vLogEntry + pStackTrace + "\n";
    }

    void FixedUpdate()
    {
        if (_logWaiting != "")
        {
            File.AppendAllText(_logFilePath, _logWaiting);
            _logWaiting = "";
        }
    }
}
