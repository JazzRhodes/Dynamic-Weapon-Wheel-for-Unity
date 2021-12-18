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
    public bool autoUpdate = true, updateInPlaymode;
    public WheelSegment wheelSegmentPrefab;
    [Range(1, 20)] public int numberOfSegments = 8;
    public Transform wheelCenter;
    public bool centerFirstSegment = true;
    bool initialized;
    bool notInPrefabMode;
    public string segmentPrefix = "Item ";
    public bool rotateTextLocal = false, rotateImageLocal = true;
    List<WheelSegment> currentSegments;
    float fillAmount = 0, fillAmountConverted = 0;
    int segments;
    Vector3[] wheelSegmentPositions, imageRadiusPositions;
    public bool counterClockwiseLayout;
    public float segmentSize = 10;
    public bool labelWheelNumbers = true;
    void Awake() {
    }
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
        segments = numberOfSegments;
        wheelSegmentPositions = SpawnObjectsAroundCircleEvenly(segments, wheelCenter, 0);
        imageRadiusPositions = SpawnObjectsAroundCircleEvenly(segments, wheelCenter, 0);
        for (int i = 0; i < segments; i++) {
            GenerateSegments(i);
        }
        Quaternion wheelRot = Quaternion.identity;
        Quaternion centeredRot = Quaternion.identity;
        Quaternion flippedRot = Quaternion.identity;
        if (!counterClockwiseLayout) {
            flippedRot = centeredRot * Quaternion.Euler(0, 180, 0);
        }
        if (centerFirstSegment) {
            centeredRot = wheelRot * Quaternion.Euler(0, 0, -(fillAmountConverted / 2f));
        }
        Quaternion finalRotation = wheelRot * flippedRot * centeredRot;
        wheelCenter.localRotation = finalRotation;
        if (!rotateTextLocal) {
            for (int i = 0; i < currentSegments.Count; i++) {
                if (!rotateImageLocal) {
                    currentSegments[i].textBgImage.transform.rotation = Quaternion.identity;
                }
                currentSegments[i].segmentText.transform.rotation = Quaternion.identity;
            }
        } else {
            if (counterClockwiseLayout) {
                for (int i = 0; i < currentSegments.Count; i++) {
                    currentSegments[i].textBgImage.transform.localRotation = Quaternion.identity;
                    currentSegments[i].segmentText.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
    void GenerateSegments(int index) {
        if (wheelSegmentPrefab) {
            fillAmount = (1f / segments);
            float rotValue = (RemapRange(fillAmount, 0, 1, 0, 360) * (index + 1));
            fillAmountConverted = RemapRange(fillAmount, 0, 1, 0, 360);
            Quaternion newSegRot = Quaternion.Euler(0, 0, rotValue);
            Quaternion newTextRot = Quaternion.Euler(0, 0, -(fillAmountConverted / 2f));
            Vector3 newPos = wheelSegmentPositions[index];
            Vector3 newImagePos = imageRadiusPositions[index];
            //form segments into circle
            var newWheelSegment = Instantiate(wheelSegmentPrefab, newPos, newSegRot, wheelCenter);
            newWheelSegment.segmentNumber = index;
            newWheelSegment.showNumbers = labelWheelNumbers;
            newWheelSegment.UpdateText(segmentPrefix);
            newWheelSegment.fillAmount = fillAmount;
            newWheelSegment.transform.localRotation = newSegRot;
            newWheelSegment.textAxis.localRotation = newTextRot;
            Vector3 scale = Vector3.zero;
            scale.Set(segmentSize, segmentSize, segmentSize);
            newWheelSegment.transform.localScale = scale;
            currentSegments.Add(newWheelSegment);
        }
    }
    void ClearWheel() {
        DestroyAllChildren(wheelCenter);
        if (currentSegments != null) {
            currentSegments.Clear();
        } else {
            currentSegments = new List<WheelSegment>();
        }
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
