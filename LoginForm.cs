using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnSignup;
        private CheckBox chkShowPassword;
        private Label lblStatus;

        public LoginForm()
        {
            InitializeComponent();
            SetupModernUI();
            TestDatabaseConnection();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Management System";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
        }

        private void SetupModernUI()
        {
            // Header Panel with Gradient
            Panel headerPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(500, 120),
                Dock = DockStyle.Top
            };
            headerPanel.Paint += (sender, e) =>
            {
                Rectangle rect = headerPanel.ClientRectangle;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect,
                    Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185), 90f))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };

            // Logo/Icon
            Label lblIcon = new Label()
            {
                Text = "📦",
                Font = new Font("Segoe UI", 48),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(210, 20),
                Size = new Size(80, 70),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitle = new Label()
            {
                Text = "INVENTORY PRO",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(150, 80),
                Size = new Size(200, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblIcon);
            headerPanel.Controls.Add(lblTitle);

            // Welcome Text
            Label lblWelcome = new Label()
            {
                Text = "Welcome Back!",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 140),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitle = new Label()
            {
                Text = "Please enter your credentials to continue",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(50, 180),
                Size = new Size(400, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username Field
            Label lblUser = new Label()
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 230),
                Size = new Size(400, 20)
            };

            txtUsername = new TextBox()
            {
                Location = new Point(50, 255),
                Size = new Size(400, 35),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "admin",
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Password Field
            Label lblPass = new Label()
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 310),
                Size = new Size(400, 20)
            };

            txtPassword = new TextBox()
            {
                Location = new Point(50, 335),
                Size = new Size(400, 35),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '*',
                Text = "admin123",
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Show Password
            chkShowPassword = new CheckBox()
            {
                Text = "Show Password",
                Location = new Point(50, 375),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            chkShowPassword.CheckedChanged += (s, e) => {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
            };

            // Login Button
            btnLogin = new Button()
            {
                Text = "SIGN IN",
                Location = new Point(50, 415),
                Size = new Size(400, 45),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Hover Effects
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(52, 152, 219);

            // Signup Link
            Label lblNoAccount = new Label()
            {
                Text = "Don't have an account?",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(120, 475),
                Size = new Size(140, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            btnSignup = new Button()
            {
                Text = "CREATE ACCOUNT",
                Location = new Point(265, 473),
                Size = new Size(185, 30),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(52, 152, 219),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSignup.FlatAppearance.BorderSize = 0;
            btnSignup.Click += (s, e) => {
                SignupForm signup = new SignupForm();
                signup.ShowDialog();
                this.Hide();
            };

            // Status Label
            lblStatus = new Label()
            {
                Location = new Point(50, 510),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Demo Info Box
            Panel demoPanel = new Panel()
            {
                Location = new Point(50, 545),
                Size = new Size(400, 40),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblDemo = new Label()
            {
                Text = "📋 Demo: admin / admin123",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(10, 10),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            demoPanel.Controls.Add(lblDemo);

            // Add all controls to form
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblWelcome);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(chkShowPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(lblNoAccount);
            this.Controls.Add(btnSignup);
            this.Controls.Add(lblStatus);
            this.Controls.Add(demoPanel);
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    lblStatus.Text = "";
                    lblStatus.ForeColor = Color.Green;
                }
            }
            catch (Exception)
            {
                lblStatus.Text = "✗ Database Connection Failed!";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                lblStatus.Text = "⚠️ Please enter username and password";
                lblStatus.ForeColor = Color.Orange;
                return;
            }

            string query = "SELECT UserID, FullName, Role FROM Users WHERE Username = @user AND PasswordHash = @pass";

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Global.UserID = reader.GetInt32(0);
                                Global.FullName = reader.GetString(1);
                                Global.UserRole = reader.GetString(2);
                                Global.UserName = txtUsername.Text;

                                lblStatus.Text = "✓ Login Successful! Redirecting...";
                                lblStatus.ForeColor = Color.Green;

                                System.Threading.Thread.Sleep(500);

                                if (Global.UserRole == "Customer")
                                {
                                    CustomerDashboardForm customerForm = new CustomerDashboardForm();
                                    customerForm.Show();
                                }
                                else
                                {
                                    MainForm main = new MainForm();
                                    main.Show();
                                }
                                this.Hide();
                            }
                            else
                            {
                                lblStatus.Text = "✗ Invalid username or password!";
                                lblStatus.ForeColor = Color.Red;
                                txtPassword.Clear();
                                txtPassword.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "✗ Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}
