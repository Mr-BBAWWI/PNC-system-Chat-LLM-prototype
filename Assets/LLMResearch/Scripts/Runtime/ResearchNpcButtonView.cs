using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchNpcButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    private NpcDefinition currentNpc;
    private ResearchGameFlowController controller;

    public void Setup(TownNpcSlot slot, ResearchGameFlowController gameController)
    {
        if (slot == null || slot.npc == null) return;

        currentNpc = slot.npc;
        controller = gameController;

        if (portraitImage != null)
            portraitImage.sprite = currentNpc.portrait;

        if (nameText != null)
            nameText.text = currentNpc.displayName;

        RectTransform rect = transform as RectTransform;
        if (rect != null)
            rect.anchoredPosition = slot.anchoredPosition;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickNpcButton);
        }
    }

    private void OnClickNpcButton()
    {
        if (controller == null || currentNpc == null) return;
        controller.OpenConversation(currentNpc);
    }
}