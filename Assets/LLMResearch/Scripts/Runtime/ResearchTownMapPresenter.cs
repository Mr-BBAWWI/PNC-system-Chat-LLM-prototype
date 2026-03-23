using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchTownMapPresenter : MonoBehaviour
{
    [SerializeField] private Image townMapBackgroundImage;
    [SerializeField] private TMP_Text townNameText;
    [SerializeField] private RectTransform npcButtonRoot;
    [SerializeField] private ResearchNpcButtonView npcButtonPrefab;
    [SerializeField] private Button backToWorldButton;

    private ResearchGameFlowController controller;
    private ResearchSceneFlowCoordinator flow;

    public void Initialize(ResearchGameFlowController gameController, ResearchSceneFlowCoordinator sceneFlow)
    {
        controller = gameController;
        flow = sceneFlow;

        if (backToWorldButton != null)
        {
            backToWorldButton.onClick.RemoveAllListeners();
            backToWorldButton.onClick.AddListener(HandleBackToWorld);
        }
    }

    public void ShowTown(TownDefinition town)
    {
        if (town == null) return;

        if (townMapBackgroundImage != null)
            townMapBackgroundImage.sprite = town.townMapSprite;

        if (townNameText != null)
            townNameText.text = town.displayName;

        RebuildNpcButtons(town);

        if (flow != null)
            flow.ShowTownMap();
    }

    private void RebuildNpcButtons(TownDefinition town)
    {
        if (npcButtonRoot == null || npcButtonPrefab == null) return;

        for (int i = npcButtonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(npcButtonRoot.GetChild(i).gameObject);
        }

        if (town.npcSlots == null) return;

        for (int i = 0; i < town.npcSlots.Count; i++)
        {
            TownNpcSlot slot = town.npcSlots[i];
            if (slot == null || slot.npc == null) continue;

            ResearchNpcButtonView instance = Instantiate(npcButtonPrefab, npcButtonRoot);
            instance.Setup(slot, controller);
        }
    }

    private void HandleBackToWorld()
    {
        controller?.ReturnToWorldMap();
    }
}