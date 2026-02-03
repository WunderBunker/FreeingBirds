using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] BirdList _birdList;

    GameObject _birdItemTemplate;

    //Les différents menus
    //Menu princpal avec bouton play et sélection du perso*
    GameObject _homeMenu;
    //Menu Satrt où on doit touch to start et d'où on peut acceder au home mmenu 
    GameObject _startMenu;
    //Menu du jeux lorsqu'il est lancé
    GameObject _inGameMenu;

    //Canvas avec l'effet de pluie sur la caméra
    public GameObject CanvasMeteo;

    //Menu de sélection d'un perso
    protected GameObject _birdMenu;

    //Prompteur du scorependant la partie
    TextMeshProUGUI _sorePrompt;

    private void Awake()
    {
        //Recuperation des objets
        _homeMenu = transform.Find("HomeMenu").gameObject;
        _birdMenu = _homeMenu.transform.Find("BirdMenu").gameObject;
        _birdItemTemplate = _birdMenu.transform.Find("BirdLayout").Find("BirdItem").gameObject;
        _startMenu = transform.Find("StartMenu").gameObject;
        _inGameMenu = transform.Find("InGameMenu").gameObject;
        _sorePrompt = _inGameMenu.transform.Find("ScorePrompt").GetComponent<TextMeshProUGUI>();

        InitBirdLayout();
    }

    //On récupère les Ids des différents birds à disposition
    void InitBirdLayout()
    {
        Transform vBirdLayout = _birdMenu.transform.Find("BirdLayout");
        _birdItemTemplate.SetActive(true);
        for (int lIndex = 0; lIndex < _birdList.List.Length; lIndex++)
        {
            _birdList.List[lIndex].Index = lIndex;
            Bird lBird = _birdList.List[lIndex];

            GameObject lNewBird = Instantiate(_birdItemTemplate, vBirdLayout);
            lNewBird.GetComponent<BirdMenuItem>().Bird = lBird;
            lNewBird.transform.Find("BirdImage").GetComponent<Image>().sprite = lBird.Image;
            lNewBird.name = lBird.Id;
        }
        _birdItemTemplate.SetActive(false);
        _birdItemTemplate.transform.SetSiblingIndex(vBirdLayout.childCount - 1);
    }

    //Initialisation de l'accessibilité de chaque bird et sélection du courrant
    public void InitBirdsMenu()
    {
        Transform vBirdLayout = _birdMenu.transform.Find("BirdLayout");

        //Pour chaque bird identifié précédemment et inscrit dans la liste...
        foreach (Bird lBird in _birdList)
        {
            Transform lBirdItem = vBirdLayout.GetChild(lBird.Index);

            //...Le premier bird  est toujours accessible
            if (lBird.Index == 0)
                lBirdItem.GetComponent<BirdMenuItem>().MakeAccessible();
            else
            {
                //...Pour les autres on regarde si le score du bird précédant est suffisant, le montant nécessaire est précisé dans le BirdMenuItem.gScoreToBeAccessible
                if (SaveManager.SafeSave.HighScores[_birdList.List[lBird.Index - 1].Id]
                   >= lBirdItem.GetComponent<BirdMenuItem>().Bird.ScoreToBeAccessible)
                    lBirdItem.GetComponent<BirdMenuItem>().MakeAccessible();
                //...Ceux dont ce n'est pas le cas sont rendus inaccessibles
                else lBirdItem.GetComponent<BirdMenuItem>().MakeUnAccessible();

                lBirdItem.GetComponent<BirdMenuItem>().CadenasColor = _birdList.List[lBird.Index - 1].Color;
            }

            //On sélectionne le bird courrant et on le rend accessible
            if (SaveManager.SafeSave.SelectedBirdId == lBird.Id)
            {
                lBirdItem.GetComponent<BirdMenuItem>().MakeAccessible();
                lBirdItem.GetComponentInChildren<Toggle>().onValueChanged.Invoke(true);
                lBirdItem.GetComponentInChildren<Toggle>().isOn = true;
            }
        }
    }

    public void ChooseHomeMenu()
    {
        AudioManager.Instance.PlayClickSound();
        LoadHomeMenu();
    }

    //chargement du home menu duquel on peut sélectionner un bird ou lancer une partie
    public void LoadHomeMenu()
    {
        _homeMenu.SetActive(true);
        _birdMenu.SetActive(false);
        _startMenu.SetActive(false);
        _inGameMenu.SetActive(false);
        CanvasMeteo?.SetActive(false);

        _homeMenu.transform.Find("HighScorePrompt").GetComponent<TextMeshProUGUI>().text
            = SaveManager.SafeSave.HighScores[SaveManager.SafeSave.SelectedBirdId].ToString("0");
        _homeMenu.transform.Find("HighScorePrompt").GetComponent<TextMeshProUGUI>().color
            = GetBird(SaveManager.SafeSave.SelectedBirdId).Color;

        if (SaveManager.MustLauchConnectionPannel)
            _homeMenu.transform.Find("ConnectionPannel").GetComponent<ConnectionPannel>().ActiveNoSaveText();

        PartieManager.Instance.LoadHomeMenu();
    }

    public void ChooseBirdMenu(Toggle pToggle)
    {
        AudioManager.Instance.PlayClickSound();
        pToggle.transform.Find("OffImage").gameObject.SetActive(!pToggle.isOn);
        LoadBirdMenu(pToggle);
    }

    //Chargement du menu de sélection des birds
    public void LoadBirdMenu(Toggle pToggle)
    {
        InitBirdsMenu();
        _birdMenu.SetActive(pToggle.isOn);
    }

    public void ChoosePlay()
    {
        _homeMenu.transform.Find("BirdButton").GetComponent<Toggle>().isOn = false;
        AudioManager.Instance.PlayClickSound();
        LoadStartMenu();
    }

    //Chargement du Game Menu duquel on peut start la partie ou aller au home menu
    public void LoadStartMenu()
    {
        _homeMenu.SetActive(false);
        _startMenu.SetActive(true);
        _inGameMenu.SetActive(false);
        CanvasMeteo?.SetActive(true);

        PartieManager.Instance.LoadStartMenu();
    }

    //Chargement du menu pour une partie en cours de jeux
    public void LoadInPartieMenu()
    {
        _homeMenu.SetActive(false);
        _startMenu.SetActive(false);
        _inGameMenu.SetActive(true);
        CanvasMeteo?.SetActive(true);
        _inGameMenu.transform.Find("ScorePrompt").GetComponent<TextMeshProUGUI>().text = "0";

        PartieManager.Instance.LoadInPartieMenu();
    }

    public void ChooseBird(string pBirdId)
    {
        Bird vNewBird = GetBird(pBirdId);
        Toggle vNewBirdToggle = _birdMenu.transform.Find("BirdLayout").GetChild(vNewBird.Index).GetComponentInChildren<Toggle>();

        if (vNewBirdToggle != null && vNewBirdToggle.isOn)
        {
            if (pBirdId != SaveManager.SafeSave.SelectedBirdId)
                AudioManager.Instance.PlayClickSound();

            PartieManager.Instance.ChangeBird(vNewBird);
        }
    }

    public void ApplyBirdData(Bird pBird)
    {
        _startMenu.transform.Find("TouchToStartText").GetComponent<TextMeshProUGUI>().color = pBird.Color;

        _homeMenu.transform.Find("HighScorePrompt").GetComponent<TextMeshProUGUI>().text = SaveManager.SafeSave.HighScores[pBird.Id].ToString("0");
        _homeMenu.transform.Find("HighScorePrompt").GetComponent<TextMeshProUGUI>().color = pBird.Color;
        _inGameMenu.transform.Find("ScorePrompt").GetComponent<TextMeshProUGUI>().color = pBird.Color;
        _inGameMenu.transform.Find("ScorePrompt").GetChild(0).GetComponent<TextMeshProUGUI>().color = pBird.Color;
        _inGameMenu.transform.Find("ScorePrompt").GetChild(0).GetComponent<TextMeshProUGUI>().alpha = 0;
    }

    public Bird GetBird(string pId)
    {
        return Array.Find(_birdList.List, (item) => item.Id == pId);
    }

    public string[] GetAllBirdIds()
    {
        string[] vList = new string[_birdList.List.Length];
        for (int i = 0; i < _birdList.List.Length; i++) vList[i] = _birdList.List[i].Id;
        return vList;
    }


    public void MajAvancementInPrompt(float pValue)
    {
        _sorePrompt.text = pValue.ToString("0");
    }

    public void AddPonctualAvancementInPrompt(int pValue)
    {
        StartCoroutine(_sorePrompt.transform.GetComponentInChildren<AdditionPrompt>().AddPoint(pValue));
    }

    public void RemovePonctualAvancementInPrompt(int pValue)
    {
        StartCoroutine(_sorePrompt.transform.GetComponentInChildren<AdditionPrompt>().RemovePoint(pValue));
    }

    public void LoadSettingsPannel()
    {
        GameObject vPannel = transform.Find("SettingsMenu").gameObject;

        if (vPannel.activeSelf) vPannel.GetComponent<PauseMenu>().OnQuit();
        else vPannel.SetActive(true);
    }

    public void LoadLeaderBoardPannel()
    {
        if (!_homeMenu.activeSelf) return;

        GameObject vPannel = _homeMenu.transform.Find("LeaderBoard").gameObject;
        if (vPannel.activeSelf) vPannel.GetComponent<LeaderBoardPannel>().OnQuit();
        else vPannel.SetActive(true);
    }
}
