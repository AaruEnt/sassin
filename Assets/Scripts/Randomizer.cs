using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    private static System.Random random = new System.Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Takes a percentage as a whole number 0-100 (i.e. 30 would be 30%) and returns true or false if the percent chance suceeds.
    public static bool Prob(float percent)
    {
        if (percent < 0) return false;
        if (percent >= 100) return true;
        double tmp = random.NextDouble() * 100f;
        return tmp < percent;
    }

    public static T PickRandomObject<T>(List<T> toPick)
    {
        return toPick[random.Next(toPick.Count - 1)];
    }

    // Returns a double between 0 and the input number
    public static double GetDouble(double max)
    {
        double tmp = random.NextDouble() * max;
        return tmp;
    }
}
