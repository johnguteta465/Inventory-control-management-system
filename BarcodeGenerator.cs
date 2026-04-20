using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace InventoryManagementSystem
{
    public class BarcodeGenerator
    {
        public static Image GenerateBarcode(string productCode, int width = 300, int height = 150)
        {
            try
            {
                if (string.IsNullOrEmpty(productCode))
                    return null;

                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = width,
                        Height = height,
                        Margin = 10,
                        PureBarcode = false
                    }
                };

                var pixelData = writer.Write(productCode);

                using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
                {
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                    try
                    {
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                            pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                    return new Bitmap(bitmap);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void PrintBarcodeLabel(string productCode, string productName, string price)
        {
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrintPage += (sender, e) =>
                {
                    Image barcode = GenerateBarcode(productCode, 250, 100);
                    if (barcode != null)
                    {
                        e.Graphics.DrawString(productName, new Font("Arial", 12, FontStyle.Bold),
                            Brushes.Black, 50, 50);
                        e.Graphics.DrawImage(barcode, 50, 100, 250, 100);
                        e.Graphics.DrawString("Price: ₱" + price, new Font("Arial", 10),
                            Brushes.Black, 50, 220);
                        e.Graphics.DrawString(productCode, new Font("Arial", 8, FontStyle.Italic),
                            Brushes.Gray, 50, 250);
                    }
                };

                PrintDialog pd = new PrintDialog();
                pd.Document = printDoc;
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}