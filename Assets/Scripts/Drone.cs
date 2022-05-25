using UnityEngine;
using System.Collections;

public class Drone : Enemy {

    GameManager gameManager;

    Rigidbody rb;

    //Movement & Rotation Variables
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;//we suppose to use it??
    public float targetRadius = 200f;

    //Boid Steering/Flocking Variables
    public float separationDistance = 1.0f;//25.0f
    public float cohesionDistance = 2.0f;//50.0f
    public float separationStrength = 1.0f;//250.0f
    public float cohesionStrength = 1.0f;//25.0f+
    private Vector3 cohesionPos = new Vector3(0f, 0f, 0f);
    private int boidIndex = 0;


    //Drone FSM Enumerator
    public enum DroneBehaviours
    {
        Idle,
        Scouting,//W7
        Foraging,//WS7
        EliteForaging,
        Attacking,//WS8
        Fleeing//WS8
    }

    public DroneBehaviours droneBehaviour;

    //Drone Behaviour Variables
    public GameObject motherShip;
    public Vector3 scoutPosition;


    private float scoutTimer;
    private float detectTimer;
    private float scoutTime = 10.0f;
    private float detectTime = 5.0f;
    private float detectionRadius = 400.0f;
    private int newResourceVal;
    public GameObject newResourceObject;

    private Vector3 tarVel;
    private Vector3 tarPrevPos;
    private Vector3 attackPos;
    private float distanceRatio = 0.05f;

    //mining Variables 
    public int dronefuel;   // this is for Part 1 - fitness heuristic (fuel based)
    public int mineCapacity = 20; 


    public Vector3 MiningTarget;
    public bool isDroneFullFromMining = false;

    public GameObject miningAsteroid;//Asteroid object from Mothership Class



    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        rb = GetComponent<Rigidbody>();

        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;

