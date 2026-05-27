using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace InventoryManagementSystem
{
    public class CheckoutForm : Form
    {
        private DataTable cartTable;
        private ComboBox cmbPaymentMethod;
        private TextBox txtAddress;
        private Label lblTotal;
        private Button btnPlaceOrder, btnCancel;

        // Bank Transfer Fields
        private Label lblBankName, lblAccountNumber, lblAccountHolder;
        private ComboBox cmbBankName;
        private TextBox txtAccountNumber, txtAccountHolder;
        private Panel pnlBankTransferFields;

        // Bank data dictionary - Ethiopian Banks
        private Dictionary<string, string> bankAccounts = new Dictionary<string, string>()
        {
            { "Commercial Bank of Ethiopia (CBE)", "1000123456789" },
            { "Dashen Bank", "2000987654321" },
            { "Awash International Bank", "3000555666777" },
            { "Abyssinia Bank", "4000444333222" },
            { "Bank of Abyssinia", "5000777888999" },
            { "United Bank", "6000222111000" },
            { "Cooperative Bank of Oromia", "7000666555444" },
            { "Addis International Bank", "8000999888777" }
        };

        private static readonly Color BackgroundColor = Color.FromArgb(134, 183, 181);
        private static readonly Color GreenButtonColor = Color.FromArgb(16, 145, 95);
        private static readonly Color WhiteText = Color.FromArgb(255, 255, 255);
        private static readonly Color DarkText = Color.FromArgb(50, 50, 50);

        public CheckoutForm(DataTable cart)
        {
            cartTable = cart;
            InitializeComponent();
            CalculateTotal();
        }

        private void InitializeComponent()
        {
            this.Text = "Checkout";
            this.Size = new Size(550, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label()
            {
                Text = "💳 COMPLETE YOUR ORDER",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 30),
                ForeColor = Color.White
            };

            Label lblTotalLabel = new Label()
            {
                Text = "Total Amount:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 70),
                Size = new Size(120, 30),
                ForeColor = DarkText
            };
            lblTotal = new Label()
            {
                Text = "Br0.00",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(160, 70),
                Size = new Size(200, 30),
                ForeColor = Color.Green
            };

            Label lblPayment = new Label()
            {
                Text = "Payment Method:",
                Location = new Point(30, 120),
                Size = new Size(120, 25),
                ForeColor = DarkText
            };
            cmbPaymentMethod = new ComboBox()
            {
                Location = new Point(160, 118),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPaymentMethod.Items.AddRange(new string[] { "Cash on Delivery", "Credit Card", "Bank Transfer", "Telebirr" });
            cmbPaymentMethod.SelectedIndex = 0;
            cmbPaymentMethod.SelectedIndexChanged += CmbPaymentMethod_SelectedIndexChanged;

            Label lblAddress = new Label()
            {
                Text = "Delivery Address:",
                Location = new Point(30, 160),
                Size = new Size(120, 25),
                ForeColor = DarkText
            };
            txtAddress = new TextBox()
            {
                Location = new Point(160, 158),
                Size = new Size(280, 60),
                Multiline = true
            };

            // ===== BANK TRANSFER PANEL =====
            pnlBankTransferFields = new Panel()
            {
                Location = new Point(30, 230),
                Size = new Size(410, 140),
                BackColor = Color.FromArgb(200, 220, 220),
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                Name = "pnlBank"
            };

            lblBankName = new Label()
            {
                Text = "Select Bank:",
                Location = new Point(10, 10),
                Size = new Size(100, 20),
                ForeColor = DarkText
            };
            cmbBankName = new ComboBox()
            {
                Location = new Point(120, 8),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var bank in bankAccounts.Keys)
            {
                cmbBankName.Items.Add(bank);
            }
            cmbBankName.SelectedIndexChanged += CmbBankName_SelectedIndexChanged;

            lblAccountNumber = new Label()
            {
                Text = "Account Number:",
                Location = new Point(10, 40),
                Size = new Size(100, 20),
                ForeColor = DarkText
            };
            txtAccountNumber = new TextBox()
            {
                Location = new Point(120, 38),
                Size = new Size(220, 25),
                Text = "",
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Button btnCopyAccount = new Button()
            {
                Text = "📋 Copy",
                Location = new Point(345, 38),
                Size = new Size(55, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopyAccount.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtAccountNumber.Text))
                {
                    Clipboard.SetText(txtAccountNumber.Text);
                    MessageBox.Show("Account number copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            lblAccountHolder = new Label()
            {
                Text = "Account Holder:",
                Location = new Point(10, 70),
                Size = new Size(100, 20),
                ForeColor = DarkText
            };
            txtAccountHolder = new TextBox()
            {
                Location = new Point(120, 68),
                Size = new Size(280, 25),
                Text = Global.FullName
            };

            Label lblPaymentInfo = new Label()
            {
                Text = "To pay: Copy account number, send payment, then click 'Place Order'",
                Location = new Point(10, 100),
                Size = new Size(390, 30),
                ForeColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false
            };

            pnlBankTransferFields.Controls.Add(lblBankName);
            pnlBankTransferFields.Controls.Add(cmbBankName);
            pnlBankTransferFields.Controls.Add(lblAccountNumber);
            pnlBankTransferFields.Controls.Add(txtAccountNumber);
            pnlBankTransferFields.Controls.Add(btnCopyAccount);
            pnlBankTransferFields.Controls.Add(lblAccountHolder);
            pnlBankTransferFields.Controls.Add(txtAccountHolder);
            pnlBankTransferFields.Controls.Add(lblPaymentInfo);

            // ===== TELEBIRR PANEL =====
            Panel pnlTelebirrFields = new Panel()
            {
                Location = new Point(30, 230),
                Size = new Size(410, 140),
                BackColor = Color.FromArgb(255, 235, 59),
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                Name = "pnlTelebirr"
            };

            Label lblTelebirrInfo = new Label()
            {
                Text = "📱 Telebirr Payment",
                Location = new Point(10, 10),
                Size = new Size(390, 20),
                ForeColor = Color.FromArgb(0, 0, 0),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            Label lblTelebirrNumber = new Label()
            {
                Text = "Send to:",
                Location = new Point(10, 35),
                Size = new Size(100, 20),
                ForeColor = DarkText
            };

            TextBox txtTelebirrNumber = new TextBox()
            {
                Location = new Point(120, 33),
                Size = new Size(220, 25),
                Text = "+251911234567",
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 255, 255)
            };

            Button btnCopyTelebirr = new Button()
            {
                Text = "📋 Copy",
                Location = new Point(345, 33),
                Size = new Size(55, 25),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopyTelebirr.Click += (s, e) =>
            {
                Clipboard.SetText("+251911234567");
                MessageBox.Show("Telebirr number copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Label lblTelebirrAmount = new Label()
            {
                Text = "Amount:",
                Location = new Point(10, 65),
                Size = new Size(100, 20),
                ForeColor = DarkText
            };

            TextBox txtTelebirrAmount = new TextBox()
            {
                Location = new Point(120, 63),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Label lblTelebirrInstruction = new Label()
            {
                Text = "To pay: Copy number, send Br amount via Telebirr, then click 'Place Order'",
                Location = new Point(10, 95),
                Size = new Size(390, 35),
                ForeColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false
            };

            pnlTelebirrFields.Controls.Add(lblTelebirrInfo);
            pnlTelebirrFields.Controls.Add(lblTelebirrNumber);
            pnlTelebirrFields.Controls.Add(txtTelebirrNumber);
            pnlTelebirrFields.Controls.Add(btnCopyTelebirr);
            pnlTelebirrFields.Controls.Add(lblTelebirrAmount);
            pnlTelebirrFields.Controls.Add(txtTelebirrAmount);
            pnlTelebirrFields.Controls.Add(lblTelebirrInstruction);

            // Update Telebirr amount when total changes
            this.Load += (s, e) =>
            {
                txtTelebirrAmount.Text = lblTotal.Text;
            };

            btnPlaceOrder = new Button()
            {
                Text = "✓ PLACE ORDER",
                Location = new Point(100, 480),
                Size = new Size(140, 40),
                BackColor = GreenButtonColor,
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPlaceOrder.Click += BtnPlaceOrder_Click;

            btnCancel = new Button()
            {
                Text = "Cancel",
                Location = new Point(260, 480),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = WhiteText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblTotalLabel);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblPayment);
            this.Controls.Add(cmbPaymentMethod);
            this.Controls.Add(lblAddress);
            this.Controls.Add(txtAddress);
            this.Controls.Add(pnlBankTransferFields);
            this.Controls.Add(pnlTelebirrFields);
            this.Controls.Add(btnPlaceOrder);
            this.Controls.Add(btnCancel);
        }

        private void CmbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "";
            
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel)
                {
                    if (ctrl.Name == "pnlBank")
                        ctrl.Visible = (selectedMethod == "Bank Transfer");
                    else if (ctrl.Name == "pnlTelebirr")
                        ctrl.Visible = (selectedMethod == "Telebirr");
                }
            }
        }

        private void CmbBankName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedBank = cmbBankName.SelectedItem?.ToString() ?? "";
            if (bankAccounts.ContainsKey(selectedBank))
            {
                txtAccountNumber.Text = bankAccounts[selectedBank];
            }
            else
            {
                txtAccountNumber.Text = "";
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in cartTable.Rows)
                total += Convert.ToDecimal(row["Total"]);
            lblTotal.Text = $"Br{total:N2}";
        }

        private int EnsureCustomerExists()
        {
            string checkQuery = "SELECT CustomerID FROM Customers WHERE Email = @email OR FullName = @name";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(checkQuery, conn);
                cmd.Parameters.AddWithValue("@email", Global.UserName + "@customer.local");
                cmd.Parameters.AddWithValue("@name", Global.FullName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
            }

            string insertQuery = @"INSERT INTO Customers (FullName, Email, Phone, Address, CreatedDate)
                                   VALUES (@name, @email, @phone, @address, GETDATE());
                                   SELECT SCOPE_IDENTITY();";
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", Global.FullName);
                cmd.Parameters.AddWithValue("@email", Global.UserName + "@customer.local");
                cmd.Parameters.AddWithValue("@phone", "");
                cmd.Parameters.AddWithValue("@address", "");
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void BtnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please enter a delivery address.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string paymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "";

            // Validate bank transfer fields if selected
            if (paymentMethod == "Bank Transfer")
            {
                if (cmbBankName.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a bank.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbBankName.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtAccountNumber.Text))
                {
                    MessageBox.Show("Account number is missing.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtAccountHolder.Text))
                {
                    MessageBox.Show("Please enter the account holder name.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtAccountHolder.Focus();
                    return;
                }
            }

            decimal subtotal = 0;
            foreach (DataRow row in cartTable.Rows)
                subtotal += Convert.ToDecimal(row["Total"]);

            string invoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int customerId = EnsureCustomerExists();

                        string saleQuery = @"INSERT INTO Sales (InvoiceNumber, CustomerID, Subtotal, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, TotalAmount, PaymentMethod, PaymentStatus, OrderStatus, CreatedBy, SaleDate)
                                             VALUES (@inv, @cust, @sub, 0, 0, 0, 0, @total, @payment, @payStatus, @orderStatus, @user, GETDATE());
                                             SELECT SCOPE_IDENTITY();";
                        SqlCommand saleCmd = new SqlCommand(saleQuery, conn, transaction);
                        saleCmd.Parameters.AddWithValue("@inv", invoiceNumber);
                        saleCmd.Parameters.AddWithValue("@cust", customerId);
                        saleCmd.Parameters.AddWithValue("@sub", subtotal);
                        saleCmd.Parameters.AddWithValue("@total", subtotal);
                        saleCmd.Parameters.AddWithValue("@payment", paymentMethod);
                        saleCmd.Parameters.AddWithValue("@payStatus", "Pending Verification");
                        saleCmd.Parameters.AddWithValue("@orderStatus", "Pending");
                        saleCmd.Parameters.AddWithValue("@user", Global.UserID);
                        int saleId = Convert.ToInt32(saleCmd.ExecuteScalar());

                        // Store bank transfer details if applicable
                        if (paymentMethod == "Bank Transfer")
                        {
                            try
                            {
                                string bankName = cmbBankName.SelectedItem?.ToString() ?? "";
                                string bankQuery = @"INSERT INTO BankTransferDetails (SaleID, BankName, AccountNumber, AccountHolder, CreatedDate)
                                                     VALUES (@saleId, @bank, @account, @holder, GETDATE())";
                                SqlCommand bankCmd = new SqlCommand(bankQuery, conn, transaction);
                                bankCmd.Parameters.AddWithValue("@saleId", saleId);
                                bankCmd.Parameters.AddWithValue("@bank", bankName);
                                bankCmd.Parameters.AddWithValue("@account", txtAccountNumber.Text);
                                bankCmd.Parameters.AddWithValue("@holder", txtAccountHolder.Text);
                                bankCmd.ExecuteNonQuery();
                            }
                            catch
                            {
                                // BankTransferDetails table may not exist yet - continue anyway
                                // The order will still be placed successfully
                            }
                        }

                        // Insert sale items
                        foreach (DataRow row in cartTable.Rows)
                        {
                            string itemQuery = @"INSERT INTO SaleItems (SaleID, ProductID, Quantity, UnitPrice, TotalPrice)
                                                 VALUES (@saleId, @pid, @qty, @price, @total)";
                            SqlCommand itemCmd = new SqlCommand(itemQuery, conn, transaction);
                            itemCmd.Parameters.AddWithValue("@saleId", saleId);
                            itemCmd.Parameters.AddWithValue("@pid", row["ProductID"]);
                            itemCmd.Parameters.AddWithValue("@qty", row["Quantity"]);
                            itemCmd.Parameters.AddWithValue("@price", row["UnitPrice"]);
                            itemCmd.Parameters.AddWithValue("@total", row["Total"]);
                            itemCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        string message = $"Order placed successfully!\nInvoice: {invoiceNumber}\nTotal: Br{subtotal:N2}\n\n⏳ Payment Verification Pending\n\nYour order has been created and is awaiting payment verification.\nAdmin/Cashier will review your payment and confirm the order.\n\nYou can track your order in 'My Orders'.";
                        
                        if (paymentMethod == "Bank Transfer")
                        {
                            string bankName = cmbBankName.SelectedItem?.ToString() ?? "";
                            message += $"\n\nBank Transfer Details:\nBank: {bankName}\nAccount: {txtAccountNumber.Text}\nHolder: {txtAccountHolder.Text}\n\nPlease send payment and provide transaction reference.";
                        }
                        else if (paymentMethod == "Telebirr")
                        {
                            message += $"\n\nTelebirr Payment:\nSend Br{subtotal:N2} to: +251911234567\nReference: {invoiceNumber}\n\nPlease provide transaction reference after payment.";
                        }

                        MessageBox.Show(message, "Order Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Order failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
