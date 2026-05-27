using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class InventoryLogForm : Form
    {
        private DataGridView dgvLog;
        private ComboBox cmbProductFilter;
        private DateTimePicker dtpStart, dtpEnd;
        private Button btnFilter, btnRefresh;
        private Label lblSummary;

        // Theme colors
        private static readonly Color BackgroundTeal = Color.FromArgb(134, 183, 181);
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color DarkButtonColor = Color.FromArgb(44, 47, 58);
        private static readonly Color WhiteText = Color.White;
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public InventoryLogForm()
        {
            InitializeComponent();
            LoadProductFilter();
            LoadLog();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Change Log";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BackgroundTeal;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;

            // Header
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 70 };
            headerPanel.Paint += (s, e) =>
            {
                Rectangle rect = headerPanel.ClientRectangle;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, PrimaryBlue, ControlPaint.Dark(PrimaryBlue, 0.1f), 90f))
                    e.Graphics.FillRectangle(brush, rect);
            };
            Label lblTitle = new Label
            {
                Text = "📋 INVENTORY CHANGE LOG",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 18),
                Size = new Size(400, 35),
                ForeColor = WhiteText
            };
            headerPanel.Controls.Add(lblTitle);

            // Filter panel (white card)
            Panel filterCard = CreateCardPanel("FILTERS", 70);
            filterCard.Location = new Point(20, 90);
            filterCard.Size = new Size(this.ClientSize.Width - 40, 70);

            Label lblProduct = new Label { Text = "Product:", Location = new Point(20, 25), Size = new Size(60, 25), ForeColor = DarkText };
            cmbProductFilter = new ComboBox { Location = new Point(85, 22), Size = new Size(200, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProductFilter.Items.Insert(0, "All Products");
            cmbProductFilter.SelectedIndex = 0;

            Label lblFrom = new Label { Text = "From:", Location = new Point(320, 25), Size = new Size(40, 25), ForeColor = DarkText };
            dtpStart = new DateTimePicker { Location = new Point(365, 22), Size = new Size(130, 25), Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddDays(-30);

            Label lblTo = new Label { Text = "To:", Location = new Point(510, 25), Size = new Size(30, 25), ForeColor = DarkText };
            dtpEnd = new DateTimePicker { Location = new Point(545, 22), Size = new Size(130, 25), Format = DateTimePickerFormat.Short };
            dtpEnd.Value = DateTime.Now;

            btnFilter = new Button
            {
                Text = "Filter",
                Location = new Point(700, 21),
                Size = new Size(80, 28),
                BackColor = PrimaryBlue,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => LoadLog();

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(790, 21),
                Size = new Size(80, 28),
                BackColor = DarkButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => { cmbProductFilter.SelectedIndex = 0; dtpStart.Value = DateTime.Now.AddDays(-30); dtpEnd.Value = DateTime.Now; LoadLog(); };

            filterCard.Controls.Add(lblProduct);
            filterCard.Controls.Add(cmbProductFilter);
            filterCard.Controls.Add(lblFrom);
            filterCard.Controls.Add(dtpStart);
            filterCard.Controls.Add(lblTo);
            filterCard.Controls.Add(dtpEnd);
            filterCard.Controls.Add(btnFilter);
            filterCard.Controls.Add(btnRefresh);

            // DataGridView card
            Panel gridCard = CreateCardPanel("STOCK MOVEMENTS", 480);
            gridCard.Location = new Point(20, 175);
            gridCard.Size = new Size(this.ClientSize.Width - 40, 480);

            dgvLog = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(gridCard.Width - 30, gridCard.Height - 60),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            ThemeManager.ConfigureDataGridView(dgvLog);
            gridCard.Controls.Add(dgvLog);

            // Summary
            lblSummary = new Label
            {
                Location = new Point(20, 670),
                Size = new Size(this.ClientSize.Width - 40, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DarkText,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(headerPanel);
            this.Controls.Add(filterCard);
            this.Controls.Add(gridCard);
            this.Controls.Add(lblSummary);

            this.Resize += (s, e) =>
            {
                filterCard.Width = this.ClientSize.Width - 40;
                gridCard.Width = this.ClientSize.Width - 40;
                dgvLog.Width = gridCard.Width - 30;
                dgvLog.Height = gridCard.Height - 60;
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

        private void LoadProductFilter()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetDataTable("SELECT ProductID, Name FROM Products ORDER BY Name");
                cmbProductFilter.DisplayMember = "Name";
                cmbProductFilter.ValueMember = "ProductID";
                cmbProductFilter.DataSource = dt;
                cmbProductFilter.Items.Insert(0, "All Products");
                cmbProductFilter.SelectedIndex = 0;
            }
            catch (Exception) { /* ignore */ }
        }

        private void LoadLog()
        {
            try
            {
                string query = @"
                    SELECT 
                        l.LogID,
                        p.Name AS Product,
                        l.QuantityChange,
                        l.NewQuantity,
                        l.Reason,
                        l.Reference,
                        l.LogDate,
                        u.FullName AS ChangedBy
                    FROM InventoryLogs l
                    INNER JOIN Products p ON l.ProductID = p.ProductID
                    LEFT JOIN Users u ON l.UserID = u.UserID
                    WHERE l.LogDate BETWEEN @start AND @end";

                if (cmbProductFilter.SelectedValue != null && cmbProductFilter.SelectedIndex > 0)
                {
                    query += " AND l.ProductID = @productId";
                }

                query += " ORDER BY l.LogDate DESC";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                parameters.Add(new SqlParameter("@start", dtpStart.Value.Date));
                parameters.Add(new SqlParameter("@end", dtpEnd.Value.Date.AddDays(1).AddSeconds(-1)));

                if (cmbProductFilter.SelectedValue != null && cmbProductFilter.SelectedIndex > 0)
                {
                    parameters.Add(new SqlParameter("@productId", cmbProductFilter.SelectedValue));
                }

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
                dgvLog.DataSource = dt;

                // Format columns
                if (dgvLog.Columns.Contains("LogID")) dgvLog.Columns["LogID"].Visible = false;
                if (dgvLog.Columns.Contains("Product")) dgvLog.Columns["Product"].HeaderText = "Product";
                if (dgvLog.Columns.Contains("QuantityChange")) dgvLog.Columns["QuantityChange"].HeaderText = "Change";
                if (dgvLog.Columns.Contains("NewQuantity")) dgvLog.Columns["NewQuantity"].HeaderText = "New Qty";
                if (dgvLog.Columns.Contains("Reason")) dgvLog.Columns["Reason"].HeaderText = "Reason";
                if (dgvLog.Columns.Contains("Reference")) dgvLog.Columns["Reference"].HeaderText = "Reference";
                if (dgvLog.Columns.Contains("LogDate")) dgvLog.Columns["LogDate"].HeaderText = "Date & Time";
                if (dgvLog.Columns.Contains("ChangedBy")) dgvLog.Columns["ChangedBy"].HeaderText = "User";

                // Color positive/negative changes
                foreach (DataGridViewRow row in dgvLog.Rows)
                {
                    if (row.Cells["QuantityChange"].Value != null)
                    {
                        int change = Convert.ToInt32(row.Cells["QuantityChange"].Value);
                        if (change > 0)
                            row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                        else if (change < 0)
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    }
                }

                lblSummary.Text = $"Showing {dt.Rows.Count} inventory movements between {dtpStart.Value.ToShortDateString()} and {dtpEnd.Value.ToShortDateString()}.";
            }
            catch (Exception)
            {
                dgvLog.DataSource = null;
                lblSummary.Text = "Error loading inventory log.";
            }
        }
    }
}