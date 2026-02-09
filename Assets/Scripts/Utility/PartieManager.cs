using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.Localization.Settings;

public class PartieManager : MonoBehaviour
{
    //Liste des différents birds pouvant servir de player
    [SerializeField] AudioClip _menuThemeIntro;
    [SerializeField] AudioClip _menuThemeLoop;
    [SerializeField] AudioClip _mainThemeIntro;
    [SerializeField] AudioClip _mainThemeLoop;

    public static PartieManager Instance = null;

    public List<DebugModes> ModeDebug = new List<DebugModes>();

    public MenuManager _menuManager { get; private set; }
    PlayerShellscript _playerShell;
    public GameObject _player => _playerShell._player;

    public PartieState _partieState = PartieState.MustLoadSave;

    //Avancement de la partie en cours
    public float _avancement { get; private set; }

    public delegate void GetInGameEventHandler();
    public event GetInGameEventHandler GetInGame;

    public delegate void GetInPartieEventHandler();
    public event GetInPartieEventHandler GetInPartie;

    public delegate void GameDataReadyEventHandler();
    public event GameDataReadyEventHandler GameDataReady;

    PathScript _pathScript;
    ChunkScript _chunkScript;
    GameObject _homeScreen;
    GameObject _noiseScreen;

    //Compteur  pour les sky background, leur sert lorsqu'ils doivent se détruire les uns les autres 
    static int _skyCompteur;

