using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestBigNumber : MonoBehaviour
{

    public BigNumber numA;
    public BigNumber numB;
    public BigNumber numC;

    public bool testMinifyA;
    public bool testAddAB;
    public bool testSubtractCB;

    private void Update()
    {
        if (testMinifyA)
        {
            testMinifyA = false;
            var s = numA.ToString();
            numA.Minify();
            numB.Minify();
            numC.Minify();
            s += " = " + numA.ToString();
            Debug.Log(s);
        }

        if (testAddAB)
        {
            testAddAB = false;
            var s = numA.ToString() + " + " + numB.ToString() + " = ";
            numC = numA.Add(numB);
            s += numC;
            Debug.Log(s);

        }

        if (testSubtractCB)
        {
            testSubtractCB = false;

            var s = numC.ToString() + " - " + numB.ToString() + " = ";
            numA = numC.Subtract(numB);
            s += numA;
            Debug.Log(s);
        }
    }
}