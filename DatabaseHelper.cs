using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class DatabaseHelper
    {
        // Use your correct SQL Server instance
        private static string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=InventoryDBs;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            try
            {
                return new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed: " + ex.Message + "\n\nUsing: " + connectionString,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static DataTable GetDataTable(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static DataTable GetDashboardStats()
        {
            string query = @"SELECT 
                ISNULL((SELECT COUNT(*) FROM Products), 0) as TotalProducts,
                ISNULL((SELECT COUNT(*) FROM Categories), 0) as TotalCategories,
                ISNULL((SELECT COUNT(*) FROM Suppliers), 0) as TotalSuppliers,
                ISNULL((SELECT SUM(Quantity) FROM Products), 0) as TotalStock,
                ISNULL((SELECT SUM(Quantity * UnitPrice) FROM Products), 0) as TotalValue,
                ISNULL((SELECT COUNT(*) FROM Products WHERE Quantity <= ReorderLevel), 0) as LowStockCount";

            return GetDataTable(query);
        }
    }
}