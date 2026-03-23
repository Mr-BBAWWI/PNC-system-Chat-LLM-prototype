using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchChatPanelPresenter : MonoBehaviour
{
    [Header("Base Chat UI")]
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text chatLogText;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button backToTownButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Research HUD")]
    [SerializeField] private TMP_Text persuasionScoreText;
    [SerializeField] private TMP_Text predictedEmotionText;
    [SerializeField] private TMP_Text situationStateText;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private RectTransform choiceButtonRoot;
    [SerializeField] private ResearchChoiceButtonView choiceButtonPrefab;

    private ResearchGameFlowController controller;
    private ResearchSceneFlowCoordinator flow;

    public TMP_InputField ChatInputField => chatInputField;
    public Button SendButton => sendButton;
    public Button BackToTownButton => backToTownButton;

    public void Initialize(ResearchGameFlowController gameController, ResearchSceneFlowCoordinator sceneFlow)
    {
        controller = gameController;
        flow = sceneFlow;

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(HandleSend);
        }

        if (backToTownButton != null)
        {
            backToTownButton.onClick.RemoveAllListeners();
            backToTownButton.onClick.AddListener(HandleBackToTown);
        }

        if (chatInputField != null)
        {
            chatInputField.onSubmit.RemoveAllListeners();
            chatInputField.onSubmit.AddListener(_ => HandleSend());
        }

        SetResearchState(0, "중립", "기본");
        HideChoices();
    }

    public void ShowConversation(NpcDefinition npc, List<SavedChatTurn> turns, string pendingUserText)
    {
        if (npc == null) return;

        if (npcPortraitImage != null)
            npcPortraitImage.sprite = npc.portrait;

        if (npcNameText != null)
            npcNameText.text = npc.displayName;

        RefreshChatLog(npc, turns, pendingUserText);

        if (statusText != null)
            statusText.text = string.Empty;

        SetResearchState(0, "중립", "대화 시작");
        HideChoices();

        if (chatInputField != null)
        {
            chatInputField.text = string.Empty;
            chatInputField.ActivateInputField();
        }

        flow?.ShowChatPanel();
    }

    public void SetBusyState(bool isBusy)
    {
        if (sendButton != null) sendButton.interactable = !isBusy;
        if (backToTownButton != null) backToTownButton.interactable = !isBusy;
    }

    public void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public string GetInputText()
    {
        return chatInputField != null ? chatInputField.text.Trim() : string.Empty;
    }

    public void ClearInput()
    {
        if (chatInputField != null)
            chatInputField.text = string.Empty;
    }

    public void FocusInput()
    {
        if (chatInputField != null)
            chatInputField.ActivateInputField();
    }

    public void SetResearchState(int persuasionScore, string predictedEmotion, string situationState)
    {
        if (persuasionScoreText != null)
            persuasionScoreText.text = $"설득도: {Mathf.Clamp(persuasionScore, 0, 100)}";

        if (predictedEmotionText != null)
            predictedEmotionText.text = $"감정 예측: {SafeOrDefault(predictedEmotion, "중립")}";

        if (situationStateText != null)
            situationStateText.text = $"상황 상태: {SafeOrDefault(situationState, "기본")}";
    }

    public void SetChoiceButtons(List<ResearchChoiceOption> choices, Action<ResearchChoiceOption> onChoiceClicked)
    {
        ClearChoiceButtons();

        if (choicePanel != null)
            choicePanel.SetActive(choices != null && choices.Count > 0);

        if (choices == null || choices.Count == 0 || choiceButtonRoot == null || choiceButtonPrefab == null)
            return;

        for (int i = 0; i < choices.Count; i++)
        {
            ResearchChoiceOption option = choices[i];
            if (option == null) continue;

            ResearchChoiceButtonView instance = Instantiate(choiceButtonPrefab, choiceButtonRoot);
            instance.Setup(option, onChoiceClicked);
        }
    }

    public void HideChoices()
    {
        ClearChoiceButtons();

        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    public void RefreshChatLog(NpcDefinition npc, List<SavedChatTurn> turns, string pendingUserText, string pendingNpcReply = "", bool isStreamingNpcReply = false)
    {
        if (chatLogText == null || npc == null) return;

        StringBuilder sb = new StringBuilder();

        if ((turns == null || turns.Count == 0) && string.IsNullOrEmpty(pendingUserText) && string.IsNullOrEmpty(pendingNpcReply))
        {
            sb.Append("<i>아직 대화 기록이 없습니다.</i>");
        }
        else
        {
            if (turns != null)
            {
                for (int i = 0; i < turns.Count; i++)
                {
                    SavedChatTurn turn = turns[i];
                    bool isUser = turn.role == "user";
                    string speaker = isUser ? "플레이어" : npc.displayName;
                    string color = isUser ? "#A7D8FF" : "#FFE7A7";

                    sb.Append("<b><color=");
                    sb.Append(color);
                    sb.Append(">[");
                    sb.Append(EscapeTmp(speaker));
                    sb.Append("]</color></b>\n");
                    sb.Append(EscapeTmp(turn.content));
                    sb.Append("\n\n");
                }
            }

            if (!string.IsNullOrEmpty(pendingUserText))
            {
                sb.Append("<b><color=#A7D8FF>[플레이어]</color></b>\n");
                sb.Append(EscapeTmp(pendingUserText));
                sb.Append("\n\n");
            }

            if (isStreamingNpcReply || !string.IsNullOrEmpty(pendingNpcReply))
            {
                sb.Append("<b><color=#FFE7A7>[");
                sb.Append(EscapeTmp(npc.displayName));
                sb.Append("]</color></b>\n");
                
                if (!string.IsNullOrEmpty(pendingNpcReply))
                {
                    sb.Append(EscapeTmp(pendingNpcReply));
                }
                
                if (isStreamingNpcReply)
                {
                    sb.Append(" <color=#aaaaaa>...</color>");
                }
                sb.Append("\n\n");
            }
        }

        chatLogText.text = sb.ToString();
        ScrollToBottom();
    }

    private void HandleSend()
    {
        controller?.SendCurrentInput();
    }

    private void HandleBackToTown()
    {
        controller?.ReturnToTownMap();
    }

    private void ClearChoiceButtons()
    {
        if (choiceButtonRoot == null) return;

        for (int i = choiceButtonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(choiceButtonRoot.GetChild(i).gameObject);
        }
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();

        if (chatScrollRect != null)
            chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private string EscapeTmp(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        return value.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
    }

    private string SafeOrDefault(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}