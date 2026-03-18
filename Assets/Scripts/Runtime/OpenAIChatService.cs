using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

public class OpenAIChatService : MonoBehaviour
{
    [SerializeField] private string modelId = "gpt-4o-mini";
    [SerializeField] private bool enableDebugLogs = false;

    private OpenAIClient client;

    private void Awake()
    {
        try
        {
            client = new OpenAIClient();
            client.EnableDebug = enableDebugLogs;
        }
        catch (Exception e)
        {
            Debug.LogError($"OpenAIClient 초기화 실패: {e}");
        }
    }

    public async Task<string> GetNpcReplyAsync(
        NpcDefinition npc,
        List<SavedChatTurn> savedTurns,
        string newUserMessage)
    {
        if (client == null)
        {
            throw new Exception("OpenAIClient가 초기화되지 않았습니다. .openai 파일을 확인하세요.");
        }

        if (npc == null)
        {
            throw new Exception("NPC 정의가 비어 있습니다.");
        }

        var messages = new List<Message>
        {
            new Message(Role.System, npc.systemPrompt)
        };

        for (int i = 0; i < savedTurns.Count; i++)
        {
            SavedChatTurn turn = savedTurns[i];
            Role role = turn.role == "assistant" ? Role.Assistant : Role.User;
            messages.Add(new Message(role, turn.content));
        }

        messages.Add(new Message(Role.User, newUserMessage));

        // 방법 A: 문자열 모델명 사용
        var chatRequest = new ChatRequest(messages, model: modelId);

        // 방법 B: Model 타입을 쓰고 싶으면 아래처럼 가능
        // var chatRequest = new ChatRequest(messages, new Model(modelId));

        var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);

        if (response == null || response.FirstChoice == null || response.FirstChoice.Message == null)
        {
            throw new Exception("LLM 응답이 비어 있습니다.");
        }

        string reply = response.FirstChoice.Message.ToString().Trim();

        if (string.IsNullOrWhiteSpace(reply))
        {
            throw new Exception("LLM 응답 텍스트가 비어 있습니다.");
        }

        return reply;
    }
}