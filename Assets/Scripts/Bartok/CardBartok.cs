using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBartok : Card
{
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_HEIGHT = 3.5f;
    static public float CARD_WIDTN = 2f;

    [Header("Parameters: CardBartok")]
    public CBState _state = CBState.drawpile;

    public List<Vector3> _bezierPts;
    public List<Quaternion> _bezierRots;
    public float _timeStart, _timeDuration;
    public int _eventualSortOrder;
    public string _eventualSortLayer;

    [System.NonSerialized] 
    public Player _callbackPlayer = null;

    public GameObject _reportFinishTo = null;

    private void Update()
    {
        switch (_state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - _timeStart) / _timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);

                if(u < 0)
                {
                    transform.localPosition = _bezierPts[0];
                    transform.rotation = _bezierRots[0];
                    return;
                }
                else if(u >= 1)
                {
                    uC = 1;

                    if (_state == CBState.toHand)
                        _state = CBState.hand;

                    if (_state == CBState.toTarget)
                        _state = CBState.target;

                    if (_state == CBState.toDrawpile)
                        _state = CBState.drawpile;

                    if (_state == CBState.to)
                        _state = CBState.idle;

                    transform.localPosition = _bezierPts[_bezierPts.Count - 1];
                    transform.rotation = _bezierRots[_bezierRots.Count - 1];

                    _timeStart = 0;

                    if (_reportFinishTo != null)
                    {
                        _reportFinishTo.SendMessage("CBCallback", this);
                        _reportFinishTo = null;
                    }
                    else if (_callbackPlayer != null)
                    {
                        _callbackPlayer.CBCallback(this);
                        _callbackPlayer = null;
                    }
                    else
                    {

                    }
                }
                else
                {
                    Vector3 pos = Utils.Bezier(uC, _bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, _bezierRots);
                    transform.rotation = rotQ;

                    if (u > 0.5f)
                    {
                        SpriteRenderer sRend = _spriteRenderers[0];

                        if(sRend.sortingOrder != _eventualSortOrder) {
                            SetSortOrder(_eventualSortOrder);
                        }
                        if(sRend.sortingLayerName != _eventualSortLayer)
                        {
                            SetSortingLayerName(_eventualSortLayer);
                        }
                    }
                }
                break;
        }
    }

    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        _bezierPts = new List<Vector3>();
        _bezierPts.Add(transform.localPosition);// point from
        _bezierPts.Add(ePos);                   // point to

        _bezierRots = new List<Quaternion>();
        _bezierRots.Add(transform.rotation);// angle from
        _bezierRots.Add(eRot);              // angle to

        if(_timeStart == 0)
        {
            _timeStart = Time.time;
        }

        _timeDuration = MOVE_DURATION;
        _state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }

    public override void OnMouseUpAsButton()
    {
        Bartok.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}

public enum CBState
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}
