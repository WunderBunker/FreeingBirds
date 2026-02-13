using UnityEngine;
using UnityEngine.UI;

public class MotionSicknessButton : MonoBehaviour
{

    void OnEnable()
    {
        GetComponent<Toggle>().isOn = SaveManager.SafeSave.SettingsSave.Performances;
    }

    public void OnToggleChange(bool pValue)
    {
        PartieManager.Instance.ChangeMotionSickness(pValue);
    }

}

