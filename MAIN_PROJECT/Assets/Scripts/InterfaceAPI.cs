/*
InterfaceAPI class
Written by Declan Ross
21/4/24

Description:
This class is a static class that acts as a monolithic controller for the accessing of 
information from the penushost server. As the class is static, it can be accessed from anywhere.

For example, getting the getting user instance (User class) can be retrieved using this.
InterfaceAPI.currentUser

Likewise, methods can be called from other scripts to update the current users information,
an example of this is when the game first loads and the LoginManager calls LoginPost

InterfaceAPI.LoginPost();

In the future, the InterfaceAPI should not control the Scenes, instead it should just return if the operation was a success
*/

using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using System.Runtime.Serialization;

public static class InterfaceAPI
{
    public static string url = "https://penushost.ddns.net";


    public class ObjectFromJSON
    {
        public string name;
        public BuildingInstance[] InnerBuildingObjects;
    }

    //Proceed to next scene, this needs to moved out of here, InterfaceAPI should not be the controller of the game, instead its methods should return if the operation was a success or not
    private static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }



    private static User currentUser;
    
    public static int getUserID()
    {
        return currentUser.user_id;
    }
    public static string getUsername()
    {
        return currentUser.username;
    }
    public static string getSeshToken()
    {
        return currentUser.sesh_token;
    }
    public static int getLevel()
    {
        return currentUser.level;
    }
    public static int getCoins()
    {
        return currentUser.coins;
    }

    public static IEnumerator setUsername(string usernameToSet)
    {
        Debug.Log("Set username");
        IEnumerator response = UpdateUserProperty("username", usernameToSet);
        while (response.MoveNext())
        {
            yield return response.Current;
        }
    }
    public static IEnumerator setLevel(int levelToSet)
    {
        string levelToSetString = levelToSet.ToString();
        Debug.Log("Set level");
        IEnumerator response = UpdateUserProperty("coins", levelToSetString);
        while (response.MoveNext())
        {
            yield return response.Current;
        }
    }
    public static IEnumerator setCoins(int coinsToSet)
    {
        string coinsToSetString = coinsToSet.ToString();
        Debug.Log("Set coins");
        IEnumerator response = UpdateUserProperty("coins", coinsToSetString);
        while (response.MoveNext())
        {
            yield return response.Current;
        }
    }

    public static List<BuildingInstance> buildingList = new List<BuildingInstance>();
    //private static User userReceived;
    //private Response serverResponse;
    /*void Start()
    {
        //Retired
        //StartCoroutine(GetRequest("https://penushost.ddns.net/api"));       //Test example
        
        //Active
        //StartCoroutine(LoginPost("https://penushost.ddns.net/login", "{\"username\": \"dec5star\"}"));
        //StartCoroutine(GetBasePost("https://penushost.ddns.net/getbase",  "{\"sesh_id\": \"abcdefg\", \"user_id\": 0}"));
    }*/



    //Needs to be re written for generic get reqs
    /*IEnumerator GetRequest(string uri)      //This function tests the connection using a get request and converts the results to an obect
    {
        //Create GET request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            
            yield return webRequest.SendWebRequest();                                                   //Send the get request
            switch (webRequest.result)                                                                  //When request arrives
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Something went wrong: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:                                                    //Information recieved successfully 
                    string jsonRaw = webRequest.downloadHandler.text;                                   //Get RAW JSON
                    string strippedString = StripSquareBrackets(jsonRaw);                               //Strip [] array brackets as it is sent as an array
                    //DataPacket data = JsonUtility.FromJson<DataPacket>(strippedString);                 //Deserialize JSON string into a DataPacket object      
                    Debug.Log("ID: " + data.id);                                                        //Now you can access the properties of the DataPacket object
                    Debug.Log("First Name: " + data.first_name);
                    Debug.Log("Last Name: " + data.last_name);
                    Debug.Log("Role: " + data.role);
                    //text.text = data.first_name;
                    break;
            }
        }
    }*/

    //Getters and Setters for currentUser

    public static IEnumerator LoginPost(string usernameArg, string passwordArg)    //This function sends a POST request to a specified URI with a JSON payload (FUTURE, JSON should be created in thisfnuction)
    {
        string uri = url + "/login";
        string jsonData = ("{\"username\": \"" + usernameArg + "\", \"password\": \"" + passwordArg + "\"}");
        Response serverResponse;
        User userReceived = new User();
        //Create POST request
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(uri, "application/json"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);          //Convert JSON to byte array
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);                    //Attach JSON to request
            webRequest.SetRequestHeader("Content-Type", "application/json");                // Set the content type
            yield return webRequest.SendWebRequest();                                       //Send the actual request
            switch (webRequest.result)                                                                  //When request arrives
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Something went wrong: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:                                                    //Information recieved successfully 
                    string jsonRaw = webRequest.downloadHandler.text;
                    //Debug.Log("Raw Response: " + jsonRaw);
                    bool isResponseUser = false;
                    bool isResponseResponse = false;
                    
                    if (jsonRaw == "[]")
                    {
                        //Response is empty
                        Debug.LogError("Empty SQL query recieved ([])");
                        break;
                    }

                    //Try Covert JSON to Response
                    try
                    {
                        serverResponse = JsonUtility.FromJson<Response>(jsonRaw);
                        if (serverResponse == null)
                        {
                            throw new Exception("Null serverResponse variable (Not Response type)");
                        }
                        isResponseResponse = true;
                        
                    }
                    catch (Exception)
                    {
                        //Debug.Log(e);
                        serverResponse = null;
                    }
                    
                    //Try Convert JSON to User
                    
                    try
                    {
                        if (isResponseResponse)
                        {

                        }
                        else
                        {
                            string strippedString = StripSquareBrackets(jsonRaw);
                            
                            userReceived = JsonUtility.FromJson<User>(strippedString);
                            if (userReceived == null)
                            {
                                throw new Exception("Null userReceived variable (Not User Type)");
                            }
                            isResponseUser = true;
                        }
                    }
                    catch (Exception)
                    {
                        //Debug.Log(e);
                    }
                    
                    if (isResponseUser)
                    {
                        //Response is User type
                        
                        //userReceived.printDetails();
                        currentUser = userReceived;
                        IEnumerator getBaseMethod = GetBase();
                        while (getBaseMethod.MoveNext())
                        {
                            yield return getBaseMethod.Current;
                        }

                        Debug.Log("Successful Login: " + InterfaceAPI.getUsername());
                        LoadScene("MainMenuScene");
                    }
                    else if (isResponseResponse)
                    {
                        //Response is a Response type
                        Debug.Log("Successful Response Recieved");
                        serverResponse.printResponse();
                    }
                    else
                    {
                        //Response is not Response type or User Type
                    }
                    break;
            }
            
            
        }
    }

    public static IEnumerator GetBase()
    {
        string uri = url + "/getbase";
        string jsonData = "{\"sesh_id\": \"" + currentUser.sesh_token + "\", \"user_id\": " + currentUser.user_id + "}";

        Response serverResponse;
        
        //Create POST request
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(uri, "application/json"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);          //Convert JSON to byte array
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);                    //Attach JSON to request
            webRequest.SetRequestHeader("Content-Type", "application/json");                // Set the content type
            yield return webRequest.SendWebRequest();                                       //Send the actual request
            switch (webRequest.result)                                                                  //When request arrives
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Something went wrong: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:                                                    //Information recieved successfully 
                    string jsonRaw = webRequest.downloadHandler.text;
                    //Debug.Log("JSON");
                    //Debug.Log(jsonRaw);
                    
                    bool isResponseBuildingListObj = false;
                    bool isResponseResponse = false;
                    
                    if (jsonRaw == "[]")
                    {
                        //Response is empty
                        Debug.LogError("Empty SQL query recieved ([])");
                        break;
                    }

                    //Try Covert JSON to Response
                    try
                    {
                        serverResponse = JsonUtility.FromJson<Response>(jsonRaw);
                        if (serverResponse == null)
                        {
                            throw new Exception("Null serverResponse variable (Not Response type)");
                        }
                        isResponseResponse = true;
                        
                    }
                    catch (Exception)
                    {
                        
                        //Debug.Log(e);
                        serverResponse = null;
                    }
                    
                    //Try Convert JSON to User
                    try
                    {
                        if (isResponseResponse)
                        {

                        }
                        else
                        {
                            //string strippedString = StripSquareBrackets(jsonRaw);
                            buildingList.Clear();
                            string appendedJson = "{\"name\": \"name\",\"InnerBuildingObjects\":" + jsonRaw + "}";
                            ObjectFromJSON buildingListObj = JsonUtility.FromJson<ObjectFromJSON>(appendedJson);

                            Debug.Log("Building Count: " + buildingListObj.InnerBuildingObjects.Length);
                            for (int i = 0; i < buildingListObj.InnerBuildingObjects.Length; i++)
                            {
                                buildingList.Add(buildingListObj.InnerBuildingObjects[i]);
                            }

                            if (buildingListObj == null)
                            {
                                throw new Exception("Null buildingListObj variable (Not User Type)");
                            }
                            isResponseBuildingListObj = true;
                        }
                    }
                    catch (Exception)
                    {
                        //Debug.Log(e);
                    }
                    
                    if (isResponseBuildingListObj)
                    {
                        //Response is building list obj
                        Debug.Log("Building List Recieved Successfully");
                        foreach (BuildingInstance buildInst in buildingList)
                        {
                            buildInst.printDetails();
                        }
                    }
                    else if (isResponseResponse)
                    {
                        //Response is a Response type
                        Debug.Log("Successful Response Recieved");
                        serverResponse.printResponse();
                    }
                    else
                    {
                        //Response is not Response type or User Type
                    }
                    break;
            }
        }
    }

    public static IEnumerator UpdateUserProperty(string propertyName, string newValue)
    {
        string uri = url + "/updateuserproperty";
        string jsonData = ("{\"sesh_id\": \"" + currentUser.sesh_token + "\", \"user_id\": \"" + currentUser.user_id + "\", \"property_to_update\": \"" + propertyName + "\", \"new_property_value\": \"" + newValue + "\"}");
        Response serverResponse;

        //Create POST request
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(uri, "application/json"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);          //Convert JSON to byte array
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);                    //Attach JSON to request
            webRequest.SetRequestHeader("Content-Type", "application/json");                // Set the content type
            yield return webRequest.SendWebRequest();                                       //Send the actual request
            switch (webRequest.result)                                                                  //When request arrives
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Something went wrong: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:                                                    //Information recieved successfully 
                    string jsonRaw = webRequest.downloadHandler.text;
                    Debug.Log("Raw Response: " + jsonRaw);
                    
                    if (jsonRaw == "[]")
                    {
                        //Response is empty
                        Debug.LogError("Empty SQL query recieved ([])");
                        break;
                    }

                    //Try Covert JSON to Response
                    try
                    {
                        serverResponse = JsonUtility.FromJson<Response>(jsonRaw);
                        if (serverResponse == null)
                        {
                            throw new Exception("Null serverResponse variable (Not Response type)");
                        }
                        //Response is a Response type
                        Debug.Log("Successful Response Recieved");
                        serverResponse.printResponse();
                        
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        serverResponse = null;
                    }
                    break;
            }
        }

        
    }
    private static string StripSquareBrackets(string input)
    {
        if (input.Length < 2)
        {
            // If the string has less than 2 characters, return it as is.
            return input;
        }
        else
        {
            // Check if the first and last characters are square brackets, and remove them if they are.
            if (input[0] == '[' && input[input.Length - 1] == ']')
            {
                return input.Substring(1, input.Length - 2);
            }
            else
            {
                // If the string does not start and end with square brackets, return it as is.
                return input;
            }
        }
    }
}