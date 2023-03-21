using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BartokLayout : MonoBehaviour
{
    [Header("Parameters")]
    public PT_XMLReader _xmlr;
    public PT_XMLHashtable _xml;
    public Vector2 _multiplier;
    public List<SlotDef> _slotDef;
    public SlotDef _drawPile;
    public SlotDef _discardPile;
    public SlotDef _target;

    public void ReadLayout(string xmlText)
    {
        _xmlr = new PT_XMLReader();
        _xmlr.Parse(xmlText);
        _xml = _xmlr.xml["xml"][0];

        _multiplier.x = float.Parse(_xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture); 
        _multiplier.y = float.Parse(_xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);

        SlotDef tSD;
        PT_XMLHashList slotX = _xml["slot"];

        for(int i = 0; i<slotX.Count; i++)
        {
            tSD = new SlotDef();

            if (slotX[i].HasAtt("type"))
            {
                tSD.type = slotX[i].att("type");
            }
            else
            {
                tSD.type = "slot";
            }

            tSD.x = float.Parse(slotX[i].att("x"), CultureInfo.InvariantCulture);
            tSD.y = float.Parse(slotX[i].att("y"), CultureInfo.InvariantCulture);
            tSD.pos = new Vector3(tSD.x * _multiplier.x, tSD.y * _multiplier.y, 0);

            tSD.layerID = int.Parse(slotX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch (tSD.type)
            {
                case "slot":
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotX[i].att("xstagger"), CultureInfo.InvariantCulture);                
                    break;

                case "discardpile":
                    _discardPile = tSD;
                    break;

                case "target":
                    _target = tSD;
                    break;

                case "hand":
                    tSD.player = int.Parse(slotX[i].att("player"));
                    tSD.rot = float.Parse(slotX[i].att("rot"));
                    _slotDef.Add(tSD);
                    break;
            }
        }

    }

}

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public float rot;
    public string type = "slot";
    public Vector2 stagger;
    public int player;
    public Vector3 pos;
}
