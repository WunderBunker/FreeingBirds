using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public static class SaveManager
{
    //Champ temporaire permettant de simuler en dur un identifiant pour le joueur (notamment pour le leaderBoard)
    public static string _player = "Wunder";
    public static PlayerSave _save { get; private set; }
    static PlayerSave _safeSave;
    public static PlayerSave SafeSave
    {
        get
        {
            if (_save != null)
                return _save;
            else
            {
                if (_safeSave == null)
                    _safeSave = ControlSaveData(new() { UserId = GetPlayerId() });
                return _safeSave;
            }
        }
    }

    static LeaderBoard _currentLeaderBoard;
    static Dictionary<string, LeaderBoard> _localLeaderBoards;
    const string SAVE_NAME = "save";
    public const float _maxLoadingTime = 15;

    public static string DebugString { get; internal set; }

    public static bool MustLauchConnectionPannel;
    static bool _cancelLoading;

    static bool _isLoadingPlayerSave;
    static bool _isLoadingBoardSave;

    static List<Action<PlayerSave>> _playerSaveLoadCB = new();

    //Chargement de la sauvegarde du joueur (Highscore, oboles, comestiques, items achetés...)
    public static IEnumerator LoadPlayerSave(Action<PlayerSave> pCB)
    {
        if (pCB != null)
            _playerSaveLoadCB.Add(pCB);

        if (_isLoadingPlayerSave)
            yield break;
        _isLoadingPlayerSave = true;
        _cancelLoading = false;

        _save = null;
#if UNITY_ANDROID
        LoadPlayerSave_Google();
#else
        LoadPlayerSave_Local();
#endif
        float vTimer = 0;
        while (_save == null)
        {
            yield return null;
            if (_cancelLoading)
                break;
            if (vTimer >= _maxLoadingTime)
            {
                Debug.Log("Player save loading time out");
                CancelLoading();
            }
            if (Time.unscaledDeltaTime < 0.5f)
                vTimer += Time.deltaTime;
        }

        if (_save == null)
        {
            Debug.Log("Fail to load player save");

            if (GameObject.Find("MainCanvas") is { } lCanvas)
                lCanvas
                    .transform.Find("HomeMenu")
                    .Find("ConnectionPannel")
                    .GetComponent<ConnectionPannel>()
                    .ActiveNoSaveText();
            else
                MustLauchConnectionPannel = true;
        }

        foreach (var lCB in _playerSaveLoadCB)
            lCB?.Invoke(SafeSave);

        _playerSaveLoadCB.Clear();
        _isLoadingPlayerSave = false;
    }

    [Obsolete]
    public static IEnumerator LoadLeaderBoard(Action<LeaderBoard> pCB)
    {
        try
        {
            if (_isLoadingBoardSave)
                yield break;
            _isLoadingBoardSave = true;

            _currentLeaderBoard = null;
            _cancelLoading = false;

            Action aLoadBoard = () =>
            {
#if UNITY_ANDROID
                LoadBoard_Google();
#else
                LoadLeaderBoards_Local();
#endif
            };

            if (_save != null)
            {
                SaveLeaderBoard(aLoadBoard);
            }
            else
                aLoadBoard.Invoke();

            float vTimer = 0;

            while (_currentLeaderBoard == null)
            {
                yield return null;
                if (_cancelLoading)
                    break;
                if (vTimer >= _maxLoadingTime)
                {
                    Debug.Log("Leaderboard loading time out");
                    CancelLoading();
                }
                if (Time.unscaledDeltaTime < 0.5f)
                    vTimer += Time.unscaledDeltaTime;
            }

            if (_currentLeaderBoard == null)
            {
                Debug.Log("Fail to load LeaderBoard");
                if (GameObject.Find("MainCanvas") is { } lCanvas)
                    lCanvas
                        .transform.Find("HomeMenu")
                        .Find("ConnectionPannel")
                        .GetComponent<ConnectionPannel>()
                        .ActiveNoSaveText();
            }
            pCB.Invoke(_currentLeaderBoard);
        }
        finally
        {
            _isLoadingBoardSave = false;
        }
    }

    //Enregistrement de la sauvegarde du joueur dans un json local
    public static void SavePlayerSave()
    {
#if UNITY_ANDROID
        SavePlayerSave_Google();
#else
        SavePlayerSave_Local();
        if (_localLeaderBoards == null)
            LoadLeaderBoards_Local();
        for (int i = 0; i < SafeSave.HighScores.Count; i++)
            MajScore(SafeSave.HighScores.ElementAt(i).Key, SafeSave.HighScores.ElementAt(i).Value);
#endif
        SaveLeaderBoard();
    }

    static void SaveLeaderBoard(Action pCB = null)
    {
#if UNITY_ANDROID
        SaveBoard_Google(pCB);
#else
        SaveLeaderBoards_Local(pCB);
#endif
    }

    static LeaderBoard GetNewLeaderBoard()
    {
        string vCurrentBird = SafeSave.SelectedBirdId;
        _currentLeaderBoard = new LeaderBoard()
        {
            Scores = { { GetPlayerId(), SafeSave.HighScores[vCurrentBird] } },
        };
        return _currentLeaderBoard;
    }

    public static string GetPlayerId()
    {
#if UNITY_ANDROID
        return PlayGamesPlatform.Instance.localUser.userName;
#else
        return _player;
#endif
    }

    static string GetSaveName()
    {
#if UNITY_ANDROID
        return SAVE_NAME;
#else
        return GetSaveFolder() + "/saveGame_" + _player;
#endif
    }

#if UNITY_ANDROID
    public static void GoogleAuthenticate(Action<bool, string> pCallBack)
    {
        PlayGamesPlatform.Instance.Authenticate(
            (SignInStatus pStatus) =>
            {
                if (pStatus != SignInStatus.Success)
                    Debug.Log("Authentification failed, caused : " + pStatus);
                else
                    Debug.Log("Authentification succeeded");
                pCallBack.Invoke(pStatus == SignInStatus.Success, pStatus.ToString());
            }
        );
    }

    static void LoadPlayerSave_Google()
    {
        Action<bool, string> aLoad = (bool pIsAuth, string pMsg) =>
        {
            if (!pIsAuth)
                CancelLoading();
            if (_cancelLoading)
                return;

            ISavedGameClient vSavedGameClient = PlayGamesPlatform.Instance.SavedGame;
            vSavedGameClient.OpenWithAutomaticConflictResolution(
                GetSaveName(),
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (SavedGameRequestStatus pStatus, ISavedGameMetadata pGame) =>
                {
                    if (pStatus == SavedGameRequestStatus.Success && pGame != null)
                    {
                        vSavedGameClient.ReadBinaryData(
                            pGame,
                            (SavedGameRequestStatus pReadStatus, byte[] pData) =>
                            {
                                if (pReadStatus == SavedGameRequestStatus.Success && pData != null)
                                {
                                    string vJsonData = Encoding.UTF8.GetString(pData);
                                    _save = JsonConvert.DeserializeObject<PlayerSave>(vJsonData);
                                    if (_save == null)
                                    {
                                        Debug.LogError("Failed to parse save");
                                        CancelLoading();
                                        return;
                                    }
                                    else
                                    {
                                        _save = ControlSaveData(_save);
                                        Debug.Log("Save loading succeeded");
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Failed to read save: " + pReadStatus);
                                    CancelLoading();
                                }
                            }
                        );
                    }
                }
            );
        };

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            GoogleAuthenticate(aLoad);
        else aLoad.Invoke(true, "");
    }

    static void SavePlayerSave_Google()
    {
        Action<bool, string> aSave = (bool pIsAuth, string pMsg) =>
        {
            if (!pIsAuth)
                return;
            ISavedGameClient vSavedGameClient = PlayGamesPlatform.Instance.SavedGame;
            vSavedGameClient.OpenWithAutomaticConflictResolution(
                GetSaveName(),
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (SavedGameRequestStatus pStatus, ISavedGameMetadata pGame) =>
                {
                    if (pStatus == SavedGameRequestStatus.Success)
                    {
                        string vJSonSave = JsonConvert.SerializeObject(SafeSave);
                        byte[] bytes = Encoding.UTF8.GetBytes(vJSonSave);

                        // Met à jour les métadonnées (date, description, etc.)
                        SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                            .WithUpdatedDescription("Sauvegarde du " + DateTime.Now)
                            .Build();

                        vSavedGameClient.CommitUpdate(
                            pGame,
                            update,
                            bytes,
                            (
                                SavedGameRequestStatus pWriteStatus,
                                ISavedGameMetadata pWrittenGame
                            ) =>
                            {
                                if (pWriteStatus == SavedGameRequestStatus.Success) Debug.Log("Sauvegarde réussie sur le cloud !");
                                else Debug.LogError("Erreur d’écriture de la sauvegarde : " + pWriteStatus);
                            }
                        );
                    }
                    else Debug.LogError("Failed to open save ");
                }
            );
        };

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            GoogleAuthenticate(aSave);
        else
            aSave.Invoke(true, "");
    }

    [Obsolete]
    static void LoadBoard_Google()
    {
        Action<bool, string> aLoad = (bool pIsAuth, string pMsg) =>
        {
            // PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_bird1);

            if (!pIsAuth)
                CancelLoading();
            if (_cancelLoading)
                return;

            Action<IScore[]> aCB = (pScores) =>
            {
                List<string> lUserIds = new();
                foreach (IScore lScore in pScores) lUserIds.Add(lScore.userID);
                string[] lUserIdsArray = lUserIds.ToArray();

                PlayGamesPlatform.Instance.LoadUsers(
                    lUserIdsArray,
                    (IUserProfile[] pUsers) =>
                    {
                        LeaderBoard vLeaderBoard = new() { Scores = new() };
                        foreach (IScore lScore in pScores)
                        {
                            IUserProfile lUser = Array.Find(pUsers, item => item.id == lScore.userID);
                            vLeaderBoard.Scores.Add(lUser.userName, lScore.value);
                        }
                        _currentLeaderBoard = vLeaderBoard;
                    }
                );
            };

            string vLBId;
            float vHighScore;
            GetCurrentBoardInfo(out vLBId, out vHighScore);
            PlayGamesPlatform.Instance.LoadScores(vLBId, aCB);
        };

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            GoogleAuthenticate(aLoad);
        else
            aLoad.Invoke(true, "");
    }

    static void SaveBoard_Google(Action pCB = null)
    {
        Action<bool, string> aSave = (bool pIsAuth, string pMsg) =>
        {
            if (!pIsAuth)
            {
                pCB?.Invoke();
                return;
            }
            string vInitBirdId = SafeSave.SelectedBirdId;
            foreach (var lScore in SafeSave.HighScores)
            {
                string vLBId;
                float vHighScore;
                SafeSave.SelectedBirdId = lScore.Key;
                GetCurrentBoardInfo(out vLBId, out vHighScore);

                PlayGamesPlatform.Instance.ReportScore(
                    (long)lScore.Value,
                    vLBId,
                    (bool success) =>
                    {
                        if (success)
                            Debug.Log("Score envoyé avec succès : " + vHighScore);
                        else
                            Debug.LogError("Échec de l’envoi du score !");
                    }
                );
            }
            SafeSave.SelectedBirdId = vInitBirdId;
            pCB?.Invoke();
        };

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            GoogleAuthenticate(aSave);
        else
            aSave.Invoke(true, "");
    }

    static void GetCurrentBoardInfo(out string oLBId, out float oHighScore)
    {
        switch (SafeSave.SelectedBirdId)
        {
            case "Bird1":
                oLBId = GPGSIds.leaderboard_bird1;
                oHighScore = SafeSave.HighScores["Bird1"];
                break;
            case "Bird2":
                oLBId = GPGSIds.leaderboard_bird2;
                oHighScore = SafeSave.HighScores["Bird2"];
                break;
            case "Bird3":
                oLBId = GPGSIds.leaderboard_bird3;
                oHighScore = SafeSave.HighScores["Bird3"];
                break;
            case "Bird4":
                oLBId = GPGSIds.leaderboard_bird4;
                oHighScore = SafeSave.HighScores["Bird4"];
                break;
            case "Bird5":
                oLBId = GPGSIds.leaderboard_bird5;
                oHighScore = SafeSave.HighScores["Bird5"];
                break;
            default:
                oLBId = "";
                oHighScore = 0;
                break;
        }
    }
#else
    public static void LoadPlayerSave_Local()
    {
        PlayerSave vSave = null;
        string vPath = GetSaveName();

        string vTextSave;

        //On récupère la sauvegarde depuis fichier JSon si existe
        if (vPath != null && File.Exists(vPath))
        {
            vTextSave = File.ReadAllText(vPath);

            try
            {
                vSave = JsonConvert.DeserializeObject<PlayerSave>(vTextSave);
            }
            catch (ArgumentException)
            {
                Debug.Log("Erreur dans le chargement de la sauvegarde");
                File.Delete(vPath);
            }
        }
        //Sinon on supprime l'éventuel fichier corrompus et on en créera une nouvelle
        else
        {
            Debug.Log("Pas de fichier save, creation d'un nouveau");
            if (File.Exists(vPath))
                File.Delete(vPath);
        }

        //Création d'une nouvelle sauverde si besoin
        if (vSave == null || vSave.UserId != _player)
            _save = ControlSaveData(new() { UserId = GetPlayerId() });
        else
            _save = vSave;

        _save = ControlSaveData(_save);
    }

    public static void LoadLeaderBoards_Local()
    {
        Dictionary<string, LeaderBoard> vLBs = null;
        string vPath = GetSaveFolder() + "/LeaderBoards.txt";

        string vLBSave;

        //On récupère la sauvegarde depuis fichier JSon si existe
        if (vPath != null && File.Exists(vPath))
        {
            vLBSave = File.ReadAllText(vPath);

            try
            {
                vLBs = JsonConvert.DeserializeObject<Dictionary<string, LeaderBoard>>(vLBSave);
            }
            catch (ArgumentException)
            {
                Debug.Log("Erreur dans le chargement des LeaderBoards");
                File.Delete(vPath);
            }
        }
        //Sinon on supprime l'éventuel fichier corrompus et on en créera une nouvelle
        else
        {
            Debug.Log("Pas de fichier save, creation d'un nouveau");
            if (File.Exists(vPath))
                File.Delete(vPath);
        }

        _localLeaderBoards = vLBs;
        if (_localLeaderBoards != null) _currentLeaderBoard = _localLeaderBoards[SafeSave.SelectedBirdId];
    }

    static Dictionary<string, LeaderBoard> GetNewLeaderBoards()
    {
        var vLeaderBoards = new Dictionary<string, LeaderBoard>
        {
            {
                "Bird1",
                new LeaderBoard() { Scores = { { GetPlayerId(), SafeSave.HighScores["Bird1"] } } }
            },
            {
                "Bird2",
                new LeaderBoard() { Scores = { { GetPlayerId(), SafeSave.HighScores["Bird2"] } } }
            },
            {
                "Bird3",
                new LeaderBoard() { Scores = { { GetPlayerId(), SafeSave.HighScores["Bird3"] } } }
            },
            {
                "Bird4",
                new LeaderBoard() { Scores = { { GetPlayerId(), SafeSave.HighScores["Bird4"] } } }
            },
            {
                "Bird5",
                new LeaderBoard() { Scores = { { GetPlayerId(), SafeSave.HighScores["Bird5"] } } }
            },
        };
        return vLeaderBoards;
    }

    static void SavePlayerSave_Local()
    {
        string vJsonFile = JsonConvert.SerializeObject(SafeSave);
        string vSavePath = GetSaveName();

        File.WriteAllText(vSavePath, vJsonFile);
    }

    static void SaveLeaderBoards_Local(Action pCB = null)
    {
        if (_localLeaderBoards == null)
            LoadLeaderBoards_Local();

        string vJsonFile = JsonConvert.SerializeObject(_localLeaderBoards, Formatting.Indented);
        string vLBPath = GetSaveFolder() + "/LeaderBoards.txt";

        File.WriteAllText(vLBPath, vJsonFile);

        pCB?.Invoke();
    }

    //Renvoie l'emplacemment pour la sauvegarde des données persistentes
    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Application.persistentDataPath;
        return vSavePath;
    }
#endif

    // *** Méthodes de maj des données de la sauvegarde ***
    public static void MajScore(string pBirdId, float pScore)
    {
        if (pScore >= SafeSave.HighScores[pBirdId])
        {
            SafeSave.HighScores[pBirdId] = pScore;
            if (_localLeaderBoards != null)
                _localLeaderBoards[pBirdId].Scores[GetPlayerId()] = pScore;
        }
    }

    public static void MajSelectedBird(string pBirdId)
    {
        SafeSave.SelectedBirdId = pBirdId;
    }

    public static void MajSoundVolumes(float pMaster, float pFX, float pMusic)
    {
        SafeSave.SettingsSave.MasterVolume = pMaster;
        SafeSave.SettingsSave.FxVolume = pFX;
        SafeSave.SettingsSave.MusicVolume = pMusic;
    }

    //***                           ***

    static PlayerSave ControlSaveData(PlayerSave pSave)
    {
        if (pSave.UserId == "")
            pSave.UserId = GetPlayerId();
        if (pSave.SettingsSave == null)
            pSave.SettingsSave = new();
        if (pSave.HighScores == null)
            pSave.HighScores = GetHighScoreDefault();
        else
            foreach (KeyValuePair<string, float> lEntry in GetHighScoreDefault())
                if (!pSave.HighScores.ContainsKey(lEntry.Key))
                    pSave.HighScores.Add(lEntry.Key, lEntry.Value);
        if (pSave.SelectedBirdId == "")
            pSave.SelectedBirdId = "Bird1";
        return pSave;
    }

    public static Dictionary<string, float> GetHighScoreDefault()
    {
        return new Dictionary<string, float>()
        {
            { "Bird1", 0 },
            { "Bird2", 0 },
            { "Bird3", 0 },
            { "Bird4", 0 },
            { "Bird5", 0 },
        };
    }

    public static void CancelLoading()
    {
        _cancelLoading = true;
    }

    public static void MajLocales(string pLanguage)
    {
        SafeSave.SettingsSave.Language = pLanguage;
    }
}

public class PlayerSave
{
    public string UserId { get; set; }
    public SettingsSave SettingsSave = new();

    public Dictionary<string, float> HighScores { get; set; } = SaveManager.GetHighScoreDefault();

    public string SelectedBirdId { get; set; } = "";
}

[Serializable]
public class SettingsSave
{
    //Volume en dB
    public float MasterVolume = -4;
    public float FxVolume = 0;
    public float MusicVolume = 0;

    public string Language;
}

[Serializable]
public class LeaderBoard
{
    public Dictionary<string, float> Scores { get; set; } = new();
}
