/// <author>Parmanand Kumar</author>
/// <created>15/11/2021</created>
/// <summary>
///     It contains the TelemetryPersistence class
///     It implements the ITelemetryPersistence interface functions.
/// </summary> 
/// 

using Dashboard.Server.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Dashboard.Server.Persistence
{

    public class TelemetryPersistence : ITelemetryPersistence
    {
        
        public TelemetryPersistence()
        {
            serverDataPath = "../../../Persistence/PersistenceDownloads/TelemetryDownloads/ServerData";
            telemetryAnalyticsPath = "../../../Persistence/PersistenceDownloads/TelemetryDownloads/TelemetryAnalytics/";
        }


        /// <summary>
        /// retrives the ServerData after end of all of the sessions.
        /// </summary>
        /// <returns>returns List of SeverData</returns>
        public ServerDataToSave RetrieveAllSeverData()
        {
            // Creating instance of XmLSerializer, inorder to deserialize an XML stream
            XmlSerializer deserialiser = new XmlSerializer(typeof(ServerDataToSave));

            //Saving the Path to save find the XML file.
            string path = serverDataPath;
            object objectsList = null;
            try
            {
                string FullPath = Path.Combine(path, "GlobalServerData.xml");
                using (StreamReader stream = new StreamReader(FullPath))
                {
                    objectsList = deserialiser.Deserialize(stream);
                }
                //TypeCasting and returning the ServerDataToSave
                return (ServerDataToSave)objectsList;
            }
            catch (IOException exp)
            {
                // Handling Exception if someone calls the retriveFunction even before saving the XML file
                Trace.WriteLine(exp.Message);
                Trace.WriteLine("Yeah it is being catched here....");
                ServerDataToSave sdts = new ServerDataToSave();
                sdts.sessionCount = -1;
                return sdts;
            }
             
        }

        /// <summary>
        /// save the UserCountVsTimeStamp, UserIdVsChatCount, InsincereMember data as png after each session.
        /// </summary>
        /// <param name="sessionAnalyticsData"> takes sessionAnalyticsData from Telemetry. </param>
        public ResponseEntity Save(SessionAnalytics sessionAnalyticsData)
        {
            // create folder of name sessionId to store all analytics data

            string sessionId = string.Format("Analytics_{0:yyyy - MM - dd_hh - mm - ss - tt}", DateTime.Now);

            // Logic to plot and save UserCount Vs TimeStamp


            ResponseEntity t1 = UserCountVsTimeStamp_PlotUtil(sessionAnalyticsData.userCountAtAnyTime, sessionId);

            // Logic to plot and save ChatCount Vs UserID

            ResponseEntity t2 = ChatCountVsUserID_PlotUtil(sessionAnalyticsData.chatCountForEachUser, sessionId);

            // Logic to save InsincereMembers list

            ResponseEntity t3 = InsincereMembers_SaveUtil(sessionAnalyticsData.insincereMembers, sessionId);

            ResponseEntity response = new ResponseEntity();
            response.IsSaved = t1.IsSaved & t2.IsSaved & t3.IsSaved;
            response.FileName = sessionId;
            return response;
        }


        /// <summary>
        /// save the InsincereMember data as png after each session.
        /// </summary>
        /// <param name="InsincereMembers"> takes InsincereMembers from Telemetry. </param>
        /// /// <param name="sessionId"> takes sessionId from Telemetry. </param>
        private ResponseEntity InsincereMembers_SaveUtil(List<int> InsincereMembers, string sessionId)
        {
            //Saving the Path to save find the XML file.
            string p1 = telemetryAnalyticsPath + sessionId;
            string TextToSave = "Followings are UserIDs of InsincereMembers : " + Environment.NewLine;
            ResponseEntity response = new ResponseEntity();
            response.FileName = "insincereMembersList.txt";
            foreach (int w in InsincereMembers)
            {
                TextToSave = TextToSave + w.ToString() + Environment.NewLine;
            }

            try
            {
                //Check if directory exists if not create a directory.
                if (!Directory.Exists(p1)) Directory.CreateDirectory(p1);

                //Writing the Text to Text file.
                File.WriteAllText(Path.Combine(p1, "insincereMembersList.txt"), TextToSave);
                Trace.WriteLine("insincereMembersList.txt saved Successfully!!");
                response.IsSaved = true;
                return response;
            }
            catch(Exception except)
            {
                Trace.WriteLine(except.Message);
                response.IsSaved = false;
                return response;
            }

        }

        /// <summary>
        /// save the ChatCountForEachUser data as png after each session.
        /// </summary>
        /// <param name="ChatCountForEachUser"> takes ChatCountForEachUser from Telemetry. </param>
        /// /// <param name="sessionId"> takes sessionId from Telemetry. </param>
        private ResponseEntity ChatCountVsUserID_PlotUtil(Dictionary<int, int> ChatCountForEachUser, string sessionId)
        {
            string p1 = telemetryAnalyticsPath + sessionId;
            // Converting the data Value of dictionary to Array, inorder to use ScottPlot library
            int[] val1 = ChatCountForEachUser.Values.ToArray();
            double[] values1 = new double[val1.Length];
            for (int i = 0; i < val1.Length; i++)
            {
                values1[i] = val1[i];
            }
            List<double> pos1 = new List<double>();
            List<string> lb1 = new List<string>();

            int x1 = 0;
            foreach (int k1 in ChatCountForEachUser.Keys)
            {
                pos1.Add(x1);
                lb1.Add(k1.ToString());
                x1++;
            }

            //Creating the Fixed labels
            string[] labels1 = lb1.ToArray();

            //Fixing the positions of X-labels
            double[] positions1 = pos1.ToArray();

            //Creating ScottPlot fig of mentioned dimension
            var plt1 = new ScottPlot.Plot(600, 400);

            // Actually plotting the Bars
            plt1.AddBar(values1, positions1);

            //Adding the Xticks
            plt1.XTicks(positions1, labels1);
            plt1.SetAxisLimits(yMin: 0);

            //Fixing the Y spacing to 1, to enable ease of readability
            plt1.YAxis.ManualTickSpacing(1);

            // Giving names to X and Y axes
            plt1.XLabel("UserID");
            plt1.YLabel("ChatCount for any User");
            ResponseEntity response = new ResponseEntity();
            response.FileName = "ChatCountVsUserID.png";

            try
            {
                //Creating Directory if required and save
                if (!Directory.Exists(p1)) Directory.CreateDirectory(p1);
                plt1.SaveFig(Path.Combine(p1, "ChatCountVsUserID.png"));
                response.IsSaved = true;
                Trace.WriteLine("ChatCountVsUserID.png saved Successfully!!");
                return response;

            }
            catch(Exception except)
            {
                Trace.WriteLine(except.Message);
                response.IsSaved = false;
                return response;
            }

        }

        /// <summary>
        /// save the UserCountAtAnyTime data as png after each session.
        /// </summary>
        /// <param name="UserCountAtAnyTime"> takes UserCountAtAnyTime from Telemetry. </param>
        /// /// <param name="sessionId"> takes sessionId from Telemetry. </param>
        private ResponseEntity UserCountVsTimeStamp_PlotUtil(Dictionary<DateTime, int> UserCountAtAnyTime, string sessionId)
        {
            // Converting the data Value of dictionary to Array, inorder to use ScottPlot library
            int[] val = UserCountAtAnyTime.Values.ToArray();
            double[] values = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                values[i] = val[i];
            }
            List<double> pos = new List<double>();
            List<string> lb = new List<string>();
            int x = 0;
            foreach (DateTime k in UserCountAtAnyTime.Keys)
            {
                pos.Add(x);
                lb.Add(k.ToString());
                x++;
            }

            //Creating the Fixed labels
            string[] labels = lb.ToArray();

            //Fixing the positions of X-labels
            double[] positions = pos.ToArray();

            //Creating ScottPlot fig of mentioned dimension
            var plt = new ScottPlot.Plot(600, 400);

            // Actually plotting the Bars
            var temp = plt.AddBar(values, positions);

            //Adding the Xticks
            plt.XTicks(positions, labels);

            //Fixing the Y spacing to 1, to enable ease of readability
            plt.YAxis.ManualTickSpacing(1);
            plt.SetAxisLimits(yMin: 0);

            // Changing BarColor to Green
            temp.FillColor = Color.Green;

            // Giving names to X and Y axes
            plt.XLabel("TimeStamp");
            plt.YLabel("UserCount At Any Instant");
            ResponseEntity response = new ResponseEntity();
            response.FileName = "UserCountVsTimeStamp.png";
            string p1 = telemetryAnalyticsPath + sessionId;

            try
            {
                //Creating Directory if required and save
                if (!Directory.Exists(p1)) Directory.CreateDirectory(p1);
                plt.SaveFig(Path.Combine(p1, "UserCountVsTimeStamp.png"));
                Trace.WriteLine("UserCountVsTimeStamp.png saved Successfully!!");
                response.IsSaved = true;
                return response;
            }
            catch(Exception except)
            {
                Trace.WriteLine(except.Message);
                response.IsSaved = false;
                return response;
            }
        }

        /// <summary>
        /// append the ServerData into a file after each session end
        /// </summary>
        /// <param name="AllserverData"> takes ServerData from Telemetry to be saved into text file </param> 
        /// <returns>Returns true if saved successfully else returns false</returns>
        public ResponseEntity SaveServerData(ServerDataToSave AllserverData)
        {
            ResponseEntity response = new ResponseEntity();

            //Creating the XmlSerializer Instance
            XmlSerializer xmlser = new XmlSerializer(typeof(ServerDataToSave));
            string path = serverDataPath;
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                // Calling Serialize method
                using (System.IO.StreamWriter stream = new System.IO.StreamWriter(Path.Combine(path, "GlobalServerData.xml")))
                {
                    xmlser.Serialize(stream, AllserverData);
                }
                Trace.WriteLine("ServerData saved Succesfully!!");
                response.IsSaved = true;
                response.FileName = "GlobalServerData.xml";
                return response;
            }
            catch(Exception except)
            {
                Trace.WriteLine(except.Message);
                response.IsSaved = false;
                return response;
            }
        }

        private string serverDataPath;
        private string telemetryAnalyticsPath;

        public string ServerDataPath { get => serverDataPath; set => serverDataPath = value; }
        public string TelemetryAnalyticsPath { get => telemetryAnalyticsPath; set => telemetryAnalyticsPath = value; }
    }
}