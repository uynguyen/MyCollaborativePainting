using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn2
{
    public partial class FormStart : Form
    {
        public FormStart()
        {
            InitializeComponent();
        }

        private void btn_PaintNetwork_Click(object sender, EventArgs e)
        {
            MyPaint mypaint = new MyPaint(0);
            this.Hide();
            mypaint.ShowDialog();
            this.Close();
        }

        private void btn_NetworkSerber_Click(object sender, EventArgs e)
        {
            MyPaint mypaint = new MyPaint(1);
            this.Hide();
            mypaint.ShowDialog();
           
            this.Close();
        }

        private void btn_NetworkClient_Click(object sender, EventArgs e)
        {
            MyPaint mypaint = new MyPaint(2);
            this.Hide();
            mypaint.ShowDialog();
            this.Close();
        }

        private void btn_About_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
