using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(RectTransform))]
public class GestureRecognizer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private List<Vector2> points = new List<Vector2>();
    private DollarRecognizer recognizer;

    // Para dibujar la línea temporal
    private LineRenderer currentLine;
    public Material lineMaterial;   // Asignar un material simple (Default-Line) en el inspector
    public float lineWidth = 0.05f;

    private void Start()
    {
        recognizer = new DollarRecognizer();

        // 🔄 Cargar automáticamente todas las plantillas entrenadas
        int loaded = recognizer.LoadTemplatesFromDisk();
        Debug.Log($"Plantillas cargadas: {loaded} (total {recognizer.TemplateCount})"); // si usaste la Opción A

        // Ejemplo: plantillas mínimas (luego puedes entrenarlas mejor)
        /*
        recognizer.AddTemplate("A", new List<Vector2> { new Vector2(0, 0), new Vector2(50, 100), new Vector2(100, 0) });
        recognizer.AddTemplate("B", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(50, 75), new Vector2(0, 50), new Vector2(50, 25), new Vector2(0, 0) });
        recognizer.AddTemplate("C", new List<Vector2> { new Vector2(100, 100), new Vector2(50, 100), new Vector2(0, 50), new Vector2(50, 0), new Vector2(100, 0) });
        recognizer.AddTemplate("D", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(75, 75), new Vector2(75, 25), new Vector2(0, 0) });
        recognizer.AddTemplate("E", new List<Vector2> { new Vector2(100, 100), new Vector2(0, 100), new Vector2(0, 50), new Vector2(75, 50), new Vector2(0, 50), new Vector2(0, 0), new Vector2(100, 0) });
        recognizer.AddTemplate("F", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(75, 100), new Vector2(0, 100), new Vector2(0, 50), new Vector2(50, 50) });
        recognizer.AddTemplate("G", new List<Vector2> { new Vector2(100, 100), new Vector2(0, 100), new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 50), new Vector2(50, 50) });
        recognizer.AddTemplate("H", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(0, 50), new Vector2(100, 50), new Vector2(100, 100), new Vector2(100, 0) });
        recognizer.AddTemplate("I", new List<Vector2> { new Vector2(50, 0), new Vector2(50, 100) });
        recognizer.AddTemplate("J", new List<Vector2> { new Vector2(100, 100), new Vector2(50, 100), new Vector2(50, 0), new Vector2(0, 0) });
        recognizer.AddTemplate("K", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(0, 50), new Vector2(100, 100), new Vector2(0, 50), new Vector2(100, 0) });
        recognizer.AddTemplate("L", new List<Vector2> { new Vector2(0, 100), new Vector2(0, 0), new Vector2(100, 0) });
        recognizer.AddTemplate("M", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(50, 50), new Vector2(100, 100), new Vector2(100, 0) });
        recognizer.AddTemplate("N", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(100, 0), new Vector2(100, 100) });
        recognizer.AddTemplate("O", new List<Vector2> { new Vector2(50, 0), new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 50), new Vector2(50, 0) });
        recognizer.AddTemplate("P", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(50, 100), new Vector2(50, 50), new Vector2(0, 50) });
        recognizer.AddTemplate("Q", new List<Vector2> { new Vector2(50, 0), new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 50), new Vector2(50, 0), new Vector2(75, 25), new Vector2(100, 0) });
        recognizer.AddTemplate("R", new List<Vector2> { new Vector2(0, 0), new Vector2(0, 100), new Vector2(50, 100), new Vector2(50, 50), new Vector2(0, 50), new Vector2(100, 0) });
        recognizer.AddTemplate("S", new List<Vector2> { new Vector2(100, 100), new Vector2(0, 100), new Vector2(0, 50), new Vector2(100, 50), new Vector2(100, 0), new Vector2(0, 0) });
        recognizer.AddTemplate("T", new List<Vector2> { new Vector2(0, 100), new Vector2(100, 100), new Vector2(50, 100), new Vector2(50, 0) });
        recognizer.AddTemplate("U", new List<Vector2> { new Vector2(0, 100), new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100) });
        recognizer.AddTemplate("V", new List<Vector2> { new Vector2(0, 100), new Vector2(50, 0), new Vector2(100, 100) });
        recognizer.AddTemplate("W", new List<Vector2> { new Vector2(0, 100), new Vector2(25, 0), new Vector2(50, 100), new Vector2(75, 0), new Vector2(100, 100) });
        recognizer.AddTemplate("X", new List<Vector2> { new Vector2(0, 0), new Vector2(100, 100), new Vector2(50, 50), new Vector2(0, 100), new Vector2(100, 0) });
        recognizer.AddTemplate("Y", new List<Vector2> { new Vector2(0, 100), new Vector2(50, 50), new Vector2(100, 100), new Vector2(50, 50), new Vector2(50, 0) });
        recognizer.AddTemplate("Z", new List<Vector2> { new Vector2(0, 100), new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 0) });
        */
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        points.Clear();
        StartNewLine();
    }

    public void OnDrag(PointerEventData eventData)
    {
        points.Add(eventData.position);
        AddPointToLine(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (points.Count < 5)
        {
            ClearLine();
            return;
        }

        string recognized = recognizer.Recognize(points);
        Debug.Log("Reconocido: " + recognized);

        GameManager.Instance.OnPlayerGesture(recognized);

        ClearLine(); // borrar trazo después de reconocer
    }

    // ---------------- Dibujado ----------------

    private void StartNewLine()
    {
        GameObject lineObj = new GameObject("DrawLine");
        lineObj.transform.SetParent(null); // no depende del canvas
        currentLine = lineObj.AddComponent<LineRenderer>();
        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.numCapVertices = 5;
        currentLine.useWorldSpace = true;
    }

    private void AddPointToLine(Vector2 screenPos)
    {
        if (currentLine == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, worldPos);
    }

    private void ClearLine()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
    }
    
}
