using System;

[Serializable]
public class ResearchNpcState
{
    public string npcId;
    public int persuasionScore;
    public string predictedEmotion = "중립";
    public string situationState = "기본";
}