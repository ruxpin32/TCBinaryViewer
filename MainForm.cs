using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TCBinaryViewer
{
	public partial class MainForm : Form
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			this.Icon = Icon.FromHandle(Properties.Resources.icon.GetHicon());

			// Try to load a file from the command line.
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				SetFile(args[1]);
			}
			else
			{
				c_text.Text = "Drop a file onto this window to view its contents.";
			}
		}

		/// <summary>
		/// Drag and Drop filter, allows only file drops.
		/// </summary>
		private void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
		}

		/// <summary>
		/// Drag and drop filter, processes file drops.
		/// </summary>
		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (files != null && files.Length > 0)
			{
				SetFile(files[0]);
			}
		}

		/// <summary>
		/// Reads the contents of the file specified, and if successful passes the data to the display method.
		/// </summary>
		/// <param name="path"></param>
		public void SetFile(string path)
		{
			// Read all data in the file.
			try
			{
				byte[] data = File.ReadAllBytes(path);
				if (data != null)
				{
					this.Text = "TC Binary Viewer - " + path;
					DisplayBytes(data);
					return;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Unable to load the contents of the file!", "Error Loading", MessageBoxButtons.OK, MessageBoxIcon.Error);
				c_text.Text = "Drop a file onto this window to view its contents.";
			}

			this.Text = "TC Binary Viewer";
			c_text.Text = "Drop a file onto this window to view its contents.";
		}

		/// <summary>
		/// Displays the actual bytes in the panel.
		/// </summary>
		private void DisplayBytes(byte[] data)
		{
			// Digits used for display.
			char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

			// Create an appropriately large data buffer.
			char[] buffer = new char[(84 * (data.Length / 16)) + 100];

			// Dump all data to the string.
			int src = 0;
			int dst = 0;
			while (src < data.Length)
			{
				// Take this one row at a time.
				int rowOffset = src;
				int rowLength = data.Length - rowOffset;
				int i = 0;

				// Row address.
				UInt32 address = (UInt32) rowOffset;
				UInt32 mask    = 0xF0000000;
				int    shift   = 28;
				for (i = 0; i < 8; ++i)
				{
					int value = (int) (address & mask) >> shift;
					buffer[dst++] = digits[value];
					mask >>= 4;
					shift -= 4;
				}

				// Row bytes.
				buffer[dst++] = ' ';
				buffer[dst++] = ' ';
				for (i = 0; i < rowLength && i < 16; ++i)
				{
					Byte value = data[src++];
					buffer[dst++] = digits[(value & 0xF0) >> 4];
					buffer[dst++] = digits[(value & 0x0F)];

					if (i == 7)
						buffer[dst++] = '-';
					else
						buffer[dst++] = ' ';
				}
				for (; i < 16; ++i)
				{
					buffer[dst++] = ' ';
					buffer[dst++] = ' ';
					buffer[dst++] = ' ';
				}
				src = rowOffset;

				// Row ASCII.
				buffer[dst++] = ' ';
				buffer[dst++] = ' ';
				buffer[dst++] = ' ';
				for (i = 0; i < rowLength && i < 16; ++i)
				{
					Byte value = data[src++];
					if (value >= ' ' && value <= '~')
						buffer[dst++] = (char) value;
					else
						buffer[dst++] = '.';
				}
				for (; i < 16; ++i)
					buffer[dst++] = ' ';

				// End of line.
				buffer[dst++] = '\r';
				buffer[dst++] = '\n';

				src += 16;
			}
			buffer[dst] = '\0';

			// Convert to a string and display.
			c_text.Text = new string(buffer);
		}
	}
}
