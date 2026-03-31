namespace ZigVS
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.Web.WebView2.Core;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

#nullable enable
#pragma warning disable VSTHRD002,VSTHRD010
    /// <summary>
    /// Interaction logic for PackageBrowserWindowControl.xaml
    /// </summary>
    public partial class PackageInstallerWindowControl : UserControl
    {
        PackageInstaller m_PackageInstaller = new PackageInstaller();

        PackageInstallerWindowViewModel m_PackageInstallerWindowViewModel = new PackageInstallerWindowViewModel();

        System.Windows.Media.Brush m_greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0x84, 0x00));

        bool m_repositoryBool = false;
        bool m_methodBool = false;
        bool m_refreshingMethodsBool = false;

        string m_zipUrlString = "";
        string m_targzUrlString = "";
        string m_gitUrlString = "";
        ProjectDependencyAddRequest? m_projectDependencyAddRequest = null;

        string m_workingDirectoryPathString = "";

        const string c_targzExtensionString = ".tar.gz";
        const string c_zipExtensionString = ".zip";

        const string c_methodGit = "git";
        const string c_methodUnzip = "unzip";
        const string c_methodZigFetch = "zig fetch (Open Folder Mode)";
        const string c_methodProjectDependencies = "Project Dependencies (.zigproj)";

        static string? s_requestedMethodString = null;

        public PackageInstallerWindowControl()
        {
            m_PackageInstallerWindowControl = this;
            InitializeComponent();

            DataContext = m_PackageInstallerWindowViewModel;

#pragma warning disable CS1998, CS4014
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                InitializeWebViewAsync();
            });
