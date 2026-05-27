using Microsoft.Data.SqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class BackupRestoreForm : Form
    {
        private Button btnBackup, btnRestore;

        public BackupRestoreForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Database Backup & Restore";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            btnBackup = new Button() { Text = "Backup Database", Location = new Point(50, 30), Size = new Size(120, 40), BackColor = System.Drawing.Color.FromArgb(46, 204, 113), ForeColor = System.Drawing.Color.White };
            btnRestore = new Button() { Text = "Restore Database", Location = new Point(200, 30), Size = new Size(120, 40), BackColor = System.Drawing.Color.FromArgb(231, 76, 60), ForeColor = System.Drawing.Color.White };
            btnBackup.Click += BtnBackup_Click;
            btnRestore.Click += BtnRestore_Click;

            this.Controls.Add(btnBackup);
            this.Controls.Add(btnRestore);
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Backup files (*.bak)|*.bak", FileName = "InventoryDBs_Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string query = $"BACKUP DATABASE InventoryDBs TO DISK = '{sfd.FileName}'";
                    DatabaseHelper.ExecuteNonQuery(query);
                    MessageBox.Show("Backup completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show("Backup failed: " + ex.Message); }
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Restore must be done by an administrator using SQL Server Management Studio for safety.\n\n" +
                            "To restore, use: RESTORE DATABASE InventoryDBs FROM DISK = 'path\\backup.bak' WITH REPLACE;",
                            "Restore Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}