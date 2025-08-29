using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Implementación simplificada del algoritmo $1 Unistroke Recognizer.
/// Entrena con gestos y los guarda como plantillas JSON.
/// Luego reconoce comparando contra esas plantillas.
/// </summary>
public class DollarRecognizer
{
    private List<GestureTemplate> templates = new List<GestureTemplate>();

    public int TemplateCount => templates.Count;

    // -----------------------------
    // Métodos públicos
    // -----------------------------

    public void AddTemplate(string name, List<Vector2> points)
    {
        GestureTemplate newTemplate = new GestureTemplate
        {
            Name = name,
            Points = Normalize(points)
        };

        templates.Add(newTemplate);
        SaveTemplateToDisk(newTemplate);
    }

    public string Recognize(List<Vector2> points)
    {
        if (templates.Count == 0) return "Sin plantillas";

        List<Vector2> candidate = Normalize(points);

        float bestDistance = float.MaxValue;
        string bestMatch = "Desconocido";

        foreach (var template in templates)
        {
            float dist = GreedyCloudMatch(candidate, template.Points);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestMatch = template.Name;
            }
        }

        return bestMatch;
    }

    public int LoadTemplatesFromDisk()
    {
        templates.Clear();
        string path = "D:/mmy unity notocar/Gestures";// aqui pegas la ruta mano
        if (!Directory.Exists(path)) return 0;

        string[] files = Directory.GetFiles(path, "*.json");
        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            GestureTemplate t = JsonUtility.FromJson<GestureTemplate>(json);
            if (t != null && t.Points != null && t.Points.Count > 0)
            {
                templates.Add(t);
            }
        }
        return templates.Count;
    }

    // -----------------------------
    // Guardado
    // -----------------------------
    // Reemplaza el método existente SaveTemplateToDisk(...) por este:
    // Reemplaza el método existente SaveTemplateToDisk(...) por este:
    private void SaveTemplateToDisk(GestureTemplate template)
    {
        string dir = Path.Combine(Application.persistentDataPath, "Gestures");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // Contar archivos existentes que empiecen por el nombre (A.json, A_1.json, A_2.json, ...)
        string[] existing = Directory.GetFiles(dir, template.Name + "*.json");
        int nextIndex = existing.Length + 1;

        // Nombre de archivo único: A_1.json, A_2.json, ...
        string fileName = $"{template.Name}_{nextIndex}.json";
        string filePath = Path.Combine(dir, fileName);

        string json = JsonUtility.ToJson(template, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Plantilla guardada (normalizada) como: {fileName}  (Name dentro del JSON = '{template.Name}')");
    }


    // -----------------------------
    // Normalización
    // -----------------------------

    private List<Vector2> Normalize(List<Vector2> points)
    {
        var resampled = Resample(points, 64);
        var rotated = RotateToZero(resampled);
        var scaled = ScaleToSquare(rotated, 200f);
        var translated = TranslateToOrigin(scaled);
        return translated;
    }

    private List<Vector2> Resample(List<Vector2> points, int n)
    {
        float pathLength = PathLength(points);
        float interval = pathLength / (n - 1);
        float D = 0f;
        List<Vector2> newPoints = new List<Vector2> { points[0] };

        for (int i = 1; i < points.Count; i++)
        {
            float d = Vector2.Distance(points[i - 1], points[i]);
            if ((D + d) >= interval)
            {
                float qx = points[i - 1].x + ((interval - D) / d) * (points[i].x - points[i - 1].x);
                float qy = points[i - 1].y + ((interval - D) / d) * (points[i].y - points[i - 1].y);
                Vector2 q = new Vector2(qx, qy);
                newPoints.Add(q);
                points.Insert(i, q);
                D = 0f;
            }
            else
            {
                D += d;
            }
        }

        if (newPoints.Count == n - 1)
            newPoints.Add(points[points.Count - 1]);

        return newPoints;
    }

    private float PathLength(List<Vector2> points)
    {
        float d = 0f;
        for (int i = 1; i < points.Count; i++)
            d += Vector2.Distance(points[i - 1], points[i]);
        return d;
    }

    private List<Vector2> RotateToZero(List<Vector2> points)
    {
        Vector2 c = Centroid(points);
        float theta = Mathf.Atan2(points[0].y - c.y, points[0].x - c.x);
        return RotateBy(points, -theta);
    }

    private List<Vector2> RotateBy(List<Vector2> points, float theta)
    {
        Vector2 c = Centroid(points);
        List<Vector2> newPoints = new List<Vector2>();
        foreach (var p in points)
        {
            float dx = p.x - c.x;
            float dy = p.y - c.y;
            float newX = dx * Mathf.Cos(theta) - dy * Mathf.Sin(theta) + c.x;
            float newY = dx * Mathf.Sin(theta) + dy * Mathf.Cos(theta) + c.y;
            newPoints.Add(new Vector2(newX, newY));
        }
        return newPoints;
    }

    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        Rect box = BoundingBox(points);
        List<Vector2> newPoints = new List<Vector2>();
        foreach (var p in points)
        {
            float qx = p.x * (size / box.width);
            float qy = p.y * (size / box.height);
            newPoints.Add(new Vector2(qx, qy));
        }
        return newPoints;
    }

    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 c = Centroid(points);
        List<Vector2> newPoints = new List<Vector2>();
        foreach (var p in points)
        {
            newPoints.Add(p - c);
        }
        return newPoints;
    }

    private Vector2 Centroid(List<Vector2> points)
    {
        float x = 0, y = 0;
        foreach (var p in points)
        {
            x += p.x;
            y += p.y;
        }
        return new Vector2(x / points.Count, y / points.Count);
    }

    private Rect BoundingBox(List<Vector2> points)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    // -----------------------------
    // Matching
    // -----------------------------
    private float GreedyCloudMatch(List<Vector2> points, List<Vector2> template)
    {
        float e = 0.5f;
        int step = Mathf.FloorToInt(Mathf.Pow(points.Count, 1f - e));
        float minDistance = float.MaxValue;
        for (int i = 0; i < points.Count; i += step)
        {
            float d1 = CloudDistance(points, template, i);
            float d2 = CloudDistance(template, points, i);
            minDistance = Mathf.Min(minDistance, Mathf.Min(d1, d2));
        }
        return minDistance;
    }

    private float CloudDistance(List<Vector2> pts1, List<Vector2> pts2, int start)
    {
        bool[] matched = new bool[pts2.Count];
        float sum = 0;
        int i = start;
        do
        {
            int index = -1;
            float minDist = float.MaxValue;
            for (int j = 0; j < pts2.Count; j++)
            {
                if (!matched[j])
                {
                    float d = Vector2.Distance(pts1[i], pts2[j]);
                    if (d < minDist)
                    {
                        minDist = d;
                        index = j;
                    }
                }
            }
            matched[index] = true;
            float weight = 1 - ((i - start + pts1.Count) % pts1.Count) / (float)pts1.Count;
            sum += weight * minDist;
            i = (i + 1) % pts1.Count;
        } while (i != start);
        return sum;
    }
}
