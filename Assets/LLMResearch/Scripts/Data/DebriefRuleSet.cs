using UnityEngine;

[CreateAssetMenu(
    fileName = "DebriefRules_",
    menuName = "LLM Research/Debrief Rule Set")]
public class DebriefRuleSet : ScriptableObject
{
    [TextArea(2, 5)]
    public string successDebrief;

    [TextArea(2, 5)]
    public string partialDebrief;

    [TextArea(2, 5)]
    public string failDebrief;
}