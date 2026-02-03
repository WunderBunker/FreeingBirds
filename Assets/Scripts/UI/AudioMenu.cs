using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenu : MonoBehaviour
{
    [SerializeField] protected AudioMixer _mixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        _mixer.GetFloat("MasterVolume", out float vMasterVol);
        _mixer.GetFloat("FXVolume", out float vFxVol);
        _mixer.GetFloat("MusicVolume", out float vMusicVol);

        transform.Find("MainVolume").GetComponent<Slider>().value
            = Mathf.Pow(10, vMasterVol / 20);
        transform.Find("FxVolume").GetComponent<Slider>().value
            = Mathf.Pow(10, vFxVol / 20);
        transform.Find("MusicVolume").GetComponent<Slider>().value
            = Mathf.Pow(10, vMusicVol / 20);
    }

    void OnDisable()
    {
        _mixer.GetFloat("MasterVolume", out float vMasterVol);
        _mixer.GetFloat("FXVolume", out float vFxVol);
        _mixer.GetFloat("MusicVolume", out float vMusicVol);

        SaveManager.MajSoundVolumes(vMasterVol, vFxVol, vMusicVol);
    }

    public void OnMasterChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("MasterVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("MasterVolume", -80);
    }

    public void OnFxChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("FXVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("FXVolume", -80);
    }

    public void OnMusicChange(float pValue)
    {
        if (pValue > 0)
        {
            float vVolumeDB = Mathf.Log10(pValue) * 20;
            _mixer.SetFloat("MusicVolume", vVolumeDB);
        }
        else
            _mixer.SetFloat("MusicVolume", -80);
    }
}
