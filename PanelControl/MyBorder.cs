using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelControl
{
    /// <summary>
    /// Lớp bao khung của đối tượng vẽ
    /// </summary>
    public class MyBorder : MyShape
    {      
        public override void drawnShap(Graphics pe)
        {
            calcHeightWidth();
         
            Pen a = new Pen(Color.Brown, 3);
            a.DashStyle = DashStyle.Dot;
            if (State.ShapeType1 == ShapeType.MyLine)
            {
                pe.FillRectangle(Brushes.Black, State.FirstPoint.X - 1, State.FirstPoint.Y - 1, 5, 5);
                pe.FillRectangle(Brushes.Black, State.SecondPoint.X - 1, State.SecondPoint.Y - 1, 5, 5);
            }
            else
            {
                if (this.State.Shift1 == true)
                {
                    calcShift();
                    pe.DrawRectangle(a, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Width1);

                }
                else
                {
                    calcHeightWidth();
                    pe.DrawRectangle(a, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Height1);
                   
                }

                int bor = 5;
               //Vẽ 8 điểm điều khiển của bao khung
                pe.FillRectangle(Brushes.Black, State.FirstPoint.X - 1, State.FirstPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, (State.FirstPoint.X + State.SecondPoint.X) / 2 - 1, State.FirstPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, State.SecondPoint.X - 1, State.FirstPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, State.SecondPoint.X - 1, (State.FirstPoint.Y + State.SecondPoint.Y) / 2 - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, State.SecondPoint.X - 1, State.SecondPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, (State.FirstPoint.X + State.SecondPoint.X) / 2 - 1, State.SecondPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, State.FirstPoint.X - 1, State.SecondPoint.Y - 1, bor, bor);
                pe.FillRectangle(Brushes.Black, State.FirstPoint.X - 1, (State.FirstPoint.Y + State.SecondPoint.Y) / 2 - 1, bor, bor);
            
            }
             

        }
        public MyBorder(PaintState myPaint)
        {
            State = myPaint;
        }


    }
}
