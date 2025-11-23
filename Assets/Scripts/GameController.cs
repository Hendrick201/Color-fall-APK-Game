using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    [Header("Spawn Objects")]
    public Transform spawnPosition;
    public int spawAmmount;
    
    public int spawnRainbowCube;
    public float increaseRainbowCubeTimer = 20f;
    public Canvas gameCanvas;
    [Header("Time Controll")]  
    public float cubeSpawnTimer;
    [Header("Prefabs")]   
    public GameObject prefabCube;
    [Header("Cubes")]   
    public GameObject Green,Blue,Yellow,Red;
    public float fallSpeed, maxFallSpeed;
    private bool endGame = false;
   [HideInInspector]
    public float currentGameTime;

    public GameObject CubPicker,Bottom,GameOverPanel, IncreasedScore;

    public List<GameObject> Cubes = new List<GameObject>();
    public bool canPickCubes = false,increase;
     public float maxSizePickCube;
     public GameObject currentSelectedCube, cubesContainer;

     public TextMeshProUGUI text,gameOverScoreText,gameOverRecordText;
     public int currentScore = 0;
     [HideInInspector]
     public int currentLife;
     private int record;
      private const string RecordKey = "record"; // Chave para identificar o valor de 'record'
    public AudioClip firstAudioSource,secondAudioSource,thirdAudioSource,fullSong,gameOverSong,pickCubeSong;
     private AudioSource audioSource;
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false; 
        Screen.orientation = ScreenOrientation.Portrait;
        record = LoadRecord();
        currentLife = 1;
        text.text = currentScore.ToString();
        CubPicker.SetActive(false);
        StartCoroutine(IncreaseFallSpeed());
        maxSizePickCube = CubPicker.GetComponent<RectTransform>().sizeDelta.y;
        StartCoroutine(IncreaseCubeAmmount());
        StartCoroutine(IncreaseRainbowCubeAmmount());
    }
    public int LoadRecord()
    {
        if (PlayerPrefs.HasKey(RecordKey)) // Verifica se o 'record' existe
        {
            int loadedRecord = PlayerPrefs.GetInt(RecordKey);
            return loadedRecord;
        }
        else
        {
            return 0; // Valor padrão caso o record não exista
        }
    }

    void Save()
    {
        if(record < currentScore)
        {
             PlayerPrefs.SetInt(RecordKey, currentScore);
        PlayerPrefs.Save(); // Garante que os dados sejam salvos no disco
        }
       
    }
    public void UpdateScore(Transform cubePosition)
    {
        IncreasedScore.transform.position = new Vector2(cubePosition.transform.position.x + 0.4f, IncreasedScore.transform.position.y);
       // StartCoroutine(ScoreUpAnimationTimerController());
        text.text = currentScore.ToString();
    }
    IEnumerator ScoreUpAnimationTimerController()
    {
        IncreasedScore.SetActive(true);
        yield return new WaitForSeconds(0.3f);
         IncreasedScore.SetActive(false);
    }
    void Update()
    {
        currentGameTime += Time.deltaTime;

        //Chama o metodo que cria os cubos N vezes
        for(int i = 0; i < spawAmmount; i ++)
        {
            SpawnCube();
        }
        PickUpCube();
        if(currentLife <= 0)
        {
           EndGame();
        }
        if(currentScore < 0)
        {
            currentScore = 0;
        }


        
    }
    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void EndGame()
    {
        if (endGame == false)
        {
            endGame = true;
            audioSource.clip = gameOverSong;
            audioSource.pitch = 0.8f;
            audioSource.ignoreListenerPause = true;
            audioSource.PlayOneShot(gameOverSong);

            if (record < currentScore)
            {
                gameOverRecordText.text = currentScore.ToString();
            }
            else
            {
                gameOverRecordText.text = record.ToString();
            }
            fallSpeed = 0f;
            GameOverPanel.SetActive(true);
            gameOverScoreText.text = text.text;
            Save(); 
        
        }
      
    }
    void PickUpCube()
    {
        if (Input.GetMouseButtonDown(0) && currentLife > 0)
        {
            CubPicker.SetActive(true);
            Cubes = new List<GameObject>();
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CubPicker.transform.position = new Vector2(position.x, CubPicker.transform.position.y);
        }      
          MoveCube();
}
void MoveCube()
{
        if (Cubes.Count > 0)
        {
            PickUpCubeSound();  /// Ativa o som de pegar o cubo
            currentSelectedCube = Cubes[0]; // Inicia com o primeiro item
            float distance = Vector3.Distance(Cubes[0].transform.position, CubPicker.transform.GetChild(0).transform.position);

            for (int i = 1; i < Cubes.Count; i++)
            {
                if (Cubes[i] != null)
                {
                    float newDistance = Vector3.Distance(Cubes[i].transform.position, CubPicker.transform.GetChild(0).transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        currentSelectedCube = Cubes[i];
                    }
                }
                 else
                    CubPicker.SetActive(false); 
            }
        }
       
     if (Input.GetMouseButton(0) && currentSelectedCube != null) // Detecta se o botão do mouse está pressionado
        {
            CubPicker.SetActive(false);
            currentSelectedCube.GetComponent<CubeController>().rainBow = false;
            currentSelectedCube.transform.SetAsLastSibling();
            if (currentSelectedCube != null)
            {
                Cubes = new List<GameObject>();
                currentSelectedCube.GetComponent<CubeController>().canDrag = false;

                // Obtém a posição do mouse na tela
                Vector2 mousePos = Input.mousePosition;

                // Obtém as dimensões do cubo (em pixels) no Canvas
                RectTransform cubeRect = currentSelectedCube.GetComponent<RectTransform>();
                float cubeWidth = cubeRect.rect.width;
                float cubeHeight = cubeRect.rect.height;

                // Limites da tela (em pixels)
                float canvasWidth = Screen.width;

                // Limita a posição do mouse dentro dos limites do Canvas, somando metade da largura/altura do cubo para mantê-lo dentro da tela
                mousePos.x = Mathf.Clamp(mousePos.x, 0 + (cubeWidth / 4 - 7f), canvasWidth - (cubeWidth / 4 - 7f));  // Limita a posição X

                // Atualiza a posição do cubo dentro do Canvas (convertendo de tela para coordenadas do mundo)
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
                currentSelectedCube.transform.position = new Vector3(worldPosition.x, currentSelectedCube.transform.position.y, currentSelectedCube.transform.position.z);

                // Não permite que o cubo caia ainda
                currentSelectedCube.GetComponent<CubeController>().canFall = false;
            }


        }
    if(Input.GetMouseButtonUp(0))
    {
        if(currentSelectedCube != null)
        {
            currentSelectedCube.GetComponent<CubeController>().canFall = true;
            if(currentSelectedCube != null && currentSelectedCube.GetComponent<CapsuleCollider2D>() != null)
            currentSelectedCube.GetComponent<CapsuleCollider2D>().enabled = true;
            currentSelectedCube.GetComponent<BoxCollider2D>().enabled = false;
            currentSelectedCube.GetComponent<CubeController>().dropedSpeed =  currentSelectedCube.GetComponent<CubeController>().fallSpeed *10;
            currentSelectedCube = null;
            Cubes = new List<GameObject>();
            
        }
           
    }
}
    public void PickUpCubeSound()
    {

        audioSource.pitch = 0.9f;
        audioSource.volume = 0.15f;
        audioSource.clip = pickCubeSong;
        audioSource.Play();
    }
    public void ExplodeCubeSound()
    {
            // Acessa o AudioSource do objeto principal da câmera e atribui seu áudio ao AudioSource atual
        float random = Random.Range(-0.08f, + 0.08f);
         Camera.main.GetComponent<AudioSource>().pitch = 0.97f + random;
        Camera.main.GetComponent<AudioSource>().volume = 0.240f;
        Camera.main.GetComponent<AudioSource>().Play();

    }


    void SpawnCube()
    {       
        GameObject cube = Instantiate(prefabCube,gameCanvas.transform);
         cube.transform.SetParent(cubesContainer.transform);
        float xRandom = Random.Range(-200, 200);
        float yRandom = Random.Range(0, 20);

        Vector2 localPos = new Vector2(
            spawnPosition.localPosition.x + xRandom,
            spawnPosition.localPosition.y + yRandom
        );

        cube.transform.localPosition = localPos;
        int randomColorSelector = Random.Range(0,4);
        if(spawnRainbowCube >0)
        {
             cube.GetComponent<CubeController>().rainBow = true;
             cube.GetComponent<CubeController>().isSpecial = true;
              cube.transform.tag = "Green";
            cube.transform.GetChild(0).tag = "Green";
             cube.transform.GetChild(0).GetComponent<Image>().color = Color.green;
             spawnRainbowCube--;
             return;
        }
        switch( randomColorSelector)
        {
            case 0:
            cube.transform.tag = "Green";
            cube.transform.GetChild(0).tag = "Green";
             cube.transform.GetChild(0).GetComponent<Image>().color = Color.green;
            break;
            case 1:
            cube.transform.tag = "Blue";
            cube.transform.GetChild(0).tag = "Blue";
            cube.transform.GetChild(0).GetComponent<Image>().color = Color.blue;
            break;
            case 2:
            cube.transform.tag = "Yellow";
            cube.transform.GetChild(0).tag = "Yellow";
            cube.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
            break;
            case 3:
            cube.transform.tag = "Red";
            cube.transform.GetChild(0).tag = "Red";
            cube.transform.GetChild(0).GetComponent<Image>().color = Color.red;
            break;
        }
        spawAmmount--;
    }
        IEnumerator IncreaseFallSpeed()
    {
        if(fallSpeed < maxFallSpeed)
        {
        yield return new WaitForSeconds(1f);
        fallSpeed += fallSpeed *0.019f;
        StartCoroutine(IncreaseFallSpeed());
        }
       
        yield break;
    }
    IEnumerator IncreaseCubeAmmount()
    {
          yield return new WaitForSeconds(cubeSpawnTimer);
          spawAmmount++;
          StartCoroutine(IncreaseCubeAmmount());
          yield break;
    }
    IEnumerator IncreaseRainbowCubeAmmount()
    {
          yield return new WaitForSeconds(increaseRainbowCubeTimer);
          spawnRainbowCube++;
          StartCoroutine(IncreaseRainbowCubeAmmount());
          yield break;
    }
}
