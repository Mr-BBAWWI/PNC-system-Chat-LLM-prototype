using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Scenario_",
    menuName = "LLM Research/Story Scenario Definition")]
public class StoryScenarioDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string scenarioId;
    public string displayName;

    [Header("Story Context")]
    [TextArea(3, 8)]
    public string situationSummary;

    [TextArea(2, 6)]
    public string playerGoal;

    [TextArea(2, 6)]
    public string successConditionText;

    [TextArea(2, 6)]
    public string failureConditionText;

    [Header("World Setup")]
    public TownDefinition startingTown;
    public List<NpcDefinition> involvedNpcs = new List<NpcDefinition>();

    [Header("Research Data")]
    public List<LearningObjectiveDefinition> learningObjectives = new List<LearningObjectiveDefinition>();
    public PersuasionRuleTable persuasionRuleTable;
    public OutcomeThresholdSet outcomeThresholds;
    public ChoiceCatalog choiceCatalog;
    public DebriefRuleSet debriefRuleSet;
}