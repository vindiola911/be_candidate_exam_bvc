using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Globalization;
using System.Dynamic;

namespace CSVtoJSONExperience {

    class Program {

        // Create read prompt constant of what the 
        const string _readPrompt = "console> ";

        // To hold the multiple parts of the command
        static List<string> commandParts = new List<string>{};

        // Keep track of processed files
        static List<string> proccesedFiles = new List<string>();

        // To watch for files to convert
        static FileSystemWatcher watcher = new FileSystemWatcher();

        // Vars to store respective output paths
        static string jsonOutputPath;
        static string errorOutputPath;

        static void Main(string[] args) {
            // Set title of this console application
            Console.Title = typeof(Program).Name;

            // Show welcome message and instructions to user
            WriteToConsole("Welcome to the CSV to JSON Experience!");
            WriteToConsole("Listen for csv files to convert to json by running this command: csvtojson /path/to/listen/ /path/to/jsonoutput/ /path/to/erroroutput/");

            // Run the program to listen for commands
            RunProgram();
        }

        static void RunProgram() {
            var consoleInput = "";

            // Listen for commands
            while(true) {
                // Make sure console input is not just empty or null
                consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                try {
                    // Try executing the command
                    ExecuteCommand(consoleInput);

                } catch(Exception ex) {
                    // If an error is caught, show its type and its message
                    WriteToConsole(ex.GetType().ToString() + " Error occured: " + ex.Message);
                }
            }
        }

        
        public static string ReadFromConsole(string promptMessage = "") {
            // Show prompt, and get input
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }

        // Simple function to display messages to console
        public static void WriteToConsole(string message = "") {
            if(message.Length > 0) {
                Console.WriteLine(message);
            }
        }

        static void ExecuteCommand(string command) {

            // If our command has white space
            if(command.Contains(" ")) {
                // Break it up and set command to first item which is our root command
                commandParts = command.Split(" ").ToList();
                command = commandParts[0];
            }

            switch(command) {
                case "csvtojson":
                    // Make sure we at least 4 command parts: root command, csv file path, output file path, error file path
                    if(commandParts.Count > 3) {

                        // Set path variables from the command line
                        string csvDirPath = Path.GetFullPath(commandParts[1]);
                        jsonOutputPath = Path.GetFullPath(commandParts[2]);
                        errorOutputPath = Path.GetFullPath(commandParts[3]);

                        // See if any of the directories are not valid
                        if ( !Directory.Exists(csvDirPath) || !Directory.Exists(jsonOutputPath) || !Directory.Exists(errorOutputPath) ) {
                            // Throw error if directory doesnt exist
                            throw new DirectoryNotFoundException();
                        }

                        // Set watched path, filters for what to look out for, type of file to look out for, and event handlers
                        watcher.Path = csvDirPath;
                        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime | NotifyFilters.Attributes | NotifyFilters.Security | NotifyFilters.Size;
                        watcher.Filter = "*.csv";
                        watcher.Changed += new FileSystemEventHandler(FileDetected);
                        watcher.Created += new FileSystemEventHandler(FileDetected);
                        watcher.Renamed += new RenamedEventHandler(FileDetected);
                        // Turn it on and check subdirectories
                        watcher.EnableRaisingEvents = true;
                        watcher.IncludeSubdirectories = true;

                        // See if there are any files that already exist
                        var currentFiles = Directory.GetFiles(csvDirPath, "*.csv", SearchOption.AllDirectories).ToList();
                        // If so process them
                        foreach(var file in currentFiles) {
                            ProcessFile(Path.GetFullPath(file));
                        }

                    } else {
                        // We don't have right amount of arguments, throw exception
                        throw new ArgumentException();
                    }

                    
                    break;
                case "exit":
                    // Exit the application
                    WriteToConsole("Exiting application...");
                    Environment.Exit(0);
                    break;
                default:
                    WriteToConsole("Unknown command");
                    break;
            }
        }

        public static void FileDetected(object source, FileSystemEventArgs e) {
            WriteToConsole("File detected.");

            try {
                // Be safe, see if the csv file exists
                if( !File.Exists(e.FullPath) ) {
                    throw new FileNotFoundException();
                } else {
                    // Process the file
                    ProcessFile(e.FullPath);
                }
            } catch (FileNotFoundException ex) {
                // If an error is caught, show its type and its message
                WriteToConsole(ex.GetType().ToString() + " Error occured: " + ex.Message);
            }
        }

