using System;
using System.Collections.Generic;

public static class SortedListExtensions
{
    public static void AddSorted(this List<int> list, int value)
    {
        int index = list.BinarySearch(value);
        if (index < 0)
        {
            // If the value is not found, BinarySearch returns a negative number.
            // ~index gives the insertion point.
            index = ~index;
        }
        list.Insert(index, value);
    }
}
