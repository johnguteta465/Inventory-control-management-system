using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace InventoryManagementSystem
{
    public class NotificationManager
    {
        private NotifyIcon notifyIcon;
        private Timer checkTimer;
        private Form parentForm;

        public NotificationManager(Form form)
        {
            parentForm = form;
            SetupNotificationIcon();
            StartMonitoring();
        }

        private void SetupNotificationIcon()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "Inventory Management System"
            };

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show Dashboard", null, (s, e) => parentForm.WindowState = FormWindowState.Normal);
            contextMenu.Items.Add("View Low Stock", null, (s, e) => new LowStockReportForm().ShowDialog());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.DoubleClick += (s, e) => parentForm.WindowState = FormWindowState.Normal;
        }

        private void StartMonitoring()
        {
            checkTimer = new Timer();
            checkTimer.Interval = 30000;
            checkTimer.Tick += (s, e) => CheckLowStock();
            checkTimer.Start();

            CheckLowStock();
        }

        private void CheckLowStock()
        {
            try
            {
                string query = @"SELECT p.ProductCode, p.Name, p.Quantity, p.ReorderLevel,
                                        c.CategoryName
                                 FROM Products p
                                 LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                                 WHERE p.Quantity <= p.ReorderLevel
                                 ORDER BY p.Quantity ASC";

                DataTable dt = DatabaseHelper.GetDataTable(query);

                if (dt.Rows.Count > 0)
                {
                    string message = $"⚠️ LOW STOCK ALERT!\n\n{dt.Rows.Count} product(s) need restocking:\n\n";
                    int count = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (count++ >= 5)
                        {
                            message += $"\n... and {dt.Rows.Count - 5} more";
                            break;
                        }
                        message += $"• {row["Name"]}: {row["Quantity"]}/{row["ReorderLevel"]}\n";
                    }

                    notifyIcon.BalloonTipTitle = "⚠️ Low Stock Alert";
                    notifyIcon.BalloonTipText = message;
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                    notifyIcon.ShowBalloonTip(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Notification error: " + ex.Message);
            }
        }

        public void StopMonitoring()
        {
            checkTimer?.Stop();
            notifyIcon?.Dispose();
        }
    }
}