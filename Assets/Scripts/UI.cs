using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private string scene;
    private PlayerMouvement playerScript;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMouvement>();
        InvokeRepeating("UpdateLifes", 0, 0.2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGame();
        }
    }

    private void UpdateLifes()
    {
        lifeText.text = playerScript.GetLife().ToString();
    }
    
    public void PauseGame()
    {
        if (!pauseCanvas.activeInHierarchy)
        {
            Time.timeScale = 0;
            pauseCanvas.SetActive(true);
        }
        else if (pauseCanvas.activeInHierarchy)
        {
            Time.timeScale = 1.0f;
            pauseCanvas.SetActive(false);

        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(scene);
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        pauseCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
