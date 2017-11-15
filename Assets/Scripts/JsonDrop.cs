using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Reflection;
using System.Net;

namespace Assets.Scripts {
    public class JsonDrop
    {
        // Name of the database to store data
        private readonly string database;
        // Url of the server to connect to
        private readonly string serverURL;
        // Cache to hold the name of the primary key value associated with a type
        private static Dictionary<Type, JsonDropTableData> knownTables = new Dictionary<Type, JsonDropTableData>();
        // Special table data structure to represent an error
        private static JsonDropTableData jsonDropTableDataError = new JsonDropTableData();
        // Default server for the connection to use
        private const string DEFAULT_SERVER = "http://jsnDrop.com/Q";

        private JsonDrop(string database, string serverURL = null)
        {
            this.database = database;
            this.serverURL = (serverURL == null ? DEFAULT_SERVER : serverURL);
        }

        public static JsonDrop newConnection(string databaseName) => new JsonDrop(databaseName);
        public static JsonDrop newConnection(string databaseName, string serverURL) => new JsonDrop(databaseName, serverURL);

        private string queryServer(string cmd, string value)
        {
            string output = JsonUtility.ToJson(new JsnDropMessage() { Message = "Failed", Type = "FAIL" });
            // Construct query string
            string query = string.Format("{0}?cmd={1}&value={2}", serverURL, cmd, value);
            Debug.Log(query);
            WebClient webClient = new WebClient();
            output = webClient.DownloadString(query);
            Debug.Log(output);
            return output;
        }

        private JsonDropTableData getTableData(object input)
        {
            return getTableData(input.GetType());
        }

        private JsonDropTableData getTableData(Type input)
        {
            if (!knownTables.ContainsKey(input) || knownTables[input].Equals(jsonDropTableDataError))
            {
                // If the client doesn't know where to find the table data on the server, it needs to find it
                JsnDropMessage message = JsonUtility.FromJson<JsnDropMessage>(queryServer("jsnReg", string.Format("{0},{1},{2}", database, input.Name, string.Format("{0}${1}",database, input.Name))));

                if (message.Message == "NEW" || message.Message == "EXISTS")
                {
                    if (knownTables.ContainsKey(input)) // If the table has been entered as an error, replace it
                        knownTables[input] = enterJsonDropTableData(message.Type, input);
                    else
                        knownTables.Add(input, enterJsonDropTableData(message.Type, input)); // If it's a completely new table, add it
                }
                else
                    throw new Exception(string.Format("[{0}] Failed to register.", input.Name));
            }
            // Return the table data
            return knownTables[input];
        }

        private JsonDropTableData enterJsonDropTableData(string connectionID, Type input)
        {
            bool objectIsValid = false;

            FieldInfo primaryKey = null;
            string name = input.Name;

            foreach (FieldInfo field in input.GetFields())
            {
                if (Attribute.IsDefined(field, typeof(JsonDropPrimaryKeyAttribute)))
                {
                    objectIsValid = true;
                    primaryKey = field;
                    break;
                }
            }
            return (objectIsValid ? new JsonDropTableData(connectionID, name, primaryKey) : jsonDropTableDataError);
        }

        /// <summary>
        /// Write data to the database - jsnPut command
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public JsonDropResult create(object input)
        {
            JsonDropResult output;
            try
            {
                JsonDropTableData tableData = getTableData(input);
                // jsnPut parameters are (Registered table id),(primary key value),(object as JSON)
                string queryParameters = string.Format("{0},{1},{2}", tableData.ID, getPrimaryKeyPattern(input), JsonUtility.ToJson(input));
                string queryResponse = queryServer("jsnPut", queryParameters);
                output = new JsonDropResult(JsonUtility.FromJson<JsnDropMessage>(queryResponse).Message == "OK", queryResponse);
            }
            catch(Exception e)
            {
                output = new JsonDropResult(false, "There was an error entering the data");
            }
            return output;
        }

        /// <summary>
        /// Read all rows from a table - jsnGet command
        /// </summary>
        /// <param name="inputKey"></param>
        public T[] read<T>()
        {
            T[] output;
            try
            { 
                JsonDropTableData tableData = getTableData(typeof(T));
                // jsnGet parameters are (Registered table id),([primary key value]) - primary key value is optional
                string queryParameters = tableData.ID;
                string queryResponse = queryServer("jsnGet", queryParameters);
                output = jsnGetToArray<T>(queryResponse);
            }
            catch(Exception e)
            {
                // Log the error and then return an empty array
                Debug.Log(e.Message);
                output = new T[] { };
            }
            return output;
        }

        /// <summary>
        /// Find an entity by its primary key - jsnGet command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputKey"></param>
        /// <returns></returns>
        public T[] read<T>(string inputKey)
        {
            T[] output;
            try
            { 
                JsonDropTableData tableData = getTableData(typeof(T));
                // jsnGet parameters are (Registered table id),([primary key value]) - primary key value is optional
                string queryParameters = string.Format("{0},{1}", tableData.ID, getFieldPattern(tableData.PrimaryKey, inputKey));
                string queryResponse = queryServer("jsnGet", queryParameters);
                output = jsnGetToArray<T>(queryResponse);
            }
            catch(Exception e)
            {
                // Log the error and then return an empty array
                Debug.Log(e.Message);
                output = new T[] { };
            }
            return output;
        }

