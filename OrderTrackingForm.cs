using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class OrderTrackingForm : Form
    {
        private DataGridView dgvOrders;
        private Label lblStatus, lblPaymentStatus;
        private TextBox txtRejectionReason;
        private Panel pnlStatusDetails;

        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);
        private static readonly Color GreenColor = Color.FromArgb(16, 145, 95);
        private static readonly Color RedColor = Color.FromArgb(231, 76, 60);
        private static readonly Color YellowColor = Color.FromArgb(230, 126, 34);
        private static readonly Color BlueColor = Color.FromArgb(52, 152, 219);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public OrderTrackingForm()
        {
            InitializeComponent();
            LoadCustomerOrders();
        }

        private void InitializeComponent()
        {
            this.Text = "My Orders - Tracking";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;

            Label lblTitle = new Label()
            {
                Text = "📦 MY ORDERS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(400, 40),
                ForeColor = Color.White
            };

            dgvOrders = new DataGridView()
            {
                Location = new Point(20, 70),
                Size = new Size(950, 250),
                BackColor = Color.White,
                ForeColor = DarkText,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

            // Status Details Panel
            pnlStatusDetails = new Panel()
            {
                Location = new Point(20, 330),
                Size = new Size(950, 280),
                BackColor = Color.FromArgb(240, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblOrderStatus = new Label()
            {
                Text = "Order Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 25),
                ForeColor = DarkText
            };

            lblStatus = new Label()
            {
                Text = "Select an order to view details",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 40),
                Size = new Size(920, 30),
                ForeColor = YellowColor,
                AutoSize = false
            };

            Label lblPayment = new Label()
            {
                Text = "Payment Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 80),
                Size = new Size(300, 25),
                ForeColor = DarkText
            };

            lblPaymentStatus = new Label()
            {
                Text = "Select an order to view details",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 110),
                Size = new Size(920, 30),
                ForeColor = YellowColor,
                AutoSize = false
            };

            Label lblRejectionLabel = new Label()
            {
                Text = "Rejection Reason (if applicable):",
                Location = new Point(10, 150),
                Size = new Size(200, 20),
                ForeColor = DarkText
            };

            txtRejectionReason = new TextBox()
            {
                Location = new Point(10, 175),
                Size = new Size(920, 90),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.White
            };

            pnlStatusDetails.Controls.Add(lblOrderStatus);
            pnlStatusDetails.Controls.Add(lblStatus);
            pnlStatusDetails.Controls.Add(lblPayment);
            pnlStatusDetails.Controls.Add(lblPaymentStatus);
            pnlStatusDetails.Controls.Add(lblRejectionLabel);
            pnlStatusDetails.Controls.Add(txtRejectionReason);

            this.Controls.Add(lblTitle);
            this.Controls.Add(dgvOrders);
            this.Controls.Add(pnlStatusDetails);
        }

        private void LoadCustomerOrders()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.SaleID,
                        s.InvoiceNumber,
                        s.TotalAmount,
                        s.PaymentMethod,
                        s.PaymentStatus,
                        s.OrderStatus,
                        s.SaleDate,
                        s.RejectionReason
                    FROM Sales s
                    WHERE s.CustomerID = (
                        SELECT CustomerID FROM Customers 
                        WHERE Email = @email OR FullName = @name
                    )
                    ORDER BY s.SaleDate DESC";

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", Global.UserName + "@customer.local");
                    cmd.Parameters.AddWithValue("@name", Global.FullName);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvOrders.DataSource = dt;
                    dgvOrders.Columns["SaleID"].Visible = false;
                    dgvOrders.Columns["RejectionReason"].Visible = false;

                    // Format columns
                    dgvOrders.Columns["InvoiceNumber"].HeaderText = "Invoice";
                    dgvOrders.Columns["InvoiceNumber"].Width = 120;
                    dgvOrders.Columns["TotalAmount"].HeaderText = "Amount (Br)";
                    dgvOrders.Columns["TotalAmount"].Width = 100;
                    dgvOrders.Columns["PaymentMethod"].HeaderText = "Payment";
                    dgvOrders.Columns["PaymentMethod"].Width = 100;
                    dgvOrders.Columns["PaymentStatus"].HeaderText = "Payment Status";
                    dgvOrders.Columns["PaymentStatus"].Width = 130;
                    dgvOrders.Columns["OrderStatus"].HeaderText = "Order Status";
                    dgvOrders.Columns["OrderStatus"].Width = 120;
                    dgvOrders.Columns["SaleDate"].HeaderText = "Date";
                    dgvOrders.Columns["SaleDate"].Width = 130;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading orders: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvOrders.SelectedRows[0];
                string invoiceNumber = row.Cells["InvoiceNumber"].Value.ToString();
                string orderStatus = row.Cells["OrderStatus"].Value.ToString();
                string paymentStatus = row.Cells["PaymentStatus"].Value.ToString();
                string rejectionReason = row.Cells["RejectionReason"].Value?.ToString() ?? "";

                // Set order status with color
                Color orderStatusColor = GetOrderStatusColor(orderStatus);
                lblStatus.Text = $"📦 Order Status: {orderStatus}";
                lblStatus.ForeColor = orderStatusColor;

                // Set payment status with color
                Color paymentStatusColor = GetPaymentStatusColor(paymentStatus);
                lblPaymentStatus.Text = $"💳 Payment Status: {paymentStatus}";
                lblPaymentStatus.ForeColor = paymentStatusColor;

                // Show rejection reason if applicable
                if (!string.IsNullOrWhiteSpace(rejectionReason))
                {
                    txtRejectionReason.Text = $"Reason: {rejectionReason}";
                    txtRejectionReason.ForeColor = RedColor;
                }
                else
                {
                    txtRejectionReason.Clear();
                }
            }
        }

        private Color GetOrderStatusColor(string status)
        {
            return status switch
            {
                "Completed" => GreenColor,
                "Pending" => YellowColor,
                "Cancelled" => RedColor,
                _ => DarkText
            };
        }

        private Color GetPaymentStatusColor(string status)
        {
            return status switch
            {
                "Verified" => GreenColor,
                "Pending Verification" => YellowColor,
                "Rejected" => RedColor,
                "Paid" => GreenColor,
                _ => DarkText
            };
        }
    }
}
