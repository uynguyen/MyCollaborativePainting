using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PanelControl
{

    public delegate void updateStatus(string status); //Sự kiện cập nhật trạng thái 
    public delegate void updateRoom(string name); //Sự kiện cập nhật thành viên trong phòng
    public delegate void updateControl(int option);  // Sự kiện cập nhật control
    public delegate void removeMember(int index); //Sự kiện xóa thành viên rời khỏi phòng
    public delegate void updateRoomClient(string name); //Sự kiện cập nhật thành viên cho client
    public delegate void ableordisable(int index,bool option); //Sự kiện thông báo những người đang được vẽ trong phòng 
    public delegate void removeroomclient(int index); //Sự kiện xóa thành viên trong phòng client
    public partial class PaintPanel : Control
    {

        public event updateStatus STATUS;
        public event updateRoom ROOM;
        public event updateControl CONTROL;
        public event removeMember REMOVEMEM;
        public event updateRoomClient ROOMCLIENT;
        public event ableordisable ABLEORDISABLE;
        public event removeroomclient REMOVEROOMCLIENT;

        public bool _startRoom = false;//Biến xét trạng thái đã bắt đầu vẽ chưa

        private Stack<MyShape> undo = new Stack<MyShape>(); //Stack cho chức năng undo và redo

        private string _statusNetWork = "Hello";//Câu lệnh thông báo các trạng thái của phòng vẽ
    
        public string StatusNetWork
        {
            get { return _statusNetWork; }
            set { _statusNetWork = value; }
        }

        public Bitmap buffer = new Bitmap(1, 1); //Bitmap dùng cho thuật toán tô loang

        public bool _isFill = false;

        public bool _isRemoveBorder = false; //Biến điều khiên trạng thái đã xóa khung viền chưa
        public bool _isStillCanMove = false; //Biến điều khiển xem chức năng di chuyển có bị hủy chưa (Do thao tác)
        public bool _isStillCanReSize = false;//Biến điều khiển xem chức năng resize có bị hủy chưa (Do thao tác)

        public bool _isServer;
        public bool _isClient;

        private Color _fillColor = Color.Black;

        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }


        public TextBox textBox = new TextBox (); // TextBox đê lấy văn bản từ người dùng nhập vào

        public PaintState myPaint = new PaintState(); //Trạng thái vẽ hiện tại


        private List<MyShape> _myData = new List<MyShape>(); //Danh sách các đối tượng hình

        public List<MyShape> MyData
        {
            get { return _myData; }
            set { _myData = value; }
        }

        private bool _isCanMove = false; //Biến cờ hiệu xét trạng thái di chuyển

        public bool IsCanMove
        {
            get { return _isCanMove; }
            set { _isCanMove = value; }
        }

        private bool _isReSize = false; //Cờ trạng thái resize

        public bool IsReSize
        {
            get { return _isReSize; }
            set { _isReSize = value; }
        }

        private MyShape _shapeCurrent; // Đối tượng hiên tại đang được chỉnh sửa

        public PaintPanel()
        {
            InitializeComponent();
            myPaint.TextSize = 10;
            myPaint.TextFont = "Arial";
            myPaint.CurrentColor = Color.Black;
            myPaint.BrushColor = Color.White;

        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            base.OnPaint(pe);

            for (int i = 0; i < _myData.Count(); i++)
            {
                _myData[i].drawnShap(pe.Graphics);
            }


        }
    
        private bool[] _isReSizePoint = new bool[8] { false, false, false, false, false, false, false, false };// Mảng lưu trạng thái resize tương ứng với 8 điểm điều khiển trên khung bao hình chữ nhật
        //0: Góc trái trên
        //1: Góc trái dưới
        //2: Góc phải trên
        //3: Góc phải dưới
        //4: Cạnh trên
        //5: Cạnh trái.
        //6: Cạnh dưới
        //7: Cạnh phải

        /// <summary>
        /// Hàm tính vị trí điểm điều khiển resize
        /// </summary>
        /// <param name="e">Vị trí điểm cần xét</param>
        /// <param name="Start">Góc trái trên của khung bao hình chữ nhật</param>
        /// <param name="End">Góc phải dưới của khung bao hình chữ nhật</param>
        /// <returns>Có nằm xung quanh 8 điểm điều khiển hay không</returns>
        public bool findReSizePoint(Point e, Point Start, Point End, Point first, Point last)
        {
            if (myPaint.ShapeType1 != ShapeType.MyLine)
            {
                if (Math.Abs(e.X - End.X) < 5 && (Math.Abs(e.Y - End.Y) < 5))
                {
                    _isReSize = true;
                    _isReSizePoint = new bool[8] { false, false, false, true, false, false, false, false };
                    _shapeCurrent = _myData[_myData.Count() - 1];
                    return true;
                }
                else
                {
                    if (Math.Abs(e.X - Start.X) < 5 && (Math.Abs(e.Y - Start.Y) < 5))
                    {
                        _shapeCurrent = _myData[_myData.Count() - 1];
                        _isReSize = true;
                        _isReSizePoint = new bool[8] { true, false, false, false, false, false, false, false };
                        return true;
                    }
                    else
                    {
                        if (Math.Abs(e.X - Start.X) < 5 && (Math.Abs(e.Y - End.Y) < 5))
                        {
                            _shapeCurrent = _myData[_myData.Count() - 1];
                            _isReSize = true;
                            _isReSizePoint = new bool[8] { false, true, false, false, false, false, false, false };
                            return true;
                        }
                        else
                        {
                            if (Math.Abs(e.X - End.X) < 5 && (Math.Abs(e.Y - Start.Y) < 5))
                            {
                                _shapeCurrent = _myData[_myData.Count() - 1];
                                _isReSize = true;
                                _isReSizePoint = new bool[8] { false, false, true, false, false, false, false, false };
                                return true;
                            }
                            else
                            {
                                if (Math.Abs(e.X - (End.X + Start.X) / 2) < 5 && (Math.Abs(e.Y - Start.Y) < 5))
                                {
                                    _shapeCurrent = _myData[_myData.Count() - 1];
                                    _isReSize = true;
                                    _isReSizePoint = new bool[8] { false, false, false, false, true, false, false, false };
                                    return true;
                                }
                                else
                                {
                                    if (Math.Abs(e.X - Start.X) < 5 && (Math.Abs(e.Y - (Start.Y + End.Y) / 2) < 5))
                                    {
                                        _shapeCurrent = _myData[_myData.Count() - 1];
                                        _isReSize = true;
                                        _isReSizePoint = new bool[8] { false, false, false, false, false, true, false, false };
                                        return true;
                                    }
                                    else
                                    {
                                        if (Math.Abs(e.X - (End.X + Start.X) / 2) < 5 && (Math.Abs(e.Y - End.Y) < 5))
                                        {
                                            _shapeCurrent = _myData[_myData.Count() - 1];
                                            _isReSize = true;
                                            _isReSizePoint = new bool[8] { false, false, false, false, false, false, true, false };
                                            return true;
                                        }
                                        else
                                        {
                                            if (Math.Abs(e.X - End.X) < 5 && (Math.Abs(e.Y - (Start.Y + End.Y) / 2) < 5))
                                            {
                                                _shapeCurrent = _myData[_myData.Count() - 1];
                                                _isReSize = true;
                                                _isReSizePoint = new bool[8] { false, false, false, false, false, false, false, true };
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_myData.Count() > 0)
                {
                    if (Math.Abs(e.X - first.X) < 5 && (Math.Abs(e.Y - first.Y) < 5))
                    {
                        _isReSize = true;
                        _isReSizePoint = new bool[8] { true, false, false, false, false, false, false, false };
                        _shapeCurrent = _myData[_myData.Count() - 1];
                        return true;
                    }
                    else
                    {
                        if (Math.Abs(e.X - last.X) < 5 && (Math.Abs(e.Y - last.Y) < 5))
                        {
                            _shapeCurrent = _myData[_myData.Count() - 1];
                            _isReSize = true;
                            _isReSizePoint = new bool[8] { false, true, false, false, false, false, false, false };
                            return true;
                        }
                    }
                }

            }
            return false;

        }

        public void Undo()
        {
            if (_myData.Count > 0)
            {
                if (!_isRemoveBorder)
                {
                    _myData.RemoveAt(_myData.Count - 1);
                }

                MyShape temp = _myData[_myData.Count - 1];

                undo.Push(temp);

                _myData.RemoveAt(_myData.Count - 1);

                _isRemoveBorder = true;
                _isStillCanMove = _isStillCanReSize = false;
                this.Cursor = System.Windows.Forms.Cursors.Default;
                this.Invalidate();
            }

        }
        public void Redo()
        {
            if (undo.Count > 0)
            {
                if (_myData.Count > 0 && !_isRemoveBorder)
                {
                    _myData.RemoveAt(_myData.Count - 1);
                }

                MyShape temp = undo.Pop();

                _myData.Add(temp);
                _isRemoveBorder = true;
                _isStillCanMove = _isStillCanReSize = false;
                this.Cursor = System.Windows.Forms.Cursors.Default;
                this.Invalidate();
            }

        }

        private void PaintPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (_myData.Count() == 0)
            {
                undo.Clear();
            }
  
            if (!_isFill)
            {
                f = new Point();
                myPaint.FirstPoint = myPaint.SecondPoint = e.Location;
                
                /////

                //
                if (myPaint.ShapeType1 != ShapeType.MyEraser)
                {
                    myPaint.TextLocation = e.Location;
                    //Xác định bao khung để xem thử có đang ở chế độ Resize không

                    Point Start = new Point();
                    Point End = new Point();
                    if (_myData.Count() > 0)
                    {
                        MyShape temp = _myData[_myData.Count() - 1];
                        temp.findBourder(ref Start, ref End);
                    }

                    // Bỏ hình viền bao quanh hình vừa mới vẽ
                    int n = _myData.Count();
                    if (n > 0 && _myData[n - 1].State.ShapeType1 != ShapeType.MyText && !_isRemoveBorder)
                    {
                        _myData.RemoveAt(n - 1);
                        n--;
                        _isRemoveBorder = true;
                      
                    }

                    //Giữ lại điểm đầu của hình cho chức năng resize đường thẳng
                    if (n > 0)
                    {
                        f = new Point(_myData[n - 1].State.SecondPoint.X, _myData[n - 1].State.SecondPoint.Y);
                    }


                    bool flag = false;
                    if (n > 0 && _myData[_myData.Count() - 1].State.ShapeType1 != ShapeType.MyEraser && _isStillCanReSize)
                    {
                        flag = findReSizePoint(e.Location, Start, End, _myData[_myData.Count() - 1].State.FirstPoint, _myData[_myData.Count() - 1].State.SecondPoint);
                    }
                    // Nếu không ở chế độ resize
                    if (!flag)
                    {

                        _isReSizePoint = new bool[8] { false, false, false, false, false, false, false, false };
                        _isReSize = false;

                        if (n > 0 && _myData[_myData.Count() - 1].State.ShapeType1 == ShapeType.MyText
                            && textBox.Text != "")
                        {
                            _myData[_myData.Count() - 1].State.Text = textBox.Text;
                            _myData[_myData.Count() - 1].State.TextLocation = textBox.Location;
                            _myData[_myData.Count() - 1].State.TextSize = (int) textBox.Font.Size;
                            _myData[_myData.Count() - 1].State.TextFont = textBox.Font.Name.ToString();
                            _myData[_myData.Count() - 1].State.CurrentColor = textBox.ForeColor;
                            this.Invalidate();
                        }

                        // Xóa đi các textbox
                        this.Controls.Clear();
                        textBox = new TextBox();

                        if (_isStillCanMove)
                        {
                            //Nếu không phải là đường thẳng thì xét điểm click nằm trong bao khung
                            if (n > 0 && _myData[n - 1].State.ShapeType1 == ShapeType.MyLine)
                            {
                                if (_myData[n - 1].calcDistanceLine(e.Location) < 5)
                                {
                                    _isCanMove = true;
                                    
                                }
                                else
                                {
                                    _isCanMove = false;
                                }

                            }
                            //Nếu là đường thẳng thì xét khoảng cách từ điểm click đến đường thẳng đó nhỏ hơn < Epxelon ( 5 )
                            else
                            {
                                if (n > 0 && _myData[n - 1].State.ShapeType1 != ShapeType.MyEraser && _myData[n - 1].checkPointInside(e.Location))// Nếu đang nằm trong vùng của hình mới vẽ thì kích hoạt trạng thái di chuyển
                                {
                                    _isCanMove = true;
                                }
                            }

                            if (_isCanMove)
                            {
                                int dentaX = myPaint.SecondPoint.X - myPaint.FirstPoint.X;
                                int dentaY = myPaint.SecondPoint.Y - myPaint.FirstPoint.Y;

                                PaintState temp1 = new PaintState(myPaint);
                                MyShape a;
                                switch (myPaint.ShapeType1)
                                {
                                    case ShapeType.MyLine:
                                        a = new MyLine(temp1);
                                        break;
                                    case ShapeType.MyRectangle:
                                        a = new MyRectangle(temp1);
                                        break;
                                    case ShapeType.MyEllipse:
                                        a = new MyEllipse(temp1);
                                        break;
                                    case ShapeType.MyTriangle:
                                        a = new MyTriangle(temp1);
                                        break;
                                    default:
                                        a = new MyLine(temp1);
                                        break;
                                }

                                a.State.FirstPoint = new Point(_myData[n - 1].State.FirstPoint.X + dentaX, _myData[n - 1].State.FirstPoint.Y + dentaY);
                                a.State.SecondPoint = new Point(_myData[n - 1].State.SecondPoint.X + dentaX, _myData[n - 1].State.SecondPoint.Y + dentaY);
                                a.State.Shift1 = _myData[n - 1].State.Shift1;
                                _shapeCurrent = _myData[n - 1];
                                _myData[n - 1] = a;

                                this.Invalidate();
                            }


                           
                        }
                        // Nếu chưa có hình nào hoặc nằm ngoài hình vừa mới vẽ hoặc trạng thái có thể di chuyển bị tắt hoặc (hình trước đó là đường thẳng và không được bật trạng thái di chuyển ) thì tắt trạng thái di chuyển và bắt đầu thêm hình mới vào danh sách các hình
                        if (n == 0 || (n > 0 && !_myData[n - 1].checkPointInside(e.Location)) || (n > 0 && _myData[n - 1].checkPointInside(e.Location) && _myData[n - 1].State.ShapeType1 == ShapeType.MyLine && !_isCanMove) || !_isStillCanMove)
                        {
                            _isCanMove = false;

                            if (n > 0)
                            {
                                MyShape temp2 = _myData[_myData.Count() - 1];
                                byte[] buffer = temp2.State.ToByteArray();
                                if (_isClient && client != null && client.Connected)
                                {
                                    try
                                    {

                                        client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }

                                if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                                {

                                    foreach (var item in _clientSocket)
                                    {
                                        item.Send(buffer);
                                    }

                                }
                            }
                            //Hình đầu tiên của các client trước ( n - 1 ) bị gới lên server bị mất nên phải gởi lại hình đầu tiên
                            if (n == 1)
                            {
                                MyShape temp3 = _myData[_myData.Count() - 1];
                                byte[] buffer = temp3.State.ToByteArray();
                                if (_isClient && client != null && client.Connected)
                                {
                                    try
                                    {

                                        client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }

                                if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                                {

                                    foreach (var item in _clientSocket)
                                    {
                                        item.Send(buffer);
                                    }

                                }
                            }
                            PaintState t = new PaintState(myPaint);

                            MyShape a;
                            switch (myPaint.ShapeType1)
                            {
                                case ShapeType.MyLine:
                                    a = new MyLine(t);
                                    break;
                                case ShapeType.MyRectangle:
                                    a = new MyRectangle(t);
                                    break;
                                case ShapeType.MyEllipse:
                                    a = new MyEllipse(t);
                                    break;
                                case ShapeType.MyTriangle:
                                    a = new MyTriangle(t);
                                    break;
                                case ShapeType.MyText:                        
                                    a = new MyText(t);
                                    break;                                
                                default:
                                    a = new MyLine(t);
                                    break;
                            }

                            _myData.Add(a);

                            // Nếu đang ở trạng thái text thì thêm 1 khung hình 
                            if (myPaint.ShapeType1 == ShapeType.MyText)
                            {
                                MyRectangle x = new MyRectangle (myPaint);
                                x.State.CurrentColor = myPaint.CurrentColor;
                                x.State.LineWidth = 1;
                                x.State.NameBrush = "NoFill";
                                x.State.IsBrushFill = false;
                                _myData.Add(x);
                            }

                        }
                    }
                }
                else// Đang ở trạng thái xóa
                {
                    // Nếu trước đó đang ở trạng thái text thì lấy nội dung của text và xóa textbox đi
                    if (_myData.Count() > 0 && _myData[_myData.Count() - 1].State.ShapeType1 == ShapeType.MyText)
                    {
                        _myData[_myData.Count() - 1].State.Text = textBox.Text;
                        this.Invalidate();
                    }
                    // Xóa đi các textbox
                    this.Controls.Clear();
                    textBox = new TextBox();
                    //
                    // Nếu trước đó là một hình nào đó mà không phải là text hay border thì chắc chắn có bao khung --> xóa bao khung đi
                    if (_myData.Count() > 0 && _myData[_myData.Count() - 1].State.ShapeType1 != ShapeType.MyText && _myData[_myData.Count() - 1].State.ShapeType1 != ShapeType.MyEraser)
                    {
                        _myData.RemoveAt(_myData.Count() - 1);
                    }
                    // Gởi hình vừa vẽ trước đó
                    if (_myData.Count() > 0)
                    {
                        MyShape temp = _myData[_myData.Count() - 1];
                        byte[] buffer = temp.State.ToByteArray();
                        if (_isClient && client != null && client.Connected)
                        {
                            try
                            {

                                client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                        {

                            foreach (var item in _clientSocket)
                            {
                                item.Send(buffer);
                            }

                        }
                    }
                    PaintState statetemp = new PaintState(myPaint);
                    MyShape a = new MyEraser(statetemp);
                    a.State.EraserPoint.Add(myPaint.FirstPoint);
                    _myData.Add(a);
                    this.Invalidate();
                }
              //
            }
            else //Trang thai tô
            {
                myPaint.FirstPoint = myPaint.SecondPoint = e.Location;

                // Nếu như đang network thì gởi hình vẽ trước đó đi
                if (_myData.Count() > 0)
                {
                    MyShape shapetemp = _myData[_myData.Count() - 1];
                    byte[] buffertemp = shapetemp.State.ToByteArray();
                    if (_isClient && client != null && client.Connected)
                    {
                        try
                        {
                            client.BeginSend(buffertemp, 0, buffertemp.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                    {

                        foreach (var item in _clientSocket)
                        {
                            item.Send(buffertemp);
                        }

                    }

                }           

                floodFill(e.Location,_fillColor);

                //Lưu dưới dạng đối tượng
                myPaint.PointFill = e.Location;
                myPaint.RF1 = _fillColor.R;
                myPaint.GF1 = _fillColor.G;
                myPaint.BF1 = _fillColor.B;
                PaintState t3 = new PaintState(myPaint);
                MyShape t1 = new MyFill(t3);
            
                _myData.Add(t1);
                ///

                //Gởi trạng thái tô

                byte[] buffer1 = t3.ToByteArray();
                if (_isClient && client != null && client.Connected)
                {
                    try
                    {
                        client.BeginSend(buffer1, 0, buffer1.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                {

                    foreach (var item in _clientSocket)
                    {
                        item.Send(buffer1);
                    }

                }

            }

        }
        public void floodFill(Point e, Color colorFill)
        {
            Color first = new Color();// Màu đầu tiên tại điểm click chuột

            int[,] maTrix = new int[this.Width, this.Height];

            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    maTrix[i, j] = 0;
                }
            }


            Queue<Point> queue = new Queue<Point>();

            Bitmap bmp = new Bitmap(this.Width, this.Height);

            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));

            first = bmp.GetPixel(e.X, e.Y);// Lấy màu tại điểm click chuột

            queue.Enqueue(e);

            int[] x = new int[4] { 1, 0, -1, 0 };
            int[] y = new int[4] { 0, 1, 0, -1 };

            while (queue.Count() != 0)
            {
                Point temp = new Point();

                temp = queue.Dequeue();

                Color col = bmp.GetPixel(temp.X, temp.Y);

                if (maTrix[temp.X, temp.Y] == 1)// Nếu điểm đó đã tô rồi thì không xét điểm đó nữa
                    continue;

                if (!isSameColor(col, colorFill) && isSameColor(col, first)) // Nếu điểm đang xét không cùng màu với màu tô và cùng màu với điểm đầu tiên
                {
                    maTrix[temp.X, temp.Y] = 1;
                    bmp.SetPixel(temp.X, temp.Y, colorFill);
                }

                for (int i = 0; i < 4; i++)
                {
                    Point t = new Point(temp.X + x[i], temp.Y + y[i]);

                    // Nếu như ra khỏi khung hình hoặc còn đang ở trong hàng đợi thì không xét
                    if (t.X >= this.Width  || t.Y >= this.Height || t.X <= 0 || t.Y <= 0 || maTrix[t.X, t.Y] == 2)
                        continue;

                    col = bmp.GetPixel(t.X, t.Y);
                    if (!isSameColor(col, colorFill) && isSameColor(col, first))//, Color.Black))
                    {
                        maTrix[t.X, t.Y] = 2;
                        queue.Enqueue(t);
                    }
                }
            }

            this.buffer = bmp;

            this.BackgroundImage = new Bitmap(buffer);

            this.Invalidate();

        }
        /// <summary>
        /// Hàm kiểm tra xem 2 màu có giống nhau không (Ba thành phần màu bằng nhau)
        /// </summary>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        public bool isSameColor(Color col1, Color col2)
        {
            return (col1.R == col2.R && col1.G == col2.G && col1.B == col2.B);
        }

        private void PaintPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isFill)
            {
                myPaint.SecondPoint = e.Location;
                if (myPaint.ShapeType1 != ShapeType.MyEraser)
                {
                    int n = _myData.Count();
                    // Nếu như vừa Resize xong thì có thể điểm đầu tiên và điểm kết thúc của hình bị sai
                    // vì thế cần phải gán lại điểm đầu tiên và kết thúc của hình
                    if (_isReSize)
                    {
                        if (_shapeCurrent.State.ShapeType1 != ShapeType.MyLine)
                        {
                            Point Start = new Point();
                            Point End = new Point();
                            _shapeCurrent.findBourder(ref Start, ref End);
                            _shapeCurrent.State.FirstPoint = Start;
                            _shapeCurrent.State.SecondPoint = End;
                        }

                    }
                    if (_isCanMove)
                    {

                        _isCanMove = false;

                        int dentaX = myPaint.SecondPoint.X - myPaint.FirstPoint.X;
                        int dentaY = myPaint.SecondPoint.Y - myPaint.FirstPoint.Y;

                        _myData[n - 1].State.FirstPoint = new Point(_shapeCurrent.State.FirstPoint.X + dentaX, _shapeCurrent.State.FirstPoint.Y + dentaY);
                        _myData[n - 1].State.SecondPoint = new Point(_shapeCurrent.State.SecondPoint.X + dentaX, _shapeCurrent.State.SecondPoint.Y + dentaY);

                    }

                    // Nếu đang ở chế độ Text thì tính kích thước cho textbox
                    if (myPaint.ShapeType1 == ShapeType.MyText)
                    {
                        this.Controls.Clear();
                        Point S = new Point ();
                        Point E = new Point ();
                        _myData[_myData.Count - 1].findBourder(ref S, ref E);
                        textBox = new TextBox();
                        _myData.RemoveAt(_myData.Count - 1);

                        textBox.Location = new Point(S.X, S.Y);


                        textBox.WordWrap = true;
                        textBox.Multiline = true;
                        textBox.AcceptsReturn = true;

                        textBox.Font = new System.Drawing.Font(myPaint.TextFont, myPaint.TextSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        textBox.ForeColor = myPaint.CurrentColor;
                        textBox.Size = new System.Drawing.Size(Math.Abs(myPaint.SecondPoint.X - myPaint.FirstPoint.X), Math.Abs(myPaint.SecondPoint.Y - myPaint.FirstPoint.Y));
                        this.Controls.Add(textBox);
                        this.Invalidate();
                       
                    }
                    else
                    {
                        // Vẽ khung viền cho hình vừa mới vẽ
                        MyShape b;
                        b = new MyBorder(_myData[n - 1].State);

                        _myData.Add(b);

                        _isRemoveBorder = false;

                        this.Invalidate();

                    }

                }
                else //Gởi cục tẩy đi
                {
                    if (_myData.Count() > 0)
                    {
                        MyShape temp = _myData[_myData.Count() - 1];
                        byte[] buffer = temp.State.ToByteArray();
                        if (_isClient && client != null && client.Connected)
                        {
                            try
                            {

                                client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (_isServer) //Nếu đang với vai trò là sever thì gởi hình trên sever về tất cả người kết nối
                        {

                            foreach (var item in _clientSocket)
                            {
                                item.Send(buffer);
                            }

                        }

                        _isRemoveBorder = true;
                        _isStillCanMove = _isStillCanReSize = false;

                    }
                  
                }

            }
            _isStillCanMove = true;
            _isStillCanReSize = true;
        }


        private Point f = new Point(); //Điểm tạm giữ lại điểm đầu của hình

        private void PaintPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isFill)
            {
                Point Start = new Point();
                Point End = new Point();
                // Hiển thị tương tác resize

                bool Line1 = false, Line2 = false;
                if (myPaint.ShapeType1 != ShapeType.MyEraser)
                {

                    if (_myData.Count() > 0 && _myData[_myData.Count() - 1].State.ShapeType1 != ShapeType.MyEraser)
                    {
                        MyShape temp = _myData[_myData.Count() - 1];
                        temp.findBourder(ref Start, ref End);
                    }

                    if (myPaint.ShapeType1 != ShapeType.MyLine)
                    {
                        if (_isStillCanReSize)
                        {
                            if (Math.Abs(e.Location.X - End.X) < 5 && (Math.Abs(e.Location.Y - End.Y) < 5) || (Math.Abs(e.Location.X - Start.X) < 5 && (Math.Abs(e.Location.Y - Start.Y) < 5)))
                            {
                                this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                            }
                            else
                            {
                                if (Math.Abs(e.Location.X - Start.X) < 5 && (Math.Abs(e.Location.Y - End.Y) < 5) || (Math.Abs(e.Location.X - End.X) < 5 && (Math.Abs(e.Location.Y - Start.Y) < 5)))
                                {
                                    this.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                                }
                                else
                                {

                                    if (Math.Abs(e.Location.X - (End.X + Start.X) / 2) < 5 && (Math.Abs(e.Location.Y - Start.Y) < 5) || (Math.Abs(e.Location.X - (End.X + Start.X) / 2) < 5 && (Math.Abs(e.Location.Y - End.Y) < 5)))
                                    {
                                        this.Cursor = System.Windows.Forms.Cursors.SizeNS;
                                    }
                                    else
                                    {
                                        if (Math.Abs(e.Location.X - Start.X) < 5 && (Math.Abs(e.Location.Y - (Start.Y + End.Y) / 2) < 5) || (Math.Abs(e.Location.X - End.X) < 5 && (Math.Abs(e.Location.Y - (Start.Y + End.Y) / 2) < 5)))
                                        {
                                            this.Cursor = System.Windows.Forms.Cursors.SizeWE;
                                        }
                                        else
                                        {
                                            this.Cursor = System.Windows.Forms.Cursors.Default;
                                        }

                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (_isStillCanReSize)
                        {
                            if (Math.Abs(e.Location.X - End.X) < 5 && (Math.Abs(e.Location.Y - End.Y) < 5) || (Math.Abs(e.Location.X - Start.X) < 5 && (Math.Abs(e.Location.Y - Start.Y) < 5)))
                            {
                                this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                                Line1 = true;
                            }
                            else
                            {
                                if (Math.Abs(e.Location.X - Start.X) < 5 && (Math.Abs(e.Location.Y - End.Y) < 5) || (Math.Abs(e.Location.X - End.X) < 5 && (Math.Abs(e.Location.Y - Start.Y) < 5)))
                                {
                                    this.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                                    Line2 = true;
                                }
                                else
                                {
                                    this.Cursor = System.Windows.Forms.Cursors.Default;
                                }
                            }
                        }

                    }

                    // Hiển thị tương tác di chuyển, ngoại trừ đường thẳng
                    if (_myData.Count() > 0 && _myData[_myData.Count() - 1].State.ShapeType1 != ShapeType.MyEraser)
                    {
                        _myData[_myData.Count() - 1].findBourder(ref Start, ref End);
                    }


                    if (myPaint.ShapeType1 != ShapeType.MyLine && _isStillCanMove)
                    {
                        if (_myData.Count() > 0 && e.Location.X > Start.X + 5 && e.Location.X < End.X - 5
                         && e.Location.Y > Start.Y + 5 && e.Location.Y < End.Y - 5)
                        {
                            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
                        }
                    }
                    else
                    {
                        if (_myData.Count() > 0 && _myData[_myData.Count() - 1].calcDistanceLine(e.Location) < 5 && !Line1 && !Line2
                            && e.Location.X > Start.X && e.Location.Y > Start.Y && e.Location.X < End.X && e.Location.Y < End.Y && _isStillCanMove)
                        {
                            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
                        }
                    }

                }
                if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
                {
                    myPaint.SecondPoint = e.Location;
                    if (_isReSize)
                    {
                        _shapeCurrent.findBourder(ref Start, ref End);
                        MyShape temp;
                        switch (_shapeCurrent.State.ShapeType1)
                        {
                            case ShapeType.MyLine:
                                temp = new MyLine(_shapeCurrent.State);
                                break;
                            case ShapeType.MyRectangle:
                                temp = new MyRectangle(_shapeCurrent.State);
                                break;
                            case ShapeType.MyEllipse:
                                temp = new MyEllipse(_shapeCurrent.State);
                                break;
                            case ShapeType.MyTriangle:
                                temp = new MyTriangle(_shapeCurrent.State);
                                break;
                            case ShapeType.MyText:
                                temp = new MyText(_shapeCurrent.State);
                                break;
                            default:
                                temp = new MyLine(_shapeCurrent.State);
                                break;
                        }
                        temp.State.Shift1 = false;
                        if (myPaint.ShapeType1 != ShapeType.MyLine) // Những hình khác đường thẳng
                        {
                            if (_isReSizePoint[3])
                            {
                                if (e.Location.X > Start.X && e.Location.Y > Start.Y)
                                {
                                    temp.State.SecondPoint = e.Location;
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }

                            }
                            if (_isReSizePoint[0])
                            {
                                if (e.Location.X < End.X && e.Location.Y < End.Y)
                                {
                                    temp.State.FirstPoint = End;
                                    temp.State.SecondPoint = e.Location;
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }

                            }
                            if (_isReSizePoint[1])
                            {
                                if (e.Location.X < End.X && e.Location.Y > Start.Y)
                                {
                                    temp.State.FirstPoint = new Point(End.X, Start.Y);
                                    temp.State.SecondPoint = e.Location;
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                            if (_isReSizePoint[2])
                            {
                                if (e.Location.X > Start.X && e.Location.Y < End.Y)
                                {
                                    temp.State.FirstPoint = new Point(Start.X, End.Y);
                                    temp.State.SecondPoint = e.Location;
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                            if (_isReSizePoint[4])
                            {
                                if (e.Location.X > Start.X && e.Location.X < End.X && e.Location.Y < End.Y)
                                {
                                    temp.State.SecondPoint = new Point(End.X, End.Y);
                                    temp.State.FirstPoint = new Point(Start.X, e.Location.Y);
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                            if (_isReSizePoint[5])
                            {
                                if (e.Location.X < End.X && e.Location.Y < End.Y)
                                {
                                    temp.State.SecondPoint = new Point(End.X, End.Y);
                                    temp.State.FirstPoint = new Point(e.Location.X, Start.Y);
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                            if (_isReSizePoint[6])
                            {
                                if (e.Location.X > Start.X && e.Location.X < End.X && e.Location.Y > Start.Y)
                                {
                                    temp.State.FirstPoint = new Point(Start.X, Start.Y);
                                    temp.State.SecondPoint = new Point(End.X, e.Location.Y);
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                            if (_isReSizePoint[7])
                            {
                                if (e.Location.X > Start.X && e.Location.Y < End.Y)
                                {
                                    temp.State.FirstPoint = new Point(Start.X, Start.Y);
                                    temp.State.SecondPoint = new Point(e.Location.X, End.Y);
                                    _myData.RemoveAt(_myData.Count() - 1);
                                    _myData.Add(temp);
                                    this.Invalidate();
                                }
                            }
                        }
                        else
                        {
                            if (_isReSizePoint[0])
                            {
                                _shapeCurrent.State.FirstPoint = f;
                                _shapeCurrent.State.SecondPoint = e.Location;
                                this.Invalidate();

                            }
                            if (_isReSizePoint[1])
                            {
                                _shapeCurrent.State.SecondPoint = e.Location;
                                this.Invalidate();

                            }
                        }
                    }
                    else
                    {
                        if (myPaint.ShapeType1 != ShapeType.MyText && myPaint.ShapeType1 != ShapeType.MyEraser)
                        {
                            if (_myData.Count() > 0)
                            {
                                _myData.RemoveAt(_myData.Count() - 1);
                            }

                            PaintState t = new PaintState(myPaint);

                            MyShape b;
                            switch (myPaint.ShapeType1)
                            {
                                case ShapeType.MyLine:
                                    b = new MyLine(t);
                                    break;
                                case ShapeType.MyRectangle:
                                    b = new MyRectangle(t);
                                    break;
                                case ShapeType.MyEllipse:
                                    b = new MyEllipse(t);
                                    break;
                                case ShapeType.MyTriangle:
                                    b = new MyTriangle(t);
                                    break;
                                default:
                                    b = new MyLine(t);
                                    break;
                            }

                            _myData.Add(b);


                            int len = _myData.Count();


                            if ((Control.ModifierKeys & Keys.Shift) != 0)
                            {
                                _myData[len - 1].State.Shift1 = true;
                                // Nễu là đường thẳng thì có chức năng vẽ 8 hướng
                                if (_myData[len - 1].State.ShapeType1 == ShapeType.MyLine)
                                {
                                    int location = _myData[len - 1].getLocation();

                                    int temp = (int)_myData[len - 1].calcDistance(_myData[len - 1].State.FirstPoint, _myData[len - 1].State.SecondPoint);

                                    switch (location)
                                    {
                                        case 1:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X + temp, _myData[len - 1].State.FirstPoint.Y - temp);
                                            break;
                                        case 2:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X + temp, _myData[len - 1].State.FirstPoint.Y);
                                            break;
                                        case 3:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X + temp, _myData[len - 1].State.FirstPoint.Y + temp);
                                            break;
                                        case 4:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X, _myData[len - 1].State.FirstPoint.Y + temp);
                                            break;
                                        case 5:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X - temp, _myData[len - 1].State.FirstPoint.Y + temp);
                                            break;
                                        case 6:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X - temp, _myData[len - 1].State.FirstPoint.Y);
                                            break;
                                        case 7:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X - temp, _myData[len - 1].State.FirstPoint.Y - temp);
                                            break;
                                        case 8:
                                            _myData[len - 1].State.SecondPoint = new Point(_myData[len - 1].State.FirstPoint.X, _myData[len - 1].State.FirstPoint.Y - temp);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                _myData[len - 1].State.Shift1 = false;
                            }

                            this.Invalidate();

                            int n = _myData.Count();
                            if (_isCanMove) // Sự kiện di chuyển hình
                            {

                                int dentaX = myPaint.SecondPoint.X - myPaint.FirstPoint.X;
                                int dentaY = myPaint.SecondPoint.Y - myPaint.FirstPoint.Y;

                                PaintState temp = new PaintState(myPaint);
                                MyShape a;
                                switch (myPaint.ShapeType1)
                                {
                                    case ShapeType.MyLine:
                                        a = new MyLine(temp);
                                        break;
                                    case ShapeType.MyRectangle:
                                        a = new MyRectangle(temp);
                                        break;
                                    case ShapeType.MyEllipse:
                                        a = new MyEllipse(temp);
                                        break;
                                    case ShapeType.MyTriangle:
                                        a = new MyTriangle(temp);
                                        break;
                                    default:
                                        a = new MyLine(temp);
                                        break;
                                }

                                a.State.FirstPoint = new Point(_shapeCurrent.State.FirstPoint.X + dentaX, _shapeCurrent.State.FirstPoint.Y + dentaY);
                                a.State.SecondPoint = new Point(_shapeCurrent.State.SecondPoint.X + dentaX, _shapeCurrent.State.SecondPoint.Y + dentaY);
                                a.State.Shift1 = _shapeCurrent.State.Shift1;

                                _myData[n - 1] = a;
                                this.Invalidate();

                            }

                        }
                        else
                        {
                            if (myPaint.ShapeType1 == ShapeType.MyEraser)
                            {
                                ((MyEraser)_myData[_myData.Count() - 1]).State.EraserPoint.Add(e.Location);
        
                                this.Invalidate();

                            }
                            else
                            {
                                _myData[_myData.Count - 1].State.SecondPoint = e.Location;
                            
                                this.Invalidate();
                            }
                        }
                    }
                }
            }
        }
        public void SaveImage(string name)
        {
            int n = _myData.Count();
            // Xóa bao khung
            if (n > 2)
            {
                if (_myData[n - 1].State.ShapeType1 == _myData[n - 2].State.ShapeType1)
                {
                    _myData.RemoveAt(n - 1);
                    n--;
                }
            }
            Bitmap bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));

            Graphics g = Graphics.FromImage(bmp);

            g.Dispose();
            bmp.Save(name, ImageFormat.Png);
        }

        // Phần cài đặt của Server
        public int _numMember; //Số lượng người kết nối
        public Socket _serverSocket;
        public List<Socket> _clientSocket = new List<Socket>(); //Danh sách các Client kết nối
        private int _currenmember = -1; 
      
        public List<Socket> ClientSocket
        {
            get { return _clientSocket; }
            set { _clientSocket = value; }
        }
        private byte[] _buffer;// Gói data gởi, nhận

        public void StartServer(string port)
        {
            try
            {         
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Int32.Parse(port)));
                _serverSocket.Listen(10);


                _statusNetWork = "Create room Success. Wating for another member";
                if (STATUS != null)
                    STATUS(_statusNetWork);

                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallbackServer), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AcceptCallbackServer(IAsyncResult AR)
        {
            Socket socket;
            try
            {

                socket = _serverSocket.EndAccept(AR);
  
                _clientSocket.Add(socket);

                if (CONTROL != null)
                {
                    CONTROL(1);
                }

                PaintState temp = new PaintState();
              
                byte[] t = new byte[10000];
                // Nếu đã đủ số lượng người vào phòng thì bắt đầu vẽ
                if (_clientSocket.Count() == _numMember)
                {
                    _statusNetWork = "All member connected. Click start so as to painting";
 
                    if (STATUS != null)
                        STATUS(_statusNetWork);

                }
                else // Ngược lại nếu chưa đủ người thì gởi 1 gói dữ liệu giả để disable tất cả người kết nối
                {
                    _serverSocket.BeginAccept(new AsyncCallback(AcceptCallbackServer), null);
                    _statusNetWork = "Create room Success. Connect: " + _clientSocket.Count().ToString() + " / " + _numMember.ToString();

                    if (STATUS != null)
                        STATUS(_statusNetWork);

                    this.Enabled = false;

                }

                temp.FirstPoint = new Point(-1, -1);

                t = temp.ToByteArray();
                foreach (var item in _clientSocket)
                {
                    item.Send(t);
                }

                _buffer = new byte[socket.ReceiveBufferSize];
                socket.BeginReceive(_buffer, 0, 10000, SocketFlags.None, ReceiveCallbackServer, socket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveCallbackServer(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            int index = _clientSocket.IndexOf(current);
            try
            {
                received = current.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }
                PaintState recPaintState = new PaintState(_buffer);
            
                if (recPaintState.FirstPoint.X == -2 && recPaintState.FirstPoint.Y == -2)
                {
                    //Cập nhật giao diện
                    
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    _clientSocket.Remove(current);
                    if (REMOVEMEM != null)
                    {
                        REMOVEMEM(index);
                    }


                    PaintState t = new PaintState();
                    t.FirstPoint = new Point(-9, -9);
                    t.SecondPoint = new Point(index, 0);
                    byte[] b = t.ToByteArray();
                    for (int i = 0; i < _clientSocket.Count; i++)
                    {
                        _clientSocket[i].Send(b);
                    }

                    return;
                }
                else
                {
                    if ((recPaintState.FirstPoint.X == -4 && recPaintState.FirstPoint.Y == -4))
                    {

                        if (MessageBox.Show("Member: " + recPaintState.Text + " want to draw. Click Yes if you allow " + recPaintState.Text + " draw.", "Request",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {

                            PaintState temp = new PaintState();
                            byte[] t = new byte[10000];

                            _currenmember = _clientSocket.IndexOf(current);

                            temp.FirstPoint = new Point(-4, -4);

                            t = temp.ToByteArray();
                            current.Send(t);
                            ABLEORDISABLE(index, true);
                        }
                    }
                    else
                    {
                        //Nhận tên thành viên
                        if (recPaintState.FirstPoint.X == -7 && recPaintState.FirstPoint.Y == -7)
                        {
                            // _isnextavatar = true;
                            if (ROOM != null && !_startRoom)
                            {
                                ROOM(recPaintState.Text);
                            }

                        }
                        else //Nhân hình vẽ của các thành viên
                        {
                            AddListShape(recPaintState, index);                        
                        }

                    }
                    
                }
                //Bắt đầu lắng nghe gói dữ liệu khác
                current.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackServer), current);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Hàm thêm hình của thành viên vào danh sách các hình của Server
        /// </summary>
        /// <param name="person"></param>
        /// <param name="index"></param>
        private void AddListShape(PaintState person, int index)
        {
            if (person.FirstPoint.X > 0 && person.FirstPoint.Y > 0)
            {
                person.CurrentColor = Color.FromArgb(person.R1, person.G1, person.B1);
                MyShape a;
                switch (person.ShapeType1)
                {
                    case ShapeType.MyLine:
                        a = new MyLine(person);
                        break;
                    case ShapeType.MyRectangle:
                        a = new MyRectangle(person);
                        break;
                    case ShapeType.MyEllipse:
                        a = new MyEllipse(person);
                        break;
                    case ShapeType.MyTriangle:
                        a = new MyTriangle(person);
                        break;
                    case ShapeType.MyText:
                        a = new MyText(person);
                        break;
                    case ShapeType.MyEraser:
                        a = new MyEraser(person);
                        break;
                    case ShapeType.MyFill:
                        {

                            if (_myData.Count() > 0 && !_isRemoveBorder)
                            {
                                _myData.RemoveAt(_myData.Count() - 1);
                            }
                            if (_myData.Count() > 0 && _isStillCanMove && _isStillCanReSize)
                            {
                                _myData.RemoveAt(_myData.Count() - 1);
                                _isStillCanReSize = _isStillCanMove = false;
                            }

                            a = new MyFill(person);
                            floodFill(a.State.PointFill, a.State.ColorFill);

                            break;
                        }
                    default:
                        a = new MyLine(person);
                        break;
                }
                if (person.ShapeType1 == ShapeType.MyFill)
                {
                    _myData.Add(a);
                    _isRemoveBorder = true;
                    _isStillCanMove = _isStillCanReSize = false;

                }
                else
                {
                    if (_myData.Count() > 1 && _isStillCanMove == true && _isStillCanReSize == true)
                    {
                        _myData.Insert(_myData.Count() - 2, a);
                    }
                    else
                    {
                        _myData.Add(a);
                        _isRemoveBorder = true;
                        _isStillCanMove = _isStillCanReSize = false;
                    }
                }

                for (int i = 0; i < _clientSocket.Count; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }
                    _clientSocket[i].Send(_buffer);
                }
                
                //Gởi hình nhận đến tất cả mọi người.
                this.Invalidate();
            }
           
        }
        /// <summary>
        /// Hàm nhận hình từ Server gởi đến 
        /// </summary>
        /// <param name="person"></param>
        private void AddListShape(PaintState person)
        {
            if (person.FirstPoint.X > 0 && person.FirstPoint.Y > 0)
            {
                person.CurrentColor = Color.FromArgb(person.R1, person.G1, person.B1);
                MyShape a;
                switch (person.ShapeType1)
                {
                    case ShapeType.MyLine:
                        a = new MyLine(person);
                        break;
                    case ShapeType.MyRectangle:
                        a = new MyRectangle(person);
                        break;
                    case ShapeType.MyEllipse:
                        a = new MyEllipse(person);
                        break;
                    case ShapeType.MyTriangle:
                        a = new MyTriangle(person);
                        break;
                    case ShapeType.MyText:
                        a = new MyText(person);
                        break;
                    case ShapeType.MyEraser:
                        a = new MyEraser(person);
                        break;
                    case ShapeType.MyFill:
                        {

                            if (_myData.Count() > 0 && !_isRemoveBorder)
                            {
                                _myData.RemoveAt(_myData.Count() - 1);
                            }
                            if (_myData.Count() > 0 && _isStillCanMove && _isStillCanReSize)
                            {
                                _myData.RemoveAt(_myData.Count() - 1);
                                _isStillCanReSize = _isStillCanMove = false;
                            }

                            a = new MyFill(person);
                            floodFill(a.State.PointFill, a.State.ColorFill);

                            break;
                        }
                    default:
                        a = new MyLine(person);
                        break;
                }
                if (person.ShapeType1 == ShapeType.MyFill)
                {
                    _myData.Add(a);
                    _isRemoveBorder = true;
                    _isStillCanMove = _isStillCanReSize = false;
                    
                }
                else
                {
                    if (_myData.Count() > 1 && _isStillCanMove == true && _isStillCanReSize == true)
                    {
                        _myData.Insert(_myData.Count() - 2, a);
                    }
                    else
                    {
                        _myData.Add(a);
                        _isRemoveBorder = true;
                        _isStillCanMove = _isStillCanReSize = false;
                     }
                }
                this.Invalidate();
            }

        }
        
        /// <summary>
        /// Hàm disConnect đến thành viên index
        /// </summary>
        /// <param name="index">index của thành viên trong danh sách các thành viên</param>
        public void disConnectMember(int index)
        {
            PaintState temp = new PaintState();
            temp.FirstPoint = new Point(-2, -2);
            byte[] buffer = temp.ToByteArray();
            try
            {
                _clientSocket[index].Send(buffer);

                Thread.Sleep(100);

                _clientSocket[index].Shutdown(SocketShutdown.Both);
                _clientSocket[index].Close();
                _clientSocket.RemoveAt(index);

                _statusNetWork = "Connect: " + _clientSocket.Count().ToString() + " / " + _numMember.ToString();

                if (STATUS != null)
                    STATUS(_statusNetWork);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           

        }

        /// <summary>
        /// Hàm disconnect đến tất cả các thành viên trong Room
        /// </summary>
        public void disconnectToAllClient()
        {
            if (_isServer && _serverSocket != null)
            {
                for (int i = 0; i < _clientSocket.Count(); i++ )
                {

                    PaintState temp = new PaintState();
                    temp.FirstPoint = new Point(-2, -2);
                    byte[] buffer = temp.ToByteArray();
                    try
                    {
                        _clientSocket[i].Send(buffer);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

                _clientSocket.Clear();
                if (_serverSocket.Connected)
                {
                    _serverSocket.Close();
                   
                }
              
            }

        }



        // Phần cài đặt của Client 
        private Socket client;
        private byte[] _bufferClient;
        private string _name; 
        private bool _isRealConnect = false; // Biến kiểm tra xem client có đang thực sự kết nối tới Server không
        public bool _isStartRoom = false;
        public void Connect(string IP, string port, string name)
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Parse(IP), Int32.Parse(port));
                _bufferClient = new byte[10000];

                _name = name;
                //Gởi tên 
                if (client != null && client.Connected)
                {
                    PaintState temp = new PaintState();
                    temp.FirstPoint = new Point(-7, -7);
                    temp.Text = _name;
                    byte[] buffer = temp.ToByteArray();
                    try
                    {
                        client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    client.BeginReceive(_bufferClient, 0, 10000, SocketFlags.None, ReceiveCallbackClient, client);
                }
                else
                {
                    _statusNetWork = "Connect to server fail ...";
                    if (STATUS != null)
                        STATUS(_statusNetWork);
                }

            }
            catch (Exception)
            {
                _statusNetWork = "Connect to server fail ...";
                if (STATUS != null)
                    STATUS(_statusNetWork);
            }
        }

        private void ConnectCallbackClient(IAsyncResult AR)
        {
            try
            {
       
                client.EndConnect(AR);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void ReceiveCallbackClient(IAsyncResult AR)
        {

            int received;

            try
            {
                received = client.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }

                PaintState person = new PaintState(_bufferClient);

                //Nhận trạng thái không cho vẽ
                if (person.FirstPoint.X == -8 && person.FirstPoint.Y == -8)
                {
                    if (_myData.Count() > 0 && !_isRemoveBorder)
                    {
                        _myData.RemoveAt(_myData.Count() - 1);
                    }
                    if (_myData.Count() > 0 && _isStillCanMove && _isStillCanReSize && _myData[_myData.Count() -1].State.ShapeType1 != ShapeType.MyEraser)
                    {
                        _myData.RemoveAt(_myData.Count() - 1);
                        _isStillCanReSize = _isStillCanMove = false;
                    }

                    _statusNetWork = "Server disabled to you, you not allow draw.....";
                    if (STATUS != null)
                        STATUS(_statusNetWork);

                    this.Enabled = false;
                }
                else
                {
                    //Nhận trạng thái thông báo các thành viên có trong phòng
                    if (person.FirstPoint.X == -7 && person.FirstPoint.Y == -7)
                    {

                        if (ROOMCLIENT != null)
                        {
                            ROOMCLIENT(person.Text);
                        }
                    }
                    else
                    {
                        //Nhận thông báo loại thành viên
                        if (person.FirstPoint.X == -9 && person.FirstPoint.Y == -9)
                        {
                            if (REMOVEROOMCLIENT != null)
                            {
                                REMOVEROOMCLIENT(person.SecondPoint.X);
                            }
                        }
                        else
                        {
                            //Nhận trạng thái ngắt kết nối từ server
                            if (person.FirstPoint.X == -2 && person.FirstPoint.Y == -2)
                            {
                                _statusNetWork = "Server disconnected to you ...";
                                if (STATUS != null)
                                    STATUS(_statusNetWork);

                                this.Enabled = true;

                                _isRealConnect = false;

                                client.Shutdown(SocketShutdown.Both);
                                client.Close();
                                if (CONTROL != null)
                                {
                                    CONTROL(1);
                                }
                                if (REMOVEROOMCLIENT != null)
                                {
                                    REMOVEROOMCLIENT(-1);
                                }
                                return;
                            }
                            else
                            {
                                //Nhận trạng thái bắt đầu vẽ
                                if (person.FirstPoint.X == -6 && person.FirstPoint.Y == -6)
                                {
                                    _statusNetWork = "Server start paiting ...";
                                    if (STATUS != null)
                                        STATUS(_statusNetWork);
                                    _isStartRoom = true;
                                    if (CONTROL != null)
                                    {
                                        CONTROL(0);
                                    }
                                }
                                else
                                {
                                    if (person.FirstPoint.X == -1 && person.FirstPoint.Y == -1)
                                    {
                                        _isRealConnect = true;
                                        _statusNetWork = "Connected to server success. You not allow draw ...";
                                        if (STATUS != null)
                                            STATUS(_statusNetWork);

                                        this.Enabled = false;
                                    }
                                    else
                                    {
                                        //Yêu cầu vẽ được Server chấp nhận
                                        if (person.FirstPoint.X == -4 && person.FirstPoint.Y == -4)
                                        {
                                            _isRemoveBorder = true;
                                            _isStillCanMove = _isStillCanReSize = false;
                                            _statusNetWork = "You allowed draw ...";
                                            if (STATUS != null)
                                                STATUS(_statusNetWork);
                                            this.Enabled = true;
                                        }
                                        else
                                        {
                                            AddListShape(person);
                                        }

                                    }
                                }
                            }

                        }
                    }
                }              

                client.BeginReceive(_bufferClient, 0, _bufferClient.Length, SocketFlags.None, new AsyncCallback(ReceiveCallbackClient), client);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendCallbackClient(IAsyncResult AR)
        {
            try
            {
                client.EndSend(AR);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gởi yêu cầu vẽ lên Server
        /// </summary>
        public void sendRequest()
        {
            if (_isClient && client != null && client.Connected)
            {
                /////////
                PaintState temp = new PaintState();

                temp.FirstPoint = new Point(0, 0);
                
                byte[] buf = temp.ToByteArray();

                try
                {
                    client.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
                //////
                Thread.Sleep(20);
                temp = new PaintState();
                temp.FirstPoint = new Point(-4, -4);
                temp.Text = _name;
                buf = temp.ToByteArray();
                try
                {
                    client.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
         
            }

        }
        public void disconnectToServer()
        {    
            if (_isClient && client != null && client.Connected && _isRealConnect)
            {
                PaintState temp = new PaintState();
                temp.FirstPoint = new Point(0, 0);
                byte[] buf = temp.ToByteArray();

                try
                {
                    client.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Thread.Sleep(100);
           
                temp = new PaintState();
                temp.FirstPoint = new Point(-2, -2);
                buf = temp.ToByteArray();

                try
                {
                    client.BeginSend(buf, 0,buf.Length, SocketFlags.None, new AsyncCallback(SendCallbackClient), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //Nếu server chưa kịp ngắt kết nối với người dùng thì sẽ xảy ra lỗi 
                //"cannot access a disposed object"
                //Tại đây nên để Thread nghỉ 20ms để Server kịp ngắt kết nối
                Thread.Sleep(100);
      
                //
                client.Shutdown(SocketShutdown.Both);
                client.Close();


                if (REMOVEROOMCLIENT != null)
                {
                    REMOVEROOMCLIENT(-1);
                }
            }

        }


    }

}
