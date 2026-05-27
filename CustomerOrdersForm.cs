using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class CustomerOrdersForm : Form
    {
        private DataGridView dgvOrders, dgvOrderItems;
        private ComboBox cmbStatusFilter;
        private DateTimePicker dtpStart, dtpEnd;
        private Button btnFilter, btnRefresh;
        private Label lblSummary;

        public CustomerOrdersForm()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.Text = "My Orders";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(134, 183, 181);

            Label lblTitle = new Label()
            {
                Text = "📋 MY ORDER HISTORY",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(400, 35),
                ForeColor = Color.White
            };

            Panel filterPanel = new Panel()
            {
                Location = new Point(20, 70),
                Size = new Size(1060, 45),
                BackColor = Color.White
            };
            Label lblStatus = new Label() { Text = "Status:", Location = new Point(10, 12), Size = new Size(60, 25) };
            cmbStatusFilter = new ComboBox() { Location = new Point(70, 10), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new string[] { "All", "Paid", "Pending", "Shipped", "Delivered" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadOrders();

            Label lblFrom = new Label() { Text = "From:", Location = new Point(210, 12), Size = new Size(40, 25) };
            dtpStart = new DateTimePicker() { Location = new Point(250, 10), Size = new Size(130, 25), Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Now.AddMonths(-3);
            Label lblTo = new Label() { Text = "To:", Location = new Point(390, 12), Size = new Size(30, 25) };
            dtpEnd = new DateTimePicker() { Location = new Point(420, 10), Size = new Size(130, 25), Format = DateTimePickerFormat.Short };
            dtpEnd.Value = DateTime.Now;

            btnFilter = new Button() { Text = "Filter", Location = new Point(570, 9), Size = new Size(80, 28), BackColor = System.Drawing.Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnFilter.Click += (s, e) => LoadOrders();
            btnRefresh = new Button() { Text = "Refresh", Location = new Point(660, 9), Size = new Size(80, 28), BackColor = System.Drawing.Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => { cmbStatusFilter.SelectedIndex = 0; dtpStart.Value = DateTime.Now.AddMonths(-3); dtpEnd.Value = DateTime.Now; LoadOrders(); };

            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cmbStatusFilter);
            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(dtpStart);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(dtpEnd);
            filterPanel.Controls.Add(btnFilter);
            filterPanel.Controls.Add(btnRefresh);

            dgvOrders = new DataGridView()
            {
                Location = new Point(20, 130),
                Size = new Size(1060, 250),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvOrders);
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

            Label lblItemsTitle = new Label()
            {
                Text = "📦 ORDER ITEMS",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 400),
                Size = new Size(200, 25),
                ForeColor = Color.White
            };
            dgvOrderItems = new DataGridView()
            {
                Location = new Point(20, 430),
                Size = new Size(1060, 200),
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true
            };
            ThemeManager.ConfigureDataGridView(dgvOrderItems);

            lblSummary = new Label()
            {
                Location = new Point(20, 645),
                Size = new Size(1060, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(filterPanel);
            this.Controls.Add(dgvOrders);
            this.Controls.Add(lblItemsTitle);
            this.Controls.Add(dgvOrderItems);
            this.Controls.Add(lblSummary);
        }

        private int GetCustomerId()
        {
            // Find the customer record linked to the logged-in user
            string query = "SELECT CustomerID FROM Customers WHERE FullName = @name OR Email = @email";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", Global.FullName);
                cmd.Parameters.AddWithValue("@email", Global.UserName + "@customer.local");
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
                else
                    throw new Exception("Customer record not found. Please ensure you have placed at least one order.");
            }
        }

        private void LoadOrders()
        {
            try
            {
                int customerId = GetCustomerId();

                string query = @"SELECT SaleID, InvoiceNumber, SaleDate, TotalAmount, PaymentMethod, PaymentStatus
                                 FROM Sales
                                 WHERE CustomerID = @custId AND SaleDate BETWEEN @start AND @end";

                if (cmbStatusFilter.SelectedIndex > 0)
                    query += " AND PaymentStatus = @status";

                query += " ORDER BY SaleDate DESC";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                parameters.Add(new SqlParameter("@custId", customerId));
                parameters.Add(new SqlParameter("@start", dtpStart.Value.Date));
                parameters.Add(new SqlParameter("@end", dtpEnd.Value.Date.AddDays(1).AddSeconds(-1)));
                if (cmbStatusFilter.SelectedIndex > 0)
                    parameters.Add(new SqlParameter("@status", cmbStatusFilter.SelectedItem.ToString()));

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
                dgvOrders.DataSource = dt;

                decimal totalSpent = 0;
                foreach (DataRow row in dt.Rows)
                    totalSpent += Convert.ToDecimal(row["TotalAmount"]);
                lblSummary.Text = $"Total Orders: {dt.Rows.Count} | Total Spent: ₱{totalSpent:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading orders: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            var cell = dgvOrders.SelectedRows[0].Cells["SaleID"];
            if (cell?.Value == null) return;
            int saleId = Convert.ToInt32(cell.Value);
            LoadOrderItems(saleId);
        }

        private void LoadOrderItems(int saleId)
        {
            try
            {
                string query = @"SELECT p.Name AS Product, si.Quantity, si.UnitPrice, si.TotalPrice
                                 FROM SaleItems si
                                 INNER JOIN Products p ON si.ProductID = p.ProductID
                                 WHERE si.SaleID = @saleId";
                SqlParameter[] p = { new SqlParameter("@saleId", saleId) };
                DataTable dt = DatabaseHelper.GetDataTable(query, p);
                dgvOrderItems.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading order items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}