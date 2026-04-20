using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class UserManagementForm : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtUsername, txtFullName, txtPassword;
        private ComboBox cmbRole;
        private Button btnSave, btnDelete, btnRefresh;
        private int selectedUserId = 0;

        public UserManagementForm()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Users (Admin Only)";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "USER MANAGEMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(300, 35),
                ForeColor = Color.DarkBlue
            };

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 60), Size = new Size(400, 220), BackColor = Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };

            Label lblUser = new Label() { Text = "Username:", Location = new Point(10, 25), Size = new Size(80, 25) };
            txtUsername = new TextBox() { Location = new Point(90, 23), Size = new Size(200, 25) };

            Label lblFull = new Label() { Text = "Full Name:", Location = new Point(10, 65), Size = new Size(80, 25) };
            txtFullName = new TextBox() { Location = new Point(90, 63), Size = new Size(250, 25) };

            Label lblPass = new Label() { Text = "Password:", Location = new Point(10, 105), Size = new Size(80, 25) };
            txtPassword = new TextBox() { Location = new Point(90, 103), Size = new Size(200, 25), PasswordChar = '*' };

            Label lblRole = new Label() { Text = "Role:", Location = new Point(10, 145), Size = new Size(80, 25) };
            cmbRole = new ComboBox() { Location = new Point(90, 143), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Staff", "Admin" });
            cmbRole.SelectedIndex = 0;

            btnSave = new Button() { Text = "SAVE", Location = new Point(50, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button() { Text = "DELETE", Location = new Point(150, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(250, 185), Size = new Size(90, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadUsers(); ClearForm(); };

            inputPanel.Controls.AddRange(new Control[] { lblUser, txtUsername, lblFull, txtFullName, lblPass, txtPassword, lblRole, cmbRole, btnSave, btnDelete, btnRefresh });

            // DataGridView
            dgvUsers = new DataGridView()
            {
                Location = new Point(440, 60),
                Size = new Size(430, 490),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            this.Controls.AddRange(new Control[] { lblTitle, inputPanel, dgvUsers });
        }

        private void LoadUsers()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT UserID, Username, FullName, Role, CreatedDate FROM Users ORDER BY UserID");
            dgvUsers.DataSource = dt;
            dgvUsers.Columns["UserID"].Visible = false;
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                selectedUserId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["UserID"].Value);
                txtUsername.Text = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();
                txtFullName.Text = dgvUsers.SelectedRows[0].Cells["FullName"].Value.ToString();
                cmbRole.Text = dgvUsers.SelectedRows[0].Cells["Role"].Value.ToString();
                txtPassword.Clear();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtFullName.Text))
            {
                MessageBox.Show("Username and Full Name are required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (selectedUserId == 0)
                {
                    if (string.IsNullOrEmpty(txtPassword.Text))
                    {
                        MessageBox.Show("Password is required for new users!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string query = "INSERT INTO Users (Username, PasswordHash, FullName, Role) VALUES (@user, @pass, @name, @role)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@user", txtUsername.Text),
                        new SqlParameter("@pass", txtPassword.Text),
                        new SqlParameter("@name", txtFullName.Text),
                        new SqlParameter("@role", cmbRole.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("User added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (!string.IsNullOrEmpty(txtPassword.Text))
                    {
                        string query = "UPDATE Users SET Username=@user, PasswordHash=@pass, FullName=@name, Role=@role WHERE UserID=@id";
                        SqlParameter[] parameters = {
                            new SqlParameter("@user", txtUsername.Text),
                            new SqlParameter("@pass", txtPassword.Text),
                            new SqlParameter("@name", txtFullName.Text),
                            new SqlParameter("@role", cmbRole.Text),
                            new SqlParameter("@id", selectedUserId)
                        };
                        DatabaseHelper.ExecuteNonQuery(query, parameters);
                    }
                    else
                    {
                        string query = "UPDATE Users SET Username=@user, FullName=@name, Role=@role WHERE UserID=@id";
                        SqlParameter[] parameters = {
                            new SqlParameter("@user", txtUsername.Text),
                            new SqlParameter("@name", txtFullName.Text),
                            new SqlParameter("@role", cmbRole.Text),
                            new SqlParameter("@id", selectedUserId)
                        };
                        DatabaseHelper.ExecuteNonQuery(query, parameters);
                    }
                    MessageBox.Show("User updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0)
            {
                MessageBox.Show("Select a user to delete", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedUserId == 1)
            {
                MessageBox.Show("Cannot delete the main admin user!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Delete this user?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserID=@id",
                    new SqlParameter[] { new SqlParameter("@id", selectedUserId) });
                MessageBox.Show("User deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsers();
            }
        }

        private void ClearForm()
        {
            selectedUserId = 0;
            txtUsername.Clear();
            txtFullName.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 0;
        }
    }
}