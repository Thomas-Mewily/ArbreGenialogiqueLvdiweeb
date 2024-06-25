using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ArbreGenia
{
    public class Graphe
    {
        public List<Command> Commandes = new();
        public Person Root;
        public Dictionary<string, Person> All = new();
        public int Length => All.Count;

        public void Reset() 
        {
            Commandes = new List<Command>();
            All = new Dictionary<string, Person>();
            Root = new Person(".", ".", ".", ".");
            Add(Root);
        }

        public void Save(string path) 
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in Commandes)
            {
                sb.AppendLine(c.Cmd);
            }

            File.Delete(path); 
            File.WriteAllText(path, sb.ToString());
        }

        public bool Load(StringBuilder sb, string path) 
        {
            bool r = true;
            // Crash if the backup file don't exist : `Save/lvdiweeb.txt`
            var s = File.ReadLines(path).ToList();
            foreach(var cmd in s) 
            {
                r &= Execute(sb, cmd, true);
            }
            return r;
        }

        public void Add(Person p)
        {
            All.Add(p._Uid, p);
        }

        public Person? this[string uid]
        {
            get 
            {
                // faire un ternaire
                if(All.TryGetValue(uid, out Person? p)) 
                { 
                    return p;
                }
                return null;
            }
        }

        public Graphe()
        {
            Root = new Person(".", ".", ".", ".");
            Reset();
        }

        public bool Reload(List<Command> cmd) 
        {
            bool succeed = true;
            foreach(var v in cmd) 
            {
                succeed &= Execute(v, true);
            }
            return succeed;
        }

        public bool Execute(Command c, bool admin_mode = false) 
        {
            if (c.Execute(this, admin_mode))
            {
                if (c.HaveSideEffect)
                {
                    Commandes.Add(c);
                }
                return true;
            }
            return false;
        }

        public bool Execute(StringBuilder sb, string command, bool admin_mode = false)
        {
            Command c = new Command(sb, command);
            return Execute(c, admin_mode);
        }
    }
}


