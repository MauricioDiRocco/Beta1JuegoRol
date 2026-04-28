using UnityEngine;

[CreateAssetMenu(fileName = "NuevoSlot", menuName = "RPG/Skill Slot Data")]
public class SkillSlotData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public bool isPotion;
    public int quantity = 1;
    public int healAmount;

    [HideInInspector] public ItemData sourceItem;
    [HideInInspector] public bool isEmpty = true;
    [HideInInspector] public bool isOnCooldown;
    [HideInInspector] public float cooldownRemaining;

    public void SetEmpty()
    {
        isEmpty = true;
        icon = null;
        skillName = "";
        isPotion = false;
        quantity = 0;
        sourceItem = null;
        isOnCooldown = false;
        cooldownRemaining = 0f;
    }

    public void CopyFrom(SkillSlotData other)
    {
        if (other == null) { SetEmpty(); return; }
        skillName = other.skillName;
        icon = other.icon;
        isPotion = other.isPotion;
        quantity = other.quantity;
        healAmount = other.healAmount;
        sourceItem = other.sourceItem;
        isEmpty = other.isEmpty;
        isOnCooldown = other.isOnCooldown;
        cooldownRemaining = other.cooldownRemaining;
    }
}