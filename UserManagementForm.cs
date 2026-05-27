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
        private Label lblStatus;
        private int selectedUserId = 0;

        // Theme colors
        private static readonly Color BackgroundTeal = Color.FromArgb(134, 183, 181);
        private static readonly Color CardWhite = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        private static readonly Color SuccessGreen = Color.FromArgb(16, 145, 95);
        private static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        private static readonly Color DarkGray = Color.FromArgb(44, 47, 58);
        private static readonly Color WhiteText = Color.White;
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);
        private static readonly Color BorderLight = Color.FromArgb(220, 220, 230);

        public UserManagementForm()
        {
            InitializeForm();
            SetupUI();
            LoadUsers();
        }

        private void InitializeForm()
        {
            this.Text = "User Management";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BackgroundTeal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
        }

        private void SetupUI()
        {
            // Header Panel
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = PrimaryBlue
            };

            header.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(header.ClientRectangle,
                    PrimaryBlue, ControlPaint.Dark(PrimaryBlue, 0.1f), 90f))
                {
                    e.Graphics.FillRectangle(brush, header.ClientRectangle);
                }
            };

            Label titleLabel = new Label
            {
                Text = "USER MANAGEMENT",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = WhiteText,
                BackColor = Color.Transparent,
                Location = new Point(25, 18),
                AutoSize = true
            };

            header.Controls.Add(titleLabel);

            // Main content
            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = BackgroundTeal
            };

            // Split layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // Left Panel - User Details
            Panel leftPanel = CreateUserDetailsPanel();
            mainLayout.Controls.Add(leftPanel, 0, 0);

            // Right Panel - Users List
            Panel rightPanel = CreateUsersListPanel();
            mainLayout.Controls.Add(rightPanel, 1, 0);

            mainContent.Controls.Add(mainLayout);

            this.Controls.Add(mainContent);
            this.Controls.Add(header);
        }

        private Panel CreateUserDetailsPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(5)
            };

            panel.Paint += (sender, e) =>
            {
                GraphicsPath path = GetRoundedRectangle(panel.ClientRectangle, 10);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawPath(pen, path);
            };

            Label titleLabel = new Label
            {
                Text = "USER DETAILS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };

            int startY = 60;
            int labelWidth = 100;
            int controlWidth = 220;
            int leftMargin = 25;
            int rowHeight = 45;

            // Username
            Label lblUsername = CreateLabel("Username:", leftMargin, startY, labelWidth);
            txtUsername = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Full Name
            Label lblFullName = CreateLabel("Full Name:", leftMargin, startY + rowHeight, labelWidth);
            txtFullName = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + rowHeight),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            Label lblPassword = CreateLabel("Password:", leftMargin, startY + (rowHeight * 2), labelWidth);
            txtPassword = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 2)),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '*',
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPasswordHint = new Label
            {
                Text = "(Leave blank to keep current)",
                Location = new Point(leftMargin + labelWidth + 5, startY + (rowHeight * 2) + 32),
                Size = new Size(controlWidth, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            // Role
            Label lblRole = CreateLabel("Role:", leftMargin, startY + (rowHeight * 3), labelWidth);
            cmbRole = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, startY + (rowHeight * 3)),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbRole.Items.AddRange(new string[] { "Admin", "Cashier", "InventoryManager", "Customer" });
            cmbRole.SelectedIndex = 1;

            // Buttons
            int buttonY = startY + (rowHeight * 4) + 20;
            btnSave = CreateButton("SAVE", new Point(leftMargin, buttonY), new Size(100, 40), SuccessGreen);
            btnSave.Click += BtnSave_Click;

            btnDelete = CreateButton("DELETE", new Point(leftMargin + 115, buttonY), new Size(100, 40), DangerRed);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("REFRESH", new Point(leftMargin + 230, buttonY), new Size(100, 40), PrimaryBlue);
            btnRefresh.Click += (s, e) => { LoadUsers(); ClearForm(); };

            // Status label
            lblStatus = new Label
            {
                Location = new Point(leftMargin, buttonY + 55),
                Size = new Size(380, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Text = "Select a user to edit, or fill in the fields to add a new user."
            };

            panel.Controls.AddRange(new Control[] {
                titleLabel,
                lblUsername, txtUsername,
                lblFullName, txtFullName,
                lblPassword, txtPassword, lblPasswordHint,
                lblRole, cmbRole,
                btnSave, btnDelete, btnRefresh,
                lblStatus
            });

            return panel;
        }

        private Panel CreateUsersListPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(5)
            };

            panel.Paint += (sender, e) =>
            {
                GraphicsPath path = GetRoundedRectangle(panel.ClientRectangle, 10);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawPath(pen, path);
            };

            Label titleLabel = new Label
            {
                Text = "USERS LIST",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };

            // Create DataGridView
            dgvUsers = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(panel.Width - 35, panel.Height - 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9),
                MultiSelect = false
            };

            // Handle DataError to prevent exceptions
            dgvUsers.DataError += (s, e) =>
            {
                // Suppress the error - just continue
                e.ThrowException = false;
            };

            // Style headers
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = WhiteText;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.ColumnHeadersHeight = 40;
            dgvUsers.EnableHeadersVisualStyles = false;

            // Style rows
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgvUsers.DefaultCellStyle.SelectionBackColor = PrimaryBlue;
            dgvUsers.DefaultCellStyle.SelectionForeColor = WhiteText;
            dgvUsers.RowTemplate.Height = 35;

            // Add CellFormatting event to handle IsActive display
            dgvUsers.CellFormatting += DgvUsers_CellFormatting;
            dgvUsers.DataBindingComplete += (s, e) => ConfigureDataGridViewColumns();
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(dgvUsers);

            return panel;
        }

        private void ConfigureDataGridViewColumns()
        {
            if (dgvUsers.Columns.Count == 0) return;

            // Hide ID column
            if (dgvUsers.Columns.Contains("UserID"))
                dgvUsers.Columns["UserID"].Visible = false;

            // Configure Username column
            if (dgvUsers.Columns.Contains("Username"))
            {
                dgvUsers.Columns["Username"].HeaderText = "Username";
                dgvUsers.Columns["Username"].MinimumWidth = 100;
            }

            // Configure FullName column
            if (dgvUsers.Columns.Contains("FullName"))
            {
                dgvUsers.Columns["FullName"].HeaderText = "Full Name";
                dgvUsers.Columns["FullName"].MinimumWidth = 150;
            }

            // Configure Role column
            if (dgvUsers.Columns.Contains("Role"))
            {
                dgvUsers.Columns["Role"].HeaderText = "Role";
                dgvUsers.Columns["Role"].MinimumWidth = 120;
            }

            // Configure IsActive column
            if (dgvUsers.Columns.Contains("IsActive"))
            {
                dgvUsers.Columns["IsActive"].HeaderText = "Active";
                dgvUsers.Columns["IsActive"].MinimumWidth = 60;
                dgvUsers.Columns["IsActive"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                // Don't try to format here - do it in CellFormatting
            }

            // Configure CreatedDate column
            if (dgvUsers.Columns.Contains("CreatedDate"))
            {
                dgvUsers.Columns["CreatedDate"].HeaderText = "Created Date";
                dgvUsers.Columns["CreatedDate"].MinimumWidth = 130;
                dgvUsers.Columns["CreatedDate"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                dgvUsers.Columns["CreatedDate"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Set column display order
            int displayIndex = 0;
            if (dgvUsers.Columns.Contains("Username"))
                dgvUsers.Columns["Username"].DisplayIndex = displayIndex++;
            if (dgvUsers.Columns.Contains("FullName"))
                dgvUsers.Columns["FullName"].DisplayIndex = displayIndex++;
            if (dgvUsers.Columns.Contains("Role"))
                dgvUsers.Columns["Role"].DisplayIndex = displayIndex++;
            if (dgvUsers.Columns.Contains("IsActive"))
                dgvUsers.Columns["IsActive"].DisplayIndex = displayIndex++;
            if (dgvUsers.Columns.Contains("CreatedDate"))
                dgvUsers.Columns["CreatedDate"].DisplayIndex = displayIndex++;
        }

        private void DgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Handle IsActive column formatting
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                string columnName = dgvUsers.Columns[e.ColumnIndex].Name;

                if (columnName == "IsActive" && e.Value != null)
                {
                    try
                    {
                        bool isActive = false;
                        string valueStr = e.Value.ToString().ToLower();

                        // Handle different formats of boolean values
                        if (valueStr == "true" || valueStr == "1" || valueStr == "yes" || valueStr == "✓" || valueStr == "✔")
                            isActive = true;
                        else if (valueStr == "false" || valueStr == "0" || valueStr == "no" || valueStr == "☐" || valueStr == "✗")
                            isActive = false;
                        else
                            bool.TryParse(e.Value.ToString(), out isActive);

                        e.Value = isActive ? "✓" : "☐";
                        e.FormattingApplied = true;
                    }
                    catch
                    {
                        e.Value = "☐";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private void LoadUsers()
        {
            try
            {
                string query = "SELECT UserID, Username, FullName, Role, IsActive, CreatedDate FROM Users ORDER BY UserID";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                dgvUsers.DataSource = dt;
                ShowStatus($"Loaded {dt.Rows.Count} user(s).", PrimaryBlue);
            }
            catch (Exception ex)
            {
                ShowStatus("Error loading users: " + ex.Message, DangerRed);
                MessageBox.Show("Error loading users: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                selectedUserId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["UserID"].Value);
                txtUsername.Text = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();
                txtFullName.Text = dgvUsers.SelectedRows[0].Cells["FullName"].Value.ToString();
                string role = dgvUsers.SelectedRows[0].Cells["Role"].Value.ToString();

                int index = cmbRole.Items.IndexOf(role);
                if (index >= 0) cmbRole.SelectedIndex = index;

                txtPassword.Clear();
                ShowStatus($"Editing user: {txtUsername.Text}", PrimaryBlue);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtFullName.Text))
            {
                ShowStatus("Username and Full Name are required!", DangerRed);
                MessageBox.Show("Username and Full Name are required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbRole.SelectedItem == null)
            {
                ShowStatus("Please select a valid role!", DangerRed);
                MessageBox.Show("Please select a valid role!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedRole = cmbRole.SelectedItem.ToString();

            try
            {
                if (selectedUserId == 0) // New user
                {
                    if (string.IsNullOrEmpty(txtPassword.Text))
                    {
                        ShowStatus("Password is required for new users!", DangerRed);
                        MessageBox.Show("Password is required for new users!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string query = @"INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedDate) 
                                     VALUES (@user, @pass, @name, @role, 1, @date)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@user", txtUsername.Text.Trim()),
                        new SqlParameter("@pass", txtPassword.Text),
                        new SqlParameter("@name", txtFullName.Text.Trim()),
                        new SqlParameter("@role", selectedRole),
                        new SqlParameter("@date", DateTime.Now)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    ShowStatus("User added successfully!", SuccessGreen);
                    MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // Update existing user
                {
                    if (!string.IsNullOrEmpty(txtPassword.Text))
                    {
                        string query = @"UPDATE Users SET Username=@user, PasswordHash=@pass, FullName=@name, Role=@role 
                                         WHERE UserID=@id";
                        SqlParameter[] parameters = {
                            new SqlParameter("@user", txtUsername.Text.Trim()),
                            new SqlParameter("@pass", txtPassword.Text),
                            new SqlParameter("@name", txtFullName.Text.Trim()),
                            new SqlParameter("@role", selectedRole),
                            new SqlParameter("@id", selectedUserId)
                        };
                        DatabaseHelper.ExecuteNonQuery(query, parameters);
                    }
                    else
                    {
                        string query = @"UPDATE Users SET Username=@user, FullName=@name, Role=@role 
                                         WHERE UserID=@id";
                        SqlParameter[] parameters = {
                            new SqlParameter("@user", txtUsername.Text.Trim()),
                            new SqlParameter("@name", txtFullName.Text.Trim()),
                            new SqlParameter("@role", selectedRole),
                            new SqlParameter("@id", selectedUserId)
                        };
                        DatabaseHelper.ExecuteNonQuery(query, parameters);
                    }
                    ShowStatus("User updated successfully!", SuccessGreen);
                    MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadUsers();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Unique constraint violation
                {
                    ShowStatus("Username already exists! Please choose a different username.", DangerRed);
                    MessageBox.Show("Username already exists! Please choose a different username.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    ShowStatus("Error: " + ex.Message, DangerRed);
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowStatus("Error: " + ex.Message, DangerRed);
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0)
            {
                ShowStatus("Please select a user to delete.", DangerRed);
                MessageBox.Show("Please select a user to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Protect the main admin account
            if (selectedUserId == 1)
            {
                ShowStatus("Cannot delete the main administrator account.", DangerRed);
                MessageBox.Show("Cannot delete the main administrator account.", "Forbidden", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to permanently delete user '{txtUsername.Text}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserID=@id",
                        new SqlParameter[] { new SqlParameter("@id", selectedUserId) });
                    ShowStatus("User deleted successfully!", SuccessGreen);
                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    ShowStatus("Cannot delete: " + ex.Message, DangerRed);
                    MessageBox.Show("Cannot delete: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            selectedUserId = 0;
            txtUsername.Clear();
            txtFullName.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 1; // Default to Cashier
            ShowStatus("Ready to add a new user. Fill in the fields above.", Color.Gray);
        }

        private void ShowStatus(string message, Color color)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 5),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DarkText
            };
        }

        private Button CreateButton(string text, Point location, Size size, Color backColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(backColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}