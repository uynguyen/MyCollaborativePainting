using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelControl
{
    public class MyRectangle : MyShape
    {

        public override void drawnShap(Graphics pe)
        {

            Brush CurrentBrush = initBrush();
            if (this.State.Shift1 == true)
            {
                calcShift();

                //Tính toán lại điểm kết thúc
                findSecondPointWhenShift();

                pe.DrawRectangle(new Pen(State.CurrentColor, State.LineWidth), State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Width1);

                if (State.IsBrushFill)
                {
                    pe.FillRectangle(CurrentBrush, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Width1);
                }

            }
            else
            {
                calcHeightWidth();
                pe.DrawRectangle(new Pen(State.CurrentColor, State.LineWidth), State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Height1);
                if (State.IsBrushFill)
                {
                    pe.FillRectangle(CurrentBrush, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Height1);
                }

            }

        }

        public MyRectangle(PaintState myPaint)
        {
            State = myPaint;
        }
        public MyRectangle()
        {
        }
    }
}
