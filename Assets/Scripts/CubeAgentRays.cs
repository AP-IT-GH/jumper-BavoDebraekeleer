using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgentRays : Agent
{
    // Initialisatie en resetten van de agent
    [SerializeField] public GameObject Target;
    [SerializeField] public GameObject GreenZone;


    public override void OnEpisodeBegin()
    {
        targetFound = false;
        Target.SetActive(true);

        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {

            this.transform.localPosition = new Vector3(0, 0.5f, 0); this.transform.localRotation = Quaternion.identity;
        }

        if (Target == null) Target = GameObject.Find("Target");
        if (GreenZone == null) Target = GameObject.Find("GreenZone");

        // verplaats de target naar een nieuwe willekeurige locatie 
        Target.transform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    // Observeren van de omgeving
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target en Agent posities
        //sensor.AddObservation(Target.transform.localPosition);
        sensor.AddObservation(this.transform.localPosition); // Moet zijn eigen positie weten om te weten op welke positie het platform eindigt -> door vallen

    }

    // Acties en Beloningen
    [SerializeField] public float speedMultiplier = 0.1f;
    [SerializeField] public float rotationMultiplier = 5;
    [SerializeField] public float fallPosition = 0.0f;
    public bool targetFound = false;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];   // Vooruit/achteruit
        transform.Translate(controlSignal * speedMultiplier);

        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);  // Roteren rond y-as

        // Beloningen

        // target bereikt
        if (!targetFound)
        {
            float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
            if (distanceToTarget < 1.5f)
            {
                Debug.Log("<color=yellow>Target reached.</color>");
                SetReward(1.0f);
                Target.SetActive(false);
                targetFound = true;
            }
        }
        else
        {
            float distanceToGreenZone = Vector3.Distance(this.transform.localPosition, GreenZone.transform.localPosition);
            if (distanceToGreenZone < 1)
            {
                Debug.Log("<color=green>Green Zone entered.</color>");
                SetReward(1.0f);
                EndEpisode();
            }
        }

        // Van het platform gevallen?
        if (this.transform.localPosition.y < fallPosition)
        {
            Debug.Log("<color=red>Fallen.</color>");
            //AddReward(-1.0f);
            EndEpisode();
        }
    }

    // Testen van de omgeving -> Behavior Parameters script -> Behavior Type
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

}
