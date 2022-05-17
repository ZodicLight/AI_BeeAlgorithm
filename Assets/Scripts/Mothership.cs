using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Mothership : MonoBehaviour {

    public GameObject enemy;
    public int numberOfEnemies = 20;

    public GameObject spawnLocation;

    //Resource Harvesting Variables
    public List<GameObject> drones = new List<GameObject>();
    public List<GameObject> scouts = new List<GameObject>();
    public List<GameObject> eliteForagers = new List<GameObject>();//elite foragers 
    public List<GameObject> foragers = new List<GameObject>();

    //20 drones in total
    public int maxScouts = 4;
    public int maxElites = 4;
    public int maxForagers = 6;//the rest is just foragers?

    public List<GameObject> resourceObjects = new List<GameObject>();

    private float forageTimer;
    private float forageTime = 10.0f;


    private int numOfEliteForages = 0;
    private int numOfNormalForages = 0;

    private int totalMineCollected;
    private int targetMineGoal = 1000;



    // initialise the boids
    void Start() {

        for (int i = 0; i < numberOfEnemies; i++) {

            Vector3 spawnPosition = spawnLocation.transform.position;

            spawnPosition.x = spawnPosition.x + Random.Range(-50, 50);
            spawnPosition.y = spawnPosition.y + Random.Range(-50, 50);
            spawnPosition.z = spawnPosition.z + Random.Range(-50, 50);

            //Instantiate(enemy, spawnPosition, spawnLocation.transform.rotation);//is this corret? Replace

            GameObject thisEnemy = Instantiate(enemy, spawnPosition, spawnLocation.transform.rotation) as GameObject;//what is this mean?
            drones.Add(thisEnemy);
        }
    }

    // Update is called once per frame
    void Update() {

        //(Re)Initialise Scouts Continuously
        //!add our fittest drone How do you add fitness? Sor the drone base on fuel
        if (scouts.Count < maxScouts)
        {

            scouts.Add(drones[0]);
            drones.Remove(drones[0]);

            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Scouting;
        }

        ////(Re)Initialise elite Continuously
        //if (eliteForagers.Count < maxElites)//!add our fittest drone How do you add fitness? Sor the drone base on fuel
        //{

        //    eliteForagers.Add(drones[0]);
        //    drones.Remove(drones[0]);

        //    //array of objects 
        //    eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.EliteForaging;
        //    //Debug.Log("Calling EliteForaging in Mothership.cs");
        //}


        //*** (Re)Determine best resource objects periodically 
        if (resourceObjects.Count > 0 && Time.time > forageTimer)//every ten second?
        {

            //Sort resource objects delegated by their resource amount in decreasing order
            resourceObjects.Sort(delegate (GameObject a, GameObject b) {
                return (b.GetComponent<Asteroid>().resource).CompareTo(a.GetComponent<Asteroid>().resource);
            });

            drones.Sort(delegate (GameObject a, GameObject b)//?is this good the good area to check this?
            {
                return (b.GetComponent<Drone>().dronefuel).CompareTo(a.GetComponent<Drone>().dronefuel);
            });

           forageTimer = Time.time + forageTime;

           //send scout out 
        }



        //=========================================================================
        //=========================================================================


        //Set the best drones (the ones with the most fuel) to be Elite forages
        if (drones.Count > 1 && numOfEliteForages < 2)
        {

            // **** TO FIX: move these next four lines down to below...
            eliteForagers.Add(drones[0]);
            drones.Remove(drones[0]);

            numOfEliteForages++;
            eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;

            //designate this elite forager to the next best resource that has been found that is not yet being foraged
            bool assignedForFrame = false;
            foreach (GameObject astroid in resourceObjects)
            {
                if (!assignedForFrame)
                {
                    if (!astroid.GetComponent<Asteroid>().isBeingForaged)      // ****  <----- OR other wise comment out this if statement, ignoring if it is already being mined, and allow multiple drones to mine it.
                    {
                        eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().target = astroid;
                        astroid.GetComponent<Asteroid>().isBeingForaged = true;
                        assignedForFrame = true;
                        // **** ... To HERE - I think and the same for the next section of code below
                        // This would mean the Drone would get assinged when there is a new resource....
                        // Or we need to change the code so we ignore if it isBeingForaged
                    }
                }
            }
        }
        //and set the next 3 drones to normal Normal forages
        else if (numOfEliteForages == 2 && drones.Count > 1 && numOfNormalForages < 3)
        {

            // **** REPEAT the same steps for this function
            foragers.Add(drones[0]);
            drones.Remove(drones[0]);

            numOfNormalForages++;
            foragers[foragers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;

            //designate normal forager to the 3 best patches, and normal foragers to the next 3 best asteroids
            bool assignedForFrame = false;
            foreach (GameObject astroid in resourceObjects)
            {
                if (!assignedForFrame)
                {
                    if (!astroid.GetComponent<Asteroid>().isBeingForaged)
                    {
                        foragers[foragers.Count - 1].GetComponent<Drone>().target = astroid;
                        astroid.GetComponent<Asteroid>().isBeingForaged = true;
                    }
                }
            }
        }



    }
}

