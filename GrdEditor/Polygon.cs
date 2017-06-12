using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bresenham
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

                delta_x = new int[] { sx, 0, sx };
                delta_y = new int[] { 0, sy, sy };
                delta_e = new int[] { asx, bsy, asx + bsy };
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
            Point[] ary = { p1, p2, p3 };
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

            this.current = this.top = ary[top_idx];
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

            Point hlp = new Point();
            bool got;
            if (!this.first_on_next_line)
            {
                got = this.first_side.NextPoint(ref hlp);
                if (!got)
                {
                    // закончили обход; больше нам тут делать нечего
                    this.first_is_done = this.first_on_next_line = true;
                }

                else if (hlp.Y != this.current.Y)
                {
                    // перешли на следующий левел
                    // запомним этот момент и пойдём ко второй прямой
                    this.first_on_next_line = true;
                    this.cur_first = hlp;
                }
                else
                {
                    // точка получена, но мы остались на том же уровне
                    // вернём эту точку в следующий заход
                    this.current.X = hlp.X;
                    return true;
                }
            }
            if (this.first_on_next_line)
            {
                got = this.second_side.NextPoint(ref hlp);
                if (!got)
                {
                    if (this.first_is_done)
                    {
                        // закончили и тут, и на первой
                        // обход закончен
                        this.run = false;
                        return true;
                    }
                    else
                    {
                        // просто надо переключиться на следующую сторону:
                        this.second_side = new LineBuilder(this.middle, this.bottom);
                        // по-идее с this.middle мы уже разобрались, поэтому
                        // берём следующую точку
                        got = this.second_side.NextPoint(ref hlp);

                        if (!got)
                        {
                            // хз, будем ли попадать сюда, но...
                            return true;
                        }
                    }
                }
                // очередная точка получена

                if (hlp.Y == this.current.Y)
                {
                    // остались на том же уровне
                    // сюда мы по-идее попадём только если на первой уже
                    // на следующей линии или даже закончили, поэтому
                    this.current.X = hlp.X;
                }
                else
                {
                    // если мы апнулись тут, то и на первой тоже, поэтому
                    // у нас новая горизонталька
                    this.horizontal = new LineBuilder(this.cur_first, hlp);
                    this.current = this.cur_first;
                    this.first_on_next_line = false;
                }
                return true;
            }
            return true;
        }

        private Point top;
        private Point middle;
        private Point bottom;

        private Point current;
        private Point cur_first;
        private bool first_on_next_line = false;
        private bool first_is_done = false;
        private bool last_sent = false;

        private bool run = true;

        private LineBuilder first_side = null;
        private LineBuilder second_side = null;
        private LineBuilder horizontal = null;
    }

    public class PolygonBuilder
    {
        public PolygonBuilder(Point[] points)
        {
            int points_len = points.Length;
            if (points == null)
            {
                throw new Exception("Argument \"points\" is null!");
            }
            else if (points_len < 3)
            {
                throw new Exception("Length of \"points\" is too short!");
            }

            this.points = points;

            int i;
            int x_cur, y_cur;

            this.x_min = this.x_max = points[0].X;
            this.y_min = this.y_max = points[0].Y;

            for (i = 1; i < points_len; ++i)
            {
                x_cur = points[i].X;
                y_cur = points[i].Y;

                if (this.x_min > x_cur) this.x_min = x_cur;
                else if (this.x_max < x_cur) this.x_max = x_cur;

                if (this.y_min > y_cur) this.y_min = y_cur;
                else if (this.y_max < y_cur) this.y_max = y_cur;
            }

            this.width = 1 + (++this.x_max) - (--this.x_min);
            this.height = 1 + (++this.y_max) - (--this.y_min);

            int pixel_count = this.width * this.height;

            this.status = new byte[pixel_count];
            for (i = 0; i < pixel_count; ++i)
            {
                this.status[i] = INSIDE;
            }
            this.SetBorders();
            this.Explore();
            this.SetFirst();
        }

        public bool NextPoint(ref Point pnt)
        {
            if (this.cur_row > this.y_max) return false;

            pnt.X = this.cur_column;
            pnt.Y = this.cur_row;

            while (true)
            {
                if (++this.cur_column > this.x_max)
                {
                    this.cur_column = this.x_min + 1;
                    if (++this.cur_row > this.y_max)
                    {
                        break;
                    }
                }

                if (this.GetPixelStatus(new Point(this.cur_column, this.cur_row)) != OUTSIDE) break;
            }

            return true;
        }

        private void SetBorders()
        {
            int last_idx = this.points.Length - 1;
            int start_idx = 0;
            int finish_idx = last_idx;

            LineBuilder b = null;
            Point cur_pnt = new Point();

            do
            {
                cur_pnt = this.points[start_idx];
                b = new LineBuilder(cur_pnt, this.points[finish_idx]);

                do
                {
                    this.SetPixelStatus(cur_pnt, ON_BORDER);
                }
                while (b.NextPoint(ref cur_pnt));

                finish_idx = start_idx++;
            }
            while (start_idx <= last_idx);
        }

        private void SetPixelStatus(Point pnt, byte status)
        {
            int idx = pnt.X - this.x_min + this.width * (pnt.Y - this.y_min);
            this.status[idx] = status;
        }

        private byte GetPixelStatus(Point pnt)
        {
            int idx = pnt.X - this.x_min + this.width * (pnt.Y - this.y_min);
            return this.status[idx];
        }

        private bool CanVisit(Point p)
        {
            return p.X >= this.x_min && p.X <= this.x_max && p.Y >= this.y_min && p.Y <= this.y_max && GetPixelStatus(p) == INSIDE;
        }

        private void Explore()
        {
            int i;
            Stack<Point> stack = new Stack<Point>();

            Point pos = new Point(this.x_min, this.y_min);
            Point nxt = new Point();

            int[] x_offset = { 1, 0, -1, 0 };
            int[] y_offset = { 0, 1, 0, -1 };

            while (true)
            {
                this.SetPixelStatus(pos, OUTSIDE);

                for (i = 0; i < 4; ++i)
                {
                    nxt.X = pos.X + x_offset[i];
                    nxt.Y = pos.Y + y_offset[i];
                    if (this.CanVisit(nxt))
                    {
                        break;
                    }
                }

                if (i < 4)
                {
                    stack.Push(pos);
                    pos = nxt;
                }
                else
                {
                    try
                    {
                        pos = stack.Pop();
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
            }
        }

        private void SetFirst()
        {
            Point pnt = new Point(this.x_min + 1, this.y_min + 1);

            for (; pnt.Y <= this.y_max; ++pnt.Y)
            {
                for (pnt.X = this.x_min + 1; pnt.X < this.x_max; ++pnt.X)
                {
                    if (this.GetPixelStatus(pnt) != OUTSIDE)
                    {
                        this.cur_column = pnt.X;
                        this.cur_row = pnt.Y;
                        return;
                    }
                }
            }
        }

        private Point[] points = null;
        private byte[] status = null;
        private int x_min, y_min;
        private int x_max, y_max;
        private int width, height;

        private int cur_column = 1;
        private int cur_row = 1;

        private const byte INSIDE = 0;
        private const byte OUTSIDE = 1;
        private const byte ON_BORDER = 2;
    }

}
