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

        // Theme colors (matching your teal dashboard)
        private static readonly Color BackgroundTeal = Color.FromArgb(134, 183, 181);
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);
        private static readonly Color DarkButtonColor = Color.FromArgb(44, 47, 58);
        private static readonly Color WhiteText = Color.White;
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public SalesReportForm()
        {
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Sales Report";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BackgroundTeal;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;

            // Header panel (gradient)
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 70 };
            headerPanel.Paint += (s, e) =>
            {
                Rectangle rect = headerPanel.ClientRectangle;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, PrimaryBlue, ControlPaint.Dark(PrimaryBlue, 0.1f), 90f))
                    e.Graphics.FillRectangle(brush, rect);
            };
            Label lblTitle = new Label
            {
                Text = "💰 SALES REPORT",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 18),
                Size = new Size(300, 35),
                ForeColor = WhiteText
            };
            headerPanel.Controls.Add(lblTitle);

            // Filter panel (white card)
            Panel filterCard = CreateCardPanel("FILTERS", 60);
            filterCard.Location = new Point(20, 90);
            filterCard.Size = new Size(this.ClientSize.Width - 40, 60);
            filterCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Label lblFrom = new Label { Text = "From:", Location = new Point(20, 18), Size = new Size(40, 25), ForeColor = DarkText };
            dtpStart = new DateTimePicker { Location = new Point(65, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);

            Label lblTo = new Label { Text = "To:", Location = new Point(230, 18), Size = new Size(30, 25), ForeColor = DarkText };
            dtpEnd = new DateTimePicker { Location = new Point(265, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short };
            dtpEnd.Value = DateTime.Now;

            btnFilter = new Button
            {
                Text = "Filter",
                Location = new Point(440, 13),
                Size = new Size(80, 30),
                BackColor = PrimaryBlue,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => LoadReport();

            btnExport = new Button
            {
                Text = "Export CSV",
                Location = new Point(530, 13),
                Size = new Size(100, 30),
                BackColor = GreenButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;

            filterCard.Controls.Add(lblFrom);
            filterCard.Controls.Add(dtpStart);
            filterCard.Controls.Add(lblTo);
            filterCard.Controls.Add(dtpEnd);
            filterCard.Controls.Add(btnFilter);
            filterCard.Controls.Add(btnExport);

            // DataGridView (white card)
            Panel gridCard = CreateCardPanel("SALES TRANSACTIONS", 480);
            gridCard.Location = new Point(20, 165);
            gridCard.Size = new Size(this.ClientSize.Width - 40, 480);
            gridCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgvSales = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(gridCard.Width - 30, gridCard.Height - 60),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            ThemeManager.ConfigureDataGridView(dgvSales);
            gridCard.Controls.Add(dgvSales);

            // Summary label
            lblSummary = new Label
            {
                Location = new Point(20, 660),
                Size = new Size(this.ClientSize.Width - 40, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DarkText,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(headerPanel);
            this.Controls.Add(filterCard);
            this.Controls.Add(gridCard);
            this.Controls.Add(lblSummary);

            // Adjust grid on resize
            this.Resize += (s, e) =>
            {
                filterCard.Width = this.ClientSize.Width - 40;
                gridCard.Width = this.ClientSize.Width - 40;
                dgvSales.Width = gridCard.Width - 30;
                dgvSales.Height = gridCard.Height - 60;
                lblSummary.Width = this.ClientSize.Width - 40;
            };
        }

        private Panel CreateCardPanel(string title, int height)
        {
            Panel card = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.None, Height = height };
            card.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 12;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                card.Region = new Region(path);
                using (Pen pen = new Pen(Color.LightGray, 1))
                    e.Graphics.DrawPath(pen, path);
                using (Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Brush titleBrush = new SolidBrush(PrimaryBlue))
                    e.Graphics.DrawString(title, titleFont, titleBrush, new PointF(15, 12));
            };
            return card;
        }

        private void LoadReport()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.SaleID,
                        s.InvoiceNumber,
                        c.FullName AS Customer,
                        s.SaleDate,
                        s.Subtotal,
                        s.DiscountPercent,
                        s.DiscountAmount,
                        s.TaxPercent,
                        s.TaxAmount,
                        s.TotalAmount,
                        s.PaymentMethod,
                        s.PaymentStatus
                    FROM Sales s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    WHERE s.SaleDate BETWEEN @start AND @end
                    ORDER BY s.SaleDate DESC";

                SqlParameter[] parameters = {
                    new SqlParameter("@start", dtpStart.Value.Date),
                    new SqlParameter("@end", dtpEnd.Value.Date.AddDays(1).AddSeconds(-1))
                };

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
                dgvSales.DataSource = dt;

                // Format columns
                if (dgvSales.Columns.Contains("SaleID")) dgvSales.Columns["SaleID"].Visible = false;
                if (dgvSales.Columns.Contains("InvoiceNumber")) dgvSales.Columns["InvoiceNumber"].HeaderText = "Invoice #";
                if (dgvSales.Columns.Contains("Customer")) dgvSales.Columns["Customer"].HeaderText = "Customer";
                if (dgvSales.Columns.Contains("SaleDate")) dgvSales.Columns["SaleDate"].HeaderText = "Date";
                if (dgvSales.Columns.Contains("Subtotal")) dgvSales.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
                if (dgvSales.Columns.Contains("DiscountPercent")) dgvSales.Columns["DiscountPercent"].HeaderText = "Disc %";
                if (dgvSales.Columns.Contains("DiscountAmount")) dgvSales.Columns["DiscountAmount"].DefaultCellStyle.Format = "N2";
                if (dgvSales.Columns.Contains("TaxPercent")) dgvSales.Columns["TaxPercent"].HeaderText = "Tax %";
                if (dgvSales.Columns.Contains("TaxAmount")) dgvSales.Columns["TaxAmount"].DefaultCellStyle.Format = "N2";
                if (dgvSales.Columns.Contains("TotalAmount")) dgvSales.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
                if (dgvSales.Columns.Contains("PaymentMethod")) dgvSales.Columns["PaymentMethod"].HeaderText = "Payment";
                if (dgvSales.Columns.Contains("PaymentStatus")) dgvSales.Columns["PaymentStatus"].HeaderText = "Status";

                // Summary
                decimal totalSales = 0;
                foreach (DataRow row in dt.Rows)
                    totalSales += Convert.ToDecimal(row["TotalAmount"]);

                lblSummary.Text = $"Period: {dtpStart.Value.ToShortDateString()} – {dtpEnd.Value.ToShortDateString()}  |  Total Sales: ₱{totalSales:N2}  |  Transactions: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvSales.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.Title = "Export Sales Report";
            sfd.FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        // Write headers
                        for (int i = 0; i < dgvSales.Columns.Count; i++)
                        {
                            if (dgvSales.Columns[i].Visible)
                            {
                                sw.Write(dgvSales.Columns[i].HeaderText);
                                if (i < dgvSales.Columns.Count - 1) sw.Write(",");
                            }
                        }
                        sw.WriteLine();

                        // Write data
                        foreach (DataGridViewRow row in dgvSales.Rows)
                        {
                            for (int i = 0; i < dgvSales.Columns.Count; i++)
                            {
                                if (dgvSales.Columns[i].Visible)
                                {
                                    string val = row.Cells[i].Value?.ToString() ?? "";
                                    if (val.Contains(",")) val = "\"" + val + "\"";
                                    sw.Write(val);
                                    if (i < dgvSales.Columns.Count - 1) sw.Write(",");
                                }
                            }
                            sw.WriteLine();
                        }
                    }
                    MessageBox.Show("Export completed successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}