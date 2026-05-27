using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class CustomerManagementForm : Form
    {
        private DataGridView dgvCustomers;
        private TextBox txtName, txtPhone, txtEmail, txtAddress;
        private Button btnSave, btnDelete, btnRefresh, btnSearch;
        private TextBox txtSearch;
        private int selectedCustomerId = 0;

        public CustomerManagementForm()
        {
            InitializeComponent();
            this.Load += (s, e) => LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Management";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 46);

            // Header
            Panel headerPanel = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(24, 24, 38) };
            Label lblTitle = new Label() { Text = "👥 CUSTOMER MANAGEMENT", Font = new Font("Consolas", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 255, 255), Location = new Point(20, 15), Size = new Size(400, 35) };
            headerPanel.Controls.Add(lblTitle);

            // Search Panel
            Panel searchPanel = new Panel() { Location = new Point(20, 80), Size = new Size(1060, 45), BackColor = Color.FromArgb(24, 24, 38) };
            Label lblSearch = new Label() { Text = "Search:", Location = new Point(10, 12), Size = new Size(60, 25), ForeColor = Color.White };
            txtSearch = new TextBox() { Location = new Point(70, 10), Size = new Size(200, 25), BackColor = Color.FromArgb(40, 40, 50), ForeColor = Color.White };
            btnSearch = new Button() { Text = "Search", Location = new Point(280, 9), Size = new Size(80, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (s, e) => SearchCustomers();
            Button btnShowAll = new Button() { Text = "Show All", Location = new Point(370, 9), Size = new Size(80, 28), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnShowAll.Click += (s, e) => { txtSearch.Clear(); LoadCustomers(); };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, btnShowAll });

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 140), Size = new Size(400, 250), BackColor = Color.FromArgb(24, 24, 38) };
            int y = 20;
            AddField(inputPanel, "Full Name:", 10, ref y, out txtName);
            AddField(inputPanel, "Phone:", 10, ref y, out txtPhone);
            AddField(inputPanel, "Email:", 10, ref y, out txtEmail);
            AddField(inputPanel, "Address:", 10, ref y, out txtAddress);

            btnSave = new Button() { Text = "💾 SAVE", Location = new Point(50, y + 10), Size = new Size(100, 35), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button() { Text = "🗑 DELETE", Location = new Point(160, y + 10), Size = new Size(100, 35), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "🔄 REFRESH", Location = new Point(270, y + 10), Size = new Size(100, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadCustomers(); ClearForm(); };

            inputPanel.Controls.AddRange(new Control[] { btnSave, btnDelete, btnRefresh });

            // DataGridView
            dgvCustomers = new DataGridView()
            {
                Location = new Point(440, 140),
                Size = new Size(640, 450),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            ThemeManager.ConfigureDataGridView(dgvCustomers);
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;

            this.Controls.Add(headerPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(inputPanel);
            this.Controls.Add(dgvCustomers);
        }

        private void AddField(Panel panel, string labelText, int x, ref int y, out TextBox textBox)
        {
            Label lbl = new Label() { Text = labelText, Location = new Point(x, y), Size = new Size(100, 25), ForeColor = Color.White };
            textBox = new TextBox() { Location = new Point(x + 110, y), Size = new Size(260, 25), BackColor = Color.FromArgb(40, 40, 50), ForeColor = Color.White };
            panel.Controls.Add(lbl);
            panel.Controls.Add(textBox);
            y += 35;
        }

        private void LoadCustomers()
        {
            string query = "SELECT CustomerID, FullName, Phone, Email, Address, CreatedDate FROM Customers ORDER BY FullName";
            DataTable dt = DatabaseHelper.GetDataTable(query);
            dgvCustomers.DataSource = dt;
            if (dgvCustomers.Columns["CustomerID"] != null)
                dgvCustomers.Columns["CustomerID"].Visible = false;
        }

        private void SearchCustomers()
        {
            string query = "SELECT CustomerID, FullName, Phone, Email, Address, CreatedDate FROM Customers WHERE FullName LIKE @search OR Phone LIKE @search ORDER BY FullName";
            SqlParameter[] parameters = { new SqlParameter("@search", "%" + txtSearch.Text + "%") };
            DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
            dgvCustomers.DataSource = dt;
        }

        private void DgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;
            var row = dgvCustomers.SelectedRows[0];
            if (row.Cells["CustomerID"].Value == null) return;
            selectedCustomerId = Convert.ToInt32(row.Cells["CustomerID"].Value);
            txtName.Text       = row.Cells["FullName"].Value?.ToString()  ?? "";
            txtPhone.Text      = row.Cells["Phone"].Value?.ToString()     ?? "";
            txtEmail.Text      = row.Cells["Email"].Value?.ToString()     ?? "";
            txtAddress.Text    = row.Cells["Address"].Value?.ToString()   ?? "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Customer name is required!");
                return;
            }

            try
            {
                if (selectedCustomerId == 0)
                {
                    string query = "INSERT INTO Customers (FullName, Phone, Email, Address) VALUES (@name, @phone, @email, @address)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@phone", txtPhone.Text),
                        new SqlParameter("@email", txtEmail.Text),
                        new SqlParameter("@address", txtAddress.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Customer added successfully!");
                }
                else
                {
                    string query = "UPDATE Customers SET FullName=@name, Phone=@phone, Email=@email, Address=@address WHERE CustomerID=@id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@phone", txtPhone.Text),
                        new SqlParameter("@email", txtEmail.Text),
                        new SqlParameter("@address", txtAddress.Text),
                        new SqlParameter("@id", selectedCustomerId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Customer updated successfully!");
                }
                ClearForm();
                LoadCustomers();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == 0) { MessageBox.Show("Select a customer to delete"); return; }
            if (MessageBox.Show("Delete this customer?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@id",
                    new SqlParameter[] { new SqlParameter("@id", selectedCustomerId) });
                MessageBox.Show("Customer deleted!");
                ClearForm();
                LoadCustomers();
            }
        }

        private void ClearForm()
        {
            selectedCustomerId = 0;
            txtName.Clear(); txtPhone.Clear(); txtEmail.Clear(); txtAddress.Clear();
        }
    }
}