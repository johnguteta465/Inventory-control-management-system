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
        private TextBox txtName, txtCode, txtPrice, txtReorderLevel;
        private ComboBox cmbCategory, cmbSupplier;
        private Button btnSave, btnDelete, btnRefresh, btnSearch, btnBarcode, btnPrintBarcode;
        private TextBox txtSearch;
        private int selectedProductId = 0;
        private Panel headerPanel;

        public ProductForm()
        {
            InitializeComponent();
            LoadProducts();
            LoadComboBoxes();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Products";
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
                Text = "📦 PRODUCT MANAGEMENT",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 25),
                Size = new Size(400, 35),
                ForeColor = Color.White
            };

            headerPanel.Controls.Add(lblTitle);

            // Search Panel
            Panel searchPanel = new Panel()
            {
                Location = new Point(20, 100),
                Size = new Size(this.Width - 40, 45),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblSearch = new Label() { Text = "🔍 Search:", Location = new Point(10, 12), Size = new Size(60, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtSearch = new TextBox() { Location = new Point(75, 10), Size = new Size(200, 25), Font = new Font("Segoe UI", 10) };
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) SearchProducts(); };

            btnSearch = new Button()
            {
                Text = "Search",
                Location = new Point(285, 9),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.Click += (s, e) => SearchProducts();

            Button btnShowAll = new Button()
            {
                Text = "Show All",
                Location = new Point(375, 9),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnShowAll.Click += (s, e) => { txtSearch.Clear(); LoadProducts(); };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, btnShowAll });

            // Input Panel
            Panel inputPanel = new Panel()
            {
                Location = new Point(20, 160),
                Size = new Size(this.Width - 40, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            int y = 15;
            int col1 = 20;
            int col2 = 150;
            int col3 = 400;
            int col4 = 530;

            // Row 1 - Product Code & Name
            Label lblCode = new Label() { Text = "📋 Product Code:", Location = new Point(col1, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtCode = new TextBox() { Location = new Point(col2, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10) };

            Label lblName = new Label() { Text = "🏷️ Product Name:", Location = new Point(col3, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtName = new TextBox() { Location = new Point(col4, y), Size = new Size(250, 25), Font = new Font("Segoe UI", 10) };

            inputPanel.Controls.Add(lblCode);
            inputPanel.Controls.Add(txtCode);
            inputPanel.Controls.Add(lblName);
            inputPanel.Controls.Add(txtName);
            y += 40;

            // Row 2 - Category & Supplier
            Label lblCategory = new Label() { Text = "📁 Category:", Location = new Point(col1, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            cmbCategory = new ComboBox() { Location = new Point(col2, y), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            Label lblSupplier = new Label() { Text = "🏢 Supplier:", Location = new Point(col3, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            cmbSupplier = new ComboBox() { Location = new Point(col4, y), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            inputPanel.Controls.Add(lblCategory);
            inputPanel.Controls.Add(cmbCategory);
            inputPanel.Controls.Add(lblSupplier);
            inputPanel.Controls.Add(cmbSupplier);
            y += 40;

            // Row 3 - Unit Price & Reorder Level
            Label lblPrice = new Label() { Text = "💰 Unit Price:", Location = new Point(col1, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtPrice = new TextBox() { Location = new Point(col2, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10) };

            Label lblReorder = new Label() { Text = "⚠️ Reorder Level:", Location = new Point(col3, y + 3), Size = new Size(110, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtReorderLevel = new TextBox() { Location = new Point(col4, y), Size = new Size(180, 25), Text = "10", Font = new Font("Segoe UI", 10) };

            inputPanel.Controls.Add(lblPrice);
            inputPanel.Controls.Add(txtPrice);
            inputPanel.Controls.Add(lblReorder);
            inputPanel.Controls.Add(txtReorderLevel);
            y += 50;

            // Buttons Row
            btnSave = new Button()
            {
                Text = "💾 SAVE",
                Location = new Point(20, y),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnDelete = new Button()
            {
                Text = "🗑 DELETE",
                Location = new Point(130, y),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;

            btnRefresh = new Button()
            {
                Text = "🔄 REFRESH",
                Location = new Point(240, y),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;

            btnBarcode = new Button()
            {
                Text = "📷 SCAN",
                Location = new Point(550, y),
                Size = new Size(90, 38),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBarcode.FlatAppearance.BorderSize = 0;
            btnBarcode.Click += BtnBarcode_Click;

            btnPrintBarcode = new Button()
            {
                Text = "🖨 PRINT",
                Location = new Point(650, y),
                Size = new Size(90, 38),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrintBarcode.FlatAppearance.BorderSize = 0;
            btnPrintBarcode.Click += BtnPrintBarcode_Click;

            inputPanel.Controls.AddRange(new Control[] { btnSave, btnDelete, btnRefresh, btnBarcode, btnPrintBarcode });

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadProducts(); ClearForm(); };

            // DataGridView
            dgvProducts = new DataGridView()
            {
                Location = new Point(20, 325),
                Size = new Size(this.Width - 40, 370),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 249, 250) },
                GridColor = Color.FromArgb(220, 220, 220)
            };
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;

            this.Controls.Add(headerPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(inputPanel);
            this.Controls.Add(dgvProducts);

            // Resize event
            this.Resize += (s, e) => {
                headerPanel.Width = this.Width;
                searchPanel.Width = this.Width - 40;
                inputPanel.Width = this.Width - 40;
                dgvProducts.Width = this.Width - 40;
                dgvProducts.Height = this.Height - 380;
            };
        }

        private void LoadComboBoxes()
        {
            DataTable categories = DatabaseHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryID";
            cmbCategory.DataSource = categories;

            DataTable suppliers = DatabaseHelper.GetDataTable("SELECT SupplierID, Name FROM Suppliers ORDER BY Name");
            cmbSupplier.DisplayMember = "Name";
            cmbSupplier.ValueMember = "SupplierID";
            cmbSupplier.DataSource = suppliers;
        }

        private void LoadProducts()
        {
            string query = @"SELECT p.ProductID, p.ProductCode, p.Name, c.CategoryName, s.Name as SupplierName, 
                                    p.Quantity, p.UnitPrice, p.ReorderLevel
                             FROM Products p
                             LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                             LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                             ORDER BY p.ProductID";

            DataTable dt = DatabaseHelper.GetDataTable(query);
            dgvProducts.DataSource = dt;
            dgvProducts.Columns["ProductID"].Visible = false;
        }

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Rows[e.RowIndex].DataBoundItem != null)
            {
                int quantity = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["Quantity"].Value);
                int reorderLevel = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["ReorderLevel"].Value);

                if (quantity <= reorderLevel && quantity > 0)
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                }
                else if (quantity == 0)
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 180, 180);
                }
                else
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                }
            }
        }

        private void SearchProducts()
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                LoadProducts();
                return;
            }

            string query = @"SELECT p.ProductID, p.ProductCode, p.Name, c.CategoryName, s.Name as SupplierName, 
                                    p.Quantity, p.UnitPrice, p.ReorderLevel
                             FROM Products p
                             LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                             LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                             WHERE p.Name LIKE @search OR p.ProductCode LIKE @search";

            SqlParameter[] parameters = { new SqlParameter("@search", "%" + txtSearch.Text + "%") };
            DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
            dgvProducts.DataSource = dt;
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                selectedProductId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
                txtCode.Text = dgvProducts.SelectedRows[0].Cells["ProductCode"].Value.ToString();
                txtName.Text = dgvProducts.SelectedRows[0].Cells["Name"].Value.ToString();
                txtPrice.Text = dgvProducts.SelectedRows[0].Cells["UnitPrice"].Value.ToString();
                txtReorderLevel.Text = dgvProducts.SelectedRows[0].Cells["ReorderLevel"].Value.ToString();
            }
        }

        private void BtnBarcode_Click(object sender, EventArgs e)
        {
            BarcodeScannerForm scanner = new BarcodeScannerForm();
            if (scanner.ShowDialog() == DialogResult.OK)
            {
                txtCode.Text = scanner.ScannedProductCode;
                SearchProductByCode(scanner.ScannedProductCode);
            }
        }

        private void BtnPrintBarcode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCode.Text) && !string.IsNullOrEmpty(txtName.Text))
            {
                try
                {
                    BarcodeGenerator.PrintBarcodeLabel(txtCode.Text, txtName.Text, txtPrice.Text);
                    MessageBox.Show("Barcode sent to printer!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Print error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter product code and name first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchProductByCode(string productCode)
        {
            string query = @"SELECT ProductID, Name, UnitPrice, ReorderLevel, CategoryID, SupplierID 
                             FROM Products WHERE ProductCode = @code";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@code", productCode);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            selectedProductId = reader.GetInt32(0);
                            txtName.Text = reader["Name"].ToString();
                            txtPrice.Text = reader["UnitPrice"].ToString();
                            txtReorderLevel.Text = reader["ReorderLevel"].ToString();

                            MessageBox.Show($"Product found: {txtName.Text}", "Product Found",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Product not found. You can add it as a new product.",
                                "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCode.Text) || string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Product Code and Name are required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Valid Unit Price is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (selectedProductId == 0)
                {
                    string query = @"INSERT INTO Products (ProductCode, Name, CategoryID, SupplierID, UnitPrice, ReorderLevel, Quantity) 
                                  VALUES (@code, @name, @cat, @sup, @price, @reorder, 0)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@code", txtCode.Text),
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value),
                        new SqlParameter("@sup", cmbSupplier.SelectedValue ?? DBNull.Value),
                        new SqlParameter("@price", price),
                        new SqlParameter("@reorder", Convert.ToInt32(txtReorderLevel.Text))
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string query = @"UPDATE Products SET ProductCode=@code, Name=@name, CategoryID=@cat, 
                                  SupplierID=@sup, UnitPrice=@price, ReorderLevel=@reorder WHERE ProductID=@id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@code", txtCode.Text),
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value),
                        new SqlParameter("@sup", cmbSupplier.SelectedValue ?? DBNull.Value),
                        new SqlParameter("@price", price),
                        new SqlParameter("@reorder", Convert.ToInt32(txtReorderLevel.Text)),
                        new SqlParameter("@id", selectedProductId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadProducts();
                LoadComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0)
            {
                MessageBox.Show("Please select a product to delete", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Delete this product? This action cannot be undone.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Products WHERE ProductID=@id",
                        new SqlParameter[] { new SqlParameter("@id", selectedProductId) });
                    MessageBox.Show("Product deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot delete: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
        }
    }
}