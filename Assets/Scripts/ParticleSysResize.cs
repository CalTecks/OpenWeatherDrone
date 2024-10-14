using UnityEngine;

// Toepasbaar op particlesystems
// Dit gaat afhankelijk van de grootte van de ondergrond de particle systemen
// die te maken hebben met het weer aanpassen qua positie en grootte

public class ParticleSysResize : MonoBehaviour
{

    private GameObject boundaryPlane;
    private Vector3 boundaryCenter; // Om het middelpunt op te slaan
    private Vector3 boundaryScale; // Om de schaal op te slaan
    void Start()
    {
        boundaryPlane = GameObject.FindGameObjectWithTag("BoundaryPlane");
        if (boundaryPlane != null)
        {
            boundaryCenter = boundaryPlane.transform.position;
            boundaryScale = boundaryPlane.transform.localScale * 10;
        }

        transform.position = boundaryCenter + new Vector3(0,boundaryScale.y,0);

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = new Vector3(boundaryScale.x, 1.0f, boundaryScale.z);
    }
}