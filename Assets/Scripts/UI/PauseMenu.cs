using System.Linq;
using UnityEngine;

public class PauseMenu : MonoBehaviour, IChildEnabler
{
    string[] _internalEnablingFlags = new string[] { "Book", "LanguageMenu", "ColorBlindMenu" };

    void OnEnable()
    {
        Time.timeScale = 0;
        EnableChilds(false, new string[0]);
    }

    public void OnQuit()
    {
        AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1;
        transform.Find("Book").GetComponent<BookImage>().Quit();
    }

    public void LaunchLanguageMenu()
    {
        transform.Find("LanguageMenu").gameObject.SetActive(true);
        AudioManager.Instance.PlayClickSound();
    }

    public void LaunchColorBlindMenu()
    {
        transform.Find("ColorBlindMenu").gameObject.SetActive(true);
        AudioManager.Instance.PlayClickSound();
    }

    public void OnBackToMenu()
    {
        Time.timeScale = 1;
        SaveManager.MajScore(SaveManager.SafeSave.SelectedBirdId, PartieManager.Instance._avancement);
        GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MenuManager>().ChooseHomeMenu();
        OnQuit();
    }

    public void EnableChilds(bool pActive, string[] pExternalFlag = null)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (!_internalEnablingFlags.Contains(transform.GetChild(i).name)
                && !pExternalFlag.Contains(transform.GetChild(i).name))
                transform.GetChild(i).gameObject.SetActive(pActive);
        transform.Find("HomeButton").gameObject.SetActive
            (!GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("HomeMenu").gameObject.activeSelf);
    }
}

public interface IChildEnabler
{
    public void EnableChilds(bool pActive, string[] pFlag);
}