        /// <summary>
        /// Find entities given a field and a value - jsnGet command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputField"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public T[] read<T>(string inputField, string inputValue)
        {
            T[] output;
            try
            {
                JsonDropTableData tableData = getTableData(typeof(T));
                // Make sure the field the user is check is a field of the table
                bool hasField = false;
                FieldInfo fieldToFind = null;
                foreach(FieldInfo f in typeof(T).GetFields())
                {
                    if(f.Name == inputField)
                    {
                        hasField = true;
                        fieldToFind = f;
                        break;
                    }
                }
                if (!hasField)
                    throw new Exception("Couldn't find the field asked for"); // Force the catch block to run
                else
                {
                    // jsnGet parameters are (Registered table id),([primary key value]) - primary key value is optional
                    string queryParameters = string.Format("{0},{1}", tableData.ID, getFieldPattern(fieldToFind, inputValue));
                    string queryResponse = queryServer("jsnGet", queryParameters);
                    output = jsnGetToArray<T>(queryResponse);
                }
            }
            catch (Exception e)
            {
                // Log the error and then return an empty array
                Debug.Log(e.Message);
                output = new T[] { };
            }
            return output;
        }

        /// <summary>
        /// Update data in the database - jsnPut command 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public JsonDropResult update(object input)
        {
            // The JsnDrop service being used uses the same command to create and update data
            return create(input);
        }

        /// <summary>
        /// Remove data in the database - jsnDel command
        /// </summary>
        public void delete()
        {
            // Unimplemented - Won't need it for this game 
        }

        /// <summary>
        /// Takes a string from the server deserialises all the objects within it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private T[] jsnGetToArray<T>(string input)
        {
            List<T> output = new List<T>();

            int openBraces = 0; // Count how many braces have been opened
            int startIndex = 0; // The index an object starts at
            char[] value = input.ToCharArray();
            for(int i = 0; i<value.Length; i++)
            {
                if (value[i] == '{')
                {
                    openBraces += 1; // Open a new brace
                    if (openBraces == 2) // Check 2, because 1 would mean only the outer wrapper has been found
                    {
                        startIndex = i; // Set start of this object
                    }
                }
                else if (value[i] == '}')
                {
                    if(openBraces == 2) // Check 2, because that means this is an outermost object's closing brace
                    {
                        string obj = input.Substring(startIndex, (i - startIndex)+1);
                        Debug.Log(obj);
                        output.Add(JsonUtility.FromJson<T>(obj));
                    }
                    openBraces -= 1; // Close an open brace.
                }
            }
            return output.ToArray();
        }


        /// <summary>
        /// Creates a pattern of ("primaryKeyFieldName":primaryKeyValue) for the input instance - can be passed to the server when inserting or updating an entity
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string getPrimaryKeyPattern(object input)
        {
            JsonDropTableData tableData = getTableData(input);
            string fieldName = string.Format("\"{0}\"", tableData.PrimaryKey.Name);
            string fieldValue = tableData.PrimaryKey.FieldType == typeof(String) ? string.Format("\"{0}\"", tableData.PrimaryKey.GetValue(input).ToString()) : tableData.PrimaryKey.GetValue(input).ToString();
            return string.Format("{0}:{1}", fieldName, fieldValue);
        }

        /// <summary>
        /// Create a pattern of ("fieldName":fieldValue) for the input instance - can be passed to the server when looking for entities
        /// </summary>
        /// <param name="inputField"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private string getFieldPattern(FieldInfo inputField, string input)
        {
            string fieldName = string.Format("\"{0}\"", inputField.Name);
            string fieldValue = inputField.FieldType == typeof(String) ? string.Format("\"{0}\"", input) : input;
            return string.Format("{0}:{1}", fieldName, fieldValue);
        }

        /// <summary>
        /// Data structure to represent a message from the server - Use this in server query implementation
        /// </summary>
        [Serializable]
        private struct JsnDropMessage
        {
            public string Message;
            public string Type;
        }
        /// <summary>
        /// Data structure to represent a message from this service - Use this to pass information back to the client viewmodel
        /// </summary>
        public struct JsonDropResult
        {
            public bool transactionSuccess;
            public string resultMessage;

            public JsonDropResult(bool transactionSuccess, string resultMessage)
            {
                this.transactionSuccess = transactionSuccess;
                this.resultMessage = resultMessage;
            }
        }
        /// <summary>
        /// Data structure to represent server table data, most importantly what field is the primary key and what the regestered ID is
        /// </summary>
        private struct JsonDropTableData
        {
            // The ID of the table in the schema
            public string ID;
            // The Name of the table in the schema
            public string Name;
            // The name of the primary key field
            public FieldInfo PrimaryKey;

            public JsonDropTableData(string ID, string Name, FieldInfo PrimaryKey)
            {
                this.ID = ID;
                this.Name = Name;
                this.PrimaryKey = PrimaryKey;
            }
        }
        /// <summary>
        /// Attribute to apply to the primary key field of data entities
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class JsonDropPrimaryKeyAttribute : Attribute
        {
        }
    }
}


