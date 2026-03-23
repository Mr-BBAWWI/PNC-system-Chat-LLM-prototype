using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChoiceEntry
{
    public string choiceId;

    [TextArea(1, 3)]
    public string displayText;

    public string moveTag;
}

[CreateAssetMenu(
    fileName = "ChoiceCatalog_",
    menuName = "LLM Research/Choice Catalog")]
public class ChoiceCatalog : ScriptableObject
{
    public List<ChoiceEntry> choices = new List<ChoiceEntry>();
}