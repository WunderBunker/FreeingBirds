using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] protected AudioClip _menuIntro;
    [SerializeField] protected AudioClip _menuLoop;
    [SerializeField] protected BirdList _birdList;

    [SerializeField] List<PlayingAudio> _audioList = new();

    struct PlayingAudio
    {
        public string Name;
        public int Token;
        public PlayingAudio(string pName, int pToken)
        {
            Name = pName;
            Token = pToken;
        }
    }

    protected void StopOneMusic(string pMusicName)
    {
        for (int i = 0; i < _audioList.Count; i++)
        {
            PlayingAudio lSchAudio = _audioList[i];
            if (lSchAudio.Name == pMusicName)
            {
                AudioManager.Instance.StopKeepSound(lSchAudio.Token);
                _audioList.RemoveAt(i);
                break;
            }
        }
    }

    public void StopAllMusic()
    {
        for (int i = _audioList.Count - 1; i >= 0; i--)
        {
            PlayingAudio lSchAudio = _audioList[i];
            AudioManager.Instance.StopKeepSound(lSchAudio.Token);
            _audioList.RemoveAt(i);
        }
    }

    public void PlayMenuTheme()
    {
        foreach (PlayingAudio lShAudio in _audioList)
            if (lShAudio.Name == "Menu1" || lShAudio.Name == "Menu2") return;

        StopAllMusic();

        (int, int) vTokens = AudioManager.Instance.PlayMusicWithIntro(_menuIntro, _menuLoop, 1);
        _audioList.Add(new PlayingAudio("Menu1", vTokens.Item1));
        _audioList.Add(new PlayingAudio("Menu2", vTokens.Item2));
    }

    public void PlayMainLoop()
    {
        foreach (PlayingAudio lShAudio in _audioList)
            if (lShAudio.Name == "Main1" || lShAudio.Name == "Main2") return;

        StopAllMusic();

        Bird vCurrentBird = _birdList.List[0];
        foreach (var lBird in _birdList)
        {
            if (lBird.Id == SaveManager.SafeSave.SelectedBirdId)
            {
                vCurrentBird = lBird;
                break;
            }
        }
        (int, int) vTokens = AudioManager.Instance.PlayMusicWithIntro(vCurrentBird.IntroMusic, vCurrentBird.LoopMusic, 1);
        _audioList.Add(new PlayingAudio("Main1", vTokens.Item1));
        _audioList.Add(new PlayingAudio("Main2", vTokens.Item2));
    }
}
