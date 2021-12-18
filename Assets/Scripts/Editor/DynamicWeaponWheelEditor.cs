using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(DynamicWeaponWheel))]
public class DynamicWeaponWheelEditor : Editor {
   
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        DynamicWeaponWheel t = (DynamicWeaponWheel)target;
        if (GUILayout.Button("Generate Wheel")) {
            t.GenerateWheel();
        }
    }
}