using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResearchNpcStateStore : MonoBehaviour
{
    [Serializable]
    private class ResearchNpcStateWrapper
    {
        public List<ResearchNpcState> states = new List<ResearchNpcState>();
    }

    [SerializeField] private string fileName = "research_npc_states.json";

    private readonly Dictionary<string, ResearchNpcState> stateMap = new Dictionary<string, ResearchNpcState>();

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        Load();
    }

    public ResearchNpcState GetState(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            return CreateDefaultState(string.Empty);
        }

        if (!stateMap.TryGetValue(npcId, out ResearchNpcState state))
        {
            state = CreateDefaultState(npcId);
            stateMap[npcId] = Clone(state);
            Save();
        }

        return Clone(state);
    }

    public void SetState(ResearchNpcState newState)
    {
        if (newState == null || string.IsNullOrWhiteSpace(newState.npcId))
            return;

        newState.persuasionScore = Mathf.Clamp(newState.persuasionScore, 0, 100);

        if (string.IsNullOrWhiteSpace(newState.predictedEmotion))
            newState.predictedEmotion = "중립";

        if (string.IsNullOrWhiteSpace(newState.situationState))
            newState.situationState = "기본";

        stateMap[newState.npcId] = Clone(newState);
        Save();
    }

    public void ClearNpcState(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return;

        if (stateMap.Remove(npcId))
        {
            Save();
        }
    }

    public void ClearAllStates()
    {
        stateMap.Clear();
        Save();
    }

    [ContextMenu("Log Research State Save Path")]
    private void LogSavePath()
    {
        Debug.Log($"[ResearchNpcStateStore] SavePath = {SavePath}");
    }

    [ContextMenu("Clear All Research NPC States")]
    private void EditorClearAllStates()
    {
        ClearAllStates();
        Debug.Log("[ResearchNpcStateStore] 모든 연구 NPC 상태를 삭제했습니다.");
    }

    private void Load()
    {
        stateMap.Clear();

        if (!File.Exists(SavePath))
            return;

        try
        {
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            ResearchNpcStateWrapper wrapper = JsonUtility.FromJson<ResearchNpcStateWrapper>(json);
            if (wrapper == null || wrapper.states == null)
                return;

            for (int i = 0; i < wrapper.states.Count; i++)
            {
                ResearchNpcState state = wrapper.states[i];
                if (state == null || string.IsNullOrWhiteSpace(state.npcId))
                    continue;

                stateMap[state.npcId] = Clone(state);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[ResearchNpcStateStore] Load 실패: " + e.Message);
        }
    }

    private void Save()
    {
        try
        {
            ResearchNpcStateWrapper wrapper = new ResearchNpcStateWrapper();

            foreach (KeyValuePair<string, ResearchNpcState> pair in stateMap)
            {
                wrapper.states.Add(Clone(pair.Value));
            }

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("[ResearchNpcStateStore] Save 실패: " + e.Message);
        }
    }

    private ResearchNpcState CreateDefaultState(string npcId)
    {
        return new ResearchNpcState
        {
            npcId = npcId,
            persuasionScore = 0,
            predictedEmotion = "중립",
            situationState = "기본"
        };
    }

    private ResearchNpcState Clone(ResearchNpcState state)
    {
        return new ResearchNpcState
        {
            npcId = state.npcId,
            persuasionScore = state.persuasionScore,
            predictedEmotion = state.predictedEmotion,
            situationState = state.situationState
        };
    }
}