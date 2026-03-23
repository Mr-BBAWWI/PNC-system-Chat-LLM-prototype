using UnityEngine;

public class ResearchSceneFlowCoordinator : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject worldMapPanel;
    [SerializeField] private GameObject townMapPanel;
    [SerializeField] private GameObject chatPanel;

    public void ShowWorldMap()
    {
        ShowOnly(worldMapPanel);
    }

    public void ShowTownMap()
    {
        ShowOnly(townMapPanel);
    }

    public void ShowChatPanel()
    {
        ShowOnly(chatPanel);
    }

    // 호환용 메서드
    public void ShowWorldMapPanel()
    {
        ShowWorldMap();
    }

    public void ShowTownMapPanel()
    {
        ShowTownMap();
    }

    private void ShowOnly(GameObject target)
    {
        if (worldMapPanel != null) worldMapPanel.SetActive(target == worldMapPanel);
        if (townMapPanel != null) townMapPanel.SetActive(target == townMapPanel);
        if (chatPanel != null) chatPanel.SetActive(target == chatPanel);
    }
}