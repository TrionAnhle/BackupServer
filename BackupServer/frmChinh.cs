using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BackupServer
{
    public partial class frmChinh : DevExpress.XtraEditors.XtraForm
    {
        public static String[] listDBName;
        public static Boolean[] listDBHasDevice;
        public frmChinh()
        {
            InitializeComponent();
            dataGVTenDB.ReadOnly = true;
            dataGVDSBackup.ReadOnly = true;
            dataGVTenDB.DataSource = getAllNameDatabase().Tables[0];
            dataGVTenDB.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGVTenDB.Columns[0].HeaderText = "Cơ sở dữ liệu";
            checkDBHasDevice();
            dateBackup.DateTime = DateTime.Now;
            timeBackup.Time = DateTime.Now;
            if (btnPhucHoiTheoThoiGian.Checked)
            {
                timeBackup.Enabled = dateBackup.Enabled = true;
            }
            else timeBackup.Enabled = dateBackup.Enabled = false;
        }
        DataSet getAllNameDatabase()
        {
            
            String query = "select name as Ten_Co_So_Du_Lieu from sys.databases";
            query += " WHERE    (database_id >= 5) AND (NOT (name LIKE N'ReportServer%')) "+
                "AND (NOT (name LIKE '%distribution%')) ORDER BY NAME";
            return Program.execAndGetDataSQL(query);
        }
        private void checkDBHasDevice()
        {
            int numberRow = Int32.Parse(dataGVTenDB.Rows.Count.ToString()) -1;
            listDBName = new String[numberRow];
            listDBHasDevice = new Boolean[numberRow];
            for (int i = 0; i < numberRow; i++)
            {
                listDBName[i] = dataGVTenDB.Rows[i].Cells[0].Value.ToString();
                String query = "select name from sys.sysdevices where name = '"+listDBName[i]+"'";
                DataSet ds = Program.execAndGetDataSQL(query);
                listDBHasDevice[i] = (ds.Tables[0].Rows.Count <= 0 ? false : true);
            }
        }
        private void loadDSBackup(String DBName)
        {
            
            String query = " SELECT     position, name, backup_start_date , user_name FROM  msdb.dbo.backupset "+
                           " WHERE database_name = '"+DBName+"' AND type = 'D' AND "+
                           "  backup_set_id >= "+
                           "          (SELECT backup_set_id FROM     msdb.dbo.backupset "+
                           "              WHERE media_set_id = "+
                           "            (SELECT  MAX(media_set_id) "+
                           "                  FROM msdb.dbo.backupset "+
                           "                        WHERE database_name = '"+DBName+"' AND type = 'D') "+
                           "               AND position = 1) "+ 
                           " ORDER BY position DESC";

            DataSet ds = Program.execAndGetDataSQL(query);
        
            if (ds.Tables[0].Rows.Count > 0)
            {
                dataGVDSBackup.DataSource = ds.Tables[0];
                dataGVDSBackup.Columns[0].HeaderText = "Bản sao lưu thứ #";
                dataGVDSBackup.Columns[1].HeaderText = "Diễn giải";
                dataGVDSBackup.Columns[2].HeaderText = "Ngày giờ sao lưu";
                dataGVDSBackup.Columns[3].HeaderText = "User sao lưu";
                dataGVDSBackup.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                btnPhucHoi.Enabled = btnPhucHoiTheoThoiGian.Enabled = true;
            }
            else
            {
                dataGVDSBackup.DataSource = null; this.dataGVDSBackup.Rows.Clear();
                dataGVDSBackup.Refresh();
                btnPhucHoi.Enabled = btnPhucHoiTheoThoiGian.Enabled = false;
            }
        }
        ///////////////////////---------------------------------------------------------------


        private void btnThoat_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Program.frmChu.Visible = false;
            Program.login = Program.password = "";
            Program.frmDangNhap.ClearInfomation();
            Program.frmDangNhap.Visible = true;
        }

        private void dataGVTenDB_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGVTenDB.CurrentRow != null && dataGVTenDB.CurrentCell.RowIndex< Int32.Parse(dataGVTenDB.Rows.Count.ToString()) - 1) { 
                String DBName = dataGVTenDB.CurrentRow.Cells[0].Value.ToString();
                int numberRow = Int32.Parse(dataGVTenDB.Rows.Count.ToString()) - 1;
                lblTenCSDL.Text = "Tên cơ sơ dữ liệu:  " + DBName;
                btnPhucHoiTheoThoiGian.Checked = btnPhucHoiTheoThoiGian.Enabled =btnPhucHoi.Enabled = btnSaoLuu.Enabled = false;
                dataGVDSBackup.DataSource = null; this.dataGVDSBackup.Rows.Clear();
                dataGVDSBackup.Refresh();
                for (int i = 0; i < numberRow; i++)
                {
                    if (listDBName[i].Equals(DBName))
                    {
                        if (listDBHasDevice[i])
                        {
                            btnSaoLuu.Enabled = true;
                            btnTaoDevice.Enabled = false;
                            loadDSBackup(DBName);
                        }
                        else
                        {
                            btnSaoLuu.Enabled = false;
                            btnTaoDevice.Enabled = true;
                        }
                    }
                }
            }else
            {
                btnPhucHoi.Enabled = btnSaoLuu.Enabled = btnPhucHoiTheoThoiGian.Enabled = btnTaoDevice.Enabled = false;
            }
            dateBackup.DateTime = DateTime.Now;
            timeBackup.Time = DateTime.Now;
            
        }

        private void btnLamMoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            dataGVTenDB.DataSource = getAllNameDatabase().Tables[0];
            checkDBHasDevice();
        }

        private void btnTaoDevice_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!File.Exists(Program.pathBackup))
            {
                Directory.CreateDirectory(Program.pathBackup);
            }
            String DBName = dataGVTenDB.CurrentRow.Cells[0].Value.ToString();
            String query = "EXEC sp_addumpdevice 'disk', '" + DBName + "', '"+Program.pathBackup+"\\"+DBName+".bak'";
            int result = Program.ExecSql(query);
            if(result == 1)
            {
                MessageBox.Show("Tạo device thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSaoLuu.Enabled = true;
                btnTaoDevice.Enabled = false;
                checkDBHasDevice();
            }
            else
            {
                MessageBox.Show("Tạo device thất bại! Mời xem lại thiết bị!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaoLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String DBName = dataGVTenDB.CurrentRow.Cells[0].Value.ToString();
            String query = "BACKUP DATABASE  " + DBName + "  TO " + DBName;
            int result = Program.ExecSql(query);
            if (result == 1)
            {
                MessageBox.Show("Sao lưu thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSaoLuu.Enabled = btnPhucHoi.Enabled = btnPhucHoiTheoThoiGian.Enabled = true;
                btnTaoDevice.Enabled = false;
                checkDBHasDevice();
                loadDSBackup(DBName);
            }
            else
            {
                MessageBox.Show("Sao lưu thất bại!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPhucHoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String query = "";
            String DBName = dataGVTenDB.CurrentRow.Cells[0].Value.ToString();
            if (!btnPhucHoiTheoThoiGian.Checked) { 
                String position = dataGVDSBackup.CurrentRow.Cells[0].Value.ToString();
                query += "ALTER DATABASE " + DBName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ";
                query += ("USE tempdb  RESTORE DATABASE " + DBName + " FROM  " + DBName + "  WITH FILE = " + position + ", REPLACE; ");
                query += ("ALTER DATABASE " + DBName + "  SET MULTI_USER; ");

            }else
            {
                string[] dateOfLastBackup = dataGVDSBackup.Rows[0].Cells[2].Value.ToString().Split(' ');
                String timeLastBackup = dateOfLastBackup[1] + " " + dateOfLastBackup[1];
                DateTime dtChose = DateTime.Parse(timeBackup.Text);
                DateTime dtLast = DateTime.Parse(dateOfLastBackup[1]+" "+dateOfLastBackup[2]);

                if (String.Compare(dateOfLastBackup[0],dateBackup.Text) == 0 && dtChose < dtLast)
                {
                    MessageBox.Show("Thời điểm phục hồi phải sau thời điểm bản sao lưu mới nhất!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                String position = dataGVDSBackup.Rows[0].Cells[0].Value.ToString();
                String time = dateBackup.DateTime.ToString("yyyy-MM-dd");
                time +=(" "+ dtChose.ToString("HH:mm:ss"));

                query += "USE master; ALTER DATABASE "+DBName+" SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ";
                query += "BACKUP LOG " + DBName + " TO DISK = '" + Program.pathBackup + "\\" + DBName + ".trn' WITH NORECOVERY; ";
                query += "RESTORE DATABASE "+DBName+" FROM TestA WITH FILE = "+position+", NORECOVERY; ";
                query += "RESTORE DATABASE "+DBName+ " FROM DISK = '" + Program.pathBackup + "\\" + DBName + ".trn' WITH STOPAT='" + time+"', RECOVERY; ";
                query += "ALTER DATABASE "+DBName+" SET MULTI_USER; ";
                Console.WriteLine(query);
            }

            
            
            int result = Program.ExecSql(query);

            if (result == 1 )
            {
                MessageBox.Show("Phục hồi thành công!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Phục hồi thất bại!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPhucHoiTheoThoiGian_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (btnPhucHoiTheoThoiGian.Checked)
            {
                timeBackup.Enabled = dateBackup.Enabled = true;
            }
            else timeBackup.Enabled = dateBackup.Enabled = false;
            string[] dateOfLastBackup = dataGVDSBackup.Rows[0].Cells[2].Value.ToString().Split(' ');
            dateBackup.Properties.MinValue = DateTime.Parse(dateOfLastBackup[0]);
            
        }

        
    }
}
