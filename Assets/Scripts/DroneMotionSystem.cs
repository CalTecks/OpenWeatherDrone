using System;
using System.Collections;
using UnityEngine;

// Afhandelen van beweging van de drone, zowel auto pilot
// als handmatige bediening.
// enkele variabelen in inspector instelbaar voor ander resultaat te krijgen

public enum DirectionStatus
{
    Forward,
    Backward,
    Left,
    Right
}

public class DroneMotionSystem : MonoBehaviour
{
    private DirectionStatus directionStatus;
    private Rigidbody rb;
    private GameObject boundaryPlane;
    public float boundaryOffset = 5f;
    public float forceAmount = 30f;
    public float maxRotationAngle = 20f;
    private Vector3 boundaryCenter; // Om het middelpunt op te slaan
    private Vector3 boundaryScale; // Om de schaal op te slaan
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool startedChangeDirTimer = false;
    private bool startedEnableAutoFlyTimer = false;
    private bool autoFlyEnabled = false;
    private bool insideBounds = true;
    private Coroutine changeDirCoroutine;
    private Coroutine enableAutoFlyCoroutine;
    private System.Random random = new System.Random();
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boundaryPlane = GameObject.FindGameObjectWithTag("BoundaryPlane");
        if (boundaryPlane != null)
        {
            boundaryCenter = boundaryPlane.transform.position;
            boundaryScale = boundaryPlane.transform.localScale * 10;
            //Debug.Log("Boundary Center: " + boundaryCenter);
            //Debug.Log("Boundary Scale: " + boundaryScale);
            directionStatus = DirectionStatus.Forward;
        }
        // spawnen in center van boundaryPlane
        rb.transform.position = boundaryCenter + new Vector3(0, boundaryScale.y/ 2, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // horizontal en vertical wijzigbaar via edit -> project settings -> input manager
        // default ingesteld op onder andere pijltjestoetsen en WASD
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");
        // checken of er user input is
        if (inputHorizontal == 0 && inputVertical == 0)
        {
            if(!startedEnableAutoFlyTimer) {
                enableAutoFlyCoroutine = StartCoroutine(EnableAutoFly());
                startedEnableAutoFlyTimer = true;
            }
            // Auto Fly
            if(autoFlyEnabled) AutoFly();
            // Als timer van ChangeDirection nog niet gestart is -> start de coroutine (timer start)
            // Mag ook pas starten als autoFly enabled is, anders vliegt hij geen 3s maar 2s voor 1x
            // Bij overgang van manueel sturen naar auto dus belangrijk dat deze timer niet direct start
            if(!startedChangeDirTimer && autoFlyEnabled && insideBounds) {
                changeDirCoroutine = StartCoroutine(ChangeDirection());
                startedChangeDirTimer = true;
            }
        }
        else
        {
            // else: als er user input is...
            // Coroutines stopzetten indien ze lopen
            StopChangeDirCoroutine();
            StopAutoFlyCoroutine();            
            // Mogelijkheid om starten coroutines resetten
            startedEnableAutoFlyTimer = false;
            startedChangeDirTimer = false;
            autoFlyEnabled = false;
            // Manual Fly
            //Debug.Log("Manual Flying");
            Vector3 forceDirection = new Vector3(inputHorizontal, 0, inputVertical);
            rb.AddForce(forceDirection * forceAmount);
        }
        float xVelocity = rb.linearVelocity.x;  // links, rechts velocity
        float xRotationAngle = Mathf.Clamp(xVelocity, -maxRotationAngle, maxRotationAngle);
        float zVelocity = rb.linearVelocity.z; // voorwaarts,achterwaars velocity
        float zRotationAngle = Mathf.Clamp(zVelocity, -maxRotationAngle, maxRotationAngle);

        // Rotatie toepassen om beweging realistisch te maken, afhankelijk van velocity op rigidbody
        Quaternion targetRotation = Quaternion.Euler(zRotationAngle,0, -xRotationAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 50);
        // twee timers toevoegen, eentje om de 3 sec direction status te veranderen
        // tweede timer om na input gebruiker x tijd te wachten voor auto pilot begint
    }

