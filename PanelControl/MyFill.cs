using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelControl
{
    public class MyFill : MyShape
    {
      
        public override void drawnShap(Graphics pe)
        {

        }

        public MyFill(PaintState myPaint)
        {
            State = myPaint;
        }
        public MyFill()
        {
        }
    }
}
