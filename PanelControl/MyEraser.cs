using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelControl
{
    public class MyEraser: MyShape
    {
        public override void drawnShap(Graphics pe)
        {
            Brush brush = new SolidBrush(Color.White);
            for (int i = 0; i < State.EraserPoint.Count() - 1; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    pe.DrawLine(new Pen(Color.White, 15), State.EraserPoint[i], State.EraserPoint[i + 1]);
                }
            }
            for (int i = 0; i < State.EraserPoint.Count(); i++)
            {
                pe.FillRectangle(brush, State.EraserPoint[i].X, State.EraserPoint[i].Y, 5, 5);
            }
        }
        public MyEraser(PaintState myPaint)
        {
            State = myPaint;
        }


    }
}