        public static void ProcessFile(string filepath) {
            // Get filename
            string filename = Path.GetFileNameWithoutExtension(filepath);

            // See if we have processed this file
            if(proccesedFiles.Contains(filename)) {
                // If so break out, we have proccesed this file before
                WriteToConsole("File already processed.");
                // Delete it too, why not
                if(File.Exists(filepath)) {
                    File.Delete(filepath);
                }
                return;
            } else {
                // Add to proccessed files list
                proccesedFiles.Add(filename);
                WriteToConsole("Begin processing...");
            }

            // Read all lines from csv
            var lines = System.IO.File.ReadAllLines(filepath);
            // Keep track of each line part (id,first,middle,etc.)
            List<string> lineParts = new List<string>{};

            // Kepp a list of allusers to then serialize to json
            List<User> allUsers = new List<User>();

            // Should we show the middle name field (no if empty)
            bool showMiddleName;

            // Keep track of int value of id, and iteration count
            int id;
            int iter = 0;

            // Keep track of any errors found thru processing
            List<KeyValuePair<int, string>> errorsFound = new List<KeyValuePair<int, string>>();

            foreach(string line in lines) {
                // Increment iter, but skip first line since it is the header
                if (iter++ == 0) {
                    continue;
                }

                // Create this loop user to store data
                User loopUser = new User();
                
                // Break up csv line, and default to show middle name
                lineParts = line.Split(",").ToList();
                showMiddleName = true;

                // ID validation
                if(lineParts[0].Length <= 0) {
                    // ERROR - id is empty
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "INTERNAL_ID-cannot-be-empty"));
                }
                if(lineParts[0].Replace("-", "").Length != 8) {
                    // ERROR - id number is not 8 digits
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "INTERNAL_ID-is-not-8-digits"));
                }
                if(!Int32.TryParse(lineParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out id)) {
                    // ERROR - not an integer
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "INTERNAL_ID-is-not-an-integer"));
                }
                if(id < 0) {
                    // ERROR - integer is not positive
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "INTERNAL_ID-is-not-a-positive-integer"));
                }

                // First name validation
                if(lineParts[1].Length <= 0) {
                    // ERROR - first name is empty
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "FIRST_NAME-cannot-be-empty"));
                }
                if(lineParts[1].Length > 15) {
                    // ERROR - first name is > 15 chars
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "FIRST_NAME-is-longer-than-15-characters"));

                    // Make sure we only do the first 15 characters
                    lineParts[1] = lineParts[1].Substring(0, 15);
                }

                // Middle name validation
                if(lineParts[2].Length <= 0) {
                    // Not an error, but dont show middle name in output
                    showMiddleName = false;
                }
                if(lineParts[2].Length > 15) {
                    // ERROR - middle name is > 15 chars
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "MIDDLE_NAME-is-longer-than-15-characters"));
                    
                    // Make sure we only do the first 15 characters
                    lineParts[2] = lineParts[2].Substring(0, 15);
                }

                // Last name validation
                if(lineParts[3].Length <= 0) {
                    // ERROR - last name is empty
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "LAST_NAME-cannot-be-empty"));
                }
                if(lineParts[3].Length > 15) {
                    // ERROR - last name is > 15 chars
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "LAST_NAME-is-longer-than-15-characters"));

                    // Make sure we only do the first 15 characters
                    lineParts[3] = lineParts[3].Substring(0, 15);
                }

                // Phone number validation
                if(lineParts[4].Length <= 0) {
                    // ERROR - phone number is empty
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "PHONE_NUM-cannot-be-empty"));
                }
                Match m = Regex.Match(lineParts[4], @"^[0-9]{3}-[0-9]{3}-[0-9]{4}$", RegexOptions.IgnoreCase);
                if(!m.Success) {
                    // ERROR - phone number not formatted correctly
                    errorsFound.Add(new KeyValuePair<int, string>(iter, "PHONE_NUM-not-formatted-correctly"));
                }

                // Build loopuser data
                loopUser.id = id;
                // Dont show middle name if there is none
                if(showMiddleName) {
                    loopUser.name = new Dictionary<string, string>{
                        { "first", lineParts[1] },
                        { "middle", lineParts[2] },
                        { "last", lineParts[3] }
                    };
                } else {
                    loopUser.name = new Dictionary<string, string>{
                        { "first", lineParts[1] },
                        { "last", lineParts[3] }
                    };
                }                            
                loopUser.phone = lineParts[4];

                // Add to list of all users
                allUsers.Add(loopUser);
            }

            // Create json file in directory with csv filename
            using (StreamWriter outputFile = File.CreateText(jsonOutputPath + filename + ".json")) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(outputFile, allUsers);
            }

            // If we have any errors
            if(errorsFound.Count > 0) {
                // Let user know errors were found
                WriteToConsole("Errors found! Check you error output directory for details.");

                // Vars for breaking up errors
                int rowKey = 0;
                string test = "";
                int iteration = 0;

                // Keep list of csv errors
                var csvRecords = new List<dynamic>();

                foreach(KeyValuePair<int, string> error in errorsFound) {
                    // If we are now on a different row then before
                    if(error.Key != rowKey) {
                        // Only add to csv record if this is not 0 (first time)
                        if(iteration != 0) {
                            dynamic csvRecord = new ExpandoObject();
                            csvRecord.LINE_NUM = rowKey;
                            csvRecord.ERROR_MSG = test;
                            csvRecords.Add(csvRecord);
                        }
                        // Set new rowkey and error message
                        test = error.Value;
                        rowKey = error.Key;
                    } else {
                        // Compund if there were more than 1 error on this row
                        test += ("-&-" + error.Value);
                    }
                    // If this is the last row
                    if(++iteration >= errorsFound.Count) {
                        // Write out final csvrecord
                        dynamic csvRecord = new ExpandoObject();
                        csvRecord.LINE_NUM = rowKey;
                        csvRecord.ERROR_MSG = test;
                        csvRecords.Add(csvRecord);
                    }
                }

                // Write to csv file with error records using filename of processed csv
                using(StreamWriter errorFile = File.CreateText(errorOutputPath + filename + ".csv")){
                    var csvWriter = new CsvWriter(errorFile);
                    csvWriter.WriteRecords(csvRecords);
                }
            }

            // Delete the file
            File.Delete(filepath);

            // Show proccessed message to user
            WriteToConsole("File processed.");
            WriteToConsole("--");
        }

        // Class to store info on user breakdowns from csv to convert to json
        public class User {
            public int id;
            public Dictionary<string,string> name;
            public string phone;
        }
    }
}
