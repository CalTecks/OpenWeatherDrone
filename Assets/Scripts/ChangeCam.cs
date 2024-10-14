using UnityEngine;
using UnityEngine.UI;

// Verschillende camera's toevoegen om met de camera switch button te werken
// Automatisch, zodat je zoveel camera's kan toevoegen als gewenst
// Volgorde afhankelijk van toevoegen in inspector (cameras array)

public class ChangeCam : MonoBehaviour
{
    public Camera[] cameras;
    public Button buttonCam;
    private int camIndexActive = 0;
    void Start()
    {
        // Zorg dat alleen de eerste camera actief is
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == camIndexActive);
        }

        // Koppel de functie aan de button
        if (buttonCam != null)
        {
            buttonCam.onClick.AddListener(SwitchCamera);
        }
    }

    void SwitchCamera()
    {
        // Deactiveer de huidige camera
        cameras[camIndexActive].gameObject.SetActive(false);

        // Verhoog de index en wrap het om
        camIndexActive = (camIndexActive + 1) % cameras.Length;

        // Activeer de nieuwe huidige camera
        cameras[camIndexActive].gameObject.SetActive(true);
    }
}
