using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class WheelSegment : Image {
    public Text segmentText;
    public int segmentNumber;
    public Transform textAxis;
    public Image textBgImage;
    public void UpdateText(string prefix = ""){
        segmentText.text = prefix + (segmentNumber + 1).ToString();
    }
    
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        Vector2 localMousePos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localMousePos)) {
            return false;
        }
        Rect rect = GetPixelAdjustedRect();
        Vector2 InitialVector = new Vector2(0, rect.height);
        //Vector2 InitialVector = fillOrigin.position;
        float angle = Vector2.Angle(InitialVector, localMousePos);
        if (localMousePos.x < 0) {
            angle = 360 - angle;
        }
        float currentMouseFill = angle / 360f;
        if (currentMouseFill < fillAmount) {
            return true;
        } else {
            return false;
        }
    }
}