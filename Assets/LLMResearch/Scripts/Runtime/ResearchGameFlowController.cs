using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class ResearchGameFlowController : MonoBehaviour
{
    [Header("Presenters")]
    [SerializeField] private ResearchSceneFlowCoordinator sceneFlowCoordinator;
    [SerializeField] private ResearchTownMapPresenter townMapPresenter;
    [SerializeField] private ResearchChatPanelPresenter chatPanelPresenter;

    [Header("Systems")]
    [SerializeField] private ConversationMemoryStore memoryStore;
    [SerializeField] private ResearchDialogueService researchDialogueService;
    [SerializeField] private ResearchNpcStateStore npcStateStore;

    [Header("Research Session")]
    [SerializeField] private string sessionNamespace = "research_v1";

    private TownDefinition currentTown;
    private NpcDefinition currentNpc;
    private bool isBusy = false;

    private string pendingUserText = string.Empty;
    private string pendingNpcReply = string.Empty;
    private bool isStreamingNpcReply = false;

    private readonly object chatLock = new object();
    private bool needsChatLogRefresh = false;

    private List<ResearchChoiceOption> currentChoices = new List<ResearchChoiceOption>();

    private Stopwatch responseStopwatch = new Stopwatch();
    private long firstTokenMs = -1;
    private long completedMs = -1;

    private void Awake()
    {
        if (chatPanelPresenter != null)
        {
            chatPanelPresenter.Initialize(this, sceneFlowCoordinator);
        }

        if (townMapPresenter != null)
        {
            townMapPresenter.Initialize(this, sceneFlowCoordinator);
        }

        ReturnToWorldMap();
    }

    private void Update()
    {
        bool shouldRefresh = false;
        string safeReply = "";

        lock (chatLock)
        {
            if (needsChatLogRefresh)
            {
                shouldRefresh = true;
                safeReply = pendingNpcReply;
                needsChatLogRefresh = false;
            }
        }

        if (shouldRefresh && currentNpc != null && chatPanelPresenter != null)
        {
            string conversationKey = GetConversationKey(currentNpc);
            List<SavedChatTurn> currentTurns = memoryStore != null
                ? memoryStore.GetTurns(conversationKey)
                : new List<SavedChatTurn>();

            chatPanelPresenter.RefreshChatLog(
                currentNpc,
                currentTurns,
                pendingUserText,
                safeReply,
                isStreamingNpcReply
            );
        }

        // Legacy Input check removed to prevent InputSystem Exceptions. 
        // Submit is now handled by ChatInputField.onSubmit in the presenter.
    }

    public void OpenTown(TownDefinition town)
    {
        if (town == null)
        {
            UnityEngine.Debug.LogError("ResearchGameFlowController.OpenTown: town이 null입니다.");
            return;
        }

        currentTown = town;
        currentNpc = null;
        pendingUserText = string.Empty;
        pendingNpcReply = string.Empty;
        isStreamingNpcReply = false;
        currentChoices.Clear();

        if (chatPanelPresenter != null)
        {
            chatPanelPresenter.HideChoices();
            chatPanelPresenter.SetStatus(string.Empty);
        }

        if (townMapPresenter != null)
        {
            townMapPresenter.ShowTown(town);
        }
        else
        {
            sceneFlowCoordinator?.ShowTownMapPanel();
        }
    }

    public void OpenConversation(NpcDefinition npc)
    {
        if (npc == null)
        {
            UnityEngine.Debug.LogError("ResearchGameFlowController.OpenConversation: npc가 null입니다.");
            return;
        }

        currentNpc = npc;
        pendingUserText = string.Empty;
        pendingNpcReply = string.Empty;
        isStreamingNpcReply = false;
        currentChoices.Clear();

        string conversationKey = GetConversationKey(npc);
        string stateKey = GetStateKey(npc);

        List<SavedChatTurn> turns = memoryStore != null
            ? memoryStore.GetTurns(conversationKey)
            : new List<SavedChatTurn>();

        ResearchNpcState state = npcStateStore != null
            ? npcStateStore.GetState(stateKey)
            : new ResearchNpcState
            {
                npcId = stateKey,
                persuasionScore = 0,
                predictedEmotion = "중립",
                situationState = "기본"
            };

        if (chatPanelPresenter != null)
        {
            chatPanelPresenter.ShowConversation(npc, turns, pendingUserText);
            chatPanelPresenter.SetResearchState(
                state.persuasionScore,
                state.predictedEmotion,
                state.situationState
            );
            chatPanelPresenter.HideChoices();
            chatPanelPresenter.SetStatus(string.Empty);
        }
    }

    public async void SendCurrentInput()
    {
        if (isBusy) return;
        if (currentNpc == null) return;
        if (chatPanelPresenter == null) return;

        string userText = chatPanelPresenter.GetInputText();
        if (string.IsNullOrWhiteSpace(userText)) return;

        await ProcessTurnAsync(userText);
    }

    public void ReturnToTownMap()
    {
        if (currentTown == null)
        {
            ReturnToWorldMap();
            return;
        }

        currentNpc = null;
        pendingUserText = string.Empty;
        pendingNpcReply = string.Empty;
        isStreamingNpcReply = false;
        currentChoices.Clear();

        if (chatPanelPresenter != null)
        {
            chatPanelPresenter.HideChoices();
            chatPanelPresenter.SetStatus(string.Empty);
        }

        sceneFlowCoordinator?.ShowTownMapPanel();
    }

    public void ReturnToWorldMap()
    {
        currentTown = null;
        currentNpc = null;
        pendingUserText = string.Empty;
        pendingNpcReply = string.Empty;
        isStreamingNpcReply = false;
        currentChoices.Clear();
        isBusy = false;

        if (chatPanelPresenter != null)
        {
            chatPanelPresenter.HideChoices();
            chatPanelPresenter.SetStatus(string.Empty);
            chatPanelPresenter.SetBusyState(false);
        }

        sceneFlowCoordinator?.ShowWorldMapPanel();
    }

    private async Task ProcessTurnAsync(string userText)
    {
        if (currentNpc == null) return;
        if (researchDialogueService == null)
        {
            UnityEngine.Debug.LogError("ResearchGameFlowController: ResearchDialogueService가 연결되지 않았습니다.");
            return;
        }

        isBusy = true;
        pendingUserText = userText;
        pendingNpcReply = string.Empty;
        isStreamingNpcReply = true;

        currentChoices.Clear();

        firstTokenMs = -1;
        completedMs = -1;
        responseStopwatch.Restart();

        chatPanelPresenter?.SetBusyState(true);
        chatPanelPresenter?.ClearInput();
        chatPanelPresenter?.SetStatus("응답 생성 중...");
        chatPanelPresenter?.HideChoices();

        string conversationKey = GetConversationKey(currentNpc);
        string stateKey = GetStateKey(currentNpc);

        List<SavedChatTurn> turns = memoryStore != null
            ? memoryStore.GetTurns(conversationKey)
            : new List<SavedChatTurn>();

        chatPanelPresenter?.RefreshChatLog(currentNpc, turns, pendingUserText, pendingNpcReply, isStreamingNpcReply);

        try
        {
            ResearchNpcState currentState = npcStateStore != null
                ? npcStateStore.GetState(stateKey)
                : new ResearchNpcState
                {
                    npcId = stateKey,
                    persuasionScore = 0,
                    predictedEmotion = "중립",
                    situationState = "기본"
                };

            ResearchResponsePayload payload = await researchDialogueService.GetResearchReplyStreamAsync(
                currentNpc,
                turns,
                userText,
                currentState,
                onDelta: delta =>
                {
                    if (string.IsNullOrEmpty(delta)) return;

                    lock (chatLock)
                    {
                        if (firstTokenMs < 0)
                        {
                            firstTokenMs = responseStopwatch.ElapsedMilliseconds;
                        }

                        pendingNpcReply += delta;
                        needsChatLogRefresh = true;
                    }
                });

            completedMs = responseStopwatch.ElapsedMilliseconds;
            responseStopwatch.Stop();

            if (memoryStore != null)
            {
                memoryStore.CommitExchange(conversationKey, userText, payload.npcText);
            }

            if (npcStateStore != null)
            {
                npcStateStore.SetState(new ResearchNpcState
                {
                    npcId = stateKey,
                    persuasionScore = payload.persuasionScore,
                    predictedEmotion = payload.predictedEmotion,
                    situationState = payload.situationState
                });
            }

            pendingUserText = string.Empty;
            pendingNpcReply = string.Empty;
            isStreamingNpcReply = false;
            currentChoices = payload.nextChoices ?? new List<ResearchChoiceOption>();

            List<SavedChatTurn> updatedTurns = memoryStore != null
                ? memoryStore.GetTurns(conversationKey)
                : new List<SavedChatTurn>();

            string timingText = firstTokenMs >= 0
                ? $"첫 글자 {firstTokenMs}ms / 전체 {completedMs}ms"
                : $"전체 {completedMs}ms";

            chatPanelPresenter?.SetStatus(timingText);
            chatPanelPresenter?.SetResearchState(
                payload.persuasionScore,
                payload.predictedEmotion,
                payload.situationState
            );
            chatPanelPresenter?.RefreshChatLog(currentNpc, updatedTurns, pendingUserText, string.Empty, false);
            chatPanelPresenter?.SetChoiceButtons(currentChoices, HandleChoiceSelected);
            chatPanelPresenter?.FocusInput();
        }
        catch (Exception e)
        {
            responseStopwatch.Stop();

            pendingUserText = string.Empty;
            pendingNpcReply = string.Empty;
            isStreamingNpcReply = false;

            List<SavedChatTurn> currentTurns = memoryStore != null
                ? memoryStore.GetTurns(conversationKey)
                : new List<SavedChatTurn>();

            chatPanelPresenter?.SetStatus("오류: " + e.Message);
            chatPanelPresenter?.RefreshChatLog(currentNpc, currentTurns, pendingUserText, string.Empty, false);
            chatPanelPresenter?.HideChoices();
            chatPanelPresenter?.FocusInput();
        }
        finally
        {
            isBusy = false;
            chatPanelPresenter?.SetBusyState(false);
        }
    }

    private void HandleChoiceSelected(ResearchChoiceOption option)
    {
        if (option == null) return;
        if (isBusy) return;
        if (string.IsNullOrWhiteSpace(option.injectedUserText)) return;

        _ = ProcessTurnAsync(option.injectedUserText);
    }

    private string GetConversationKey(NpcDefinition npc)
    {
        return $"{sessionNamespace}__{npc.npcId}";
    }

    private string GetStateKey(NpcDefinition npc)
    {
        return $"{sessionNamespace}__{npc.npcId}";
    }
}