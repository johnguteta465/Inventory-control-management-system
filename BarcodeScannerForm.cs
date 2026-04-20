using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class BarcodeScannerForm : Form
    {
        private TextBox txtBarcode;
        private Button btnScan, btnConfirm;
        private Label lblResult, lblProductInfo;
        private PictureBox pbBarcode;

        public string ScannedProductCode { get; private set; }
        public string ScannedProductName { get; private set; }
        public decimal ScannedProductPrice { get; private set; }

        public BarcodeScannerForm()
        {
            InitializeComponent();
            SetupForm();
            LoadDemoBarcode();
        }

        private void InitializeComponent()
        {
            this.Text = "Barcode Scanner";
            this.Size = new Size(550, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void SetupForm()
        {
            // Title
            Label lblTitle = new Label()
            {
                Text = "🔍 BARCODE SCANNER",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            // Barcode Display
            pbBarcode = new PictureBox()
            {
                Location = new Point(125, 80),
                Size = new Size(300, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            // OR Label
            Label lblOr = new Label()
            {
                Text = "OR",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(255, 245),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            // Help Text
            Label lblHelp = new Label()
            {
                Text = "📝 Type product code and click CONFIRM",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(100, 275),
                Size = new Size(350, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Barcode Input
            txtBarcode = new TextBox()
            {
                Location = new Point(125, 305),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 12),
                TextAlign = HorizontalAlignment.Center
            };
            txtBarcode.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) ProcessBarcode(); };

            // Demo Button
            btnScan = new Button()
            {
                Text = "📷 SCAN DEMO",
                Location = new Point(125, 355),
                Size = new Size(145, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnScan.Click += (s, e) => SimulateScan();

            // Confirm Button
            btnConfirm = new Button()
            {
                Text = "✓ CONFIRM",
                Location = new Point(280, 355),
                Size = new Size(145, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirm.Click += (s, e) => ProcessBarcode();

            // Product Info Label
            lblProductInfo = new Label()
            {
                Location = new Point(20, 410),
                Size = new Size(510, 40),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Result Label
            lblResult = new Label()
            {
                Location = new Point(20, 460),
                Size = new Size(510, 35),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Red
            };

            // Demo Codes Label
            Label lblDemoCodes = new Label()
            {
                Text = "💡 Try these codes: P001, P002, P003, P004, P005, P006",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(125, 405),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(pbBarcode);
            this.Controls.Add(lblOr);
            this.Controls.Add(lblHelp);
            this.Controls.Add(txtBarcode);
            this.Controls.Add(btnScan);
            this.Controls.Add(btnConfirm);
            this.Controls.Add(lblProductInfo);
            this.Controls.Add(lblResult);
            this.Controls.Add(lblDemoCodes);
        }

        private void LoadDemoBarcode()
        {
            try
            {
                Bitmap bmp = new Bitmap(280, 120);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    g.DrawString("P001", new Font("Courier New", 36, FontStyle.Bold), Brushes.Black, 80, 35);
                    g.DrawString("||||||||||||||||||||||||||||||||", new Font("Courier New", 14), Brushes.Black, 20, 80);
                }
                pbBarcode.Image = bmp;
            }
            catch { }
        }

        private void SimulateScan()
        {
            string[] demoBarcodes = { "P001", "P002", "P003", "P004", "P005", "P006" };
            Random rnd = new Random();
            txtBarcode.Text = demoBarcodes[rnd.Next(demoBarcodes.Length)];
            ProcessBarcode();
        }

        private void ProcessBarcode()
        {
            if (string.IsNullOrEmpty(txtBarcode.Text))
            {
                lblResult.Text = "⚠️ Please enter a product code!";
                lblResult.ForeColor = Color.Orange;
                lblProductInfo.Text = "";
                return;
            }

            ScannedProductCode = txtBarcode.Text.Trim().ToUpper();

            try
            {
                string query = "SELECT ProductID, Name, UnitPrice, Quantity FROM Products WHERE ProductCode = @code";
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@code", ScannedProductCode);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int productId = reader.GetInt32(0);
                            ScannedProductName = reader.GetString(1);
                            ScannedProductPrice = reader.GetDecimal(2);
                            int quantity = reader.GetInt32(3);

                            lblProductInfo.Text = $"✅ Product: {ScannedProductName} | Price: ₱{ScannedProductPrice:N2} | Stock: {quantity}";
                            lblProductInfo.BackColor = Color.FromArgb(200, 255, 200);
                            lblProductInfo.ForeColor = Color.DarkGreen;

                            lblResult.Text = "✅ Product found! Click OK to use this product.";
                            lblResult.ForeColor = Color.Green;

                            DialogResult result = MessageBox.Show(
                                $"Product Found!\n\nCode: {ScannedProductCode}\nName: {ScannedProductName}\nPrice: ₱{ScannedProductPrice:N2}\nStock: {quantity}\n\nUse this product?",
                                "Product Found",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                        }
                        else
                        {
                            lblProductInfo.Text = $"❌ Product '{ScannedProductCode}' not found in database!";
                            lblProductInfo.BackColor = Color.FromArgb(255, 200, 200);
                            lblProductInfo.ForeColor = Color.Red;

                            lblResult.Text = "❌ Product not found! Would you like to add it?";
                            lblResult.ForeColor = Color.Red;

                            DialogResult result = MessageBox.Show(
                                $"Product '{ScannedProductCode}' not found.\n\nWould you like to create it?",
                                "Product Not Found",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                ProductForm productForm = new ProductForm();
                                productForm.ShowDialog();
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = $"❌ Database Error: {ex.Message}";
                lblResult.ForeColor = Color.Red;
                lblProductInfo.Text = "";
            }
        }
    }
}