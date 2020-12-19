//#define showProgress

using KEUtils.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace WinConsole.Misc {
    /// <summary>
    /// Class to find CSP Brushes for a given Tool and Group. Optionally change
    /// the color of these brushes.
    /// Set DRY_RUN = false to set the color.
    /// 
    /// It would be safer to 
    /// (1) backup the real database,
    /// (2) run this program on a copy of the real database, and 
    /// (3) replace the real data base with the copy when satisfied.
    /// </summary>
    class CSPFindBrushesForGroup : Runnable {
        private static bool DRY_RUN = true;
        public static string TAB = "   ";
        public static string networkString = "\\\\";
        //private static readonly string DATABASE_NAME = @"C:\Users\evans\Documents\CELSYS\CLIPStudioPaintVer1_5_0\Tool\EditImageTool.todb";
        private static readonly string DATABASE_NAME = @"C:\Scratch\AAA\CSP\EditImageTool.todb";
        private static readonly string TOOL_NAME = "PS";
        private static readonly string GROUP_NAME = "PS";

        /// <summary>
        /// Gets a dictionary of all the tools, gropups, and brushes in the
        /// database. Borrowed from CSP Brush Info.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        private Dictionary<String, Tool> getToolList(string database) {
            Dictionary<String, Tool> map = new Dictionary<String, Tool>();
            SQLiteConnection conn = null;
            SQLiteDataReader dataReader;
            DateTime modTime = File.GetLastWriteTime(database);
            Tool tool;
            try {
                string openName = getSqliteOpenName(database);
                using (conn = new SQLiteConnection("Data Source=" + openName
                    + ";Version=3;Read Only=True;")) {
                    conn.Open();
                    SQLiteCommand command;
                    command = conn.CreateCommand();
                    command.CommandText = "SELECT _PW_ID, NodeVariantID, NodeName," +
                        " hex(NodeUUid), hex(NodeFirstChildUuid)," +
                        " hex(NodeNextUuid), hex(NodeSelectedUuid)" +
                        " FROM Node";
                    long id, nodeVariantID;
                    string nodeName;
                    string nodeUuid, nodeFirstChildUuid, nodeNextUuid, nodeSelectedUuid;
                    using (dataReader = command.ExecuteReader()) {
                        while (dataReader.Read()) {
                            id = dataReader.GetInt64(0);
                            nodeVariantID = dataReader.GetInt64(1);
                            nodeName = dataReader.GetString(2);
                            nodeUuid = dataReader.GetString(3);
                            nodeFirstChildUuid = dataReader.GetString(4);
                            nodeNextUuid = dataReader.GetString(5);
                            nodeSelectedUuid = dataReader.GetString(6);
                            tool = new Tool(id, nodeVariantID, nodeName,
                                nodeUuid, nodeFirstChildUuid,
                                nodeNextUuid, nodeSelectedUuid);
                            map.Add(nodeUuid, tool);
                        }
                    }
                    if (map.Count == 0) {
                        Console.WriteLine("Did not find any tools");
                        return null;
                    }
                }
            } catch (Exception ex) {
                Utils.excMsg("Error reading " + database, ex);
                return null;
            }
            return map;
        }

        /// <summary>
        /// Get a list of all the brushes for the given tool and group.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="toolName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private List<Brush> getBrushes(string database, string toolName,
            string groupName) {
            List<Brush> brushes = new List<Brush>();
            // First get the dictionary
            Dictionary<String, Tool> map = getToolList(database);
            if (map == null) return null;

            // Loop over the elements finding the top one (name is blank)
            // Then trace the first and next values for the groups and subtools
            // For each one found, set the nodeParentUUid.
            Tool firstChild, firstChild1, firstChild2;
#if showProgress
            int nTools = 0, nGroups = 0, nSubTools = 0;
#endif
            Brush brush;
            Tool tool;
            // Use this to sort the Dictionary (doesn't matter here)
            foreach (KeyValuePair<string, Tool> entry in map) {
                tool = entry.Value;
                // Only process the top level which has a blank name and _PW_ID=1
                if (tool.nodeName.Length != 0) {
                    continue;
                }
                String nodeFirstChildUuid = tool.nodeFirstChildUuid;
                if (nodeFirstChildUuid == null
                    || nodeFirstChildUuid.Length != 32) {
                    continue;
                }
                map.TryGetValue(nodeFirstChildUuid, out firstChild);
                // Get the tools
                while (firstChild != null
                    && firstChild.nodeUuid.Length == 32
                    //&& firstChild.nodeName.Equals(TOOL_NAME)
                    ) {
                    if (!firstChild.nodeName.Equals(TOOL_NAME)) {
                        // Get the next tool item
                        map.TryGetValue(firstChild.nodeNextUuid, out firstChild);
                        continue;
                    }
                    firstChild.nodeParentUuid = tool.nodeUuid;
#if showProgress
                    // sb.AppendLine("Tool: " + firstChild.nodeName
                    // + " nodeUuid=" + firstChild.nodeUuid
                    // + " nodeFirstChildUuid=" + firstChild.nodeFirstChildUuid);
                    nTools++;
                    Console.WriteLine("Tool: " + firstChild.nodeName);
                    //Console.WriteLine(firstChild.nodeName.Equals(toolName));
#endif
                    map.TryGetValue(firstChild.nodeFirstChildUuid, out firstChild1);
                    // Get the group with the given name
                    while (firstChild1 != null
                        && firstChild1.nodeUuid.Length == 32
                        //&& firstChild1.nodeName.Equals(groupName)
                        ) {
                        if (!firstChild1.nodeName.Equals(GROUP_NAME)) {
                            // Get the next tool item
                            map.TryGetValue(firstChild1.nodeNextUuid, out firstChild1);
                            continue;
                        }
#if showProgress
                        //Console.WriteLine(
                        // TAB + "Group: " + firstChild1.nodeName + " nodeUuid="
                        // + firstChild1.nodeUuid + " nodeFirstChildUuid="
                        // + firstChild1.nodeFirstChildUuid);
                        Console.WriteLine("  Group: " + firstChild1.nodeName);
#endif
                        firstChild1.nodeParentUuid = firstChild.nodeUuid;

                        // Get the sub tools
                        map.TryGetValue(firstChild1.nodeFirstChildUuid, out firstChild2);
                        while (firstChild2 != null
                            && firstChild2.nodeUuid.Length == 32) {
#if showProgress
                           nGroups++;
                           Console.WriteLine("  Group: " + firstChild1.nodeName);
                            //Console.WriteLine(TAB + TAB + "SubTool: "
                            // + firstChild2.nodeName + " nodeUuid="
                            // + firstChild2.nodeUuid + " nodeFirstChildUuid="
                            // + firstChild2.nodeFirstChildUuid);
                            Console.WriteLine(TAB + TAB + "SubTool: " + firstChild2.nodeName);
                            //Console.WriteLine(firstChild.nodeName.Equals(TOOL_NAME)
                            //    && firstChild1.nodeName.Equals(GROUP_NAME));
#endif                            //nSubTools++;
                            brush = new Brush(firstChild2.nodeName, firstChild2.nodeVariantID);
                            brushes.Add(brush);
                            firstChild2.nodeParentUuid = firstChild1.nodeUuid;
                            // Get the next subtool item
                            map.TryGetValue(firstChild2.nodeNextUuid, out firstChild2);
                        }
                        // Get the next group item
                        map.TryGetValue(firstChild1.nodeNextUuid, out firstChild1);
                    }
                    // Get the next tool item
                    map.TryGetValue(firstChild.nodeNextUuid, out firstChild);
                }
            }
            return brushes;
        }

        /// <summary>
        /// List the brushes. Optionall update the NodeIconColor to Photoshop 
        /// Blue (17193389).
        /// </summary>
        private void listBrushes(string database, List<Brush> brushes, bool doUpdate) {
            long psIconColor = 17193389L;
            foreach (Brush brush in brushes) {
                SQLiteConnection conn = null;
                SQLiteDataReader dataReader;
                SQLiteCommand command, command1;
                try {
                    string openName = getSqliteOpenName(database);
                    using (conn = new SQLiteConnection("Data Source=" + openName
                        + ";Version=3;Read Only=False;")) {
                        conn.Open();
                        command = conn.CreateCommand();
                        command.CommandText = "SELECT _PW_ID, NodeVariantID,"
                            + " NodeName, NodeIconColor"
                            + " FROM Node"
                            + " WHERE NodeVariantID=" + brush.nodeVariantID;
                        string nodeName;
                        long nodeVariantID, nodeIconColor;
                        using (dataReader = command.ExecuteReader()) {
                            while (dataReader.Read()) {
                                nodeVariantID = dataReader.GetInt64(1);
                                nodeName = dataReader.GetString(2);
                                nodeIconColor = dataReader.GetInt64(3);
                                Console.WriteLine($"[{nodeVariantID}] \tnodeIconColor={nodeIconColor,-9} \t{nodeName,-40}");
                                if (doUpdate) {
                                    command1 = new SQLiteCommand(conn);
                                    command1.CommandText = "Update Node SET NodeIconColor="
                                        + psIconColor
                                        + " WHERE nodeVariantID = " + nodeVariantID;
                                    command1.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    Utils.excMsg("Error listing brushes for " + database, ex);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns a name to use with new SQLiteConnection.
        /// Appends \\ if it is a network name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getSqliteOpenName(string name) {
            if (name.StartsWith(networkString) &&
                !name.StartsWith(networkString + networkString)) {
                return networkString + name;
            } else {
                return name;
            }
        }

        public override void Main(string[] args) {
            Console.WriteLine("database=" + DATABASE_NAME);
            Console.WriteLine("group=" + GROUP_NAME);
            List<Brush> brushes = getBrushes(DATABASE_NAME, TOOL_NAME, GROUP_NAME);
            if (brushes == null) return;
            Console.WriteLine("Number of brushes=" + brushes.Count);

            // The first run changes them if DRY_RUN is false
            if (!DRY_RUN) {
                listBrushes(DATABASE_NAME, brushes, true);
                Console.WriteLine();
            }

            // The second run lists them
            listBrushes(DATABASE_NAME, brushes, false);

            Console.WriteLine();
            Console.WriteLine("All done");
        }
    }

    public class Tool {
        public long id;
        public long nodeVariantID;
        public String nodeName;
        public String nodeUuid;
        public String nodeFirstChildUuid;
        public String nodeNextUuid;
        public String nodeSelectedUuid;
        // This is not a database column but is used for tracking orphans
        public String nodeParentUuid;

        public Tool(long id, long nodeVariantID, String nodeName, String nodeUuid,
            String nodeFirstChildUuid, String nodeNextUuid,
            String nodeSelectedUuid) {
            this.id = id;
            this.nodeVariantID = nodeVariantID;
            this.nodeName = nodeName;
            this.nodeUuid = nodeUuid;
            this.nodeFirstChildUuid = nodeFirstChildUuid;
            this.nodeNextUuid = nodeNextUuid;
            this.nodeSelectedUuid = nodeSelectedUuid;
        }
    }

    public class Brush {
        public string nodeName;
        public long nodeVariantID;
        public int iconColor;

        public Brush(string nodeName, long nodeVariantID) {
            this.nodeName = nodeName;
            this.nodeVariantID = nodeVariantID;
        }
    }
}
