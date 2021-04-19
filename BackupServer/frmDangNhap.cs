using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackupServer
{
    public partial class frmDangNhap : Form
    {
        public frmDangNhap()
        {
            InitializeComponent();
        }

        public void ClearInfomation()
        {
            txtPassword.Text = txtUsername.Text = "";
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            if(txtUsername.Text.Trim()=="" || txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Tên đăng nhập hoặc Mật khẩu không được để trống!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Program.login = txtUsername.Text;
            Program.password = txtPassword.Text;

            if (Program.KetNoi() == 0)
            {
                Program.login = Program.password = "";
                return;
            }
            Program.conn.Close();
            Program.frmDangNhap.Visible = false;
            Program.frmChu.Visible = true;
        }
    }
}
