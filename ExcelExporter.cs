using System;
using System.Data;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class ExcelExporter
    {
        public static void ExportToCSV(DataGridView dataGridView, string sheetName = "Report")
        {
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No data to export!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.Title = "Export to CSV";
            sfd.FileName = $"{sheetName}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        for (int i = 0; i < dataGridView.Columns.Count; i++)
                        {
                            sw.Write(dataGridView.Columns[i].HeaderText);
                            if (i < dataGridView.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();

                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            for (int i = 0; i < dataGridView.Columns.Count; i++)
                            {
                                string value = row.Cells[i].Value?.ToString() ?? "";
                                if (value.Contains(",")) value = "\"" + value + "\"";
                                sw.Write(value);
                                if (i < dataGridView.Columns.Count - 1) sw.Write(",");
                            }
                            sw.WriteLine();
                        }
                    }
                    MessageBox.Show($"Exported to CSV successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void ExportToExcel(DataGridView dataGridView, string sheetName = "Report")
        {
            ExportToCSV(dataGridView, sheetName);
        }
    }
}