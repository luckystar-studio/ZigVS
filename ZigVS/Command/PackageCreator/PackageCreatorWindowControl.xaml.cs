namespace ZigVS
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Forms;
    using System.Linq;

#nullable enable
    public partial class PackageCreatorWindowControl : System.Windows.Controls.UserControl
    {
        PackageCreator m_PackageCreator = new PackageCreator();

        bool m_DirectoryBool = false;
        bool m_PackageNameBool = false;
        bool m_OpenBool = false;

        System.Windows.Media.Brush m_greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0x84, 0x00));

        public PackageCreatorWindowControl()
        {
            InitializeComponent();

            m_PackageCreatorWindowControl = this;

            m_DoNotOpen_Radio_Button.Foreground = m_Directory_TextBlock.Foreground;
            m_DoNotOpen_Radio_Button.Background = m_Directory_TextBlock.Background;
            m_Open_Radio_Button.Foreground = m_Directory_TextBlock.Foreground;
            m_Open_Radio_Button.Background = m_Directory_TextBlock.Background;

            Reset();
        }

        static PackageCreatorWindowControl? m_PackageCreatorWindowControl = null;
        static public PackageCreatorWindowControl? GetInstance()
        {
            return m_PackageCreatorWindowControl;
        }

        public void Reset()
        {
            UpdateCreatorButtonText();
        }

        void UpdateCreatorButtonText()
        {
            if (m_PackageCreator.GetState() == InstallerBase.State.Installing)
            {
                m_Create_Button.Content = "Cancel";
            }
            else
            {
                m_Create_Button.Content = "Create";
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void m_Directory_Button_Click(object sender, RoutedEventArgs e)
        {
            var l_FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (l_FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                m_DirecotryPath_TextBox.Text = l_FolderBrowserDialog.SelectedPath;
                m_DirectoryBool = true;
                m_One_TextBlock.Foreground = m_greenBrush;
                Check_Status();
            }
        }

        private void m_DirecotryPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool l_ifPathIsValid = false;
            try
            {
                if (Path.IsPathRooted(m_DirecotryPath_TextBox.Text) && Directory.Exists(m_DirecotryPath_TextBox.Text))
                {
                    var l_path = Path.GetFullPath(m_DirecotryPath_TextBox.Text);
                    l_ifPathIsValid = true;
                }

            }
            catch { }

            m_DirectoryBool = l_ifPathIsValid;
            m_One_TextBlock.Foreground = m_DirectoryBool ? m_greenBrush : m_Directory_TextBlock.Foreground;
            Check_Status();
        }

        private void m_PackageName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_PackageNameBool = IsValidDirectoryName(m_PackageName_TextBox.Text);
            m_Two_TextBlock.Foreground = m_PackageNameBool ? m_greenBrush : m_Directory_TextBlock.Foreground;
            Check_Status();
        }

        private bool IsValidDirectoryName( string l_directoryNameString )
        {
            if(String.IsNullOrEmpty(m_PackageName_TextBox.Text))
            {
                return false;
            }

            char[] l_additionalInvalidCharArray = { '.' };
            char[] l_invalidCharArray = l_additionalInvalidCharArray.Concat(Path.GetInvalidPathChars()).Concat(Path.GetInvalidFileNameChars()).ToArray();

            foreach (char l_char in l_invalidCharArray )
            {
                if (l_directoryNameString.Contains(l_char.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        private void m_DoNotOpen_Radio_Button_Checked(object sender, RoutedEventArgs e)
        {
            m_OpenBool = true;
            m_Three_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        private void m_Open_Radio_Button_Checked(object sender, RoutedEventArgs e)
        {
            m_OpenBool = true;
            m_Three_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void Check_Status()
        {
            m_Create_Button.IsEnabled = m_DirectoryBool && m_PackageNameBool && m_OpenBool;
        }

        private void m_Create_Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_PackageCreator.GetState() == InstallerBase.State.None)
            {
                bool l_checked = (m_Open_Radio_Button.IsChecked != null) && m_Open_Radio_Button.IsChecked.Value;
#pragma warning disable VSTHRD010
                m_PackageCreator.Start(m_DirecotryPath_TextBox.Text, m_PackageName_TextBox.Text, l_checked);
                ((PackageCreatorWindow)this.Parent).Close();
#pragma warning restore VSTHRD010
            }
            else if (m_PackageCreator.GetState() == InstallerBase.State.Installing)
            {
                m_PackageCreator.CancelInstallation();
            }
            UpdateCreatorButtonText();
        }
    }
}
