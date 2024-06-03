using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LilDudes : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spriteName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move the lil dude name to above the lil dude
        Vector3 spritePosition = this.gameObject.transform.position;
        float spriteHeight = this.gameObject.GetComponent<RectTransform>().rect.height;
        spriteName.transform.position = new Vector3(spritePosition.x, spritePosition.y + spriteHeight/2, spritePosition.z);
    }
}
