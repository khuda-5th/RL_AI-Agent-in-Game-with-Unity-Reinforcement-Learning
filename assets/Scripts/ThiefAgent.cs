using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ThiefAgent : Agent
{
    private Rigidbody rBody;
    public Transform police;
    public float moveSpeed = 0.5f;
    private float episodeTime;
    private bool isEpisodeEnd = false;
    private AgentManager agentManager;
    private float t_cumulativeReward = 0.0f;

    public void SetManager(AgentManager manager)
    {
        agentManager = manager;
    }

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        rBody.useGravity = false;
        Time.timeScale = 1.0f; // 타임 스케일 설정
    }

    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        isEpisodeEnd = false;
        t_cumulativeReward = 0.0f;

        Vector3 newPolicePosition;
        Vector3 newThiefPosition;
        do
        {
            newPolicePosition = new Vector3(Random.Range(-50, 50), 0.5f, Random.Range(-50, 50));
            newThiefPosition = new Vector3(Random.Range(-50, 50), 0.5f, Random.Range(-50, 50));
        }
        while (Vector3.Distance(newPolicePosition, newThiefPosition) < 15.1f);

        transform.position = newThiefPosition;
        police.position = newPolicePosition;

        episodeTime = 199.0f;

        InvokeRepeating("MoveAgent", 0f, 0.001f);
    }

    void MoveAgent()
    {
        if (isEpisodeEnd || agentManager == null) return;

        // 경찰을 피해서 도망가기
        Vector3 direction = transform.position - police.position;
        direction.Normalize();
        Vector3 move = direction * moveSpeed * 0.02f;
        transform.Translate(move, Space.World);

        Vector3 fixedPosition = transform.position;
        fixedPosition.y = 0.5f;
        transform.position = fixedPosition;

        float distanceToPolice = Vector3.Distance(transform.position, police.position);

        if (distanceToPolice < 2.241592f)
        {
            AddReward(-6000.0f);
            t_cumulativeReward += -6000.0f;
            isEpisodeEnd = true;
            CancelInvoke("MoveAgent");
            agentManager.EndEpisodes("Thief was caught by the police");
        }
        /*else if (distanceToPolice < 12.5f && distanceToPolice >= 2.541592f)
        {
            AddReward(-0.001f);
            t_cumulativeReward += -0.001f;
        }
        else
        {
            AddReward(0.07f);
            t_cumulativeReward += 0.07f;
        }*/

        /*if (transform.position.x < -48.0f || transform.position.x > 48.0f || transform.position.z < -48.0f || transform.position.z > 48.0f)
        {
            AddReward(-0.1f);
            t_cumulativeReward += -0.001f;
        }*/
        if (transform.position.x > -10.0f || transform.position.x < 10.0f || transform.position.z > -10.0f || transform.position.z < 10.0f)
        {
            AddReward(0.01f);
            t_cumulativeReward += 0.01f;
        }

        episodeTime -= Time.deltaTime;
        if (episodeTime <= 0)
        {
            AddReward(6000.0f);
            t_cumulativeReward += 6000.0f;
            isEpisodeEnd = true;
            CancelInvoke("MoveAgent");
            agentManager.EndEpisodes("Episode time ended");
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(police.localPosition);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        // 추가적인 관측값을 채우기 위해 더 많은 정보를 추가합니다.
        for (int i = 0; i < 30; i++) // 100 - 8 = 92
        {
            sensor.AddObservation(0.0f); // 더미 값 추가
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 이동 명령은 MoveAgent 메서드에서 처리
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public new float GetCumulativeReward()
    {
        return t_cumulativeReward;
    }
}