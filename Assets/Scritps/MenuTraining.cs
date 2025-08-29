using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuTraining : MonoBehaviour
{
    // Start is called before the first frame update
    public void Regresar()
    {
        //Regresar al Scene Juego
        SceneManager.LoadScene("Juego");
    }
    public void EntregarMenu()
    {
        SceneManager.LoadScene("TrainingScene");
    }

}
