﻿using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class SalesInvoiceForm : Form
    {
        // Controls
        private ComboBox cmbCustomer, cmbProduct;
        private NumericUpDown nudQuantity;
        private TextBox txtDiscountPercent, txtTaxPercent, txtBarcode;
        private Label lblSubtotal, lblDiscount, lblTax, lblTotal, lblInvoiceNo;
        private Button btnAddToCart, btnRemoveFromCart, btnProcessSale, btnPrint, btnBarcodeSearch;
        private DataGridView dgvCart;
        private DataTable cartTable;
        private decimal subtotal = 0;

        // Theme colors (same teal theme)
        private static readonly Color BackgroundTeal = Color.FromArgb(134, 183, 181);
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color SuccessGreen = Color.FromArgb(16, 145, 95);
        private static readonly Color DarkGray = Color.FromArgb(44, 47, 58);
        private static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        private static readonly Color TextDark = Color.FromArgb(50, 50, 50);
        private static readonly Color WhiteText = Color.White;

        public SalesInvoiceForm()
        {
            InitializeComponent();
            LoadCustomers();
            LoadProducts();
            InitializeCart();
            GenerateInvoiceNumber();
        }

        private void InitializeComponent()
        {
            this.Text = "Point of Sale - Sales Invoice";
            this.Size = new Size(1300, 850);
            this.MinimumSize = new Size(1100, 750);
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
                Text = "💰 POINT OF SALE - INVOICE",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 18),
                Size = new Size(450, 35),
                ForeColor = WhiteText
            };
            lblInvoiceNo = new Label
            {
                Text = "Invoice: INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 230, 255),
                Location = new Point(850, 20),
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleRight
            };
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblInvoiceNo);

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10,
                BackColor = BackgroundTeal
            };
            splitContainer.SplitterDistance = 760;

            // LEFT PANEL
            Panel leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = BackgroundTeal, AutoScroll = true };

            // Top card
            Panel topCard = CreateCardPanel("CUSTOMER & PRODUCT SELECTION", 200);
            topCard.Location = new Point(0, 0);
            topCard.Size = new Size(730, 200);

            // Customer
            Label lblCustomer = new Label { Text = "Customer:", Location = new Point(20, 45), Size = new Size(80, 27), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = TextDark };
            cmbCustomer = new ComboBox { Location = new Point(110, 42), Size = new Size(220, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            Button btnNewCustomer = new Button { Text = "+ New", Location = new Point(340, 41), Size = new Size(70, 28), BackColor = SuccessGreen, ForeColor = WhiteText, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnNewCustomer.Click += (s, e) => { new CustomerManagementForm().ShowDialog(); LoadCustomers(); };
            btnNewCustomer.FlatAppearance.BorderSize = 0;

            // Barcode
            Label lblBarcode = new Label { Text = "Barcode:", Location = new Point(20, 85), Size = new Size(80, 27), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = TextDark };
            txtBarcode = new TextBox { Location = new Point(110, 82), Size = new Size(160, 27), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            btnBarcodeSearch = new Button { Text = "Search", Location = new Point(280, 81), Size = new Size(80, 28), BackColor = DarkGray, ForeColor = WhiteText, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBarcodeSearch.Click += (s, e) => SearchProductByBarcode();

            // Product selection
            Label lblProduct = new Label { Text = "Product:", Location = new Point(420, 45), Size = new Size(70, 27), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = TextDark };
            cmbProduct = new ComboBox { Location = new Point(500, 42), Size = new Size(180, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            Label lblQty = new Label { Text = "Qty:", Location = new Point(420, 85), Size = new Size(45, 27), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = TextDark };
            nudQuantity = new NumericUpDown { Location = new Point(475, 82), Size = new Size(70, 27), Minimum = 1, Maximum = 9999, Value = 1, Font = new Font("Segoe UI", 10) };
            btnAddToCart = new Button { Text = "➕ Add to Cart", Location = new Point(560, 81), Size = new Size(130, 28), BackColor = SuccessGreen, ForeColor = WhiteText, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddToCart.Click += BtnAddToCart_Click;

            topCard.Controls.AddRange(new Control[] {
                lblCustomer, cmbCustomer, btnNewCustomer,
                lblBarcode, txtBarcode, btnBarcodeSearch,
                lblProduct, cmbProduct, lblQty, nudQuantity, btnAddToCart
            });

            // Cart card
            Panel cartCard = CreateCardPanel("SHOPPING CART", 490);
            cartCard.Location = new Point(0, 215);
            cartCard.Size = new Size(730, 490);

            dgvCart = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(700, 400),
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                AutoGenerateColumns = true   // CHANGE TO TRUE – automatically creates columns from DataTable
            };
            ThemeManager.ConfigureDataGridView(dgvCart);
            cartCard.Controls.Add(dgvCart);

            btnRemoveFromCart = new Button
            {
                Text = "🗑 Remove Selected",
                Location = new Point(15, 450),
                Size = new Size(150, 30),
                BackColor = DangerRed,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRemoveFromCart.Click += (s, e) => RemoveFromCart();
            cartCard.Controls.Add(btnRemoveFromCart);

            leftPanel.Controls.Add(topCard);
            leftPanel.Controls.Add(cartCard);

            // RIGHT PANEL (Summary)
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = BackgroundTeal };

            Panel summaryCard = CreateCardPanel("ORDER SUMMARY", 420);
            summaryCard.Location = new Point(0, 20);
            summaryCard.Size = new Size(440, 420);

            int sy = 45;
            Label lblSubL = new Label { Text = "Subtotal:", Location = new Point(30, sy), Size = new Size(100, 30), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleLeft };
            lblSubtotal = new Label { Text = "₱0.00", Location = new Point(280, sy), Size = new Size(130, 30), Font = new Font("Segoe UI", 11), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleRight };
            sy += 35;

            Label lblDiscL = new Label { Text = "Discount:", Location = new Point(30, sy), Size = new Size(100, 30), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleLeft };
            lblDiscount = new Label { Text = "₱0.00", Location = new Point(280, sy), Size = new Size(130, 30), Font = new Font("Segoe UI", 11), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleRight };
            sy += 35;

            Label lblTaxL = new Label { Text = "Tax:", Location = new Point(30, sy), Size = new Size(100, 30), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleLeft };
            lblTax = new Label { Text = "₱0.00", Location = new Point(280, sy), Size = new Size(130, 30), Font = new Font("Segoe UI", 11), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleRight };
            sy += 45;

            Label lblDiscPct = new Label { Text = "Discount (%):", Location = new Point(30, sy), Size = new Size(100, 30), Font = new Font("Segoe UI", 10), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleLeft };
            txtDiscountPercent = new TextBox { Text = "0", Location = new Point(180, sy), Size = new Size(60, 27), TextAlign = HorizontalAlignment.Right, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtDiscountPercent.TextChanged += (s, e) => CalculateTotal();
            sy += 40;

            Label lblTaxPct = new Label { Text = "Tax (%):", Location = new Point(30, sy), Size = new Size(100, 30), Font = new Font("Segoe UI", 10), ForeColor = TextDark, TextAlign = ContentAlignment.MiddleLeft };
            txtTaxPercent = new TextBox { Text = "0", Location = new Point(180, sy), Size = new Size(60, 27), TextAlign = HorizontalAlignment.Right, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtTaxPercent.TextChanged += (s, e) => CalculateTotal();
            sy += 45;

            Label lblTotalLbl = new Label { Text = "TOTAL:", Location = new Point(30, sy), Size = new Size(100, 35), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = PrimaryBlue, TextAlign = ContentAlignment.MiddleLeft };
            lblTotal = new Label { Text = "₱0.00", Location = new Point(280, sy), Size = new Size(130, 35), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = SuccessGreen, TextAlign = ContentAlignment.MiddleRight };
            sy += 60;

            summaryCard.Controls.Add(lblSubL);
            summaryCard.Controls.Add(lblSubtotal);
            summaryCard.Controls.Add(lblDiscL);
            summaryCard.Controls.Add(lblDiscount);
            summaryCard.Controls.Add(lblTaxL);
            summaryCard.Controls.Add(lblTax);
            summaryCard.Controls.Add(lblDiscPct);
            summaryCard.Controls.Add(txtDiscountPercent);
            summaryCard.Controls.Add(lblTaxPct);
            summaryCard.Controls.Add(txtTaxPercent);
            summaryCard.Controls.Add(lblTotalLbl);
            summaryCard.Controls.Add(lblTotal);

            btnProcessSale = new Button
            {
                Text = "✓ PROCESS SALE",
                Location = new Point(30, 480),
                Size = new Size(180, 45),
                BackColor = SuccessGreen,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnProcessSale.Click += BtnProcessSale_Click;

            btnPrint = new Button
            {
                Text = "🖨 PRINT RECEIPT",
                Location = new Point(230, 480),
                Size = new Size(180, 45),
                BackColor = DarkGray,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.Click += (s, e) => PrintReceipt();

            rightPanel.Controls.Add(summaryCard);
            rightPanel.Controls.Add(btnProcessSale);
            rightPanel.Controls.Add(btnPrint);

            splitContainer.Panel1.Controls.Add(leftPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);
            this.Controls.Add(splitContainer);
            this.Controls.Add(headerPanel);
        }

        private Panel CreateCardPanel(string title, int height)
        {
            Panel card = new Panel { BackColor = CardWhite, BorderStyle = BorderStyle.None, Height = height };
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

        // ==================== DATA METHODS ====================

        private void LoadCustomers()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT CustomerID, FullName FROM Customers ORDER BY FullName");
            cmbCustomer.DataSource = dt;
            cmbCustomer.DisplayMember = "FullName";
            cmbCustomer.ValueMember = "CustomerID";
        }

        private void LoadProducts()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT ProductID, Name, UnitPrice FROM Products WHERE Quantity > 0 ORDER BY Name");
            cmbProduct.DataSource = dt;
            cmbProduct.DisplayMember = "Name";
            cmbProduct.ValueMember = "ProductID";
        }

        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductID", typeof(int));
            cartTable.Columns.Add("Product Name", typeof(string));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Unit Price", typeof(decimal));
            cartTable.Columns.Add("Total", typeof(decimal));
            dgvCart.DataSource = cartTable;
            dgvCart.AutoGenerateColumns = true; // ensures columns appear
        }

        private void GenerateInvoiceNumber()
        {
            lblInvoiceNo.Text = "Invoice: INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void SearchProductByBarcode()
        {
            if (string.IsNullOrEmpty(txtBarcode.Text)) return;
            string query = "SELECT ProductID, Name, UnitPrice FROM Products WHERE ProductCode = @code AND Quantity > 0";
            SqlParameter[] p = { new SqlParameter("@code", txtBarcode.Text.Trim()) };
            DataTable dt = DatabaseHelper.GetDataTable(query, p);
            if (dt.Rows.Count > 0)
            {
                cmbProduct.SelectedValue = Convert.ToInt32(dt.Rows[0]["ProductID"]);
                txtBarcode.Clear();
                nudQuantity.Focus();
            }
            else
                MessageBox.Show("Product not found or out of stock.", "Barcode Scan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product first.", "Add to Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(cmbProduct.SelectedValue);
            string productName = cmbProduct.Text;
            int qty = (int)nudQuantity.Value;
            decimal price = GetProductPrice(productId);
            int available = GetProductStock(productId);
            if (qty > available)
            {
                MessageBox.Show($"Only {available} units available.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal itemTotal = qty * price;
            DataRow[] existing = cartTable.Select($"ProductID = {productId}");
            if (existing.Length > 0)
            {
                int newQty = Convert.ToInt32(existing[0]["Quantity"]) + qty;
                if (newQty > available)
                {
                    MessageBox.Show($"Cannot add {qty}. Only {available} total allowed.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existing[0]["Quantity"] = newQty;
                existing[0]["Total"] = newQty * price;
            }
            else
            {
                cartTable.Rows.Add(productId, productName, qty, price, itemTotal);
            }

            // Force the DataGridView to refresh – crucial
            dgvCart.Refresh();
            CalculateTotal();
            nudQuantity.Value = 1;
        }

        private decimal GetProductPrice(int productId)
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT UnitPrice FROM Products WHERE ProductID=@id", new SqlParameter[] { new SqlParameter("@id", productId) });
            return Convert.ToDecimal(dt.Rows[0]["UnitPrice"]);
        }

        private int GetProductStock(int productId)
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT Quantity FROM Products WHERE ProductID=@id", new SqlParameter[] { new SqlParameter("@id", productId) });
            return Convert.ToInt32(dt.Rows[0]["Quantity"]);
        }

        private void RemoveFromCart()
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                cartTable.Rows.RemoveAt(dgvCart.SelectedRows[0].Index);
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            subtotal = 0;
            foreach (DataRow row in cartTable.Rows)
                subtotal += Convert.ToDecimal(row["Total"]);

            decimal discountPercent = decimal.TryParse(txtDiscountPercent.Text, out decimal d) ? d : 0;
            decimal taxPercent = decimal.TryParse(txtTaxPercent.Text, out decimal t) ? t : 0;
            decimal discountAmount = subtotal * discountPercent / 100;
            decimal taxAmount = subtotal * taxPercent / 100;
            decimal total = subtotal - discountAmount + taxAmount;

            lblSubtotal.Text = $"₱{subtotal:N2}";
            lblDiscount.Text = $"₱{discountAmount:N2}";
            lblTax.Text = $"₱{taxAmount:N2}";
            lblTotal.Text = $"₱{total:N2}";
        }

        private void BtnProcessSale_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty! Add products first.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbCustomer.SelectedValue == null)
                cmbCustomer.SelectedValue = 1;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        decimal discountPercent = decimal.TryParse(txtDiscountPercent.Text, out decimal d) ? d : 0;
                        decimal discountAmount = subtotal * discountPercent / 100;
                        decimal taxPercent = decimal.TryParse(txtTaxPercent.Text, out decimal t) ? t : 0;
                        decimal taxAmount = subtotal * taxPercent / 100;
                        decimal totalAmount = subtotal - discountAmount + taxAmount;
                        string invoiceNumber = lblInvoiceNo.Text.Replace("Invoice: ", "");

                        string saleQuery = @"INSERT INTO Sales (InvoiceNumber, CustomerID, Subtotal, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, TotalAmount, PaymentMethod, CreatedBy)
                                             VALUES (@inv, @cust, @sub, @discPct, @discAmt, @taxPct, @taxAmt, @total, 'Cash', @user);
                                             SELECT SCOPE_IDENTITY();";
                        SqlCommand saleCmd = new SqlCommand(saleQuery, conn, trans);
                        saleCmd.Parameters.AddWithValue("@inv", invoiceNumber);
                        saleCmd.Parameters.AddWithValue("@cust", cmbCustomer.SelectedValue);
                        saleCmd.Parameters.AddWithValue("@sub", subtotal);
                        saleCmd.Parameters.AddWithValue("@discPct", discountPercent);
                        saleCmd.Parameters.AddWithValue("@discAmt", discountAmount);
                        saleCmd.Parameters.AddWithValue("@taxPct", taxPercent);
                        saleCmd.Parameters.AddWithValue("@taxAmt", taxAmount);
                        saleCmd.Parameters.AddWithValue("@total", totalAmount);
                        saleCmd.Parameters.AddWithValue("@user", Global.UserID);
                        int saleId = Convert.ToInt32(saleCmd.ExecuteScalar());

                        foreach (DataRow row in cartTable.Rows)
                        {
                            string itemQuery = @"INSERT INTO SaleItems (SaleID, ProductID, Quantity, UnitPrice, TotalPrice)
                                                 VALUES (@saleId, @pid, @qty, @price, @total)";
                            SqlCommand itemCmd = new SqlCommand(itemQuery, conn, trans);
                            itemCmd.Parameters.AddWithValue("@saleId", saleId);
                            itemCmd.Parameters.AddWithValue("@pid", row["ProductID"]);
                            itemCmd.Parameters.AddWithValue("@qty", row["Quantity"]);
                            itemCmd.Parameters.AddWithValue("@price", row["Unit Price"]);
                            itemCmd.Parameters.AddWithValue("@total", row["Total"]);
                            itemCmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show($"Sale completed!\nTotal: ₱{totalAmount:N2}\nInvoice: {invoiceNumber}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        PrintReceipt();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Transaction failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PrintReceipt()
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == DialogResult.OK)
            {
                PrintDocument doc = new PrintDocument();
                doc.PrintPage += (s, e) =>
                {
                    int y = 50;
                    e.Graphics.DrawString("INVENTORY PRO - RECEIPT", new Font("Consolas", 14, FontStyle.Bold), Brushes.Black, 50, y); y += 35;
                    e.Graphics.DrawString(lblInvoiceNo.Text, new Font("Consolas", 10), Brushes.Black, 50, y); y += 25;
                    e.Graphics.DrawString($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", new Font("Consolas", 10), Brushes.Black, 50, y); y += 25;
                    e.Graphics.DrawString($"Customer: {cmbCustomer.Text}", new Font("Consolas", 10), Brushes.Black, 50, y); y += 30;
                    e.Graphics.DrawString("----------------------------------------", new Font("Consolas", 10), Brushes.Black, 50, y); y += 20;
                    e.Graphics.DrawString("Item", new Font("Consolas", 10, FontStyle.Bold), Brushes.Black, 50, y);
                    e.Graphics.DrawString("Qty", new Font("Consolas", 10, FontStyle.Bold), Brushes.Black, 300, y);
                    e.Graphics.DrawString("Price", new Font("Consolas", 10, FontStyle.Bold), Brushes.Black, 380, y);
                    e.Graphics.DrawString("Total", new Font("Consolas", 10, FontStyle.Bold), Brushes.Black, 480, y); y += 25;
                    foreach (DataRow row in cartTable.Rows)
                    {
                        e.Graphics.DrawString(row["Product Name"].ToString(), new Font("Consolas", 10), Brushes.Black, 50, y);
                        e.Graphics.DrawString(row["Quantity"].ToString(), new Font("Consolas", 10), Brushes.Black, 300, y);
                        e.Graphics.DrawString($"₱{row["Unit Price"]:N2}", new Font("Consolas", 10), Brushes.Black, 380, y);
                        e.Graphics.DrawString($"₱{row["Total"]:N2}", new Font("Consolas", 10), Brushes.Black, 480, y);
                        y += 20;
                    }
                    e.Graphics.DrawString($"Subtotal: ₱{subtotal:N2}", new Font("Consolas", 10), Brushes.Black, 350, y); y += 20;
                    e.Graphics.DrawString($"Discount: ₱{lblDiscount.Text.Replace("₱", "")}", new Font("Consolas", 10), Brushes.Black, 350, y); y += 20;
                    e.Graphics.DrawString($"Tax: ₱{lblTax.Text.Replace("₱", "")}", new Font("Consolas", 10), Brushes.Black, 350, y); y += 20;
                    e.Graphics.DrawString($"TOTAL: {lblTotal.Text}", new Font("Consolas", 12, FontStyle.Bold), Brushes.Black, 350, y);
                };
                doc.Print();
            }
        }
    }
}