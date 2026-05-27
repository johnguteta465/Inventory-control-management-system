using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class SignupForm : Form
    {
        private TextBox txtUsername, txtFullName, txtPassword, txtConfirmPassword;
        private Button btnSignup, btnLogin, btnClear;
        private Label lblStatus;
        private CheckBox chkShowPassword;

        public SignupForm()
        {
            InitializeComponent();
            SetupModernUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Create New Account";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
        }

        private void SetupModernUI()
        {
            // Header Panel
            Panel headerPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(500, 100),
                BackColor = Color.FromArgb(52, 152, 219)
            };

            Label lblIcon = new Label()
            {
                Text = "📦",
                Font = new Font("Segoe UI", 40),
                ForeColor = Color.White,
                Location = new Point(220, 20),
                Size = new Size(60, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitle = new Label()
            {
                Text = "INVENTORY PRO",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(150, 65),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblIcon);
            headerPanel.Controls.Add(lblTitle);

            // Welcome Text
            Label lblWelcome = new Label()
            {
                Text = "Create Customer Account",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 120),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitle = new Label()
            {
                Text = "Fill in your details to get started",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(50, 160),
                Size = new Size(400, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int y = 210;
            int startX = 50;
            int fieldWidth = 400;

            // Username (with character counter)
            AddFieldWithCounter("Username (max 8 chars)", startX, ref y, fieldWidth, out txtUsername);
            y += 50;

            // Full Name (with character counter)
            AddFieldWithCounter("Full Name (max 8 chars)", startX, ref y, fieldWidth, out txtFullName);
            y += 50;

            // Password (with character counter)
            AddFieldWithCounter("Password (max 8 chars)", startX, ref y, fieldWidth, out txtPassword);
            txtPassword.PasswordChar = '*';
            y += 50;

            // Confirm Password
            AddField("Confirm Password", startX, ref y, fieldWidth, out txtConfirmPassword);
            txtConfirmPassword.PasswordChar = '*';
            y += 40;

            // Show Password
            chkShowPassword = new CheckBox()
            {
                Text = "Show Password",
                Location = new Point(startX + 120, y),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9)
            };
            chkShowPassword.CheckedChanged += (s, e) => {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
                txtConfirmPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
            };
            y += 40;

            // Status
            lblStatus = new Label()
            {
                Location = new Point(startX, y),
                Size = new Size(fieldWidth, 30),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };
            y += 45;

            // Buttons
            btnSignup = new Button()
            {
                Text = "CREATE ACCOUNT",
                Location = new Point(startX, y),
                Size = new Size(190, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSignup.FlatAppearance.BorderSize = 0;
            btnSignup.Click += BtnSignup_Click;
            btnSignup.MouseEnter += (s, e) => btnSignup.BackColor = Color.FromArgb(39, 174, 96);
            btnSignup.MouseLeave += (s, e) => btnSignup.BackColor = Color.FromArgb(46, 204, 113);

            btnClear = new Button()
            {
                Text = "CLEAR",
                Location = new Point(startX + 210, y),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearForm();
            y += 60;

            // Login Link
            Label lblHaveAccount = new Label()
            {
                Text = "Already have an account?",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(startX + 80, y),
                Size = new Size(150, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            btnLogin = new Button()
            {
                Text = "SIGN IN",
                Location = new Point(startX + 240, y - 3),
                Size = new Size(100, 28),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(52, 152, 219),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => {
                LoginForm login = new LoginForm();
                login.Show();
                this.Hide();
            };

            // Add all controls
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblWelcome);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnSignup);
            this.Controls.Add(btnClear);
            this.Controls.Add(lblHaveAccount);
            this.Controls.Add(btnLogin);
            this.Controls.Add(chkShowPassword);
        }

        private void AddField(string labelText, int x, ref int y, int width, out TextBox textBox)
        {
            Label lbl = new Label()
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(x, y),
                Size = new Size(100, 30)
            };

            textBox = new TextBox()
            {
                Location = new Point(x + 100, y),
                Size = new Size(width - 100, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            this.Controls.Add(lbl);
            this.Controls.Add(textBox);
        }

        private void AddFieldWithCounter(string labelText, int x, ref int y, int width, out TextBox textBox)
        {
            Label lbl = new Label()
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(x, y),
                Size = new Size(130, 30)
            };

            textBox = new TextBox()
            {
                Location = new Point(x + 130, y),
                Size = new Size(width - 170, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                MaxLength = 8  // Limit to 8 characters
            };

            Label charCounter = new Label()
            {
                Text = "0/8",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(x + width - 35, y + 8),
                Size = new Size(35, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBox.TextChanged += (s, e) => {
                charCounter.Text = textBox.Text.Length + "/8";
                charCounter.ForeColor = textBox.Text.Length == 8 ? Color.Red : Color.Gray;
            };

            this.Controls.Add(lbl);
            this.Controls.Add(textBox);
            this.Controls.Add(charCounter);
        }

        private void BtnSignup_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            if (IsUsernameExists(txtUsername.Text.Trim()))
            {
                lblStatus.Text = "❌ Username already exists!";
                lblStatus.ForeColor = Color.Red;
                txtUsername.Focus();
                return;
            }

            // Role is forced to "Customer"
            if (CreateUser("Customer"))
            {
                MessageBox.Show("✅ Customer account created successfully!\n\nUsername: " + txtUsername.Text.Trim(),
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoginForm login = new LoginForm();
                login.Show();
                this.Hide();
            }
        }

        private bool ValidateInputs()
        {
            // Username validation
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            { lblStatus.Text = "❌ Username required!"; txtUsername.Focus(); return false; }
            if (txtUsername.Text.Length > 8)
            { lblStatus.Text = "❌ Username cannot exceed 8 characters!"; txtUsername.Focus(); return false; }
            if (txtUsername.Text.Length < 3)
            { lblStatus.Text = "❌ Username min 3 characters!"; txtUsername.Focus(); return false; }

            // Full Name validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            { lblStatus.Text = "❌ Full Name required!"; txtFullName.Focus(); return false; }
            if (txtFullName.Text.Length > 8)
            { lblStatus.Text = "❌ Full Name cannot exceed 8 characters!"; txtFullName.Focus(); return false; }

            // Password validation
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            { lblStatus.Text = "❌ Password required!"; txtPassword.Focus(); return false; }
            if (txtPassword.Text.Length > 8)
            { lblStatus.Text = "❌ Password cannot exceed 8 characters!"; txtPassword.Focus(); return false; }
            if (txtPassword.Text.Length < 6)
            { lblStatus.Text = "❌ Password min 6 characters!"; txtPassword.Focus(); return false; }
            if (!Regex.IsMatch(txtPassword.Text, "[0-9]"))
            { lblStatus.Text = "❌ Password needs a number!"; txtPassword.Focus(); return false; }
            if (!Regex.IsMatch(txtPassword.Text, "[a-zA-Z]"))
            { lblStatus.Text = "❌ Password needs a letter!"; txtPassword.Focus(); return false; }
            if (txtPassword.Text != txtConfirmPassword.Text)
            { lblStatus.Text = "❌ Passwords don't match!"; txtConfirmPassword.Focus(); return false; }

            lblStatus.Text = "";
            return true;
        }

        private bool IsUsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool CreateUser(string role)
        {
            try
            {
                string query = @"INSERT INTO Users (Username, PasswordHash, FullName, Role, CreatedDate) 
                                 VALUES (@username, @password, @fullname, @role, GETDATE())";

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                    cmd.Parameters.AddWithValue("@fullname", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@role", role);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtFullName.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            chkShowPassword.Checked = false;
            lblStatus.Text = "";
            txtUsername.Focus();
        }
    }
}