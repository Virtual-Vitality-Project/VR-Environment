using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Data;

public class Login : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorOutput;
    public Button loginButton;
    public Button exitButton;

    ArrayList credentials;

    void Start()
    {
        loginButton.onClick.AddListener(login);
        exitButton.onClick.AddListener(Exit);
    }

    void login()
    {
        string loginAPIfilePath = Application.dataPath + "/APIConfigSettings/loginAPI.ini";
        LoginAPI test = new LoginAPI(emailInput.text, passwordInput.text, "docent", loginAPIfilePath);

        // Debug.Log(test.APIEncryptFunc("http://192.168.50.2/api/login"));

        int loginResult = test.isValidLogin();//Of using the obj, execute this function first.

        //Test with obj
        LoginAPI.OkJsonResponse LoginAPIResponse = test.getJsonObj();

        if (loginResult == 2)
        {//Config file not found.
            DisplayError("Config file for LoginAPI not found.\n");
        }
        else if (loginResult == 3)
        {//LoginAPI not active or endpoint url incorrect. (Offline)
            DisplayError("LoginAPI is not reachable or offline.\n");
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok" && LoginAPIResponse.Data.Is_valid_login == "true")
        {
            Debug.Log("Logged in! Name: " + LoginAPIResponse.Data.Name + " Nickname: " + LoginAPIResponse.Data.Nickname + "!\n");

            Debug.Log($"Email: '{emailInput.text}'");
            GlobalVariableStorage.TeacherEmail = emailInput.text;
            GlobalVariableStorage.TeacherName = LoginAPIResponse.Data.Name;

            SceneManager.LoadScene("SituationSelectionHub"); // go to the selection scene tab
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok" && LoginAPIResponse.Data.Is_valid_login == "false")
        {
            DisplayError("Invalid login details. Please try again.\n");
        }
        else if (LoginAPIResponse.Data.Is_valid_login != "true")
        {
            DisplayError("Error: " + LoginAPIResponse.Error.Stat + "\n");
        }
    
    }

    void Exit()
    {
        Debug.Log("User exitted the application!");
        Application.Quit();
        // UnityEditor.EditorApplication.isPlaying = false;
    }

    void DisplayError(string message)
    {
        Debug.LogError(message);
        errorOutput.text = message;
    }
}






///////////////////////////////////

public class LoginAPI
{
    /* LoginAPI class explained by wwwqr-000.
     * 
     * This class is made by the Beheersysteem department.
     * 
     * You only need to specify the email, password and the role of the user in the class constructor when making a new object based off the class.
     * Also, you need to give the path to the config settings file. (From the location of the compiled .exe file)
     * 
     * If you want to know if the login data of the user is valid, then run the 'isValidLogin' function. It returns an int. (0-13.)
     * Look into the class code or the given example code to get the meaning of a int return status. The best is to handle all the possible returns.
     * 
     * The key you see here needs to be changed when the POC or the full release is done. If you change the key, you also have to change the
     * encrypted strings in the configSettings file. In the LoginAPI class is also a function for generating a encrypted string based on the current key.
     * Use that function do encrypt every raw value and place it in the configSettings file.
     * The configSettings file contains the following data:
     * 
     * Use the getJsonResponse(); to get the json response string.
     * 
     * (The data of the lines is based on the data in the API code itself. (API is active in laravel environment.))
     * Line 1: encrypted protocol string
     * Line 2: encrypted type string
     * Line 3: encrypted location string
     * Line 4 - 8: encrypted jellyfish strings from 1 to 5
     * Line 9: encrypted endpoint url
     * Line 10: encrypted AES key from the source API (Key for web enc and dec)
     * Line 11: encrypted IV key / hash from web AES
     * 
     * For more questions about the API or class, ask wwwqr-000.
     * 
     */
    private String email, password, role, dataPath, url, prot, type, loc, j1, j2, j3, j4, j5, responseString, externalAESKey, externalIVKey;
    private static int globalLoginAPIObjCount = -1;
    // private bool validObj = true;
    private int index;
    private String boerenkoolStamppot_dano1 = "00080E2255A4CDC4210DCC5AABB574CCoi2hro32j@#I$o32h432n";//Key


    public class Error
    {
        public string Active { get; set; }
        public string Stat { get; set; }
    }

    public class Data
    {
        public string Is_valid_login { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Roles { get; set; }
    }
    public class OkJsonResponse
    {
        public Error Error { get; set; }
        public Data Data { get; set; }
    }

    private OkJsonResponse okJsonObj;

    private static byte[] GetValidKey(String key, int keySize)
    {
        using (var sha = SHA256.Create())
        {
            byte[] keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
            byte[] validKey = new byte[keySize];
            Array.Copy(keyBytes, validKey, Math.Min(keyBytes.Length, validKey.Length));
            return validKey;
        }
    }

    public String getJsonResponse()
    {
        return responseString;
    }

    public OkJsonResponse getJsonObj()
    {
        return okJsonObj;
    }

    //Self AES
    private static String EncryptString(String plainText, String key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = GetValidKey(key, aesAlg.KeySize / 8);
            aesAlg.IV = new byte[16];

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    //Self AES
    private static String DecryptString(String cipherText, String key)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = GetValidKey(key, aesAlg.KeySize / 8);
            aesAlg.IV = new byte[16];

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new System.IO.MemoryStream(cipherBytes))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    //WEB AES
    private static string DecryptStringWEB(string cipherText, string key, string iv)
    {
        try
        {
            byte[] inputBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("CryptographicException: " + ex.Message);
            return null;
        }
        catch (FormatException ex)
        {
            Console.WriteLine("FormatException: " + ex.Message);
            return null;
        }
    }

    public LoginAPI(String email_i, String password_i, String role_i, String path_i)
    {
        index = ++globalLoginAPIObjCount;
        dataPath = path_i;
        email = email_i;
        password = password_i;
        role = role_i;
    }
    public int getIndex()
    {
        return index;
    }

    public String APIEncryptFunc(String i)
    {
        return EncryptString(i, boerenkoolStamppot_dano1);
    }

    public int isValidLogin()
    {
        //Get encrypted vars from extrnal file
        if (!File.Exists(dataPath))
        {
            return 2;//Config file not found
        }
        using (StreamReader sr = new StreamReader(dataPath))
        {
            String tmpLine = "";
            int lineCounter = 0;
            while ((tmpLine = sr.ReadLine()) != null)
            {//Getting encrypted settings from config file and setting enc vars to dec vars.
                ++lineCounter;
                String v = DecryptString(tmpLine, boerenkoolStamppot_dano1);
                switch (lineCounter)
                {
                    case 1:
                        prot = v;
                        break;
                    case 2:
                        type = v;
                        break;
                    case 3:
                        loc = v;
                        break;
                    case 4:
                        j1 = v;
                        break;
                    case 5:
                        j2 = v;
                        break;
                    case 6:
                        j3 = v;
                        break;
                    case 7:
                        j4 = v;
                        break;
                    case 8:
                        j5 = v;
                        break;
                    case 9:
                        url = v;
                        break;
                    case 10:
                        externalAESKey = v;
                        break;
                    case 11:
                        externalIVKey = v;
                        break;
                }
            }
        }
        //
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Protocol", prot);
            client.DefaultRequestHeaders.Add("Type", type);
            client.DefaultRequestHeaders.Add("Location", loc);
            var postData = new System.Collections.Generic.Dictionary<string, string> {
                        {"email", email},
                        {"password", password},
                        {"role", role},
                        {"j1", j1},
                        {"j2", j2},
                        {"j3", j3},
                        {"j4", j4},
                        {"j5", j5}
                    };
            FormUrlEncodedContent POSTContent = new FormUrlEncodedContent(postData);


            HttpResponseMessage response;
            try
            {
                response = client.PostAsync(url, POSTContent).Result;
            }
            catch (Exception)
            {//Endpoint not active or not found
                return 3;
            }

            responseString = response.Content.ReadAsStringAsync().Result;
            okJsonObj = JsonConvert.DeserializeObject<OkJsonResponse>(responseString);

            try
            {
                okJsonObj.Data.Name = DecryptStringWEB(okJsonObj.Data.Name, externalAESKey, externalIVKey);
                okJsonObj.Data.Nickname = DecryptStringWEB(okJsonObj.Data.Nickname, externalAESKey, externalIVKey);
                okJsonObj.Data.Roles = DecryptStringWEB(okJsonObj.Data.Roles, externalAESKey, externalIVKey);
            }
            catch (Exception) { }
            return 0;
        }
    }
}