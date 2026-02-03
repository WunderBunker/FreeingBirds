using UnityEngine;

public class FlapTrail : MonoBehaviour
{
    [SerializeField] Color gColor1Start;
    [SerializeField] Color gColor1End;
    [SerializeField] Color gColor2Start;
    [SerializeField] Color gColor2End;

    TrailRenderer _trail;
    float _trailDuration;

    bool _isEmitting;
    float _startTime;

    // Start is called before the first frame update
    void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _trailDuration = _trail.time;
        //if (transform.parent.GetComponent<PlayerControl>() is { } vPC) vPC.FlapTrail = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isEmitting)
        {
            float vTimeSpent = Time.time - _startTime;
            float vCoef = vTimeSpent / _trailDuration;
            if (vTimeSpent < _trailDuration)
            {
                Gradient vGradient = new Gradient();
                GradientColorKey vColorKeyStart = new GradientColorKey(Color.Lerp(gColor1Start, gColor2Start, vCoef), 0f);
                GradientColorKey vColorKeyEnd = new GradientColorKey(Color.Lerp(gColor1End, gColor2End, vCoef), 100f);
                GradientAlphaKey vAlphaKeyStart = new GradientAlphaKey(Mathf.Lerp(gColor1Start.a, gColor2Start.a, vCoef), 0f);
                GradientAlphaKey vAlphaKeyEnd = new GradientAlphaKey(Mathf.Lerp(gColor1End.a, gColor2End.a, vCoef), 100f);

                vGradient.SetKeys(new[] { vColorKeyStart, vColorKeyEnd }, new[] { vAlphaKeyStart, vAlphaKeyEnd });

                _trail.colorGradient = vGradient;
            }
            else
            {
                _trail.emitting = false;
                _isEmitting = false;
            }
        }
    }

    public void StartTrailing()
    {
        Gradient vGradient = new Gradient();
        GradientColorKey vColorKeyStart = new GradientColorKey(gColor1Start, 0f);
        GradientColorKey vColorKeyEnd = new GradientColorKey(gColor1End, 100f);
        GradientAlphaKey vAlphaKeyStart = new GradientAlphaKey(gColor1Start.a, 0f);
        GradientAlphaKey vAlphaKeyEnd = new GradientAlphaKey(gColor1End.a, 100f);

        vGradient.SetKeys(new[] { vColorKeyStart, vColorKeyEnd }, new[] { vAlphaKeyStart, vAlphaKeyEnd });

        _trail.colorGradient = vGradient;

        _trail.emitting = true;
        _isEmitting = true;
        _startTime = Time.time;
    }
}
