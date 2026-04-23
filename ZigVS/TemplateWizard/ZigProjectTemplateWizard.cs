namespace ZigVS.TemplateWizard
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TemplateWizard;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows;

    public sealed class ZigProjectTemplateWizard : IWizard
    {
        private const string DestinationDirectoryReplacementKey = "$destinationdirectory$";
        private const string SafeProjectNameReplacementKey = "$safeprojectname$";
        private const string SafeItemNameReplacementKey = "$safeitemname$";
        private const string ZigFingerprintReplacementKey = "$zigfingerprint$";
        private static readonly Regex s_FingerprintRegex = new Regex(@"\.fingerprint\s*=\s*(0x[0-9a-fA-F]{16})", RegexOptions.Compiled);

        private DTE? m_Dte;
        private string? m_DestinationDirectoryPathString;
        private string? m_GeneratedProjectDirectoryPathString;
        private string? m_GeneratedProjectFilePathString;
        private WizardRunKind m_RunKind;
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (project == null)
                {
                    return;
                }

                try
                {
                    m_GeneratedProjectFilePathString = project.FullName;
                    if (!string.IsNullOrEmpty(m_GeneratedProjectFilePathString))
                    {
                        m_GeneratedProjectDirectoryPathString = Path.GetDirectoryName(m_GeneratedProjectFilePathString);
                    }
                }
                catch
                {
                    // Fall back to the replacement dictionary values captured during RunStarted.
                }
            });
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
            if (m_RunKind != WizardRunKind.AsNewProject)
            {
                return;
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(SwitchToGeneratedFolderMode));
                return;
            }

            SwitchToGeneratedFolderMode();
        }

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            m_Dte = automationObject as DTE;
            m_RunKind = runKind;
            replacementsDictionary[ZigFingerprintReplacementKey] = CreatePackageFingerprintLiteral(replacementsDictionary);
            if (replacementsDictionary.TryGetValue(DestinationDirectoryReplacementKey, out string l_DestinationDirectoryPathString))
            {
                m_DestinationDirectoryPathString = l_DestinationDirectoryPathString;
            }

            if (replacementsDictionary.TryGetValue(SafeProjectNameReplacementKey, out string l_SafeProjectNameString) &&
                !string.IsNullOrEmpty(m_DestinationDirectoryPathString))
            {
                m_GeneratedProjectFilePathString = Path.Combine(m_DestinationDirectoryPathString, l_SafeProjectNameString + ".zigproj");
                m_GeneratedProjectDirectoryPathString = m_DestinationDirectoryPathString;
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        private static string CreatePackageFingerprintLiteral(Dictionary<string, string> replacementsDictionary)
        {
            if (TryCreatePackageFingerprintLiteral(replacementsDictionary, out string l_FingerprintLiteralString))
            {
                return l_FingerprintLiteralString;
            }

            // Deliberately omit a valid identity if we cannot resolve zig.exe here.
            // Zig will report the exact suggested value during the first build.
            return "0x0000000000000001";
        }

        private static bool TryCreatePackageFingerprintLiteral(
            Dictionary<string, string> replacementsDictionary,
            out string o_FingerprintLiteralString)
        {
            o_FingerprintLiteralString = string.Empty;
            string? l_packageNameString = TryGetPackageName(replacementsDictionary);
            if (string.IsNullOrWhiteSpace(l_packageNameString))
            {
                return false;
            }

            string? l_zigPathString = TryResolveZigExecutablePath();
            if (string.IsNullOrWhiteSpace(l_zigPathString) || !File.Exists(l_zigPathString))
            {
                return false;
            }

            string l_temporaryRootPathString = Path.Combine(Path.GetTempPath(), "ZigVS", "Fingerprint", Guid.NewGuid().ToString("N"));
            string l_temporaryPackagePathString = Path.Combine(l_temporaryRootPathString, l_packageNameString);
            try
            {
                Directory.CreateDirectory(l_temporaryPackagePathString);
                System.Diagnostics.ProcessStartInfo l_processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = l_zigPathString,
                    Arguments = "init",
                    WorkingDirectory = l_temporaryPackagePathString,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using (System.Diagnostics.Process l_process = new System.Diagnostics.Process())
                {
                    l_process.StartInfo = l_processStartInfo;
                    if (!l_process.Start())
                    {
                        return false;
                    }

                    l_process.WaitForExit(5000);
                    if (!l_process.HasExited || l_process.ExitCode != 0)
                    {
                        try
                        {
                            if (!l_process.HasExited)
                            {
                                l_process.Kill();
                            }
                        }
                        catch
                        {
                        }

                        return false;
                    }
                }

                string l_manifestPathString = Path.Combine(l_temporaryPackagePathString, "build.zig.zon");
                if (!File.Exists(l_manifestPathString))
                {
                    return false;
                }

                string l_manifestTextString = File.ReadAllText(l_manifestPathString);
                Match l_match = s_FingerprintRegex.Match(l_manifestTextString);
                if (!l_match.Success)
                {
                    return false;
                }

                o_FingerprintLiteralString = l_match.Groups[1].Value;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                TryDeleteDirectory(l_temporaryRootPathString);
            }
        }

        private static string? TryGetPackageName(Dictionary<string, string> replacementsDictionary)
        {
            if (replacementsDictionary.TryGetValue(SafeProjectNameReplacementKey, out string l_ProjectNameString) &&
                !string.IsNullOrWhiteSpace(l_ProjectNameString))
            {
                return l_ProjectNameString;
            }

            if (replacementsDictionary.TryGetValue(SafeItemNameReplacementKey, out string l_ItemNameString) &&
                !string.IsNullOrWhiteSpace(l_ItemNameString))
            {
                return l_ItemNameString;
            }

            return null;
        }

        private static string? TryResolveZigExecutablePath()
        {
            try
            {
                GeneralOptions l_generalOptions = ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    return await GeneralOptions.GetLiveInstanceAsync();
                });

                string? l_resolvedConfiguredPathString = Utilities.ResolvePath(l_generalOptions.ToolPathExpanded);
                if (!string.IsNullOrWhiteSpace(l_resolvedConfiguredPathString))
                {
                    return l_resolvedConfiguredPathString;
                }
            }
            catch
            {
            }

            return Utilities.ResolvePath(Parameter.c_compilerFileName);
        }

        private static void TryDeleteDirectory(string i_DirectoryPathString)
        {
            if (string.IsNullOrWhiteSpace(i_DirectoryPathString))
            {
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (!Directory.Exists(i_DirectoryPathString))
                    {
                        return;
                    }

                    Directory.Delete(i_DirectoryPathString, recursive: true);
                    return;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

                System.Threading.Thread.Sleep(50);
            }
        }

        private string? GetGeneratedProjectDirectoryPath()
        {
            if (!string.IsNullOrEmpty(m_GeneratedProjectDirectoryPathString))
            {
                return m_GeneratedProjectDirectoryPathString;
            }

            if (!string.IsNullOrEmpty(m_GeneratedProjectFilePathString))
            {
                m_GeneratedProjectDirectoryPathString = Path.GetDirectoryName(m_GeneratedProjectFilePathString);
                return m_GeneratedProjectDirectoryPathString;
            }

            return m_DestinationDirectoryPathString;
        }

        private string? GetGeneratedProjectFilePath()
        {
            if (!string.IsNullOrEmpty(m_GeneratedProjectFilePathString))
            {
                return m_GeneratedProjectFilePathString;
            }

            if (!string.IsNullOrEmpty(m_DestinationDirectoryPathString))
            {
                string[] l_ProjectFiles = Directory.GetFiles(m_DestinationDirectoryPathString, "*.zigproj", SearchOption.TopDirectoryOnly);
                if (l_ProjectFiles.Length > 0)
                {
                    m_GeneratedProjectFilePathString = l_ProjectFiles[0];
                }
            }

            return m_GeneratedProjectFilePathString;
        }

        private static void TryDeleteGeneratedProjectFile(string i_ProjectFilePathString)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (!File.Exists(i_ProjectFilePathString))
                    {
                        return;
                    }

                    File.Delete(i_ProjectFilePathString);
                    return;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        private void SwitchToGeneratedFolderMode()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                string? l_projectFilePathString = GetGeneratedProjectFilePath();
                string? l_projectDirectoryPathString = GetGeneratedProjectDirectoryPath();
                if (string.IsNullOrEmpty(l_projectDirectoryPathString) || !Directory.Exists(l_projectDirectoryPathString))
                {
                    return;
                }

                try
                {
                    m_Dte?.ExecuteCommand("File.CloseSolution");
                }
                catch
                {
                    // Ignore if no project solution is currently open.
                }

                if (l_projectFilePathString is string l_ProjectFilePathString && l_ProjectFilePathString.Length > 0)
                {
                    TryDeleteGeneratedProjectFile(l_ProjectFilePathString);
                }

                try
                {
                    m_Dte?.ExecuteCommand("File.OpenFolder", l_projectDirectoryPathString);
                }
                catch
                {
                    // Keep the generated files even if folder-mode activation fails.
                }
            });
        }
    }
}
