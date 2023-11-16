using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakePicture : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] Image _image;

    public void TakeAPicture()
    {
        // Take A Picture
        RenderTexture captureScreen = new RenderTexture(500, 500, 24);
        _camera.targetTexture = captureScreen;
        Texture2D screenShot = new Texture2D(500, 500, TextureFormat.RGB24, false);
        _camera.Render();
        RenderTexture.active = captureScreen;
        screenShot.ReadPixels(new Rect(0, 0, 500, 500), 0, 0);
        screenShot.Apply();
        _image.sprite = Sprite.Create(screenShot, new Rect(0, 0, 500, 500), new Vector2(0.5f, 0.5f));
    }


    private void Update()
    {
        if (_leftHand != null && _rightHand != null)
        {
            Vector3 centerPos = (_leftHand.position + _rightHand.position) * 0.5f;
            Vector3 centerPosDirection = (-_leftHand.up + -_rightHand.up) * 0.05f;
            Vector3 lookAtDirection = (centerPos + centerPosDirection) - centerPos;
            Quaternion lookAtRotation = Quaternion.LookRotation(lookAtDirection);
            this.transform.position = centerPos + centerPosDirection;
            this.transform.rotation = lookAtRotation * Quaternion.Euler(-15f,0,0);
        }
    }
}
