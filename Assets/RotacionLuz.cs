using UnityEngine;

public class RotacionLuz : MonoBehaviour
{
    public float rotationSpeed = 45f; // grados por segundo
    private Quaternion rotA;
    private Quaternion rotB;
    private Quaternion targetRotation;

    void Start()
    {
        // Rotaciones objetivo
        rotA = Quaternion.Euler(0, 0, -45);
        rotB = Quaternion.Euler(0, 0, -120);

        // Iniciar en -45
        transform.rotation = rotA;
        targetRotation = rotB;

        // Iniciar bucle
        StartCoroutine(RotateLoop());
    }

    private System.Collections.IEnumerator RotateLoop()
    {
        while (true)
        {
            // Rotar hacia el objetivo
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Asegurar que quede exacto
            transform.rotation = targetRotation;

            // Cambiar el objetivo al opuesto
            targetRotation = (targetRotation == rotA) ? rotB : rotA;

            // Espera opcional antes de volver
            // yield return new WaitForSeconds(0.5f);
        }
    }
}
