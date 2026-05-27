using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class SupplierForm : Form
    {
        private DataGridView dgvSuppliers;
        private TextBox txtName, txtPhone, txtEmail, txtAddress;
        private Button btnSave, btnDelete, btnRefresh;
        private int selectedId = 0;

        public SupplierForm()
        {
            InitializeComponent();
            this.Load += (s, e) => LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier Management";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterParent;

            // Header
            Label lblTitle = new Label()
            {
                Text = "🏢 SUPPLIER MANAGEMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(300, 35)
            };

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 60), Size = new Size(400, 220), BackColor = Color.FromArgb(240, 240, 240) };
            int y = 20;
            AddField(inputPanel, "Name:", 10, ref y, out txtName);
            AddField(inputPanel, "Phone:", 10, ref y, out txtPhone);
            AddField(inputPanel, "Email:", 10, ref y, out txtEmail);
            AddField(inputPanel, "Address:", 10, ref y, out txtAddress);

            btnSave = new Button() { Text = "SAVE", Location = new Point(50, y + 10), Size = new Size(90, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White };
            btnDelete = new Button() { Text = "DELETE", Location = new Point(150, y + 10), Size = new Size(90, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(250, y + 10), Size = new Size(90, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadSuppliers(); ClearForm(); };
            inputPanel.Controls.AddRange(new Control[] { btnSave, btnDelete, btnRefresh });

            // DataGridView
            dgvSuppliers = new DataGridView()
            {
                Location = new Point(440, 60),
                Size = new Size(430, 420),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            ThemeManager.ConfigureDataGridView(dgvSuppliers);
            dgvSuppliers.SelectionChanged += DgvSuppliers_SelectionChanged;

            this.Controls.Add(lblTitle);
            this.Controls.Add(inputPanel);
            this.Controls.Add(dgvSuppliers);
        }

        private void AddField(Panel panel, string labelText, int x, ref int y, out TextBox textBox)
        {
            Label lbl = new Label() { Text = labelText, Location = new Point(x, y), Size = new Size(80, 25) };
            textBox = new TextBox() { Location = new Point(x + 80, y), Size = new Size(290, 25) };
            panel.Controls.Add(lbl);
            panel.Controls.Add(textBox);
            y += 35;
        }

        private void LoadSuppliers()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT SupplierID, Name, Phone, Email, Address FROM Suppliers ORDER BY Name");
            dgvSuppliers.DataSource = dt;
            if (dgvSuppliers.Columns["SupplierID"] != null)
                dgvSuppliers.Columns["SupplierID"].Visible = false;
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) return;
            var row = dgvSuppliers.SelectedRows[0];
            if (row.Cells["SupplierID"].Value == null) return;
            selectedId       = Convert.ToInt32(row.Cells["SupplierID"].Value);
            txtName.Text     = row.Cells["Name"].Value?.ToString()    ?? "";
            txtPhone.Text    = row.Cells["Phone"].Value?.ToString()   ?? "";
            txtEmail.Text    = row.Cells["Email"].Value?.ToString()   ?? "";
            txtAddress.Text  = row.Cells["Address"].Value?.ToString() ?? "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) { MessageBox.Show("Supplier name required!"); return; }
            try
            {
                if (selectedId == 0)
                {
                    string query = "INSERT INTO Suppliers (Name, Phone, Email, Address) VALUES (@name, @phone, @email, @address)";
                    SqlParameter[] p = { new SqlParameter("@name", txtName.Text), new SqlParameter("@phone", txtPhone.Text), new SqlParameter("@email", txtEmail.Text), new SqlParameter("@address", txtAddress.Text) };
                    DatabaseHelper.ExecuteNonQuery(query, p);
                    MessageBox.Show("Supplier added!");
                }
                else
                {
                    string query = "UPDATE Suppliers SET Name=@name, Phone=@phone, Email=@email, Address=@address WHERE SupplierID=@id";
                    SqlParameter[] p = { new SqlParameter("@name", txtName.Text), new SqlParameter("@phone", txtPhone.Text), new SqlParameter("@email", txtEmail.Text), new SqlParameter("@address", txtAddress.Text), new SqlParameter("@id", selectedId) };
                    DatabaseHelper.ExecuteNonQuery(query, p);
                    MessageBox.Show("Supplier updated!");
                }
                ClearForm();
                LoadSuppliers();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) { MessageBox.Show("Select a supplier to delete"); return; }
            if (MessageBox.Show("Delete this supplier?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE SupplierID=@id", new SqlParameter[] { new SqlParameter("@id", selectedId) });
                MessageBox.Show("Supplier deleted!");
                ClearForm();
                LoadSuppliers();
            }
        }

        private void ClearForm()
        {
            selectedId = 0;
            txtName.Clear(); txtPhone.Clear(); txtEmail.Clear(); txtAddress.Clear();
        }
    }
}