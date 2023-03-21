using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private Text txt;

    private void Awake()
    {
        txt = GetComponent<Text>();
        txt.text = "";
    }

    private void Update()
    {
        if(Bartok.S._phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }

        if (Bartok.CURRENT_PLAYER == null)
            return;

        if(Bartok.CURRENT_PLAYER._type == PlayerType.human)
        {
            txt.text = "Победа!!!";
        }
        else
        {
            txt.text = "Конец игры!";
        }
    }
}
