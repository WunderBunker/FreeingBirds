using UnityEngine;

public class ClickNoise : MonoBehaviour
{
    public void Click()
    {
        AudioManager.Instance.PlayClickSound();
    }
}
