using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameFlowController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject worldMapPanel;
    [SerializeField] private GameObject townMapPanel;
    [SerializeField] private GameObject chatPanel;

    [Header("Town UI")]
    [SerializeField] private Image townMapBackgroundImage;
    [SerializeField] private TMP_Text townNameText;
    [SerializeField] private RectTransform npcButtonRoot;
    [SerializeField] private NpcButtonView npcButtonPrefab;
    [SerializeField] private Button backToWorldButton;

    [Header("Chat UI")]
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text chatLogText;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button backToTownButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Systems")]
    [SerializeField] private ConversationMemoryStore memoryStore;
    [SerializeField] private OpenAIChatService openAIChatService;

    private TownDefinition currentTown;
    private NpcDefinition currentNpc;
    private bool isBusy = false;
    private string pendingUserText = "";

    private void Awake()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        backToTownButton.onClick.AddListener(ShowTownMap);
        backToWorldButton.onClick.AddListener(ShowWorldMap);

        ShowWorldMap();
    }

    private void Update()
    {
        if (!chatPanel.activeSelf || isBusy) return;

        if (chatInputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnSendClicked();
        }
    }

    public void OpenTown(TownDefinition town)
    {
        if (town == null)
        {
            Debug.LogError("OpenTown: town이 null입니다.");
            return;
        }

        currentTown = town;

        if (townMapBackgroundImage != null)
        {
            townMapBackgroundImage.sprite = town.townMapSprite;
        }

        if (townNameText != null)
        {
            townNameText.text = town.displayName;
        }

        BuildNpcButtons();
        ShowOnly(townMapPanel);
    }

    public void OpenConversation(NpcDefinition npc)
    {
        if (npc == null)
        {
            Debug.LogError("OpenConversation: npc가 null입니다.");
            return;
        }

        currentNpc = npc;
        pendingUserText = "";
        statusText.text = "";

        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = npc.portrait;
        }

        if (npcNameText != null)
        {
            npcNameText.text = npc.displayName;
        }

        RefreshChatLog();
        ShowOnly(chatPanel);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }
    private async void OnSendClicked()
    {
        if (isBusy) return;
        if (currentNpc == null) return;

        string userText = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(userText)) return;

        isBusy = true;
        sendButton.interactable = false;
        backToTownButton.interactable = false;
        pendingUserText = userText;
        chatInputField.text = "";
        statusText.text = "생각 중...";
        RefreshChatLog();

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            string reply = await openAIChatService.GetNpcReplyAsync(
                currentNpc,
                memoryStore.GetTurns(currentNpc.npcId),
                userText);

            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            memoryStore.CommitExchange(currentNpc.npcId, userText, reply);

            pendingUserText = "";
            statusText.text = $"응답 시간: {elapsedSeconds:F2}초";
            RefreshChatLog();
            chatInputField.ActivateInputField();
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            pendingUserText = "";
            statusText.text = $"오류 ({elapsedSeconds:F2}초): " + e.Message;
            RefreshChatLog();
            chatInputField.ActivateInputField();
        }
        finally
        {
            isBusy = false;
            sendButton.interactable = true;
            backToTownButton.interactable = true;
        }
    }

    private void BuildNpcButtons()
    {
        for (int i = npcButtonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(npcButtonRoot.GetChild(i).gameObject);
        }

        if (currentTown == null || currentTown.npcSlots == null) return;

        for (int i = 0; i < currentTown.npcSlots.Count; i++)
        {
            TownNpcSlot slot = currentTown.npcSlots[i];
            if (slot == null || slot.npc == null) continue;

            NpcButtonView instance = Instantiate(npcButtonPrefab, npcButtonRoot);
            instance.Setup(slot, this);
        }
    }

    private void RefreshChatLog()
    {
        if (currentNpc == null)
        {
            chatLogText.text = "";
            return;
        }

        var turns = memoryStore.GetTurns(currentNpc.npcId);
        StringBuilder sb = new StringBuilder();

        if (turns.Count == 0 && string.IsNullOrEmpty(pendingUserText))
        {
            sb.Append("<i>아직 대화 기록이 없습니다.</i>");
        }
        else
        {
            for (int i = 0; i < turns.Count; i++)
            {
                SavedChatTurn turn = turns[i];
                bool isUser = turn.role == "user";
                string speaker = isUser ? "플레이어" : currentNpc.displayName;
                string color = isUser ? "#A7D8FF" : "#FFE7A7";

                sb.Append("<b><color=");
                sb.Append(color);
                sb.Append(">[");
                sb.Append(EscapeTmp(speaker));
                sb.Append("]</color></b>\n");
                sb.Append(EscapeTmp(turn.content));
                sb.Append("\n\n");
            }

            if (!string.IsNullOrEmpty(pendingUserText))
            {
                sb.Append("<b><color=#A7D8FF>[플레이어]</color></b>\n");
                sb.Append(EscapeTmp(pendingUserText));
                sb.Append("\n\n");
            }
        }

        chatLogText.text = sb.ToString();
        ScrollToBottom();
    }

    private void ShowWorldMap()
    {
        currentTown = null;
        currentNpc = null;
        pendingUserText = "";
        statusText.text = "";
        ShowOnly(worldMapPanel);
    }

    private void ShowTownMap()
    {
        if (currentTown == null)
        {
            ShowWorldMap();
            return;
        }

        currentNpc = null;
        pendingUserText = "";
        statusText.text = "";
        ShowOnly(townMapPanel);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        worldMapPanel.SetActive(panelToShow == worldMapPanel);
        townMapPanel.SetActive(panelToShow == townMapPanel);
        chatPanel.SetActive(panelToShow == chatPanel);
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private string EscapeTmp(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}