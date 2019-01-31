using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class scaler : MonoBehaviour
{
    public ToothpickPlacerTest placer;
    public GameObject scalerbutton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (placer.highlighted.Count > 0)
        {
            scalerbutton.SetActive(true);
        }
        else
        {
            scalerbutton.SetActive(false);
        }
    }
}
