using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Encoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //string original = "";
        string str_Key_template = "ABCDEFGHIJKLMNOPQRSTUVWX";
        string str_Key;
        // byte[] encrypted;

        public string strIV = "1234567890123456";//вектор инициализации в строковом представлении
        byte[] vect;//вектор инициализации
        
        private static Random rng = new Random();
        //функция перетасовки списка(для ключа):
        public List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public String ExtractTxtFile(String path)
        {
            String result = "";
            // Open the file to read from.
            //if (radioButton1.Checked)
            //{
            //-------
            string[] readText = File.ReadAllLines(path);
            foreach (string s in readText)
            {
                result += s + Environment.NewLine;
            }
            
            return result;
        }

        public byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider asAlg = new AesCryptoServiceProvider())
            {
                asAlg.Key = Key;
                asAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = asAlg.CreateEncryptor(asAlg.Key, asAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();

                    }
                }
            }


            string str_encrypted = System.Text.Encoding.UTF8.GetString(encrypted);

            Console.WriteLine("str_encrypted line: {0}", str_encrypted);

            // Return the encrypted bytes from the memory stream.
            return encrypted;


        }

        public string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider asAlg = new AesCryptoServiceProvider())
            {



                asAlg.Key = Key;
                asAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = asAlg.CreateDecryptor(asAlg.Key, asAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try {
                if (textBox1.Text != "") str_Key = textBox1.Text;
                Encryptor.Properties.Settings.Default["KeyValue"] = str_Key;
                Encryptor.Properties.Settings.Default.Save();
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            str_Key = Encryptor.Properties.Settings.Default["KeyValue"].ToString();

            if (!Convert.ToBoolean(Encryptor.Properties.Settings.Default["KeyIsHidden"])) textBox1.Text = str_Key;
            vect = Encoding.ASCII.GetBytes(strIV);
        }
        //кодируем и сохраняем текст в файл:
        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                if (radioButton1.Checked)// если нужно закодировать обычный текст:
                {
                    if (textBox3.Text == "")
                    {
                        using (SaveFileDialog sfd = new SaveFileDialog())
                        {
                            if (sfd.ShowDialog(this) == DialogResult.OK)
                            {
                                textBox3.Text = sfd.FileName;

                            }
                        }
                    }
                
                    string original = textBox2.Text;
                    // Create a new instance of the AesCryptoServiceProvider
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
                    {
                        //ключь:
                        if (textBox1.Text != "") str_Key = textBox1.Text;
                        byte[] byte_key = Encoding.ASCII.GetBytes(str_Key);
                        myAes.Key = byte_key;

                        //vect = myAes.IV;
                        myAes.IV = vect;
                        // Encrypt the string to an array of bytes.
                        original = textBox2.Text;
                        byte[] encrypted = EncryptStringToBytes(original, myAes.Key, myAes.IV);

                        //удаляем исходный файл:
                        string path = textBox3.Text;
                        string filename = Path.GetFileName(path);
                        string filenameWE = Path.GetFileNameWithoutExtension(path);

                        File.Delete(path);
                        //записываем в файл зашифрованный байтовый массив:
                        path.Replace(filename, filenameWE);

                        File.WriteAllBytes(path, encrypted);

                    }
                }  /////////////////////////////////////////////////////             
                else//если нужно закодировать файл
                {
                    byte[] fileInBytes = File.ReadAllBytes(textBox3.Text);

                    string original = Convert.ToBase64String(fileInBytes);//System.Text.Encoding.UTF8.GetString(fileInBytes);
                                                                          // Create a new instance of the AesCryptoServiceProvider
                                                                          // class.  This generates a new key and initialization 
                                                                          // vector (IV).
                    using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
                    {
                        //ключь:
                        if (textBox1.Text != "") str_Key = textBox1.Text;
                        byte[] byte_key = Encoding.ASCII.GetBytes(str_Key);
                        myAes.Key = byte_key;

                        //vect = myAes.IV;
                        myAes.IV = vect;
                        // Encrypt the string to an array of bytes.
                        //original = textBox2.Text;
                        byte[] encrypted = EncryptStringToBytes(original, myAes.Key, myAes.IV);

                        //удаляем исходный файл:
                        string path = textBox3.Text;
                        //string filename = Path.GetFileName(path);
                        //string filenameWE = Path.GetFileNameWithoutExtension(path);

                        File.Delete(path);
                        //записываем в файл зашифрованный байтовый массив:
                        //path.Replace(filename, filenameWE);

                        File.WriteAllBytes(path, encrypted);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void button3_Click(object sender, EventArgs e)
        {
            try { 
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    textBox3.Text = ofd.FileName;

                }
            }
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        //извлекаем и декодируем байтовый файл:
        private void button4_Click(object sender, EventArgs e)
        {
            try {
                if (radioButton1.Checked)//если нужно декодировать обычный текст:
                {
                    string roundtrip = "";
                    using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
                    {
                        //ключь:
                        if (textBox1.Text != "") str_Key = textBox1.Text;
                        byte[] byte_key = Encoding.ASCII.GetBytes(str_Key);
                        myAes.Key = byte_key;

                        String path = textBox3.Text;
                        //String encoded = ExtractTxtFile(path);
                        //считываем байты из файла в байтовый массив:
                        byte[] encrypted = File.ReadAllBytes(path);

                        //String original = textBox2.Text;
                        //byte[] encrypted = EncryptStringToBytes(original, myAes.Key, myAes.IV);


                        // Decrypt the bytes to a string.
                        //byte[] byte_Key = biResult.ToByteArray();//BitConverter.GetBytes(biResult);
                        roundtrip = DecryptStringFromBytes(encrypted, myAes.Key, vect);


                    }
                    textBox2.Text = roundtrip;
                }else//если нужно декодировать файл
                {
                    ////////////////////////////////////////////////////////////////
                    //путь:
                    String path = textBox3.Text;
                    string roundtrip = "";

                    using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
                    {
                        //ключь:
                        if (textBox1.Text != "") str_Key = textBox1.Text;
                        byte[] byte_key = Encoding.ASCII.GetBytes(str_Key);
                        myAes.Key = byte_key;


                        //String encoded = ExtractTxtFile(path);
                        //считываем байты из файла в байтовый массив:
                        byte[] encrypted = File.ReadAllBytes(path);

                        //String original = textBox2.Text;
                        //byte[] encrypted = EncryptStringToBytes(original, myAes.Key, myAes.IV);


                        // Decrypt the bytes to a string.
                        //byte[] byte_Key = biResult.ToByteArray();//BitConverter.GetBytes(biResult);
                        roundtrip = DecryptStringFromBytes(encrypted, myAes.Key, vect);


                    }
                    //удаляем закодированный файл:
                    File.Delete(path);
                    byte[] FileInBytes = Convert.FromBase64String(roundtrip);//Encoding.ASCII.GetBytes(roundtrip);


                    File.WriteAllBytes(path, FileInBytes);
                }
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}
        //генерация случайного ключа:
        private void button5_Click(object sender, EventArgs e)
        {
            try { 

            string source = str_Key_template;
            //Console.WriteLine("source:" + source);
            List<char> sourceList = new List<char>(source);
            List<char> outputList = new List<char>(Shuffle<char>(sourceList));


            string output = string.Join("", outputList.ToArray());

            textBox1.Text = output;
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = "";
                str_Key = "";
                Encryptor.Properties.Settings.Default["KeyValue"] = str_Key;
                Encryptor.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                //проверяем свойство KeyIsHidden, отвечающее за то, является ли ключ скрытым:
                if (Convert.ToBoolean(Encryptor.Properties.Settings.Default["KeyIsHidden"]))
                {
                    Encryptor.Properties.Settings.Default["KeyIsHidden"] = false;
                }
                else
                {
                    Encryptor.Properties.Settings.Default["KeyIsHidden"] = true;
                }
                Encryptor.Properties.Settings.Default.Save();

                if (!Convert.ToBoolean(Encryptor.Properties.Settings.Default["KeyIsHidden"])) { textBox1.Text = str_Key; }
                else
                {
                    textBox1.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) {

                button4.Text = "Extract text (decrypt)";
                button2.Text = "Save File As (encrypt)";
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {

                button4.Text = "Decrypt the file";
                button2.Text = "Save File (encrypt)";

            }
        }

        //About button:
        private void button7_Click_2(object sender, EventArgs e)
        {
            MessageBox.Show("   Encryptor\n   Copyright ©  2017 YG");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MessageBox.Show("\"Encryptor\" uses the Advanced Encryption Standard (AES), also known by its original name Rijndael," +
"is a specification for the encryption of electronic data established by the U.S." +
"National Institute of Standards and Technology(NIST) in 2001."+

"\n\nFor text encryption:" +
"\n1)Choose the \"Text\" radio button." +
"\n2)Input text into the textbox." +
"\n3)Enter a key that consist of 24 characters of the English alphabet or numerals into key field," +
"or generate it by clicking the \"Generate a Key\" button." +
"\n4)You can save the key for future use (press \"Save Key\" button)." +
"\n5)You can hide the key(press \"Hide /Show Key\" button)." +
"\n6)Click \"Save File As(encrypt)\" button to save encrypted text into a file."+

"\n\nFor text decryption:" +
"\n1)Choose the \"Text\" radio button." +
"\n2)Select the encrypted file by clicking on the \"Open a File\" button." +
"\n4)If you did not save the key, enter the key with which the file was encrypted into the key field." +
"\n3)Click \"Extract text(decrypt)\" button to extract decrypted text into the textbox." +

"\n\nFor file encryption:" +
"\n1)Choose the \"File\" radio button." +
"\n2)Select the file for encryption by clicking on the \"Open a File\" button." +
"\n3) Enter a key that consist of 24 characters of the English alphabet or numerals into key field," +
" or generate it by clicking the \"Generate a Key\" button." +
"\n4)You can save the key for future use (press \"Save Key\" button)." +
"\n5)You can hide the key(press \"Hide/Show Key\" button)." +
"\n6)Click \"Save File(encrypt)\" button to save encrypted file." +

"\n\nFor file decryption:" +
"\n1)Choose the \"File\" radio button." +
"\n2)Select the encrypted file by clicking on the \"Open a File\" button." +
"\n3)If you did not save the key, enter the key with which the file was encrypted into the key field." +
"\n4)Click \"Decrypt the file\" button to decrypt file."
);
        }
    }
}
