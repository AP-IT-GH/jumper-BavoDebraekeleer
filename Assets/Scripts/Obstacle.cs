using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float obstacleSpeed = 0.0f;
    [SerializeField] private int minSpeed = 2;
    [SerializeField] private int maxSpeed = 5;
    [SerializeField] private float obstacleSpeedMultiplier = 1.0f;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        SetRandomSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += obstacleSpeed * Time.deltaTime * transform.forward;
    }

    public void Reset()
    {
        transform.position = startPos;
        SetRandomSpeed();
    }

    private void SetRandomSpeed()
    {
        obstacleSpeed = Random.Range(minSpeed, maxSpeed + 1) * obstacleSpeedMultiplier;
        Debug.Log($"obstacleSpeed={obstacleSpeed}");
    }
}
