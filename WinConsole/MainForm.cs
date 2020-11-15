//#define debugTimer

using System;
using System.IO;
using System.Windows.Forms;
using WinConsole.Misc;
using System.Timers;
using System.Reflection;
using KEUtils.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using KEUtils.MultichoiceListDialog;
using System.Linq;
using KEUtils.About;
using System.Drawing;

namespace WinConsole {
    public partial class MainForm : Form {
        public static readonly string NL = Environment.NewLine;
        public static readonly string MISC_NAMESPACE = "WinConsole.Misc";
        public static readonly int N_MRU = 5;

        public string[] MruList { get; set; } = new string[N_MRU];
        StreamWriter ConsoleWriter { get; set; }
        MemoryStream MemoryStream { get; set; }
        System.Timers.Timer Timer { get; set; }

        public MainForm() {
            InitializeComponent();

            // Get MRU list from settings
            try {
                string json = Properties.Settings.Default.MruList;
                if (!String.IsNullOrEmpty(json)) {
                    List<string> mruNames = JsonConvert.DeserializeObject<List<string>>(json);
                    for (int i = 0; i < mruNames.Count; i++) {
                        MruList[i] = mruNames[i];
                    }
                }
            } catch (Exception ex) {
                Utils.excMsg("Error setting MRU list for Settings", ex);
            }

            MemoryStream = new MemoryStream();
            ConsoleWriter = new StreamWriter(MemoryStream);
            Console.SetOut(ConsoleWriter);
            // Set reading the stream in a Timer
            startTimer();
        }

        private void addToMruList(string name) {
            // Check if it is already there
            int existsIndex = -1;
            for (int i = 0; i < N_MRU; i++) {
                if (String.IsNullOrEmpty(MruList[i])) continue;
                if (MruList[i].Equals(name)) {
                    existsIndex = i;
                    break;
                }
            }
            if (existsIndex == 0) return;
            for (int i = 1; i < N_MRU - 1; i++) {
                MruList[i - 1] = MruList[i];
                if (i == existsIndex) break;
            }
            MruList[0] = name;
            saveMruList();
        }

        private void saveMruList() {
            List<string> mruNames = new List<string>();
            for (int i = 0; i < N_MRU; i++) {
                mruNames.Add(MruList[i]);
            }
            string json = JsonConvert.SerializeObject(mruNames);
            Properties.Settings.Default.MruList = json;
            Properties.Settings.Default.Save();
        }

        private void startTimer() {
            if (Timer == null) {
                Timer = new System.Timers.Timer();
                Timer.Interval = 500; // In milliseconds
                Timer.AutoReset = true; // Stops it from repeating
                Timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
#if debugTimer
                textBox.AppendText("start: Timer was null" + NL);
#endif
            }
#if debugTimer
            textBox.AppendText("start(1): Enabled=" + Timer.Enabled + NL);
#endif
            if (!Timer.Enabled) {
                if (ConsoleWriter != null) ConsoleWriter.Flush();
                if (MemoryStream != null) MemoryStream.SetLength(0);
                if (Timer != null) Timer.Start();
#if debugTimer
                textBox.AppendText("start(2): Enabled=" + Timer.Enabled + NL);
#endif
            }
        }

        private void stopTimer() {
            if (Timer == null || !Timer.Enabled) {
#if debugTimer
                else textBox.AppendText("stop: Enabled=" + Timer.Enabled + NL);
#endif
                return;
            }
#if debugTimer
            textBox.AppendText("stop(1): Enabled=" + Timer.Enabled + NL);
#endif  
            Timer.Stop();
            if (ConsoleWriter != null) ConsoleWriter.Flush();
            if (MemoryStream != null) MemoryStream.SetLength(0);
#if debugTimer
            textBox.AppendText("stop(2): Enabled=" + Timer.Enabled + NL);
#endif
        }

