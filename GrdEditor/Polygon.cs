using System;
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
            this.points = points;

            


            this.triangle_builder = new TriangleBuilder(points[0], points[1], points[2]);
            this.СalcCircumscribedRectangleBounds();
        }

        public bool NextPoint(ref Point pnt)
        {
            do
            {
                if (!this.triangle_builder.NextPoint(ref pnt))
                {
                    if (this.next_vertex_index >= this.points.Length)
                    {
                        return false;
                    }
                    this.triangle_builder = new TriangleBuilder(
                        this.points[0],
                        this.points[this.next_vertex_index - 1],
                        this.points[this.next_vertex_index]
                    );
                    ++this.next_vertex_index;
                }
                else
                {
                    if (!this.IsUsed(pnt))
                    {
                        return true;
                    }
                }
            }
            while (true);
        }

        private void СalcCircumscribedRectangleBounds()
        {
            int x_max, y_max;

            this.x_min = x_max = this.points[0].X;
            this.y_min = y_max = this.points[0].Y;

            foreach (Point pnt in this.points)
            {
                if (pnt.X < this.x_min)
                {
                    this.x_min = pnt.X;
                }
                else if (pnt.X > x_max)
                {
                    x_max = pnt.X;
                }
                if (pnt.Y < this.y_min)
                {
                    this.y_min = pnt.Y;
                }
                else if (pnt.Y > y_max)
                {
                    y_max = pnt.Y;
                }
            }

            this.column_count = x_max - this.x_min + 1;
            int dy = y_max - this.y_min + 1;
            int cell_count = this.column_count * dy;

            this.used = new bool[cell_count];
            for (int i = 0; i < cell_count; ++i)
            {
                this.used[i] = false;
            }
        }

        private bool IsUsed(Point pnt)
        {
            int dx = pnt.X - this.x_min;
            int dy = pnt.Y - this.y_min;
            int cell_idx = dy * this.column_count + dx;
            if (this.used[cell_idx])
            {
                return true;
            }
            else
            {
                this.used[cell_idx] = true;
                return false;
            }
        }

        private Point[] points = null;
        private TriangleBuilder triangle_builder = null;
        private int next_vertex_index = 3;
        private int x_min, y_min, column_count;
        private bool[] used = null;
    }
}
