using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundResultUI : MonoBehaviour
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

        Player pl = Bartok.CURRENT_PLAYER;
        if (pl == null || pl._type == PlayerType.human)
        {
            txt.text = "";
        }
        else
        {
            txt.text = "Игрок " + pl._playerNum + " выйграл!";
        }
    }
}
