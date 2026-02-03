using System.Linq;
using UnityEngine;

public class ColorBlindMenu : MonoBehaviour,IChildEnabler
{
    void OnEnable()
    {
        EnableChilds(false, new string[]{"Book"});
    }

    public void Quit()
    {
        AudioManager.Instance.PlayClickSound();
        transform.Find("Book").GetComponent<BookImage>().Quit();
    }

    public void EnableChilds(bool pActive, string[] pExternalFlag = null)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (!pExternalFlag.Contains(transform.GetChild(i).name)) transform.GetChild(i).gameObject.SetActive(pActive);
    }
}
