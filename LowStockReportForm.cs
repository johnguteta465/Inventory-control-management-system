using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class LowStockReportForm : Form
    {
        private DataGridView dgvLowStock;
        private Label lblSummary;

        public LowStockReportForm()
        {
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Low Stock Alert Report";
            this.Size = new Size(1000, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "LOW STOCK ALERT REPORT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(500, 35),
                ForeColor = Color.Red
            };

            dgvLowStock = new DataGridView()
            {
                Location = new Point(20, 60),
                Size = new Size(940, 420),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };

            lblSummary = new Label()
            {
                Location = new Point(20, 490),
                Size = new Size(940, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Red
            };

            this.Controls.AddRange(new Control[] { lblTitle, dgvLowStock, lblSummary });
        }

        private void LoadReport()
        {
            string query = @"SELECT p.ProductCode, p.Name, c.CategoryName, 
                                    p.Quantity, p.ReorderLevel, 
                                    (p.ReorderLevel - p.Quantity) as NeedToOrder
                             FROM Products p
                             LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                             WHERE p.Quantity <= p.ReorderLevel
                             ORDER BY p.Quantity ASC";

            DataTable dt = DatabaseHelper.GetDataTable(query);
            dgvLowStock.DataSource = dt;

            int lowStockCount = dt.Rows.Count;
            if (lowStockCount == 0)
            {
                lblSummary.Text = "No low stock items found! All products are above reorder level.";
                lblSummary.ForeColor = Color.Green;
            }
            else
            {
                lblSummary.Text = $"{lowStockCount} product(s) are below or at reorder level. Please restock soon!";
                lblSummary.ForeColor = Color.Red;
            }
        }
    }
}