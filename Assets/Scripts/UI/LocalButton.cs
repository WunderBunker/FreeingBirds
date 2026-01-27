using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalButton : MonoBehaviour
{
    bool _isActive;

    public void OnClick()
    {
        StartCoroutine(ChangeLocale());
    }

    IEnumerator ChangeLocale()
    {
        if (_isActive) yield return null;

        SetActive(true);

        StartCoroutine(PartieManager.Instance.ChangeLocale(gameObject.name));
    }

    public void SetActive(bool pActive)
    {
        _isActive = pActive;
    }

}
