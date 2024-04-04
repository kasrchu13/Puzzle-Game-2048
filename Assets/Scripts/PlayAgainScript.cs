using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayAgainScript : MonoBehaviour
{
    public void GoToScene(string sceneName){
        gameObject.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }
}
