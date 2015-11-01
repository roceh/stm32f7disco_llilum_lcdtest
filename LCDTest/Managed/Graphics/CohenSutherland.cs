using System;
using System.Collections.Generic;

namespace Managed.Graphics
{
    /// <summary>
    /// The Cohen Sutherland line clipping algorithm
    /// http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
    /// </summary>
    public class CohenSutherland
    {
        /// <summary>
        /// Bitfields used to partition the space into 9 regiond
        /// </summary>
        private const byte INSIDE = 0; // 0000
        private const byte LEFT = 1;   // 0001
        private const byte RIGHT = 2;  // 0010
        private const byte BOTTOM = 4; // 0100
        private const byte TOP = 8;    // 1000

        /// <summary>
        /// Compute the bit code for a point (x, y) using the clip rectangle
        /// bounded diagonally by (xmin, ymin), and (xmax, ymax)
        /// ASSUME THAT xmax , xmin , ymax and ymin are global constants.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static byte ComputeOutCode(Rect extents, float x, float y)
        {
            // initialised as being inside of clip window
            byte code = INSIDE;

            if (x < extents.Left)           // to the left of clip window
                code |= LEFT;
            else if (x > extents.Right)     // to the right of clip window
                code |= RIGHT;
            if (y > extents.Bottom)         // below the clip window
                code |= BOTTOM;
            else if (y < extents.Top)       // above the clip window
                code |= TOP;

            return code;
        }

        /// <summary>
        /// Cohen–Sutherland clipping algorithm clips a line from
        /// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
        /// diagonal from (xmin, ymin) to (xmax, ymax).
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0""</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns>a list of two points in the resulting clipped line, or zero</returns>
        public static List<Point> CohenSutherlandLineClip(Rect extents, int x1, int y1, int x2, int y2)
        {
            float xf1 = x1;
            float yf1 = y1;
            float xf2 = x2;
            float yf2 = y2;

            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            byte outcode0 = CohenSutherland.ComputeOutCode(extents, xf1, yf1);
            byte outcode1 = CohenSutherland.ComputeOutCode(extents, xf2, yf2);
            bool accept = false;

            while (true)
            {
                // Bitwise OR is 0. Trivially accept and get out of loop
                if ((outcode0 | outcode1) == 0)
                {
                    accept = true;
                    break;
                }
                // Bitwise AND is not 0. Trivially reject and get out of loop
                else if ((outcode0 & outcode1) != 0)
                {
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    float x, y;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    byte outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                    if ((outcodeOut & TOP) != 0)
                    {   // point is above the clip rectangle
                        x = xf1 + (xf2 - xf1) * (extents.Top - yf1) / (yf2 - yf1);
                        y = extents.Top;
                    }
                    else if ((outcodeOut & BOTTOM) != 0)
                    { // point is below the clip rectangle
                        x = xf1 + (xf2 - xf1) * (extents.Bottom - yf1) / (yf2 - yf1);
                        y = extents.Bottom;
                    }
                    else if ((outcodeOut & RIGHT) != 0)
                    {  // point is to the right of clip rectangle
                        y = yf1 + (yf2 - yf1) * (extents.Right - xf1) / (xf2 - xf1);
                        x = extents.Right;
                    }
                    else if ((outcodeOut & LEFT) != 0)
                    {   // point is to the left of clip rectangle
                        y = yf1 + (yf2 - yf1) * (extents.Left - xf1) / (xf2 - xf1);
                        x = extents.Left;
                    }
                    else
                    {
                        x = float.NaN;
                        y = float.NaN;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        xf1 = x;
                        yf1 = y;
                        outcode0 = CohenSutherland.ComputeOutCode(extents, xf1, yf1);
                    }
                    else
                    {
                        xf2 = x;
                        yf2 = y;
                        outcode1 = CohenSutherland.ComputeOutCode(extents, xf2, yf2);
                    }
                }
            }

            // return the clipped line
            return (accept) ? new List<Point>() { new Point((int)xf1, (int)yf1), new Point((int)xf2, (int)yf2),} : null;
        }
    }
}
