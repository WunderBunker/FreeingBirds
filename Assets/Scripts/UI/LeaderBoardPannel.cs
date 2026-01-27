using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class LeaderBoardPannel : MonoBehaviour, IChildEnabler
{
    LocalizedString _rankLoc;
    int _playerRank = 0;

    void Awake()
    {
        _rankLoc = transform.Find("PlayerScore").GetComponent<LocalizeStringEvent>().StringReference;
        _rankLoc.StringChanged += (string pText) => transform.Find("PlayerScore").GetComponent<TextMeshProUGUI>().text = pText;
        _rankLoc.Arguments = new object[] { "0" };
        _rankLoc.RefreshString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Obsolete]
    void OnEnable()
    {
        EnableChilds(false, new string[] { "Book" });
        InitBoard();
    }

    [Obsolete]
    void InitBoard()
    {
        Transform vLayout = transform.Find("ScrollView").Find("Viewport").Find("Layout");
        GameObject vElementTemplate = vLayout.GetChild(0).gameObject;
        vElementTemplate.SetActive(false);

        for (int lCptChild = vLayout.childCount - 1; lCptChild >= 0; lCptChild--)
            if (vLayout.GetChild(lCptChild).gameObject != vElementTemplate) Destroy(vLayout.GetChild(lCptChild).gameObject);

        Action<LeaderBoard> aLBsLoaded = (LeaderBoard pLB) =>
        {
            transform.parent.Find("LoadingSymbol").gameObject.SetActive(false);

            if (pLB == null) return;

            pLB.Scores = pLB.Scores.OrderByDescending(item1 => item1.Value).ToDictionary(x => x.Key, x => x.Value);

            vElementTemplate.SetActive(true);

            int vOrder = 0;
            string vLastScore = "";
            Color vBGElemColor = vElementTemplate.transform.Find("BG").GetComponent<Image>().color;

            foreach (KeyValuePair<string, float> pScore in pLB.Scores)
            {
                vOrder++;

                var lNewPlayerElement = Instantiate(vElementTemplate, vLayout);
                Color vBGColor = vBGElemColor + vOrder % 2 * new Color(1, 1, 1, 0);
                lNewPlayerElement.transform.Find("BG").GetComponent<Image>().color
                    = new Color(Mathf.Clamp01(vBGColor.r), Mathf.Clamp01(vBGColor.g), Mathf.Clamp01(vBGColor.b), vBGColor.a);
                lNewPlayerElement.transform.Find("Number").GetComponent<TextMeshProUGUI>().text = vOrder.ToString();
                lNewPlayerElement.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = pScore.Key;
                lNewPlayerElement.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = pScore.Value.ToString("0");

                if (pScore.Key == SaveManager.GetPlayerId()) _playerRank = vOrder;
                vLastScore = pScore.Key;
            }

            float vCurrentScore = SaveManager.SafeSave.HighScores[SaveManager.SafeSave.SelectedBirdId];
            if (_playerRank == 0 && vCurrentScore > 0
               && (pLB.Scores.Count < 100 || vCurrentScore > pLB.Scores[vLastScore]))
                transform.parent.Find("ConnectionPannel").GetComponent<ConnectionPannel>().ActivePrivateAccountText();

            vElementTemplate.SetActive(false);

            UpdateRankString();
        };

        UpdateRankString();

        transform.parent.Find("LoadingSymbol").gameObject.SetActive(true);
        StartCoroutine(SaveManager.LoadLeaderBoard(aLBsLoaded));
    }

    public void OnQuit()
    {
        AudioManager.Instance.PlayClickSound();
        transform.Find("Book").GetComponent<BookImage>().Quit();
        transform.parent.Find("LoadingSymbol").gameObject.SetActive(false);
    }

    void UpdateRankString()
    {
        _rankLoc.Arguments = new object[] { _playerRank.ToString() };
        _rankLoc.RefreshString();
    }

    public void EnableChilds(bool pActive, string[] pFlag = null)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (!pFlag.Contains(transform.GetChild(i).name)) transform.GetChild(i).gameObject.SetActive(pActive);
    }
}
