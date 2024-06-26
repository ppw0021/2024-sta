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
using Unity.VisualScripting;
using System.Linq;

public static class InterfaceAPI
{
    public static bool multiplayerViewStart = false;
    private static string url = "https://penushost.ddns.net";
    [SerializeField]
    private static string mainMenuScene = "GridPlacementSystem";
    public static string getMainMenuSceneName()
    {
        return mainMenuScene;
    }
    public static List<BuildingPrefab> buildingPrefabs = new List<BuildingPrefab>();
    public static void LoadPrefabs()
    {
        buildingPrefabs.Clear();
        buildingPrefabs.Add(new BuildingPrefab(0, "Path", 1, 1, 99));
        buildingPrefabs.Add(new BuildingPrefab(1, "House", 1, 1, 99));
        buildingPrefabs.Add(new BuildingPrefab(2, "Windmill", 1, 1, 99));
        buildingPrefabs.Add(new BuildingPrefab(3, "Townhall", 1, 1, 99));
    }
    public class BuildingPrefab 
    {
        public int structure_id;
        public string building_name;
        public int x_width;
        public int y_width;
        public int max_count;

        public BuildingPrefab(int structure_id, string building_name, int x_width, int y_width, int max_count)
        {
            this.structure_id = structure_id;
            this.building_name = building_name;
            this.x_width = x_width;
            this.y_width = y_width;
            this.max_count = max_count;
        }
    }
    public class ObjectFromJSON
    {
        public string name;
        public BuildingInstance[] InnerBuildingObjects;
    }
    [System.Serializable]
    public class UseridInstance
    {
        public string user_id;
    }
    public class ListOfUseridsFromJSON
    {
        public string name;
        public UseridInstance[] InnerUseridObjects;
    }
    //Proceed to next scene, this needs to moved out of here, InterfaceAPI should not be the controller of the game, instead its methods should return if the operation was a success or not
    private static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    private static MonoBehaviour monoBehaviourInstance;
    public static void Initialize(MonoBehaviour monoBehaviour)
    {
        monoBehaviourInstance = monoBehaviour;
    }
    public static int getXp()
    {
        return currentUser.xp;
    }
    public static bool addXp(int xpToAdd)
    {
        int currentXp = getXp();
        int targetXp = CalculateTargetExp(getLevel());
        int newLevel = getLevel();
        int newXp;
        if ((currentXp + xpToAdd) >= targetXp)
        {
            //Level up
            newLevel++;
            newXp = (currentXp + xpToAdd) - targetXp;
        }
        else
        {
            newXp = currentXp + xpToAdd;
        }
        if (monoBehaviourInstance == null)
        {
            Debug.LogError("MonoBehaviour instance not set. Call Initialize() first.");
            return false;
        }

        // Call the coroutine using the MonoBehaviour instance
        currentUser.level = newLevel;
        monoBehaviourInstance.StartCoroutine(UpdateUserProperty("level", newLevel.ToString()));
        currentUser.xp = newXp;
        monoBehaviourInstance.StartCoroutine(UpdateUserProperty("xp", newXp.ToString()));
        return true;
    }
    private static int CalculateTargetExp(int level)
    {
        return 100 + (level * 50); // Adjust this formula as needed
    }
    public static void printUser()
    {
        currentUser.printDetails();
    }
    public static User currentUser = new User();
    public static List<BuildingInstance> buildingList = new List<BuildingInstance>();
    public static List<int> useridList = new List<int>();
    public static List<User> userList = new List<User>();
    public static List<Base> baseList = new List<Base>();
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
    public static bool setLevel(int levelToSet)
    {
        if (monoBehaviourInstance == null)
        {
            Debug.LogError("MonoBehaviour instance not set. Call Initialize() first.");
            return false;
        }

        // Call the coroutine using the MonoBehaviour instance
        currentUser.level = levelToSet;
        monoBehaviourInstance.StartCoroutine(UpdateUserProperty("level", levelToSet.ToString()));
        return true;
    }
    public static bool setCoins(int coinsToSet)
    {
        if (monoBehaviourInstance == null)
        {
            Debug.LogError("MonoBehaviour instance not set. Call Initialize() first.");
            return false;
        }

        // Call the coroutine using the MonoBehaviour instance
        currentUser.coins = coinsToSet;
        monoBehaviourInstance.StartCoroutine(UpdateUserProperty("coins", coinsToSet.ToString()));
        return true;
    }
    public static bool setXp(int xpToSet)
    {
        if (monoBehaviourInstance == null)
        {
            Debug.LogError("MonoBehaviour instance not set. Call Initialize() first.");
            return false;
        }

        // Call the coroutine using the MonoBehaviour instance
        currentUser.xp = xpToSet;
        monoBehaviourInstance.StartCoroutine(UpdateUserProperty("xp", xpToSet.ToString()));
        return true;
    }
    public static bool addBuilding(int structure_id, int x_pos, int y_pos)
    {
        
        if (monoBehaviourInstance == null)
        {
            Debug.LogError("MonoBehaviour instance not set. Call Initialize() first.");
            return false;
        }

        // Call the coroutine using the MonoBehaviour instance
        BuildingPrefab bPrefab = buildingPrefabs[structure_id];
        BuildingInstance buildingInstanceToAdd = new BuildingInstance("-1", structure_id.ToString(), bPrefab.building_name, x_pos.ToString(), y_pos.ToString(), getUsername());
        buildingList.Add(buildingInstanceToAdd);
        monoBehaviourInstance.StartCoroutine(InterfaceAPI.PlaceBuilding(structure_id, x_pos, y_pos));
        monoBehaviourInstance.StartCoroutine(InterfaceAPI.GetBase(currentUser.user_id));
        return true;
    }
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
                        IEnumerator getBaseMethod = GetBase(currentUser.user_id);
                        while (getBaseMethod.MoveNext())
                        {
                            yield return getBaseMethod.Current;
                        }

