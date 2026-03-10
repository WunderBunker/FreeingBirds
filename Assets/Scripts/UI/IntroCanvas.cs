using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCanvas : MonoBehaviour
{
    [SerializeField] float _imgAlphaSpeed = 1.5f;
    [SerializeField] float _showImageTime = 2;
    Image _mainImage;
    float _startTime;
    bool _mustGoToNextScene;
    bool _saveLoadingEnded;
    float _showImageTimer;

    enum ImageState { Rising, Hiding, Showing, Null };
    ImageState _imageState;
    private bool _forceGoToNextScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _mainImage = transform.Find("Image").GetComponent<Image>();
        _imageState = ImageState.Rising;

        _startTime = Time.time;

        if (SaveManager._save == null)
            CoroutineRunner.Instance.StartCoroutine(SaveManager.LoadPlayerSave((PlayerSave pCB) =>
            {
                _saveLoadingEnded = true;
                transform.Find("LoadingSaveText").gameObject.SetActive(false);
            }, gameObject));
    }

    // Update is called once per frame
    void Update()
    {
        switch (_imageState)
        {
            case ImageState.Rising:
                _mainImage.color += new Color(0, 0, 0, Time.deltaTime * _imgAlphaSpeed);
                if (_mainImage.color.a >= 1)
                {
                    _imageState = ImageState.Showing;
                    _showImageTimer = _showImageTime;
                }
                break;
            case ImageState.Showing:
                _showImageTimer -= Time.deltaTime;
                if (_showImageTimer <= 0) _imageState = ImageState.Hiding;
                break;
            case ImageState.Hiding:
                _mainImage.color -= new Color(0, 0, 0, Time.deltaTime * _imgAlphaSpeed);
                if (_mainImage.color.a <= 0)
                {
                    _imageState = ImageState.Null;
                    _mustGoToNextScene = true;
                }
                break;
            default: break;
        }

        if (_forceGoToNextScene ||
            (_mustGoToNextScene && _saveLoadingEnded))
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void OnBGClick()
    {
        _forceGoToNextScene = true;
    }

}
