using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelControl
{
    public class MyText: MyShape
    {

        public override void drawnShap(System.Drawing.Graphics pe)
        {

            // Tạo font và bút vẽ
            Font drawFont = new Font(State.TextFont, State.TextSize);
            SolidBrush drawBrush = new SolidBrush(State.CurrentColor);
            pe.DrawString(State.Text, drawFont, drawBrush, State.TextLocation);

        }

        public MyText(PaintState myPaint)
        {
            State = myPaint;
        }



    }
}
