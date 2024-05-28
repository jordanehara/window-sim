using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILogic : MonoBehaviour
{
    [SerializeField] private GameObject CameraManager;

    // Start is called before the first frame update
    void Start()
    {
        CameraManager.SetActive(false);
    }
}
