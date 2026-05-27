using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace InventoryManagementSystem
{
    public class StockInForm : Form
    {
        // UI Controls
        private System.Windows.Forms.ComboBox cmbProduct;
        private System.Windows.Forms.ComboBox cmbSupplier;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label lblSellingPrice;
        private System.Windows.Forms.NumericUpDown nudQuantity;
        private System.Windows.Forms.TextBox txtUnitCost;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Button btnProcessStock;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridView dgvHistory;
        private System.Windows.Forms.Label lblStatus;

        // Colors
        private static readonly Color BgTeal = Color.FromArgb(134, 183, 181);
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color SuccessGreen = Color.FromArgb(16, 145, 95);
        private static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        private static readonly Color DarkTextColor = Color.FromArgb(44, 62, 80);
        private static readonly Color BorderLight = Color.FromArgb(220, 225, 230);

        public StockInForm()
        {
            InitializeForm();
            SetupForm();
        }

        private void InitializeForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Text = "Stock In - Purchase Order";
            this.BackColor = BgTeal;
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void SetupForm()
        {
            // Header Panel
            Panel header = CreateHeaderPanel();

            // Main content
            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = BgTeal
            };

            // Left Panel - Purchase Details
            Panel leftPanel = CreatePurchaseDetailsPanel();

            // Right Panel - History
            Panel rightPanel = CreateHistoryPanel();

            // Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            mainContent.Controls.Add(mainLayout);

            this.Controls.Add(mainContent);
            this.Controls.Add(header);

            // Load data
            this.Load += StockInForm_Load;
        }

        private void StockInForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LoadSuppliers();
            LoadStockHistory();
        }

        private Panel CreateHeaderPanel()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = PrimaryBlue
            };

            header.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(header.ClientRectangle,
                    PrimaryBlue, ControlPaint.Dark(PrimaryBlue, 0.1f), 90f))
                {
                    e.Graphics.FillRectangle(brush, header.ClientRectangle);
                }
            };

            Label titleLabel = new Label
            {
                Text = "STOCK IN - PURCHASE ORDER",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(25, 18),
                AutoSize = true
            };

            header.Controls.Add(titleLabel);
            return header;
        }

        private Panel CreatePurchaseDetailsPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(5)
            };

            panel.Paint += (sender, e) =>
            {
                GraphicsPath path = GetRoundedRectangle(panel.ClientRectangle, 10);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawPath(pen, path);
            };

            Label titleLabel = new Label
            {
                Text = "PURCHASE DETAILS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };

            int startY = 60;
            int labelWidth = 100;
            int controlWidth = 250;
            int leftMargin = 25;
            int rowHeight = 45;

            // Product
            Label lblProduct = CreateLabel("Product:", leftMargin, startY, labelWidth);
            cmbProduct = CreateComboBox(leftMargin + labelWidth, startY, controlWidth);
            cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            // Supplier
            Label lblSupplier = CreateLabel("Supplier:", leftMargin, startY + rowHeight, labelWidth);
            cmbSupplier = CreateComboBox(leftMargin + labelWidth, startY + rowHeight, controlWidth);
            cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;

            // Current Stock
            Label lblCurrentLabel = CreateLabel("Current:", leftMargin, startY + (rowHeight * 2), labelWidth);
            lblCurrent = new Label
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 2) + 5),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "0",
                ForeColor = PrimaryBlue
            };

            // Selling Price
            Label lblSellingPriceLabel = CreateLabel("Selling Price:", leftMargin, startY + (rowHeight * 3), labelWidth);
            lblSellingPrice = new Label
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 3) + 5),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "0.00",
                ForeColor = SuccessGreen
            };

            // Quantity
            Label lblQuantity = CreateLabel("Quantity:", leftMargin, startY + (rowHeight * 4), labelWidth);
            nudQuantity = new NumericUpDown
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 4)),
                Size = new Size(controlWidth, 30),
                Minimum = 1,
                Maximum = 99999,
                Value = 1,
                Font = new Font("Segoe UI", 10),
                TextAlign = HorizontalAlignment.Center
            };

            // Unit Cost
            Label lblUnitCost = CreateLabel("Unit Cost:", leftMargin, startY + (rowHeight * 5), labelWidth);
            txtUnitCost = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 5)),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                Text = "0.00",
                TextAlign = HorizontalAlignment.Right
            };

            // Notes
            Label lblNotes = CreateLabel("Notes:", leftMargin, startY + (rowHeight * 6), labelWidth);
            txtNotes = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 6)),
                Size = new Size(controlWidth, 60),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Buttons
            int buttonY = startY + (rowHeight * 7) + 20;
            btnProcessStock = CreateButton("✓ PROCESS STOCK",
                new Point(leftMargin, buttonY),
                new Size(160, 40),
                SuccessGreen);
            btnProcessStock.Click += BtnProcessStock_Click;

            btnRefresh = CreateButton("↻ REFRESH",
                new Point(leftMargin + 175, buttonY),
                new Size(130, 40),
                PrimaryBlue);
            btnRefresh.Click += BtnRefresh_Click;

            // Status label
            lblStatus = new Label
            {
                Location = new Point(leftMargin, buttonY + 55),
                Size = new Size(380, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Text = "Ready to process stock in..."
            };

            panel.Controls.AddRange(new Control[] {
                titleLabel,
                lblProduct, cmbProduct,
                lblSupplier, cmbSupplier,
                lblCurrentLabel, lblCurrent,
                lblSellingPriceLabel, lblSellingPrice,
                lblQuantity, nudQuantity,
                lblUnitCost, txtUnitCost,
                lblNotes, txtNotes,
                btnProcessStock, btnRefresh,
                lblStatus
            });

            return panel;
        }

        private Panel CreateHistoryPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(5)
            };

            panel.Paint += (sender, e) =>
            {
                GraphicsPath path = GetRoundedRectangle(panel.ClientRectangle, 10);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawPath(pen, path);
            };

            Label titleLabel = new Label
            {
                Text = "RECENT STOCK IN HISTORY",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(400, 30)
            };

            dgvHistory = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(panel.Width - 35, panel.Height - 85),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9)
            };

            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(dgvHistory);

            return panel;
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue != null)
            {
                int productId = Convert.ToInt32(cmbProduct.SelectedValue);
                LoadProductDetails(productId);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            LoadSuppliers();
            LoadStockHistory();
            ClearForm();
        }

        private void LoadProducts()
        {
            try
            {
                string query = "SELECT ProductID, Name FROM Products ORDER BY Name";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.DataSource = dt;

                if (dt.Rows.Count > 0)
                {
                    cmbProduct.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading products: " + ex.Message, DangerRed);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                string query = "SELECT SupplierID, Name FROM Suppliers ORDER BY Name";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "SupplierID";
                cmbSupplier.DataSource = dt;
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading suppliers: " + ex.Message, DangerRed);
            }
        }

        private void LoadProductDetails(int productId)
        {
            try
            {
                string query = "SELECT Quantity, UnitPrice FROM Products WHERE ProductID = @pid";
                DataTable dt = DatabaseHelper.GetDataTable(query, new[] { new SqlParameter("@pid", productId) });

                if (dt.Rows.Count > 0)
                {
                    lblCurrent.Text = dt.Rows[0]["Quantity"].ToString();
                    lblSellingPrice.Text = Convert.ToDecimal(dt.Rows[0]["UnitPrice"]).ToString("N2");
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading product details: " + ex.Message, DangerRed);
            }
        }

        private void LoadStockHistory()
        {
            try
            {
                // Try to get the date column name dynamically
                string dateColumnName = GetDateColumnName();

                string query = $@"SELECT TOP 50 
                                    SI.StockInID, 
                                    P.Name as Product,
                                    SI.Quantity, 
                                    SI.UnitCost,
                                    (SI.Quantity * SI.UnitCost) as Total,
                                    {dateColumnName} as Date,
                                    ISNULL(S.Name, 'N/A') as Supplier
                                FROM StockIn SI
                                INNER JOIN Products P ON SI.ProductID = P.ProductID
                                LEFT JOIN Suppliers S ON SI.SupplierID = S.SupplierID
                                ORDER BY {dateColumnName} DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvHistory.DataSource = dt;

                if (dgvHistory.Columns.Contains("StockInID"))
                    dgvHistory.Columns["StockInID"].Visible = false;

                if (dgvHistory.Columns.Contains("Total"))
                    dgvHistory.Columns["Total"].DefaultCellStyle.Format = "N2";

                if (dgvHistory.Columns.Contains("UnitCost"))
                    dgvHistory.Columns["UnitCost"].DefaultCellStyle.Format = "N2";

                if (dgvHistory.Columns.Contains("Date"))
                    dgvHistory.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

                ShowStatus($"Loaded {dt.Rows.Count} stock in records", PrimaryBlue);
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading history: " + ex.Message, DangerRed);
            }
        }

        private string GetDateColumnName()
        {
            try
            {
                // Check for common date column names
                string[] possibleDateColumns = { "StockInDate", "Date", "CreatedDate", "TransactionDate", "EntryDate" };

                foreach (string colName in possibleDateColumns)
                {
                    string checkQuery = $@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'StockIn' 
                        AND COLUMN_NAME = '{colName}'";

                    int exists = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery));
                    if (exists > 0)
                    {
                        return colName;
                    }
                }

                // If no date column found, use GETDATE() as fallback
                return "GETDATE()";
            }
            catch
            {
                return "GETDATE()";
            }
        }

        private void BtnProcessStock_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                ShowStatus("Please select a product!", DangerRed);
                return;
            }

            if (cmbSupplier.SelectedValue == null)
            {
                ShowStatus("Please select a supplier!", DangerRed);
                return;
            }

            if (nudQuantity.Value <= 0)
            {
                ShowStatus("Quantity must be greater than 0!", DangerRed);
                return;
            }

            if (!decimal.TryParse(txtUnitCost.Text, out decimal unitCost) || unitCost <= 0)
            {
                ShowStatus("Please enter a valid unit cost!", DangerRed);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Process stock in?\n\nProduct: {cmbProduct.Text}\nQuantity: {nudQuantity.Value}\nUnit Cost: {unitCost:N2}\nTotal Cost: {(nudQuantity.Value * unitCost):N2}",
                "Confirm Stock In",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                int productId = Convert.ToInt32(cmbProduct.SelectedValue);
                int supplierId = Convert.ToInt32(cmbSupplier.SelectedValue);
                int quantity = (int)nudQuantity.Value;

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Get the date column name for insertion
                            string dateColumnName = GetDateColumnName();
                            string insertStockSql;

                            if (dateColumnName != "GETDATE()")
                            {
                                insertStockSql = $@"INSERT INTO StockIn 
                                    (ProductID, SupplierID, Quantity, UnitCost, Notes, {dateColumnName}) 
                                    VALUES (@pid, @sid, @qty, @cost, @notes, @date)";
                            }
                            else
                            {
                                insertStockSql = @"INSERT INTO StockIn 
                                    (ProductID, SupplierID, Quantity, UnitCost, Notes) 
                                    VALUES (@pid, @sid, @qty, @cost, @notes)";
                            }

                            using (var cmd = new SqlCommand(insertStockSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@pid", productId);
                                cmd.Parameters.AddWithValue("@sid", supplierId);
                                cmd.Parameters.AddWithValue("@qty", quantity);
                                cmd.Parameters.AddWithValue("@cost", unitCost);
                                cmd.Parameters.AddWithValue("@notes", txtNotes.Text.Trim());

                                if (dateColumnName != "GETDATE()")
                                {
                                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                                }

                                cmd.ExecuteNonQuery();
                            }

                            string updateProductSql = "UPDATE Products SET Quantity = Quantity + @qty WHERE ProductID = @pid";
                            using (var cmd = new SqlCommand(updateProductSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@qty", quantity);
                                cmd.Parameters.AddWithValue("@pid", productId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            ShowStatus($"Successfully added {quantity} units of {cmbProduct.Text}!", SuccessGreen);

                            LoadProductDetails(productId);
                            LoadStockHistory();
                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error processing stock: " + ex.Message, DangerRed);
                MessageBox.Show($"Failed to process stock in:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            nudQuantity.Value = 1;
            txtUnitCost.Text = "0.00";
            txtNotes.Clear();
            if (cmbProduct.Items.Count > 0)
                cmbProduct.SelectedIndex = -1;
            if (cmbSupplier.Items.Count > 0)
                cmbSupplier.SelectedIndex = -1;
        }

        private void ShowStatus(string message, Color color)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 5),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DarkTextColor
            };
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
        }

        private Button CreateButton(string text, Point location, Size size, Color backColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}