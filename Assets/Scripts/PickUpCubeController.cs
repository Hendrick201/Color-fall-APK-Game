using Unity.VisualScripting;
using UnityEngine;

public class PickUpCubeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
       

    }

    // Update is called once per frame
    void Update()
    {
         this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, Screen.height);
        
    }
}