        //Part 1 - give each drone a random amount of fuel.
        dronefuel = Random.Range(8000, 10000);


    }

    // Update is called once per frame
    void Update() {

        //Acquire player if spawned in
        if (gameManager.gameStarted)
        {
            target = gameManager.playerDreadnaught;
            droneBehaviour = DroneBehaviours.Attacking;
        }


        ////Move towards valid targets
        if (target)//comment this line for attack and prey to work 
            MoveTowardsTarget(target.transform.position);//comment this line for attack and prey to work 



        BoidBehaviour();

        switch (droneBehaviour)
        {
            // <-- where is Idle? Do we need to code that?

            case DroneBehaviours.Scouting:
                Scouting();
                break;

            case DroneBehaviours.EliteForaging:
                EliteForaging();
                break;

            case DroneBehaviours.Foraging:
                Foraging();
                break;
            case DroneBehaviours.Attacking:
             
                Attacking();
                break;
        }
    }


    private void BoidBehaviour()
    {
        //Debug.Log("BoidBehavior");
        //Increment boid index reference
        boidIndex++;

        //Check if last boid in Enemy list
        if (boidIndex >= gameManager.enemyList.Length)//why 19? 20?
        {
            //Re-Compute the cohesionForce
            Vector3 cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos, transform.position)) * (cohesionPos - transform.position);


            //Apply Force
            rb.AddForce(cohesiveForce);
            //Reset boidIndex
            boidIndex = 0;//only 19 or 20 reset to zero 
            //Reset cohesion position
            cohesionPos.Set(0f, 0f, 0f);
          
        }

        Debug.DrawLine(transform.position, gameManager.enemyList[boidIndex].transform.position);

            //Currently analysed boid variables
            Vector3 pos = gameManager.enemyList[boidIndex].transform.position;
            Quaternion rot = gameManager.enemyList[boidIndex].transform.rotation;
            float dist = Vector3.Distance(transform.position, pos);

            //If not this boid
            if (dist > 0f)
            {
                //If within separation
                if (dist <= separationDistance)
                {
                    //Compute scale of separation
                    float scale = separationStrength / dist;
                    //Apply force to ourselves
                    rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
                }
                else if (dist < cohesionDistance && dist > separationDistance)
                {

                    //Calculate the current cohesionPos
                    cohesionPos = cohesionPos + pos * (1f / (float)gameManager.enemyList.Length);
                    //Rotate slightly towards current boid
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
                }
            }
    }

    private void MoveTowardsTarget(Vector3 targetPos) {//this i sthe code to find target 
        
        //Rotate and move towards target if out of range
        if (Vector3.Distance(targetPos, transform.position) > targetRadius) {

            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
            dronefuel = dronefuel - 1;
        }
        else
        {

        }
    }


    //Drone FSM Behaviour - Scouting
    //***need update with fuel/cap 
    //***periodically perform a Global Search 

    private void Scouting()
    {
        //Debug.Log("Calling Scouting in Drone.cs");
        //If no new resource object found, than scout
        if (!newResourceObject)
        {
            //If close to scoutPosition, randomize new position to investigate within gamespace around mothership
            if (Vector3.Distance(transform.position, scoutPosition) < detectionRadius && Time.time > scoutTimer)
            {
                Vector3 position;
                position.x = motherShip.transform.position.x + Random.Range(-1500, 1500);
                position.y = motherShip.transform.position.y + Random.Range(-400, 400);
                position.z = motherShip.transform.position.z + Random.Range(-1500, 1500);

                scoutPosition = position;

                //Update scoutTimer
                scoutTimer = Time.time + scoutTime;

            }
            else
            {
                MoveTowardsTarget(scoutPosition);
                Debug.DrawLine(transform.position, scoutPosition, Color.yellow);
                //Every few seconds, check for new resources
            }

            //Every few seconds, check for new resources
            if (Time.time > detectTimer)
            {
                newResourceObject = DetectNewResources();
                detectTimer = Time.time + detectTime;
            }


        }
        //Resource found, head back to Mothership
        else
        {
            target = motherShip;
            Debug.DrawLine(transform.position, target.transform.position, Color.green);

            //In range of mothership, relay information and reset to drone again
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
            {
                //dronefuel = dronefuel + 5000; Can't base on frame must be distance 

                dronefuel = dronefuel + 10000;
                //Debug.Log("Add Scout");
                motherShip.GetComponent<Mothership>().drones.Add(this.gameObject);
                motherShip.GetComponent<Mothership>().scouts.Remove(this.gameObject);

                //Double check to see if the Mothership already knows about it and return it if not
                if (motherShip.GetComponent<Mothership>().resourceObjects.Contains(newResourceObject) == false)
                    {
                        motherShip.GetComponent<Mothership>().resourceObjects.Add(newResourceObject);
                    }

                newResourceVal = 0;
                newResourceObject = null;


                droneBehaviour = DroneBehaviours.Idle;//<-----


            }

        }

    }

    //Method used periodically by scouts/elite forages to check for new valid resources
    private GameObject DetectNewResources()
    {
        //Go through list of asteroids and ...
        for (int i = 0; i < gameManager.asteroids.Length; i++)
        {

            //... check if they are within detection distance
            if (Vector3.Distance(transform.position, gameManager.asteroids[i].transform.position) <= detectionRadius)
            {

                //Find the best one
                if (gameManager.asteroids[i].GetComponent<Asteroid>().resource > newResourceVal)
                {
                    newResourceObject = gameManager.asteroids[i];
                }
            }
        }

        //Double check to see if the Mothership already knows about it and return it if not
        if (motherShip.GetComponent<Mothership>().resourceObjects.Contains(newResourceObject))
        {
            return null;
        }
        else
            return newResourceObject;
    }

    //Drone FSM Behaviour - Attacking
    private void Attacking()
    {
        //Calculate target's velocity (without using RB)
        tarVel = (target.transform.position - tarPrevPos) / Time.deltaTime;
        tarPrevPos = target.transform.position;

        //Calculate intercept attack position (p = t + r * d * v)
        attackPos = target.transform.position + distanceRatio * Vector3.Distance(transform.position, target.transform.position) * tarVel;

        attackPos.y = attackPos.y + 10;
        Debug.DrawLine(transform.position, attackPos, Color.red);

        // Not in range of intercept vector - move into position
        if (Vector3.Distance(transform.position, attackPos) > targetRadius)
            MoveTowardsTarget(attackPos);
        else
        {
            //Look at target - Lerp Towards target
            targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            //Fire Weapons at target
            //...

            //if(Time.time > fireTimer)
            //{

            //}


        }
    }

    private void Foraging()
    {
        //normal foraging 
        //one patch two drones max?
        //three patches, each patch two drones max? 6 drones
        //mine resource 

        //if foragers continously return no new resources after number of attempts -> abandon the site -> removing it from resource list
        //send scout 4 max

        //===============================================
        //resourceObjects.Count from mothership

        if (miningAsteroid.GetComponent<Asteroid>().isDepleted == true)//Do I need to change miningAsteroid?
        {
            //Stop mining the resouce

            Debug.Log("Add Forage");
            motherShip.GetComponent<Mothership>().drones.Add(this.gameObject);
            motherShip.GetComponent<Mothership>().foragers.Remove(this.gameObject);

            droneBehaviour = DroneBehaviours.Idle;
            miningAsteroid.GetComponent<Asteroid>().isBeingForaged = false;
        }
        else
        {
            if (isDroneFullFromMining == false)
            {
                //Mine - Move towards Asteroid and mine

                //Debug.Log("MiningTarget" + MiningTarget);
                MiningResource(MiningTarget);
                Debug.DrawLine(transform.position, MiningTarget, Color.cyan);//on the way to Asteroid
            }
            else
            {
                //Return to mothership 
                MoveTowardsTarget(motherShip.transform.position);//on the way to MotherShip
                Debug.DrawLine(transform.position, motherShip.transform.position, Color.magenta);

                //In range of mothership, relay information and reset to drone again
                if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
                {
                    miningAsteroid.GetComponent<Asteroid>().minusResource(mineCapacity);//<-- the asteroid has delay death..

                    dronefuel = dronefuel + 10000;
                    isDroneFullFromMining = false;
                }
            }
        }

    }

    private void EliteForaging()
    {
        //Debug.Log("Calling EliteForaging in Drone.cs");
        //invesitigate the local area 
        //if found better resource -> scout 
        //mine resource 

        //if no elite foragers find new resources -> neighbourhood shrinking 

        //if foragers continously return no new resources after number of attempts -> abandon the site -> removing it from resource list
        //send scout 4 max


        if (miningAsteroid.GetComponent<Asteroid>().isDepleted == true)//Do I need to change miningAsteroid?
        {
            //topAsteroidCount in mothership should -1 
            //Debug.Log("Is Asteroid depleted: " + miningAsteroid.GetComponent<Asteroid>().isDepleted);
            //Debug.Log("Is Asteroid topAsteroidCount: " + motherShip.GetComponent<Mothership>().topAsteroidCount);

            //motherShip.GetComponent<Mothership>().minustopAsteroidCount();//decease the topAsteroidCount back down otherwise only one elite drone 
            Debug.Log("Add Elite");
            motherShip.GetComponent<Mothership>().drones.Add(this.gameObject);
            motherShip.GetComponent<Mothership>().eliteForagers.Remove(this.gameObject);

            droneBehaviour = DroneBehaviours.Idle;

            //miningAsteroid.GetComponent<Asteroid>().minusResource(mineCapacity);
        }
        else
        {
            if (isDroneFullFromMining == false)
            {
                //Debug.Log("MiningTarget" + MiningTarget);
                MiningResource(MiningTarget);
                Debug.DrawLine(transform.position, MiningTarget, Color.blue);//on the way to Asteroid
            }
            else
            {
                //return to mothership 
                MoveTowardsTarget(motherShip.transform.position);//on the way to MotherShip
                Debug.DrawLine(transform.position, motherShip.transform.position, Color.red);


                //how can I call a function just once here 

                //on return trip 
                //Debug.Log("Asteroid resource: " + miningAsteroid.GetComponent<Asteroid>().resource);
                //miningAsteroid.GetComponent<Asteroid>().minusResource(10);//take 10 resource from Asteroid
                
                //In range of mothership, relay information and reset to drone again
                if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
                {
                    //Debug.Log("Asteroid resource: " + miningAsteroid.GetComponent<Asteroid>().resource);
                   

                    miningAsteroid.GetComponent<Asteroid>().minusResource(mineCapacity);//<-- the asteroid has delay death..

                    dronefuel = dronefuel + 10000;
                    isDroneFullFromMining = false;
                }
            }
        }

        //droneBehaviour = DroneBehaviours.Idle;//<-----


    }

    private void MiningResource(Vector3 miningPos)
    {//this is the code to find target 

        //Rotate and move towards target if out of range
        if ((Vector3.Distance(miningPos, transform.position) > targetRadius) && !isDroneFullFromMining)
        {
            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(miningPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
            dronefuel = dronefuel - 1;
        }
        else
        {
            //this part was never called??
            isDroneFullFromMining = true;       
        }

    }
}
