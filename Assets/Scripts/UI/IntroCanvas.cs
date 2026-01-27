using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCanvas : MonoBehaviour
{
    float _startTime;
    bool _mustGoToNextScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _startTime = Time.time;
        _mustGoToNextScene = true;

        if (SaveManager._save == null)
            StartCoroutine(SaveManager.LoadPlayerSave(null));
    }

    // Update is called once per frame
    void Update()
    {
        if (_mustGoToNextScene && (SaveManager._save != null || Time.time - _startTime >= SaveManager._maxLoadingTime + 1))
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