    bool _mustReloadAfterDeath;
    GameObject _camMeteo;
    GameObject _mapMeteo;
    ColorblindRendererFeature _colorBlindRF;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        _colorBlindRF = Tools.GetRenderFeature<ColorblindRendererFeature>();
    }
    void Start()
    {
        Action<PlayerSave> aGetSave = (lResult) =>
        {
            _menuManager.transform.Find("LoadingSaveText").gameObject.SetActive(false);
            ChangeState(PartieState.GameDataIsReady);
        };
        if (SaveManager._save == null)
        {
            _menuManager.transform.Find("LoadingSaveText").gameObject.SetActive(true);
            StartCoroutine(SaveManager.LoadPlayerSave(aGetSave));
        }
        else aGetSave.Invoke(SaveManager._save);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BlackScreen").gameObject.SetActive(true);

        _menuManager = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MenuManager>();
        _playerShell = GameObject.FindGameObjectWithTag("PlayerShell").GetComponent<PlayerShellscript>();

        if (_mustReloadAfterDeath) ChangeState(PartieState.ReloadingScene);
    }

    private void InitSettings()
    {
        SettingsSave vSettings = SaveManager.SafeSave.SettingsSave;
        StartCoroutine(ChangeLocale(vSettings.Language));
        ChangeColorBlindMode(vSettings.ColorBlindMode);
        ChangeMotionSickness(vSettings.MotionSickness);
    }

    private void InitialiserScene()
    {
        //Instanciation du player
        var vBird = _menuManager.GetBird(SaveManager.SafeSave.SelectedBirdId);
        _playerShell.InstanciatePlayer(vBird);
        ChangeDecor(vBird);
        _menuManager.ApplyBirdData(vBird);

        _pathScript = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>();
        _chunkScript = GameObject.FindGameObjectWithTag("Map").GetComponent<ChunkScript>();

        Transform vCanvas = GameObject.FindGameObjectWithTag("MainCanvas").transform;
        _homeScreen = vCanvas.Find("HomeScreen").gameObject;
        _homeScreen.SetActive(false);
        _noiseScreen = vCanvas.Find("NoiseScreen").gameObject;
        _noiseScreen.SetActive(false);

        vCanvas.Find("BlackScreen").gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_partieState == PartieState.LoadingHomeMenu && _homeScreen.GetComponent<HomeScreen>()._stopMoving) FinishLoadHomeMenu();
    }

    private void ChangeState(PartieState pState)
    {
        _partieState = pState;
        switch (_partieState)
        {
            case PartieState.ReloadingScene:
                _mustReloadAfterDeath = false;
                InitSettings();
                InitialiserScene();
                _menuManager.LoadStartMenu();
                break;
            case PartieState.GameDataIsReady:
                InitSettings();
                InitialiserScene();
                GameDataReady?.Invoke();
                _menuManager.LoadHomeMenu();
                break;
            case PartieState.LoadingHomeMenu:
                if (!_homeScreen.activeSelf) _partieState = PartieState.GameDataIsReady;
                break;
            case PartieState.InGame:
                break;
            case PartieState.PartieStarted:
                break;
        }
    }

    //chargement du home menu duquel on peut sélectionner un bird ou lancer une partie
    public void LoadHomeMenu()
    {
        _homeScreen.SetActive(true);
        _homeScreen.GetComponent<HomeScreen>()._isRaising = false;
        _homeScreen.GetComponent<HomeScreen>()._stopMoving = false;

        SaveManager.SavePlayerSave();

        GameObject.FindGameObjectWithTag("Map").GetComponentInChildren<OverlayParticles>().Activate(false);

        ChangeState(PartieState.LoadingHomeMenu);
        // Suite du traitement dans le Update lorsque le HomeScreen a finit de descendre
    }

    //Appelé par le Home Screen lorsqu'il a finit de s'abaisser
    public void FinishLoadHomeMenu()
    {
        _chunkScript.DeleteAllhunks();
        GetComponent<MusicManager>().PlayMenuTheme();

        ChangeState(PartieState.InHomenu);
    }

    //Chargement du Game Menu duquel on peut start la partie ou aller au home menu
    public void LoadStartMenu()
    {
        _chunkScript.DeleteAllhunks();
        ChangeBird(_menuManager.GetBird(SaveManager.SafeSave.SelectedBirdId));
        _pathScript.StartPath();
        _avancement = 0;

        _homeScreen.SetActive(true);
        _homeScreen.GetComponent<HomeScreen>()._isRaising = true;
        _homeScreen.GetComponent<HomeScreen>()._stopMoving = false;

        ChangeState(PartieState.InGame);

        GetInGame?.Invoke();

        GetComponent<MusicManager>().PlayMenuTheme();
        GameObject.FindGameObjectWithTag("Map").GetComponentInChildren<OverlayParticles>().Activate(true);
    }

    //Chargement du menu pour une partie en cours de jeux
    public void LoadInPartieMenu()
    {
        ChangeState(PartieState.PartieStarted);
        GetInPartie?.Invoke();

        _player.GetComponent<Animator>().SetTrigger("StartFalling");

        GetComponent<MusicManager>().PlayMainLoop();
    }

    //Chgmt d'un Bird
    public void ChangeBird(Bird pBird)
    {
        SaveManager.MajSelectedBird(pBird.Id);
        _playerShell.InstanciatePlayer(pBird);

        //Changement du décor
        ChangeDecor(pBird);

        GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<DragonSpawnManager>().enabled = pBird.Id == "Bird4";

        _menuManager.ApplyBirdData(pBird);
    }

    public void ChangeDecor(Bird pBird)
    {
        //L'humain gère ça lui-même
        if (pBird.Id == "Bird5") return;

        if (_menuManager.CanvasMeteo) Destroy(_menuManager.CanvasMeteo);
        _menuManager.CanvasMeteo = Instantiate(pBird.CanvasMeteo, _menuManager.transform);
        if (_camMeteo) Destroy(_camMeteo);
        _camMeteo = Instantiate(pBird.CamMeteo, GameObject.FindGameObjectWithTag("MainCamera").transform);
        if (_mapMeteo) Destroy(_mapMeteo);
        _mapMeteo = Instantiate(pBird.MapMeteo, GameObject.FindGameObjectWithTag("Map").transform);
        GameObject.FindGameObjectWithTag("Map").transform.Find("PolygonA")
            .GetComponent<MeshRenderer>().material = pBird.Pipes;
        GameObject.FindGameObjectWithTag("Map").transform.Find("PolygonB")
            .GetComponent<MeshRenderer>().material = pBird.Pipes;

        GameObject.FindGameObjectWithTag("Map").GetComponentInChildren<OverlayParticles>().SetColor(pBird.Color);
    }

    //Reload de la scène lorsqu'on meurt
    protected void ReloadPartie()
    {
        SaveManager.MajScore(SaveManager.SafeSave.SelectedBirdId, _avancement);

        if (!ModeDebug.Contains(DebugModes.MoveAlone))
        {
            //On repasse tous les menus en actif pour pouvoir les retrouver après reload
            GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BlackScreen").gameObject.SetActive(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            _mustReloadAfterDeath = true;
        }
    }

    //Maj de l'avancmeent de la partie en cours
    public void AddToAvancement(float pAvancement)
    {
        if (_partieState != PartieState.PartieStarted) return;
        _avancement += pAvancement;
        _menuManager.MajAvancementInPrompt(_avancement);
    }

    public void AddPonctualAvancement(int pAvancement)
    {
        if (_partieState != PartieState.PartieStarted) return;
        AddToAvancement(pAvancement);
        _menuManager.AddPonctualAvancementInPrompt(pAvancement);
    }

    public void RemovePonctualAvancement(int pAvancement)
    {
        AddToAvancement(-pAvancement);
        _menuManager.RemovePonctualAvancementInPrompt(pAvancement);
    }

    //Compteur pour différencier les SkyBlocks entre eux
    public int GetNewSkyCompteur()
    {
        _skyCompteur += 1;
        //Il n'y aura jamais 100 sky bg en mm temps, rien ne sert de grimper à l'infini
        if (_skyCompteur > 100) _skyCompteur = 0;
        return _skyCompteur;
    }

    //On sauvegarde ici les données à la fermeture de l'application
    void OnApplicationQuit()
    {
        SaveManager.SavePlayerSave();
    }

    public IEnumerator KillPlayer(float pDeathTime)
    {
        GetComponent<MusicManager>().StopAllMusic();

        ChangeState(PartieState.PlayerIsDying);

        _noiseScreen.SetActive(true);
        _noiseScreen.GetComponent<NoiseScreen>()._isAppearing = true;
        yield return new WaitForSeconds(pDeathTime);

        GetInGame = null;
        ReloadPartie();
    }

    public IEnumerator ChangeLocale(string pLocale)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(pLocale);
        SaveManager.MajLocales(pLocale);
    }

    public void ChangeColorBlindMode(int pType)
    {
        _colorBlindRF.Type = pType;
        SaveManager.MajColorBlindMode(pType);
    }

    public void ChangeMotionSickness(bool pValue)
    {
        SaveManager.MajMotionSick(pValue);
        _menuManager.transform.Find("InGameMenu").Find("Reticule").gameObject.SetActive(pValue);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>().DontSmoothSpeed = pValue;
    }
}

public enum PartieState
{
    MustLoadSave,
    LoadingSave,
    ReloadingScene,
    GameDataIsReady,
    LoadingHomeMenu,
    InHomenu,
    InGame,
    PartieStarted,
    PlayerIsDying
}

public enum DebugModes
{
    Invincible,
    MoveAlone,
    PrintFlappInfo
}