                        Debug.Log("Successful Login: " + InterfaceAPI.getUsername());
                        //currentUser.printDetails();
                        LoadScene(mainMenuScene);
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

    public static IEnumerator GetAndAddUserToList(int user_id)    //This function sends a POST request to a specified URI with a JSON payload (FUTURE, JSON should be created in thisfnuction)
    {
        string uri = url + "/getuser";
        string jsonData = ("{\"user_id\": \"" + user_id + "\"}");
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
                        //currentUser = userReceived;
                        userList.Add(userReceived);
                        //IEnumerator getBaseMethod = GetBase();
                        //while (getBaseMethod.MoveNext())
                        //{
                            //yield return getBaseMethod.Current;
                        //}

                        Debug.Log("Added User: " + userReceived.username);
                        //currentUser.printDetails();
                        //LoadScene(mainMenuScene);
                    }
                    else if (isResponseResponse)
                    {
                        //Response is a Response type
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

    public static IEnumerator AddUser(string usernameArg, string passwordArg, string answerArg)    //This function sends a POST request to a specified URI with a JSON payload (FUTURE, JSON should be created in thisfnuction)
    {
        string uri = url + "/adduser";
        string jsonData = "{\"username\": \"" + usernameArg + "\", \"password\": \"" + passwordArg + "\", \"sesh_token\": \"ABCDEFG\", \"coins\": 1000}";
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
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        serverResponse = null;
                    }
                    
                    if (isResponseResponse)
                    {
                        //Response is a Response type
                        Debug.Log("Successful Response Recieved");
                        serverResponse.printResponse();
                        if (serverResponse.response == "success")
                        {
                            Debug.Log("Added Successfully");
                        }
                    }
                    break;
            }
        }
    }

    public static IEnumerator GetUserNoPassword(string usernameArg)    //This function sends a POST request to a specified URI with a JSON payload (FUTURE, JSON should be created in thisfnuction)
    {
        string uri = url + "/login";
        string jsonData = ("{\"username\": \"" + usernameArg + "\"}");
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
                        IEnumerator getBaseMethod = GetBase(currentUser.user_id);
                        while (getBaseMethod.MoveNext())
                        {
                            yield return getBaseMethod.Current;
                        }

                        Debug.Log("Successful User Retreival: " + InterfaceAPI.getUsername());
                        currentUser.printDetails();
                        //LoadScene(mainMenuScene);
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

    public static IEnumerator UpdateUser()
    {
        string uri = url + "/updateuser";
        string jsonData = ("{\"user_id\": \"" + getUserID() + "\", \"sesh_id\": \"" + getSeshToken() + "\"}");
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
                        /*IEnumerator getBaseMethod = GetBase();
                        while (getBaseMethod.MoveNext())
                        {
                            yield return getBaseMethod.Current;
                        }*/

                        Debug.Log("Successful UserUpdate: " + InterfaceAPI.getUsername());
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
    public static IEnumerator GetUseridList()
    {
        string uri = url + "/getalluserids";
        Response serverResponse;
    
        // Create GET request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");                // Set the content type
            yield return webRequest.SendWebRequest();                                       // Send the actual request
            switch (webRequest.result)                                                      // When request arrives
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(string.Format("Error: {0}", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:                                        // Information received successfully 
                    string jsonRaw = webRequest.downloadHandler.text;
                
                    bool isResponseUseridlist = false;
                    bool isResponseResponse = false;
                
                    if (jsonRaw == "[]")
                    {
                        // Response is empty
                        Debug.LogError("Empty SQL query received ([])");
                        break;
                    }

                    // Try Convert JSON to Response
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
                        serverResponse = null;
                    }
                
                    // Try Convert JSON to User
                    try
                    {
                        if (isResponseResponse)
                        {

                        }
                        else
                        {
                            LoadPrefabs();
                            useridList.Clear();
                            string appendedJson = "{\"name\": \"name\",\"InnerUseridObjects\":" + jsonRaw + "}";
                            Debug.Log("Userid List: " + appendedJson);
                            ListOfUseridsFromJSON useridListOBJ = JsonUtility.FromJson<ListOfUseridsFromJSON>(appendedJson);

                            for (int i = 0; i < useridListOBJ.InnerUseridObjects.Length; i++)
                            {
                                useridList.Add(int.Parse(useridListOBJ.InnerUseridObjects[i].user_id));
                            }

                            if (useridListOBJ == null)
                            {
                                throw new Exception("Null usernameListOBJ variable (Not User Type)");
                            }
                            isResponseUseridlist = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                
                    if (isResponseUseridlist)
                    {
                        // Response is username list
                        Debug.Log("Username List Received Successfully");
                    }
                    else if (isResponseResponse)
                    {
                        // Response is a Response type
                        LoadPrefabs();
                        Debug.Log("Successful Response Received");
                        serverResponse.printResponse();
                    }
                    else
                    {
                        // Response is not Response type or User Type
                    }
                    break;
            }
        }
    }

    public static IEnumerator GetAllBases(int currentUserNameToCheck)
    {

        string uri = url + "/getbase";
        string jsonData = "{\"sesh_id\": \"" + currentUser.sesh_token + "\", \"user_id\": " + currentUserNameToCheck + "}";
        //Debug.Log(jsonData);

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
                            LoadPrefabs();
                            //buildingList.Clear();
                            string appendedJson = "{\"name\": \"name\",\"InnerBuildingObjects\":" + jsonRaw + "}";
                            
                            ObjectFromJSON buildingListObj = JsonUtility.FromJson<ObjectFromJSON>(appendedJson);

                            //Debug.Log("Building Count: " + buildingListObj.InnerBuildingObjects.Length);
                            baseList.Add(new Base(currentUserNameToCheck));
                            int currentIndex = baseList.Count - 1;
                            for (int i = 0; i < buildingListObj.InnerBuildingObjects.Length; i++)
                            {
                                baseList[currentIndex].buildingList.Add(buildingListObj.InnerBuildingObjects[i]);

                            }

                            if (buildingListObj == null)
                            {
                                throw new Exception("Null buildingListObj variable (Not User Type)");
                            }
                            isResponseBuildingListObj = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                    
                    if (isResponseBuildingListObj)
                    {
                        //Response is building list obj
                        //Debug.Log("Building List Recieved Successfully");
                        foreach (BuildingInstance buildInst in buildingList)
                        {
                            //buildInst.printDetails();
                        }
                    }
                    else if (isResponseResponse)
                    {
                        //Response is a Response type
                        LoadPrefabs();
                        //Debug.Log("Successful Response Recieved");
                        //Debug.Log(serverResponse.response);
                        //serverResponse.printResponse();
                    }
                    else
                    {
                        //Response is not Response type or User Type
                    }
                    break;
            }
        
        }

    }
    public static IEnumerator GetBase(int userIdBaseToLoad)
    {
        string uri = url + "/getbase";
        string jsonData = "{\"user_id\": " + userIdBaseToLoad + "}";

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
                            LoadPrefabs();
                            buildingList.Clear();
                            string appendedJson = "{\"name\": \"name\",\"InnerBuildingObjects\":" + jsonRaw + "}";
                            Debug.Log("Building List: " + appendedJson);
                            ObjectFromJSON buildingListObj = JsonUtility.FromJson<ObjectFromJSON>(appendedJson);

                            //Debug.Log("Building Count: " + buildingListObj.InnerBuildingObjects.Length);
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
                        //Debug.Log("Building List Recieved Successfully");
                        foreach (BuildingInstance buildInst in buildingList)
                        {
                            //buildInst.printDetails();
                        }
                    }
                    else if (isResponseResponse)
                    {
                        //Response is a Response type
                        LoadPrefabs();
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
    private static IEnumerator UpdateUserProperty(string propertyName, string newValue)
    {
        string uri = url + "/updateuserproperty";
        string jsonData = "{\"sesh_id\": \"" + currentUser.sesh_token + "\", \"user_id\": \"" + currentUser.user_id + "\", \"property_to_update\": \"" + propertyName + "\", \"new_property_value\": \"" + newValue + "\"}";
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
    public static IEnumerator UpdatePassword(string propertyName, string newValue, string username_arg)
    {
        string uri = url + "/updateuserpassword";
        string jsonData = "{\"username\": \"" + username_arg + "\", \"property_to_update\": \"" + propertyName + "\", \"new_property_value\": \"" + newValue + "\"}";
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
    private static IEnumerator PlaceBuilding(int structure_id, int x_pos, int y_pos)
    {
        string uri = url + "/placebuilding";
        string jsonData = "{" +
            "\"user_id\":" + currentUser.user_id + "," +
            "\"sesh_id\":\"" + currentUser.sesh_token + "\"," +
            "\"structure_id\":" + structure_id + "," +
            "\"x_pos\":" + x_pos + "," +
            "\"y_pos\":" + y_pos +
            "}";        
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
