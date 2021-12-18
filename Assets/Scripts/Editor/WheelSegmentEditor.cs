using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(WheelSegment))]
public class WheelSegmentEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        WheelSegment t = (WheelSegment)target;
    }
}