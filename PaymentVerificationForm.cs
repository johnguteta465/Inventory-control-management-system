using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class PaymentVerificationForm : Form
    {
        private DataGridView dgvPendingOrders;
        private Label lblOrderDetails, lblPaymentStatus, lblOrderStatus;
        private TextBox txtTransactionRef, txtRejectionReason;
        private Button btnVerify, btnReject, btnRefresh;
        private Panel pnlDetails;

        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);
        private static readonly Color RedButtonColor = Color.FromArgb(231, 76, 60);
        private static readonly Color BlueButtonColor = Color.FromArgb(52, 152, 219);
        private static readonly Color WhiteText = Color.FromArgb(255, 255, 255);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public PaymentVerificationForm()
        {
            InitializeComponent();
            LoadPendingOrders();
        }

        private void InitializeComponent()
        {
            this.Text = "Payment Verification";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;

            Label lblTitle = new Label()
            {
                Text = "💳 PAYMENT VERIFICATION",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(400, 40),
                ForeColor = Color.White
            };

            Label lblPendingOrders = new Label()
            {
                Text = "Pending Verification Orders:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 70),
                Size = new Size(300, 25),
                ForeColor = DarkText
            };

            dgvPendingOrders = new DataGridView()
            {
                Location = new Point(20, 100),
                Size = new Size(950, 250),
                BackColor = Color.White,
                ForeColor = DarkText,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgvPendingOrders.SelectionChanged += DgvPendingOrders_SelectionChanged;

            // Details Panel
            pnlDetails = new Panel()
            {
                Location = new Point(20, 360),
                Size = new Size(950, 280),
                BackColor = Color.FromArgb(240, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblOrderDetails = new Label()
            {
                Text = "Order Details",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 25),
                ForeColor = DarkText
            };

            lblPaymentStatus = new Label()
            {
                Text = "Payment Status: Pending Verification",
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 40),
                Size = new Size(400, 20),
                ForeColor = Color.FromArgb(230, 126, 34)
            };

            lblOrderStatus = new Label()
            {
                Text = "Order Status: Pending",
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 65),
                Size = new Size(400, 20),
                ForeColor = Color.FromArgb(230, 126, 34)
            };

            Label lblTransRef = new Label()
            {
                Text = "Transaction Reference:",
                Location = new Point(10, 95),
                Size = new Size(150, 20),
                ForeColor = DarkText
            };

            txtTransactionRef = new TextBox()
            {
                Location = new Point(170, 93),
                Size = new Size(300, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Label lblRejection = new Label()
            {
                Text = "Rejection Reason (if rejecting):",
                Location = new Point(10, 130),
                Size = new Size(200, 20),
                ForeColor = DarkText
            };

            txtRejectionReason = new TextBox()
            {
                Location = new Point(10, 155),
                Size = new Size(920, 80),
                Multiline = true,
                BackColor = Color.White
            };

            btnVerify = new Button()
            {
                Text = "✓ VERIFY PAYMENT",
                Location = new Point(10, 245),
                Size = new Size(150, 35),
                BackColor = GreenButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerify.Click += BtnVerify_Click;

            btnReject = new Button()
            {
                Text = "✗ REJECT PAYMENT",
                Location = new Point(170, 245),
                Size = new Size(150, 35),
                BackColor = RedButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReject.Click += BtnReject_Click;

            btnRefresh = new Button()
            {
                Text = "🔄 REFRESH",
                Location = new Point(330, 245),
                Size = new Size(120, 35),
                BackColor = BlueButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadPendingOrders();

            pnlDetails.Controls.Add(lblOrderDetails);
            pnlDetails.Controls.Add(lblPaymentStatus);
            pnlDetails.Controls.Add(lblOrderStatus);
            pnlDetails.Controls.Add(lblTransRef);
            pnlDetails.Controls.Add(txtTransactionRef);
            pnlDetails.Controls.Add(lblRejection);
            pnlDetails.Controls.Add(txtRejectionReason);
            pnlDetails.Controls.Add(btnVerify);
            pnlDetails.Controls.Add(btnReject);
            pnlDetails.Controls.Add(btnRefresh);

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblPendingOrders);
            this.Controls.Add(dgvPendingOrders);
            this.Controls.Add(pnlDetails);
        }

        private void LoadPendingOrders()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.SaleID,
                        s.InvoiceNumber,
                        c.FullName AS CustomerName,
                        s.TotalAmount,
                        s.PaymentMethod,
                        s.PaymentStatus,
                        s.OrderStatus,
                        s.SaleDate,
                        s.TransactionRef
                    FROM Sales s
                    JOIN Customers c ON s.CustomerID = c.CustomerID
                    WHERE s.PaymentStatus = 'Pending Verification'
                    ORDER BY s.SaleDate DESC";

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPendingOrders.DataSource = dt;
                    dgvPendingOrders.Columns["SaleID"].Visible = false;

                    // Format columns
                    dgvPendingOrders.Columns["InvoiceNumber"].HeaderText = "Invoice";
                    dgvPendingOrders.Columns["InvoiceNumber"].Width = 120;
                    dgvPendingOrders.Columns["CustomerName"].HeaderText = "Customer";
                    dgvPendingOrders.Columns["CustomerName"].Width = 150;
                    dgvPendingOrders.Columns["TotalAmount"].HeaderText = "Amount (Br)";
                    dgvPendingOrders.Columns["TotalAmount"].Width = 100;
                    dgvPendingOrders.Columns["PaymentMethod"].HeaderText = "Method";
                    dgvPendingOrders.Columns["PaymentMethod"].Width = 100;
                    dgvPendingOrders.Columns["SaleDate"].HeaderText = "Date";
                    dgvPendingOrders.Columns["SaleDate"].Width = 130;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading orders: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
                int saleId = Convert.ToInt32(row.Cells["SaleID"].Value);
                string invoiceNumber = row.Cells["InvoiceNumber"].Value.ToString();
                string customerName = row.Cells["CustomerName"].Value.ToString();
                decimal totalAmount = Convert.ToDecimal(row.Cells["TotalAmount"].Value);
                string paymentMethod = row.Cells["PaymentMethod"].Value.ToString();
                string transactionRef = row.Cells["TransactionRef"].Value?.ToString() ?? "Not provided";

                lblOrderDetails.Text = $"Order Details - {invoiceNumber} | Customer: {customerName} | Amount: Br{totalAmount:N2} | Method: {paymentMethod}";
                lblPaymentStatus.Text = "Payment Status: ⏳ Pending Verification";
                lblOrderStatus.Text = "Order Status: ⏳ Pending";
                txtTransactionRef.Text = transactionRef;
                txtRejectionReason.Clear();
            }
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order to verify.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
            int saleId = Convert.ToInt32(row.Cells["SaleID"].Value);
            string invoiceNumber = row.Cells["InvoiceNumber"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"Verify payment for Invoice {invoiceNumber}?\n\nThis will mark the order as Completed.",
                "Confirm Verification",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string updateQuery = @"
                        UPDATE Sales 
                        SET PaymentStatus = 'Verified',
                            OrderStatus = 'Completed',
                            VerifiedBy = @verifiedBy,
                            VerifiedDate = GETDATE()
                        WHERE SaleID = @saleId";

                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        SqlCommand cmd = new SqlCommand(updateQuery, conn);
                        cmd.Parameters.AddWithValue("@saleId", saleId);
                        cmd.Parameters.AddWithValue("@verifiedBy", Global.UserID);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Payment verified successfully!\nInvoice: {invoiceNumber}\nOrder Status: Completed", 
                                   "Verification Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPendingOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error verifying payment: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order to reject.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRejectionReason.Text))
            {
                MessageBox.Show("Please enter a rejection reason.", "Missing Reason", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
            int saleId = Convert.ToInt32(row.Cells["SaleID"].Value);
            string invoiceNumber = row.Cells["InvoiceNumber"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"Reject payment for Invoice {invoiceNumber}?\n\nReason: {txtRejectionReason.Text}\n\nThis will cancel the order.",
                "Confirm Rejection",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string updateQuery = @"
                        UPDATE Sales 
                        SET PaymentStatus = 'Rejected',
                            OrderStatus = 'Cancelled',
                            RejectionReason = @reason,
                            VerifiedBy = @verifiedBy,
                            VerifiedDate = GETDATE()
                        WHERE SaleID = @saleId";

                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        SqlCommand cmd = new SqlCommand(updateQuery, conn);
                        cmd.Parameters.AddWithValue("@saleId", saleId);
                        cmd.Parameters.AddWithValue("@reason", txtRejectionReason.Text);
                        cmd.Parameters.AddWithValue("@verifiedBy", Global.UserID);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Payment rejected!\nInvoice: {invoiceNumber}\nOrder Status: Cancelled\nReason: {txtRejectionReason.Text}", 
                                   "Rejection Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPendingOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting payment: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
