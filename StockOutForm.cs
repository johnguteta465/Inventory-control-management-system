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
        private ComboBox cmbProduct;
        private NumericUpDown nudQuantity;
        private TextBox txtUnitPrice, txtCustomer, txtNotes;
        private Button btnSave, btnRefresh;
        private Label lblCurrentStock, lblTotalAmount;
        private DataGridView dgvHistory;

        public StockOutForm()
        {
            InitializeComponent();
            LoadProducts();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Out (Sales)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "STOCK OUT - SALES",
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

            // Current Stock
            Label lblStock = new Label() { Text = "Current Stock:", Location = new Point(10, y), Size = new Size(100, 25) };
            lblCurrentStock = new Label() { Text = "0", Location = new Point(110, y), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Blue };
            inputPanel.Controls.Add(lblStock);
            inputPanel.Controls.Add(lblCurrentStock);
            y += 40;

            // Quantity
            Label lblQty = new Label() { Text = "Quantity:", Location = new Point(10, y), Size = new Size(100, 25) };
            nudQuantity = new NumericUpDown() { Location = new Point(110, y), Size = new Size(150, 25), Minimum = 1, Maximum = 100000, Value = 1 };
            nudQuantity.ValueChanged += (s, e) => CalculateTotal();
            inputPanel.Controls.Add(lblQty);
            inputPanel.Controls.Add(nudQuantity);
            y += 40;

            // Selling Price
            Label lblPrice = new Label() { Text = "Selling Price:", Location = new Point(10, y), Size = new Size(100, 25) };
            txtUnitPrice = new TextBox() { Location = new Point(110, y), Size = new Size(150, 25) };
            txtUnitPrice.TextChanged += (s, e) => CalculateTotal();
            inputPanel.Controls.Add(lblPrice);
            inputPanel.Controls.Add(txtUnitPrice);
            y += 40;

            // Total Amount
            Label lblTotal = new Label() { Text = "Total Amount:", Location = new Point(10, y), Size = new Size(100, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblTotalAmount = new Label() { Text = "₱0.00", Location = new Point(110, y), Size = new Size(150, 25), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Green };
            inputPanel.Controls.Add(lblTotal);
            inputPanel.Controls.Add(lblTotalAmount);
            y += 40;

            // Customer Name
            Label lblCustomer = new Label() { Text = "Customer:", Location = new Point(10, y), Size = new Size(100, 25) };
            txtCustomer = new TextBox() { Location = new Point(110, y), Size = new Size(250, 25) };
            inputPanel.Controls.Add(lblCustomer);
            inputPanel.Controls.Add(txtCustomer);
            y += 40;

            // Notes
            Label lblNote = new Label() { Text = "Notes:", Location = new Point(10, y), Size = new Size(100, 25) };
            txtNotes = new TextBox() { Location = new Point(110, y), Size = new Size(250, 25) };
            inputPanel.Controls.Add(lblNote);
            inputPanel.Controls.Add(txtNotes);
            y += 50;

            // Buttons
            btnSave = new Button() { Text = "PROCESS SALE", Location = new Point(110, y), Size = new Size(150, 40), BackColor = Color.FromArgb(241, 196, 15), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(270, y), Size = new Size(100, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnRefresh.Click += (s, e) => { LoadProducts(); LoadHistory(); };

            inputPanel.Controls.Add(btnSave);
            inputPanel.Controls.Add(btnRefresh);

            // History Panel
            Label lblHistory = new Label() { Text = "RECENT SALES HISTORY", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(440, 60), Size = new Size(250, 30), ForeColor = Color.DarkBlue };

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
                string query = "SELECT ProductID, Name, UnitPrice FROM Products WHERE Quantity > 0 ORDER BY Name";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductInfo()
        {
            if (cmbProduct.SelectedValue != null)
            {
                try
                {
                    string query = "SELECT Quantity, UnitPrice FROM Products WHERE ProductID=@id";
                    SqlParameter[] parameters = { new SqlParameter("@id", cmbProduct.SelectedValue) };
                    DataTable dt = DatabaseHelper.GetDataTable(query, parameters);

                    if (dt.Rows.Count > 0)
                    {
                        int stock = Convert.ToInt32(dt.Rows[0]["Quantity"]);
                        lblCurrentStock.Text = stock.ToString();
                        nudQuantity.Maximum = stock;
                        txtUnitPrice.Text = dt.Rows[0]["UnitPrice"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading product info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CalculateTotal()
        {
            if (decimal.TryParse(txtUnitPrice.Text, out decimal price) && nudQuantity.Value > 0)
            {
                decimal total = price * nudQuantity.Value;
                lblTotalAmount.Text = "₱" + total.ToString("N2");
            }
            else
            {
                lblTotalAmount.Text = "₱0.00";
            }
        }

        private void LoadHistory()
        {
            try
            {
                string query = @"SELECT TOP 20 so.StockOutID, p.Name as Product, so.Quantity, so.UnitPrice, 
                                        (so.Quantity * so.UnitPrice) as TotalAmount, so.Date, so.CustomerName
                                 FROM StockOut so
                                 INNER JOIN Products p ON so.ProductID = p.ProductID
                                 ORDER BY so.StockOutID DESC";
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

            int productId = Convert.ToInt32(cmbProduct.SelectedValue);
            int quantity = (int)nudQuantity.Value;
            int currentStock = Convert.ToInt32(lblCurrentStock.Text);

            if (quantity > currentStock)
            {
                MessageBox.Show($"Insufficient stock! Available: {currentStock}", "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Please enter valid selling price!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Insert StockOut record
                string insertQuery = @"INSERT INTO StockOut (ProductID, Quantity, UnitPrice, CustomerName, Notes) 
                                       VALUES (@pid, @qty, @price, @customer, @notes)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@pid", productId),
                    new SqlParameter("@qty", quantity),
                    new SqlParameter("@price", unitPrice),
                    new SqlParameter("@customer", txtCustomer.Text),
                    new SqlParameter("@notes", txtNotes.Text)
                };
                DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);

                // Update product quantity
                string updateQuery = "UPDATE Products SET Quantity = Quantity - @qty, LastUpdated = GETDATE() WHERE ProductID = @pid";
                SqlParameter[] updateParams = {
                    new SqlParameter("@qty", quantity),
                    new SqlParameter("@pid", productId)
                };
                DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);

                decimal totalAmount = quantity * unitPrice;
                MessageBox.Show($"Sale Successful!\n\nProduct: {cmbProduct.Text}\nQuantity: {quantity}\nTotal Amount: ₱{totalAmount:N2}\nCustomer: {txtCustomer.Text}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear and refresh
                nudQuantity.Value = 1;
                txtCustomer.Clear();
                txtNotes.Clear();
                LoadProducts();
                LoadProductInfo();
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing sale: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}