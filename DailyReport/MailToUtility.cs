using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyReport
{
    class MailToUtility
    {
        public static string SendMailToURI(List<string> vAddress, List<string> vCC, List<string> vBCC, string strSubject, string strBody)
        {
            string strURL = @"mailto:";

            try
            {
                // address
                if (vAddress != null && vAddress.Count > 0)
                {
                    string strAddress = string.Join(";", vAddress);
                    if (!string.IsNullOrEmpty(strAddress))
                        strURL += Uri.EscapeUriString(strAddress);
                }
                strURL += @"?";

                /// Encoding see:
                /// https://stackoverflow.com/a/1517709/3576052
                /// 
                List<string> arguments = new List<string>();

                // subject
                if (!string.IsNullOrEmpty(strSubject))
                    arguments.Add(@"subject=" + Uri.EscapeDataString(strSubject));

                // body
                if (!string.IsNullOrEmpty(strBody))
                    arguments.Add(@"body=" + Uri.EscapeDataString(strBody));

                // CC
                if (vCC != null && vCC.Count > 0)
                {
                    string strCC = string.Join(";", vCC);
                    if (!string.IsNullOrEmpty(strCC))
                        arguments.Add(@"cc=" + Uri.EscapeUriString(strCC));
                }

                // BCC        
                if (vBCC != null && vBCC.Count > 0)
                {
                    string strBCC = string.Join(";", vBCC);
                    if (!string.IsNullOrEmpty(strBCC))
                        arguments.Add(@"bcc=" + Uri.EscapeUriString(strBCC));
                }

                strURL += string.Join("&", arguments);
                System.Diagnostics.Process.Start(strURL);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return string.Empty;
        }

        public static bool IsMailClientInstalledRoot()
        {
            bool bReturn = false;
            bool bReturn2 = false;
            string subKeyPath = @"mailto\shell\";
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(subKeyPath);
                if (key != null)
                {
                    object objValue = key.GetValue("");
                    if (objValue != null)
                    {
                        if (key.GetValueKind("") == RegistryValueKind.String)
                        {
                            string strValue = Convert.ToString(objValue);
                            if (strValue != @"(value not set)")
                                bReturn = true;
                        }
                    }
                    key.Close();
                    key.Dispose();
                    key = null;
                    objValue = null;

                    // 2nd test
                    subKeyPath = @"mailto\shell\open\command\";
                    key = Registry.ClassesRoot.OpenSubKey(subKeyPath);
                    if (key != null)
                    {
                        objValue = key.GetValue("");
                        if (objValue != null)
                        {
                            if (key.GetValueKind("") == RegistryValueKind.String)
                            {
                                string strValue = Convert.ToString(objValue);
                                if (strValue != null)
                                {
                                    if (strValue.IndexOf(@"C:\windows\system", StringComparison.OrdinalIgnoreCase) >= 0)
                                        bReturn2 = false;
                                    else
                                        bReturn2 = true;
                                }
                            }
                        }
                        key.Close();
                        key.Dispose();
                        key = null;
                    }
                }
            }
            catch { }
            return (bReturn || bReturn2);
        }

        public static bool IsMailClientInstalledForUser()
        {
            bool bReturn = false;
            bool bReturn2 = false;
            string subKeyPath = @"SOFTWARE\Classes\mailto\shell\";
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(subKeyPath);
                if (key != null)
                {
                    object objValue = key.GetValue("");
                    if (objValue != null)
                    {
                        if (key.GetValueKind("") == RegistryValueKind.String)
                        {
                            string strValue = Convert.ToString(objValue);
                            if (strValue != @"(value not set)")
                                bReturn = true;
                        }
                    }
                    key.Close();
                    key.Dispose();
                    key = null;
                    objValue = null;

                    // 2nd test
                    subKeyPath = @"SOFTWARE\Classes\mailto\shell\open\command\";
                    key = Registry.LocalMachine.OpenSubKey(subKeyPath);
                    if (key != null)
                    {
                        objValue = key.GetValue("");
                        if (objValue != null)
                        {
                            if (key.GetValueKind("") == RegistryValueKind.String)
                            {
                                string strValue = Convert.ToString(objValue);
                                if (strValue != null)
                                {
                                    if (strValue.IndexOf(@"C:\windows\system", StringComparison.OrdinalIgnoreCase) >= 0)
                                        bReturn2 = false;
                                    else
                                        bReturn2 = true;
                                }
                            }
                        }
                        key.Close();
                        key.Dispose();
                        key = null;
                    }
                }
            }
            catch { }
            return (bReturn || bReturn2);
        }
    }
}

