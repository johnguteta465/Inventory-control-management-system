using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class StockOutForm : Form
    {
        // UI Controls
        private ComboBox cmbProduct;
        private NumericUpDown nudQuantity;
        private TextBox txtSellingPrice, txtCustomer, txtNotes;
        private Button btnProcessSale, btnRefresh;
        private Label lblCurrentStock, lblTotalAmount;
        private DataGridView dgvHistory;
        private Label lblStatus;

        // Colors
        private static readonly Color BgTeal = Color.White; // Changed from teal to white
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color SuccessGreen = Color.FromArgb(16, 145, 95);
        private static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        private static readonly Color WarningOrange = Color.FromArgb(243, 156, 18);
        private static readonly Color DarkText = Color.FromArgb(44, 62, 80);
        private static readonly Color MutedText = Color.FromArgb(127, 140, 141);
        private static readonly Color BorderLight = Color.FromArgb(220, 225, 230);

        public StockOutForm()
        {
            InitializeForm();
            SetupForm();
        }

        private void InitializeForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Text = "Stock Out - Sales";
            this.BackColor = BgTeal;
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void SetupForm()
        {
            Panel header = CreateHeaderPanel();

            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = BgTeal
            };

            Panel leftPanel = CreateSaleDetailsPanel();
            Panel rightPanel = CreateHistoryPanel();

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

            this.Load += (s, e) => { LoadProducts(); LoadStockHistory(); };
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
                Text = "STOCK OUT - SALES",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(25, 18),
                AutoSize = true
            };

            header.Controls.Add(titleLabel);
            return header;
        }

        private Panel CreateSaleDetailsPanel()
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
                Text = "SALE DETAILS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };

            int startY = 60;
            int labelWidth = 120;
            int controlWidth = 220;
            int leftMargin = 25;
            int rowHeight = 45;

            // Product
            Label lblProduct = CreateLabel("Product:", leftMargin, startY, labelWidth);
            cmbProduct = CreateComboBox(leftMargin + labelWidth, startY, controlWidth);
            cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            // Current Stock
            Label lblCurrentLabel = CreateLabel("Current Stock:", leftMargin, startY + rowHeight, labelWidth);
            lblCurrentStock = new Label
            {
                Location = new Point(leftMargin + labelWidth, startY + rowHeight + 5),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "0",
                ForeColor = PrimaryBlue,
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            // Quantity
            Label lblQuantity = CreateLabel("Quantity:", leftMargin, startY + (rowHeight * 2), labelWidth);
            nudQuantity = new NumericUpDown
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 2)),
                Size = new Size(controlWidth, 30),
                Minimum = 1,
                Maximum = 99999,
                Value = 1,
                Font = new Font("Segoe UI", 10),
                TextAlign = HorizontalAlignment.Center
            };
            nudQuantity.ValueChanged += CalculateTotal;

            // Selling Price
            Label lblSellingPrice = CreateLabel("Selling Price:", leftMargin, startY + (rowHeight * 3), labelWidth);
            txtSellingPrice = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 3)),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                Text = "0.00",
                TextAlign = HorizontalAlignment.Right,
                BackColor = Color.FromArgb(255, 255, 224)
            };
            txtSellingPrice.TextChanged += CalculateTotal;

            // Total Amount
            Label lblTotalLabel = CreateLabel("Total:", leftMargin, startY + (rowHeight * 4), labelWidth);
            lblTotalAmount = new Label
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 4) + 5),
                Size = new Size(controlWidth, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Text = "₱0.00",
                ForeColor = SuccessGreen,
                BackColor = Color.FromArgb(240, 255, 240),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };

            // Customer with placeholder effect
            Label lblCustomer = CreateLabel("Customer:", leftMargin, startY + (rowHeight * 5), labelWidth);
            txtCustomer = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 5)),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                Text = "Enter customer name (optional)",
                ForeColor = Color.Gray
            };
            txtCustomer.GotFocus += (s, e) =>
            {
                if (txtCustomer.Text == "Enter customer name (optional)")
                {
                    txtCustomer.Text = "";
                    txtCustomer.ForeColor = DarkText;
                }
            };
            txtCustomer.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCustomer.Text))
                {
                    txtCustomer.Text = "Enter customer name (optional)";
                    txtCustomer.ForeColor = Color.Gray;
                }
            };

            // Notes with placeholder effect
            Label lblNotes = CreateLabel("Notes:", leftMargin, startY + (rowHeight * 6), labelWidth);
            txtNotes = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 6)),
                Size = new Size(controlWidth, 60),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "Additional notes...",
                ForeColor = Color.Gray
            };
            txtNotes.GotFocus += (s, e) =>
            {
                if (txtNotes.Text == "Additional notes...")
                {
                    txtNotes.Text = "";
                    txtNotes.ForeColor = DarkText;
                }
            };
            txtNotes.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNotes.Text))
                {
                    txtNotes.Text = "Additional notes...";
                    txtNotes.ForeColor = Color.Gray;
                }
            };

            // Buttons
            int buttonY = startY + (rowHeight * 7) + 20;
            btnProcessSale = CreateButton("✓ PROCESS SALE",
                new Point(leftMargin, buttonY),
                new Size(160, 45),
                SuccessGreen);
            btnProcessSale.Click += BtnProcessSale_Click;

            btnRefresh = CreateButton("↻ REFRESH",
                new Point(leftMargin + 175, buttonY),
                new Size(130, 45),
                PrimaryBlue);
            btnRefresh.Click += (s, e) => { LoadProducts(); LoadStockHistory(); ClearForm(); };

            // Status label
            lblStatus = new Label
            {
                Location = new Point(leftMargin, buttonY + 60),
                Size = new Size(380, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = MutedText,
                Text = "Ready to process sales..."
            };

            panel.Controls.AddRange(new Control[] {
                titleLabel,
                lblProduct, cmbProduct,
                lblCurrentLabel, lblCurrentStock,
                lblQuantity, nudQuantity,
                lblSellingPrice, txtSellingPrice,
                lblTotalLabel, lblTotalAmount,
                lblCustomer, txtCustomer,
                lblNotes, txtNotes,
                btnProcessSale, btnRefresh,
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
                Text = "RECENT STOCK OUT HISTORY",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(300, 30),
                AutoSize = false
            };

            // Search
            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(20, 55),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DarkText
            };

            TextBox txtSearchHistory = new TextBox
            {
                Location = new Point(75, 53),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9),
                Text = "Product or customer...",
                ForeColor = Color.Gray
            };
            txtSearchHistory.GotFocus += (s, e) =>
            {
                if (txtSearchHistory.Text == "Product or customer...")
                {
                    txtSearchHistory.Text = "";
                    txtSearchHistory.ForeColor = DarkText;
                }
            };
            txtSearchHistory.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearchHistory.Text))
                {
                    txtSearchHistory.Text = "Product or customer...";
                    txtSearchHistory.ForeColor = Color.Gray;
                }
            };
            txtSearchHistory.TextChanged += (s, e) =>
            {
                if (txtSearchHistory.ForeColor != Color.Gray)
                    FilterHistory(txtSearchHistory.Text);
            };

            Button btnSearchHistory = new Button
            {
                Text = "🔍",
                Location = new Point(260, 52),
                Size = new Size(35, 28),
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9)
            };
            btnSearchHistory.Click += (s, e) =>
            {
                string search = txtSearchHistory.ForeColor == Color.Gray ? "" : txtSearchHistory.Text;
                FilterHistory(search);
            };

            // DataGridView
            dgvHistory = new DataGridView
            {
                Location = new Point(15, 90),
                Size = new Size(panel.Width - 35, panel.Height - 110),
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
            panel.Controls.Add(lblSearch);
            panel.Controls.Add(txtSearchHistory);
            panel.Controls.Add(btnSearchHistory);
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

        private void LoadProducts()
        {
            try
            {
                string query = "SELECT ProductID, Name, Quantity, UnitPrice FROM Products WHERE Quantity > 0 ORDER BY Name";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.DataSource = dt;

                if (dt.Rows.Count > 0)
                {
                    cmbProduct.SelectedIndex = -1;
                }
                else
                {
                    ShowStatus("No products with available stock!", WarningOrange);
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading products: " + ex.Message, DangerRed);
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
                    int stock = Convert.ToInt32(dt.Rows[0]["Quantity"]);
                    decimal price = Convert.ToDecimal(dt.Rows[0]["UnitPrice"]);
                    lblCurrentStock.Text = stock.ToString();
                    nudQuantity.Maximum = stock;
                    txtSellingPrice.Text = price.ToString("N2");

                    if (stock < 10)
                    {
                        lblCurrentStock.ForeColor = DangerRed;
                        ShowStatus($"Low stock alert! Only {stock} units remaining.", DangerRed);
                    }
                    else
                    {
                        lblCurrentStock.ForeColor = PrimaryBlue;
                        ShowStatus($"Product selected. Available stock: {stock} units", SuccessGreen);
                    }

                    CalculateTotal();
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading product details: " + ex.Message, DangerRed);
            }
        }

        private void CalculateTotal(object sender = null, EventArgs e = null)
        {
            if (decimal.TryParse(txtSellingPrice.Text, out decimal price) && nudQuantity.Value > 0)
            {
                decimal total = price * nudQuantity.Value;
                lblTotalAmount.Text = $"₱{total:N2}";

                if (total > 10000)
                    lblTotalAmount.ForeColor = Color.FromArgb(255, 69, 0);
                else if (total > 5000)
                    lblTotalAmount.ForeColor = WarningOrange;
                else
                    lblTotalAmount.ForeColor = SuccessGreen;
            }
            else
            {
                lblTotalAmount.Text = "₱0.00";
            }
        }

        private void LoadStockHistory()
        {
            try
            {
                string query = @"SELECT TOP 50 
                                    s.SaleID, 
                                    p.Name as Product,
                                    si.Quantity, 
                                    si.UnitPrice,
                                    si.TotalPrice as Total,
                                    s.SaleDate,
                                    ISNULL(c.FullName, 'Walk-in') as Customer
                                FROM Sales s
                                INNER JOIN SaleItems si ON s.SaleID = si.SaleID
                                INNER JOIN Products p ON si.ProductID = p.ProductID
                                LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                                ORDER BY s.SaleDate DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvHistory.DataSource = dt;

                if (dgvHistory.Columns.Contains("SaleID"))
                    dgvHistory.Columns["SaleID"].Visible = false;
                if (dgvHistory.Columns.Contains("Product"))
                    dgvHistory.Columns["Product"].HeaderText = "Product";
                if (dgvHistory.Columns.Contains("Quantity"))
                    dgvHistory.Columns["Quantity"].HeaderText = "Qty";
                if (dgvHistory.Columns.Contains("UnitPrice"))
                {
                    dgvHistory.Columns["UnitPrice"].HeaderText = "Unit Price";
                    dgvHistory.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
                }
                if (dgvHistory.Columns.Contains("Total"))
                {
                    dgvHistory.Columns["Total"].HeaderText = "Total";
                    dgvHistory.Columns["Total"].DefaultCellStyle.Format = "N2";
                }
                if (dgvHistory.Columns.Contains("SaleDate"))
                {
                    dgvHistory.Columns["SaleDate"].HeaderText = "Date";
                    dgvHistory.Columns["SaleDate"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                }
                if (dgvHistory.Columns.Contains("Customer"))
                    dgvHistory.Columns["Customer"].HeaderText = "Customer";

                ShowStatus($"Loaded {dt.Rows.Count} sales records", PrimaryBlue);
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading history: " + ex.Message, DangerRed);
            }
        }

        private void FilterHistory(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Product or customer...")
            {
                LoadStockHistory();
                return;
            }

            try
            {
                string query = @"SELECT TOP 50 
                                    s.SaleID, 
                                    p.Name as Product,
                                    si.Quantity, 
                                    si.UnitPrice,
                                    si.TotalPrice as Total,
                                    s.SaleDate,
                                    ISNULL(c.FullName, 'Walk-in') as Customer
                                FROM Sales s
                                INNER JOIN SaleItems si ON s.SaleID = si.SaleID
                                INNER JOIN Products p ON si.ProductID = p.ProductID
                                LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                                WHERE p.Name LIKE @search OR c.FullName LIKE @search
                                ORDER BY s.SaleDate DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query, new[] { new SqlParameter("@search", "%" + searchText + "%") });
                dgvHistory.DataSource = dt;
                ShowStatus($"Found {dt.Rows.Count} result(s) for '{searchText}'", PrimaryBlue);
            }
            catch (Exception ex)
            {
                ShowStatus("Error searching: " + ex.Message, DangerRed);
            }
        }

        private void BtnProcessSale_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                ShowStatus("Please select a product!", DangerRed);
                MessageBox.Show("Please select a product to sell.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudQuantity.Value <= 0)
            {
                ShowStatus("Quantity must be greater than 0!", DangerRed);
                MessageBox.Show("Please enter a valid quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice) || sellingPrice <= 0)
            {
                ShowStatus("Please enter a valid selling price!", DangerRed);
                MessageBox.Show("Please enter a valid selling price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(cmbProduct.SelectedValue);
            int quantity = (int)nudQuantity.Value;
            int currentStock = Convert.ToInt32(lblCurrentStock.Text);

            if (quantity > currentStock)
            {
                ShowStatus($"Insufficient stock! Only {currentStock} units available.", DangerRed);
                MessageBox.Show($"Insufficient stock!\n\nAvailable: {currentStock} units\nRequested: {quantity} units",
                    "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal totalAmount = quantity * sellingPrice;
            string customerName = (txtCustomer.Text == "Enter customer name (optional)" || string.IsNullOrWhiteSpace(txtCustomer.Text))
                ? "Walk-in" : txtCustomer.Text;

            DialogResult result = MessageBox.Show(
                $"Confirm Sale?\n\n" +
                $"Product: {cmbProduct.Text}\n" +
                $"Quantity: {quantity}\n" +
                $"Unit Price: ₱{sellingPrice:N2}\n" +
                $"Total Amount: ₱{totalAmount:N2}\n" +
                $"Customer: {customerName}\n\n" +
                $"Click Yes to process this sale.",
                "Confirm Sale",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertSql = @"INSERT INTO Sales 
                                (InvoiceNumber, CustomerID, Subtotal, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, TotalAmount, PaymentMethod, PaymentStatus, CreatedBy, SaleDate) 
                                VALUES (@inv, @cust, @total, 0, 0, 0, 0, @total, 'Cash', 'Paid', @user, @date);
                                SELECT SCOPE_IDENTITY();";

                            using (var cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@inv", "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                                cmd.Parameters.AddWithValue("@cust", 1); // Default walk-in customer ID
                                cmd.Parameters.AddWithValue("@total", totalAmount);
                                cmd.Parameters.AddWithValue("@user", Global.UserID); // Use UserID (integer) not UserName (string)
                                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                                int saleId = Convert.ToInt32(cmd.ExecuteScalar());

                                // Insert into SaleItems
                                string itemSql = @"INSERT INTO SaleItems (SaleID, ProductID, Quantity, UnitPrice, TotalPrice) 
                                                   VALUES (@saleId, @pid, @qty, @price, @itemTotal)";
                                using (var itemCmd = new SqlCommand(itemSql, conn, transaction))
                                {
                                    itemCmd.Parameters.AddWithValue("@saleId", saleId);
                                    itemCmd.Parameters.AddWithValue("@pid", productId);
                                    itemCmd.Parameters.AddWithValue("@qty", quantity);
                                    itemCmd.Parameters.AddWithValue("@price", sellingPrice);
                                    itemCmd.Parameters.AddWithValue("@itemTotal", totalAmount);
                                    itemCmd.ExecuteNonQuery();
                                }
                            }

                            string updateSql = "UPDATE Products SET Quantity = Quantity - @qty WHERE ProductID = @pid";
                            using (var cmd = new SqlCommand(updateSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@qty", quantity);
                                cmd.Parameters.AddWithValue("@pid", productId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            ShowStatus($"Sale completed! {quantity} unit(s) of {cmbProduct.Text} sold for ₱{totalAmount:N2}", SuccessGreen);
                            MessageBox.Show($"✓ Sale Successful!\n\n" +
                                $"Product: {cmbProduct.Text}\n" +
                                $"Quantity: {quantity}\n" +
                                $"Total: ₱{totalAmount:N2}\n" +
                                $"Customer: {customerName}\n\n" +
                                $"Receipt has been recorded.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearForm();
                            LoadProducts();
                            LoadStockHistory();
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
                ShowStatus("Error processing sale: " + ex.Message, DangerRed);
                MessageBox.Show($"Failed to process sale:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            nudQuantity.Value = 1;
            txtSellingPrice.Text = "0.00";
            // Reset customer textbox placeholder
            txtCustomer.Text = "Enter customer name (optional)";
            txtCustomer.ForeColor = Color.Gray;
            // Reset notes placeholder
            txtNotes.Text = "Additional notes...";
            txtNotes.ForeColor = Color.Gray;
            lblCurrentStock.Text = "0";
            lblTotalAmount.Text = "₱0.00";
            if (cmbProduct.Items.Count > 0)
                cmbProduct.SelectedIndex = -1;
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
                ForeColor = DarkText
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