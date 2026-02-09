using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class Tools
{
    public static bool SegmentsIntersect(Vector2 pSegment1Start, Vector2 pSegment1End, Vector2 pSegment2Start, Vector2 pSegment2End)
    {

        //pour plus de détails sur l'algo : https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        //On a ici une version simplifiée qui ne gère pas les segments collinéaires
        if (PointsOrientation(pSegment1Start, pSegment1End, pSegment2Start) != PointsOrientation(pSegment1Start, pSegment1End, pSegment2End)
            && PointsOrientation(pSegment2Start, pSegment2End, pSegment1Start) != PointsOrientation(pSegment2Start, pSegment2End, pSegment1End))
            return true;
        else return false;
    }

    // Détemine l'orientation qu'ont 3 points successifs (p, q, r).     
    // Return 0 --> collineaires
    // 1 --> Clockwise 
    // 2 --> Counterclockwise 
    public static int PointsOrientation(Vector2 pPoint1, Vector2 pPoint2, Vector2 pPoint3)
    {
        float vReturn = (pPoint2.y - pPoint1.y) * (pPoint3.x - pPoint2.x) -
                  (pPoint2.x - pPoint1.x) * (pPoint3.y - pPoint2.y);

        return (vReturn > 0) ? 1 : ((vReturn < 0) ? 2 : 0);
    }

    public static Vector2 GetPolygonColliderSize(PolygonCollider2D pCollider)
    {
        float vMinX = pCollider.points[0].x;
        float vMinY = pCollider.points[0].y;
        float vMaxX = pCollider.points[0].x;
        float vMaxY = pCollider.points[0].y;

        foreach (Vector2 lPoint in pCollider.points)
        {
            if (lPoint.x < vMinX) vMinX = lPoint.x;
            if (lPoint.x > vMaxX) vMaxX = lPoint.x;
            if (lPoint.y < vMinY) vMinY = lPoint.y;
            if (lPoint.y > vMaxY) vMaxY = lPoint.y;
        }

        return new Vector2(vMaxX - vMinX, vMaxY - vMinY);
    }


    public static T GetRenderFeature<T>()
    {
        var vUrpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (vUrpAsset != null)
        {
            var vRendererData = vUrpAsset.rendererDataList[0] as UniversalRendererData;
            if (vRendererData != null)
                foreach (var lFeature in vRendererData.rendererFeatures)
                {
                    if (lFeature is T myFeature)
                    {
                        return myFeature;
                    }
                }
        }

        return default;
    }

    public static Component CopyComponent(Component pOriginal, GameObject pDestination, string[] pNoGoFlags = null, bool pCreateIfNone = false)
    {
        Type vType = pOriginal.GetType();
        Component vCopy = pDestination.GetComponent(vType);
        if (vCopy == null)
        {
            if (pCreateIfNone) vCopy = pDestination.AddComponent(vType);
            else
            {
                Debug.Log(string.Format("Ne peux pas copier {0} sur {1} ", vType, pDestination.name));
                return null;
            }
        }

        var vFields = vType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var lField in vFields)
        {
            if (lField.IsStatic || (!lField.IsPublic && lField.GetCustomAttribute<SerializeField>() == null)) continue;
           
            lField.SetValue(vCopy, lField.GetValue(pOriginal));
        }
        var vProps = vType.GetProperties();
        string[] vDefaultNoGoFlags = new string[] { "name" };
        foreach (var lProp in vProps)
        {
            if (!lProp.CanWrite || !lProp.CanRead || vDefaultNoGoFlags.Contains(lProp.Name)
                || (pNoGoFlags != null && pNoGoFlags.Contains(lProp.Name))) continue;

            try
            {
                lProp.SetValue(vCopy, lProp.GetValue(pOriginal, null), null);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Can't copy property : " + lProp.Name + " because : " + e);
            }
        }
        return vCopy;
    }

    public static float EllipseRadius(float pRX, float pRY, float pAlpha)
    {
        float vCosAlpha = Mathf.Cos(pAlpha);
        float vSinAlpha = Mathf.Sin(pAlpha);

        return pRX * pRY / Mathf.Sqrt(Mathf.Pow(pRY * vCosAlpha, 2) + Mathf.Pow(pRX * vSinAlpha, 2));
    }
}


public class TiltingTransforms : IEnumerable, IEnumerator
{
    private float _currentTilt = 0;
    private float _maxTilt;
    float _speed;

    private Transform[] _camTransforms;

    public TiltingTransforms(float pMaxTilt, float pSpeed, Transform[] pCamTransforms)
    {
        _maxTilt = Math.Abs(pMaxTilt);
        _camTransforms = pCamTransforms;
        _speed = pSpeed;
    }

    // Implémentation d'IEnumerator 
    public object Current => null;
    public void Reset()
    {
        _currentTilt = 0;
    }

    public bool MoveNext()
    {
        if (Mathf.Abs(_currentTilt) < _maxTilt)
        {
            float vIncrement = Time.deltaTime * _speed;

            foreach (Transform lTransform in _camTransforms)
                lTransform.Rotate(0, 0, vIncrement);

            _currentTilt += Mathf.Abs(vIncrement);
            return true;
        }
        return false; // Arrête l'itération
    }

    public void Dispose() { }

    // Implémentation d'IEnumerable
    // Implémentation d'IEnumerable
    public IEnumerator GetEnumerator()
    {
        return this;
    }

}

