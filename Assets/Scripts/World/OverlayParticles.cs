using UnityEngine;

public class OverlayParticles : MonoBehaviour
{
    [SerializeField] float Alpha = 0.8f;
    [SerializeField] float PositionOffset = 10;

    Transform _cameraTransform;
    Vector3 _lastCamPos;

    float _initCamSize;
    ParticleSystem.MinMaxCurve _initSizes;


    void Awake()
    {
        _cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _lastCamPos = _cameraTransform.position;
        _initCamSize = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().orthographicSize;
        _initSizes = GetComponent<ParticleSystem>().main.startSize;
    }

    public void SetColor(Color pColor)
    {
        Color vFirstColor = new Color(pColor.r, pColor.g, pColor.b, Alpha);

        float vHue, vSaturation, vValue;
        Color.RGBToHSV(vFirstColor, out vHue, out vSaturation, out vValue);
        vValue *= 1.5f;
        vSaturation /= 1.5f;
        Color vSecondColor = Color.HSVToRGB(vHue, vSaturation, vValue);
        vSecondColor.a = Alpha;

        var ColorGradient = new ParticleSystem.MinMaxGradient(vFirstColor, vSecondColor);
        var vMain = GetComponent<ParticleSystem>().main;
        vMain.startColor = ColorGradient;
    }

    public void SetSize(Vector3 pSize)
    {

        var vShape = GetComponent<ParticleSystem>().shape;
        vShape.scale = pSize;

        float vSizeCoef = pSize.y / 2 / _initCamSize;
        var vMain = GetComponent<ParticleSystem>().main;
        vMain.startSize = new(_initSizes.constantMin * vSizeCoef, _initSizes.constantMax * vSizeCoef);
    }

    public void Activate(bool pActivate)
    {
        if (pActivate) GetComponent<ParticleSystem>().Play();
        else GetComponent<ParticleSystem>().Stop();
    }

    void Update()
    {
        transform.position = (Vector3)(Vector2)_cameraTransform.position + (_cameraTransform.position - _lastCamPos).normalized * PositionOffset;
        _lastCamPos = _cameraTransform.position;
    }
}
