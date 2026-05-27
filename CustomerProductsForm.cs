using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class CustomerProductsForm : Form
    {
        private DataGridView dgvProducts;
        private NumericUpDown nudQuantity;
        private Button btnAddToCart, btnViewCart, btnCheckout, btnRefresh;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private DataTable cartTable;

        // Theme colors
        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);
        private static readonly Color DarkButtonColor = Color.FromArgb(44, 47, 58);
        private static readonly Color WhiteText = Color.FromArgb(255, 255, 255);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public CustomerProductsForm()
        {
            InitializeComponent();
            LoadProducts();
            LoadCategories();
            InitializeCart();
        }

        private void InitializeComponent()
        {
            this.Text = "Browse Products";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BackgroundColor;

            // Header
            Label lblTitle = new Label()
            {
                Text = "📦 PRODUCT CATALOG",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 35),
                ForeColor = WhiteText
            };

            // Search panel
            Panel searchPanel = new Panel()
            {
                Location = new Point(20, 70),
                Size = new Size(1060, 45),
                BackColor = Color.White
            };
            Label lblSearch = new Label() { Text = "Search:", Location = new Point(10, 12), Size = new Size(60, 25) };
            txtSearch = new TextBox() { Location = new Point(70, 10), Size = new Size(200, 25) };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            Label lblCategory = new Label() { Text = "Category:", Location = new Point(300, 12), Size = new Size(70, 25) };
            cmbCategory = new ComboBox() { Location = new Point(370, 10), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadProducts();

            btnRefresh = new Button()
            {
                Text = "🔄 Refresh",
                Location = new Point(550, 9),
                Size = new Size(90, 28),
                BackColor = DarkButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbCategory.SelectedIndex = -1; LoadProducts(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblCategory);
            searchPanel.Controls.Add(cmbCategory);
            searchPanel.Controls.Add(btnRefresh);

            // Products DataGridView
            dgvProducts = new DataGridView()
            {
                Location = new Point(20, 130),
                Size = new Size(1060, 400),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvProducts);

            // Quantity and Add to Cart
            Label lblQty = new Label() { Text = "Quantity:", Location = new Point(20, 550), Size = new Size(70, 30), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudQuantity = new NumericUpDown() { Location = new Point(90, 548), Size = new Size(80, 30), Minimum = 1, Maximum = 999, Value = 1 };

            btnAddToCart = new Button()
            {
                Text = "➕ Add to Cart",
                Location = new Point(190, 545),
                Size = new Size(130, 35),
                BackColor = GreenButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddToCart.Click += BtnAddToCart_Click;

            btnViewCart = new Button()
            {
                Text = "🛒 View Cart",
                Location = new Point(340, 545),
                Size = new Size(130, 35),
                BackColor = DarkButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnViewCart.Click += (s, e) => ViewCart();

            btnCheckout = new Button()
            {
                Text = "💳 Checkout",
                Location = new Point(490, 545),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.Click += (s, e) => Checkout();

            this.Controls.Add(lblTitle);
            this.Controls.Add(searchPanel);
            this.Controls.Add(dgvProducts);
            this.Controls.Add(lblQty);
            this.Controls.Add(nudQuantity);
            this.Controls.Add(btnAddToCart);
            this.Controls.Add(btnViewCart);
            this.Controls.Add(btnCheckout);
        }

        private void LoadCategories()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories UNION SELECT 0, 'All Categories' ORDER BY CategoryName");
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryID";
            cmbCategory.DataSource = dt;
            cmbCategory.SelectedValue = 0;
        }

        private void LoadProducts()
        {
            string query = @"SELECT p.ProductID, p.ProductCode, p.Name, c.CategoryName, p.Quantity, p.UnitPrice
                             FROM Products p
                             LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                             WHERE p.Quantity > 0";

            if (!string.IsNullOrEmpty(txtSearch.Text))
                query += " AND (p.Name LIKE @search OR p.ProductCode LIKE @search)";

            if (cmbCategory.SelectedValue != null && Convert.ToInt32(cmbCategory.SelectedValue) > 0)
                query += " AND p.CategoryID = @catId";

            query += " ORDER BY p.Name";

            var parameters = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrEmpty(txtSearch.Text))
                parameters.Add(new SqlParameter("@search", "%" + txtSearch.Text + "%"));
            if (cmbCategory.SelectedValue != null && Convert.ToInt32(cmbCategory.SelectedValue) > 0)
                parameters.Add(new SqlParameter("@catId", cmbCategory.SelectedValue));

            DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
            dgvProducts.DataSource = dt;
            if (dgvProducts.Columns["ProductID"] != null)
                dgvProducts.Columns["ProductID"].Visible = false;
        }

        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductID", typeof(int));
            cartTable.Columns.Add("ProductName", typeof(string));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("UnitPrice", typeof(decimal));
            cartTable.Columns.Add("Total", typeof(decimal));
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
            string productName = dgvProducts.SelectedRows[0].Cells["Name"].Value.ToString();
            int available = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["Quantity"].Value);
            int qty = (int)nudQuantity.Value;

            if (qty > available)
            {
                MessageBox.Show($"Only {available} units available.", "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal price = Convert.ToDecimal(dgvProducts.SelectedRows[0].Cells["UnitPrice"].Value);
            decimal total = qty * price;

            DataRow[] existing = cartTable.Select($"ProductID = {productId}");
            if (existing.Length > 0)
            {
                int newQty = Convert.ToInt32(existing[0]["Quantity"]) + qty;
                if (newQty > available)
                {
                    MessageBox.Show($"Cannot add. Only {available} units available in total.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existing[0]["Quantity"] = newQty;
                existing[0]["Total"] = newQty * price;
            }
            else
                cartTable.Rows.Add(productId, productName, qty, price, total);

            nudQuantity.Value = 1;
            MessageBox.Show($"Added {qty} x {productName} to cart.", "Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewCart()
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Your cart is empty.", "Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string cartSummary = "🛒 YOUR CART:\n\n";
            decimal grandTotal = 0;
            foreach (DataRow row in cartTable.Rows)
            {
                cartSummary += $"{row["ProductName"]} x {row["Quantity"]} = ₱{row["Total"]:N2}\n";
                grandTotal += Convert.ToDecimal(row["Total"]);
            }
            cartSummary += $"\nTotal: ₱{grandTotal:N2}\n\nDo you want to proceed to checkout?";
            if (MessageBox.Show(cartSummary, "Cart Summary", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Checkout();
        }

        private void Checkout()
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Your cart is empty. Add products first.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheckoutForm checkout = new CheckoutForm(cartTable);
            if (checkout.ShowDialog() == DialogResult.OK)
            {
                cartTable.Clear(); // Clear cart after successful order
                LoadProducts();    // Refresh product list (stock updated)
                MessageBox.Show("Thank you for your order!", "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}