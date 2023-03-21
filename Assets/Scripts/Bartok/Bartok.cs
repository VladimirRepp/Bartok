using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bartok : MonoBehaviour
{
    static public Bartok S;
    static public Player CURRENT_PLAYER;

    [Header("Settings")]
    public TextAsset _deckXML;
    public TextAsset _layoutXML;
    public Vector3 _layoutCenter = Vector3.zero;
    public float _handFanDegrees = 10f;
    public int _numStartingCards = 7;
    public float _drawTimeStagger = 0.1f;

    [Header("Parameters")]
    public Deck _deck;
    public List<CardBartok> _drawPile;
    public List<CardBartok> _discardPile;
    public List<Player> _players;
    public CardBartok _targetCard;
    public TurnPhase _phase = TurnPhase.idle;

    private BartokLayout _layout;
    private Transform _layoutAnchor;

    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        _deck = GetComponent<Deck>();
        _deck.InitDeck(_deckXML.text);
        Deck.Shuffle(ref _deck._cards);

        _layout = GetComponent<BartokLayout>();
        _layout.ReadLayout(_layoutXML.text);
        _drawPile = UpgradeCardsList(_deck._cards);

        LayoutGame();
    }

    private void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _players[0].AddCard(Draw());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _players[1].AddCard(Draw());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _players[2].AddCard(Draw());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _players[3].AddCard(Draw());
        }*/
    }

    public void ArrangeDrawPile()
    {
        CardBartok tCB;

        for(int i = 0; i<_drawPile.Count; i++)
        {
            tCB = _drawPile[i];
            tCB.transform.SetParent(_layoutAnchor);
            tCB.transform.localPosition = _layout._drawPile.pos;

            tCB.faceUp = false;
            tCB.SetSortingLayerName(_layout._drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB._state = CBState.drawpile;
        }
    }

    void LayoutGame()
    {
        if(_layoutAnchor == null)
        {
            GameObject go = new GameObject("LayoutAnchor");
            _layoutAnchor = go.transform;
            _layoutAnchor.transform.position = _layoutCenter;
        }

        ArrangeDrawPile();

        Player pl;
        _players = new List<Player>();
        foreach(SlotDef s in _layout._slotDef)
        {
            pl = new Player();
            pl._handSlotDef = s;
            _players.Add(pl);
            pl._playerNum = s.player;
        }

        _players[0]._type = PlayerType.human;

        CardBartok cd;
        for(int i = 0; i< _numStartingCards; i++)
        {
            for(int j = 0; j<4; j++)
            {
                cd = Draw();
                cd._timeStart = Time.time + _drawTimeStagger * (i * 4 + j);

                _players[(j + 1) % 4].AddCard(cd);
            }
        }

        Invoke("DrawFirstTarget", _drawTimeStagger * (_numStartingCards * 4 + 4));
    }

    public void DrawFirstTarget()
    {
        CardBartok cd = MoveToTarget(Draw());
        cd._reportFinishTo = this.gameObject;
    }

    public void CBCallback(CardBartok cd)
    {
        Utils.tr("Bartok::CBCallback()", cd.name);
        StartGame();
    }

    public void StartGame()
    {
        PassTurn(1);
    }

    public void PassTurn(int num = -1)
    {
        if(num == -1)
        {
            int index = _players.IndexOf(CURRENT_PLAYER);
            num = (index + 1) % 4;
        }

        int lastPlayerNum = -1;
        if(CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER._playerNum;

            if (CheckGameOver())
                return;
        }

        CURRENT_PLAYER = _players[num];
        _phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        Utils.tr("Bartok::PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER._playerNum);
    }

    public bool CheckGameOver()
    {
        if(_drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach(CardBartok c in _discardPile)
            {
                cards.Add(c);
            }

            _discardPile.Clear();
            Deck.Shuffle(ref cards);
            _drawPile = UpgradeCardsList(cards);
            ArrangeDrawPile();
        }

        if(CURRENT_PLAYER._hand.Count == 0)
        {
            _phase = TurnPhase.gameOver;
            Invoke("RestartGame", 3);
            return true;
        }

        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__Bartok_Scene_0");
    }

    public bool ValidPlay(CardBartok cd)
    {
        if (cd._rank == _targetCard._rank)
            return true;

        if (cd._suit == _targetCard._suit)
            return true;

        return false;
    }

    public CardBartok MoveToTarget(CardBartok cd)
    {
        cd._timeStart = 0;
        cd.MoveTo(_layout._discardPile.pos + Vector3.back);
        cd._state = CBState.toTarget;
        cd.faceUp = true;

        cd.SetSortingLayerName("10");
        cd._eventualSortLayer = _layout._target.layerName;

        if(_targetCard != null)
        {
            MoveToDiscard(_targetCard);
        }

        _targetCard = cd;

        return cd;
    }

    public CardBartok MoveToDiscard(CardBartok cd)
    {
        cd._state = CBState.discard;
        _discardPile.Add(cd);
        cd.SetSortingLayerName(_layout._discardPile.layerName);
        cd.SetSortOrder(_discardPile.Count * 4);
        cd.transform.localPosition = _layout._discardPile.pos + Vector3.back/2;

        return cd;
    }

    public CardBartok Draw()
    {
        CardBartok cd = _drawPile[0];

        if(_drawPile.Count == 0)
        {
            int index;
            while(_discardPile.Count > 0)
            {
                index = Random.Range(0, _discardPile.Count);
                _drawPile.Add(_discardPile[index]);
                _discardPile.RemoveAt(index);
            }

            ArrangeDrawPile();

            float t = Time.time;
            foreach(CardBartok c in _drawPile)
            {
                c.transform.localPosition = _layout._discardPile.pos;
                c._callbackPlayer = null;
                c.MoveTo(_layout._drawPile.pos);
                c._timeStart = t;
                t += 0.02f;
                c._state = CBState.toDrawpile;
                c._eventualSortLayer = "0";
            }
        }

        _drawPile.Remove(cd);
        return cd;
    }

    List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();

        foreach(Card c in lCD)
        {
            lCB.Add(c as CardBartok);
        }

        return lCB;
    }

    public void CardClicked(CardBartok cd)
    {
        if(CURRENT_PLAYER._type != PlayerType.human)
        {
            return;
        }

        if (_phase == TurnPhase.waiting)
            return;

        switch (cd._state)
        {
            case CBState.drawpile:
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb._callbackPlayer = CURRENT_PLAYER;
                Utils.tr("Bartok.CardClicked()", "Draw", cb.name);
                _phase = TurnPhase.waiting;
                break;

            case CBState.hand:
                if (ValidPlay(cd))
                {
                    CURRENT_PLAYER.RemoveCard(cd);
                    MoveToTarget(cd);
                    cd._callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("Bartok.CardClicked()", "Play", cd.name, _targetCard.name + " is target");
                    _phase = TurnPhase.waiting;
                }
                else
                {
                    Utils.tr("Bartok.CardClicked()", "Attempted to Play " + cd.name, _targetCard.name + " is target");
                }
                break;
        }
    }
}

public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}