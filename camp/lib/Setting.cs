using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace kokal.lib
{


   
    internal class Setting
    {

        private static Setting? instance;
        private AES aes;

        private Setting()
        {
            aes = new AES(GetApplicationDirectory());
           
        }

        public static Setting Instance => instance ?? (instance = new Setting());


        public static void Set(string key, string value)
        {
           
            var retrive = Instance.RetrieveVariableFromFile();
            if (retrive == null)
            {
                retrive = new Dictionary<string, string>();
            }
            retrive[key] = value;

            Instance.SaveVariableToFile(retrive);

        }

        public static string? Get(string key)
        {
            
            var retrive = Instance.RetrieveVariableFromFile();
            if (retrive != null && retrive.ContainsKey(key))
            {
                return retrive[key];
            }
            return null;
        }



        public static string GetApplicationDirectory()
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "kokal");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        private void SaveVariableToFile(Dictionary<string, string> Variable)
        {
            // Serialize and encrypt the variable to JSON
            string jsonString = JsonSerializer.Serialize(Variable);
            byte[] encryptedBytes = aes.Encrypt(jsonString);

            // Save the encrypted data to a file
            File.WriteAllBytes(Path.Combine(GetApplicationDirectory(), "data.dat"), encryptedBytes);
        }




        private Dictionary<string, string>? RetrieveVariableFromFile()
        {

            string path = Path.Combine(GetApplicationDirectory(), "data.dat");
            if (File.Exists(path))
            {
                // Read encrypted data from the file
                byte[] encryptedBytes = File.ReadAllBytes(Path.Combine(GetApplicationDirectory(), "data.dat"));

                // Decrypt the data
                string decryptedString = aes.Decrypt(encryptedBytes);

                // Deserialize the decrypted JSON string to a Dictionary
                return JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedString);
            }

            return null;
        }












        

     

    }
}
