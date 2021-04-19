using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using System.Data;
using System.Data.SqlClient;
namespace BackupServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        public static SqlConnection conn = new SqlConnection();
        public static String connstr;
        public static String connectionString = @"Data Source=DESKTOP-K4GFHGT;Initial Catalog=master;Integrated Security=True";
        public static String servername = "DESKTOP-K4GFHGT";
        public static String database = "master";
        public static String login = "";
        public static String password = "";
        public static String pathBackup = "c:\\backupdatabase";

        public static frmChinh frmChu;
        public static frmDangNhap frmDangNhap;
        
        /// </summary>
        // connect server
        public static int KetNoi()
        {
            if (Program.conn != null && Program.conn.State == ConnectionState.Open)
                Program.conn.Close();
            try
            {
                Program.connstr = "Data Source=" + Program.servername + ";Initial Catalog=" +
                      Program.database + ";User ID=" +
                      Program.login + ";password=" + Program.password;
                Program.conn.ConnectionString = Program.connstr;
                Program.conn.Open();
                return 1;
            }

            catch (Exception e)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu\nBạn xem lại user name và password.\n" + e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        public static int ExecSql(String strLenh)
        {
            SqlCommand sqlcmd = new SqlCommand(strLenh, Program.conn);
            sqlcmd.CommandType = CommandType.Text;
            if (Program.conn.State == ConnectionState.Closed) Program.conn.Open();
            try
            {
                sqlcmd.ExecuteReader();
                return 1;
            }
            catch (InvalidOperationException ex)
            {
                Program.conn.Close();
                return 0;
            }
            catch (SqlException ex)
            {
                Program.conn.Close();
                return 0;
            }
        }

        public static DataSet execAndGetDataSQL(string query)
        {
            DataSet data = new DataSet();
            using (SqlConnection connection = new SqlConnection(Program.connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(data);
                connection.Close();
            }
            return data;
        }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Program.frmChu = new frmChinh();
            Program.frmChu.Visible = false;

            Program.frmDangNhap = new frmDangNhap();
            Program.frmDangNhap.Visible = true;

            Application.Run(Program.frmDangNhap);
        }
    }
}
