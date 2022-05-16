using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject Agent;
    private GameObject Obstacle;
    private GameObject Road;

    private Vector3 obstacleStartPos;

    [SerializeField] private bool isWithBonus = false;
    private GameObject Bonus;
    private Vector3 bonusStartPos;

    // Start is called before the first frame update
    void Start()
    {
        Agent = GameObject.FindGameObjectWithTag("Agent");
        Obstacle = GameObject.FindGameObjectWithTag("Obstacle");
        Road = GameObject.FindGameObjectWithTag("Road");

        obstacleStartPos = Obstacle.transform.localPosition;

        if (isWithBonus)
        {
            Bonus = GameObject.FindGameObjectWithTag("Bonus");
            bonusStartPos = Bonus.transform.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Obstacle.transform.localPosition.z < obstacleStartPos.z - 18.0f)
        {
            Agent.GetComponent<CubeAgentRaysJumper>().ObstacleHasPassed();
            Obstacle.GetComponent<Obstacle>().Reset();
            Debug.Log("Obstacle passed.");
        }

        if (isWithBonus)
        {
            if (Bonus.transform.localPosition.z < bonusStartPos.z - 18.0f)
            {
                Bonus.GetComponent<Obstacle>().Reset();
                Debug.Log("Bonus passed.");
            }
        }
    }
}
