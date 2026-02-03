using UnityEngine;

public class ColorBlindButton : MonoBehaviour
{
    [SerializeField] int _type;
    public void OnButtonClick()
    {
        PartieManager.Instance.ChangeColorBlindMode(_type);
    }
}
