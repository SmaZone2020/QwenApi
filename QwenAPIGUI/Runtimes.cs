using QwenApi.Apis;
using QwenApi.Models.ResponseM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenAPIGUI
{
    internal class Runtimes
    {
        public static DateTime UpdateTime { get; set; }
        public static string Bx_UA { get; set; } = "";
        public static string Bx_Umidtoken { get; set; } = "";
        public static string Cookies { get; set; } = "";

        public static List<SessionItem> SessionItems { get; set; }
        public static GetSessionHistory.SessionData CurrentSession { get; set; } = null;

    }
}
