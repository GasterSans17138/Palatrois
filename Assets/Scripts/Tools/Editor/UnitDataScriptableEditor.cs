#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitDataScriptable))]
public class UnitDataScriptableEditor : Editor
{
    SerializedProperty bonusProp;
    SerializedProperty attackBonusProp;
    SerializedProperty speedBonusProp;
    SerializedProperty healthBonusProp;
    SerializedProperty defenseBonusProp;
    SerializedProperty rangeBonusProp;
    SerializedProperty repairBonusProp;
    SerializedProperty attackSpeedBonusProp;
    SerializedProperty captureRangeBonusProp;

    void OnEnable()
    {
        bonusProp = serializedObject.FindProperty("Bonus");
        attackBonusProp = serializedObject.FindProperty("AttackBonus");
        speedBonusProp = serializedObject.FindProperty("SpeedBonus");
        healthBonusProp = serializedObject.FindProperty("HealthBonus");
        defenseBonusProp = serializedObject.FindProperty("DefenseBonus");
        rangeBonusProp = serializedObject.FindProperty("RangeBonus");
        repairBonusProp = serializedObject.FindProperty("RepairBonus");
        attackSpeedBonusProp = serializedObject.FindProperty("AttackSpeedBonus");
        captureRangeBonusProp = serializedObject.FindProperty("CaptureRangeBonus");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Affiche tout sauf les bonus spécifiques
        DrawPropertiesExcluding(serializedObject,
            "AttackBonus", "SpeedBonus", "HealthBonus", "DefenseBonus",
            "RangeBonus", "RepairBonus", "AttackSpeedBonus", "CaptureRangeBonus");

        BonusFlags flags = (BonusFlags)bonusProp.intValue;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Conditional Bonuses", EditorStyles.boldLabel);

        DrawBonusField(flags, BonusFlags.Attack, attackBonusProp);
        DrawBonusField(flags, BonusFlags.Speed, speedBonusProp);
        DrawBonusField(flags, BonusFlags.Health, healthBonusProp);
        DrawBonusField(flags, BonusFlags.Defense, defenseBonusProp);
        DrawBonusField(flags, BonusFlags.Range, rangeBonusProp);
        DrawBonusField(flags, BonusFlags.Repair, repairBonusProp);
        DrawBonusField(flags, BonusFlags.AttackSpeed, attackSpeedBonusProp);
        DrawBonusField(flags, BonusFlags.CaptureRange, captureRangeBonusProp);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBonusField(BonusFlags flags, BonusFlags flagToCheck, SerializedProperty prop)
    {
        if (flags.HasFlag(flagToCheck))
        {
            EditorGUILayout.PropertyField(prop);
        }
        else
        {
            prop.floatValue = 0f;
        }
    }
}
#endif
