using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RainActivator : MonoBehaviour
{
    [SerializeField] RenderTexture _rainText;
    GrabAllScreenRP _grabAllScreenRF;
    RenderObjects _distortionPass;

    void Awake()
    {
        _grabAllScreenRF = Tools.GetRenderFeature<GrabAllScreenRP>();
        _distortionPass = Tools.GetRenderFeature<RenderObjects>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Activate(true);
    }

    void OnDisable()
    {
        Activate(false);
    }

    public void Activate(bool pActivate)
    {
        if (pActivate)
        {
            if (SaveManager.SafeSave.SettingsSave.Performances) return;

            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollowPath>().RainCam.gameObject.SetActive(true);
            _grabAllScreenRF.SetActive(true);
            _distortionPass.SetActive(true);
        }
        else
        {
            try
            {
                GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<CamFollowPath>()?.RainCam.gameObject.SetActive(false);
                _grabAllScreenRF.SetActive(false);
                _distortionPass.SetActive(false);

                Graphics.SetRenderTarget(_rainText);
                GL.Clear(true, true, new Color(0, 0, 0, 0));
                Graphics.SetRenderTarget(null);
            }
            catch (System.Exception) { }
        }
    }
}
