using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class GestureTrainer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References (asignar en Inspector)")]
    public RawImage drawArea;            // RawImage donde se pinta (UI)
    public InputField inputGestureName;  // Nombre base (por ejemplo "A")
    public Button btnSave;
    public Button btnClear;
    public TextMeshProUGUI statusText;              // (opcional) para mostrar mensajes

    [Header("Texture Settings")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color backgroundColor = Color.white;
    public Color drawColor = Color.black;
    public int brushRadius = 4;
    public bool savePngAlongJson = true;

    // Internals
    private Texture2D texture;
    private RectTransform drawRect;
    private List<Vector2> points = new List<Vector2>(); // puntos en coordenadas de textura (0..width-1,0..height-1)
    private DollarRecognizer recognizer;

    
    private void Start()
    {
        inputGestureName.text = "A";
        TrainingSession.CurrentLetter = inputGestureName.text.ToUpper().Trim();
        // Después de guardar el archivo .png
        FindObjectOfType<GestureGallery>()?.LoadGallery();
    }
    void Awake()
    {
        // Seguridad: campos obligatorios
        if (drawArea == null) Debug.LogError("GestureTrainer: asigna DrawArea (RawImage) en el inspector.");
        if (inputGestureName == null) Debug.LogError("GestureTrainer: asigna InputField en el inspector.");
        if (btnSave == null) Debug.LogError("GestureTrainer: asigna Save Button en el inspector.");
        if (btnClear == null) Debug.LogError("GestureTrainer: asigna Clear Button en el inspector.");

        // crear texture y asignarla
        drawRect = drawArea.GetComponent<RectTransform>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture();
        drawArea.texture = texture;

        // hooks botones
        btnSave.onClick.AddListener(OnSaveClicked);
        btnClear.onClick.AddListener(OnClearClicked);

        // recognizer local (usamos la clase DollarRecognizer)
        recognizer = new DollarRecognizer();

        // asegura folder Gestures existe (opcional)
        string dir = Path.Combine(Application.persistentDataPath, "Gestures");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    // ---- pointer events sobre el RawImage ----
    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo procesar si el click/touch fue dentro del drawArea
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(drawRect, eventData.position, eventData.pressEventCamera, out local)) return;

        points.Clear();
        AddPointAndDraw(local);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(drawRect, eventData.position, eventData.pressEventCamera, out local)) return;

        AddPointAndDraw(local);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // nada extra por ahora
    }

    // ---- dibujo en la textura ----
    private void AddPointAndDraw(Vector2 localPoint)
    {
        // localPoint está en coordenadas del rect (-w/2..+w/2, -h/2..+h/2)
        float rectW = drawRect.rect.width;
        float rectH = drawRect.rect.height;

        // convertir a coordenadas de textura (0..textureWidth-1, 0..textureHeight-1)
        float u = (localPoint.x + rectW * 0.5f) / rectW;
        float v = (localPoint.y + rectH * 0.5f) / rectH;

        int px = Mathf.Clamp(Mathf.RoundToInt(u * (textureWidth - 1)), 0, textureWidth - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (textureHeight - 1)), 0, textureHeight - 1);

        Vector2 texPoint = new Vector2(px, py);

        // evitar puntos repetidos muy cercanos
        if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], texPoint) > 1f)
        {
            points.Add(texPoint);
        }

        // dibujar círculo en la textura
        DrawBrush(px, py);
        texture.Apply();
    }

    private void DrawBrush(int cx, int cy)
    {
        int r = Mathf.Max(1, brushRadius);
        int x0 = Mathf.Clamp(cx - r, 0, textureWidth - 1);
        int x1 = Mathf.Clamp(cx + r, 0, textureWidth - 1);
        int y0 = Mathf.Clamp(cy - r, 0, textureHeight - 1);
        int y1 = Mathf.Clamp(cy + r, 0, textureHeight - 1);

        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                // círculo simple
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                {
                    texture.SetPixel(x, y, drawColor);
                }
            }
        }
    }

    // ---- acciones UI ----
    private void OnSaveClicked()
    {
        string baseName = inputGestureName.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(baseName))
        {
            SetStatus("Escribe la letra/nombre antes de guardar.");
            return;
        }

        if (points == null || points.Count < 6)
        {
            SetStatus("Dibuja mas claramente antes de guardar (min 6 puntos).");
            return;
        }

        // Para consistencia, convertimos los puntos guardados (pixel coords) a Vector2 (float)
        // y los pasamos al recognizer (que internamente normaliza y guarda JSON)
        List<Vector2> gesturePoints = new List<Vector2>(points.Count);
        foreach (var p in points) gesturePoints.Add(new Vector2(p.x, p.y));

        // Determinar índice siguiente para guardar PNG con nombre consistente.
        string dir = Path.Combine(Application.persistentDataPath, "Gestures");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string[] existing = Directory.GetFiles(dir, baseName + "*.json");
        int nextIndex = existing.Length + 1;

        // Llamamos al recognizer (esto normaliza, añade a memoria y guarda JSON).
        recognizer.AddTemplate(baseName, gesturePoints);

        // Guardar PNG de referencia (opcional)
        if (savePngAlongJson)
        {
            string pngName = $"{baseName}_{nextIndex}.png";
            string pngPath = Path.Combine(dir, pngName);
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(pngPath, png);
            Debug.Log($"PNG guardado: {pngPath}");
        }

        SetStatus($"Plantilla '{baseName}_{nextIndex}' guardada.");
        // opcional: limpiar el canvas después de guardar
        ClearDrawing();
        // Después de guardar el archivo .png
        FindObjectOfType<GestureGallery>()?.LoadGallery();
    }

    private void OnClearClicked()
    {
        ClearDrawing();
        SetStatus("Dibujo limpiado.");
    }

    private void ClearDrawing()
    {
        points.Clear();
        ClearTexture();
        drawArea.texture = texture; // reasignar por si acaso
    }

    // ---- utilidades ----
    private void ClearTexture()
    {
        Color32[] fill = new Color32[textureWidth * textureHeight];
        Color32 bg = backgroundColor;
        for (int i = 0; i < fill.Length; i++) fill[i] = bg;
        texture.SetPixels32(fill);
        texture.Apply();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    // opción: listar archivos guardados (útil para debug)
    public string[] ListSavedFiles()
    {
        string dir = Path.Combine(Application.persistentDataPath, "Gestures");
        if (!Directory.Exists(dir)) return new string[0];
        return Directory.GetFiles(dir, "*.json");
    }
}
