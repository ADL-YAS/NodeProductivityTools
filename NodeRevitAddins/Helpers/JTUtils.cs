using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins.Helpers
{
    class JTUtils
    {
        #region Geometrical Comparison
        const double _eps = 1.0e-9;

        public static bool IsZero(
          double a,
          double tolerance)
        {
            return tolerance > Math.Abs(a);
        }

        public static bool IsZero(double a)
        {
            return IsZero(a, _eps);
        }

        public static bool IsEqual(double a, double b)
        {
            return IsZero(b - a);
        }
        #endregion // Geometrical Comparison

        #region Unit conversion
        const double _feet_to_mm = 25.4 * 12;

        public static int ConvertFeetToMillimetres(
          double d)
        {
            //return (int) ( _feet_to_mm * d + 0.5 );
            return (int)Math.Round(_feet_to_mm * d,
              MidpointRounding.AwayFromZero);
        }

        public static double ConvertMillimetresToFeet(int d)
        {
            return d / _feet_to_mm;
        }

        const double _radians_to_degrees = 180.0 / Math.PI;

        public static double ConvertDegreesToRadians(int d)
        {
            return d * Math.PI / 180.0;
        }

        public static int ConvertRadiansToDegrees(
          double d)
        {
            //return (int) ( _radians_to_degrees * d + 0.5 );
            return (int)Math.Round(_radians_to_degrees * d,
              MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Return true if the type b is either a 
        /// subclass of OR equal to the base class itself.
        /// IsSubclassOf returns false if the two types
        /// are the same. It only returns true for true
        /// non-equal subclasses.
        /// </summary>
        public static bool IsSameOrSubclassOf(
          Type a,
          Type b)
        {
            // http://stackoverflow.com/questions/2742276/in-c-how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object

            return a.IsSubclassOf(b) || a == b;
        }
        #endregion // Unit conversion

        #region Formatting
        /// <summary>
        /// Uncapitalise string, i.e. 
        /// lowercase its first character.
        /// </summary>
        public static string Uncapitalise(string s)
        {
            return Char.ToLowerInvariant(s[0])
              + s.Substring(1);
        }

        /// <summary>
        /// Return an English plural suffix for the given
        /// number of items, i.e. 's' for zero or more
        /// than one, and nothing for exactly one.
        /// </summary>
        public static string PluralSuffix(int n)
        {
            return 1 == n ? "" : "s";
        }

        /// <summary>
        /// Return an English plural suffix 'ies' or
        /// 'y' for the given number of items.
        /// </summary>
        public static string PluralSuffixY(int n)
        {
            return 1 == n ? "y" : "ies";
        }

        /// <summary>
        /// Return an English pluralised string for the 
        /// given thing or things. If the thing ends with
        /// 'y', the plural is assumes to end with 'ies', 
        /// e.g. 
        /// (2, 'chair') -- '2 chairs'
        /// (2, 'property') -- '2 properties'
        /// (2, 'furniture item') -- '2 furniture items'
        /// If in doubt, appending 'item' or 'entry' to 
        /// the thing description is normally a pretty 
        /// safe bet. Replaces calls to PluralSuffix 
        /// and PluralSuffixY.
        /// </summary>
        public static string PluralString(
          int n,
          string thing)
        {
            if (1 == n)
            {
                return "1 " + thing;
            }

            int i = thing.Length - 1;
            char cy = thing[i];

            return n.ToString() + " " + (('y' == cy)
              ? thing.Substring(0, i) + "ies"
              : thing + "s");
        }

        /// <summary>
        /// Return a dot (full stop) for zero
        /// or a colon for more than zero.
        /// </summary>
        public static string DotOrColon(int n)
        {
            return 0 < n ? ":" : ".";
        }

        /// <summary>
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        /// <summary>
        /// Return a string representation in degrees
        /// for an angle given in radians.
        /// </summary>
        public static string AngleString(double angle)
        {
            return RealString(angle * 180 / Math.PI) + " degrees";
        }

        /// <summary>
        /// Return a string for a UV point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(UV p)
        {
            return string.Format("({0},{1})",
              RealString(p.U),
              RealString(p.V));
        }

        /// <summary>
        /// Return a string for an XYZ 
        /// point or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// Return a string for the XY values of an XYZ 
        /// point or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString2d(XYZ p)
        {
            return string.Format("({0},{1})",
              RealString(p.X),
              RealString(p.Y));
        }

        /// <summary>
        /// Return a string displaying the two XYZ 
        /// endpoints of a geometry curve element.
        /// </summary>
        public static string CurveEndpointString(Curve c)
        {
            return string.Format("({0},{1})",
              PointString2d(c.GetEndPoint(0)),
              PointString2d(c.GetEndPoint(1)));
        }

        /// <summary>
        /// Return a string displaying only the XY values
        /// of the two XYZ endpoints of a geometry curve 
        /// element.
        /// </summary>
        public static string CurveEndpointString2d(Curve c)
        {
            return string.Format("({0},{1})",
              PointString(c.GetEndPoint(0)),
              PointString(c.GetEndPoint(1)));
        }

        /// <summary>
        /// Return a string for a 2D bounding box
        /// formatted to two decimal places.
        /// </summary>
        public static string BoundingBoxString(
          BoundingBoxUV b)
        {
            //UV d = b.Max - b.Min;

            return string.Format("({0},{1})",
              PointString(b.Min),
              PointString(b.Max));
        }

        /// <summary>
        /// Return a string for a 3D bounding box
        /// formatted to two decimal places.
        /// </summary>
        public static string BoundingBoxString(
          BoundingBoxXYZ b)
        {
            //XYZ d = b.Max - b.Min;

            return string.Format("({0},{1})",
              PointString(b.Min),
              PointString(b.Max));
        }

        /// <summary>
        /// Return a string for an Outline
        /// formatted to two decimal places.
        /// </summary>
        public static string OutlineString(Outline o)
        {
            //XYZ d = o.MaximumPoint - o.MinimumPoint;

            return string.Format("({0},{1})",
              PointString(o.MinimumPoint),
              PointString(o.MaximumPoint));
        }
        #endregion // Formatting
    }
}
