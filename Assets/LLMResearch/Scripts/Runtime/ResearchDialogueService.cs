using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResearchDialogueService : MonoBehaviour
{
    [SerializeField] private OpenAIChatService openAIChatService;
    [SerializeField] private bool useDefaultChoicesWhenMissing = true;

    public async Task<ResearchResponsePayload> GetResearchReplyAsync(
        NpcDefinition npc,
        List<SavedChatTurn> turns,
        string userInput,
        ResearchNpcState currentState)
    {
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        if (openAIChatService == null)
            throw new InvalidOperationException("ResearchDialogueService: OpenAIChatService가 연결되지 않았습니다.");

        ResearchNpcState safeState = currentState ?? new ResearchNpcState
        {
            npcId = npc.npcId,
            persuasionScore = 0,
            predictedEmotion = "중립",
            situationState = "기본"
        };

        NpcDefinition tempNpc = Instantiate(npc);
        tempNpc.systemPrompt = BuildCombinedSystemPrompt(npc, safeState);

        try
        {
            string raw = await openAIChatService.GetNpcReplyAsync(tempNpc, turns, userInput);
            return ParsePayloadOrFallback(raw, safeState);
        }
        finally
        {
            Destroy(tempNpc);
        }
    }

    public async Task<ResearchResponsePayload> GetResearchReplyStreamAsync(
        NpcDefinition npc,
        List<SavedChatTurn> turns,
        string userInput,
        ResearchNpcState currentState,
        Action<string> onDelta)
    {
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));

        if (openAIChatService == null)
            throw new InvalidOperationException("ResearchDialogueService: OpenAIChatService가 연결되지 않았습니다.");

        ResearchNpcState safeState = currentState ?? new ResearchNpcState
        {
            npcId = npc.npcId,
            persuasionScore = 0,
            predictedEmotion = "중립",
            situationState = "기본"
        };

        NpcDefinition tempNpc = Instantiate(npc);
        tempNpc.systemPrompt = BuildCombinedSystemPrompt(npc, safeState);

        StringBuilder rawBuilder = new StringBuilder();
        string currentExtractedNpcText = string.Empty;

        try
        {
            await openAIChatService.GetNpcReplyStreamAsync(
                tempNpc,
                turns,
                userInput,
                delta =>
                {
                    if (string.IsNullOrEmpty(delta)) return;
                    rawBuilder.Append(delta);
                    
                    string rawSoFar = rawBuilder.ToString();
                    string extracted = ExtractStreamingNpcText(rawSoFar);
                    
                    if (extracted.Length > currentExtractedNpcText.Length)
                    {
                        string newDelta = extracted.Substring(currentExtractedNpcText.Length);
                        currentExtractedNpcText = extracted;
                        onDelta?.Invoke(newDelta);
                    }
                });

            string raw = rawBuilder.ToString();
            return ParsePayloadOrFallback(raw, safeState);
        }
        finally
        {
            Destroy(tempNpc);
        }
    }

    private string ExtractStreamingNpcText(string raw)
    {
        string targetKey = "\"npcText\"";
        int keyIndex = raw.IndexOf(targetKey);
        if (keyIndex < 0) return string.Empty;

        int colonIndex = raw.IndexOf(':', keyIndex);
        if (colonIndex < 0) return string.Empty;

        int quoteStart = raw.IndexOf('\"', colonIndex);
        if (quoteStart < 0) return string.Empty;

        bool inEscape = false;
        StringBuilder sb = new StringBuilder();

        for (int i = quoteStart + 1; i < raw.Length; i++)
        {
            char c = raw[i];
            if (inEscape)
            {
                if (c == 'n') sb.Append('\n');
                else if (c == 't') sb.Append('\t');
                else sb.Append(c);
                inEscape = false;
            }
            else if (c == '\\')
            {
                inEscape = true;
            }
            else if (c == '"')
            {
                break; // end of string
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private string BuildCombinedSystemPrompt(NpcDefinition npc, ResearchNpcState state)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(npc.systemPrompt);
        sb.AppendLine();
        sb.AppendLine("추가 연구 규칙:");
        sb.AppendLine("- 이 대화는 포인트앤클릭 기반 스토리 게임의 설득 장면이다.");
        sb.AppendLine("- 너는 NPC의 현재 감정 상태와 설득 정도를 내부적으로 추적한다.");
        sb.AppendLine("- persuasionScore는 0~100의 정수여야 한다.");
        sb.AppendLine("- predictedEmotion은 짧은 한국어 단어 또는 짧은 구로 작성한다. 예: 중립, 경계, 흔들림, 공감, 협조적");
        sb.AppendLine("- situationState는 현재 장면 상태를 짧게 요약한다. 예: 대화 시작, 경계 유지, 설득 진행, 거의 수락");
        sb.AppendLine("- npcText는 자연스러운 한국어 대사여야 한다.");
        sb.AppendLine("- nextChoices는 플레이어가 누를 수 있는 다음 선택지 2개를 제안한다.");
        sb.AppendLine("- nextChoices의 buttonLabel은 버튼에 바로 들어갈 짧은 문장이어야 한다.");
        sb.AppendLine("- nextChoices의 injectedUserText는 플레이어가 실제로 보낸 것처럼 자연스러운 한국어 문장이어야 한다.");
        sb.AppendLine("- 응답은 반드시 JSON 객체 하나만 출력한다.");
        sb.AppendLine("- 코드블럭 마크다운(```)을 절대 쓰지 마라.");
        sb.AppendLine("- ★중요★: 스트리밍 빠른 출력을 위해 JSON의 첫 번째 키는 반드시 \"npcText\"가 되어야 한다.");
        sb.AppendLine();
        sb.AppendLine("현재 NPC 상태:");
        sb.AppendLine($"- persuasionScore: {Mathf.Clamp(state.persuasionScore, 0, 100)}");
        sb.AppendLine($"- predictedEmotion: {SafeOrDefault(state.predictedEmotion, "중립")}");
        sb.AppendLine($"- situationState: {SafeOrDefault(state.situationState, "기본")}");
        sb.AppendLine();
        sb.AppendLine("반드시 아래 JSON 스키마를 따른다:");
        sb.AppendLine("{");
        sb.AppendLine("  \"npcText\": \"NPC의 실제 대사\",");
        sb.AppendLine("  \"predictedEmotion\": \"짧은 감정 요약\",");
        sb.AppendLine("  \"persuasionScore\": 35,");
        sb.AppendLine("  \"situationState\": \"현재 장면 상태\",");
        sb.AppendLine("  \"nextChoices\": [");
        sb.AppendLine("    {");
        sb.AppendLine("      \"choiceId\": \"c1\",");
        sb.AppendLine("      \"buttonLabel\": \"짧은 버튼 문구\",");
        sb.AppendLine("      \"injectedUserText\": \"플레이어가 실제로 보낼 한국어 문장\"");
        sb.AppendLine("    },");
        sb.AppendLine("    {");
        sb.AppendLine("      \"choiceId\": \"c2\",");
        sb.AppendLine("      \"buttonLabel\": \"짧은 버튼 문구\",");
        sb.AppendLine("      \"injectedUserText\": \"플레이어가 실제로 보낼 한국어 문장\"");
        sb.AppendLine("    }");
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private ResearchResponsePayload ParsePayloadOrFallback(string raw, ResearchNpcState currentState)
    {
        string json = ExtractJsonObject(raw);

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                ResearchResponsePayload payload = JsonUtility.FromJson<ResearchResponsePayload>(json);
                if (payload != null && !string.IsNullOrWhiteSpace(payload.npcText))
                {
                    return NormalizePayload(payload, currentState);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ResearchDialogueService] JSON 파싱 실패, fallback 사용: " + e.Message);
            }
        }

        return BuildFallbackPayload(raw, currentState);
    }

    private ResearchResponsePayload NormalizePayload(ResearchResponsePayload payload, ResearchNpcState currentState)
    {
        payload.persuasionScore = Mathf.Clamp(payload.persuasionScore, 0, 100);

        if (string.IsNullOrWhiteSpace(payload.predictedEmotion))
            payload.predictedEmotion = SafeOrDefault(currentState.predictedEmotion, "중립");

        if (string.IsNullOrWhiteSpace(payload.situationState))
            payload.situationState = SafeOrDefault(currentState.situationState, "기본");

        if (payload.nextChoices == null)
            payload.nextChoices = new List<ResearchChoiceOption>();

        List<ResearchChoiceOption> sanitized = new List<ResearchChoiceOption>();

        for (int i = 0; i < payload.nextChoices.Count && i < 2; i++)
        {
            ResearchChoiceOption option = payload.nextChoices[i];
            if (option == null) continue;

            if (string.IsNullOrWhiteSpace(option.choiceId))
                option.choiceId = "c" + (i + 1);

            if (string.IsNullOrWhiteSpace(option.buttonLabel))
                option.buttonLabel = "선택지 " + (i + 1);

            if (string.IsNullOrWhiteSpace(option.injectedUserText))
                option.injectedUserText = option.buttonLabel;

            sanitized.Add(option);
        }

        payload.nextChoices = sanitized;

        if (useDefaultChoicesWhenMissing && payload.nextChoices.Count == 0)
        {
            payload.nextChoices = BuildDefaultChoices();
        }

        return payload;
    }

    private ResearchResponsePayload BuildFallbackPayload(string raw, ResearchNpcState currentState)
    {
        ResearchResponsePayload payload = new ResearchResponsePayload();
        payload.npcText = string.IsNullOrWhiteSpace(raw)
            ? "지금은 바로 대답하기 어렵군요. 다시 말씀해 주시겠어요?"
            : raw.Trim();

        payload.predictedEmotion = SafeOrDefault(currentState.predictedEmotion, "중립");
        payload.persuasionScore = Mathf.Clamp(currentState.persuasionScore, 0, 100);
        payload.situationState = SafeOrDefault(currentState.situationState, "기본");
        payload.nextChoices = useDefaultChoicesWhenMissing ? BuildDefaultChoices() : new List<ResearchChoiceOption>();

        return payload;
    }

    private List<ResearchChoiceOption> BuildDefaultChoices()
    {
        return new List<ResearchChoiceOption>
        {
            new ResearchChoiceOption
            {
                choiceId = "fallback_1",
                buttonLabel = "상대 입장을 먼저 묻기",
                injectedUserText = "당신 입장에서 가장 걱정되는 점이 무엇인지 먼저 듣고 싶어요."
            },
            new ResearchChoiceOption
            {
                choiceId = "fallback_2",
                buttonLabel = "차분하게 다시 설득하기",
                injectedUserText = "제 의도를 오해하지 않으셨으면 해요. 왜 이 제안이 필요한지 차분히 다시 설명드릴게요."
            }
        };
    }

    private string ExtractJsonObject(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        string trimmed = raw.Trim();

        if (trimmed.StartsWith("```json"))
            trimmed = trimmed.Replace("```json", "").Trim();

        if (trimmed.StartsWith("```"))
            trimmed = trimmed.Replace("```", "").Trim();

        if (trimmed.EndsWith("```"))
            trimmed = trimmed.Replace("```", "").Trim();

        int start = trimmed.IndexOf('{');
        int end = trimmed.LastIndexOf('}');

        if (start >= 0 && end > start)
        {
            return trimmed.Substring(start, end - start + 1);
        }

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            return trimmed;

        return string.Empty;
    }

    private string SafeOrDefault(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}