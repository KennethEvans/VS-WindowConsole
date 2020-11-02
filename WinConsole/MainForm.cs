//#define debugTimer

using System;
using System.IO;
using System.Windows.Forms;
using WinConsole.Misc;
using System.Timers;

namespace WinConsole {
    public partial class MainForm : Form {
        public static readonly string NL = Environment.NewLine;
        StreamWriter ConsoleWriter { get; set; }
        MemoryStream MemoryStream { get; set; }
        System.Timers.Timer Timer { get; set; }

        public MainForm() {
            InitializeComponent();

            MemoryStream = new MemoryStream();
            ConsoleWriter = new StreamWriter(MemoryStream);
            Console.SetOut(ConsoleWriter);
            // Set reading the stream in a Timer
            startTimer();
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
            Test app = new Test();
            app.run();
        }
        private void OnStartClick(object sender, EventArgs e) {
            startTimer();
        }

        private void OnStopClick(object sender, EventArgs e) {
            stopTimer();
        }
    }
}
