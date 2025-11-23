using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class CubeController : MonoBehaviour
{
    [HideInInspector]
    public float fallSpeed,dropedSpeed;
    [HideInInspector]
    public bool canDrag,rainBow, isSpecial = false;
   public GameObject GameControllerObj, Bottom;
   public Transform particleSystemObject;
       RectTransform rectTransform;

   [HideInInspector]
    public Transform spawnPosition;
    public bool canFall;
    private int dragAmmount = 1;

 public AudioClip cubeFallSong;
     private AudioSource audioSource;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false; 
        GameControllerObj = GameObject.FindGameObjectWithTag("GameController");
        Bottom = GameObject.FindGameObjectWithTag("bottom");
        canDrag = false;
        canFall = true;
        rainBow = false;
    } 
    void Start()
    {
        if(rainBow)
            StartCoroutine(RainbowCubeIE());
    }
    public void RandomCubePosition(GameObject cube)
    {
        float yRandom = UnityEngine.Random.Range(80,200);
        float xRandom = UnityEngine.Random.Range(-300,300);
        float realX = xRandom + this.transform.localPosition.x;
        realX = Mathf.Clamp(realX,-300,300);
        cube.transform.localPosition = new Vector2(realX, this.transform.localPosition.y + yRandom);
        
    }
    void Update()
    {
        fallSpeed = GameControllerObj.GetComponent<GameController>().fallSpeed + dropedSpeed;
        //Change the transform position to do the block fall
        if (canFall)
            this.transform.position = new Vector2(this.transform.position.x, this.transform.position.y - fallSpeed * Time.deltaTime);
        if (GameControllerObj.GetComponent<GameController>().currentLife < 0)
            fallSpeed = 0;
        if (IsVisible() && dragAmmount >= 1)
        {
             dragAmmount--;
            this.canDrag = true;
        } 
    }

    void OnTriggerStay2D(Collider2D otherCollider)
    {
        if(otherCollider != null)
        {
             if((otherCollider.transform.tag == "Red" || otherCollider.transform.tag == "Blue" || otherCollider.transform.tag == "Yellow"|| otherCollider.transform.tag == "Green") && canDrag && otherCollider.GetComponent<CubeController>().canDrag && GameControllerObj.GetComponent<GameController>().currentSelectedCube != otherCollider.gameObject)
        {
            bool thisBlockIsMoreClose = Vector2.Distance(this.transform.position, Bottom.transform.position) < Vector2.Distance(otherCollider.transform.position, Bottom.transform.position);
            if (thisBlockIsMoreClose)
                RandomCubePosition(otherCollider.gameObject);
        }
        }
       
    }
      public bool IsVisible()
    {
        // Pega os cantos do objeto em coordenadas de tela
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

        // Se pelo menos 1 canto está dentro da tela, considera visível
        foreach (Vector3 corner in corners)
        {
            if (screenRect.Contains(corner))
                return true;
        }

        return false;
    }
    public void FallCubeSound()
    {
        audioSource.pitch = 1.2f;
        audioSource.volume = 0.070f;
        audioSource.clip = cubeFallSong;
        audioSource.Play();
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {

        if (otherCollider.tag == this.transform.tag && otherCollider.name != "ImageContainer(Clone)")
            {
                GameControllerObj.GetComponent<GameController>().ExplodeCubeSound();
                this.GetComponent<BoxCollider2D>().enabled = false;
                otherCollider.GetComponent<BottomColorController>().particle.GetComponent<ParticleSystem>().Play();
                particleSystemObject.GetComponent<ParticleSystem>().Play();
                GameControllerObj.GetComponent<GameController>().currentScore++;
                GameControllerObj.GetComponent<GameController>().UpdateScore(this.transform);
                Destroy(this.gameObject);
                if (rainBow)
                {
                    GameControllerObj.GetComponent<GameController>().spawnRainbowCube++;
                    return;
                }
                if(this.isSpecial)
                    GameControllerObj.GetComponent<GameController>().spawnRainbowCube++;
                else
                    GameControllerObj.GetComponent<GameController>().spawAmmount++;
            }
            else if (otherCollider.tag == "PickCube")
            {
                if (canDrag)
                    GameControllerObj.GetComponent<GameController>().Cubes.Add(this.gameObject);
            }
            else if (otherCollider.name != "ImageContainer(Clone)" && otherCollider.tag != "TopCanDragTrigger")
            {
                this.GetComponent<BoxCollider2D>().enabled = false;
                GameControllerObj.GetComponent<GameController>().currentLife--;
            }
    }
    IEnumerator RainbowCubeIE()
    {
        if(rainBow)
        {
            List<String> Colors = new List<string>{"Red","Blue","Green","Yellow"};
            if(Colors.Contains(this.transform.tag))
                Colors.Remove(this.transform.tag);
            int randomColor = UnityEngine.Random.Range(0,Colors.Count);
            String tag = Colors[randomColor];
            Color color;
            Dictionary<string, string> colorMapping = new Dictionary<string, string>
            {
                { "Red", "#FF0000" },
                { "Blue", "#0000FF" },
                { "Green", "#00FF1C" },
                { "Yellow", "#FFFF00" }
            };
            string hexColor = colorMapping[tag];
            this.transform.tag = tag;
            ColorUtility.TryParseHtmlString(hexColor, out color);
            this.transform.GetChild(0).GetComponent<Image>().color = color;
            this.transform.GetChild(0).transform.tag = tag;
            yield return new WaitForSeconds(1f);
            StartCoroutine(RainbowCubeIE());
            yield break;
        }
        yield break;
    }
}
