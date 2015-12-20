﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;

namespace OutlookAddInTest
{
	[ComVisible(true)]

	public class MainRibbon : Office.IRibbonExtensibility
	{
		private Office.IRibbonUI ribbon;
		private MainForm CompaniesAndContacts;
		private LoadingForm loadingForm;

		public MainRibbon()
		{
		}

		#region IRibbonExtensibility Members

		public string GetCustomUI(string ribbonID)
		{
			return GetResourceText("OutlookAddInTest.Ribbon1.xml");
		}

		#endregion

		#region Ribbon Callbacks

		public void Ribbon_Load(Office.IRibbonUI ribbonUI)
		{
			this.ribbon = ribbonUI;
		}

		#endregion

		public void OnTextButton(Office.IRibbonControl control)
		{
			Outlook.MailItem item = getMailItem();
			if (item != null)
			{
				loadingForm = new LoadingForm();
				loadingForm.Show();

				BackgroundWorker bw = new BackgroundWorker();
				bw.DoWork += new DoWorkEventHandler(bw_DoWork);
				bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

				try
				{
					bw.RunWorkerAsync();
					loadingForm.Show();
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
		}

		void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			Outlook.MailItem item = getMailItem();
			if (item != null)
			{
				CompaniesAndContacts = new MainForm(item);
			}
		}
		void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			loadingForm.Close();
			CompaniesAndContacts.Show();
		}

		private Outlook.MailItem getMailItem()
		{
			try
			{
				Object explorer = Globals.PhoenixPlugin.Application.ActiveWindow().Selection[1];
				if (explorer is Outlook.MailItem)
				{
					return (Outlook.MailItem)explorer;
				}
				System.Windows.Forms.MessageBox.Show("The item you selected was not an email item.");
				return null;
			}
			catch (System.Runtime.InteropServices.COMException)
			{
				System.Windows.Forms.MessageBox.Show("You must select an Email first.");
				return null;
			}
		}

		public void OnRemoveButton(Office.IRibbonControl control)
		{
			Outlook.MailItem mailItem = getMailItem();
                      
			if (mailItem != null)
			{
                //API.Remove();
                mailItem.MessageClass = "IPM.Note";
				mailItem.Save();
			}
		}

		#region Helpers

		private static string GetResourceText(string resourceName)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string[] resourceNames = asm.GetManifestResourceNames();
			for (int i = 0; i < resourceNames.Length; ++i)
			{
				if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
				{
					using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
					{
						if (resourceReader != null)
						{
							return resourceReader.ReadToEnd();
						}
					}
				}
			}
			return null;
		}

		#endregion
	}
}