    void StopAutoFlyCoroutine() {
        if (enableAutoFlyCoroutine != null) {
            StopCoroutine(enableAutoFlyCoroutine);
            enableAutoFlyCoroutine = null; // Reset reference
            startedEnableAutoFlyTimer = false;
        }
    }
    void StopChangeDirCoroutine() {
        if (changeDirCoroutine != null) {
            StopCoroutine(changeDirCoroutine);
            changeDirCoroutine = null; // Reset reference  
            startedChangeDirTimer = false; 
        } 
    }
    void AutoFly()
    {
        // checken of we op huidige locatie mogen vliegen
        CheckBoundaries();
        switch (directionStatus)
        {
            case DirectionStatus.Forward:
                rb.AddForce(new Vector3(0,0,1) * forceAmount);  // Vooruit op de Z-as
                break;
                
            case DirectionStatus.Backward:
                rb.AddForce(new Vector3(0,0,-1) * forceAmount); // Achteruit op de Z-as
                break;
                
            case DirectionStatus.Right:
                rb.AddForce(new Vector3(1,0,0) * forceAmount);  // Rechts op de X-as
                break;
                
            case DirectionStatus.Left:
                rb.AddForce(new Vector3(-1,0,0) * forceAmount); // Links op de X-as
                break;
        }
    }

    void CheckBoundaries() {
        float dronePositionZ = transform.position.z;
        float dronePositionX = transform.position.x;

        if (dronePositionZ > boundaryCenter.z + (boundaryScale.z * 0.5f - boundaryOffset))
        {
            directionStatus = DirectionStatus.Backward;
            //Debug.Log("forward overschreden");
            insideBounds = false;
            StopChangeDirCoroutine();
        }
        else if (dronePositionZ < boundaryCenter.z - (boundaryScale.z * 0.5f - boundaryOffset))
        {
            directionStatus = DirectionStatus.Forward; // Ga vooruit als we de grens overschrijden
            //Debug.Log("backward overschreden");
            insideBounds = false;
            StopChangeDirCoroutine();
        }
        else if (dronePositionX > boundaryCenter.x + (boundaryScale.x * 0.5f - boundaryOffset))
        {
            directionStatus = DirectionStatus.Left; // Ga links als we de grens overschrijden
            //Debug.Log("right overschreden");
            insideBounds = false;
            StopChangeDirCoroutine();
        }
        else if (dronePositionX < boundaryCenter.x - (boundaryScale.x * 0.5f - boundaryOffset))
        {
            directionStatus = DirectionStatus.Right; // Ga rechts als we de grens overschrijden
            //Debug.Log("left overschreden");
            insideBounds = false;
            StopChangeDirCoroutine();
        }
        else
        {
            insideBounds = true;
            // Debug.Log("inside bounds");
        }


    }
    IEnumerator EnableAutoFly()
    {
        //Debug.Log("EnableAutoFly timer started");

        // wachten voor x aantal seconden
        yield return new WaitForSeconds(1);

        // Code na wachttijd
        //Debug.Log("Autofly enabled");
        // net voor auto pilot gaat beginnen mag hij een nieuwe random direction krijgen
        ChangeDirNow();
        autoFlyEnabled = true;
    }
    IEnumerator ChangeDirection()
    {
        //Debug.Log("ChangeDirection timer started");

        // wachten voor x aantal seconden
        yield return new WaitForSeconds(3);

        // Code na wachttijd
        //Debug.Log("Change direction activated");
        ChangeDirNow();
        // Coroutine ChangeDirection timer start terug mogelijk maken
        startedChangeDirTimer = false;
    }
    void ChangeDirNow() {
        DirectionStatus oldStatus = directionStatus;
        // Krijg alle waarden van de DirectionStatus enum
        DirectionStatus[] directions = (DirectionStatus[])Enum.GetValues(typeof(DirectionStatus));

        // van richting veranderen tot deze niet meer gelijk is aan de vorige richting
        while (directionStatus == oldStatus){
        // Kies een willekeurige index van de enum waarden
        int randomIndex = random.Next(directions.Length);
        // Geef de willekeurige waarde terug
        directionStatus = directions[randomIndex];
        }
    }
}
