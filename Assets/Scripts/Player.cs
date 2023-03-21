using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public PlayerType _type = PlayerType.ai;
    public int _playerNum;
    public SlotDef _handSlotDef;
    public List<CardBartok> _hand;

    public CardBartok AddCard(CardBartok eCB)
    {
        if(_hand == null)
        {
            _hand = new List<CardBartok>();
        }

        _hand.Add(eCB);

        // Sorting cards by LINQ at human
        if(_type == PlayerType.human)
        {
            CardBartok[] cards = _hand.ToArray();

            // Call LINQ. Slow sorting by time
            cards = cards.OrderBy(cd => cd._rank).ToArray();

            _hand = new List<CardBartok>(cards);
        }

        eCB.SetSortingLayerName("10");
        eCB._eventualSortLayer = _handSlotDef.layerName;

        FanHand();
        return eCB;
    }

    public CardBartok RemoveCard(CardBartok cb)
    {
        if (_hand == null || !_hand.Contains(cb))
            return null;

        _hand.Remove(cb);
        FanHand();
        return cb;
    }

    public void FanHand()
    {
        float startRot = 0;
        startRot = _handSlotDef.rot;

        if(_hand.Count > 1)
        {
            startRot += Bartok.S._handFanDegrees * (_hand.Count - 1) / 2;
        }

        Vector3 pos;
        float rot;
        Quaternion rotQ;

        for(int i = 0; i<_hand.Count; i++)
        {
            rot = startRot - Bartok.S._handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
            pos = rotQ * pos;

            pos += _handSlotDef.pos;
            pos.z = -0.5f * i;

            if(Bartok.S._phase != TurnPhase.idle)
            {
                _hand[i]._timeStart = 0;
            }

            _hand[i].MoveTo(pos, rotQ);
            _hand[i]._state = CBState.toHand;
            _hand[i].faceUp = (_type == PlayerType.human);

            _hand[i]._eventualSortOrder = i * 4;
        }
    }

    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn()");

        if (_type == PlayerType.human)
            return;

        Bartok.S._phase = TurnPhase.waiting;
        CardBartok cb;

        List<CardBartok> validCards = new List<CardBartok>();
        foreach (CardBartok tCB in _hand)
        {
            if (Bartok.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }

        if(validCards.Count == 0)
        {
            cb = AddCard(Bartok.S.Draw());
            cb._callbackPlayer = this;
            return;
        }

        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb._callbackPlayer = this;
    }

    public void CBCallback(CardBartok cb)
    {
        Utils.tr("Player.CBCallbacl()", cb.name, "Player: " + _playerNum);
        Bartok.S.PassTurn();
    }
}

public enum PlayerType {
    human,
    ai
}
