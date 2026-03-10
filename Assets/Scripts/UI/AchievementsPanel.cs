using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class AchievementsPanel : MonoBehaviour, IChildEnabler
{
    [SerializeField] AchievementList _achievementsList;

    Transform _elementLayout;
    List<GameObject> _elementsList = new();
    LocalizedString _achievementsCountLocal;

    int _gottenCount;

    void Awake()
    {
        _elementLayout = transform.Find("ScrollView").Find("Viewport").Find("Content");

        _achievementsCountLocal = transform.Find("Counter").Find("Text").GetComponent<LocalizeStringEvent>().StringReference;
        _achievementsCountLocal.StringChanged += (string pText) => transform.Find("Counter").Find("Text").GetComponent<TextMeshProUGUI>().text = pText;
        UpdateCountString();

        if (SaveManager.SafeSave.GottenAchievement.Count == 0)
            foreach (var lHS in SaveManager.SafeSave.HighScores)
                AchievementsManager.Instance.UpdateScoreAchievements(lHS.Value, lHS.Key,true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        EnableChilds(false, new string[] { "Book" });
        InitLayout();
    }

    void InitLayout()
    {
        foreach (var e in _elementsList) Destroy(e);
        _elementsList.Clear();

        GameObject vElementTemplate = _elementLayout.GetChild(0).gameObject;
        vElementTemplate.SetActive(false);

        _gottenCount = 0;
        foreach (Achievement lAchievement in _achievementsList.List)
        {
            GameObject vNewElement;
            vNewElement = Instantiate(vElementTemplate, _elementLayout);

            vNewElement.SetActive(true);

            vNewElement.GetComponent<AchievementUI>().Achievement = lAchievement;

            if (SaveManager.SafeSave.GottenAchievement.Contains(lAchievement.Id))
            {
                vNewElement.GetComponent<AchievementUI>().State = AchievementUI.AchievState.Got;
                _gottenCount++;
            }
            else vNewElement.GetComponent<AchievementUI>().State = AchievementUI.AchievState.NotGot;

            _elementsList.Add(vNewElement);
        }
        UpdateCountString();
    }

    void UpdateCountString()
    {
        _achievementsCountLocal.Arguments = new object[] { _gottenCount.ToString(), _achievementsList.List.Count.ToString() };
        _achievementsCountLocal.RefreshString();
    }

    public void OnQuit()
    {
        AudioManager.Instance.PlayClickSound();
        transform.Find("Book").GetComponent<BookImage>().Quit();
    }

    public void EnableChilds(bool pActive, string[] pFlag = null)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (!pFlag.Contains(transform.GetChild(i).name)) transform.GetChild(i).gameObject.SetActive(pActive);
    }
}
