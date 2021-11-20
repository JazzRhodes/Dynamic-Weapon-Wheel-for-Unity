using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class DynamicWeaponWheel : MonoBehaviour {
    public bool autoUpdate = true;
    public WheelSegment wheelSegmentPrefab;
    [Range(1, 20)] public int numberOfSegments;
    public Transform wheelCenter;
    public bool updateInPlaymode;
    bool initialized;
    bool notInPrefabMode;
    bool inEditor() {
        return Application.isEditor;
    }
    bool IsPrefab() {
        return !(PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab);
    }
    void OnValidate() {
        if (!initialized) {
            initialized = true;
#if UNITY_EDITOR
            if (IsPrefab()) {
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }
#endif
        if (initialized) {
            UnityEditor.EditorApplication.delayCall += () => {
                bool editorCheck = Application.isPlaying && autoUpdate && updateInPlaymode || !Application.isPlaying && autoUpdate;
#if UNITY_EDITOR
                notInPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() == null;
#endif
                if (editorCheck && notInPrefabMode || editorCheck && !notInPrefabMode && !inEditor()) {
                    notInPrefabMode = false;
                    GenerateWheel();
                }
            };
        }
    }
    public void GenerateWheel() {
        ClearWheel();
        if (wheelSegmentPrefab) {
            int segments = numberOfSegments;
            Vector3[] wheelSegmentPositions = SpawnObjectsAroundCircleEvenly(segments, wheelCenter, 0);
            Vector3[] imageRadiusPositions = SpawnObjectsAroundCircleEvenly(segments, wheelCenter, 0);
            for (int i = 0; i < segments; i++) {
                float fillAmount = (1f / segments);
                float rotValue = RemapRange(fillAmount, 0, 1, 0, 360) * (i + 1);
                Quaternion newRot = Quaternion.Euler(0, 0, rotValue);
                Vector3 newPos = wheelSegmentPositions[i];
                Vector3 newImagePos = imageRadiusPositions[i];
                //form segments into circle
                var newWheelSegment = Instantiate(wheelSegmentPrefab, newPos, newRot, wheelCenter);
                newWheelSegment.transform.localRotation = newRot;
                newWheelSegment.fillAmount = fillAmount;
            }
        }
    }
    void ClearWheel() {
        DestroyAllChildren(wheelCenter);
    }
    public void DestroyAllChildren(Transform t) {
        for (int i = t.childCount - 1; i >= 0; i--) {
            if (Application.isPlaying) {
                GameObject.Destroy(t.GetChild(i).gameObject);
            } else {
                if (!IsPrefab()) {
                    UnityEditor.Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);
                }
            }
        }
    }
    public Vector3[] SpawnObjectsAroundCircleEvenly(int num, Transform point, float radius) {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < num; i++) {
            /* Distance around the circle */
            var radians = 2 * (float)System.Math.PI / num * i;
            /* Get the vector direction */
            var vertical = MathF.Sin(radians);
            var horizontal = MathF.Cos(radians);
            var spawnDir = new Vector3(horizontal, 0, vertical);
            /* Get the spawn position */
            var spawnPos = point.position + spawnDir * radius; // Radius is just the distance away from the point
            /* Now spawn */
            //var newObject = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
            /* Rotate the enemy to face towards player */
            // if(lookAtPos)
            // newObject.transform.LookAt(point);
            /* Adjust height */
            //newObject.transform.Translate(new Vector3(0, newObject.transform.localScale.y / 2, 0));
            result.Add(spawnPos);
        }
        return result.ToArray();
    }
    public static float RemapRange(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
}
