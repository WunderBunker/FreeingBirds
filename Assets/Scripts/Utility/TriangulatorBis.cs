using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public static class TriangulatorBis
{
    private const float EPSILON = 1e-6f;

    static public int[] Triangulate(ref Vector2[] pPointsPolygone)
    {

        int vNbPointsPolygone = pPointsPolygone.Length;
        List<int> vListeIndicesPointsTriangle = new List<int>();

        if (pPointsPolygone == null || vNbPointsPolygone < 3)
            return Array.Empty<int>();

        //Nettoyage : retirer doublons consécutifs et points très proches
        List<Vector2> vCleaned = Clean(pPointsPolygone);
        if (vCleaned.Count < 3)
            return Array.Empty<int>();
        pPointsPolygone = vCleaned.ToArray();
        vNbPointsPolygone = pPointsPolygone.Length;

        //Table d'indirection permettant de considérer la contraction de la liste de point restant à traiter aisni que leur ordre de lecture
        int[] vTrueIndicesPointsPolygone = new int[vNbPointsPolygone];
        for (int i = 0; i < vNbPointsPolygone; i++) vTrueIndicesPointsPolygone[i] = i;

        /*On détermine le bon ordre de parcours*/
        if (Area(pPointsPolygone) < 0)
            Array.Reverse(vTrueIndicesPointsPolygone);

        //Le nombre de vertex à traiter, on l'initialise au nombre de points total
        int vRemaining = vNbPointsPolygone;
        int vCount = 2 * vRemaining;
        int vCurrVertex = 0;

        while (vRemaining > 2)
        {
            // Polygone peut être dégénéré
            if (vCount-- <= 0)
                break;

            int lPrev = Mod(vCurrVertex - 1, vRemaining);
            int lNext = Mod(vCurrVertex + 1, vRemaining);

            int a = vTrueIndicesPointsPolygone[lPrev];
            int b = vTrueIndicesPointsPolygone[vCurrVertex];
            int c = vTrueIndicesPointsPolygone[lNext];

            if (Snip(a, b, c, vRemaining, vTrueIndicesPointsPolygone, pPointsPolygone))
            {
                vListeIndicesPointsTriangle.Add(a);
                vListeIndicesPointsTriangle.Add(b);
                vListeIndicesPointsTriangle.Add(c);

                // Retire vTrueIndicesPointsPolygone[vVertex]
                for (int i = vCurrVertex; i < vRemaining - 1; i++)
                    vTrueIndicesPointsPolygone[i] = vTrueIndicesPointsPolygone[i + 1];

                vRemaining--;
                vCount = 2 * vRemaining;
            }
            else
            {
                vCurrVertex++;
            }

            if (vCurrVertex >= vRemaining)
                vCurrVertex = 0;
        }

        vListeIndicesPointsTriangle.Reverse();
        return vListeIndicesPointsTriangle.ToArray();
    }

    private static List<Vector2> Clean(Vector2[] pPoints)
    {
        var vOutList = new List<Vector2>();
        Vector2 vPrev = pPoints[0];
        vOutList.Add(vPrev);
        for (int i = 1; i < pPoints.Length; i++)
        {
            Vector2 lCur = pPoints[i];
            if ((lCur - vPrev).sqrMagnitude > EPSILON * EPSILON)
            {
                vOutList.Add(lCur);
                vPrev = lCur;
            }
        }
        // fermer si le dernier est trop proche du premier
        if (vOutList.Count > 1 && (vOutList[vOutList.Count - 1] - vOutList[0]).sqrMagnitude < EPSILON * EPSILON)
        {
            vOutList.RemoveAt(vOutList.Count - 1);
        }
        return vOutList;
    }

    private static float Area(Vector2[] pPoints)
    {
        int vNbPointsPOlygone = pPoints.Length;
        float vArea = 0.0f;

        for (int lCurrentPoint = 0, lPreviousPoint = vNbPointsPOlygone - 1; lCurrentPoint < vNbPointsPOlygone; lPreviousPoint = lCurrentPoint++)
        {
            Vector2 lCurrVal = pPoints[lCurrentPoint];
            Vector2 lPrevVal = pPoints[lPreviousPoint];
            vArea += lPrevVal.x * lCurrVal.y - lCurrVal.x * lPrevVal.y;
        }
        return vArea * 0.5f;
    }

    private static bool Snip(int pBfPoint, int pCurPoint, int pNextPoint, int pPointNb, int[] pTrueIndicesPoints, Vector2[] pPoints)
    {
        Vector2 A = pPoints[pBfPoint];
        Vector2 B = pPoints[pCurPoint];
        Vector2 C = pPoints[pNextPoint];

        // Triangle orienté correctement ?
        if ((((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))) <= Mathf.Epsilon)
            return false;

        // Vérifie qu’aucun autre point n’est dans le triangle
        for (int lOtherPoint = 0; lOtherPoint < pPointNb; lOtherPoint++)
        {
            int lTrueinx = pTrueIndicesPoints[lOtherPoint];
            if ((lTrueinx == pBfPoint) || (lTrueinx == pCurPoint) || (lTrueinx == pNextPoint))
                continue;

            if (InsideTriangle(A, B, C, pPoints[lTrueinx]))
                return false;
        }
        return true;
    }

    private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        // Produits vectoriels
        float cross1 = Cross(B - A, P - A);
        float cross2 = Cross(C - B, P - B);
        float cross3 = Cross(A - C, P - C);

        return cross1 >= 0f && cross2 >= 0f && cross3 >= 0f;
    }

    private static float Cross(Vector2 u, Vector2 v)
    {
        return u.x * v.y - u.y * v.x;
    }

    //Version "circulaire" du modulo, permet de gérer les cas négatifs (ex : -1 va donner m-1) 
    private static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}