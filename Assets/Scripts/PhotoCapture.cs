using System.Collections;
using System.ComponentModel.Design;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    #region Unity object input fields
    [Header ("Camera")]
    [SerializeField] private Camera camera;

    [Header("Photo taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;
    [SerializeField] private GameObject cameraUI;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator fadingAnimation;
    #endregion

    #region Photo capture variables
    private const int height = 800, width = height;
    private Texture2D screenCapture; // The photo we're capturing
    private bool viewingPhoto;
    #endregion

    #region Camera zoom variables
    private float zoomLevel;
    private float zoomMultiplier = 4f;
    private float minZoomLevel = 2f;
    private float maxZoomLevel = 5f;
    private float velocity = 0f;
    private float smoothTime = 0.1f;

    #endregion

    private void Start()
    {
        zoomLevel = camera.orthographicSize;
        screenCapture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!viewingPhoto)
                StartCoroutine(CapturePhoto());
            else
                RemovePhoto();
        }
        else
        {
            if (!viewingPhoto)
                Zoom();
        }
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoomLevel -= scroll * zoomMultiplier;
        zoomLevel = Mathf.Clamp(zoomLevel, minZoomLevel, maxZoomLevel);
        camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, zoomLevel, ref velocity, smoothTime);
    }

    IEnumerator CapturePhoto()
    {
        cameraUI.SetActive(false);
        viewingPhoto = true;

        yield return new WaitForEndOfFrame(); // make sure everything is rendered
         
        Rect regionToRead = new Rect(Screen.width/2-height/2, Screen.height/2-width/2, height, width); // area to "read pixels"

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply(); // expensive function
        ShowPhoto();
    }

    void ShowPhoto()
    {
        // Create a sprite that will display the image
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, height, width), new Vector2(0.5f, 0.5f), 100.0f);

        // Set where the photo area is going to be and spawn the sprite
        photoDisplayArea.sprite = photoSprite;
        photoFrame.SetActive(true);
        fadingAnimation.Play("photoFade");
    }

    void RemovePhoto()
    {
        viewingPhoto = false;
        photoFrame.SetActive(false);
        cameraUI.SetActive(true);
    }
}
