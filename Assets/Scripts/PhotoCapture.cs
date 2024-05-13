using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("Photo taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator fadingAnimation;

    private const int height = 800, width = height;
    private Texture2D screenCapture; // The photo we're capturing
    private bool viewingPhoto;
    
    private void Start()
    {
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
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
    }

    IEnumerator CapturePhoto()
    {
        // Camera UI elements remove
        viewingPhoto = true;

        yield return new WaitForEndOfFrame(); // make sure everything is rendered
         
        Rect regionToRead = new Rect(Screen.width/2-height/2, Screen.height/2-width/2, height, width); // area to "read pixels"

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply(); // expensive function
        ShowPhoto();
    }

    void ShowPhoto()
    {
        // create a sprite that will display the image
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
    }
}
