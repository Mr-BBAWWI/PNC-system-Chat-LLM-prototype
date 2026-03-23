using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchChoiceButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text labelText;

    public void Setup(ResearchChoiceOption option, Action<ResearchChoiceOption> onClicked)
    {
        if (option == null) return;

        if (labelText != null)
            labelText.text = option.buttonLabel;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onClicked?.Invoke(option);
            });
        }
    }
}