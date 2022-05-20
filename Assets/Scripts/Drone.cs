﻿using UnityEngine;
using System.Collections;

public class Drone : Enemy {

    GameManager gameManager;

    Rigidbody rb;

    //Movement & Rotation Variables
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
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

    public int dronefuel;
    public int resourceCarry;

    public Vector3 MiningTarget;
    public bool isDroneFullFromMining = false;

    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        rb = GetComponent<Rigidbody>();

        motherShip = gameManager.alienMothership;
        scoutPosition = motherShip.transform.position;

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
          

        //Move towards valid targets
        if(target)
            MoveTowardsTarget(target.transform.position);

        BoidBehaviour();

        switch (droneBehaviour)
        {

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
             
                Scouting();
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
        }
        else
        {
            //target = motherShip;
            //Debug.DrawLine(transform.position, target.transform.position, Color.green);

            //get asteriod resource?
            //resourceObject[]
        }

        dronefuel = dronefuel - 1;

    }


    //Drone FSM Behaviour - Scouting
    //***need update with fuel/cap 
    //***periodically perform a Global Search 

    private void Scouting()
    {
        //Debug.Log("Calling Scouting in Drone.cs");
        //If no new resource object found
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

                motherShip.GetComponent<Mothership>().drones.Add(this.gameObject);
                motherShip.GetComponent<Mothership>().scouts.Remove(this.gameObject);

                motherShip.GetComponent<Mothership>().resourceObjects.Add(newResourceObject);

                newResourceVal = 0;
                newResourceObject = null;


                droneBehaviour = DroneBehaviours.Idle;

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


    }

    private void EliteForaging()
    {
        Vector3 MiningAsteroidPos;

        if (isDroneFullFromMining == false)
        {
            MiningResource(MiningTarget);
            Debug.DrawLine(transform.position, MiningTarget, Color.blue);
        }
        else
        {
            //return to mothership 
            MoveTowardsTarget(motherShip.transform.position);//MotherShip
            Debug.DrawLine(transform.position, motherShip.transform.position, Color.red);
            //In range of mothership, relay information and reset to drone again
            if (Vector3.Distance(transform.position, motherShip.transform.position) < targetRadius)
            {
                isDroneFullFromMining = false;
            }
        }



        //Debug.Log("Calling EliteForaging in Drone.cs");
        //elite foraging
        //only the top two tier drone selected
        //two patches, each patch two drones max? 4 drones 

        //invesitigate the local area 
        //if found better resource -> scout 
        //mine resource 
        //return to mothership await instruction??

        //if no elite foragers find new resources -> neighbourhood shrinking 

        //if foragers continously return no new resources after number of attempts -> abandon the site -> removing it from resource list
        //send scout 4 max

    }

    private void MiningResource(Vector3 miningPos)
    {//this i sthe code to find target 

        //Rotate and move towards target if out of range
        if ((Vector3.Distance(miningPos, transform.position) > targetRadius) && !isDroneFullFromMining)
        {
            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(miningPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
        }
        else
        {
            //this part was never called
            isDroneFullFromMining = true;
            
            
            //MoveTowardsTarget(target.transform.position);
            //Debug.DrawLine(transform.position, target.transform.position, Color.red);
            //get asteriod resource?
            //resourceObject[]
        }

        dronefuel = dronefuel - 1;

    }





}
