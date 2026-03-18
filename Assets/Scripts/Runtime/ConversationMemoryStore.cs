using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SavedChatTurn
{
    public string role;          // "user" or "assistant"
    public string content;
    public string createdAtIso;
}

[Serializable]
public class SavedNpcConversation
{
    public string npcId;
    public List<SavedChatTurn> turns = new List<SavedChatTurn>();
}

[Serializable]
public class SavedConversationDatabase
{
    public List<SavedNpcConversation> records = new List<SavedNpcConversation>();
}

public class ConversationMemoryStore : MonoBehaviour
{
    private SavedConversationDatabase database = new SavedConversationDatabase();

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "npc_conversations.json");

    private void Awake()
    {
        Load();
    }

    public List<SavedChatTurn> GetTurns(string npcId)
    {
        return GetOrCreateRecord(npcId).turns;
    }

    public void CommitExchange(string npcId, string userText, string assistantText)
    {
        SavedNpcConversation record = GetOrCreateRecord(npcId);

        record.turns.Add(new SavedChatTurn
        {
            role = "user",
            content = userText,
            createdAtIso = DateTime.UtcNow.ToString("o")
        });

        record.turns.Add(new SavedChatTurn
        {
            role = "assistant",
            content = assistantText,
            createdAtIso = DateTime.UtcNow.ToString("o")
        });

        Save();
    }

    public void ClearConversation(string npcId)
    {
        SavedNpcConversation record = GetOrCreateRecord(npcId);
        record.turns.Clear();
        Save();
    }

    private SavedNpcConversation GetOrCreateRecord(string npcId)
    {
        for (int i = 0; i < database.records.Count; i++)
        {
            if (database.records[i].npcId == npcId)
            {
                return database.records[i];
            }
        }

        SavedNpcConversation newRecord = new SavedNpcConversation
        {
            npcId = npcId
        };

        database.records.Add(newRecord);
        return newRecord;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                database = new SavedConversationDatabase();
                return;
            }

            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                database = new SavedConversationDatabase();
                return;
            }

            database = JsonUtility.FromJson<SavedConversationDatabase>(json);
            if (database == null)
            {
                database = new SavedConversationDatabase();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ConversationMemoryStore Load 실패: {e}");
            database = new SavedConversationDatabase();
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(database, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"ConversationMemoryStore Save 실패: {e}");
        }
    }
}