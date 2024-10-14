using UnityEngine;

// Dit script gaat een cube toevoegen aan de scene en een half transparant material toepassen.
// Dit helpt bij het visualiseren van de randen waar de drone mag vliegen.
// Hoogte is instelbaar via scale van de boundaryPlane Y as.
// deze waarde heeft op de plane zelf geen effect maar wordt wel gebruikt om de boundaries te bepalen.
// Hou er rekening mee dat de scale values van een plane * 10 worden gedaan.
// scale 5 x 2 x 5 is dan een kubus van 50 x 20 x 50

public class DrawBoundaryCube : MonoBehaviour
{
    public Material cubeMaterial; // material van de cube
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 center = transform.position;
        Vector3 scale = transform.localScale;
        GameObject boundaryCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        BoxCollider collider = boundaryCube.GetComponent<BoxCollider>();
        // als er een collider aanwezig is op de cube, deze uitschakelen
        if (collider != null)
        {
            collider.enabled = false; // De collider is nu uitgeschakeld
        }
        // juiste positie en schaal toepassen
        boundaryCube.transform.position = center + new Vector3(0, scale.y * 10 / 2,0);
        boundaryCube.transform.localScale = scale * 10;
        if (cubeMaterial != null)
        {
            MeshRenderer meshRenderer = boundaryCube.GetComponent<MeshRenderer>();
            meshRenderer.material = cubeMaterial;
        }
    }
}
