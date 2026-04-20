using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Suppliers";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "SUPPLIER MANAGEMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(350, 35),
                ForeColor = Color.DarkBlue
            };

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 60), Size = new Size(400, 220), BackColor = Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };

            Label lblName = new Label() { Text = "Name:", Location = new Point(10, 25), Size = new Size(80, 25) };
            txtName = new TextBox() { Location = new Point(90, 23), Size = new Size(280, 25) };

            Label lblPhone = new Label() { Text = "Phone:", Location = new Point(10, 65), Size = new Size(80, 25) };
            txtPhone = new TextBox() { Location = new Point(90, 63), Size = new Size(200, 25) };

            Label lblEmail = new Label() { Text = "Email:", Location = new Point(10, 105), Size = new Size(80, 25) };
            txtEmail = new TextBox() { Location = new Point(90, 103), Size = new Size(280, 25) };

            Label lblAddress = new Label() { Text = "Address:", Location = new Point(10, 145), Size = new Size(80, 25) };
            txtAddress = new TextBox() { Location = new Point(90, 143), Size = new Size(280, 25) };

            btnSave = new Button() { Text = "SAVE", Location = new Point(50, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button() { Text = "DELETE", Location = new Point(150, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(250, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadSuppliers(); ClearForm(); };

            inputPanel.Controls.AddRange(new Control[] { lblName, txtName, lblPhone, txtPhone, lblEmail, txtEmail, lblAddress, txtAddress, btnSave, btnDelete, btnRefresh });

            // DataGridView
            dgvSuppliers = new DataGridView()
            {
                Location = new Point(440, 60),
                Size = new Size(430, 490),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvSuppliers.SelectionChanged += DgvSuppliers_SelectionChanged;

            this.Controls.AddRange(new Control[] { lblTitle, inputPanel, dgvSuppliers });
        }

        private void LoadSuppliers()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT SupplierID, Name, Phone, Email, Address FROM Suppliers ORDER BY Name");
            dgvSuppliers.DataSource = dt;
            dgvSuppliers.Columns["SupplierID"].Visible = false;
        }

        private void DgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count > 0)
            {
                selectedId = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["SupplierID"].Value);
                txtName.Text = dgvSuppliers.SelectedRows[0].Cells["Name"].Value.ToString();
                txtPhone.Text = dgvSuppliers.SelectedRows[0].Cells["Phone"].Value.ToString();
                txtEmail.Text = dgvSuppliers.SelectedRows[0].Cells["Email"].Value.ToString();
                txtAddress.Text = dgvSuppliers.SelectedRows[0].Cells["Address"].Value.ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Supplier name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (selectedId == 0)
                {
                    string query = "INSERT INTO Suppliers (Name, Phone, Email, Address) VALUES (@name, @phone, @email, @address)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@phone", txtPhone.Text),
                        new SqlParameter("@email", txtEmail.Text),
                        new SqlParameter("@address", txtAddress.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Supplier added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string query = "UPDATE Suppliers SET Name=@name, Phone=@phone, Email=@email, Address=@address WHERE SupplierID=@id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@phone", txtPhone.Text),
                        new SqlParameter("@email", txtEmail.Text),
                        new SqlParameter("@address", txtAddress.Text),
                        new SqlParameter("@id", selectedId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Supplier updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Select a supplier to delete", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Delete this supplier?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE SupplierID=@id",
                        new SqlParameter[] { new SqlParameter("@id", selectedId) });
                    MessageBox.Show("Supplier deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadSuppliers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            selectedId = 0;
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
        }
    }
}