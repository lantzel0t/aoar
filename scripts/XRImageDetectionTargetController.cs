using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XRImageDetectionTargetController : MonoBehaviour {
  public XRImageDetectionController controller;
  public Texture2D targetTexture;
  public OnImageTargetDetected onImageTargetDetected;

  public void Start() {
    if (controller == null) {
      Debug.LogError("You have to select an XRImageDetectionController");
    } else {
      controller.SetCallbackOnTarget(targetTexture.name, onImageTargetDetected);
    }
  }

  public void MoveToTarget(XRDetectedImageTarget target) {
    transform.position = target.position;
    transform.rotation = target.rotation;
  }

}
