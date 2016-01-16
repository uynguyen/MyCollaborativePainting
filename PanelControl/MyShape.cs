using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PanelControl
{
    public abstract class MyShape
    {
        private PaintState state; //Trạng thái vẽ của đối tượng

        public PaintState State
        {
            get { return state; }
            set { state = value; }
        }
         
        public abstract void drawnShap(Graphics pe);
        /// <summary>
        /// Hàm thực hiện tính khoảng cách của hai điểm trên mặt phẳng
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public double calcDistance(Point A, Point B)
        {
            return Math.Sqrt(Math.Pow(A.X - B.X, 2.0) + Math.Pow(A.Y - B.Y, 2.0));
        }
        /// <summary>
        /// Hàm thực hiện tính chiều cao và chiều rộng của hình chữ nhật, bao khung, ellipse ...
        /// </summary>
        public void calcHeightWidth()
        {
            Point temp = new Point(state.FirstPoint.X, state.SecondPoint.Y);
            Point startPointTemp; // Điểm bắt đầu của hình chữ nhật
            // Xét 4 hướng vẽ của hình chữ nhật.
            if ((state.SecondPoint.X < state.FirstPoint.X) && (state.SecondPoint.Y < state.FirstPoint.Y))
            {
                startPointTemp = state.SecondPoint;
            }
            else
            {
                if ((state.SecondPoint.X < state.FirstPoint.X) && (state.SecondPoint.Y > state.FirstPoint.Y))
                {
                    startPointTemp = new Point(state.SecondPoint.X, state.FirstPoint.Y);
                }
                else
                {
                    if ((state.SecondPoint.X > state.FirstPoint.X) && (state.SecondPoint.Y < state.FirstPoint.Y))
                    {
                        startPointTemp = new Point(state.FirstPoint.X, state.SecondPoint.Y);
                    }
                    else
                    {
                        startPointTemp = state.FirstPoint;
                    }

                }

            }
            state.Width1 = (float)calcDistance(temp, state.SecondPoint);

            state.Height1 = (float)calcDistance(state.FirstPoint, temp);

            state.StartPoint = startPointTemp;
        }
        public void calcShift() // Tính toán tọa độ của sự kiện shift
        {
            Point temp = new Point(state.FirstPoint.X, state.SecondPoint.Y);
            Point startPointTemp; // Điểm bắt đầu của hình chữ nhật
            state.Width1 = (float)calcDistance(temp, state.SecondPoint);
            // Xét 4 hướng vẽ của hình chữ nhật.
            // Cung thứ 2
            if ((state.SecondPoint.X < state.FirstPoint.X) && (state.SecondPoint.Y < state.FirstPoint.Y))
            {
                startPointTemp = new Point(state.FirstPoint.X - (int)state.Width1, state.FirstPoint.Y - (int)state.Width1);
            }
            else
            {
                // Cung thứ 3
                if ((state.SecondPoint.X < state.FirstPoint.X) && (state.SecondPoint.Y > state.FirstPoint.Y))
                {
                    startPointTemp = new Point(state.FirstPoint.X - (int)state.Width1, state.FirstPoint.Y);
                }
                else
                {
                    // Cung thứ 1
                    if ((state.SecondPoint.X > state.FirstPoint.X) && (state.SecondPoint.Y < state.FirstPoint.Y))
                    {
                        startPointTemp = new Point(state.FirstPoint.X, state.FirstPoint.Y - (int)state.Width1);
                    }

                    // cung thứ 4
                    else
                    {
                        startPointTemp = state.FirstPoint;
                    }

                }

            }

            state.StartPoint = startPointTemp;
        }

        /// <summary>
        /// Hàm thực hiện tìm tọa độ bao khung của đối tượng
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void findBourder( ref Point start, ref Point end)
        {
            int xMax, xMin, yMax, yMin;
            xMax = xMin = state.FirstPoint.X;
            yMax = yMin = state.FirstPoint.Y;

            if (state.SecondPoint.X < xMin)
                xMin = state.SecondPoint.X;

            if (state.SecondPoint.X > xMax)
                xMax = state.SecondPoint.X;

            if (state.SecondPoint.Y < yMin)
                yMin = state.SecondPoint.Y;

            if (state.SecondPoint.Y > yMax)
                yMax = state.SecondPoint.Y;
            start = new Point(xMin, yMin);
            end = new Point(xMax, yMax);
        }

        /// <summary>
        /// Hàm kiểm tra một điểm có nằm trong bao khung của đối tượng không
        /// </summary>
        /// <param name="point">Điểm cần kiểm tra</param>
        /// <returns></returns>
        public bool checkPointInside(Point point)
        {
            int xMax, xMin, yMax, yMin;
            xMax = xMin = state.FirstPoint.X;
            yMax = yMin = state.FirstPoint.Y;

            if (state.SecondPoint.X < xMin)
                xMin = state.SecondPoint.X;

            if (state.SecondPoint.X > xMax)
                xMax = state.SecondPoint.X;

            if (state.SecondPoint.Y < yMin)
                yMin = state.SecondPoint.Y;

            if (state.SecondPoint.Y > yMax)
                yMax = state.SecondPoint.Y;
            return (point.X > xMin && point.X < xMax && point.Y > yMin && point.Y < yMax);  

        }
        /// <summary>
        /// Hàm tính lại điểm thứ 2 khi sự kiện Shift xảy ra
        /// </summary>
        public void findSecondPointWhenShift()
        {
            if (State.SecondPoint.X < State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y)
            {
                State.SecondPoint = new Point((int)(State.FirstPoint.X - State.Width1), (int)(State.FirstPoint.Y - State.Width1));
            }
            else
            {
                if (State.SecondPoint.X < State.FirstPoint.X && State.SecondPoint.Y > State.FirstPoint.Y)
                {
                    State.SecondPoint = new Point((int)(State.FirstPoint.X - State.Width1), (int)(State.FirstPoint.Y + State.Width1));
                }
                else
                {
                    if (State.SecondPoint.X > State.FirstPoint.X && State.SecondPoint.Y < State.FirstPoint.Y)
                    {
                        State.SecondPoint = new Point((int)(State.FirstPoint.X + State.Width1), (int)(State.FirstPoint.Y - State.Width1));
                    }
                    else
                    {
                        State.SecondPoint = new Point((int)(State.FirstPoint.X + State.Width1), (int)(State.FirstPoint.Y + State.Width1));
                    }
                }

            }
        }

        /// <summary>
        /// Hàm xét xem một điểm có nằm gần với một đường thẳng không
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public float calcDistanceLine(Point check)
        {
            // Tính các hệ số của đường thẳng y = ax + b
            float a = -(State.SecondPoint.Y - State.FirstPoint.Y);
            float b = (State.SecondPoint.X - State.FirstPoint.X);
            float c = (-a * ( State.FirstPoint.X ) - b * State.FirstPoint.Y);
            float distance = (float)(Math.Abs(a * check.X + b * check.Y + c) / Math.Sqrt(a * a + b * b));

            return distance;
        }


        /// <summary>
        /// Hàm thực hiện tìm vị trí cung phần tư của điểm trong sự kiện ấn shift đường thẳng
        /// </summary>
        /// <returns>Vị trí các cung được đánh số từ 1 đến 8 theo cùng chiều kim đồng hồ</returns>
        public int getLocation()
        {
            Point p1 = new Point(0, 1);
            Point p2 = new Point(State.SecondPoint.Y - State.FirstPoint.Y, State.SecondPoint.X - State.FirstPoint.X);
            float xDiff = p2.X - p1.X;
            float yDiff = p2.Y - p1.Y;

            float angle = (float)(Math.Atan2(yDiff, xDiff) * (180 / Math.PI));

            if (angle > 135 && angle <= 180)
            {
                return 1;
            }
            if (angle > 90 && angle <= 135)
            {
                return 2;
            }
            if (angle > 45 && angle <= 90)
            {
                return 3;
            }

            if (angle > 0 && angle <= 45)
            {
                return 4;
            }
            if (angle <= 0 && angle > -45)
            {
                return 5;
            }
            if (angle <= -45 && angle > -90)
            {
                return 6;
            }
            if (angle <= -90 && angle > -135)
            {
                return 7;
            }
            return 8;
        }

        public Brush initBrush()
        {
            Brush CurrentBrush = new SolidBrush(State.BrushColor); 
            State.IsBrushFill = true;
            switch (State.NameBrush)
            {
                case "SolidBrush":
                    CurrentBrush = new SolidBrush(State.BrushColor);

                    break;
                case "Wave":
                    CurrentBrush = new HatchBrush(HatchStyle.Wave, State.BrushColor);

                    break;
                case "ZigZag":
                    CurrentBrush = new HatchBrush(HatchStyle.ZigZag, State.BrushColor);

                    break;
                case "Shingle":
                    CurrentBrush = new HatchBrush(HatchStyle.Shingle, State.BrushColor);

                    break;
                case "Sphere":
                    CurrentBrush = new HatchBrush(HatchStyle.Sphere, State.BrushColor);

                    break;
                case "Vertical":
                    CurrentBrush = new HatchBrush(HatchStyle.Vertical, State.BrushColor);

                    break;
                case "Trellis":
                    CurrentBrush = new HatchBrush(HatchStyle.Trellis, State.BrushColor);

                    break;
                case "Plaid":
                    CurrentBrush = new HatchBrush(HatchStyle.Plaid, State.BrushColor);

                    break;
                case "SmallGrid":
                    CurrentBrush = new HatchBrush(HatchStyle.SmallGrid, State.BrushColor);
                    break;
                case "Weave":
                    CurrentBrush = new HatchBrush(HatchStyle.Weave, State.BrushColor);
                    break;
                default:
                    State.IsBrushFill = false;
                    break;
            }
            return CurrentBrush;
        }


        public MyShape()
        {

        }
        public MyShape(PaintState a)
        {


        }

    }
}
