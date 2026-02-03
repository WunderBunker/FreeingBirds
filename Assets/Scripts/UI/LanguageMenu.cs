using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LanguageMenu : MonoBehaviour, IChildEnabler
{
    Transform _localLayout;
    GameObject _localButton;
    List<AsyncOperationHandle<Sprite>> _imgTasks = new();

    void OnEnable()
    {
        EnableChilds(false, new string[]{"Book"});
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _localLayout = transform.Find("Languages").Find("Viewport").Find("Layout");
        _localButton = _localLayout.GetChild(0).gameObject;
        StartCoroutine(LoadFlags());
    }

    IEnumerator LoadFlags()
    {
        SettingsSave vSettingsSave = SaveManager.SafeSave.SettingsSave;

        foreach (var lLocale in LocalizationSettings.AvailableLocales.Locales)
        {
            AssetTable lImgTable = LocalizationSettings.AssetDatabase.GetTable("Images", lLocale);
            if (lImgTable == null) continue;
            StringTable lTxtTable = LocalizationSettings.StringDatabase.GetTable("Texts", lLocale);
            if (lTxtTable == null) continue;

            AsyncOperationHandle<Sprite> lImgTask = lImgTable.GetAssetAsync<Sprite>("Flags");
            while (!lImgTask.IsDone)
                yield return null;
            string lLocalName = lTxtTable.GetEntry("IdLocal").Value;

            Sprite lSprite = lImgTask.Result;
            _imgTasks.Add(lImgTask);

            GameObject vNewButton = Instantiate(_localButton, _localLayout);
            vNewButton.GetComponent<Image>().sprite = lSprite;
            vNewButton.name = lLocale.Identifier.Code;
            vNewButton.GetComponentInChildren<TextMeshProUGUI>().text = lLocalName;

            if (vSettingsSave.Language == lLocalName) vNewButton.GetComponent<Button>().Select();
        }

        Destroy(_localButton);
    }

    public void Quit()
    {
        AudioManager.Instance.PlayClickSound();
        transform.Find("Book").GetComponent<BookImage>().Quit();
    }

    void OnDestroy()
    {
        foreach (var lTask in _imgTasks)
            if (lTask.IsValid()) Addressables.Release(lTask);
    }

    public void EnableChilds(bool pActive, string[] pExternalFlag = null)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (!pExternalFlag.Contains(transform.GetChild(i).name)) transform.GetChild(i).gameObject.SetActive(pActive);
    }
}
