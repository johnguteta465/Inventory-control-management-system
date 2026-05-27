using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class CustomerDashboardForm : Form
    {
        private Label lblWelcome, lblClock;
        private Timer clockTimer;
        private Button btnProducts, btnOrders, btnLogout;

        // Theme colors
        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);
        private static readonly Color DarkButtonColor = Color.FromArgb(44, 47, 58);
        private static readonly Color WhiteText = Color.FromArgb(255, 255, 255);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public CustomerDashboardForm()
        {
            InitializeComponent();
            StartClock();
        }

        private void InitializeComponent()
        {
            this.Text = "Customer Dashboard - Inventory Pro";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;

            // Header Panel (Gradient)
            Panel headerPanel = new Panel() { Dock = DockStyle.Top, Height = 80 };
            headerPanel.Paint += (s, e) =>
            {
                Rectangle rect = headerPanel.ClientRectangle;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect,
                    Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185), 90f))
                    e.Graphics.FillRectangle(brush, rect);
            };
            Label lblTitle = new Label()
            {
                Text = "🛍️ CUSTOMER DASHBOARD",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(20, 22),
                Size = new Size(400, 40),
                ForeColor = Color.White
            };
            lblClock = new Label()
            {
                Font = new Font("Consolas", 10),
                ForeColor = Color.White,
                Location = new Point(700, 30),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblClock);

            // Welcome message
            lblWelcome = new Label()
            {
                Text = $"Welcome, {Global.FullName}!",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DarkText,
                Location = new Point(30, 110),
                Size = new Size(400, 35)
            };

            // Buttons
            btnProducts = new Button()
            {
                Text = "📦 Browse Products",
                Location = new Point(50, 180),
                Size = new Size(250, 60),
                BackColor = GreenButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnProducts.Click += (s, e) => new CustomerProductsForm().ShowDialog();

            btnOrders = new Button()
            {
                Text = "📋 My Orders",
                Location = new Point(350, 180),
                Size = new Size(250, 60),
                BackColor = DarkButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOrders.Click += (s, e) => new CustomerOrdersForm().ShowDialog();

            btnLogout = new Button()
            {
                Text = "🚪 Logout",
                Location = new Point(650, 180),
                Size = new Size(200, 60),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.Click += (s, e) => Logout();

            // Info panel
            Panel infoPanel = new Panel()
            {
                Location = new Point(50, 280),
                Size = new Size(800, 250),
                BackColor = Color.FromArgb(255, 255, 255, 200),
                BorderStyle = BorderStyle.FixedSingle
            };
            Label lblInfo = new Label()
            {
                Text = "Welcome to Inventory Pro!\n\n" +
                       "• Browse our wide range of products\n" +
                       "• Add items to your cart and place orders\n" +
                       "• Track your order status\n" +
                       "• View your complete purchase history\n\n" +
                       "Need help? Contact support@inventorypro.com",
                Font = new Font("Segoe UI", 10),
                ForeColor = DarkText,
                Location = new Point(20, 20),
                Size = new Size(760, 200),
                TextAlign = ContentAlignment.TopLeft
            };
            infoPanel.Controls.Add(lblInfo);

            this.Controls.Add(headerPanel);
            this.Controls.Add(lblWelcome);
            this.Controls.Add(btnProducts);
            this.Controls.Add(btnOrders);
            this.Controls.Add(btnLogout);
            this.Controls.Add(infoPanel);
        }

        private void StartClock()
        {
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
                lblClock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - hh:mm:ss tt");
            clockTimer.Start();
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                LoginForm login = new LoginForm();
                login.Show();
                this.Close();
            }
        }
    }
}