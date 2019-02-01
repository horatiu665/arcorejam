using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct BigNumber
{
    [SerializeField]
    [Tooltip("Positive, range: [0, 1000)")]
    private double _value;
    public double value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
        }
    }

    /// <summary>
    /// BASE (1000) ^ n
    /// </summary>
    public enum Orders
    {
        Zero,
        Km,
        Thousand,
        Million,
        Billion,
        Trillion,
        Quadrillion,
        Quintillion,
        Sextillion,
        Septillion,
        Octillion,
        Nonillion,
        Decillion,
        Undecillion,
        Duodecillion,
        Tredecillion,
        Quattuordecillion,
        Quindecillion,
        Sexdecillion,
        Septendecillion,
        Octodecillion,
        Novemdecillion,
        Vigintillion,
    }

    public BigNumber Multiply(double deltaTime)
    {
        var n = new BigNumber(this);
        n.value *= deltaTime;
        n.Minify();
        return n;
    }

    public Orders order;
    private int orderInt => (int)order;

    // should be private and always called on big number operations.
    public void Minify()
    {
        while (value >= 1000 && (int)order < 21)
        {
            value /= 1000;
            order += 1;
        }
        if (value < 0)
        {
            value = 0;
            order = 0;
        }
        while (value <= 0.0001 && (int)order > 0)
        {
            value *= 1000;
            order -= 1;
        }
    }

    public BigNumber(BigNumber other)
    {
        _value = other.value;
        order = other.order;
    }

    public BigNumber Add(BigNumber other)
    {
        var n = new BigNumber();

        // find the minimum order of magnitude and convert values...
        Orders newOrder;
        double newValue;
        if (this.orderInt < other.orderInt)
        {
            newOrder = this.order;
            newValue = this.value;
            int delta = other.orderInt - this.orderInt;
            newValue += Math.Pow(1000, delta) * other.value;
        }
        else
        {
            newOrder = other.order;
            newValue = other.value;
            int delta = this.orderInt - other.orderInt;
            newValue += Math.Pow(1000, delta) * this.value;
        }

        n.value = newValue;
        n.order = newOrder;
        n.Minify();

        return n;
    }

    public BigNumber Subtract(BigNumber other)
    {
        var otherMinus = new BigNumber(other);
        otherMinus.value *= -1;
        return this.Add(otherMinus);
    }

    public override string ToString()
    {
        var valueInt = Math.Floor(value);
        var valueDec = Math.Floor((value - valueInt) * 10) / 10;
        // format is 0.0 except if value is <10 and no decimal ;)
        if (order == Orders.Zero)
        {
            var s = valueInt;
            return s.ToString("F0");
        }
        else if (order == Orders.Km)
        {
            var s = string.Format("{0:0.0}", valueInt + valueDec);
            s += " km";
            return s.ToString();
        }
        else
        {
            var s = string.Format("{0:0.0}", valueInt + valueDec);
            s += " " + order.ToString() + " km";
            //if (value != 1)
            //    s += "s";
            return s;
        }
    }

}