using System;
using System.Drawing;

namespace Rextester
{
    public class LineBuilder
    {
        public LineBuilder(Point p1, Point p2)
        {
            cur = this.p1 = p1;
            this.p2 = p2;

            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;

            sy = dy < 0 ? 1 : -1;
            sx = dx < 0 ? 1 : -1;

            if (dx == 0)
            {
                if (dy == 0)
                {
                    run = false;
                }
                NextPoint = VerticalCase;
            }
            else if (dy == 0)
            {
                NextPoint = HorizontalCase;
            }
            else if (Math.Abs(dx) == Math.Abs(dy))
            {
                NextPoint = DiagonalCase;
            }
            else
            {
                NextPoint = GeneralCase;
                int a = -dy;
                int b = dx;

                int asx = a * sx;
                int bsy = b * sy;

                delta_x = new int[]{sx, 0, sx};
                delta_y = new int[]{0, sy, sy};
                delta_e = new int[]{asx, bsy, asx+bsy};
            }
        }

        public delegate bool NextPointGetter(ref Point pnt);

        public NextPointGetter NextPoint;

        private bool VerticalCase(ref Point pnt)
        {
            if (!run)
            {
                return false;
            }

            cur.Y += sy;
            pnt = cur;

            if (cur.Y == p2.Y)
            {
                run = false;
            }

            return true;
        }

        private bool HorizontalCase(ref Point pnt)
        {
            if (!run)
            {
                return false;
            }

            cur.X += sx;
            pnt = cur;

            if (cur.X == p2.X)
            {
                run = false;
            }

            return true;
        }

        private bool DiagonalCase(ref Point pnt)
        {
            if (!run)
            {
                return false;
            }

            cur.X += sx;
            cur.Y += sy;
            pnt = cur;

            if (cur.X == p2.X)
            {
                run = false;
            }

            return true;
        }

        private bool GeneralCase(ref Point pnt)
        {
            if (!run)
            {
                return false;
            }

            int way_idx = 0;
            int min_err = err + delta_e[0];
            int cur_err;

            for (int i = 1; i < 3; ++i)
            {
                cur_err = err + delta_e[i];
                if (Math.Abs(cur_err) < Math.Abs(min_err))
                {
                    way_idx = i;
                    min_err = cur_err;
                }
            }

            cur.X += delta_x[way_idx];
            cur.Y += delta_y[way_idx];
            err = min_err;
            pnt = cur;

            if (cur.X == p2.X && cur.Y == p2.Y)
            {
                run = false;
            }

            return true;
        }

        private bool run = true;

        // Границы отрезка
        private Point p1, p2;

        // Текущее положение
        private Point cur;

        // Смещения вдоль осей
        private int sx, sy;

        // Начальное значение ошибки
        private int err = 0;

        private int[] delta_x;
        private int[] delta_y;
        private int[] delta_e;
    }

    public class TriangleBuilder
    {
        public TriangleBuilder(Point p1, Point p2, Point p3)
        {
            Point[] ary = {p1, p2, p3};
            int top_idx = 0;
            int bottom_idx = 0;

            for (int i = 1; i < 3; ++i)
            {
                if (ary[top_idx].Y < ary[i].Y)
                {
                    top_idx = i;
                }
                else if (ary[bottom_idx].Y > ary[i].Y)
                {
                    bottom_idx = i;
                }
            }

            this.cur_second = this.cur_first = this.current = this.top = ary[top_idx];
            this.bottom = ary[bottom_idx];
            this.middle = ary[3 - top_idx - bottom_idx];

            this.first_side = new LineBuilder(this.top, this.bottom);
            this.second_side = new LineBuilder(this.top, this.middle);
            this.horizontal = new LineBuilder(this.top, this.top);
        }

        public bool NextPoint(ref Point pnt)
        {
            if (!this.run)
            {
                return false;
            }

            pnt = this.current;
            if (this.horizontal.NextPoint(ref this.current))
            {
                return true;
            }

            if (this.current.Y == this.bottom.Y)
            {
                run = false;
                return false;
            }

            Point first_point = new Point();
            do
            {
                if (!first_side.NextPoint(ref first_point))
                {
                    first_side = new LineBuilder(this.middle, this.bottom);
                    first_point = this.middle;
                }
            }
            while (first_point.Y == this.current.Y);

            Point second_point = new Point();
            do
            {
                if (!second_side.NextPoint(ref second_point))
                {
                    second_side = new LineBuilder(this.middle, this.bottom);
                    second_point = this.middle;
                }
            }
            while(second_point.Y == this.current.Y);

            this.horizontal = new LineBuilder(first_point, second_point);
            this.current = first_point;
            return true;
        }

        private Point top;
        private Point middle;
        private Point bottom;

        private Point current;
        private Point cur_first;
        private Point cur_second;

        private bool run = true;

        private LineBuilder first_side = null;
        private LineBuilder second_side = null;
        private LineBuilder horizontal = null;
    }
}