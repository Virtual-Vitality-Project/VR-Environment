using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

public class Login : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorOutput;
    public Button loginButton;
    public Button exitButton;

    void Start()
    {
        loginButton.onClick.AddListener(login);
        exitButton.onClick.AddListener(Exit);
    }

    void login()
    {
        string loginAPIfilePath = Application.dataPath + "/APIConfigSettings/loginAPI.ini";
       

        string email = emailInput.text;
        string password = passwordInput.text;

        LoginAPI test = new LoginAPI(email, password, loginAPIfilePath, "00080E2255A4CDC4210DCC5AABB574CCoi2hro32j@#I$o32h432n");
        int loginResult = test.isValidLogin();//Login function
        LoginAPI.OkJsonResponse LoginAPIResponse = test.getJsonObj();//Get response obj

        if (loginResult == 2)
        {//Config file not found.
            DisplayError("Config file for LoginAPI not found.\n");
        }
        else if (loginResult == 3)
        {//LoginAPI not active or endpoint url incorrect. (Offline)
            DisplayError("LoginAPI not reachable or offline.\n");
        }
        else if (LoginAPIResponse.Error.Num == "0")
        {
            DisplayError("You are blacklisted!\n");
        }
        else if (LoginAPIResponse.Error.Num == "1" || LoginAPIResponse.Error.Num == "2" || LoginAPIResponse.Error.Num == "3")
        {
            DisplayError("Something went wrong!\n");//1: Missing or invalid POST data | 2: Invalid jellyfish | 3: Department doesn't exist
        }
        else if (LoginAPIResponse.Error.Num == "4" || LoginAPIResponse.Error.Num == "5" || LoginAPIResponse.Error.Num == "6")
        {
            DisplayError("Wrong login details or no access, please try again.\n");//4: invalid email format | 5: user doesn't exist | 6: no matching role
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok")
        {
            Debug.Log("Welcome, " + LoginAPIResponse.Data.Firstname + " " + LoginAPIResponse.Data.Lastname + "!\n");
            Debug.Log(test.getJsonResponse() + "\nFirst right the user has for this application: " + test.getJsonObj().Data.RightArr[0]);
            GlobalVariableStorage.TeacherEmail = emailInput.text;
            GlobalVariableStorage.TeacherName = LoginAPIResponse.Data.Firstname;

            SceneManager.LoadScene("SituationSelectionHub"); // go to the selection scene tab
        }
        else
        {
            DisplayError("Unknown error: " + LoginAPIResponse.Error.Stat + "\n");
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




/////////////// LoginAPI ///////////////////

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
    private String email, password, dataPath, url, j1, j2, j3, j4, j5, responseString, externalAESKey, externalIVKey;
    private static int globalLoginAPIObjCount = -1;
    private int index;
    private const String application = "unity";
    private BeheersysteemMethods BM;

    //Response obj
    public class Error
    {
        public string Active { get; set; }
        public string Stat { get; set; }
        public string Num { get; set; }
    }

    public class Data
    {
        public string Token { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Rights { get; set; }
        public String[] RightArr { get; set; }

    }
    public class OkJsonResponse
    {
        public Error Error { get; set; }
        public Data Data { get; set; }
    }
    //

    private OkJsonResponse okJsonObj;

    public String getJsonResponse()
    {
        return responseString;
    }

    public OkJsonResponse getJsonObj()
    {
        return okJsonObj;
    }

    public LoginAPI(String email_i, String password_i, String path_i, String BMI)
    {
        index = ++globalLoginAPIObjCount;
        dataPath = path_i;
        email = email_i;
        password = password_i;
        this.BM = new BeheersysteemMethods(BMI);
    }
    public int getIndex()
    {
        return index;
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
                String v = BM.Decrypt(tmpLine);
                switch (lineCounter)
                {
                    case 1:
                        url = v;
                        break;
                    case 2:
                        j1 = v;
                        break;
                    case 3:
                        j2 = v;
                        break;
                    case 4:
                        j3 = v;
                        break;
                    case 5:
                        j4 = v;
                        break;
                    case 6:
                        j5 = v;
                        break;
                    case 7:
                        externalAESKey = v;
                        break;
                    case 8:
                        externalIVKey = v;
                        break;
                }
            }
        }
        //
        using (HttpClient client = new HttpClient())
        {
            var postData = new System.Collections.Generic.Dictionary<string, string> {
                        {"email", email},
                        {"password", password},
                        {"application", application},
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
                if (okJsonObj.Data != null)
                {
                    okJsonObj.Data.Firstname = BM.WebDecrypt(okJsonObj.Data.Firstname, externalAESKey, externalIVKey);
                    okJsonObj.Data.Lastname = BM.WebDecrypt(okJsonObj.Data.Lastname, externalAESKey, externalIVKey);
                    okJsonObj.Data.Rights = BM.WebDecrypt(okJsonObj.Data.Rights, externalAESKey, externalIVKey);
                    okJsonObj.Data.RightArr = okJsonObj.Data.Rights.Split('^');
                }
            }
            catch (Exception) { }
            return 0;
        }
    }
}

public class BeheersysteemMethods
{
    private String J2b4uy24b;

    public BeheersysteemMethods(String AES_key_LoginAPI)
    {
        J2b4uy24b = AES_key_LoginAPI;
    }

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

    private static String EncryptString(String plainText, String key)
    {//Self
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

    private static String DecryptString(String cipherText, String key)
    {//Self
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

    private static string DecryptStringWEB(string cipherText, string key, string iv)
    {//Web
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

    public String Encrypt(String Input)
    {
        return EncryptString(Input, J2b4uy24b);
    }

    public String Decrypt(String Input)
    {
        return DecryptString(Input, J2b4uy24b);
    }

    public String WebDecrypt(String Input, String Key, String IV)
    {
        return DecryptStringWEB(Input, Key, IV);
    }
}