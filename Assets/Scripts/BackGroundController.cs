using Unity.VisualScripting;
using UnityEngine;

public class BackGroundController : MonoBehaviour
{
    public Transform parent,bottom;
    void Update()
    {
        this.transform.position = new Vector2(this.transform.position.x,this.transform.position.y - 50f *Time.deltaTime);
    }
}
