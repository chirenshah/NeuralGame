using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class playerArea : MonoBehaviour
{
    [Tooltip("The agent inside the area")]
    public playerAgent playerAgent;

    [Tooltip("The TextMeshPro text that shows the cumulative reward of the agent")]
    public TextMeshPro cumulativeRewardText;

    /// <summary>
    /// Reset the area, including Car placement
    /// </summary>
    public void ResetArea()
    {
        SpawnCar();
        //playerAgent.SetReward(0.0f);

    }
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = minRadius;
        float angle = minAngle;

        if (maxRadius > minRadius)
        {
            // Pick a random radius
            radius = Random.Range(minRadius, maxRadius);
        }

        if (maxAngle > minAngle)
        {
            // Pick a random angle
            angle = Random.Range(minAngle, maxAngle);
        }

        // Center position + forward vector rotated around the Y axis by "angle" degrees, multiplies by "radius"
        return center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
    }

    public void SpawnCar()
    {
        playerAgent.transform.localPosition = new Vector3(8f, 3f, 0f);
        playerAgent.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        //playerAgent.transform.position = ChooseRandomPosition(transform.position, 0f, 90f, 0f, 8f) + Vector3.up * .5f;
        //playerAgent.transform.rotation = Quaternion.Euler(0f,Random.Range(0f, 360f), 0f);
    }
    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        ResetArea();
    }
    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        // Update the cumulative reward text
        cumulativeRewardText.text = playerAgent.GetCumulativeReward().ToString("0.00");
    }
}
