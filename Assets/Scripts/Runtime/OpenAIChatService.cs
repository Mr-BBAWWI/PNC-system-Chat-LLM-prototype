using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

public class OpenAIChatService : MonoBehaviour
{
    [SerializeField] private string modelId = "gpt-4o-mini";
    [SerializeField] private bool enableDebugLogs = false;

    [Header("OpenAI Auth & Settings (선택: 경고 제거용)")]
    [SerializeField] private OpenAIConfiguration configuration;

    private OpenAIClient client;

    private void Awake()
    {
        try
        {
            var auth = configuration != null ? new OpenAIAuthentication(configuration) : OpenAIAuthentication.Default;
            var clientSettings = configuration != null ? new OpenAISettings(configuration) : OpenAISettings.Default;

            client = new OpenAIClient(auth, clientSettings);
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
            string content = role == Role.Assistant 
                ? $"{{\"npcText\": \"{turn.content.Replace("\"", "\\\"").Replace("\n", "\\n")}\"}}" 
                : turn.content;
            messages.Add(new Message(role, content));
        }

        messages.Add(new Message(Role.User, newUserMessage));

        var chatRequest = new ChatRequest(messages, model: modelId);
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

    public async Task<string> GetNpcReplyStreamAsync(
        NpcDefinition npc,
        List<SavedChatTurn> savedTurns,
        string newUserMessage,
        Action<string> onDelta)
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
            string content = role == Role.Assistant 
                ? $"{{\"npcText\": \"{turn.content.Replace("\"", "\\\"").Replace("\n", "\\n")}\"}}" 
                : turn.content;
            messages.Add(new Message(role, content));
        }

        messages.Add(new Message(Role.User, newUserMessage));

        var chatRequest = new ChatRequest(messages, model: modelId);

        StringBuilder fullText = new StringBuilder();

        await client.ChatEndpoint.StreamCompletionAsync(chatRequest, partialResponse =>
        {
            if (partialResponse == null) return;
            if (partialResponse.FirstChoice == null) return;

            string delta = partialResponse.FirstChoice.Delta?.Content;

            if (string.IsNullOrEmpty(delta)) return;

            fullText.Append(delta);
            onDelta?.Invoke(delta);
        });

        string finalReply = fullText.ToString().Trim();

        if (string.IsNullOrWhiteSpace(finalReply))
        {
            throw new Exception("스트리밍 응답 텍스트가 비어 있습니다.");
        }

        return finalReply;
    }
}