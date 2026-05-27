using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class ProductForm : Form
    {
        private DataGridView dgvProducts;
        private TextBox txtName, txtCode, txtPrice, txtReorderLevel, txtSearch;
        private ComboBox cmbCategory, cmbSupplier;
        private Button btnSave, btnDelete, btnRefresh, btnSearch, btnBarcode, btnPrintBarcode, btnClear;
        private Label lblStatus;
        private int selectedProductId = 0;

        private static readonly Color BgTeal = Color.White; // Changed from teal to white
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color SuccessGreen = Color.FromArgb(16, 145, 95);
        private static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        private static readonly Color DarkGray = Color.FromArgb(44, 47, 58);
        private static readonly Color PurpleColor = Color.FromArgb(142, 68, 173);
        private static readonly Color AmberColor = Color.FromArgb(243, 156, 18);
        private static readonly Color WhiteText = Color.White;
        private static readonly Color ProductDarkText = Color.FromArgb(44, 62, 80);
        private static readonly Color MutedText = Color.FromArgb(127, 140, 141);
        private static readonly Color BorderLight = Color.FromArgb(220, 225, 230);

        public ProductForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new Font("Segoe UI", 9.5F);
            BuildUI();
            this.Load += (s, e) => {
                LoadComboBoxes();
                LoadProducts();
                if (dgvProducts != null)
                {
                    dgvProducts.Visible = true;
                    dgvProducts.BringToFront();
                    dgvProducts.Refresh();
                }
            };
        }

        private void BuildUI()
        {
            this.Text = "Product Management";
            this.Size = new Size(1300, 820);
            this.MinimumSize = new Size(1100, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BgTeal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = PrimaryBlue };
            header.Paint += (s, e) =>
            {
                using (var b = new LinearGradientBrush(header.ClientRectangle, PrimaryBlue, ControlPaint.Dark(PrimaryBlue, 0.12f), 90f))
                    e.Graphics.FillRectangle(b, header.ClientRectangle);
            };
            header.Controls.Add(new Label { Text = "PRODUCT MANAGEMENT", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = WhiteText, BackColor = Color.Transparent, Location = new Point(20, 15), Size = new Size(500, 30) });

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 2, BackColor = Color.White, SplitterDistance = 300 };
            split.Panel1.Padding = new Padding(8, 5, 2, 5);
            split.Panel1.BackColor = Color.FromArgb(245, 245, 245); // Light gray for left panel
            split.Panel2.Padding = new Padding(2, 5, 8, 5);
            split.Panel2.BackColor = Color.White; // White for right panel (DataGridView)

            Panel leftScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(245, 245, 245) }; // Light gray background

            Panel searchCard = MakeCard("SEARCH");
            searchCard.Size = new Size(290, 55);
            searchCard.Location = new Point(0, 0);
            txtSearch = new TextBox { Location = new Point(8, 25), Size = new Size(140, 20), Font = new Font("Segoe UI", 8.5f), BorderStyle = BorderStyle.FixedSingle, Text = "Search by name or code...", ForeColor = Color.Gray };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.ForeColor == Color.Gray) { txtSearch.Text = ""; txtSearch.ForeColor = ProductDarkText; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Search by name or code..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) SearchProducts(); };
            btnSearch = MakeBtn("Search", new Point(155, 24), new Size(60, 20), PrimaryBlue);
            btnSearch.Click += (s, e) => SearchProducts();
            Button btnShowAll = MakeBtn("Show All", new Point(220, 24), new Size(60, 20), SuccessGreen);
            btnShowAll.Click += (s, e) => { txtSearch.Text = "Search by name or code..."; txtSearch.ForeColor = Color.Gray; LoadProducts(); };
            searchCard.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnShowAll });

            Panel detailCard = MakeCard("PRODUCT DETAILS");
            detailCard.Size = new Size(290, 240);
            detailCard.Location = new Point(0, 60);
            int lw = 80, fw = 190, lx = 8, fx = lx + lw + 4, dy = 25;
            const int DR = 32;
            detailCard.Controls.Add(MakeLbl("Product Code:", lx, dy, lw)); txtCode = MakeField(fx, dy, fw); detailCard.Controls.Add(txtCode); dy += DR;
            detailCard.Controls.Add(MakeLbl("Product Name:", lx, dy, lw)); txtName = MakeField(fx, dy, fw); detailCard.Controls.Add(txtName); dy += DR;
            detailCard.Controls.Add(MakeLbl("Category:", lx, dy, lw)); cmbCategory = MakeCombo(fx, dy, fw); detailCard.Controls.Add(cmbCategory); dy += DR;
            detailCard.Controls.Add(MakeLbl("Supplier:", lx, dy, lw)); cmbSupplier = MakeCombo(fx, dy, fw); detailCard.Controls.Add(cmbSupplier); dy += DR;
            detailCard.Controls.Add(MakeLbl("Unit Price:", lx, dy, lw)); txtPrice = MakeField(fx, dy, fw); detailCard.Controls.Add(txtPrice); dy += DR;
            detailCard.Controls.Add(MakeLbl("Reorder Level:", lx, dy, lw)); txtReorderLevel = MakeField(fx, dy, 70); txtReorderLevel.Text = "10"; detailCard.Controls.Add(txtReorderLevel);

            Panel actCard = MakeCard("ACTIONS");
            actCard.Size = new Size(290, 80);
            actCard.Location = new Point(0, 305);
            const int BW = 85, BH = 26, BG = 4;
            int bx = 8, by = 24;
            btnSave = MakeBtn("SAVE", new Point(bx, by), new Size(BW, BH), SuccessGreen); bx += BW + BG;
            btnDelete = MakeBtn("DELETE", new Point(bx, by), new Size(BW, BH), DangerRed); bx += BW + BG;
            btnClear = MakeBtn("CLEAR", new Point(bx, by), new Size(BW, BH), DarkGray);
            by += BH + BG; bx = 8;
            btnRefresh = MakeBtn("REFRESH", new Point(bx, by), new Size(BW, BH), PrimaryBlue); bx += BW + BG;
            btnBarcode = MakeBtn("SCAN", new Point(bx, by), new Size(BW, BH), PurpleColor); bx += BW + BG;
            btnPrintBarcode = MakeBtn("PRINT", new Point(bx, by), new Size(BW, BH), AmberColor);
            btnSave.Click += BtnSave_Click; btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();
            btnRefresh.Click += (s, e) => { LoadProducts(); ClearForm(); };
            btnBarcode.Click += BtnBarcode_Click; btnPrintBarcode.Click += BtnPrintBarcode_Click;
            actCard.Controls.AddRange(new Control[] { btnSave, btnDelete, btnClear, btnRefresh, btnBarcode, btnPrintBarcode });

            lblStatus = new Label { Location = new Point(0, 390), Size = new Size(290, 15), Font = new Font("Segoe UI", 7f, FontStyle.Italic), ForeColor = MutedText, BackColor = Color.Transparent, Text = "Fill in the fields above to add a new product." };

            leftScroll.Controls.AddRange(new Control[] { searchCard, detailCard, actCard, lblStatus });
            split.Panel1.Controls.Add(leftScroll);

            dgvProducts = new DataGridView();
            ConfigureCleanDataGridView();

            dgvProducts.DataBindingComplete += DgvProducts_DataBindingComplete;
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            split.Panel2.Controls.Add(dgvProducts);
            split.Panel2.BackColor = Color.White;

            this.Controls.Add(split);
            this.Controls.Add(header);
        }

        private void ConfigureCleanDataGridView()
        {
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.MultiSelect = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.BorderStyle = BorderStyle.FixedSingle;
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.GridColor = Color.FromArgb(200, 200, 200);
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.Visible = true;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; // Changed to AllCells for proper sizing
            dgvProducts.RowTemplate.Height = 32;
            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219); // Blue header
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvProducts.ColumnHeadersHeight = 40;
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgvProducts.ScrollBars = ScrollBars.Both; // Enable both horizontal and vertical scrollbars
        }

        // SINGLE DataBindingComplete handler - REMOVE THE DUPLICATE ONE
        private void DgvProducts_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvProducts.Columns.Count == 0) return;

            // Hide ID column
            if (dgvProducts.Columns.Contains("ProductID"))
                dgvProducts.Columns["ProductID"].Visible = false;

            // Configure columns with specific widths
            if (dgvProducts.Columns.Contains("ProductCode"))
            {
                dgvProducts.Columns["ProductCode"].HeaderText = "Product Code";
                dgvProducts.Columns["ProductCode"].Width = 100;
            }
            if (dgvProducts.Columns.Contains("Name"))
            {
                dgvProducts.Columns["Name"].HeaderText = "Product Name";
                dgvProducts.Columns["Name"].Width = 150;
            }
            if (dgvProducts.Columns.Contains("CategoryName"))
            {
                dgvProducts.Columns["CategoryName"].HeaderText = "Category";
                dgvProducts.Columns["CategoryName"].Width = 120;
            }
            if (dgvProducts.Columns.Contains("SupplierName"))
            {
                dgvProducts.Columns["SupplierName"].HeaderText = "Supplier";
                dgvProducts.Columns["SupplierName"].Width = 140;
            }
            if (dgvProducts.Columns.Contains("Quantity"))
            {
                dgvProducts.Columns["Quantity"].HeaderText = "Stock";
                dgvProducts.Columns["Quantity"].Width = 80;
            }
            if (dgvProducts.Columns.Contains("UnitPrice"))
            {
                dgvProducts.Columns["UnitPrice"].HeaderText = "Price";
                dgvProducts.Columns["UnitPrice"].Width = 100;
                dgvProducts.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
            }
            if (dgvProducts.Columns.Contains("ReorderLevel"))
            {
                dgvProducts.Columns["ReorderLevel"].HeaderText = "Reorder Level";
                dgvProducts.Columns["ReorderLevel"].Width = 100;
            }
        }

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex >= 0 && dgvProducts.Columns[e.ColumnIndex].Name == "UnitPrice" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal price))
                {
                    e.Value = price.ToString("C2");
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            var row = dgvProducts.SelectedRows[0];
            if (row.Cells["ProductID"]?.Value == null) return;
            selectedProductId = Convert.ToInt32(row.Cells["ProductID"].Value);
            txtCode.Text = row.Cells["ProductCode"]?.Value?.ToString() ?? "";
            txtName.Text = row.Cells["Name"]?.Value?.ToString() ?? "";
            txtPrice.Text = row.Cells["UnitPrice"]?.Value?.ToString() ?? "";
            txtReorderLevel.Text = row.Cells["ReorderLevel"]?.Value?.ToString() ?? "";
            cmbCategory.Text = row.Cells["CategoryName"]?.Value?.ToString() ?? "";
            cmbSupplier.Text = row.Cells["SupplierName"]?.Value?.ToString() ?? "";
            SetStatus($"Editing: {txtName.Text}", PrimaryBlue);
        }

        private void LoadComboBoxes()
        {
            var cats = DatabaseHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");
            cmbCategory.DisplayMember = "CategoryName"; cmbCategory.ValueMember = "CategoryID"; cmbCategory.DataSource = cats;
            var sups = DatabaseHelper.GetDataTable("SELECT SupplierID, Name FROM Suppliers ORDER BY Name");
            cmbSupplier.DisplayMember = "Name"; cmbSupplier.ValueMember = "SupplierID"; cmbSupplier.DataSource = sups;
        }

        private void LoadProducts()
        {
            try
            {
                string query = "SELECT p.ProductID, p.ProductCode, p.Name, ISNULL(c.CategoryName,'Uncategorized') AS CategoryName, ISNULL(s.Name,'No Supplier') AS SupplierName, p.Quantity, p.UnitPrice, p.ReorderLevel FROM Products p LEFT JOIN Categories c ON p.CategoryID=c.CategoryID LEFT JOIN Suppliers s ON p.SupplierID=s.SupplierID ORDER BY p.Name";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvProducts.DataSource = dt;
                SetStatus($"Showing {dt.Rows.Count} product(s).", MutedText);
                
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No products found in database.\n\nPlease run Database_Setup.sql to create sample data.", 
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading products: {ex.Message}", DangerRed);
                MessageBox.Show($"Error loading products:\n\n{ex.Message}\n\nPlease check database connection.", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchProducts()
        {
            string q = txtSearch.ForeColor == Color.Gray ? "" : txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(q)) { LoadProducts(); return; }
            string query = "SELECT p.ProductID, p.ProductCode, p.Name, ISNULL(c.CategoryName,'Uncategorized') AS CategoryName, ISNULL(s.Name,'No Supplier') AS SupplierName, p.Quantity, p.UnitPrice, p.ReorderLevel FROM Products p LEFT JOIN Categories c ON p.CategoryID=c.CategoryID LEFT JOIN Suppliers s ON p.SupplierID=s.SupplierID WHERE p.Name LIKE @s OR p.ProductCode LIKE @s";
            DataTable dt = DatabaseHelper.GetDataTable(query, new[] { new SqlParameter("@s", "%" + q + "%") });
            dgvProducts.DataSource = dt;
            SetStatus($"{dt.Rows.Count} result(s) for \"{q}\".", PrimaryBlue);
        }

        private void BtnBarcode_Click(object sender, EventArgs e)
        {
            var scanner = new BarcodeScannerForm();
            if (scanner.ShowDialog() == DialogResult.OK)
            {
                txtCode.Text = scanner.ScannedProductCode;
                SearchProductByCode(scanner.ScannedProductCode);
            }
        }

        private void BtnPrintBarcode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCode.Text) || string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Please select or enter a product first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                BarcodeGenerator.PrintBarcodeLabel(txtCode.Text, txtName.Text, txtPrice.Text);
                SetStatus("Barcode sent to printer.", SuccessGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchProductByCode(string code)
        {
            using (var conn = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand("SELECT ProductID,Name,UnitPrice,ReorderLevel,CategoryID,SupplierID FROM Products WHERE ProductCode=@code", conn))
            {
                cmd.Parameters.AddWithValue("@code", code); conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        selectedProductId = r.GetInt32(0);
                        txtName.Text = r["Name"].ToString();
                        txtPrice.Text = r["UnitPrice"].ToString();
                        txtReorderLevel.Text = r["ReorderLevel"].ToString();
                        cmbCategory.SelectedValue = r.GetInt32(4);
                        cmbSupplier.SelectedValue = r.GetInt32(5);
                        SetStatus($"Product found: {txtName.Text}", SuccessGreen);
                    }
                    else SetStatus("Product not found.", DangerRed);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                SetStatus("Product Code and Name are required.", DangerRed);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                SetStatus("A valid Unit Price is required.", DangerRed);
                return;
            }
            if (!int.TryParse(txtReorderLevel.Text, out int reorder) || reorder < 0)
            {
                SetStatus("A valid Reorder Level is required.", DangerRed);
                return;
            }
            try
            {
                if (selectedProductId == 0)
                    DatabaseHelper.ExecuteNonQuery("INSERT INTO Products(ProductCode,Name,CategoryID,SupplierID,UnitPrice,ReorderLevel,Quantity) VALUES(@code,@name,@cat,@sup,@price,@reorder,0)",
                        new[] { new SqlParameter("@code", txtCode.Text.Trim()), new SqlParameter("@name", txtName.Text.Trim()), new SqlParameter("@cat", cmbCategory.SelectedValue ?? (object)DBNull.Value), new SqlParameter("@sup", cmbSupplier.SelectedValue ?? (object)DBNull.Value), new SqlParameter("@price", price), new SqlParameter("@reorder", reorder) });
                else
                    DatabaseHelper.ExecuteNonQuery("UPDATE Products SET ProductCode=@code,Name=@name,CategoryID=@cat,SupplierID=@sup,UnitPrice=@price,ReorderLevel=@reorder WHERE ProductID=@id",
                        new[] { new SqlParameter("@code", txtCode.Text.Trim()), new SqlParameter("@name", txtName.Text.Trim()), new SqlParameter("@cat", cmbCategory.SelectedValue ?? (object)DBNull.Value), new SqlParameter("@sup", cmbSupplier.SelectedValue ?? (object)DBNull.Value), new SqlParameter("@price", price), new SqlParameter("@reorder", reorder), new SqlParameter("@id", selectedProductId) });
                SetStatus($"Product saved successfully.", SuccessGreen);
                ClearForm(); LoadProducts(); LoadComboBoxes();
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message, DangerRed);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0)
            {
                SetStatus("Select a product from the list first.", DangerRed);
                return;
            }
            int used = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT (SELECT COUNT(*) FROM StockIn WHERE ProductID=@id)+(SELECT COUNT(*) FROM SaleItems WHERE ProductID=@id)", new[] { new SqlParameter("@id", selectedProductId) }) ?? 0);
            if (used > 0)
            {
                MessageBox.Show($"Cannot delete - {used} transaction(s) exist.", "Delete Prevented", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Permanently delete \"{txtName.Text}\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Products WHERE ProductID=@id", new[] { new SqlParameter("@id", selectedProductId) });
                    SetStatus("Product deleted.", SuccessGreen);
                    ClearForm();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    SetStatus("Error: " + ex.Message, DangerRed);
                }
            }
        }

        private void ClearForm()
        {
            selectedProductId = 0;
            txtCode.Clear();
            txtName.Clear();
            txtPrice.Clear();
            txtReorderLevel.Text = "10";
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = -1;
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = -1;
            SetStatus("Ready - fill in the fields to add a new product.", MutedText);
        }

        private void SetStatus(string msg, Color color)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = color;
        }

        private Panel MakeCard(string title)
        {
            Panel card = new Panel { BackColor = CardWhite };
            card.Paint += (s, e) => { var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; var path = RoundedPath(card.ClientRectangle, 10); card.Region = new Region(path); using (var pen = new Pen(BorderLight, 1)) g.DrawPath(pen, path); using (var f = new Font("Segoe UI", 9.5f, FontStyle.Bold)) using (var b = new SolidBrush(PrimaryBlue)) g.DrawString(title, f, b, new PointF(14, 10)); };
            return card;
        }

        private GraphicsPath RoundedPath(Rectangle r, int rad) { var p = new GraphicsPath(); p.AddArc(r.X, r.Y, rad, rad, 180, 90); p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90); p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90); p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90); p.CloseFigure(); return p; }
        private Label MakeLbl(string t, int x, int y, int w) => new Label { Text = t, Location = new Point(x, y + 3), Size = new Size(w, 24), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ProductDarkText };
        private TextBox MakeField(int x, int y, int w) => new TextBox { Location = new Point(x, y), Size = new Size(w, 26), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 251, 252) };
        private ComboBox MakeCombo(int x, int y, int w) => new ComboBox { Location = new Point(x, y), Size = new Size(w, 26), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(250, 251, 252) };
        private Button MakeBtn(string text, Point loc, Size size, Color back) { var btn = new Button { Text = text, Location = loc, Size = size, BackColor = back, ForeColor = WhiteText, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand }; btn.FlatAppearance.BorderSize = 0; btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(back, 0.1f); btn.MouseLeave += (s, e) => btn.BackColor = back; return btn; }
    }
}