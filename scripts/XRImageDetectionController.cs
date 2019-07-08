using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class OnImageTargetDetected : UnityEvent<XRDetectedImageTarget>{}

public class XRImageDetectionController : MonoBehaviour {
  private XRController xr_;
  private Dictionary<string, OnImageTargetDetected> targetToCallback_ = new Dictionary<string, OnImageTargetDetected>();

  // A UI Text element to describe the status of the image detection: how many is being performed,
  // what has been detected ...
  public UnityEngine.UI.Text statusText;
  // The list of textures to detect. Their texture names have to be unique.
  public List<XRDetectionTexture> detectionTextures;

  public void Start() {
    xr_ = GameObject.FindWithTag("XRController").GetComponent<XRController>();

    if (detectionTextures.Count > 0) {
      Dictionary<string, XRDetectionImage> detectionImages =
        new Dictionary<string, XRDetectionImage>();
      foreach (XRDetectionTexture detectionTexture in detectionTextures) {
        detectionImages.Add(
          detectionTexture.tex.name, XRDetectionImage.FromDetectionTexture(detectionTexture));
      }
      xr_.SetDetectionImages(detectionImages);
  
    }
  }

  public void Update() {
    List<XRDetectedImageTarget> detectedTargets = xr_.GetDetectedImageTargets();
    SetStatusText(detectedTargets);
    foreach (XRDetectedImageTarget target in detectedTargets) {
      if (targetToCallback_.ContainsKey(target.name)) {
        OnImageTargetDetected cb = targetToCallback_[target.name];
        if (cb != null) {
          cb.Invoke(target);
        }
      }
    }
  }

  private void SetStatusText(List<XRDetectedImageTarget> foundTargets) {
    if (statusText != null) {
      string statusTextStr = "Image Detection found " + foundTargets.Count +  " of " + detectionTextures.Count + "\n";
      foreach (XRDetectedImageTarget target in foundTargets) {
        statusTextStr += target.name + "\n";
      }
      statusText.text = statusTextStr;
    }
  }

  public void SetCallbackOnTarget(string targetName, OnImageTargetDetected callback) {
    targetToCallback_.Add(targetName, callback);
  }
}
