using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinConsole {
    public abstract class Runnable {
        public static readonly string NL = Environment.NewLine;

        public abstract void Main(String[] args);
    }
}
