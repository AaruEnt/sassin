using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    private static System.Random random = new System.Random();

    // Input: an integer determining the length of the resulting string
    // Output: A string of random characters A-Z,0-9 of a length equal to the input
    public static string RandomString(int length, bool useNumbers = true)
    {
        const string chars1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string chars2 = "0123456789";
        string chars = chars1;
        if (useNumbers)
        {
            chars += chars2;
        }
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string RandomName(bool male, bool includeLast, bool includeMiddleInitial)
    {
        string res = string.Empty;

        List<string> mNames = new List<string>() { "John", "Jacob" };
        List<string> fNames = new List<string>() { "Chloe", "Ariana", "Vanessa", "Ivy", "Alyssa", "Abigail" };
        List<string> lNames = new List<string>() { "Fox", "Sutton", "Barlow", "Fitzgerald", "Craig", "McCarthy", "Burke", "Fleming" };

        if (male)
        {
            res += PickRandomObject(mNames);
        }
        else
        {
            res += PickRandomObject(fNames);
        }

        res += " ";
        if (includeMiddleInitial)
        {
            res += RandomString(1, false);
            res += " ";
        }
        if (includeLast)
        {
            res += PickRandomObject(lNames);
        }
        return res;
    }

    // Takes a percentage as a whole number 0-100 (i.e. 30 would be 30%) and returns true or false if the percent chance suceeds.
    // Usage: if (Prob(50)) {} has a 50% chance to go into the if
    public static bool Prob(float percent)
    {
        if (percent < 0) return false;
        if (percent >= 100) return true;
        double tmp = random.NextDouble() * 100f;
        return tmp < percent;
    }

    // Input: A list of any type
    // Output: A random object from the list
    public static T PickRandomObject<T>(List<T> toPick)
    {
        return toPick[random.Next(toPick.Count)];
    }

    // Input: A dictionary where the key is an object of any type, and the value is the objects random selection weight
    // Output: A random key from the dictionary, chosen using the weights
    // Example: With input { {"string1", 30}, {"string2", 5}, {"string3", 15} }
    // string1 has a 60% chance to be returned, string2 has a 10% chance to be returned, and string3 has a 30% chance to be returned
    public static T PickRandomObjectWeighted<T>(Dictionary<T, int> toPick)
    {
        List<T> list = new List<T>();
        foreach (KeyValuePair<T, int> entry in toPick)
        {
            for (int i = 0; i < entry.Value; i++)
            {
                list.Add(entry.Key);
            }
        }
        if (list.Count == 0)
            UnityEngine.Debug.LogWarning("PickObjectsWeighted called with 0 weighted entries");
        return list[random.Next(list.Count)];
    }

    // Input: A double representing the maximum number
    // Output: Returns a double between 0 and the input number
    // Example: return GetDouble(12.3);
    // would return a double between 0 and 12.3 inclusive
    public static double GetDouble(double max)
    {
        double tmp = random.NextDouble() * max;
        return tmp;
    }

    // Input: 2 doubles representing the min and max value
    // Output: A double between the min and max, inclusive
    public static double RandomRange(double min, double max)
    {
        double tmp = random.NextDouble();
        return (min + ((max - min) * tmp));
    }

    public static int RandomInt(int max, int min = 0)
    {
        return random.Next(min, max);
    }
}
