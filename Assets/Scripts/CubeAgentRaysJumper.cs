using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgentRaysJumper : Agent
{
    //private Transform agentStartTrans;
    private bool isGrounded = true;
    private Rigidbody body;
    private bool isObstaclePassed = false;
    [SerializeField] public float jumpSpeed = 10.0f;

    public override void Initialize()
    {
        base.Initialize();

        //agentStartTrans = this.transform;
        body = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // plaats en beweging resetten
        //if (this.transform.localPosition.y < 0)
        //{
        //    this.transform.localPosition = agentStartTrans.localPosition;
        //    this.transform.localRotation = agentStartTrans.localRotation;
        //    body.velocity = new Vector3(0, 0, 0);
        //}
    }

    // Observeren van de omgeving
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target en Agent posities
        sensor.AddObservation(this.transform.localPosition); // Moet zijn eigen positie weten om te weten op welke positie het geraakt is
    }

    // Acties en Beloningen
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (actionBuffers.ContinuousActions[0] == 1.0f)
        {
            if (isGrounded)
            {
                body.velocity = new Vector3(0, jumpSpeed, 0);
                Debug.Log("<color=blue>Jump!</color>");
                isGrounded = false;
            }
        }

        if (isObstaclePassed)
        {
            //SetReward(1.0f);
            AddReward(1.0f);
            Debug.Log("<color=green>Succes!</color>");
            isObstaclePassed = false;
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision with {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Road"))
        {
            isGrounded = true;
        }
        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            SetReward(-1.0f);
            //AddReward(-1.0f);
            Debug.Log("<color=red>Failed!</color>");
            collision.gameObject.GetComponent<Obstacle>().Reset();
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Bonus"))
        {
            AddReward(0.5f);
            Debug.Log("<color=yellow>BONUS!</color>");
            collision.gameObject.GetComponent<Obstacle>().Reset();
        }
    }

    public void ObstacleHasPassed()
    {
        isObstaclePassed = true;
    }

    // Testen van de omgeving -> Behavior Parameters script -> Behavior Type
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetButton("Jump"))
        {
            continuousActionsOut[0] = 1.0f;
        }
    }

}