        private void run(string className) {
            try {
                Runnable runnable = (Runnable)Assembly.GetExecutingAssembly().
                    CreateInstance(MISC_NAMESPACE + "." + className);
                if (runnable == null) {
                    Utils.errMsg("No Runnable found for " + className);
                    return;
                }
                runnable.Main(null);
                addToMruList(className);
            } catch (Exception ex) {
                Utils.excMsg("Error running " + className, ex);
                return;
            }
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e) {
            string msg = null; ;
            ConsoleWriter.Flush();
            lock (MemoryStream) {
#if false
                textBox.AppendText(DateTime.Now
                    + " Length=" + MemoryStream.Length
                    + " Position=" + MemoryStream.Position + NL);
#endif
                if (MemoryStream.Position > 0) {
                    MemoryStream.Seek(0, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(MemoryStream);
                    msg = reader.ReadToEnd();
                    MemoryStream.SetLength(0);
                }
            }
            if (!String.IsNullOrEmpty(msg)) textBox.AppendText(msg);
        }

        private void OnExitClick(object sender, EventArgs e) {
            Close();
        }

        private void OnClearClick(object sender, EventArgs e) {
            textBox.Clear();
        }

        private void OnRunClick(object sender, EventArgs e) {
            string className = "Test";
            run(className);
        }

        private void OnStartClick(object sender, EventArgs e) {
            startTimer();
        }

        private void OnStopClick(object sender, EventArgs e) {
            stopTimer();
        }

        private void OnRunPreviousClick(object sender, EventArgs e) {
            if (!String.IsNullOrEmpty(MruList[0])) {
                run(MruList[0]);
            } else {
                Utils.errMsg("There is no previous run defined");
            }
        }

        private void OnRunMruClick(object sender, EventArgs e) {
            if (MruList == null || MruList.Length == 0) {
                Utils.errMsg("There are no MRU items");
                return;
            }
            // Check for all null
            bool empty = true;
            foreach (string item in MruList) {
                if (item != null) {
                    empty = false;
                    break;
                }
            }
            if (empty) {
                Utils.errMsg("There are no MRU items");
                return;
            }
            List<string> mruNames = MruList.ToList<string>();
            MultiChoiceListDialog dlg = new MultiChoiceListDialog(mruNames);
            dlg.Text = "Runnables";
            dlg.Prompt = "Select one of the MRU items";
            dlg.SelectionMode = SelectionMode.One;
            dlg.SelectedIndex = 0;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                List<string> selectedList = dlg.SelectedList;
                if (selectedList == null || selectedList.Count == 0) {
                    Utils.errMsg("No items selected");
                    return;
                }
                string className = selectedList[0];
                run(className);
            }
        }

        private void OnRunConfigurationsClick(object sender, EventArgs e) {
            List<string> mruNames = Assembly.GetExecutingAssembly().GetTypes().
                 Where(type => type.Namespace == MISC_NAMESPACE)
                .Select(type => type.Name).ToList();
            if (mruNames == null || mruNames.Count() == 0) {
                Utils.errMsg("Error finding runnable classes");
                return;
            }
            MultiChoiceListDialog dlg = new MultiChoiceListDialog(mruNames);
            dlg.Text = "Runnables";
            dlg.Prompt = "Select one of the available classes";
            dlg.SelectionMode = SelectionMode.One;
            dlg.SelectedIndex = 0;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                List<string> selectedList = dlg.SelectedList;
                if (selectedList == null || selectedList.Count == 0) {
                    Utils.errMsg("No items selected");
                    return;
                }
                string className = selectedList[0];
                run(className);
            }
        }

        private void OnHelpAboutClick(object sender, EventArgs e) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Image image = null;
            try {
                image = Image.FromFile(@".\Help\BlueMouse.256x256.png");
            } catch (Exception ex) {
                Utils.excMsg("Failed to get AboutBox image", ex);
            }
            AboutBox dlg = new AboutBox("About Windows Console", image, assembly);
            dlg.ShowDialog();
        }
    }
}
