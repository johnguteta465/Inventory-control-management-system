using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class MainForm : Form
    {
        private Panel sidePanel;
        private Panel contentPanel;
        private Label lblWelcome;
        private Timer clockTimer;
        private Label lblClock;
        private FlowLayoutPanel cardPanel;
        private Panel headerPanel;
        private NotificationManager notificationManager;

        // Background color - Teal (#86B7B5)
        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);

        // Button colors
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);      // #10915F
        private static readonly Color DarkButtonColor = Color.FromArgb(44, 47, 58);        // #2C2F3A
        private static readonly Color PinkButtonColor = Color.FromArgb(230, 168, 168);     // #E6A8A8

        // Card colors
        private static readonly Color CardNumberColor = Color.FromArgb(44, 95, 93);         // #2C5F5D - Dark Teal
        private static readonly Color CardLabelColor = Color.FromArgb(74, 85, 104);         // #4A5568 - Dark Slate
        private static readonly Color ActiveNumberColor = Color.FromArgb(16, 145, 95);      // #10915F - Green
        private static readonly Color ActiveTextColor = Color.FromArgb(16, 145, 95);        // #10915F - Green
        private static readonly Color StatusLabelColor = Color.FromArgb(102, 102, 102);     // #666666 - Medium Gray

        // Text colors
        private static readonly Color WhiteText = Color.FromArgb(255, 255, 255);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        // Additional UI colors
        private static readonly Color CardBackground = Color.FromArgb(255, 255, 255);       // White background for cards
        private static readonly Color TopBarColor = DarkButtonColor;

        public MainForm()
        {
            InitializeComponents();
            LoadDashboard();
            StartClock();
            notificationManager = new NotificationManager(this);
        }

        private void InitializeComponents()
        {
            this.Text = "Inventory Management System - Dashboard";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Set background color
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.None;

            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0xA1, 0x2, 0); } };

            // ========== TOP BAR ==========
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = TopBarColor
            };

            // Close Button
            Button btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(this.Width - 50, 5),
                BackColor = Color.Transparent,
                ForeColor = WhiteText,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(255, 100, 100);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = WhiteText;

            // Minimize Button
            Button btnMinimize = new Button
            {
                Text = "─",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(this.Width - 95, 5),
                BackColor = Color.Transparent,
                ForeColor = WhiteText,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // App Title
            Label lblAppTitle = new Label
            {
                Text = "📦 INVENTORY PRO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = WhiteText,
                Location = new Point(20, 12),
                Size = new Size(200, 30)
            };

            // Notification Bell
            Button btnNotifications = new Button
            {
                Text = "🔔",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(this.Width - 150, 5),
                BackColor = Color.Transparent,
                ForeColor = WhiteText,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.Click += (s, e) => new LowStockReportForm().ShowDialog();

            // Charts Button
            Button btnCharts = new Button
            {
                Text = "📈",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(this.Width - 200, 5),
                BackColor = Color.Transparent,
                ForeColor = WhiteText,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnCharts.FlatAppearance.BorderSize = 0;
            btnCharts.Click += (s, e) => new DashboardChartsForm().ShowDialog();

            // Clock
            lblClock = new Label
            {
                Font = new Font("Consolas", 10),
                ForeColor = WhiteText,
                Location = new Point(this.Width - 450, 15),
                Size = new Size(330, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            topBar.Controls.Add(btnClose);
            topBar.Controls.Add(btnMinimize);
            topBar.Controls.Add(lblAppTitle);
            topBar.Controls.Add(btnNotifications);
            topBar.Controls.Add(btnCharts);
            topBar.Controls.Add(lblClock);

            // ========== SIDE PANEL ==========
            sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 20, 10, 20)
            };

            // Profile Section with Circular Avatar
            Panel profilePanel = new Panel
            {
                Height = 150,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };

            PictureBox avatar = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(100, 15),
                BackColor = Color.Transparent
            };
            avatar.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, 80, 80);
                avatar.Region = new Region(path);
                using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, 80, 80),
                    GreenButtonColor, DarkButtonColor, 45f))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 80, 80);
                }
                e.Graphics.DrawString("👤", new Font("Segoe UI", 32), new SolidBrush(WhiteText), 22, 20);
            };

            lblWelcome = new Label
            {
                Text = Global.FullName + "\n" + Global.UserRole,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WhiteText,
                Location = new Point(70, 105),
                Size = new Size(140, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            profilePanel.Controls.Add(avatar);
            profilePanel.Controls.Add(lblWelcome);

            sidePanel.Controls.Add(profilePanel);

            // Create circular navigation buttons
            sidePanel.Controls.Add(CreateCircularNavButton("📦 PRODUCTS", 0, GreenButtonColor, WhiteText));
            sidePanel.Controls.Add(CreateCircularNavButton("📥 STOCK", 1, DarkButtonColor, WhiteText));
            sidePanel.Controls.Add(CreateCircularNavButton("📊 REPORTS", 2, PinkButtonColor, DarkText));
            if (Global.UserRole == "Admin")
                sidePanel.Controls.Add(CreateCircularNavButton("👑 ADMIN", 3, GreenButtonColor, WhiteText));
            sidePanel.Controls.Add(CreateCircularNavButton("🚪 LOGOUT", 4, DarkButtonColor, WhiteText));

            // ========== CONTENT PANEL ==========
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor,
                Padding = new Padding(25)
            };

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.Transparent
            };

            Label lblDashboardTitle = new Label
            {
                Text = "📊 DASHBOARD",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = WhiteText,
                Location = new Point(0, 20),
                Size = new Size(300, 35)
            };

            headerPanel.Controls.Add(lblDashboardTitle);
            contentPanel.Controls.Add(headerPanel);

            // Stats Cards
            cardPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 90),
                Width = this.Width - 310,
                Height = 260,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0)
            };
            contentPanel.Controls.Add(cardPanel);

            // Recent Activity Panel
            Panel activityPanel = new Panel
            {
                Location = new Point(0, 370),
                Width = this.Width - 310,
                Height = 380,
                BackColor = CardBackground,
                BorderStyle = BorderStyle.None
            };

            activityPanel.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 15;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(activityPanel.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(activityPanel.Width - radius, activityPanel.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, activityPanel.Height - radius, radius, radius, 90, 90);
                activityPanel.Region = new Region(path);
                using (Pen pen = new Pen(GreenButtonColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, activityPanel.Width - 1, activityPanel.Height - 1);
                }
            };

            Label lblActivityTitle = new Label
            {
                Text = "📋 RECENT ACTIVITY",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DarkButtonColor,
                Location = new Point(20, 15),
                Size = new Size(300, 30)
            };
            activityPanel.Controls.Add(lblActivityTitle);
            contentPanel.Controls.Add(activityPanel);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidePanel);
            this.Controls.Add(topBar);

            this.Resize += (s, e) => {
                btnClose.Location = new Point(this.Width - 50, 5);
                btnMinimize.Location = new Point(this.Width - 95, 5);
                btnNotifications.Location = new Point(this.Width - 150, 5);
                btnCharts.Location = new Point(this.Width - 200, 5);
                lblClock.Location = new Point(this.Width - 450, 15);
                cardPanel.Width = this.Width - 310;
                activityPanel.Width = this.Width - 310;
            };
        }

        private void StartClock()
        {
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => {
                lblClock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - hh:mm:ss tt");
            };
            clockTimer.Start();
        }

        private Button CreateCircularNavButton(string text, int index, Color buttonColor, Color textColor)
        {
            Button btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(240, 50),
                Location = new Point(20, 170 + (index * 65)),
                BackColor = buttonColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Tag = index
            };

            // Make button circular with rounded corners
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (sender, e) =>
            {
                Button b = sender as Button;
                if (b == null) return;
                GraphicsPath path = new GraphicsPath();
                int radius = 25;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(b.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(b.Width - radius, b.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, b.Height - radius, radius, radius, 90, 90);
                b.Region = new Region(path);
            };

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = ControlPaint.Light(buttonColor, 0.2f);
                btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = buttonColor;
                btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            };

            if (text.Contains("PRODUCTS"))
                btn.Click += (s, e) => OpenForm(new ProductForm());
            else if (text.Contains("STOCK"))
                btn.Click += (s, e) => ShowStockMenu();
            else if (text.Contains("REPORTS"))
                btn.Click += (s, e) => ShowReportsMenu();
            else if (text.Contains("ADMIN"))
                btn.Click += (s, e) => OpenForm(new UserManagementForm());
            else if (text.Contains("LOGOUT"))
                btn.Click += (s, e) => Logout();

            return btn;
        }

        private void ShowStockMenu()
        {
            ContextMenuStrip stockMenu = new ContextMenuStrip
            {
                BackColor = DarkButtonColor,
                ForeColor = WhiteText,
                Font = new Font("Segoe UI", 9)
            };
            stockMenu.Items.Add("📥 Stock In (Purchase)", null, (s, e) => OpenForm(new StockInForm()));
            stockMenu.Items.Add("📤 Stock Out (Sale)", null, (s, e) => OpenForm(new StockOutForm()));
            stockMenu.Items.Add(new ToolStripSeparator());
            stockMenu.Items.Add("📷 Barcode Scanner", null, (s, e) => new BarcodeScannerForm().ShowDialog());
            stockMenu.Show(Cursor.Position);
        }

        private void ShowReportsMenu()
        {
            ContextMenuStrip reportsMenu = new ContextMenuStrip
            {
                BackColor = PinkButtonColor,
                ForeColor = DarkText,
                Font = new Font("Segoe UI", 9)
            };
            reportsMenu.Items.Add("📋 Current Stock Report", null, (s, e) => OpenForm(new StockReportForm()));
            reportsMenu.Items.Add("⚠️ Low Stock Alert", null, (s, e) => OpenForm(new LowStockReportForm()));
            reportsMenu.Items.Add("💰 Sales Report", null, (s, e) => OpenForm(new SalesReportForm()));
            reportsMenu.Items.Add(new ToolStripSeparator());
            reportsMenu.Items.Add("📈 Analytics Dashboard", null, (s, e) => new DashboardChartsForm().ShowDialog());
            reportsMenu.Show(Cursor.Position);
        }

        private void LoadDashboard()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetDashboardStats();
                if (dt.Rows.Count > 0)
                {
                    cardPanel.Controls.Clear();

                    // Regular cards with Dark Teal numbers
                    CreateStatCard("📦 Total Products", dt.Rows[0]["TotalProducts"].ToString(), CardNumberColor, CardLabelColor);
                    CreateStatCard("📁 Categories", dt.Rows[0]["TotalCategories"].ToString(), CardNumberColor, CardLabelColor);
                    CreateStatCard("🏢 Suppliers", dt.Rows[0]["TotalSuppliers"].ToString(), CardNumberColor, CardLabelColor);
                    CreateStatCard("📊 Total Stock", dt.Rows[0]["TotalStock"].ToString(), CardNumberColor, CardLabelColor);

                    decimal totalValue = Convert.ToDecimal(dt.Rows[0]["TotalValue"]);
                    CreateStatCard("💰 Inventory Value", "₱" + totalValue.ToString("N2"), CardNumberColor, CardLabelColor);

                    // Low Stock card with Green color (Active style)
                    string lowStockCount = dt.Rows[0]["LowStockCount"].ToString();
                    CreateStatCard("⚠️ Low Stock Items", lowStockCount, ActiveNumberColor, ActiveTextColor);
                }
                LoadRecentActivity();
            }
            catch (Exception)
            {
                // Exception handled silently - no action needed
            }
        }

        private void CreateStatCard(string title, string value, Color numberColor, Color labelColor)
        {
            Panel card = new Panel
            {
                Size = new Size(220, 130),
                BackColor = CardBackground,
                Margin = new Padding(12, 8, 12, 8),
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 12;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                card.Region = new Region(path);
                using (Pen pen = new Pen(GreenButtonColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(245, 245, 245);
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = CardBackground;
            };

            // Title label with Card Label Color
            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(15, 15),
                Size = new Size(190, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = labelColor
            };

            // Value label with specified number color
            Label valueLabel = new Label
            {
                Text = value,
                Location = new Point(15, 50),
                Size = new Size(190, 60),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = numberColor
            };

            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            cardPanel.Controls.Add(card);
        }

        private void LoadRecentActivity()
        {
            try
            {
                Panel activityPanel = null;
                foreach (Control ctrl in contentPanel.Controls)
                {
                    if (ctrl is Panel && ctrl.Name == "" && ctrl.Location.Y > 300)
                    {
                        activityPanel = (Panel)ctrl;
                        break;
                    }
                }
                if (activityPanel == null) return;

                // Clear existing activity items but keep the title
                for (int i = activityPanel.Controls.Count - 1; i >= 0; i--)
                {
                    if (activityPanel.Controls[i] is Label && activityPanel.Controls[i].Location.Y > 50)
                    {
                        activityPanel.Controls.RemoveAt(i);
                    }
                }

                int y = 55;

                // Recent Stock In section
                Label lblInTitle = new Label
                {
                    Text = "📥 RECENT STOCK IN",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = GreenButtonColor,
                    Location = new Point(20, y),
                    Size = new Size(300, 30)
                };
                activityPanel.Controls.Add(lblInTitle);
                y += 35;

                DataTable dtIn = DatabaseHelper.GetDataTable("SELECT TOP 5 p.Name, si.Quantity, si.Date FROM StockIn si INNER JOIN Products p ON si.ProductID = p.ProductID ORDER BY si.Date DESC");
                foreach (DataRow row in dtIn.Rows)
                {
                    Label lblItem = new Label
                    {
                        Text = $"  • {row["Name"]} | +{row["Quantity"]} units | {Convert.ToDateTime(row["Date"]):yyyy-MM-dd}",
                        Font = new Font("Consolas", 10),
                        ForeColor = StatusLabelColor,
                        Location = new Point(35, y),
                        Size = new Size(700, 25)
                    };
                    activityPanel.Controls.Add(lblItem);
                    y += 28;
                }

                y += 20;

                // Recent Stock Out section
                Label lblOutTitle = new Label
                {
                    Text = "📤 RECENT STOCK OUT",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = PinkButtonColor,
                    Location = new Point(20, y),
                    Size = new Size(300, 30)
                };
                activityPanel.Controls.Add(lblOutTitle);
                y += 35;

                DataTable dtOut = DatabaseHelper.GetDataTable("SELECT TOP 5 p.Name, so.Quantity, so.Date FROM StockOut so INNER JOIN Products p ON so.ProductID = p.ProductID ORDER BY so.Date DESC");
                foreach (DataRow row in dtOut.Rows)
                {
                    Label lblItem = new Label
                    {
                        Text = $"  • {row["Name"]} | -{row["Quantity"]} units | {Convert.ToDateTime(row["Date"]):yyyy-MM-dd}",
                        Font = new Font("Consolas", 10),
                        ForeColor = StatusLabelColor,
                        Location = new Point(35, y),
                        Size = new Size(700, 25)
                    };
                    activityPanel.Controls.Add(lblItem);
                    y += 28;
                }
            }
            catch (Exception)
            {
                // Exception handled silently - no action needed
            }
        }

        private void OpenForm(Form form)
        {
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
            LoadDashboard();
        }

        private void Logout()
        {
            if (notificationManager != null)
            {
                notificationManager.StopMonitoring();
            }
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                LoginForm login = new LoginForm();
                login.Show();
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (notificationManager != null)
            {
                notificationManager.StopMonitoring();
            }
            base.OnFormClosing(e);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}