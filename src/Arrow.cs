using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricFieldVis
{
    public class Arrow
    {
        public Vector2 StartPosition { get; }
        public Vector2 Direction { get; }

        public Arrow(Vector2 startPosition, Vector2 direction)
        {
            StartPosition = startPosition;
            Direction = direction;
        }
        public void DrawArrow(Graphics g, Pen pen)
        {
            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            g.DrawLine(pen, this.StartPosition.X, this.StartPosition.Y, this.StartPosition.X + this.Direction.X, this.StartPosition.Y + this.Direction.Y);
        }

    }
}
