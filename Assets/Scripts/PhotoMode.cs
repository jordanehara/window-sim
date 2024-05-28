using System.Collections;
using System.ComponentModel.Design;
using UnityEditor;
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
    [SerializeField] private Image cameraUIImage;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator fadingAnimation;
    #endregion

    #region Photo capture variables
    private const int photoHeight = 800, photoWidth = photoHeight; // "resolution" of the photo
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
        Cursor.visible = false; // Mouse will be moving with the photo zone

        // Set map extent
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
        // Move the photo frame UI as the new "cursor" 
        cameraUI.transform.position = ClampCameraUI(Input.mousePosition);

        if (Input.GetKey(KeyCode.Escape)) // Exit camera mode
        {
            Reset();
        }
        else if (Input.GetMouseButtonDown(0)) // Take a picture
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
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
        // Reset camera components
        RemovePhoto();
        ResetCameraPosition();

        // Reset the UI 
        Cursor.visible = true;
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
        Vector3 newZoomPosition = ClampCamera(_camera.transform.position);
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoomLevel, ref zoomVelocity, smoothTime);
        _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, newZoomPosition, ref panVelocity, smoothTime);
    }

    Vector3 ClampCameraUI(Vector3 targetPos)
    {
        var newPosX = Mathf.Clamp(targetPos.x, cameraUIImage.rectTransform.rect.width/2, Screen.width - cameraUIImage.rectTransform.rect.width/2);
        var newPosY = Mathf.Clamp(targetPos.y, cameraUIImage.rectTransform.rect.height/2, Screen.height - cameraUIImage.rectTransform.rect.height/2);
        return new Vector3(newPosX, newPosY, targetPos.z);
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
        float width = cameraUIImage.rectTransform.rect.width;
        float height = cameraUIImage.rectTransform.rect.height;
        
        cameraUI.SetActive(false);
        viewingPhoto = true;

        yield return new WaitForEndOfFrame(); // Make sure everything is rendered

        Rect regionToRead = new Rect(cameraUI.transform.position.x - width/2, cameraUI.transform.position.y - height/2, 
        width, height); // Area to "read pixels"

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply(); // Expensive function
        ShowPhoto();
    }

    void ShowPhoto()
    {
        // Create a sprite that will display the image
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, cameraUIImage.rectTransform.rect.width, cameraUIImage.rectTransform.rect.height), new Vector2(0.5f, 0.5f), 100.0f);

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
