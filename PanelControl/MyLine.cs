using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelControl
{
    public class MyLine : MyShape
    {

        public override void drawnShap(Graphics pe)
        {

            pe.DrawLine(new Pen(State.CurrentColor, State.LineWidth), State.FirstPoint, State.SecondPoint);
        }

        public MyLine(PaintState myPaint)
        {
            State = myPaint;
        }
        public MyLine()
        {
        }
    }
}
