using System;
using UnityEngine;

public class NoAdsButton : MonoBehaviour
{

    void Start()
    {
        Action<bool> aGotPurchase = (pAdsRemoved) =>
        {
            Debug.Log("vho NoAdsButton pAdsRemoved : " + pAdsRemoved);
            if (pAdsRemoved) gameObject.SetActive(false);
        };

        AdManager.Instance.GotPurchases += aGotPurchase;
        if (AdManager.Instance._IAPIsInit) aGotPurchase.Invoke(AdManager.Instance._adsRemoved);

    }

    public void OnClick()
    {
        AudioManager.Instance.PlayClickSound();
        GameObject vPanel = transform.parent.Find("NoAdsPanel").gameObject;
        vPanel.SetActive(!vPanel.activeSelf);
    }
}
