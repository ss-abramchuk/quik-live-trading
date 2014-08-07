using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Helpers;
using WLDSolutions.QUIKLiveTrading.Dispatcher;
using WLDSolutions.QUIKLiveTrading.Abstract;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO;

namespace WLDSolutions.QUIKLiveTrading.Settings
{
    public partial class QUIKSettingsPanel : LTSettingsPanel
    {
        public QUIKSettingsPanel()
        {
            InitializeComponent();

            cbSettings.SelectedIndex = 0;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            switch (cbSettings.SelectedIndex)
            {
                case (0):
                    QUIKGeneralSettings quikGeneralSettings = new QUIKGeneralSettings(QUIKDispatcher.Instance.SettingsProvider);

                    quikGeneralSettings.MdiParent = GetParentForm();

                    quikGeneralSettings.Show();
                    quikGeneralSettings.Activate();

                    break;

                case (1):
                    QUIKDataImportSettings quikDataImportSettings = new QUIKDataImportSettings(QUIKDispatcher.Instance.SettingsProvider);

                    quikDataImportSettings.MdiParent = GetParentForm();

                    quikDataImportSettings.Show();
                    quikDataImportSettings.Activate();

                    break;
            }
        }


        private Form GetParentForm()
        {
            Form result = null;

            foreach (Form form in Application.OpenForms)
            {
                if (form.Name == "MainForm")
                    result = form;
            }

            return result;
        }
    }
}
