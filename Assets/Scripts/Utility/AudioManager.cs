using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//GESTION DU LANCEMENT ET DE L'ARRET DES SONS DU JEUX
public class AudioManager : MonoBehaviour
{
    static public AudioManager Instance;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected AudioMixer _mainMixer;
    [SerializeField] protected AudioClip _clickSound;

    private Transform __center;
    private Transform _center
    {
        get
        {
            if (__center == null)
            {
                __center = GameObject.Find("PlayerCenteredSounds")?.transform;
                if (__center == null) __center = GameObject.FindGameObjectWithTag("MainCamera").transform;
            }
            return __center;
        }
        set { __center = value; }
    }

    Dictionary<int, AudioSource> _audioSources = new Dictionary<int, AudioSource>();

    private int _keepSoundToken = 0;

    List<AudioSource> _musics = new();

    //Audio / fadeout speed
    Dictionary<AudioSource, float> _fadingOutSources = new();
    Dictionary<AudioSource, float> _fadingOutSourcesForPause = new();

    //Audio / (fadeIn speed, final volume)
    Dictionary<AudioSource, (float, float)> _fadingInSources = new();
    Dictionary<AudioSource, (float, float)> _fadingInSourcesForPause = new();

    bool _unPausing;

    float _cameraRange;

    void Awake()
    {
        //Application du design Singleton + persistence de scène
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        _cameraRange = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().farClipPlane;

        //TO-DO : charger sauvegarde avec paramètres de son
        Action<PlayerSave> aGetSave = (pPlayerSave) =>
        {
            _mainMixer.SetFloat("MasterVolume", pPlayerSave.SettingsSave.MasterVolume);
            _mainMixer.SetFloat("FXVolume", pPlayerSave.SettingsSave.MasterVolume);
            _mainMixer.SetFloat("MusicVolume", pPlayerSave.SettingsSave.MasterVolume);
        };

        if (SaveManager._save == null)
            StartCoroutine(SaveManager.LoadPlayerSave(aGetSave));
        else aGetSave.Invoke(SaveManager._save);
    }

    void Update()
    {
        FadeInSounds();
        FadeOutSounds();
    }

