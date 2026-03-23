using UnityEngine;

[CreateAssetMenu(
    fileName = "Objective_",
    menuName = "LLM Research/Learning Objective Definition")]
public class LearningObjectiveDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string objectiveId;
    public string displayName;

    [Header("Description")]
    [TextArea(2, 6)]
    public string description;

    [TextArea(2, 6)]
    public string successHint;
}