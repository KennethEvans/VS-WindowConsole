using System;
    using System.Timers;

namespace WinConsole.Misc {
    class Test : Runnable {

        void OnTimerElapsed(object sender, ElapsedEventArgs e) {
            Console.WriteLine("Test: Finished at " + DateTime.Now);
        }

        public override void Main(string[] args) {
            Console.WriteLine("Test: Run at " + DateTime.Now);

            // Want it to pause a while
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 2000; // In milliseconds
            timer.AutoReset = false; // Stops it from repeating
            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            timer.Start();
        }
    }
}
