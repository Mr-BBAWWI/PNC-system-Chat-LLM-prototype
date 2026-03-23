using System;

[Serializable]
public class NpcResearchState
{
    public string npcId;
    public int persuasionScore;
    public int trustScore;
    public NpcEmotionType currentEmotion;
    public string currentPhase;
    public string lastEvaluationSummary;

    public NpcResearchState(string npcId)
    {
        this.npcId = npcId;
        persuasionScore = 0;
        trustScore = 0;
        currentEmotion = NpcEmotionType.Neutral;
        currentPhase = "default";
        lastEvaluationSummary = "";
    }

    public void ClampScores()
    {
        persuasionScore = Math.Clamp(persuasionScore, -100, 100);
        trustScore = Math.Clamp(trustScore, -100, 100);
    }
}