using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(NewInputRecorder))]
public class NewInputRecorderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NewInputRecorder vIR = (NewInputRecorder)target;
        if (GUILayout.Button("Start Recording")) vIR.StartRecording();
        if (GUILayout.Button("Stop and save Recording")) vIR.StopAndSave();
        if (GUILayout.Button("Play back")) vIR.Playback();
    }
}
#endif
public class NewInputRecorder : MonoBehaviour
{
    [Serializable]
    public class InputFrame
    {
        public float Time;
        public bool MousePressed;
        public Vector2 MouseCoord;
    }

    [Serializable]
    public class InputTrace
    {
        public List<InputFrame> Frames = new List<InputFrame>();
    }

    [Header("Recording")]
    public string FileName = "input_trace.json";

    private InputTrace _trace = new InputTrace();
    private float _lastInputTime;
    private bool _isRecording = false;
    private Touchscreen _virtualTouch;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _virtualTouch = InputSystem.AddDevice<Touchscreen>();
    }

    public void StartRecording()
    {
        _trace.Frames.Clear();
        _isRecording = true;

        Debug.Log("Recording started.");
    }

    public void StopAndSave()
    {
        _isRecording = false;
        SaveToFile();
    }

    void SaveToFile()
    {
        string vJson = JsonUtility.ToJson(_trace, true);
        string vPath = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(vPath, vJson);
    }

    public void Playback()
    {
        LoadTrace();
        StartCoroutine(ReplayCoroutine());
    }

    void Update()
    {
        if (!_isRecording)
            return;

        bool vIsPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

        if (_trace.Frames.Count == 0 ||
            _trace.Frames[_trace.Frames.Count - 1].MousePressed != vIsPressed)
        {
            InputFrame frame = new InputFrame
            {
                Time = (_trace.Frames.Count == 0) ? 0 : Time.time - _lastInputTime,
                MousePressed = vIsPressed,
                MouseCoord = Touchscreen.current.position.ReadValue()
            };
            _trace.Frames.Add(frame);

            _lastInputTime = Time.time;
        }
    }

    void LoadTrace()
    {
        string vPath = Path.Combine(Application.persistentDataPath, FileName);

        if (!File.Exists(vPath))
        {
            Debug.LogError("Trace file not found: " + vPath);
            return;
        }

        string _json = File.ReadAllText(vPath);
        _trace = JsonUtility.FromJson<InputTrace>(_json);

        Debug.Log("Trace loaded.");
    }

    IEnumerator ReplayCoroutine()
    {
        if (_trace == null || _trace.Frames == null || _trace.Frames.Count == 0)
            yield break;

        foreach (var lFrame in _trace.Frames)
        {
            float lWaitTime = lFrame.Time;
            if (lWaitTime > 0)
                yield return new WaitForSecondsRealtime(lWaitTime);

            TouchState lTouchState = new TouchState
            {
                phase = lFrame.MousePressed ? UnityEngine.InputSystem.TouchPhase.Began
                        : UnityEngine.InputSystem.TouchPhase.Ended,
                position = lFrame.MouseCoord
            };

            InputSystem.QueueStateEvent(_virtualTouch, lTouchState);
            InputSystem.Update();
        }
        
        Debug.Log("Replay finished.");
    }
}