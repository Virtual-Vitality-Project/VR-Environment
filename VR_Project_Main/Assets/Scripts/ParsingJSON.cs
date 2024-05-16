using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Unity.Mathematics;

// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
public class Eigenschap
{
    public int id { get; set; }
    // POSITION
    public float positie_X { get; set; }
    public float positie_Y { get; set; }
    public float positie_Z { get; set; }
    // ROTATION
    public float rotation_X { get; set; }
    public float rotation_Y { get; set; }
    public float rotation_Z { get; set; }
    // SCALE
    public float scale_X { get; set; }
    public float scale_Y { get; set; }
    public float scale_Z { get; set; }
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
                        var eigenschap = item.eigenschap;
                        Vector3 positie = new Vector3(eigenschap.positie_X, eigenschap.positie_Y, eigenschap.positie_Z);
                        Vector3 rotation = new Vector3(eigenschap.rotation_X, eigenschap.rotation_Y, eigenschap.rotation_Z);
                        Vector3 scale = new Vector3(eigenschap.scale_X, eigenschap.scale_Y, eigenschap.scale_Z);
                        GameObject spawnedPrefab = Instantiate(prefabToSpawn, positie, Quaternion.Euler(rotation));
                        spawnedPrefab.transform.localScale = scale;


                        // Toevoegen van materiaal
                        Material material = Resources.Load<Material>("Materials/" + item.materiaal.naam); // "Materials/" is de map in Resources


                        if (material != null)
                        {
                            // Parse de RGBA-waarde naar een Color object
                            Color color = ParseColor(item.kleur.naam);

                            // Toepassen van de kleur op het materiaal
                            material.color = color;

                            Renderer renderer = spawnedPrefab.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                renderer.material = material;
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

                        // Toewijzen van scripts
                        foreach (var scriptItem in item.scripts)
                        {
                            string scriptPath = "Scripts/" + scriptItem.script.naam; // "Scripts/" is de map in Resources
                            GameObject scriptObject = Resources.Load<GameObject>(scriptPath);
                            if (scriptObject != null)
                            {
                                scriptObject.AddComponent(Type.GetType(scriptItem.script.naam));
                            }
                            else
                            {
                                Debug.LogWarning("Script not found at path: " + scriptPath);
                            }
                        }
                    }
                }
            }


        }
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