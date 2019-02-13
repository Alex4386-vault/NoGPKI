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
        public string gpkiPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"assets\\gpkiroot.cer");
        public X509Certificate2 gpkiCert;

        public const int FLAG_FAIL_ON_LM = 0b0001;
        public const int FLAG_FAIL_ON_CU = 0b0010;
        public const int FLAG_TRUST_ON_LM = 0b0100;
        public const int FLAG_TRUST_ON_CU = 0b1000;


        public void OpenStores()
        {
            try
            {
                lmban.Open(OpenFlags.ReadWrite);
                cuban.Open(OpenFlags.ReadWrite);
            }
            catch (CryptographicException e)
            {
                MessageBox.Show("로컬 컴퓨터의 인증서 불신목록 RW 마운트 실패", "오류: 권한 부족", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        public void CloseStores()
        {
            lmban.Close();
            cuban.Close();
        }

        public MainWindow()
        {
            OpenStores();

            if (!VerifyIsGPKI())
            {
                GPKICheckSumFail();
                this.Close();
            } else {
                gpkiCert = new X509Certificate2(gpkiPath);
            }

            InitializeComponent();

            checkGPKIstatus();
        }

        public void checkGPKIstatus()
        {

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

            if (found_cucerts || found_lmcerts)
            {
                if (found_cucerts != found_lmcerts)
                {
                    certStat.Content = "GPKI인증서를 불신, 일부만 적용됨";
                    statusLabel.Content = "준비 완료! 명령만 주세요!";
                }
                else
                {
                    certStat.Content = "비밀키가 일치하는 GPKI인증서 발견";
                    statusLabel.Content = "준비 완료! 명령만 주세요!";
                }
            }
            else
            {
                certStat.Content = "GPKI인증서를 신뢰하고 있음";
                statusLabel.Content = "준비 완료! 명령만 주세요!";
            }

            if (found_abnormality)
            {
                statusLabel.Content = "뭔가 이상해요! README를 읽어봐요!";
            }

            if(err_certs != 0)
            {
                // with partial untrust or with abnormality
                // error code will be appended
                certStat.Content += ("("+ err_certs + ")");
            }
        }

        public void GPKICheckSumFail()
        {
            MessageBox.Show("MD5 체크섬 검사 실패", "오류: 유효성 검사 실패", MessageBoxButton.OK, MessageBoxImage.Error);
            MessageBox.Show("실행 경로아래에 assets\\gpkiroot.cer 파일이 있는지 확인 해 주시기 바랍니다.\n있다면, 파일이 손상된것 같습니다.", "오류: 유효성 검사 실패", MessageBoxButton.OK, MessageBoxImage.Error);
            
        }

        public bool VerifyIsGPKI()
        {
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
            statusLabel.Content = "GPKI 인증서 신뢰 확인";
            MessageBoxResult i = MessageBox.Show("정말로 인증서를 신뢰하시겠습니까?", "인증서 신뢰", MessageBoxButton.YesNo, MessageBoxImage.Question);
            statusProgress.Value = 50;
            if (i == MessageBoxResult.Yes)
            {
                statusLabel.Content = "GPKI 인증서 체크섬 검사 중";
                if (!VerifyIsGPKI())
                {
                    statusLabel.Content = "GPKI 인증서 체크섬 검사 실패!";
                    GPKICheckSumFail();
                    return;
                }

                statusLabel.Content = "GPKI 인증서 신뢰 시작";

                int err_trust = 0; // no errors
                
                statusLabel.Content = "LM 에서 인증서 신뢰중";
                try
                {
                    lmban.Remove(gpkiCert);
                }
                catch
                {
                    err_trust -= FLAG_FAIL_ON_LM;   // -1, lm removal fail
                }
                statusProgress.Value = 60;
    
                statusLabel.Content = "CU 에서 인증서 신뢰중";
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
                    statusLabel.Content = "GPKI 인증서 신뢰처리 완료";
                }
                else
                {
                    statusLabel.Content = "GPKI 인증서 신뢰중 예외 발생! ("+err_trust+")";
                }
                statusProgress.Value = 100;

            } else
            {
                statusProgress.Value = 0;
                statusLabel.Content = "GPKI 인증서 신뢰 취소";
            }
            CloseStores();

            checkGPKIstatus();
        }

        private void deleteCert_Click(object sender, RoutedEventArgs e)
        {
            OpenStores();
            statusProgress.Value = 0;
            statusLabel.Content = "GPKI 인증서 불신 확인";
            MessageBoxResult i = MessageBox.Show("정말로 인증서를 불신하시겠습니까?", "인증서 불신", MessageBoxButton.YesNo, MessageBoxImage.Question);
            statusProgress.Value = 50;
            if (i == MessageBoxResult.Yes)
            {
                statusLabel.Content = "GPKI 인증서 체크섬 검사 중";
                if (!VerifyIsGPKI())
                {
                    statusLabel.Content = "GPKI 인증서 체크섬 검사 실패!";
                    GPKICheckSumFail();
                    return;
                }

                statusLabel.Content = "GPKI 인증서 불신 시작";

                int err_ban = 0; // no errors

                statusLabel.Content = "LM 에 인증서 불신중";                    
                try
                {
                    lmban.Add(gpkiCert);
                }
                catch
                {
                    err_ban -= FLAG_FAIL_ON_LM;   // -1, lm ban fail
                }
                statusProgress.Value = 60;

                statusLabel.Content = "CU 에 인증서 불신중";
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
                    statusLabel.Content = "GPKI 인증서 불신 완료";
                }
                else
                {
                    statusLabel.Content = "GPKI 인증서 불신중 예외 발생! (" + err_ban + ")";
                }
                statusProgress.Value = 100;

            } else
            {
                statusProgress.Value = 0;
                statusLabel.Content = "GPKI 인증서 불신 취소";
            }
            CloseStores();

            checkGPKIstatus();
        }
    }
}
