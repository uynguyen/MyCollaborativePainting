using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelControl
{
    public class PaintState
    {
        private Point _pointFill = new Point(); //Điểm bắt đầu thực hiện tô loang

        public Point PointFill
        {
            get { return _pointFill; }
            set { _pointFill = value; }
        }

        private Color _colorFill = new Color(); //Màu tô loang

        public Color ColorFill
        {
            get { return _colorFill; }
            set { _colorFill = value; }
        }

        private int RF; //RedFill

        public int RF1
        {
            get { return RF; }
            set { RF = value; }
        }
        private int GF; //GreenFill

        public int GF1
        {
            get { return GF; }
            set { GF = value; }
        }
        private int BF; //BlueFill

        public int BF1
        {
            get { return BF; }
            set { BF = value; }
        }


        private string _text; //Text

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        private string _textFont; //Font text

        public string TextFont
        {
            get { return _textFont; }
            set { _textFont = value; }
        }
        private int _textSize; //Text size

        public int TextSize
        {
            get { return _textSize; }
            set { _textSize = value; }
        }
        private Point _textLocation; //Vị trí đặt text

        public Point TextLocation
        {
            get { return _textLocation; }
            set { _textLocation = value; }
        }

        private bool _isBrushFill; // Cờ hiệu xét xem có thực hiện brushfill không

        public bool IsBrushFill
        {
            get { return _isBrushFill; }
            set { _isBrushFill = value; }
        }
        private Color _brushColor = new Color(); //Màu brush

        public Color BrushColor
        {
            get { return _brushColor; }
            set { _brushColor = value; }
        }

        private int RB; //RedBrush

        public int RB1
        {
            get { return RB; }
            set { RB = value; }
        }

        private int GB; //GreenBrush

        public int GB1
        {
            get { return GB; }
            set { GB = value; }
        }
        private int BB; //BlueBrush

        public int BB1
        {
            get { return BB; }
            set { BB = value; }
        }

        private Point firstPoint; //Điểm đầu tiên click chuột của đối tượng

        public Point FirstPoint
        {
            get { return firstPoint; }
            set { firstPoint = value; }
        }

        private Point secondPoint; //Điểm thức hai click chuột của đối tượng

        public Point SecondPoint
        {
            get { return secondPoint; }
            set { secondPoint = value; }
        }

        private Color currentColor = new Color(); //Màu viền của đối tượng

        public Color CurrentColor
        {
            get { return currentColor; }
            set { currentColor = value; }
        }

        private int R, G, B; //Red, Green, Blue màu viền

        public int B1 
        {
            get { return B; }
            set { B = value; }
        }

        public int G1
        {
            get { return G; }
            set { G = value; }
        }

        public int R1
        {
            get { return R; }
            set { R = value; }
        }

        private List<Point> _eraserPoint = new List<Point>(); //Danh sách các điểm xóa

        public List<Point> EraserPoint
        {
            get { return _eraserPoint; }
            set { _eraserPoint = value; }
        }

        private int lineWidth = 1; //Độ dày nét vẽ

        public int LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }


        private ShapeType shapeType; //Loại hình

        public ShapeType ShapeType1
        {
            get { return shapeType; }
            set { shapeType = value; }
        }

        private bool Shift; // Giữ phím shift

        public bool Shift1
        {
            get { return Shift; }
            set { Shift = value; }
        }
        private float Width; // Chiều dài hình chữ nhật , ellipse

        public float Width1
        {
            get { return Width; }
            set { Width = value; }
        }
        private float Height; // Chiều cao hình chữ nhật , ellipse

        public float Height1
        {
            get { return Height; }
            set { Height = value; }
        }
        private Point startPoint; // Điểm bắt đầu vẽ hình chữ nhật, ellipse

        public Point StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        public PaintState(PaintState a)
        {
            //Text
            _text = a._text;
            _textFont = a._textFont;
            _textLocation = a._textLocation;
            _textSize = a._textSize;

            //
            currentColor = a.currentColor;
            lineWidth = a.lineWidth;
            firstPoint = a.firstPoint;
            secondPoint = a.secondPoint;
            ShapeType1 = a.ShapeType1;
            R = a.R;
            G = a.G;
            B = a.B;
            Shift = a.Shift;

            //Fill
            RF = a.RF;
            GF = a.GF;
            BF = a.BF;
            _pointFill = a._pointFill;

            //Brush
            _isBrushFill = a._isBrushFill;      
            _brushColor = a._brushColor;
            RB = a.RB;
            GB = a.GB;
            BB = a.BB;
            _nameBrush = a._nameBrush;

            //Erase
            _eraserPoint = new List<Point>();
            foreach (var item in a._eraserPoint)
            {
                _eraserPoint.Add(item);
            }
            

        }
        public PaintState()
        {
            lineWidth = 3;
            currentColor = Color.Black;
            R = currentColor.R;
            G = currentColor.G;
            B = currentColor.B;
            _text = "";
            _textFont = "Arial";
            _textSize = 0;
            _textLocation = new Point(0, 0);
            BrushColor = Color.White;
            RB = BrushColor.R;
            GB = BrushColor.G;
            BB = BrushColor.B;          
        }

        private string _nameBrush = "NoFill"; //Loại brush

        public string NameBrush
        {
            get { return _nameBrush; }
            set { _nameBrush = value; }
        }

        /// <summary>
        /// Hàm khởi tạo State từ 1 dãy byte
        /// </summary>
        /// <param name="data">dãy byte chứa dữ liệu</param>
        public PaintState(byte[] data)
        {
            firstPoint.X = BitConverter.ToInt32(data, 0); //Bắt đầu từ byte thứ 0
            firstPoint.Y = BitConverter.ToInt32(data, 4); //Tùy vào kích thước của mỗi kiểu dữ liệu mà sẽ tăng vị trí đọc dãy byte (int = 4, char = 1 ...)
            secondPoint.X = BitConverter.ToInt32(data, 8);
            secondPoint.Y = BitConverter.ToInt32(data, 12);
            R = BitConverter.ToInt32(data, 16);
            G = BitConverter.ToInt32(data, 20);
            B = BitConverter.ToInt32(data, 24);
            lineWidth = BitConverter.ToInt32(data, 28);
            int shapetypeLength = BitConverter.ToInt32(data, 32);
            string type = Encoding.ASCII.GetString(data, 36, shapetypeLength);
            switch (type)
            {
                case "MyLine":
                    shapeType = ShapeType.MyLine;
                    break;
                case "MyRectangle":
                    shapeType = ShapeType.MyRectangle;
                    break;
                case "MyEllipse":
                    shapeType = ShapeType.MyEllipse;
                    break;
                case "MyTriangle":
                    shapeType = ShapeType.MyTriangle;
                    break;
                case "MyText":                 
                    shapeType = ShapeType.MyText;
                    break;
                case "MyEraser":
                    shapeType = ShapeType.MyEraser;
                    break;
                case "MyFill":
                     shapeType = ShapeType.MyFill;
                    break;
            }
            int index = 36 + shapetypeLength;
            Shift = BitConverter.ToBoolean(data, index);
            Width = BitConverter.ToInt32(data, index + 1);
            Height = BitConverter.ToInt32(data, index + 5);
            startPoint.X = BitConverter.ToInt32(data, index + 9);
            startPoint.Y = BitConverter.ToInt32(data, index + 13);

            int textLeng = BitConverter.ToInt32(data, index + 17);
            _text = Encoding.Unicode.GetString(data, index + 21, textLeng);

            int fontLeng = BitConverter.ToInt32(data, index + 21 + textLeng);
            _textFont = Encoding.ASCII.GetString(data, index + 21 + textLeng + 4 , fontLeng);

            index = index + 21 + textLeng + 4 + fontLeng;
            _textSize = BitConverter.ToInt32(data, index);

            _textLocation.X = BitConverter.ToInt32(data, index + 4);

            _textLocation.Y = BitConverter.ToInt32(data, index + 8);


            int brushLeng = BitConverter.ToInt32(data, index + 12);

            _nameBrush = Encoding.ASCII.GetString(data, index + 16, brushLeng);

            RB = BitConverter.ToInt32(data, index + 16 + brushLeng);
            GB = BitConverter.ToInt32(data, index + 20 + brushLeng);
            BB = BitConverter.ToInt32(data, index + 24 + brushLeng);

            _brushColor = Color.FromArgb(RB, GB, BB);       

            _pointFill.X = BitConverter.ToInt32(data, index + 28 + brushLeng);
            _pointFill.Y = BitConverter.ToInt32(data, index + 32 + brushLeng);


            RF = BitConverter.ToInt32(data, index + 36 + brushLeng);
            GF = BitConverter.ToInt32(data, index + 40 + brushLeng);
            BF = BitConverter.ToInt32(data, index + 44 + brushLeng);
            _colorFill = Color.FromArgb(RF, GF, BF);


            int eraserpoint = BitConverter.ToInt32(data, index + 48 + brushLeng);
            index = index + 52 + brushLeng;
            for (int i = 0; i < eraserpoint; i++)
            {
                int x = BitConverter.ToInt32(data, index);
                index += 4;
                int y = BitConverter.ToInt32(data, index);
                index += 4;
                Point temp = new Point(x, y);
                _eraserPoint.Add(temp);
            }

        }

        /// <summary>
        /// Hàm chuyển đổi State thành các byte để truyền đi trong network
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            List<byte> byteList = new List<byte>();

            byteList.AddRange(BitConverter.GetBytes(firstPoint.X));
            byteList.AddRange(BitConverter.GetBytes(firstPoint.Y));

            byteList.AddRange(BitConverter.GetBytes(secondPoint.X));
            byteList.AddRange(BitConverter.GetBytes(secondPoint.Y));

            byteList.AddRange(BitConverter.GetBytes(R));
            byteList.AddRange(BitConverter.GetBytes(G));
            byteList.AddRange(BitConverter.GetBytes(B));

            byteList.AddRange(BitConverter.GetBytes(lineWidth));

            string type = shapeType.ToString();
            int leng = type.Length;

            byteList.AddRange(BitConverter.GetBytes(leng));
            byteList.AddRange(Encoding.ASCII.GetBytes(type));

            byteList.AddRange(BitConverter.GetBytes(Shift));


            byteList.AddRange(BitConverter.GetBytes(Width));
            byteList.AddRange(BitConverter.GetBytes(Height));

            byteList.AddRange(BitConverter.GetBytes(startPoint.X));
            byteList.AddRange(BitConverter.GetBytes(startPoint.Y));


            int textLeng = _text.Length;
            byteList.AddRange(BitConverter.GetBytes(textLeng * 2)); //Mỗi ký tự Unicode 16 bit (2 byte)
            byteList.AddRange(Encoding.Unicode.GetBytes(_text));

            int fontLeng = _textFont.Length;
            byteList.AddRange(BitConverter.GetBytes(fontLeng));
            byteList.AddRange(Encoding.ASCII.GetBytes(_textFont));

            byteList.AddRange(BitConverter.GetBytes(_textSize));


            byteList.AddRange(BitConverter.GetBytes(_textLocation.X));
            byteList.AddRange(BitConverter.GetBytes(_textLocation.Y));

            //Brush

            byteList.AddRange(BitConverter.GetBytes(_nameBrush.Length));
            byteList.AddRange(Encoding.ASCII.GetBytes(_nameBrush));

            byteList.AddRange(BitConverter.GetBytes(RB));
            byteList.AddRange(BitConverter.GetBytes(GB));
            byteList.AddRange(BitConverter.GetBytes(BB));


            //Fill

            byteList.AddRange(BitConverter.GetBytes(_pointFill.X));
            byteList.AddRange(BitConverter.GetBytes(_pointFill.Y));
            byteList.AddRange(BitConverter.GetBytes(RF));
            byteList.AddRange(BitConverter.GetBytes(GF));
            byteList.AddRange(BitConverter.GetBytes(BF));


            //Eraser
            byteList.AddRange(BitConverter.GetBytes(_eraserPoint.Count));
            for (int i = 0; i < _eraserPoint.Count; i++)
            {
                byteList.AddRange(BitConverter.GetBytes(_eraserPoint[i].X));
                byteList.AddRange(BitConverter.GetBytes(_eraserPoint[i].Y));
            }

            return byteList.ToArray();
        }

    }
}
