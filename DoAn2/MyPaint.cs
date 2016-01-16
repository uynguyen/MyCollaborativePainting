using PanelControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DoAn2
{
    public partial class MyPaint : Form
    {

        List<RibbonButton> _myMember = new List<RibbonButton>(); //List các chứa các button thành viên
        
        public MyPaint(short option)
        {
            InitializeComponent();
            for (int i = 5; i <= 50; i++)
            {
                RibbonLabel temp = new RibbonLabel();
                temp.Text = i.ToString();
                this.rbcmbbx_SizeText.DropDownItems.Add(temp);
            }
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (FontFamily family in fonts.Families)
            {
                RibbonLabel temp = new RibbonLabel();
                temp.Text = family.Name.ToString();
                this.rbcmbbx_Font.DropDownItems.Add(temp);
         
            }

            if (option == 0) //Offline
            {
                rbtb_NetWorkServer.Visible = false;
                rbtb_NetWorkClient.Visible = false;
                PnlPaint._isClient = PnlPaint._isServer = false;
            }
            else// Online
            {
                PnlPaint._isFill = false;
                PnlPaint._isStillCanMove = false;
                PnlPaint._isStillCanReSize = false;
                PnlPaint.myPaint.ShapeType1 = ShapeType.MyLine;
                PnlPaint.ROOM += updateRoom;
                PnlPaint.STATUS += update;
                PnlPaint.CONTROL += updateControl;
                PnlPaint.REMOVEMEM += removeMem;
                PnlPaint.ROOMCLIENT += roomclient;
                PnlPaint.ABLEORDISABLE += ableordisable;
                PnlPaint.REMOVEROOMCLIENT += removeroomClient;
                if (option == 1) // server
                {
                    PnlPaint._isServer = true;
                    PnlPaint._isClient = false;
                    rbtb_NetWorkClient.Visible = false;
                }
                else //Client
                {
                    PnlPaint._isClient = true;
                    PnlPaint._isServer = false;
                    rbtb_NetWorkServer.Visible = false;
                }
               
            }
         


        }

        /// <summary>
        /// Hàm remove một thành viên trong phòng vẽ
        /// </summary>
        /// <param name="index">Index của thành viên trong List</param>
        public void removeroomClient(int index)
        {
            if (index == -1)
            {
                _myMember.Clear();
                rbpnl_RoomClient.Items.Clear();
            }
            else
            {
                _myMember.RemoveAt(index);
                rbpnl_RoomClient.Items.RemoveAt(index);
            }
            rbbtn_Disconnect.Enabled = true;
        }

        /// <summary>
        /// Hàm cập nhật trạng thái được vẽ hay không được phép vẽ của các thành viên trong phòng
        /// </summary>
        /// <param name="index">Index của thành viên trong List</param>
        /// <param name="flag">Cờ hiệu option ( true được phép vẽ, false không được phép vẽ)</param>
        public void ableordisable(int index, bool flag)
        {
            _myMember[index].Text = _myMember[index].Text.Remove(_myMember[index].Text.Length - 3, 3);
            if (flag)
            {
                
                _myMember[index].Text += "(T)";
            }
            else
            {

                _myMember[index].Text += "(F)";
            }

        }
        
        /// <summary>
        /// Cập nhật thành viên trong phòng phía client
        /// </summary>
        /// <param name="name"></param>
        public void roomclient(string name)
        {
            RibbonButton button = new RibbonButton();
            button.Text = name;

            button.MaxSizeMode = System.Windows.Forms.RibbonElementSizeMode.Medium;
            button.MinSizeMode = System.Windows.Forms.RibbonElementSizeMode.Medium;
            _myMember.Add(button);
            rbpnl_RoomClient.Items.Add(button);
            rbbtn_Connect.Enabled = false;
        }
        /// <summary>
        /// Sự kiện cập nhật thông báo các trạng thái vẽ (Tạo phòng, người vẽ hiện tại ....)
        /// </summary>
        /// <param name="status">Trạng thái</param>
        public void update(string status)
        {
            rblbl_StatusServer.Text = PnlPaint.StatusNetWork;
            rblbl_StatusClient.Text = PnlPaint.StatusNetWork;
        }


        public void removeMem(int index)
        {
            _myMember.RemoveAt(index);

            rbpnl_Room.Items.RemoveAt(index);
            if (!PnlPaint._startRoom)
            {
                PnlPaint._serverSocket.BeginAccept(new AsyncCallback(PnlPaint.AcceptCallbackServer), null);
            }
            if (PnlPaint._clientSocket.Count != PnlPaint._numMember && !PnlPaint._startRoom)
            {
                rbbtn_Start.Enabled = false;
                rblbl_StatusServer.Text = "Waiting for another member";
            }
        }

        /// <summary>
        /// Hàm cập nhật phòng phía Server
        /// </summary>
        /// <param name="_name">Tên của thành viên</param>
        public void updateRoom(string _name)
        {
            if (_myMember.Count < PnlPaint._clientSocket.Count) 
            {
                RibbonButton button = new RibbonButton();
                button.Text = _name + "(F)";
                button.MaxSizeMode = System.Windows.Forms.RibbonElementSizeMode.Medium;
                button.MinSizeMode = System.Windows.Forms.RibbonElementSizeMode.Medium;
 
                button.Click += Remove;
                _myMember.Add(button);
                rbpnl_Room.Items.Add(button);
                rbbtn_Create.Enabled = false;

                //Gởi thành viên vừa mới gia nhập đến các thành viên còn lại
                PaintState temp = new PaintState();
                byte[] buffer;

                for (int i = 0; i < _myMember.Count - 1; i++)
                {
                    string t = _myMember[i].Text.Remove(_myMember[i].Text.Length - 3, 3);
                    temp = new PaintState();

                    temp.FirstPoint = new Point(-7, -7);

                    temp.Text = t;
                    buffer = temp.ToByteArray();
                    PnlPaint._clientSocket[PnlPaint._clientSocket.Count - 1].Send(buffer);
                    Thread.Sleep(50);

                }

               
                Thread.Sleep(50);
                temp = new PaintState();

                temp.FirstPoint = new Point(-7, -7);

                temp.Text = _name;

                buffer = temp.ToByteArray();
                for (int i = 0; i < PnlPaint._clientSocket.Count; i++)
                {
                    PnlPaint._clientSocket[i].Send(buffer);
                }

            }

        }

        /// <summary>
        /// Cập nhật các button
        /// </summary>
        /// <param name="option"></param>
        public void updateControl(int option)
        {
            if (option == 1)
            {
                if (PnlPaint._numMember == PnlPaint._clientSocket.Count())
                {
                    rbbtn_Start.Enabled = true;
                }
                else
                {
                    rbbtn_Start.Enabled = false;
                }
                if (PnlPaint._isClient && !PnlPaint._isStartRoom)
                {
                    rbbtn_Connect.Enabled = true;
                }
            }
            else
            {
                if (PnlPaint._isClient)
                {
                    rbbtn_Request.Enabled = true;
                }
                if (PnlPaint._isServer)
                {
                    rbbtn_Start.Enabled = false;
                }
            }


        }

        private void btnPenColor_Click(object sender, EventArgs e)
        {
            clrDlg.ShowDialog();
            PnlPaint.myPaint.CurrentColor = clrDlg.Color;
            PnlPaint.myPaint.R1 = PnlPaint.myPaint.CurrentColor.R;
            PnlPaint.myPaint.G1 = PnlPaint.myPaint.CurrentColor.G;
            PnlPaint.myPaint.B1 = PnlPaint.myPaint.CurrentColor.B;
        }

        /// <summary>
        /// Hàm xét thử đã xóa bao khung chưa, nếu chưa thì thực hiện xóa bao khung
        /// </summary>
        public void removeBorder()
        {
            int n = PnlPaint.MyData.Count();
            if (n > 0 && PnlPaint.MyData[n - 1].State.ShapeType1 != ShapeType.MyText && !PnlPaint._isRemoveBorder)
            {
                PnlPaint.MyData.RemoveAt(n - 1);
                n--;
                PnlPaint._isRemoveBorder = true;
                PnlPaint.Invalidate();
            }
        }

        public void initRGB()
        {
            PnlPaint.myPaint.R1 = PnlPaint.myPaint.CurrentColor.R;
            PnlPaint.myPaint.G1 = PnlPaint.myPaint.CurrentColor.G;
            PnlPaint.myPaint.B1 = PnlPaint.myPaint.CurrentColor.B;
        }
        private void rbbtn_PenColor_Click(object sender, EventArgs e)
        {
            clrDlg.ShowDialog();
            PnlPaint.myPaint.CurrentColor = clrDlg.Color;
            initRGB();


            rbClrChss_ColorChooser.Color = clrDlg.Color;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], clrDlg.Color);
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.textBox.ForeColor = clrDlg.Color;
        }

        private void rbbtn_BlackColor_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.CurrentColor = Color.Black;
            initRGB();


            rbClrChss_ColorChooser.Color = Color.Black;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], Color.Black);
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.textBox.ForeColor = Color.Black;
        }

        private void rbbtn_GreenColor_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.CurrentColor = Color.Lime;
            initRGB();
            rbClrChss_ColorChooser.Color = Color.Lime;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], Color.Lime);
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.textBox.ForeColor = Color.Lime;
        }

        private void rbbtn_Yellow_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.CurrentColor = Color.Yellow;
            initRGB();

            rbClrChss_ColorChooser.Color = Color.Yellow;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], Color.Yellow);
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.textBox.ForeColor = Color.Yellow;
        }

        private void rbbtn_RedColor_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.CurrentColor = Color.Red;
            initRGB();

            rbClrChss_ColorChooser.Color = Color.Red;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], Color.Red);
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.textBox.ForeColor = Color.Red;
        }


        public void updateShape(MyShape shape, Color r)
        {
            shape.State.CurrentColor = r;
            shape.State.R1 = r.R;
            shape.State.G1 = r.G;
            shape.State.B1 = r.B;

        }

        private void rbbtn_OrangeColor_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.CurrentColor = Color.Orange;
            initRGB();

            rbClrChss_ColorChooser.Color = Color.Orange;

            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    updateShape(PnlPaint.MyData[PnlPaint.MyData.Count() - 1], Color.Orange);
                    PnlPaint.Invalidate();
                }
            }
        
            PnlPaint.textBox.ForeColor = Color.Orange;

        }

        private void rbbtn_Create_Click(object sender, EventArgs e)
        {
            if (PnlPaint.MyData.Count > 0)
            {
                if (MessageBox.Show("You are drawing! Click yes if you want to save your image before create a new room.", "Save information",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    svflDlg_SaveFile.Title = "Save Image";

                    if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string name = svflDlg_SaveFile.FileName;
                        PnlPaint.SaveImage(name);
                    }
                }
                else
                {
                    PnlPaint.MyData.Clear();
                    PnlPaint.buffer = new Bitmap(1, 1);
                    PnlPaint.Invalidate();
                }
            }

            rbmn_LoadProject.Enabled = false;

            PnlPaint._isServer = true;
            rbtb_NetWorkClient.Visible = false;

            PnlPaint._numMember = Int32.Parse(rbtxtbx_Member.TextBoxText);
            PnlPaint.StartServer(rbtxtbx_PortServer.TextBoxText);
            rbbtn_Create.Enabled = false; //Tránh người dùng click nhiều lần
            PnlPaint.Enabled = false; // Đợi mọi người kết nối vào đủ mới cho vẽ
        }
       
        private void rbUD_LineWidth_TextBoxTextChanged(object sender, EventArgs e)
        {
            int width = 1;
            try
            {
                width = Int32.Parse(rbUD_LineWidth.TextBoxText);

            }
            catch (FormatException)
            {
                MessageBox.Show("Format is incorrect");
            }
            PnlPaint.myPaint.LineWidth = width;
            if (PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                if (PnlPaint.MyData.Count() > 0)
                {
                    PnlPaint.MyData[PnlPaint.MyData.Count() - 1].State.LineWidth = width;
                    PnlPaint.Invalidate();
                }

            }

        }
        private void rbbtn_Connect_Click(object sender, EventArgs e)
        {
            if (PnlPaint.MyData.Count > 0)
            {
                if (MessageBox.Show("You are drawing! Click yes if you want to save your image before create a new room.", "Save information",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    svflDlg_SaveFile.Title = "Save Image";

                    if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string name = svflDlg_SaveFile.FileName;
                        PnlPaint.SaveImage(name);
                    }

                }
            }

            //PnlPaint._isRemoveBorder = true;
            //PnlPaint._isStillCanMove = PnlPaint._isStillCanReSize = false;

            rbmn_LoadProject.Enabled = false;
            PnlPaint.MyData.Clear();
            PnlPaint.buffer = new Bitmap(1, 1);
            PnlPaint.Invalidate();

            rbbtn_Connect.Enabled = false;
            PnlPaint._isClient = true;
            rbtb_NetWorkServer.Visible = false;
            PnlPaint.Connect(rbtxtbx_IP.TextBoxText, rbtxtbx_PortClient.TextBoxText, rbtxt_Name.TextBoxText);
        }

        private void rbbtn_Disconnect_Click(object sender, EventArgs e)
        {
            PnlPaint.disconnectToAllClient();
            PnlPaint.Enabled = true;
            rbpnl_Room.Items.Clear();
            rblbl_StatusServer.Text = "All member was disconnected ";
            rbmn_LoadProject.Enabled = true;
        }
        private void rbbtn_DisConnectToServer_Click(object sender, EventArgs e)
        {
            PnlPaint.disconnectToServer();
            PnlPaint.Enabled = true;
            rbmn_LoadProject.Enabled = true;
            if (!PnlPaint._isStartRoom)
            {
                rbbtn_Connect.Enabled = true;
            }
           
        }


        private void ribbonOrbMenuItem_Save_Click(object sender, EventArgs e)
        {
            svflDlg_SaveFile.Title = "Save Image";
            svflDlg_SaveFile.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
            {
                string name = svflDlg_SaveFile.FileName;
                PnlPaint.SaveImage(name);
            }
         
        }

        private void rbcmbbx_Font_TextBoxTextChanged(object sender, EventArgs e)
        {
            PnlPaint.myPaint.TextFont = rbcmbbx_Font.TextBoxText;
            PnlPaint.textBox.Font = new System.Drawing.Font(PnlPaint.myPaint.TextFont, PnlPaint.myPaint.TextSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

        private void rbcmbbx_SizeText_TextBoxTextChanged(object sender, EventArgs e)
        {
            PnlPaint.myPaint.TextSize = Int32.Parse(rbcmbbx_SizeText.TextBoxText);
            PnlPaint.textBox.Font = new System.Drawing.Font(PnlPaint.myPaint.TextFont, PnlPaint.myPaint.TextSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

       
        /// <summary>
        /// Đóng kết nối trước khi thoát
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPaint_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (PnlPaint._isClient)
            {
                PnlPaint.disconnectToServer();
            }
            if (PnlPaint._isServer)
            {
                PnlPaint.disconnectToAllClient();
            }

            if (PnlPaint.MyData.Count > 0)
            {
                if (MessageBox.Show("Do you want to save your image before exit?", "Save information",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    svflDlg_SaveFile.Title = "Save Image";
                    svflDlg_SaveFile.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                    if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string name = svflDlg_SaveFile.FileName;
                        PnlPaint.SaveImage(name);
                    }
                }
                else
                {
                    PnlPaint.MyData.Clear();
                    PnlPaint.buffer = new Bitmap(1, 1);
                    PnlPaint.Invalidate();
                }
            }
        }
       

        private void rbbtn_Fill_Click(object sender, EventArgs e)
        {
            // Bỏ bao khung
            if (!PnlPaint._isFill)
            {
                int n = PnlPaint.MyData.Count();
                if (n > 0 && PnlPaint.MyData[n - 1].State.ShapeType1 != ShapeType.MyText && !PnlPaint._isRemoveBorder)
                {
                    PnlPaint.MyData.RemoveAt(n - 1);
                    n--;
                    PnlPaint._isRemoveBorder = true;
                    PnlPaint.Invalidate();
                }
            }
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyFill;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            PnlPaint._isFill = true;          
        }

        private void rbbtn_FillColor_Click(object sender, EventArgs e)
        {
            clrDlg.ShowDialog();
            PnlPaint.FillColor = clrDlg.Color;
        }

        private void rbbtn_Request_Click(object sender, EventArgs e)
        {
            PnlPaint.sendRequest();
        }

        private void rbcmbx_BrushStyle_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            PnlPaint.myPaint.IsBrushFill = true;
            string temp;
            temp = PnlPaint.myPaint.NameBrush = rbcmbx_BrushStyle.TextBoxText;
            if (PnlPaint.MyData.Count() > 1 && PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.NameBrush = PnlPaint.myPaint.NameBrush;
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.IsBrushFill = PnlPaint.myPaint.IsBrushFill;
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.NameBrush = temp;
                PnlPaint.Invalidate();
            }
        }

        private void rbbtn_BrushColor_Click(object sender, EventArgs e)
        {
            clrDlg.ShowDialog();
            PnlPaint.myPaint.BrushColor = clrDlg.Color;
            PnlPaint.myPaint.RB1 = clrDlg.Color.R;
            PnlPaint.myPaint.GB1 = clrDlg.Color.G;
            PnlPaint.myPaint.BB1 = clrDlg.Color.B;

            if (PnlPaint.MyData.Count() > 1 && PnlPaint._isStillCanMove && PnlPaint._isStillCanReSize)
            {
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.BrushColor = clrDlg.Color;
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.RB1 = clrDlg.Color.R;
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.GB1 = clrDlg.Color.G;
                PnlPaint.MyData[PnlPaint.MyData.Count - 2].State.BB1 = clrDlg.Color.B;
                PnlPaint.Invalidate();
            }
        }

        

        /// <summary>
        /// Hàm thực hiện quản lý phòng
        /// Nếu click chuột trái lên thành viên thì remove khỏi phòng
        /// Nếu click chuột phải lên thành viên thì không cho thành viên đó được phép vẽ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Remove(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Left)//Chuột trái thì kích khỏi phòng
            {
                if (_myMember.Count > 0 && MessageBox.Show("Do you want remove this member?!.", "RemoveMem",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int index = _myMember.IndexOf((RibbonButton)sender);
                    _myMember.RemoveAt(index);
                    //
                    PnlPaint.disConnectMember(index);

                    //Cập nhật lại giao diện
                    rbpnl_Room.Items.RemoveAt(index);
                    if (!PnlPaint._startRoom)
                    {
                        PnlPaint._serverSocket.BeginAccept(new AsyncCallback(PnlPaint.AcceptCallbackServer), null);
                        rbbtn_Start.Enabled = false;
                    }

                    PaintState temp = new PaintState();
                    temp.FirstPoint = new Point(-9, -9);
                    temp.SecondPoint = new Point(index, 0);
                    for (int i = 0; i < PnlPaint._clientSocket.Count; i++)
                    {
                        byte[] buf = temp.ToByteArray();
                        PnlPaint._clientSocket[i].Send(buf);
                    }
                }
            }
            else//chuột phải thì không cho vẽ
            {
                if (PnlPaint._startRoom && _myMember.Count > 0) //Nếu đã bắt đầu vẽ
                {
                    if (MessageBox.Show("Do you want disable this member?!.", "DisableMem",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int index = _myMember.IndexOf((RibbonButton)sender);

                        PaintState temp = new PaintState();
                        temp.FirstPoint = new Point(-8, -8);
                        byte[] buffer = temp.ToByteArray();
                        PnlPaint._clientSocket[index].Send(buffer);

                        ableordisable(index, false);
                    }
                }
            }
           
          
        }


        private void rbbtn_Start_Click(object sender, EventArgs e)
        {
            PnlPaint.Enabled = true;
            PnlPaint._startRoom = true;
            rbbtn_Start.Enabled = false;

            PaintState temp = new PaintState();
            temp.FirstPoint = new Point(-6, -6);

            byte[] t = temp.ToByteArray();
            foreach (var item in PnlPaint._clientSocket)
            {
                item.Send(t);
            }
            rbmn_LoadProject.Enabled = true;
        }

        private void MyPaint_KeyDown(object sender, KeyEventArgs e)
        {
            if (!PnlPaint._isClient && !PnlPaint._isServer)
            {
                if (e.Control)
                {
                    if (e.KeyCode.Equals(Keys.Z))
                    {
                        PnlPaint.Undo();

                    }
                    if (e.KeyCode.Equals(Keys.Y))
                    {
                        PnlPaint.Redo();
                    }
                }

            }
          
        }

        private void rbUD_LineWidth_DownButtonClicked(object sender, MouseEventArgs e)
        {
            int value = Int32.Parse(((RibbonUpDown)sender).TextBoxText);
            if (value > 0)
            {
                value--;
                ((RibbonUpDown)sender).TextBoxText = value.ToString();
            }
        }

        private void rbUD_LineWidth_UpButtonClicked(object sender, MouseEventArgs e)
        {
            int value = Int32.Parse(((RibbonUpDown)sender).TextBoxText);
            value++;
            ((RibbonUpDown)sender).TextBoxText = value.ToString();
        }

   
        /// <summary>
        /// Hàm load file
        /// </summary>
        /// <param name="name">Đường dẫn tới file</param>
        public void LoadProject(string name)
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<PaintState>));
            FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<PaintState> temp = new List<PaintState>();

            temp = (List<PaintState>)ser.Deserialize(fs);
            fs.Close();

            foreach (var item in temp)
            {
                item.CurrentColor = Color.FromArgb(item.R1, item.G1, item.B1);
                item.BrushColor = Color.FromArgb(item.RB1, item.GB1, item.BB1);
                MyShape a;
                switch (item.ShapeType1)
                {
                    case ShapeType.MyLine:
                        a = new MyLine(item);
                        break;
                    case ShapeType.MyRectangle:
                        a = new MyRectangle(item);
                        break;
                    case ShapeType.MyEllipse:
                        a = new MyEllipse(item);
                        break;
                    case ShapeType.MyTriangle:
                        a = new MyTriangle(item);
                        break;
                    case ShapeType.MyText:
                        a = new MyText(item);
                        break;
                    case ShapeType.MyFill:
                        {
                            a = new MyFill(item);
                            a.State.ColorFill = Color.FromArgb(a.State.RF1, a.State.GF1, a.State.BF1);
                            PnlPaint.floodFill(a.State.PointFill, a.State.ColorFill);
                            break;
                        }
                    case ShapeType.MyEraser:
                        {
                            a = new MyEraser(item);
                            break;
                        }
                    default:
                        a = new MyLine(item);
                        break;
                }

                byte[] buf = item.ToByteArray();
                for (int i = 0; i < PnlPaint._clientSocket.Count; i++)
                {
                    PnlPaint._clientSocket[i].Send(buf);
                }
                //Gởi nhanh quá người nhận không kịp
                Thread.Sleep(100);

                PnlPaint.MyData.Add(a);
                PnlPaint._isRemoveBorder = true;
                PnlPaint._isStillCanMove = PnlPaint._isStillCanReSize = false;
                PnlPaint.Invalidate();
            }
        }



        /// <summary>
        /// Hàm save project
        /// </summary>
        /// <param name="name">Tên</param>
        public void SaveProject(string name)
        {
            List<PaintState> temp = new List<PaintState>();
            for (int i = 0; i < PnlPaint.MyData.Count; i++)
            {
                temp.Add(PnlPaint.MyData[i].State);
            }
            XmlSerializer ser = new XmlSerializer(typeof(List<PaintState>));

            TextWriter writer = new StreamWriter(name);
            ser.Serialize(writer, temp);
            writer.Close();

        }
      

        private void rbbtn_Ellipses_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyEllipse;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            removeBorder();
        }

        private void rbbtn_Lines_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyLine;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            removeBorder();
        }

 
        private void rbbtn_Triangles_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyTriangle;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            removeBorder();
        }

        private void rbbtn_Rectangle_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyRectangle;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            removeBorder();
        }

        private void rbbtn_Texts_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyText;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
            removeBorder();
        }

        private void rbbtn_Erasers_Click(object sender, EventArgs e)
        {
            PnlPaint.myPaint.ShapeType1 = ShapeType.MyEraser;
            PnlPaint._isFill = false;
            PnlPaint._isStillCanMove = false;
            PnlPaint._isStillCanReSize = false;
        }

        private void ribbonOrbMenuItem_New_Click(object sender, EventArgs e)
        {
            svflDlg_SaveFile.Title = "Save Project";
            svflDlg_SaveFile.Filter = "1212505 File (*.1212505) | *.1212505";
            if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
            {
                string name = svflDlg_SaveFile.FileName;
                SaveProject(name);
            }
        }

        private void rbmn_LoadProject_Click(object sender, EventArgs e)
        {
            if (PnlPaint.MyData.Count > 0)
            {
                if (MessageBox.Show("You are drawing! Click yes if you want to save your project before load an old project", "Save Project information",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    svflDlg_SaveFile.Title = "Save Project";

                    if (svflDlg_SaveFile.ShowDialog() == DialogResult.OK)
                    {
                        string name = svflDlg_SaveFile.FileName;
                        SaveProject(name);
                    }

                }
            }

            PnlPaint.buffer = new Bitmap(1, 1);
            PnlPaint.BackgroundImage = new Bitmap(PnlPaint.buffer);
            PnlPaint.MyData.Clear();

            PnlPaint.Invalidate();
   
            opnfldlg_OpenFile.Title = "Open Project";
            opnfldlg_OpenFile.Filter = "1212505 File (*.1212505) | *.1212505";
            if (opnfldlg_OpenFile.ShowDialog() == DialogResult.OK)
            {

                string name = opnfldlg_OpenFile.FileName;
                LoadProject(name);
            }
        }

        private void rbbtn_Back_Click(object sender, EventArgs e)
        {
            if (PnlPaint._isServer)
            {
                PnlPaint.disconnectToAllClient();
            }
            Application.Restart();
            
        }
 
        private void rbmn_BackHome_Click(object sender, EventArgs e)
        {
            if (PnlPaint._isServer)
            {
                PnlPaint.disconnectToAllClient();
            }
            if (PnlPaint._isClient)
            {
                PnlPaint.disconnectToServer();
            }
            Application.Restart();
        }
      
    }
}
