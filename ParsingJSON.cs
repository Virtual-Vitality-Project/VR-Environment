using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Mathematics;
using Unity.VisualScripting;
using System.ComponentModel;


// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
public class Eigenschap
{
    public int id { get; set; }
    public double positie_X { get; set; }
    public double positie_Y { get; set; }
    public double positie_Z { get; set; }
    public int materiaalID { get; set; }
    public int kleurID { get; set; }
}

public class Gebruiker
{
    public int id { get; set; }
    public string voornaam { get; set; }
    public string achternaam { get; set; }
    public string email { get; set; }
    public string wachtwoord { get; set; }
}

public class Kleur
{
    public int id { get; set; }
    public string naam { get; set; }
}

public class Materiaal
{
    public int id { get; set; }
    public string naam { get; set; }
}

public class Prefab
{
    public int id { get; set; }
    public string naam { get; set; }
    public string description { get; set; }
    public string path { get; set; }
}

public class Root
{
    public Gebruiker gebruiker { get; set; }
    public Scene scene { get; set; }
    public Prefab prefab { get; set; }
    public Eigenschap eigenschap { get; set; }
    public Materiaal materiaal { get; set; }
    public Kleur kleur { get; set; }
    public List<Script> scripts { get; set; }
}

public class Scene
{
    public int id { get; set; }
    public string naam { get; set; }
    public string script { get; set; }
    public DateTime tijdstip { get; set; }
    public int omgevingID { get; set; }
}

public class Script
{
    public Script2 script { get; set; }
}

public class Script2
{
    public int id { get; set; }
    public string naam { get; set; }
}


public class ParsingJSON : MonoBehaviour
{
    public string apiUrl = "http://localhost:5153/odata/Objecten"; // De URL van de API
    public string email = "test@gmail.com";
    public string password = "wachtwoord123";
    public int SceneID = 25;
    public GameObject[] cubePrefabs; // Array van prefabs voor de kubussen
    public GameObject bed1;

    IEnumerator Start()
    {
        string auth = email + ":" + password;
        string authEncoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(auth));
        string authHeader = "Basic " + authEncoded;

        string apiUrlWithQuery = apiUrl + "?SceneID=" + SceneID;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrlWithQuery))
        {
            webRequest.SetRequestHeader("Authorization", authHeader);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Web request error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                Debug.Log(json);
                List<Root> myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(json);

                foreach (var item in myDeserializedClass)
                {
                    if (item.prefab != null && item.eigenschap != null && item.kleur != null && item.materiaal != null && item.scripts != null)
                    {
                        // Laden van het prefab
                        string prefabPath = "Prefabs/" + item.prefab.naam; // "Prefabs/" is de map in Resources
                        GameObject prefabToSpawn = Resources.Load<GameObject>(prefabPath);

                        if (prefabToSpawn == null)
                        {
                            Debug.LogError("Prefab not found at path: " + prefabPath);
                            continue; // Ga door naar het volgende item als prefab niet is gevonden
                        }

                        // Instantiëren van het prefab
                        GameObject spawnedPrefab = Instantiate(prefabToSpawn, new Vector3((float)item.eigenschap.positie_X,(float)item.eigenschap.positie_Y,(float)item.eigenschap.positie_Z),quaternion.identity);

                        // Toevoegen van materiaal
                        Material material = Resources.Load<Material>("Materials/" + item.materiaal.naam); // "Materials/" is de map in Resources


                        if (material != null)
                        {
                            // Instantiate a new instance of the material
                            Material newMaterial = Instantiate(material);

                            // Parse de RGBA-waarde naar een Color object
                            Color color = ParseColor(item.kleur.naam);

                            // Toepassen van de kleur op het nieuwe materiaal
                            newMaterial.color = color;

                            Renderer renderer = spawnedPrefab.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                renderer.material = newMaterial;
                            }
                            else
                            {
                                Debug.LogWarning(item.prefab.naam + "Does not have a Renderer component to apply the material.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Material not found with name: " + item.materiaal.naam);
                        }
                        foreach (Script script in item.scripts)
                        {
                            // Laad alle scripts uit de Resources map
                            TextAsset scriptAssets = Resources.Load<TextAsset>("Scripts/" + script.script.naam);

                            /*foreach (TextAsset scriptAsset in scriptAssets)
                            {*/
                                // Controleer of het script niet null is
                                if (scriptAssets != null)
                                {
                                    // Bepaal de typenaam op basis van de scriptnaam
                                    string typeName = scriptAssets.name;

                                    // Zoek het type op basis van de typenaam
                                    Type scriptType = Type.GetType(typeName);
                                    if (scriptType != null)
                                    {
                                        // Voeg het script toe aan het gameobject
                                        var component = spawnedPrefab.AddComponent(scriptType);
                                        Debug.Log("Script added to gameobject: " + typeName);
                                        if (typeName == "Interactable")
                                        {
                                            Interactable interactable = component as Interactable;
                                            if (interactable != null)
                                            {
                                                interactable.prefab = bed1;
                                                Debug.Log("Prefab attached to Interactable script");
                                            }
                                            
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Type not found for script: " + typeName);
                                    }

                                }
                                else
                                {
                                    Debug.LogWarning("Script asset is null");
                                }
                            /*}*/
                        }

                    }
                }
            }


        }
    }

    // Log alle bestanden in de Resources map
    void LogResources()
    {
        string[] paths = GetAllResourcePaths();
        foreach (string path in paths)
        {
            Debug.Log("Resource found: " + path);
        }
    }

    // Haal alle bestandspaden op binnen de Resources map
    string[] GetAllResourcePaths()
    {
        List<string> paths = new List<string>();

        // Array van submappen binnen de Resources map waarin je wilt zoeken
        string[] folders = { "" };

        foreach (string folder in folders)
        {
            // Laad alle objecten binnen de huidige map en submappen
            UnityEngine.Object[] objects = Resources.LoadAll(folder);

            foreach (UnityEngine.Object obj in objects)
            {
                // Controleer of het object een script is en of het de gewenste naam heeft
                if (obj is TextAsset textAsset && (textAsset.name == "Interactable" || textAsset.name == "Physics"))
                {
                    // Bepaal het pad van het object binnen de Resources map
                    string path = folder + "/" + textAsset.name;
                    paths.Add(path);
                }
            }
        }

        return paths.ToArray();
    }


    // Hulpmethode om een RGBA-string naar een Color object te parsen
    private Color ParseColor(string rgbaString)
    {
        string[] rgba = rgbaString.Split(',');
        float r, g, b, a;
        if (rgba.Length == 4 && float.TryParse(rgba[0], out r) && float.TryParse(rgba[1], out g) && float.TryParse(rgba[2], out b) && float.TryParse(rgba[3], out a))
        {
            return new Color(r, g, b, a);
        }
        else
        {
            Debug.LogWarning("Invalid RGBA string: " + rgbaString);
            return Color.white; // Standaardkleur als de string ongeldig is
        }
    }
}