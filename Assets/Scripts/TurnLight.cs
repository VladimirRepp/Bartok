using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLight : MonoBehaviour
{
    void Update()
    {
        transform.position = Vector3.back;
        
        if (Bartok.CURRENT_PLAYER == null)
            return;

        transform.position += Bartok.CURRENT_PLAYER._handSlotDef.pos;
    }
}
