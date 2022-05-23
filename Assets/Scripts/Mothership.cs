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

    public int topAsteroidCount;

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




        if (scouts.Count < maxScouts)
        {
            //add timer here 
            //once asteroid list reach 10 then stop and restart every 120 sec
            //only assign the drone with most fuel 
            scouts.Add(drones[0]);
            drones.Remove(drones[0]);

            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Scouting;
        }

        //===============================================================================
        //Elite Forager 
        //wait till number of Asteroids discover 
        if (resourceObjects.Count >= 3)//the last three asteroid can be farm!!!
        {
            //Start elite droning 
            //(Re)Initialise elite Continuously
            if (eliteForagers.Count < maxElites)//!add our fittest drone How do you add fitness? Sor the drone base on fuel
            {
                Debug.Log("MotherShip Elite Forager method here");

                eliteForagers.Add(drones[0]);
                drones.Remove(drones[0]);

                //array of objects 
                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.EliteForaging;


                //topAsteroidCount starts at zero thus this gets the position of the highest resourced Asteroid
                Vector3 asteriodPos = resourceObjects[topAsteroidCount].transform.position;

                GameObject asteroidObject = resourceObjects[topAsteroidCount];

                //this two 
                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().MiningTarget = asteriodPos;
                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().miningAsteroid = asteroidObject;

                eliteForagers[eliteForagers.Count - 1].GetComponent<Drone>().isDroneFullFromMining = false;

                if (topAsteroidCount < 2)// 0, 1, 2?? it should be three drone?
                {
                    Debug.Log("topAsteroidCount: " + topAsteroidCount);
                    topAsteroidCount = topAsteroidCount + 1;
                }
            }
        }

        if (resourceObjects.Count > 0)
        {
            //Debug.Log("resourceObjects.Count: " + resourceObjects.Count); //5min test seems ok
            removeDestroyedAsteroids();
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

    }

    //Remove all asteroids from the list of found resourceObjects once they no longer have any resources left... as they will be destroyed.
    private void removeDestroyedAsteroids()
    {
        //ArgumentOutOfRangeException. Negative or less then the size of the collection

        for (int i = 0; i < resourceObjects.Count; i++) //I have to test it -1, important
        {
            if (resourceObjects[i].GetComponent<Asteroid>().resource <= 0)
            {
                resourceObjects.Remove(resourceObjects[i]);
                //Destroy(asteroid);
            }
        }

    }

    public void minustopAsteroidCount()
    {

            Debug.Log("topAsteroidCount: " + topAsteroidCount);//-343432
            topAsteroidCount = topAsteroidCount - 1;

    }

}

