using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; private set; } = false;

        public static void ToggleTheme(Form form)
        {
            IsDarkMode = !IsDarkMode;
            ApplyTheme(form, IsDarkMode);
        }

        public static void ApplyTheme(Control control, bool dark)
        {
            Color backColor = dark ? Color.FromArgb(30, 30, 46) : Color.FromArgb(240, 240, 240);
            Color foreColor = dark ? Color.White : Color.Black;
            control.BackColor = backColor;
            control.ForeColor = foreColor;

            foreach (Control child in control.Controls)
            {
                ApplyTheme(child, dark);
                // Special handling for DataGridView
                if (child is DataGridView dgv)
                {
                    dgv.BackgroundColor = dark ? Color.FromArgb(40, 40, 50) : Color.White;
                    dgv.GridColor = dark ? Color.FromArgb(60, 60, 70) : Color.LightGray;
                    dgv.DefaultCellStyle.BackColor = dark ? Color.FromArgb(30, 30, 46) : Color.White;
                    dgv.DefaultCellStyle.ForeColor = dark ? Color.White : Color.Black;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = dark ? Color.FromArgb(20, 20, 30) : Color.LightGray;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = dark ? Color.White : Color.Black;
                    dgv.RowsDefaultCellStyle.SelectionBackColor = dark ? Color.FromArgb(60, 120, 210) : Color.FromArgb(52, 152, 219);
                    dgv.RowsDefaultCellStyle.SelectionForeColor = dark ? Color.White : Color.White;
                }
                // Keep status strip or menus
                if (child is MenuStrip || child is StatusStrip) continue;
            }
        }

        public static void ConfigureDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;

            // Basic DataGridView Properties
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.White;
            dgv.GridColor = Color.FromArgb(230, 235, 240);

            // Column and Row Sizing - Professional Settings
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgv.RowTemplate.Height = 35;
            dgv.AllowUserToResizeColumns = true;
            dgv.AllowUserToResizeRows = false;
            dgv.EnableHeadersVisualStyles = false;

            // Column Headers Styling - Professional Blue Theme
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeight = 42;

            // Default Cell Styling - Clean and Professional
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219, 40);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(44, 62, 80);
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Alternating Row Colors for Better Readability
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219, 40);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(44, 62, 80);

            // Row Header Styling
            dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250);
            dgv.RowHeadersDefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgv.RowHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            // Scrolling and Performance
            dgv.ScrollBars = ScrollBars.Both;
            dgv.AutoGenerateColumns = true;
            dgv.VirtualMode = false;

            // Professional Appearance Settings
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        }
    }
}