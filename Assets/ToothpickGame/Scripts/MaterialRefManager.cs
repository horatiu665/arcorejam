using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MaterialRefManager : MonoBehaviour
{
    private static MaterialRefManager _instance;
    public static MaterialRefManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MaterialRefManager>();
            }
            return _instance;
        }
    }

    public Material planePlacement;
    public Material planeShadows;


}