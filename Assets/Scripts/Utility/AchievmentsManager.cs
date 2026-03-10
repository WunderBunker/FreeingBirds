using System.Linq;
using UnityEngine;

public class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager Instance;

    [SerializeField] BirdList _birdList;
    [SerializeField] AchievementList _achievList;

    AchievementNotif _notif;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        _notif = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("AchievementNotif").GetComponent<AchievementNotif>();
    }

    void TryAchievement(int pId, bool pNoNotif = false)
    {
        if (SaveManager.AddGottenAchievement(pId) && !pNoNotif)
            _notif.SendAchievNotif(_achievList.List.First((_) => _.Id == pId));
    }

    public void UpdateScoreAchievements(float pScore, string pBirdID ="", bool pNoNotif = false)
    {
        if (pBirdID == "") pBirdID = SaveManager.SafeSave.SelectedBirdId;

        switch (pBirdID)
        {
            case "Bird1":
                if (pScore > 3000) TryAchievement(10, pNoNotif);
                else if (pScore > 2000) TryAchievement(5, pNoNotif);

                if (pScore >= _birdList.First((_) => _.Id == "Bird2").ScoreToBeAccessible)
                    TryAchievement(0, pNoNotif);
                break;
            case "Bird2":
                if (pScore > 3000) TryAchievement(11, pNoNotif);
                else if (pScore > 2000) TryAchievement(6, pNoNotif);

                if (pScore >= _birdList.First((_) => _.Id == "Bird2").ScoreToBeAccessible)
                    TryAchievement(1, pNoNotif);
                break;
            case "Bird3":
                if (pScore > 3000) TryAchievement(12, pNoNotif);
                else if (pScore > 2000) TryAchievement(7, pNoNotif);

                if (pScore >= _birdList.First((_) => _.Id == "Bird2").ScoreToBeAccessible)
                    TryAchievement(2, pNoNotif);
                break;
            case "Bird4":
                if (pScore > 3000) TryAchievement(13, pNoNotif);
                else if (pScore > 2000) TryAchievement(8, pNoNotif);

                if (pScore >= _birdList.First((_) => _.Id == "Bird2").ScoreToBeAccessible)
                    TryAchievement(3, pNoNotif);
                break;
            case "Bird5":
                if (pScore > 3000) TryAchievement(14, pNoNotif);
                else if (pScore > 2000) TryAchievement(9, pNoNotif);
                break;
            default:
                break;
        }
    }

    public void AllHumanModes() => TryAchievement(4);
}