    //Joue un son sans boucle
    public void PlaySound(AudioClip pAudio, float pVolume, Vector3? pPosition = null, bool pFollowCenter = false,
        float pFadeInSpeed = 0, bool pIsMusic = false)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);
        if (pFollowCenter) vAudioSource.transform.SetParent(_center);

        vAudioSource.maxDistance = _cameraRange / 2;
        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups(pIsMusic ? "Music" : "FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pFadeInSpeed != 0 ? 0 : pVolume;
        vAudioSource.Play();

        if (pFadeInSpeed != 0) _fadingInSources.Add(vAudioSource, (pVolume, pFadeInSpeed));

        if (pIsMusic) _musics.Add(vAudioSource);
        else Destroy(vAudioSource.gameObject, vAudioSource.clip.length);
    }

    //Joue un son avec boucle. Renvoie un token permettant à l'objet appelant de demander la coupure du son 
    public int PlayKeepSound(AudioClip pAudio, float pVolume, Vector3? pPosition = null, Transform pParent = null, bool pFollowCenter = false,
    float pFadeInSpeed = 0, bool pIsMusic = false)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity, pParent);
        if (pFollowCenter) vAudioSource.transform.SetParent(_center);

        vAudioSource.maxDistance = _cameraRange / 2;
        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups(pIsMusic ? "Music" : "FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pFadeInSpeed != 0 ? 0 : pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();
        _keepSoundToken += 1;

        if (pFadeInSpeed != 0) _fadingInSources.Add(vAudioSource, (pVolume, pFadeInSpeed));

        _audioSources.Add(_keepSoundToken, vAudioSource);

        if (pIsMusic) _musics.Add(vAudioSource);

        return _keepSoundToken;
    }

    //Coupure d'un son qui boucle
    public void StopKeepSound(int pToken, float pSpeed = 2)
    {
        if (!_audioSources.ContainsKey(pToken))
        {
            Debug.Log("Pas de son à stopper pour le token  : " + pToken);
            return;
        }

        //On bascule en fait le son dans une liste dont les éléments sont tûs progressivement
        _fadingOutSources.Add(_audioSources[pToken], pSpeed);
        _audioSources.Remove(pToken);
    }

    //Joue un son qui boucle mais s'arrête de lui-même au bout d'un certain temps
    public IEnumerator PlayKeepSoundForATime(AudioClip pAudio, float pVolume, float pTime, float pFadeOutSpeed = 2, Vector3? pPosition = null, Transform pParent = null,
    bool pFollowCenter = false, float pFadeInSpeed = 0, bool pIsMusic = false)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity, pParent);
        if (pFollowCenter) vAudioSource.transform.SetParent(_center);

        vAudioSource.maxDistance = _cameraRange / 2;
        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups(pIsMusic ? "Music" : "FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pFadeInSpeed != 0 ? 0 : pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();

        if (pFadeInSpeed != 0) _fadingInSources.Add(vAudioSource, (pVolume, pFadeInSpeed));
        if (pIsMusic) _musics.Add(vAudioSource);

        yield return new WaitForSeconds(pTime);

        if (vAudioSource != null) _fadingOutSources.Add(vAudioSource, pFadeOutSpeed);
    }

    //Joue le son de click sur un bouton (son très utilisé donc on en fait une méthode à part sans paramètres)
    public void PlayClickSound()
    {
        PlaySound(_clickSound, 1);
    }

    public void StopAllMusic(float pFadeOutSpeed = 2)
    {
        foreach (AudioSource lSource in _musics)
        {
            if (!_fadingOutSources.ContainsKey(lSource))
                _fadingOutSources.Add(lSource, pFadeOutSpeed);
        }

        _musics.Clear();

    }

    public void PauseAllMusic(bool pPause, float pFadeOutSpeed = 2)
    {
        List<AudioSource> vMusicToRemove = new();

        if (pPause)
        {
            _fadingInSourcesForPause.Clear();
            foreach (AudioSource lSource in _musics)
            {
                if (lSource == null)
                {
                    vMusicToRemove.Add(lSource);
                    continue;
                }
                _fadingOutSourcesForPause.Add(lSource, pFadeOutSpeed);
                _fadingInSourcesForPause.Add(lSource, (pFadeOutSpeed, lSource.volume));
            }
        }
        else
        {
            _fadingOutSourcesForPause.Clear();
            foreach (KeyValuePair<AudioSource, (float, float)> lSource in _fadingInSourcesForPause)
                lSource.Key.UnPause();
        }

        foreach (AudioSource lSource in vMusicToRemove) _musics.Remove(lSource);
        vMusicToRemove.Clear();

        _unPausing = !pPause;

    }


    //Augmentation progressive des sons en FadeIn
    private void FadeInSounds()
    {
        if (_fadingInSources.Count > 0)
        {

            //On augmente progressivement
            List<AudioSource> vRemoveList = new List<AudioSource>();
            foreach (KeyValuePair<AudioSource, (float, float)> lSource in _fadingInSources)
            {
                if (lSource.Key == null) vRemoveList.Add(lSource.Key);
                else
                {
                    lSource.Key.volume += Time.deltaTime * lSource.Value.Item2;

                    if (lSource.Key.volume >= lSource.Value.Item1)
                    {
                        lSource.Key.volume = lSource.Value.Item1;
                        vRemoveList.Add(lSource.Key);
                    }
                }
            }

            //On supprime les sons dont le volume à atteint 0
            foreach (AudioSource lSource in vRemoveList)
                _fadingInSources.Remove(lSource);
        }
        if (_unPausing && _fadingInSourcesForPause.Count > 0)
        {

            //On augmente progressivement
            List<AudioSource> vUnPausedList = new List<AudioSource>();
            foreach (KeyValuePair<AudioSource, (float, float)> lSource in _fadingInSourcesForPause)
            {
                if (lSource.Key == null) vUnPausedList.Add(lSource.Key);
                else
                {
                    lSource.Key.volume += Time.deltaTime * lSource.Value.Item2;

                    if (lSource.Key.volume >= lSource.Value.Item1)
                    {
                        lSource.Key.volume = lSource.Value.Item1;
                        vUnPausedList.Add(lSource.Key);
                    }
                }
            }

            //On supprime les sons dont le volume à atteint 0
            foreach (AudioSource lSource in vUnPausedList)
                _fadingInSourcesForPause.Remove(lSource);
        }
    }

    //Diminution progressive des sons à couper puis destruction des sources
    private void FadeOutSounds()
    {
        if (_fadingOutSources.Count > 0)
        {
            //On diminue progressivement
            List<AudioSource> vAudioToStop = new List<AudioSource>();
            foreach (KeyValuePair<AudioSource, float> lSource in _fadingOutSources)
            {
                if (lSource.Key == null) vAudioToStop.Add(lSource.Key);
                else
                {
                    lSource.Key.volume -= Time.unscaledDeltaTime * lSource.Value;
                    if (lSource.Key.volume <= 0)
                    {
                        lSource.Key.Stop();
                        vAudioToStop.Add(lSource.Key);
                    }
                }
            }

            //On supprime les sons dont le volume à atteint 0
            foreach (AudioSource lSource in vAudioToStop)
            {
                _fadingOutSources.Remove(lSource);
                if (lSource != null) Destroy(lSource.gameObject);
            }
            vAudioToStop.Clear();
        }

        if (_fadingOutSourcesForPause.Count > 0)
        {
            //On diminue progressivement
            List<AudioSource> vPausedList = new List<AudioSource>();
            foreach (KeyValuePair<AudioSource, float> lSource in _fadingOutSourcesForPause)
            {
                if (lSource.Key == null) vPausedList.Add(lSource.Key);
                else
                {
                    lSource.Key.volume -= Time.unscaledDeltaTime * lSource.Value;
                    if (lSource.Key.volume <= 0)
                    {
                        lSource.Key.Pause();
                        vPausedList.Add(lSource.Key);
                    }
                }
            }

            //On supprime les sons dont le volume à atteint 0
            foreach (AudioSource lSource in vPausedList)
                _fadingOutSourcesForPause.Remove(lSource);
            vPausedList.Clear();
        }
    }

    public (int, int) PlayMusicWithIntro(AudioClip pIntro, AudioClip pLoop, float pVolume, float pFadeInSpeed = 0)
    {
        if (pIntro == null)
        {
            Debug.Log("PlayMusicWithIntro needs an intro and a loop, please use PlayKeepSound otherwise");
            return (0, 0);
        }

        double vStartTime = AudioSettings.dspTime;

        AudioSource vIntroAudioSource = Instantiate(_audioSource, _center.position, Quaternion.identity);
        vIntroAudioSource.maxDistance = _cameraRange / 2;
        vIntroAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("Music")[0];
        vIntroAudioSource.clip = pIntro;
        vIntroAudioSource.volume = pVolume;

        vIntroAudioSource.PlayScheduled(vStartTime);
        Destroy(vIntroAudioSource.gameObject, vIntroAudioSource.clip.length);
        _musics.Add(vIntroAudioSource);

        if (pFadeInSpeed != 0) _fadingInSources.Add(vIntroAudioSource, (pVolume, pFadeInSpeed));

        _keepSoundToken += 1;
        _audioSources.Add(_keepSoundToken, vIntroAudioSource);
        int vIntroToken = _keepSoundToken;

        AudioSource vLoopAudioSource = Instantiate(_audioSource, _center.position, Quaternion.identity);
        vLoopAudioSource.maxDistance = _cameraRange / 2;
        vLoopAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("Music")[0];
        vLoopAudioSource.clip = pLoop;
        vLoopAudioSource.volume = pVolume;
        vLoopAudioSource.loop = true;

        double vIntroDuration = (double)pIntro.samples / pIntro.frequency;
        vLoopAudioSource.PlayScheduled(vStartTime + vIntroDuration);

        _keepSoundToken += 1;
        _audioSources.Add(_keepSoundToken, vLoopAudioSource);
        _musics.Add(vLoopAudioSource);
        int vLoopToken = _keepSoundToken;

        return (vIntroToken, vLoopToken);
    }
}