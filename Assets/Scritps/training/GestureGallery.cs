using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GestureGallery : MonoBehaviour
{
    public GameObject imagePrefab;   // Prefab con un Image
    public Transform gridParent;     // El GridLayoutGroup
    public InputField letterInput;   // El InputField donde escribes la letra a entrenar
    public string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "Gestures");
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        LoadGallery(); // ✅ carga al entrar
    }

    public void LoadGallery()
    {
        // limpiar grilla anterior
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        if (letterInput == null || string.IsNullOrEmpty(letterInput.text))
            return;

        string letter = letterInput.text.ToUpper();
        string[] files = Directory.GetFiles(savePath, letter + "_*.png");

        foreach (string file in files)
        {
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            GameObject imgObj = Instantiate(imagePrefab, gridParent);
            imgObj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }
    }
}
