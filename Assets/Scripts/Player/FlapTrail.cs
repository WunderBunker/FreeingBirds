using UnityEngine;

public class FlapTrail : MonoBehaviour
{
    [SerializeField] Color gColor1Start;
    [SerializeField] Color gColor1End;
    [SerializeField] Color gColor2Start;
    [SerializeField] Color gColor2End;

    TrailRenderer gTrail;
    float gTrailDuration;

    bool gIsEmitting;
    float gStartTime;

    // Start is called before the first frame update
    void Awake()
    {
        gTrail = GetComponent<TrailRenderer>();
        gTrailDuration = gTrail.time;
        if (transform.parent.GetComponent<PlayerControl>() is { } vPC) vPC.FlapTrail = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (gIsEmitting)
        {
            float vTimeSpent = Time.time - gStartTime;
            float vCoef = vTimeSpent / gTrailDuration;
            if (vTimeSpent < gTrailDuration)
            {
                Gradient vGradient = new Gradient();
                GradientColorKey vColorKeyStart = new GradientColorKey(Color.Lerp(gColor1Start, gColor2Start, vCoef), 0f);
                GradientColorKey vColorKeyEnd = new GradientColorKey(Color.Lerp(gColor1End, gColor2End, vCoef), 100f);
                GradientAlphaKey vAlphaKeyStart = new GradientAlphaKey(Mathf.Lerp(gColor1Start.a, gColor2Start.a, vCoef), 0f);
                GradientAlphaKey vAlphaKeyEnd = new GradientAlphaKey(Mathf.Lerp(gColor1End.a, gColor2End.a, vCoef), 100f);

                vGradient.SetKeys(new[] { vColorKeyStart, vColorKeyEnd }, new[] { vAlphaKeyStart, vAlphaKeyEnd });

                gTrail.colorGradient = vGradient;
            }
            else
            {
                gTrail.emitting = false;
                gIsEmitting = false;
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

        gTrail.colorGradient = vGradient;

        gTrail.emitting = true;
        gIsEmitting = true;
        gStartTime = Time.time;
    }
}
