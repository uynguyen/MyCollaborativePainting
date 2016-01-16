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
    public class MyEllipse : MyShape
    {
        public override void drawnShap(Graphics pe)
        {
            Brush CurrentBrush = initBrush();
            if (this.State.Shift1 == true)
            {
                calcShift();

                findSecondPointWhenShift();

                pe.DrawEllipse(new Pen(State.CurrentColor, State.LineWidth), State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Width1);
                if (State.IsBrushFill == true)
                {
                    pe.FillEllipse(CurrentBrush, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Width1);
                }
            }
            else
            {
                calcHeightWidth();
                pe.DrawEllipse(new Pen(State.CurrentColor, State.LineWidth), State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Height1);

                if (State.IsBrushFill == true)
                {
                    pe.FillEllipse(CurrentBrush, State.StartPoint.X, State.StartPoint.Y, State.Width1, State.Height1);
                }
            }


        }
        public MyEllipse(PaintState myPaint)
        {
            State = myPaint;
        }
        public MyEllipse()
        {
        }


    }
}
