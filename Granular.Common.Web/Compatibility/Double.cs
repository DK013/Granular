﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Granular.Compatibility
{
    public static class Double
    {
        //[ws][$][sign][integral-digits[,]]integral-digits[.[fractional-digits]][E[sign]exponential-digits][ws]
        private static readonly Granular.Compatibility.Regex DoubleFormat = new Granular.Compatibility.Regex(@"^[+-]?([0-9]*,)*[0-9]*(\.([0-9]*))?([eE]([+-]?)[0-9]*)?$");

        public static bool TryParse(string s, out double result)
        {
            if (DoubleFormat.Match(s) == null)
            {
                result = System.Double.NaN;
                return false;
            }

            result = System.Double.Parse(s);
            return true;
        }

        public static bool IsInfinity(double d)
        {
            return !System.Double.IsFinite(d);
        }
    }
}
