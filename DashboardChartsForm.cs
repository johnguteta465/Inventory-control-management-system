using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class DashboardChartsForm : Form
    {
        private DataGridView dgvCategorySummary;
        private DataGridView dgvStockStatus;
        private DataGridView dgvSalesTrend;
        private Timer refreshTimer;

        public DashboardChartsForm()
        {
            InitializeComponent();
            LoadData();
            StartAutoRefresh();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 Analytics Dashboard";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(15, 25, 45);

            Label lblTitle = new Label()
            {
                Text = "📊 INVENTORY ANALYTICS DASHBOARD",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(600, 40)
            };

            // Panel 1 - Inventory by Category
            Panel panel1 = new Panel()
            {
                Location = new Point(20, 80),
                Size = new Size(550, 280),
                BackColor = Color.FromArgb(25, 40, 60)
            };

            Label lblCatTitle = new Label()
            {
                Text = "📦 Inventory Value by Category",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };
            panel1.Controls.Add(lblCatTitle);

            dgvCategorySummary = new DataGridView()
            {
                Location = new Point(10, 45),
                Size = new Size(530, 225),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvCategorySummary);
            panel1.Controls.Add(dgvCategorySummary);

            // Panel 2 - Stock Status
            Panel panel2 = new Panel()
            {
                Location = new Point(590, 80),
                Size = new Size(550, 280),
                BackColor = Color.FromArgb(25, 40, 60)
            };

            Label lblLowTitle = new Label()
            {
                Text = "⚠️ Stock Status Overview",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };
            panel2.Controls.Add(lblLowTitle);

            dgvStockStatus = new DataGridView()
            {
                Location = new Point(10, 45),
                Size = new Size(530, 225),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvStockStatus);
            panel2.Controls.Add(dgvStockStatus);

            // Panel 3 - Sales Trend
            Panel panel3 = new Panel()
            {
                Location = new Point(20, 380),
                Size = new Size(1120, 270),
                BackColor = Color.FromArgb(25, 40, 60)
            };

            Label lblSalesTitle = new Label()
            {
                Text = "💰 Sales Trend (Last 30 Days)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };
            panel3.Controls.Add(lblSalesTitle);

            dgvSalesTrend = new DataGridView()
            {
                Location = new Point(10, 45),
                Size = new Size(1100, 215),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvSalesTrend);
            panel3.Controls.Add(dgvSalesTrend);

            // Refresh Button
            Button btnRefresh = new Button()
            {
                Text = "🔄 Refresh Data",
                Location = new Point(980, 20),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadData();

            this.Controls.Add(lblTitle);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(panel1);
            this.Controls.Add(panel2);
            this.Controls.Add(panel3);
        }

        private void LoadData()
        {
            LoadCategorySummary();
            LoadStockStatus();
            LoadSalesTrend();
        }

        private void LoadCategorySummary()
        {
            try
            {
                string query = @"SELECT 
                    ISNULL(c.CategoryName, 'Uncategorized') as Category,
                    COUNT(p.ProductID) as ProductCount,
                    SUM(p.Quantity) as TotalStock,
                    SUM(p.Quantity * p.UnitPrice) as TotalValue
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    GROUP BY c.CategoryName
                    ORDER BY TotalValue DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvCategorySummary.DataSource = dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadStockStatus()
        {
            try
            {
                string query = @"SELECT 
                    'Low Stock (≤ Reorder Level)' as Status,
                    COUNT(*) as Count,
                    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Products) AS DECIMAL(5,2)) as Percentage
                    FROM Products WHERE Quantity <= ReorderLevel AND Quantity > 0
                    UNION ALL
                    SELECT 'Good Stock (> Reorder Level)', COUNT(*),
                    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Products) AS DECIMAL(5,2))
                    FROM Products WHERE Quantity > ReorderLevel
                    UNION ALL
                    SELECT 'Out of Stock (0)', COUNT(*),
                    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Products) AS DECIMAL(5,2))
                    FROM Products WHERE Quantity = 0";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvStockStatus.DataSource = dt;

                // Color rows
                foreach (DataGridViewRow row in dgvStockStatus.Rows)
                {
                    string status = row.Cells["Status"].Value.ToString();
                    if (status.Contains("Low"))
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    else if (status.Contains("Out"))
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 180, 180);
                    else
                        row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadSalesTrend()
        {
            try
            {
                string query = @"SELECT 
                    FORMAT(s.SaleDate, 'yyyy-MM-dd') as SaleDate,
                    COUNT(DISTINCT s.SaleID) as TransactionCount,
                    SUM(si.Quantity) as TotalItemsSold,
                    SUM(si.TotalPrice) as TotalSales
                    FROM Sales s
                    INNER JOIN SaleItems si ON s.SaleID = si.SaleID
                    WHERE s.SaleDate >= DATEADD(day, -30, GETDATE())
                    GROUP BY FORMAT(s.SaleDate, 'yyyy-MM-dd')
                    ORDER BY SaleDate DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvSalesTrend.DataSource = dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 60000; // Refresh every minute
            refreshTimer.Tick += (s, e) => LoadData();
            refreshTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}