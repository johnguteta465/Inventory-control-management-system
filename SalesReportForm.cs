using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class SalesReportForm : Form
    {
        private DataGridView dgvSales;
        private DateTimePicker dtpStart, dtpEnd;
        private Button btnFilter, btnExport;
        private Label lblSummary;

        public SalesReportForm()
        {
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales Report";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "SALES REPORT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(300, 35),
                ForeColor = Color.DarkBlue
            };

            // Filter Panel
            Panel filterPanel = new Panel() { Location = new Point(20, 60), Size = new Size(1040, 40), BackColor = Color.FromArgb(240, 240, 240) };

            Label lblFrom = new Label() { Text = "From:", Location = new Point(10, 10), Size = new Size(40, 25) };
            dtpStart = new DateTimePicker() { Location = new Point(50, 8), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);

            Label lblTo = new Label() { Text = "To:", Location = new Point(210, 10), Size = new Size(30, 25) };
            dtpEnd = new DateTimePicker() { Location = new Point(240, 8), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpEnd.Value = DateTime.Now;

            btnFilter = new Button() { Text = "Filter", Location = new Point(400, 8), Size = new Size(80, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnFilter.Click += (s, e) => LoadReport();

            btnExport = new Button() { Text = "Export to CSV", Location = new Point(900, 8), Size = new Size(120, 28), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White };
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.AddRange(new Control[] { lblFrom, dtpStart, lblTo, dtpEnd, btnFilter, btnExport });

            // DataGridView
            dgvSales = new DataGridView()
            {
                Location = new Point(20, 110),
                Size = new Size(1040, 460),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };

            // Summary Label
            lblSummary = new Label()
            {
                Location = new Point(20, 580),
                Size = new Size(1040, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            this.Controls.AddRange(new Control[] { lblTitle, filterPanel, dgvSales, lblSummary });
        }

        private void LoadReport()
        {
            string query = @"SELECT so.StockOutID, p.Name as Product, so.Quantity, so.UnitPrice, 
                                    (so.Quantity * so.UnitPrice) as TotalAmount, so.Date, so.CustomerName
                             FROM StockOut so
                             INNER JOIN Products p ON so.ProductID = p.ProductID
                             WHERE so.Date BETWEEN @start AND @end
                             ORDER BY so.Date DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@start", dtpStart.Value.Date),
                new SqlParameter("@end", dtpEnd.Value.Date.AddDays(1).AddSeconds(-1))
            };

            DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
            dgvSales.DataSource = dt;

            // Calculate totals
            decimal totalSales = 0;
            int totalItems = 0;
            foreach (DataRow row in dt.Rows)
            {
                totalSales += Convert.ToDecimal(row["TotalAmount"]);
                totalItems += Convert.ToInt32(row["Quantity"]);
            }

            lblSummary.Text = $"Period: {dtpStart.Value.ToShortDateString()} to {dtpEnd.Value.ToShortDateString()} | Total Sales: ₱{totalSales:N2} | Total Items Sold: {totalItems} | Transactions: {dt.Rows.Count}";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.FileName = "SalesReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        for (int i = 0; i < dgvSales.Columns.Count; i++)
                        {
                            sw.Write(dgvSales.Columns[i].HeaderText);
                            if (i < dgvSales.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();

                        foreach (DataGridViewRow row in dgvSales.Rows)
                        {
                            for (int i = 0; i < dgvSales.Columns.Count; i++)
                            {
                                sw.Write(row.Cells[i].Value?.ToString());
                                if (i < dgvSales.Columns.Count - 1) sw.Write(",");
                            }
                            sw.WriteLine();
                        }
                    }
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}