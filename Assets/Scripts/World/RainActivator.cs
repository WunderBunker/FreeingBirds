using UnityEngine;

public class RainActivator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>().RainCam.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        try
        {
            GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<CamFollowPath>()?.RainCam.gameObject.SetActive(false);
        }
        catch (System.Exception) { }
    }
}
