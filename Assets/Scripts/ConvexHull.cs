using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProcGenTiles;


public class ConvexHull
{

    public static double cross((int x, int y) O, (int x, int y) A, (int x, int y) B)
    {
        return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
    }

    /*public static List<(int x, int y)> GetConvexHull(List<(int x, int y)> points)
    {*/
   public static List<(int x, int y)> GetConvexHull(List<Tile> points)
    {
        if (points == null)
            return null;

        if (points.Count() <= 1)
            return null;

        int n = points.Count(), k = 0;

        List<(int x, int y)> H = new List<(int x, int y)>(new (int x, int y)[2 * n]);

        points.Sort((a, b) =>
             a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        // Build lower hull
        for (int i = 0; i < n; ++i)
        {
            while (k >= 2 && cross(H[k - 2], H[k - 1], (points[i].x, points[i].y)) <= 0)
                k--;
            H[k++] = (points[i].x, points[i].y);
        }

        // Build upper hull
        for (int i = n - 2, t = k + 1; i >= 0; i--)
        {
            while (k >= t && cross(H[k - 2], H[k - 1], (points[i].x, points[i].y)) <= 0)
                k--;
            H[k++] = (points[i].x, points[i].y);
        }


        return H.Take(k - 1).ToList();
    }
}

