using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public class PlaceholderTextBox : TextBox
    {
        private string placeholderText;
        private bool isPlaceholderActive = true;

        public string PlaceholderText
        {
            get { return placeholderText; }
            set
            {
                placeholderText = value;
                if (isPlaceholderActive && string.IsNullOrEmpty(this.Text))
                {
                    this.Text = placeholderText;
                    this.ForeColor = Color.Gray;
                }
            }
        }

        public PlaceholderTextBox()
        {
            this.Enter += OnEnter;
            this.Leave += OnLeave;
        }

        private void OnEnter(object sender, EventArgs e)
        {
            if (isPlaceholderActive && this.Text == placeholderText)
            {
                this.Text = "";
                this.ForeColor = Color.Black;
                isPlaceholderActive = false;
            }
        }

        private void OnLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                this.Text = placeholderText;
                this.ForeColor = Color.Gray;
                isPlaceholderActive = true;
            }
        }

        public new string Text
        {
            get { return isPlaceholderActive ? "" : base.Text; }
            set { base.Text = value; }
        }
    }
}