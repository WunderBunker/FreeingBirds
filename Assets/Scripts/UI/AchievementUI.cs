using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class AchievementUI : MonoBehaviour
{
    public enum AchievState { Got, NotGot };
    public AchievState State;
    public Achievement Achievement;

    private List<(LocalizedString, LocalizedString.ChangeHandler)> _majArgsList = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (State == AchievState.Got) GetComponent<Animator>().SetBool("Got", true);
        else if (State == AchievState.NotGot) GetComponent<Animator>().SetBool("NotGot", true);

        Achievement.LocalText.StringChanged += UpdateString;

        if (Achievement.LocalArg.Length > 0)
        {
            Achievement.LocalText.Arguments = new object[Achievement.LocalArg.Length];

            foreach (var lArg in Achievement.LocalArg)
            {
                if (lArg.LocalizedString.TableReference != null)
                {

                    LocalizedString.ChangeHandler vMajArgs = (string pText) =>
                    {
                        Achievement.LocalText.Arguments[lArg.Index] = pText;
                        Achievement.LocalText.RefreshString();
                    };

                    lArg.LocalizedString.StringChanged += vMajArgs;
                    _majArgsList.Add((lArg.LocalizedString, vMajArgs));

                    lArg.LocalizedString.RefreshString();
                }
                else Achievement.LocalText.Arguments[lArg.Index] = lArg.String;
            }
        }
        Achievement.LocalText.RefreshString();
    }

    void UpdateString(string pText) => transform.Find("Text").GetComponent<TextMeshProUGUI>().text = pText;

    void OnDestroy()
    {
        Achievement.LocalText.StringChanged -= UpdateString;

        foreach (var (lLocalizedString, lMajArg) in _majArgsList)
            lLocalizedString.StringChanged -= lMajArg;
    }
}
