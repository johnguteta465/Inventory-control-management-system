using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class CategoryForm : Form
    {
        private DataGridView dgvCategories;
        private TextBox txtName, txtDescription;
        private Button btnSave, btnDelete, btnRefresh;
        private int selectedId = 0;

        public CategoryForm()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Categories";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label()
            {
                Text = "CATEGORY MANAGEMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(350, 35),
                ForeColor = Color.DarkBlue
            };

            // Input Panel
            Panel inputPanel = new Panel() { Location = new Point(20, 60), Size = new Size(300, 150), BackColor = Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };

            Label lblName = new Label() { Text = "Category Name:", Location = new Point(10, 25), Size = new Size(100, 25) };
            txtName = new TextBox() { Location = new Point(110, 23), Size = new Size(170, 25) };

            Label lblDesc = new Label() { Text = "Description:", Location = new Point(10, 65), Size = new Size(100, 25) };
            txtDescription = new TextBox() { Location = new Point(110, 63), Size = new Size(170, 25) };

            btnSave = new Button() { Text = "SAVE", Location = new Point(30, 105), Size = new Size(80, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button() { Text = "DELETE", Location = new Point(120, 105), Size = new Size(80, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button() { Text = "REFRESH", Location = new Point(210, 105), Size = new Size(80, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { LoadCategories(); ClearForm(); };

            inputPanel.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDescription, btnSave, btnDelete, btnRefresh });

            // DataGridView
            dgvCategories = new DataGridView()
            {
                Location = new Point(340, 60),
                Size = new Size(330, 440),
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvCategories.SelectionChanged += DgvCategories_SelectionChanged;

            this.Controls.AddRange(new Control[] { lblTitle, inputPanel, dgvCategories });
        }

        private void LoadCategories()
        {
            DataTable dt = DatabaseHelper.GetDataTable("SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryName");
            dgvCategories.DataSource = dt;
            dgvCategories.Columns["CategoryID"].Visible = false;
        }

        private void DgvCategories_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count > 0)
            {
                selectedId = Convert.ToInt32(dgvCategories.SelectedRows[0].Cells["CategoryID"].Value);
                txtName.Text = dgvCategories.SelectedRows[0].Cells["CategoryName"].Value.ToString();
                txtDescription.Text = dgvCategories.SelectedRows[0].Cells["Description"].Value.ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Category name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (selectedId == 0)
                {
                    string query = "INSERT INTO Categories (CategoryName, Description) VALUES (@name, @desc)";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@desc", txtDescription.Text)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Category added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string query = "UPDATE Categories SET CategoryName=@name, Description=@desc WHERE CategoryID=@id";
                    SqlParameter[] parameters = {
                        new SqlParameter("@name", txtName.Text),
                        new SqlParameter("@desc", txtDescription.Text),
                        new SqlParameter("@id", selectedId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Category updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Select a category to delete", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Delete this category?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Categories WHERE CategoryID=@id",
                        new SqlParameter[] { new SqlParameter("@id", selectedId) });
                    MessageBox.Show("Category deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            selectedId = 0;
            txtName.Clear();
            txtDescription.Clear();
        }
    }
}