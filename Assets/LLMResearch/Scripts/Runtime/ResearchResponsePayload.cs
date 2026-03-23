using System;
using System.Collections.Generic;

[Serializable]
public class ResearchResponsePayload
{
    public string npcText;
    public string predictedEmotion;
    public int persuasionScore;
    public string situationState;
    public List<ResearchChoiceOption> nextChoices = new List<ResearchChoiceOption>();
}