using System;
using UnityEngine;

[Serializable]
public class ResearchChoiceOption
{
    public string choiceId;
    public string buttonLabel;

    [TextArea(2, 4)]
    public string injectedUserText;
}