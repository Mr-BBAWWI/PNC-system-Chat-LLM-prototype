using UnityEngine;

[CreateAssetMenu(menuName = "LLM Demo/NPC Definition")]
public class NpcDefinition : ScriptableObject
{
    public string npcId = "npc_default";
    public string displayName = "NPC";
    public Sprite portrait;

    [TextArea(6, 20)]
    public string systemPrompt =
        "당신은 게임 속 NPC입니다. 항상 한국어로 자연스럽게 대답하세요.";
}