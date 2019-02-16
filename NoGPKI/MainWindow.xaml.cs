using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;

namespace NoGPKI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public X509Store lmban = new X509Store(StoreName.Disallowed, StoreLocation.LocalMachine);
        public X509Store cuban = new X509Store(StoreName.Disallowed, StoreLocation.CurrentUser);
        public X509Certificate2 gpki = new X509Certificate2();
        public string gpkiMD5 = "fa396a2bb384a05f3fa237609d68d516";
        public string assetsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
        public string gpkiPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"assets\\gpkiroot.cer");
        public X509Certificate2 gpkiCert;

        public const int FLAG_FAIL_ON_LM = 0b0001;  // 1, used to flag fail on lm
        public const int FLAG_FAIL_ON_CU = 0b0010;  // 2, used to flag fail on cu
        public const int FLAG_TRUST_ON_LM = 0b0100; // 4, used to flag trust on lm
        public const int FLAG_TRUST_ON_CU = 0b1000; // 8, used to flag trust on cu

        private ResStrings strRes = new ResStrings();

        public void OpenStores()
        {
            try
            {
                lmban.Open(OpenFlags.ReadWrite);
                cuban.Open(OpenFlags.ReadWrite);
            }
            catch (CryptographicException e)
            {
                MessageBox.Show(strRes.getStr(ResStrings.STR_ERR_MOUNT_FAIL), strRes.getStr(ResStrings.STR_ERR_NOT_AUTHORIZED), MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        public void CloseStores()
        {
            lmban.Close();
            cuban.Close();
        }

        public void outputGPKI()
        {
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }
            if (!File.Exists(gpkiPath))
            {
                File.WriteAllBytes(gpkiPath, Properties.Resources.gpkiroot);
            }
        }

        public void deleteAndOutputGPKI()
        {
            if (Directory.Exists(assetsPath))
            {
                if (File.Exists(gpkiPath))
                {
                    try
                    {
                        File.Delete(gpkiPath);
                    }
                    catch
                    {
                        GPKICheckSumFail();
                        this.Close();
                    }
                }
            }
            outputGPKI();
        }

        public MainWindow()
        {
            if (!VerifyIsGPKI())
            {
                deleteAndOutputGPKI();
                if (!VerifyIsGPKI())
                {
                    GPKICheckSumFail();
                }
                this.Close();
            } else {
                gpkiCert = new X509Certificate2(gpkiPath);
            }

            InitializeComponent();

            // empty statusLabel on init;
            statusLabel.Content = "";
            checkGPKIstatus();
        }

        public void checkGPKIstatus()
        {
            OpenStores();

            var lm_certs = lmban.Certificates.Find(X509FindType.FindByThumbprint, gpkiCert.Thumbprint, false);
            var cu_certs = cuban.Certificates.Find(X509FindType.FindByThumbprint, gpkiCert.Thumbprint, false);

            bool found_cucerts = false;
            bool found_lmcerts = false;
            bool found_abnormality = false;

            int err_certs = 0;
            if (lm_certs != null && lm_certs.Count > 0)
            {
                if (lm_certs[0].PrivateKey == gpkiCert.PrivateKey)
                {
                    found_lmcerts = true;
                }
                else
                {
                    found_abnormality = true;
                    err_certs -= FLAG_FAIL_ON_LM; // -1, abnormality on lm
                }
            }
            else
            {
                err_certs -= FLAG_TRUST_ON_LM; // -4, trusted on lm 
            }

            if (cu_certs != null && cu_certs.Count > 0)
            {
                if (cu_certs[0].PrivateKey == gpkiCert.PrivateKey)
                {
                    found_cucerts = true;
                }
                else
                {
                    found_abnormality = true;
                    err_certs -= FLAG_FAIL_ON_CU; // -2, abnormality on cu
                }
            }
            else
            {
                err_certs -= FLAG_TRUST_ON_CU; // -8, trusted on cu
            }

            string msg_to_append = "";
            if (found_cucerts || found_lmcerts)
            {
                if (found_cucerts != found_lmcerts)
                {
                    certStat.Content = strRes.getStr(ResStrings.STR_MSG_PARTIAL_DISTRUST);
                    msg_to_append = strRes.getStr(ResStrings.STR_MSG_READY_FOR_CMD);
                }
                else
                {
                    certStat.Content = strRes.getStr(ResStrings.STR_MSG_FULL_DISTRUST);
                    msg_to_append = strRes.getStr(ResStrings.STR_MSG_READY_FOR_CMD);
                }
            }
            else
            {
                certStat.Content = strRes.getStr(ResStrings.STR_MSG_FULLLY_TRUSTED);
                msg_to_append = strRes.getStr(ResStrings.STR_MSG_READY_FOR_CMD);
            }

            if (found_abnormality)
            {
                msg_to_append = strRes.getStr(ResStrings.STR_MSG_ABNORMALITY_FOUND);
            }
            statusLabel.Content += "\n";
            statusLabel.Content += msg_to_append;

            if (err_certs != 0)
            {
                // with partial untrust or with abnormality
                // error code will be appended
                certStat.Content += ("("+ err_certs + ")");
            }
        }

        public void GPKICheckSumFail()
        {
            MessageBox.Show(strRes.getStr(ResStrings.STR_ERR_MD5_CHECK_FAIL), strRes.getStr(ResStrings.STR_ERR_VALIDATION_FAIL), MessageBoxButton.OK, MessageBoxImage.Error);
            MessageBox.Show(String.Format(strRes.getStr(ResStrings.STR_MSG_CHECK_FILE_IS_THERE), gpkiPath), strRes.getStr(ResStrings.STR_ERR_VALIDATION_FAIL), MessageBoxButton.OK, MessageBoxImage.Error);
            
        }

        public bool VerifyIsGPKI()
        {
            if (!File.Exists(gpkiPath))
            {
                outputGPKI();
            }
            MD5 md5Hash = MD5.Create();
            return VerifyMd5Hash(md5Hash, gpkiPath, gpkiMD5);
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            //byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            byte[] data = md5Hash.ComputeHash(File.ReadAllBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            /*
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
            */
            return (0 == comparer.Compare(hashOfInput, hash));
        }

        private void recoverCert_Click(object sender, RoutedEventArgs e)
        {
            OpenStores();
            statusProgress.Value = 0;
            statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CHECK_TRUST);
            MessageBoxResult i = MessageBox.Show(strRes.getStr(ResStrings.STR_MSG_CONFIRM_TRUST), strRes.getStr(ResStrings.STR_MSG_CHECK_TRUST), MessageBoxButton.YesNo, MessageBoxImage.Question);
            statusProgress.Value = 50;
            if (i == MessageBoxResult.Yes)
            {
                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CHECKING_CHECKSUM);
                if (!VerifyIsGPKI())
                {
                    statusLabel.Content = strRes.getStr(ResStrings.STR_ERR_CHECKSUM_FAIL);
                    GPKICheckSumFail();
                    return;
                }

                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_PROCESS_TRUST);

                int err_trust = 0; // no errors
                
                statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_MSG_TRUSTING_IN),"LM");
                try
                {
                    lmban.Remove(gpkiCert);
                }
                catch
                {
                    err_trust -= FLAG_FAIL_ON_LM;   // -1, lm removal fail
                }
                statusProgress.Value = 60;
    
                statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_MSG_TRUSTING_IN), "CU");
                try
                {
                    cuban.Remove(gpkiCert);
                }
                catch
                {
                    err_trust -= FLAG_FAIL_ON_CU;   // -2, cu removal fail
                }
                statusProgress.Value = 80;

                if (err_trust == 0)
                {
                    statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_COMPLETED_TRUSTING);
                }
                else
                {
                    statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_ERR_WHILE_TRUST), err_trust);
                }
                statusProgress.Value = 100;

            } else
            {
                statusProgress.Value = 0;
                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CANCELLED_TRUSTING);
            }
            CloseStores();

            checkGPKIstatus();
        }

        private void deleteCert_Click(object sender, RoutedEventArgs e)
        {
            OpenStores();
            statusProgress.Value = 0;
            statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CHECK_DISTRUST);
            MessageBoxResult i = MessageBox.Show(strRes.getStr(ResStrings.STR_MSG_CONFIRM_DISTRUST), strRes.getStr(ResStrings.STR_MSG_CHECK_DISTRUST), MessageBoxButton.YesNo, MessageBoxImage.Question);
            statusProgress.Value = 50;
            if (i == MessageBoxResult.Yes)
            {
                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CHECKING_CHECKSUM);
                if (!VerifyIsGPKI())
                {
                    statusLabel.Content = strRes.getStr(ResStrings.STR_ERR_CHECKSUM_FAIL);
                    GPKICheckSumFail();
                    return;
                }

                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_PROCESS_DISTRUST);

                int err_ban = 0; // no errors

                statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_MSG_DISTRUSTING_IN),"LM");                    
                try
                {
                    lmban.Add(gpkiCert);
                }
                catch
                {
                    err_ban -= FLAG_FAIL_ON_LM;   // -1, lm ban fail
                }
                statusProgress.Value = 60;

                statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_MSG_DISTRUSTING_IN), "CU");
                try
                {
                    cuban.Add(gpkiCert);
                }
                catch
                {
                    err_ban -= FLAG_FAIL_ON_CU;   // -2, cu ban fail
                }
                statusProgress.Value = 80;
                                
                if (err_ban == 0)
                {
                    statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_COMPLETED_DISTRUSTING);
                }
                else
                {
                    statusLabel.Content = String.Format(strRes.getStr(ResStrings.STR_ERR_WHILE_DISTRUST), err_ban);
                }
                statusProgress.Value = 100;

            } else
            {
                statusProgress.Value = 0;
                statusLabel.Content = strRes.getStr(ResStrings.STR_MSG_CANCELLED_DISTRUSTING);
            }
            CloseStores();

            checkGPKIstatus();
        }
    }
}
