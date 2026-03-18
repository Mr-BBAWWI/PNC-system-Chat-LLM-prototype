using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    private NpcDefinition npcDefinition;
    private GameFlowController controller;

    public void Setup(TownNpcSlot slot, GameFlowController gameFlowController)
    {
        npcDefinition = slot.npc;
        controller = gameFlowController;

        if (portraitImage != null)
        {
            portraitImage.sprite = npcDefinition.portrait;
        }

        if (nameText != null)
        {
            nameText.text = npcDefinition.displayName;
        }

        RectTransform rect = transform as RectTransform;
        rect.anchoredPosition = slot.anchoredPosition;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickNpc);
    }

    private void OnClickNpc()
    {
        if (controller == null || npcDefinition == null)
        {
            Debug.LogError("NpcButtonView: controller 또는 npcDefinition이 비어 있습니다.");
            return;
        }

        controller.OpenConversation(npcDefinition);
    }
}