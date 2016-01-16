using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelControl
{
    public class MyTriangle : MyShape
    {
        public override void drawnShap(Graphics pe)
        {
            Brush CurrentBrush = initBrush();

            //Thực hiện tính toán điểm thứ 3 dựa trên điểm đầu và điểm cuối
            if (this.State.Shift1 == true)
            {
                calcShift();
                findSecondPointWhenShift();

                Point midPoint = new Point((State.FirstPoint.X + State.SecondPoint.X) / 2, State.FirstPoint.Y);

                if ((State.SecondPoint.X < State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y)
                   || (State.SecondPoint.X > State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y))
                {
                    midPoint = new Point((State.FirstPoint.X + State.SecondPoint.X) / 2, State.SecondPoint.Y);

                    float temp1 = (float)calcDistance(midPoint, new Point(State.SecondPoint.X, State.FirstPoint.Y));

                    float temp2 = (float)calcDistance(midPoint, State.SecondPoint);

                    double temp = (double)((Math.Pow((double)temp1, 2.0) - Math.Pow((double)temp2, 2.0)));
                    double d = Math.Sqrt(temp);


                    Point A = new Point(State.SecondPoint.X, (int)(State.SecondPoint.Y + d));

                    Point B = new Point(State.FirstPoint.X, (int)(State.SecondPoint.Y + d));

                    Point[] pts = new Point[]
                    {
                        midPoint, A,B
                    };

                    pe.DrawPolygon(new Pen(State.CurrentColor, State.LineWidth), pts);
                    if (State.IsBrushFill)
                    {
                        pe.FillPolygon(CurrentBrush, pts);
                    }
                }
                else
                {

                    midPoint = new Point((State.FirstPoint.X + State.SecondPoint.X) / 2, State.FirstPoint.Y);

                    float temp1 = (float)calcDistance(midPoint, new Point(State.FirstPoint.X, State.SecondPoint.Y));

                    float temp2 = (float)calcDistance(midPoint, State.FirstPoint);

                    double temp = (double)((Math.Pow((double)temp1, 2.0) - Math.Pow((double)temp2, 2.0)));
                    double d = Math.Sqrt(temp);


                    Point A = new Point(State.FirstPoint.X, (int)(State.FirstPoint.Y + d));

                    Point B = new Point(State.SecondPoint.X, (int)(State.FirstPoint.Y + d));

                    Point[] pts = new Point[]
                    {
                        midPoint, A,B
                    };

                    pe.DrawPolygon(new Pen(State.CurrentColor, State.LineWidth), pts);
                    if (State.IsBrushFill)
                    {
                        pe.FillPolygon(CurrentBrush, pts);
                    }
                }
            }
            else
            {
                Point midPoint = new Point();
                Point pointTemp = new Point();
                if ((State.SecondPoint.X < State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y)
                    || (State.SecondPoint.X > State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y))
                {
                    midPoint = new Point((State.FirstPoint.X + State.SecondPoint.X) / 2, State.SecondPoint.Y);

                    pointTemp = new Point(State.SecondPoint.X, State.FirstPoint.Y);
                    Point[] pts = new Point[]
                    {
                        midPoint, pointTemp, State.FirstPoint
                    };
                    pe.DrawPolygon(new Pen(State.CurrentColor, State.LineWidth), pts);

                    if (State.IsBrushFill)
                    {
                        pe.FillPolygon(CurrentBrush, pts);
                    }
                }
                else
                {

                    midPoint = new Point((State.FirstPoint.X + State.SecondPoint.X) / 2, State.FirstPoint.Y);

                    pointTemp = new Point(State.FirstPoint.X, State.SecondPoint.Y);

                    Point[] pts = new Point[]
                        {
                            midPoint, pointTemp, State.SecondPoint
                        };
                    pe.DrawPolygon(new Pen(State.CurrentColor, State.LineWidth), pts);

                    if (State.IsBrushFill)
                    {
                        pe.FillPolygon(CurrentBrush, pts);
                    }

                }
            }

        }

        public MyTriangle(PaintState myPaint)
        {
            State = myPaint;
        }
        public MyTriangle()
        {

        }



    }
}
