using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TownNpcSlot
{
    public NpcDefinition npc;
    public Vector2 anchoredPosition;
}

[CreateAssetMenu(menuName = "LLM Demo/Town Definition")]
public class TownDefinition : ScriptableObject
{
    public string townId = "town_default";
    public string displayName = "마을";
    public Sprite townMapSprite;
    public List<TownNpcSlot> npcSlots = new List<TownNpcSlot>();
}