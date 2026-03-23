using UnityEngine;

[CreateAssetMenu(
    fileName = "OutcomeThresholds_",
    menuName = "LLM Research/Outcome Threshold Set")]
public class OutcomeThresholdSet : ScriptableObject
{
    [Header("Score Thresholds")]
    public int successMinScore = 70;
    public int partialMinScore = 40;
    public int failBelowScore = 39;
}