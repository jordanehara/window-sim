using System.Collections;
using System.ComponentModel.Design;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    #region Unity object input fields
    [Header ("General Objects")]
    [SerializeField] private Camera _camera;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private GameObject UIOverlay;
    [SerializeField] private GameObject cameraManager;

    [Header("Photo taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;
    [SerializeField] private GameObject cameraUI;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator fadingAnimation;
    #endregion

    #region Photo capture variables
    private const int photoHeight = 800, photoWidth = photoHeight;
    private Texture2D screenCapture; // The photo we're capturing
    private bool viewingPhoto;
    #endregion

    #region Camera movement variables
    private Vector3 cameraOriginalPosition;
    private float zoomLevel;
    private float zoomMultiplier = 4f;
    private float minZoomLevel = 2f;
    private float maxZoomLevel = 5f;
    private float zoomVelocity = 0f;
    private Vector3 panVelocity = Vector3.zero;
    private float smoothTime = 0.1f;
    private Vector3 dragOrigin;
    private float mapMinX, mapMinY, mapMaxX, mapMaxY;

    #endregion

    private void Start()
    {
        // set map extent
        mapMinX = background.transform.position.x - background.bounds.size.x / 2f;
        mapMaxX = background.transform.position.x + background.bounds.size.x / 2f;

        mapMinY = background.transform.position.y - background.bounds.size.y / 2f;
        mapMaxY = background.transform.position.y + background.bounds.size.y / 2f;

        cameraOriginalPosition = _camera.transform.position;
        zoomLevel = _camera.orthographicSize - 1f;
        screenCapture = new Texture2D(photoWidth, photoHeight, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) // Exit camera mode
        {
            Reset();
        }
        else if (Input.GetMouseButtonDown(0)) // Take a picture
        {
            if (!viewingPhoto)
                StartCoroutine(CapturePhoto());
            else
                RemovePhoto();
        }
        else // Zoom or pan the background
        {
            if (!viewingPhoto)
                MoveCamera();
        }
    }

    void Reset()
    {
        RemovePhoto();
        ResetCameraPosition();

        // Reset the UI 
        cameraManager.SetActive(false);
        UIOverlay.SetActive(true);
    }

    #region Camera movement
    void MoveCamera()
    {
        // Panning
        if (Input.GetMouseButtonDown(1))
        {
            // Save the position of the mouse in the space when drag starts
            dragOrigin = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        // Calculate distance between drag origin and new position
        if (Input.GetMouseButton(1))
        {
            Vector3 diff = dragOrigin - _camera.ScreenToWorldPoint(Input.mousePosition);

            // Move the camera
            Vector3 newPosition = _camera.transform.position + diff;
            newPosition = ClampCamera(newPosition);
            _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, newPosition, ref panVelocity, smoothTime);
        }


        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoomLevel -= scroll * zoomMultiplier;
        zoomLevel = Mathf.Clamp(zoomLevel, minZoomLevel, maxZoomLevel);
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoomLevel, ref zoomVelocity, smoothTime);
    }

    Vector3 ClampCamera(Vector3 targetPos)
    {
        float camHeight = _camera.orthographicSize;
        float camWidth = _camera.orthographicSize * _camera.aspect;

        float minX = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;

        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(targetPos.x, minX, maxX);
        float newY = Mathf.Clamp(targetPos.y, minY, maxY);

        return  new Vector3(newX, newY, targetPos.z);
    }

    void ResetCameraPosition()
    {
        _camera.transform.position = cameraOriginalPosition;
        _camera.orthographicSize = maxZoomLevel;
        zoomLevel = _camera.orthographicSize - 1f;
    }
    #endregion

    #region Photo methods
    IEnumerator CapturePhoto()
    {
        cameraUI.SetActive(false);
        viewingPhoto = true;

        yield return new WaitForEndOfFrame(); // make sure everything is rendered
         
        Rect regionToRead = new Rect(Screen.width/2-photoHeight/2, Screen.height/2-photoWidth/2, photoHeight, photoWidth); // area to "read pixels"

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply(); // expensive function
        ShowPhoto();
    }

    void ShowPhoto()
    {
        // Create a sprite that will display the image
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, photoHeight, photoWidth), new Vector2(0.5f, 0.5f), 100.0f);

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
    #endregion
}
