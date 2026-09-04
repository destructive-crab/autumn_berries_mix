using Godot;

namespace destructive_code.Debug
{
    public static class LGR
    {
        private static string currentBlock = null;
        
        /// <summary>
        /// Push message to logs
        /// </summary>
        public static void PM(params object[] o)
        {
            PushString(GetFormattedMessage(o));
        }
        
        /// <summary>
        /// Push warning to logs
        /// </summary>
        public static void PW(params object[] o)
        {
            PushString("[!] "+GetFormattedMessage(o));
        }

        /// <summary>
        /// Push errpr to logs
        /// </summary>
        public static void PERR(params object[] o)
        {
            PushString("[ERROR] "+GetFormattedMessage(o));
        }
        
        public static void BegB(string header = "")
        {
            currentBlock = $"[{header}]\n";
        }

        public static void EnB()
        {
            currentBlock += "[END]";
            Print(currentBlock);
            currentBlock = null;
        }

        public static string GetFormattedMessage(object[] o)
        {
            return string.Join(separator: " ", o);
        }

        private static void PushString(string s)
        {
            if (currentBlock != null)
            {
                currentBlock += "- "+s;
                currentBlock += '\n';
                return;
            }
            Print(s);
        }

        private static void Print(string s)
        {
            GD.Print(s);
        }
    }
}