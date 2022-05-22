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

    public List<GameObject> resourceObjects = new List<GameObject>();

    //public GameObject asteroidObject; 

    //20 drones in total
    private int maxScouts = 4;
    private int maxElites = 2;
    private int maxForagers = 6;//the rest is just foragers?
   

    private float forageTimer;
    private float forageTime = 10.0f;


    private int numOfEliteForages = 0;
    private int numOfNormalForages = 0;

    private int totalMineCollected;
    private int targetMineGoal = 1000;

    private int topAsteroidCount = 0;

    //public gameObject asteroidDeath; 


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

        removeDestroyedAsteroids();


        if (scouts.Count < maxScouts)
        {

            scouts.Add(drones[0]);
            drones.Remove(drones[0]);

            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Scouting;
        }

        //===============================================================================

        //wait till number of Asteroids discover 
        if (resourceObjects.Count >= 3)
        {
            //Start elite droning 
            //(Re)Initialise elite Continuously
            if (eliteForagers.Count < maxElites)//!add our fittest drone How do you add fitness? Sor the drone base on fuel
            {
                
                eliteForagers.Add(drones[0]);
                drones.Remove(drones[0]);

                //array of objects 
                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.EliteForaging;
                //eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().tempTarget = resourceObjects[0].transform.position;


                //topAsteroidCount starts at zero thus this gets the position of the highest resourced Asteroid
                Vector3 asteriodPos = resourceObjects[topAsteroidCount].transform.position;

                GameObject asteroidObject = resourceObjects[topAsteroidCount];

                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().MiningTarget = asteriodPos;
                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().miningAsteroid = asteroidObject;

                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().isDroneFullFromMining = false;
                
                if(topAsteroidCount <= 2)//tested Passed 
                {
                    topAsteroidCount = topAsteroidCount + 1;
                }
            }
        }
        


        //*** (Re)Determine best resource objects periodically 
        if (resourceObjects.Count > 0 && Time.time > forageTimer)//every ten second?
        {

            //Sort resource objects delegated by their resource amount in decreasing order
            resourceObjects.Sort(delegate (GameObject a, GameObject b) {
                return (b.GetComponent<Asteroid>().resource).CompareTo(a.GetComponent<Asteroid>().resource);
            });

            drones.Sort(delegate (GameObject a, GameObject b)//***is this good the good area to check this?
            {
                return (b.GetComponent<Drone>().dronefuel).CompareTo(a.GetComponent<Drone>().dronefuel);
            });

           forageTimer = Time.time + forageTime;

           //send scout out 
        }



        //=========================================================================
        //=========================================================================


        ////Set the best drones (the ones with the most fuel) to be Elite forages
        //if (drones.Count > 1 && numOfEliteForages < 2)
        //{

        //    // **** TO FIX: move these next four lines down to below...
        //    eliteForagers.Add(drones[0]);
        //    drones.Remove(drones[0]);

        //    numOfEliteForages++;
        //    eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;

        //    //designate this elite forager to the next best resource that has been found that is not yet being foraged
        //    bool assignedForFrame = false;
        //    foreach (GameObject astroid in resourceObjects)
        //    {
        //        if (!assignedForFrame)
        //        {
        //            if (!astroid.GetComponent<Asteroid>().isBeingForaged)      // ****  <----- OR other wise comment out this if statement, ignoring if it is already being mined, and allow multiple drones to mine it.
        //            {
        //                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().target = astroid;
        //                astroid.GetComponent<Asteroid>().isBeingForaged = true;
        //                assignedForFrame = true;
        //                // **** ... To HERE - I think and the same for the next section of code below
        //                // This would mean the Drone would get assinged when there is a new resource....
        //                // Or we need to change the code so we ignore if it isBeingForaged
        //            }
        //        }
        //    }
        //}
        ////and set the next 3 drones to normal Normal forages
        //else if (numOfEliteForages == 2 && drones.Count > 1 && numOfNormalForages < 3)
        //{

        //    // **** REPEAT the same steps for this function
        //    foragers.Add(drones[0]);
        //    drones.Remove(drones[0]);

        //    numOfNormalForages++;
        //    foragers[foragers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Foraging;

        //    //designate normal forager to the 3 best patches, and normal foragers to the next 3 best asteroids
        //    bool assignedForFrame = false;
        //    foreach (GameObject astroid in resourceObjects)
        //    {
        //        if (!assignedForFrame)
        //        {
        //            if (!astroid.GetComponent<Asteroid>().isBeingForaged)
        //            {
        //                foragers[foragers.Count - 1].GetComponent<Drone>().target = astroid;
        //                astroid.GetComponent<Asteroid>().isBeingForaged = true;
        //            }
        //        }
        //    }
        //}



    }


    //Remove all asteroids from the list of found resourceObjects once they no longer have any resources left... as they will be destroyed.
    private void removeDestroyedAsteroids()
    {
        //List<GameObject> tempListToRemove = new List<GameObject>();

        //foreach (GameObject asteroid in resourceObjects)
        //{
        //    if (asteroid.GetComponent<Asteroid>().resource <= 0)
        //    {
        //        tempListToRemove.Add(asteroid);
        //        //Destroy(asteroid);
        //    }
        //}

        //foreach (GameObject asteroid in tempListToRemove)
        //{
        //    resourceObjects.Remove(asteroid);
        //}

        for (int i = 0; i < resourceObjects.Count-1; i++) 
        {
            if (resourceObjects[i].GetComponent<Asteroid>().resource <= 0)
            {
                resourceObjects.Remove(resourceObjects[i]);
                //Destroy(asteroid);
            }
        }
    }

}

