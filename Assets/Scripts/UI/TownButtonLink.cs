using UnityEngine;
using UnityEngine.UI;

public class TownButtonLink : MonoBehaviour
{
    [SerializeField] private TownDefinition townDefinition;
    [SerializeField] private GameFlowController controller;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickTownButton);
        }
    }

    private void OnClickTownButton()
    {
        if (controller == null)
        {
            Debug.LogError("TownButtonLink: controller가 연결되지 않았습니다.");
            return;
        }

        if (townDefinition == null)
        {
            Debug.LogError("TownButtonLink: townDefinition이 연결되지 않았습니다.");
            return;
        }

        controller.OpenTown(townDefinition);
    }
}