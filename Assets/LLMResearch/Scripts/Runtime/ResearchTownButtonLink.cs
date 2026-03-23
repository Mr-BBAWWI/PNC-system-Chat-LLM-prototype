using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResearchTownButtonLink : MonoBehaviour
{
    [SerializeField] private TownDefinition townDefinition;
    [SerializeField] private ResearchGameFlowController controller;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponent<Button>();
        cachedButton.onClick.RemoveAllListeners();
        cachedButton.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (townDefinition == null || controller == null) return;
        controller.OpenTown(townDefinition);
    }
}