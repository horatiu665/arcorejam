using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToothpickArtChooser : MonoBehaviour
{
    [SerializeField]
    private ToothpickPlaceable _toothpick;
    public ToothpickPlaceable toothpick
    {
        get
        {
            if (_toothpick == null)
            {
                _toothpick = GetComponent<ToothpickPlaceable>();
            }
            return _toothpick;
        }
    }

    public List<GameObject> swapArt = new List<GameObject>();
    public int curSwapArt
    {
        get
        {
            return swapArt.FindIndex(go => go.activeSelf);
        }
    }

    [DebugButton]
    private void ToggleNext()
    {
        var curIndex = curSwapArt;
        for (int i = 0; i < swapArt.Count; i++)
        {
            swapArt[i].SetActive(false);
        }
        swapArt[(curIndex + 1) % swapArt.Count].SetActive(true);


        toothpick.RefreshModel();
    }
}