#pragma warning restore CS1998, CS4014

            RefreshMethodItems();
            Reset();
        }

        static PackageInstallerWindowControl? m_PackageInstallerWindowControl = null;
        static public PackageInstallerWindowControl? GetInstance()
        {
            return m_PackageInstallerWindowControl;
        }

        bool SwitchToUIThreadIfRequired(Action i_action)
        {
            if (Dispatcher.CheckAccess())
            {
                return false;
            }

            _ = Dispatcher.BeginInvoke(i_action);
            return true;
        }

        public void Reset()
        {
            if (SwitchToUIThreadIfRequired(Reset))
            {
                return;
            }

            RefreshMethodItems();
            UpdateInstallerButtonText();
            CheckStatus();
        }

        public static void RequestManagedProjectDependencyMode()
        {
            s_requestedMethodString = c_methodProjectDependencies;
            GetInstance()?.SelectManagedProjectDependencyMethod();
        }

        public void SelectManagedProjectDependencyMethod()
        {
            if (SwitchToUIThreadIfRequired(SelectManagedProjectDependencyMethod))
            {
                return;
            }

            s_requestedMethodString = c_methodProjectDependencies;
            RefreshMethodItems();
            CheckStatus();
        }

        void UpdateInstallerButtonText()
        {
            if (SwitchToUIThreadIfRequired(UpdateInstallerButtonText))
            {
                return;
            }

            if (m_PackageInstaller.GetState() == InstallerBase.State.Installing)
            {
                m_PackageInstallerWindowViewModel.InstallButtonTextString = "Cancel";
            }
            else
            {
                m_PackageInstallerWindowViewModel.InstallButtonTextString = "Install";
            }
        }

        async Task InitializeWebViewAsync()
        {
            // create webview2 environment and load the webview
            string l_webViewDirectoryString = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            l_webViewDirectoryString = System.IO.Path.Combine(l_webViewDirectoryString, "LuckyStarStudio", "ZigVS");
            System.IO.Directory.CreateDirectory(l_webViewDirectoryString);

            var l_CoreWebView2Environment = await CoreWebView2Environment.CreateAsync(null, l_webViewDirectoryString);

            // load the webview2
            await m_WebView2.EnsureCoreWebView2Async(l_CoreWebView2Environment);

            GoHome();
        }

        void Home_Click(object sender, RoutedEventArgs e)
        {
            GoHome();
        }
        
        void GoHome()
        {
            var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                                    return await GeneralOptions.GetLiveInstanceAsync();
                                });
            m_WebView2.Source = new Uri(l_GeneralOptions.HomeUrl);
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            if(m_WebView2.CanGoBack)
            {
                m_WebView2.GoBack();
            }
        }

        void Forward_Click(object sender, RoutedEventArgs e)
        {
            if(m_WebView2.CanGoForward)
            {
                m_WebView2.GoForward();
            }
        }

        void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckStatus();
        }

        void webView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            m_URL_TextBlock.Text = m_WebView2.Source.ToString();
            CheckStatus();
        }

        void Method_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_refreshingMethodsBool)
            {
                return;
            }

            CheckStatus();
        }

        void Install_button_Click(object sender, RoutedEventArgs e)
        {
            CheckStatus();

            if (m_PackageInstallerWindowViewModel.InstallButtonEnabled)
            {
                if (m_PackageInstaller.GetState() == InstallerBase.State.None)
                {
                    var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                        return await GeneralOptions.GetLiveInstanceAsync();
                    });

                    if (string.IsNullOrEmpty(m_Method_ComboBox.Text))
                    {
                    }
                    else if (m_Method_ComboBox.Text == c_methodGit)
                    {
                        m_PackageInstaller.StartCommand(m_workingDirectoryPathString, l_GeneralOptions.GitPathExpanded, l_GeneralOptions.GitOption + ' ' + m_gitUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodUnzip)
                    {
                        m_PackageInstaller.StartUnzip(m_workingDirectoryPathString, m_zipUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodZigFetch && Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
                    {
                        var l_toolPathString = l_GeneralOptions.ToolPathExpanded;
                        m_PackageInstaller.StartCommand(m_workingDirectoryPathString, l_toolPathString, l_GeneralOptions.ZigFetchOption + ' ' + m_targzUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodProjectDependencies)
                    {
                        var l_ProjectNode = Utilities.GetCachedActiveZigVSProjectNode();
                        if (l_ProjectNode != null && m_projectDependencyAddRequest != null)
                        {
                            m_PackageInstaller.StartManagedDependencyInstall(l_ProjectNode, m_projectDependencyAddRequest);
                        }
                    }
                    UpdateInstallerButtonText();
                }
                else if (m_PackageInstaller.GetState() == InstallerBase.State.Installing)
                {
                    m_PackageInstaller.CancelInstallation();
                    Reset();
                }
            }
        }

        void CheckStatus()
        {
            if (SwitchToUIThreadIfRequired(CheckStatus))
            {
                return;
            }

            try
            {
                RefreshMethodItems();
                GetDirectoryPath();
                CheckRepositoryStatus();
                CheckMethodStatus();
                CheckInstallationStatus();
            }
            catch { }
        }

        void RefreshMethodItems()
        {
            if (SwitchToUIThreadIfRequired(RefreshMethodItems))
            {
                return;
            }

            if (m_refreshingMethodsBool)
            {
                return;
            }

            m_refreshingMethodsBool = true;
            try
            {
                string l_selectedMethodString = m_Method_ComboBox.SelectedItem as string ?? m_Method_ComboBox.Text;
                ZigVSProjectNode? l_ProjectNode = Utilities.GetCachedActiveZigVSProjectNode();
                bool l_hasProjectContextBool = l_ProjectNode != null;
                m_Method_ComboBox.Items.Clear();

                m_Method_ComboBox.Items.Add(c_methodZigFetch);
                m_Method_ComboBox.Items.Add(c_methodGit);

                m_Method_ComboBox.Items.Add(c_methodUnzip);
                if (l_hasProjectContextBool)
                {
                    m_Method_ComboBox.Items.Add(c_methodProjectDependencies);
                }

                string? l_requestedMethodString = s_requestedMethodString;
                s_requestedMethodString = null;

                if (!string.IsNullOrWhiteSpace(l_requestedMethodString) && m_Method_ComboBox.Items.Contains(l_requestedMethodString))
                {
                    m_Method_ComboBox.SelectedItem = l_requestedMethodString;
                }
                else if (!string.IsNullOrWhiteSpace(l_selectedMethodString) && m_Method_ComboBox.Items.Contains(l_selectedMethodString))
                {
                    m_Method_ComboBox.SelectedItem = l_selectedMethodString;
                }
                else if (l_hasProjectContextBool && m_Method_ComboBox.Items.Contains(c_methodProjectDependencies))
                {
                    m_Method_ComboBox.SelectedItem = c_methodProjectDependencies;
                }
                else if (Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode && m_Method_ComboBox.Items.Contains(c_methodZigFetch))
                {
                    m_Method_ComboBox.SelectedItem = c_methodZigFetch;
                }
                else if (m_Method_ComboBox.Items.Count > 0)
                {
                    m_Method_ComboBox.SelectedIndex = 0;
                }
            }
            finally
            {
                m_refreshingMethodsBool = false;
            }
        }

        void GetDirectoryPath()
        {
            ZigVSProjectNode? l_ProjectNode = Utilities.GetCachedActiveZigVSProjectNode();
            if (l_ProjectNode != null)
            {
                m_workingDirectoryPathString = System.IO.Path.Combine(l_ProjectNode.ProjectFolder, "package");
            }
            else if (Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
            {
                var l_FolderModeOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                                        return await FolderModeOptions.GetLiveInstanceAsync();
                                    });
                m_workingDirectoryPathString = System.IO.Path.Combine(Utilities.GetOpenFolderPath(), l_FolderModeOptions.PackageDirectoryName);
            }
            else
            {
                m_workingDirectoryPathString = "";
            }
        }

        void CheckRepositoryStatus()
        {
            var l_repoTopRegex =    new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)/?$");
            var l_tagRegex =        new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)/tree/(?<tree>.+)$");
            var l_commitRegex =     new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)/commit/(?<commit>[^/]+)$");
            var l_repoTopMatch = l_repoTopRegex.Match(m_URL_TextBlock.Text);
            var l_tagMatch = l_tagRegex.Match(m_URL_TextBlock.Text);
            var l_commitMatch = l_commitRegex.Match(m_URL_TextBlock.Text);
            m_repositoryBool = l_repoTopMatch.Success || l_tagMatch.Success || l_commitMatch.Success;
            m_projectDependencyAddRequest = null;

            m_zipUrlString = "";
            m_targzUrlString = "";
            string l_tempUrlString = "";
            if (l_repoTopMatch.Success)
            {
                l_tempUrlString = $"https://github.com/{l_repoTopMatch.Groups["username"]}/{l_repoTopMatch.Groups["repo"]}/archive/refs/heads/master";
            } 
            else if(l_tagMatch.Success)
            {
                var l_branchString = l_tagMatch.Groups["tree"].ToString();
                if (Regex.IsMatch(l_branchString, @"^[0-9.]*$"))
                {
                    l_tempUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}/archive/refs/tags/{l_tagMatch.Groups["tree"]}";
                }
                else
                {
                    l_tempUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}/archive/refs/heads/{l_tagMatch.Groups["tree"]}";
                }
            }
            else if(l_commitMatch.Success)
            {
                l_tempUrlString = $"https://github.com/{l_commitMatch.Groups["username"]}/{l_commitMatch.Groups["repo"]}/archive/{l_commitMatch.Groups["commit"]}";
            }
            m_zipUrlString = l_tempUrlString + c_zipExtensionString;
            m_targzUrlString = l_tempUrlString + c_targzExtensionString;


            m_gitUrlString = "";
            if (l_repoTopMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_repoTopMatch.Groups["username"]}/{l_repoTopMatch.Groups["repo"]}.git";
                m_projectDependencyAddRequest = new ProjectDependencyAddRequest
                {
                    RepositoryUrl = m_gitUrlString,
                    SuggestedName = l_repoTopMatch.Groups["repo"].ToString()
                };
            }
            else if (l_tagMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}.git --branch {l_tagMatch.Groups["tree"]}";
                m_projectDependencyAddRequest = new ProjectDependencyAddRequest
                {
                    RepositoryUrl = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}.git",
                    ReferenceName = l_tagMatch.Groups["tree"].ToString(),
                    SuggestedName = l_tagMatch.Groups["repo"].ToString()
                };
            }
            else if (l_commitMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_commitMatch.Groups["username"]}/{l_commitMatch.Groups["repo"]}.git";
                m_projectDependencyAddRequest = new ProjectDependencyAddRequest
                {
                    RepositoryUrl = m_gitUrlString,
                    Commit = l_commitMatch.Groups["commit"].ToString(),
                    SuggestedName = l_commitMatch.Groups["repo"].ToString()
                };
            }

            m_One_TextBlock.Foreground = m_repositoryBool ? m_greenBrush : Text.Foreground;
        }

        void CheckMethodStatus()
        {
            m_methodBool = false;
            string l_displayString = "";

            var l_GeneralOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                return await GeneralOptions.GetLiveInstanceAsync();
            });

            if (string.IsNullOrEmpty(m_Method_ComboBox.Text))
            {
            }
            else if (m_Method_ComboBox.Text == c_methodGit)
            {
                m_methodBool = true;
                l_displayString = l_GeneralOptions.GitPathExpanded + ' ' + l_GeneralOptions.GitOption + ' ' + m_gitUrlString;
            }
            else if(m_Method_ComboBox.Text== c_methodUnzip)
            {
                m_methodBool = true;
                l_displayString = "Unzip(internal command) " + m_zipUrlString;
            }
            else if (m_Method_ComboBox.Text == c_methodZigFetch && Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
            {
                m_methodBool = true;
                l_displayString = l_GeneralOptions.ToolPathExpanded + ' ' + l_GeneralOptions.ZigFetchOption + ' ' + m_targzUrlString;
            }
            else if (m_Method_ComboBox.Text == c_methodProjectDependencies)
            {
                var l_ProjectNode = Utilities.GetCachedActiveZigVSProjectNode();
                m_methodBool = l_ProjectNode != null && m_projectDependencyAddRequest != null;
                if (l_ProjectNode == null)
                {
                    l_displayString = "Select a Zig project or dependency node in Solution Explorer.";
                }
                else if (m_projectDependencyAddRequest != null)
                {
                    string l_commitOrReference = !string.IsNullOrWhiteSpace(m_projectDependencyAddRequest.Commit)
                        ? m_projectDependencyAddRequest.Commit!
                        : (!string.IsNullOrWhiteSpace(m_projectDependencyAddRequest.ReferenceName)
                            ? m_projectDependencyAddRequest.ReferenceName!
                            : "HEAD");
                    l_displayString = $"Managed dependency add: {m_projectDependencyAddRequest.RepositoryUrl} @ {l_commitOrReference}";
                }
                else
                {
                    l_displayString = "Select a GitHub repository page first.";
                }
            }

            m_Two_TextBlock.Foreground = m_methodBool ? m_greenBrush : Text.Foreground;
            m_Command_TextBlock.Text = m_workingDirectoryPathString + " > " + l_displayString;
        }

        void CheckInstallationStatus()
        {
            if (Utilities.GetSolutionMode() == Utilities.SolutionMode.None)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Folder or project is not open. Not ready to install.";
            }
            else if (!m_repositoryBool)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Repository is not selected. Not ready to install.";
            }
            else if (!m_methodBool)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Method is not selected. Not ready to install.";
            }
            else
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = true;
                m_Warning_TextBlock.Text = "";
            }
        }
    }

    public class PackageInstallerWindowViewModel : INotifyPropertyChanged
    {
        private string m_InstallButtonTextString = "";

        public string InstallButtonTextString
        {
            get => m_InstallButtonTextString;
            set
            {
                if (m_InstallButtonTextString != value)
                {
                    m_InstallButtonTextString = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool m_InstallButtonEnabled = false;
        public bool InstallButtonEnabled
        {
            get => m_InstallButtonEnabled;
            set
            {
                if (m_InstallButtonEnabled != value)
                {
                    m_InstallButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string i_propertyNameString = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(i_propertyNameString));
        }
    }
#pragma warning restore VSTHRD002,VSTHRD010
}
