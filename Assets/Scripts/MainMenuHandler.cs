using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FadeCallback(int param) {
        switch (param) {
            case 0:
                MyNetworkManager.startMode = NetworkCreateMode.CREATE;
                SceneManager.LoadScene(1, LoadSceneMode.Single);
                break;
            case 1:
                MyNetworkManager.startMode = NetworkCreateMode.JOIN;
                SceneManager.LoadScene(1, LoadSceneMode.Single);
                break;
        }
    }

    public void OnCreateGamePressed() {
        GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 0);
    }

    public void OnJoinGamePressed() {
        GameObject.Find("FadeScreen").GetComponent<Fader>().FadeToBlack(1.0F, FadeCallback, 1);
    }
}
