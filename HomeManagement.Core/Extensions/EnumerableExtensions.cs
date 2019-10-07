﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeManagement.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OneElement<T>(this T value)
        {
            var enumerable = Enumerable.Empty<T>();
            enumerable.Append(value);
            return enumerable;
        }

        public static IEnumerable<int> MonthsInYear(this DateTime dateTime)
        {
            for (int i = 1; i <= 12; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<int> MonthsInYear(this DateTime dateTime, int monthLimit)
        {
            for (int i = 1; i <= monthLimit; i++)
            {
                yield return i;
            }
        }
    }
}
