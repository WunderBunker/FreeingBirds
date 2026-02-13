using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class NoAdsPanel : MonoBehaviour
{

    LocalizedString _priceLoc;
    void Awake()
    {
        _priceLoc = transform.Find("Text").GetComponent<LocalizeStringEvent>().StringReference;
        _priceLoc.StringChanged += UpdateTextPannel;

        Action<bool> aGotPurchase = (pAdsRemoved) =>
        {
            _priceLoc.Arguments = new object[] { AdManager.Instance._localizedRemoveAdsPrice };
            _priceLoc.RefreshString();
        };

        AdManager.Instance.GotPurchases += aGotPurchase;
        if (AdManager.Instance._IAPIsInit) aGotPurchase.Invoke(AdManager.Instance._adsRemoved);
    }

    public void Quit()
    {
        gameObject.SetActive(false);
    }

    void UpdateTextPannel(string pText)
    {
        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = pText;
        _priceLoc.Arguments = new object[] { AdManager.Instance._localizedRemoveAdsPrice };
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlayClickSound();
    }
}
