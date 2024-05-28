using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILogic : MonoBehaviour
{
    [Header("Camera Objects")]
    [SerializeField] private GameObject CameraManager;
    [SerializeField] private GameObject PhotoFrame;

    // Start is called before the first frame update
    void Start()
    {
        CameraManager.SetActive(false);
        PhotoFrame.SetActive(false);
    }
}
