using TMPro;
using UnityEngine;

public class IntensityAnimator : MonoBehaviour
{
    [SerializeField] Material _mat;
    [SerializeField] bool _textTexture;
    [SerializeField] float _speed;
    [SerializeField] Vector2 _minMax;
    [SerializeField] float _strength = 1;
    [SerializeField] string _intensityProperty = "_Intensity";

    float _time;
    float _noiseOffset;

    void Start()
    {
        _noiseOffset = Random.Range(0f, 1000f); // pour éviter synchro entre objets
        if (_textTexture) _mat = GetComponent<TextMeshProUGUI>().font.material;
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime * _speed;

        float vNoise = Mathf.PerlinNoise(_time, _noiseOffset);
        vNoise = Mathf.Pow(vNoise, _strength);

        float vIntensity = Mathf.Lerp(_minMax.x, _minMax.y, vNoise);

        if (_textTexture)
        {
            Color vHdrColor = _mat.GetColor("_OutlineColor");
            Vector3 vAsVect = new Vector3(vHdrColor.r, vHdrColor.g, vHdrColor.b);
            vAsVect = vAsVect.normalized * vIntensity;
            vHdrColor = new Color(vAsVect.x, vAsVect.y, vAsVect.z);
            _mat.SetColor("_OutlineColor", vHdrColor);
        }
        else _mat.SetFloat(_intensityProperty, vIntensity);
    }
}
