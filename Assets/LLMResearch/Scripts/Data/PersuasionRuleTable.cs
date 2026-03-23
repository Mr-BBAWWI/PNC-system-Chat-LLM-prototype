using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersuasionTagRule
{
    public string tagId;
    public string displayName;
    public int trustDelta;
    public int resistanceDelta;
    public int persuasionDelta;

    [TextArea(1, 3)]
    public string designerNote;
}

[CreateAssetMenu(
    fileName = "PersuasionRules_",
    menuName = "LLM Research/Persuasion Rule Table")]
public class PersuasionRuleTable : ScriptableObject
{
    public List<PersuasionTagRule> rules = new List<PersuasionTagRule>();
}