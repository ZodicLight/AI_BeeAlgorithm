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

    public int maxScouts = 4;

    public List<GameObject> resourceObjects = new List<GameObject>();

    private float forageTimer;
    private float forageTime = 10.0f;



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
        if (scouts.Count < maxScouts)//!add our fittest drone How do you add fitness? Sor the drone base on fuel
        {

            scouts.Add(drones[0]);
            drones.Remove(drones[0]);

            scouts[scouts.Count - 1].GetComponent<Drone>().droneBehaviour = Drone.DroneBehaviours.Scouting;
        }

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
        }

       


    }
}

