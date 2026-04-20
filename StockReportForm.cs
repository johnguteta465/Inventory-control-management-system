using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class StockReportForm : Form
    {
        private DataGridView dgvStock;
        private TextBox txtSearch;
        private Button btnSearch, btnExport, btnExportExcel;
        private Label lblSummary;
        private Panel headerPanel;
        private ComboBox cmbCategory;
        private NumericUpDown nudMinStock, nudMaxStock;

        public StockReportForm()
        {
            InitializeComponent();
            LoadReport();
            LoadCategoryFilter();
        }

        private void InitializeComponent()
        {
            this.Text = "Current Stock Report";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;

            // Header Panel with Gradient
            headerPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, 80),
                Dock = DockStyle.Top
            };
            headerPanel.Paint += (s, e) =>
            {
                Rectangle rect = headerPanel.ClientRectangle;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect,
                    Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185), 90f))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };

            Label lblTitle = new Label()
            {
                Text = "📋 CURRENT STOCK REPORT",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 25),
                Size = new Size(400, 35),
                ForeColor = Color.White
            };

            headerPanel.Controls.Add(lblTitle);

            // Filter Panel
            Panel filterPanel = new Panel()
            {
                Location = new Point(20, 100),
                Size = new Size(this.Width - 40, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Search Box
            Label lblSearch = new Label() { Text = "🔍 Search:", Location = new Point(15, 15), Size = new Size(70, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtSearch = new TextBox() { Location = new Point(85, 12), Size = new Size(200, 25), Font = new Font("Segoe UI", 10) };
            txtSearch.TextChanged += (s, e) => LoadReport();

            // Category Filter
            Label lblCategory = new Label() { Text = "📁 Category:", Location = new Point(310, 15), Size = new Size(70, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            cmbCategory = new ComboBox() { Location = new Point(380, 12), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadReport();

            // Stock Range Filter
            Label lblStockRange = new Label() { Text = "📊 Stock Range:", Location = new Point(560, 15), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudMinStock = new NumericUpDown() { Location = new Point(660, 12), Size = new Size(60, 25), Minimum = 0, Maximum = 99999 };
            Label lblTo = new Label() { Text = "to", Location = new Point(725, 15), Size = new Size(25, 25) };
            nudMaxStock = new NumericUpDown() { Location = new Point(750, 12), Size = new Size(60, 25), Minimum = 0, Maximum = 99999, Value = 99999 };
            nudMinStock.ValueChanged += (s, e) => LoadReport();
            nudMaxStock.ValueChanged += (s, e) => LoadReport();

            // Buttons
            btnSearch = new Button()
            {
                Text = "🔍 Apply Filters",
                Location = new Point(840, 10),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.Click += (s, e) => LoadReport();

            Button btnReset = new Button()
            {
                Text = "🔄 Reset",
                Location = new Point(960, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.Click += (s, e) => ResetFilters();

            btnExport = new Button()
            {
                Text = "📄 Export CSV",
                Location = new Point(880, 45),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;

            btnExportExcel = new Button()
            {
                Text = "📊 Export Excel",
                Location = new Point(990, 45),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportExcel.Click += BtnExportExcel_Click;

            filterPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblCategory, cmbCategory,
                lblStockRange, nudMinStock, lblTo, nudMaxStock,
                btnSearch, btnReset, btnExport, btnExportExcel
            });

            // DataGridView
            dgvStock = new DataGridView()
            {
                Location = new Point(20, 195),
                Size = new Size(this.Width - 40, 470),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 249, 250) },
                GridColor = Color.FromArgb(220, 220, 220)
            };
            dgvStock.CellFormatting += DgvStock_CellFormatting;

            // Summary Panel
            Panel summaryPanel = new Panel()
            {
                Location = new Point(20, 675),
                Size = new Size(this.Width - 40, 40),
                BackColor = Color.FromArgb(52, 73, 94)
            };

            lblSummary = new Label()
            {
                Location = new Point(15, 10),
                Size = new Size(this.Width - 70, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            summaryPanel.Controls.Add(lblSummary);

            this.Controls.Add(headerPanel);
            this.Controls.Add(filterPanel);
            this.Controls.Add(dgvStock);
            this.Controls.Add(summaryPanel);

            // Resize event
            this.Resize += (s, e) => {
                filterPanel.Width = this.Width - 40;
                dgvStock.Width = this.Width - 40;
                dgvStock.Height = this.Height - 280;
                summaryPanel.Width = this.Width - 40;
                lblSummary.Width = this.Width - 70;
                headerPanel.Width = this.Width;
            };
        }

        private void LoadCategoryFilter()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories UNION SELECT 0, 'All Categories' ORDER BY CategoryName");
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryID";
                cmbCategory.DataSource = dt;
                cmbCategory.SelectedValue = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading categories: " + ex.Message);
            }
        }

        private void LoadReport()
        {
            try
            {
                string query = @"SELECT p.ProductCode, p.Name, c.CategoryName, s.Name as Supplier, 
                                        p.Quantity, p.UnitPrice, (p.Quantity * p.UnitPrice) as TotalValue,
                                        CASE 
                                            WHEN p.Quantity <= p.ReorderLevel THEN '⚠️ LOW STOCK' 
                                            WHEN p.Quantity = 0 THEN '❌ OUT OF STOCK'
                                            ELSE '✅ IN STOCK' 
                                        END as Status
                                 FROM Products p
                                 LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                                 LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                                 WHERE 1=1";

                // Add search filter
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    query += " AND (p.Name LIKE @search OR p.ProductCode LIKE @search)";
                }

                // Add category filter
                if (cmbCategory.SelectedValue != null && Convert.ToInt32(cmbCategory.SelectedValue) > 0)
                {
                    query += " AND p.CategoryID = @catId";
                }

                // Add stock range filter
                query += " AND p.Quantity BETWEEN @minStock AND @maxStock";

                query += " ORDER BY p.Name";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    parameters.Add(new SqlParameter("@search", "%" + txtSearch.Text + "%"));
                }

                if (cmbCategory.SelectedValue != null && Convert.ToInt32(cmbCategory.SelectedValue) > 0)
                {
                    parameters.Add(new SqlParameter("@catId", cmbCategory.SelectedValue));
                }

                parameters.Add(new SqlParameter("@minStock", nudMinStock.Value));
                parameters.Add(new SqlParameter("@maxStock", nudMaxStock.Value));

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
                dgvStock.DataSource = dt;

                // Update summary
                decimal totalValue = 0;
                int totalItems = 0;
                int lowStockCount = 0;
                int outOfStockCount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    totalValue += Convert.ToDecimal(row["TotalValue"]);
                    totalItems += Convert.ToInt32(row["Quantity"]);
                    string status = row["Status"].ToString();
                    if (status.Contains("LOW")) lowStockCount++;
                    if (status.Contains("OUT")) outOfStockCount++;
                }

                lblSummary.Text = $"📊 Total Products: {dt.Rows.Count} | Total Units: {totalItems:N0} | Total Value: ₱{totalValue:N2} | ⚠️ Low Stock: {lowStockCount} | ❌ Out of Stock: {outOfStockCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvStock.Rows[e.RowIndex].DataBoundItem != null)
            {
                var row = (dgvStock.Rows[e.RowIndex].DataBoundItem as DataRowView);
                if (row != null)
                {
                    string status = row["Status"].ToString();
                    if (status.Contains("LOW"))
                    {
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (status.Contains("OUT"))
                    {
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 180, 180);
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                    }
                    else
                    {
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                        dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkGreen;
                    }
                }
            }
        }

        private void ResetFilters()
        {
            txtSearch.Clear();
            cmbCategory.SelectedValue = 0;
            nudMinStock.Value = 0;
            nudMaxStock.Value = 99999;
            LoadReport();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.FileName = "StockReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        // Write headers
                        for (int i = 0; i < dgvStock.Columns.Count; i++)
                        {
                            sw.Write(dgvStock.Columns[i].HeaderText);
                            if (i < dgvStock.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();

                        // Write data
                        foreach (DataGridViewRow row in dgvStock.Rows)
                        {
                            for (int i = 0; i < dgvStock.Columns.Count; i++)
                            {
                                string value = row.Cells[i].Value?.ToString() ?? "";
                                // Escape commas and quotes
                                if (value.Contains(",") || value.Contains("\""))
                                {
                                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                                }
                                sw.Write(value);
                                if (i < dgvStock.Columns.Count - 1) sw.Write(",");
                            }
                            sw.WriteLine();
                        }
                    }
                    MessageBox.Show("Report exported to CSV successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Use the ExcelExporter class
                ExcelExporter.ExportToExcel(dgvStock, "StockReport");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel export failed: " + ex.Message + "\n\nMake sure Excel is installed.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}