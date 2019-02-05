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
        public X509Store store = new X509Store("ROOT", StoreLocation.LocalMachine);
        public X509Certificate2 gpki = new X509Certificate2();
        public string gpkiMD5 = "fa396a2bb384a05f3fa237609d68d516";
        public string gpkiPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"assets\\gpkiroot.cer");
        public X509Certificate2 gpkiCert;

        public MainWindow()
        {
            try
            {
                store.Open(OpenFlags.ReadWrite);
            } catch(CryptographicException e) {
                MessageBox.Show("로컬 컴퓨터의 CA Root 인증서 RW 마운트 실패", "오류: 권한 부족", MessageBoxButton.OK, MessageBoxImage.Error);   
                this.Close();
            }

            if (!VerifyIsGPKI())
            {
                MessageBox.Show("MD5 체크섬 검사 실패", "오류: 유효성 검사 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show("실행 경로아래에 assets\\gpkiroot.cer 파일이 있는지 확인 해 주시기 바랍니다.\n있다면, 파일이 손상된것 같습니다.", "오류: 유효성 검사 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            } else {
                gpkiCert = new X509Certificate2(gpkiPath);
            }
            InitializeComponent();
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

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
