using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class StockInForm : Form
    {
        private ComboBox cmbProduct, cmbSupplier;
        private NumericUpDown nudQuantity;
        private TextBox txtUnitCost, txtNotes;
        private Button btnSave, btnRefresh;
        private Label lblCurrentStock, lblProductPrice;
        private DataGridView dgvHistory;

        public StockInForm()
        {
            InitializeComponent();
            LoadProducts();
            LoadSuppliers();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock In (Purchase Order)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "STOCK IN - PURCHASE ORDER",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 35),
                ForeColor = Color.DarkBlue
            };

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 60), Size = new Size(400, 340), BackColor = Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };

            int y = 20;

            // Product
            Label lblProduct = new Label() { Text = "Product:", Location = new Point(10, y), Size = new Size(100, 25) };
            cmbProduct = new ComboBox() { Location = new Point(110, y), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProduct.SelectedIndexChanged += (s, e) => LoadProductInfo();
            inputPanel.Controls.Add(lblProduct);
            inputPanel.Controls.Add(cmbProduct);
            y += 40;

            // Supplier
            Label lblSupplier = new Label() { Text = "Supplier:", Location = new Point(10, y), Size = new Size(100, 25) };
            cmbSupplier = new ComboBox() { Location = new Point(110, y), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(lblSupplier);
            inputPanel.Controls.Add(cmbSupplier);
            y += 40;

            // Current Stock
            Label lblStock = new Label() { Text = "Current Stock:", Location = new Point(10, y), Size = new Size(100, 25) };
            lblCurrentStock = new Label() { Text = "0", Location = new Point(110, y), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Blue };
            inputPanel.Controls.Add(lblStock);
            inputPanel.Controls.Add(lblCurrentStock);
            y += 40;

            // Selling Price
            Label lblPrice = new Label() { Text = "Selling Price:", Location = new Point(10, y), Size = new Size(100, 25) };
            lblProductPrice = new Label() { Text = "0.00", Location = new Point(110, y), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Green };
            inputPanel.Controls.Add(lblPrice);
            inputPanel.Controls.Add(lblProductPrice);
            y += 40;

            // Quantity
            Label lblQty = new Label() { Text = "Quantity:", Location = new Point(10, y), Size = new Size(100, 25) };
            nudQuantity = new NumericUpDown() { Location = new Point(110, y), Size = new Size(150, 25), Minimum = 1, Maximum = 100000, Value = 1 };
            inputPanel.Controls.Add(lblQty);
            inputPanel.Controls.Add(nudQuantity);
            y += 40;

            // Unit Cost
            Label lblCost = new Label() { Text = "Unit Cost:", Location = new Point(10, y), Size = new Size(100, 25) };
            txtUnitCost = new TextBox() { Location = new Point(110, y), Size = new Size(150, 25) };
            inputPanel.Controls.Add(lblCost);
            inputPanel.Controls.Add(txtUnitCost);
            y += 40;

            // Notes
            Label lblNote = new Label() { Text = "Notes:", Location = new Point(10, y), Size = new Size(100, 25) };
            txtNotes = new TextBox() { Location = new Point(110, y), Size = new Size(250, 25) };
            inputPanel.Controls.Add(lblNote);
            inputPanel.Controls.Add(txtNotes);
            y += 50;

            // Buttons
            btnSave = new Button() { Text = "PROCESS STOCK IN", Location = new Point(110, y), Size = new Size(150, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(270, y), Size = new Size(100, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnRefresh.Click += (s, e) => { LoadProducts(); LoadSuppliers(); LoadHistory(); };

            inputPanel.Controls.Add(btnSave);
            inputPanel.Controls.Add(btnRefresh);

            // History Panel
            Label lblHistory = new Label() { Text = "RECENT STOCK IN HISTORY", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(440, 60), Size = new Size(250, 30), ForeColor = Color.DarkBlue };

            dgvHistory = new DataGridView()
            {
                Location = new Point(440, 100),
                Size = new Size(430, 450),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };

            this.Controls.AddRange(new Control[] { lblTitle, inputPanel, lblHistory, dgvHistory });
        }

        private void LoadProducts()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetDataTable("SELECT ProductID, Name FROM Products ORDER BY Name");
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetDataTable("SELECT SupplierID, Name FROM Suppliers ORDER BY Name");
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "SupplierID";
                cmbSupplier.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductInfo()
        {
            if (cmbProduct.SelectedValue != null)
            {
                try
                {
                    SqlParameter[] parameters = { new SqlParameter("@id", cmbProduct.SelectedValue) };
                    DataTable dt = DatabaseHelper.GetDataTable("SELECT Quantity, UnitPrice FROM Products WHERE ProductID=@id", parameters);

                    if (dt.Rows.Count > 0)
                    {
                        lblCurrentStock.Text = dt.Rows[0]["Quantity"].ToString();
                        lblProductPrice.Text = Convert.ToDecimal(dt.Rows[0]["UnitPrice"]).ToString("N2");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading product info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadHistory()
        {
            try
            {
                string query = @"SELECT TOP 20 si.StockInID, p.Name as Product, si.Quantity, si.UnitCost, 
                                        (si.Quantity * si.UnitCost) as TotalCost, si.Date, s.Name as Supplier
                                 FROM StockIn si
                                 INNER JOIN Products p ON si.ProductID = p.ProductID
                                 INNER JOIN Suppliers s ON si.SupplierID = s.SupplierID
                                 ORDER BY si.StockInID DESC";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvHistory.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudQuantity.Value <= 0)
            {
                MessageBox.Show("Please enter valid quantity!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtUnitCost.Text, out decimal unitCost) || unitCost <= 0)
            {
                MessageBox.Show("Please enter valid unit cost!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(cmbProduct.SelectedValue);
            int quantity = (int)nudQuantity.Value;

            try
            {
                // Insert StockIn record
                string insertQuery = @"INSERT INTO StockIn (ProductID, Quantity, UnitCost, SupplierID, Notes) 
                                       VALUES (@pid, @qty, @cost, @sid, @notes)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@pid", productId),
                    new SqlParameter("@qty", quantity),
                    new SqlParameter("@cost", unitCost),
                    new SqlParameter("@sid", cmbSupplier.SelectedValue),
                    new SqlParameter("@notes", txtNotes.Text)
                };
                DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);

                // Update product quantity
                string updateQuery = "UPDATE Products SET Quantity = Quantity + @qty, LastUpdated = GETDATE() WHERE ProductID = @pid";
                SqlParameter[] updateParams = {
                    new SqlParameter("@qty", quantity),
                    new SqlParameter("@pid", productId)
                };
                DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);

                decimal totalCost = quantity * unitCost;
                MessageBox.Show($"Stock In Successful!\n\nProduct: {cmbProduct.Text}\nQuantity: {quantity}\nTotal Cost: ₱{totalCost:N2}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear and refresh
                nudQuantity.Value = 1;
                txtUnitCost.Clear();
                txtNotes.Clear();
                LoadProductInfo();
                LoadHistory();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing stock in: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}