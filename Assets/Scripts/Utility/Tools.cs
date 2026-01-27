using System;
using System.Collections;
using System.Linq;
using UnityEngine;

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


    // Représentation d’un rectangle orienté
    public struct OrientedRect
    {
        public Vector2 Center;
        public Vector2 HalfExtents;  // largeur/2, hauteur/2
        public float AngleRad;       // angle en radians

        public OrientedRect(Vector2 pCenter, Vector2 pSize, float pAngleDeg)
        {
            Center = pCenter;
            HalfExtents = pSize * 0.5f;
            AngleRad = pAngleDeg * Mathf.Deg2Rad;
        }

        public Vector2 Right => new Vector2(Mathf.Cos(AngleRad), Mathf.Sin(AngleRad));
        public Vector2 Up => new Vector2(-Mathf.Sin(AngleRad), Mathf.Cos(AngleRad));
    }

    // Test principal SAT
    public static bool Overlap(OrientedRect pRectA, OrientedRect pRectB)
    {
        // Axes à tester : les axes locaux de A et de B
        Vector2[] vAxes = {
            pRectA.Right,
            pRectA.Up,
            pRectB.Right,
            pRectB.Up
        };

        foreach (var lAxis in vAxes)
        {
            if (!OverlapOnAxis(pRectA, pRectB, lAxis))
                return false; // Axe séparateur trouvé => pas d’overlap
        }

        return true; // Aucun axe séparateur => overlap
    }

    // Projection d’un OBB sur un axe
    private static bool OverlapOnAxis(OrientedRect pRectA, OrientedRect pRectB, Vector2 pAxis)
    {
        // Normaliser l’axe
        pAxis.Normalize();

        float vProjA = ProjectRadius(pRectA, pAxis);
        float vProjB = ProjectRadius(pRectB, pAxis);

        float vDistance = Mathf.Abs(Vector2.Dot(pRectB.Center - pRectA.Center, pAxis));

        return vDistance <= vProjA + vProjB;
    }

    // Rayon projeté d’un OBB sur un axe
    private static float ProjectRadius(OrientedRect pRect, Vector2 pAxis)
    {
        float vRadius =
            Mathf.Abs(Vector2.Dot(pRect.Right * pRect.HalfExtents.x, pAxis)) +
            Mathf.Abs(Vector2.Dot(pRect.Up * pRect.HalfExtents.y, pAxis));

        return vRadius;
    }

    public static Component CopyComponent(Component pOriginal, GameObject pDestination, string[] pNoGoFlags = null)
    {
        Type vType = pOriginal.GetType();
        Component vCopy = pDestination.GetComponent(vType);
        if (vCopy == null)
        {
            Debug.Log(string.Format("Ne peux pas copier {0} sur {1} ", vType, pDestination.name));
            return null;
        }

        var vFields = vType.GetFields();
        foreach (var lField in vFields)
        {
            if (lField.IsStatic) continue;
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

    public static Vector2 GetVectorToRectEdge(float pL, float pH, float pAngle)
    {
        float tan = Mathf.Tan(pAngle);
        float cos = Mathf.Cos(pAngle);
        float sin = Mathf.Sin(pAngle);

        if (Mathf.Abs(tan) <= pH / pL)
        {
            float x = Mathf.Sign(cos) * pL / 2;
            return new Vector2(x, x * tan);
        }
        else
        {
            float y = Mathf.Sign(sin) * pH / 2;
            return new Vector2(y / tan, y);
        }
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

