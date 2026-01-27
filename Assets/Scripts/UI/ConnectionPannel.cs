using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ConnectionPannel : MonoBehaviour
{
    [SerializeField] LocalizeStringEvent _noSaveText;
    [SerializeField] LocalizeStringEvent _noBoardText;
    [SerializeField] LocalizeStringEvent _privateAccountText;
    [SerializeField] GameObject _privateAccountButton;

    public void ActiveNoSaveText()
    {
        gameObject.SetActive(true);
        _noSaveText.StringReference.StringChanged += UpdateString;
        _noBoardText.StringReference.StringChanged -= UpdateString;
        _privateAccountText.StringReference.StringChanged -= UpdateString;
        _privateAccountButton.SetActive(false);
        _noSaveText.RefreshString();

        SaveManager.MustLauchConnectionPannel = false;
    }

    public void ActiveNoBoardText()
    {
        gameObject.SetActive(true);
        _noBoardText.StringReference.StringChanged += UpdateString;
        _noSaveText.StringReference.StringChanged -= UpdateString;
        _privateAccountText.StringReference.StringChanged -= UpdateString;
        _privateAccountButton.SetActive(false);
        _noBoardText.RefreshString();

        SaveManager.MustLauchConnectionPannel = false;
    }

    public void ActivePrivateAccountText()
    {
        gameObject.SetActive(true);
        _privateAccountText.StringReference.StringChanged += UpdateString;
        _noBoardText.StringReference.StringChanged -= UpdateString;
        _noSaveText.StringReference.StringChanged -= UpdateString;
        _privateAccountButton.SetActive(true);
        _noBoardText.RefreshString();
    }

    public void Quit()
    {
        AudioManager.Instance.PlayClickSound();
        gameObject.SetActive(false);
        _privateAccountButton.SetActive(false);
    }

    void UpdateString(string pText)
    {
        AudioManager.Instance.PlayClickSound();
        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = pText;
    }
}
