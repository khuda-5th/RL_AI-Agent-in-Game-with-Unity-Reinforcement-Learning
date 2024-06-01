using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public ThiefAgent thiefAgent;
    public PoliceAgent policeAgent;

    void Start()
    {
        if (thiefAgent != null && policeAgent != null)
        {
            thiefAgent.SetManager(this);
            policeAgent.SetManager(this);
            Time.timeScale = 1.0f; // 타임 스케일 설정
        }
        else
        {
            Debug.LogError("ThiefAgent or PoliceAgent is not assigned in the AgentManager.");
        }
    }

    public void EndEpisodes(string reason)
    {
        Debug.Log($"Episode ended due to: {reason}");
        Debug.Log($"Thief total reward: {thiefAgent.GetCumulativeReward()}");
        Debug.Log($"Police total reward: {policeAgent.GetCumulativeReward()}");

        thiefAgent.EndEpisode();
        policeAgent.EndEpisode();
    }
}
