using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ArbreGenia
{
    public struct DisplayMode
    {
        public bool Pseudo;
        public bool Nom;
        public bool Prenom;
        public bool Uid;

        public DisplayMode(bool pseudo = true, bool nom = true, bool prenom = true, bool uid = true)
        {
            Pseudo = pseudo;
            Nom = nom;
            Prenom = prenom;
            Uid = uid;
        }
    }

    public struct Match 
    {
        public Person Person;
        public int Score;

        public Match(Person person, int score)
        {
            Person = person;
            Score = score;
        }
    }

    public class Command
    {
        private static Random Rng = new Random();

        public static string Prefix = "$";
        public static char ArgSeparators = ' ';


        public bool ShouldSendSaveFile = false;
        public bool ShouldStop = false;

        public bool HaveSideEffect { get; private set; } = false;
        public bool IsUndo = false;

        private StringBuilder Sb;
        public string Cmd { get; private set; }
        private Graphe G = new();
        public List<string> PartsNotModified = new();
        public List<string> Parts = new();

        public Command(StringBuilder sb, string cmd)
        {
            Sb = sb;
            Cmd = cmd.Replace("\n", "");
        }

        bool UnknowCommand()
        {
            Sb.AppendLine("Commande inconnue");
            return false;
        }

        bool HelpAdd()
        {
            Help("add prenom nom pseudo", "uid", "Ajoute une nouvelle personne et renvoie l'uid de celle-ci");
            return false;
        }

        bool HelpOrphan()
        {
            Help("orphan", "", "Affiche les personnes sans parrain/marraine");
            return false;
        }


        bool HelpEdit()
        {
            Help("edit uid champs valeur", "", "Modifie un champs de la personne");
            ListField();
            return false;
        }

        bool HelpIsParent()
        {
            Help("set_parent parent_uid child_uid", "", "Ajoute un filleul/filleule à la personne");
            return false;
        }

        bool HelpTree()
        {
            Help("tree uid <'n' : nom>? <'p': prénom>? <'l' ou 'P': pseudo>? <'u' ou 'i': uid>? <int profondeur>?", "", "liste le sous arbre à partir de l'uid");
            return false;
        }

        bool HelpSearch()
        {
            Help("search keyword", "liste personne", "Recherche une personne via son nom/prénom/pseudo");
            return false;
        }

        bool HelpAnagram()
        {
            Help("anagram keyword", "liste personne", "Recherche une personne en cherchant un anagram dans son nom/prénom/pseudo");
            return false;
        }

        bool HelpRandom()
        {
            Help("random", "", "retourne une personne aléatoire");
            return false;
        }

        bool HelpBan()
        {
            Help("ban discord_uid", "", "(discord_uid : pseudo#XXXX ). Ban la personne jusqu'au redémarrage du bot (flemme de sauvegarder)", true);
            return false;
        }

        bool HelpOp()
        {
            Help("op discord_uid", "", "(discord_uid : pseudo#XXXX ). La personne peut acceder aux commandes op", true);
            return false;
        }

        bool HelpDeop()
        {
            Help("deop discord_uid", "", "(discord_uid : pseudo#XXXX ). La personne est promu au rang d'utilisateur", true);
            return false;
        }

        bool HelpSave()
        {
            Help("save", "", "Envoie le fichier de sauvegarde", true);
            return false;
        }

        bool HelpUndo()
        {
            Help("undo nb_command", "", "Supprime les X (nb_command) dernières commandes", true);
            return false;
        }

        bool HelpSTOP()
        {
            Help("STOP", "", "Termine le programme !", true);
            return false;
        }

        bool HelpBanned()
        {
            Help("banned", "", "Liste les bannis");
            return false;
        }

        bool HelpAdmin()
        {
            Help("admin", "", "Liste les admins");
            return false;
        }


        bool HelpIsAdmin()
        {
            Help("admin?", "", "te dis si tu es un/une admin ou pas");
            return false;
        }


        bool HelpInfo()
        {
            Help("info uid", "", "Voir les informations d'une personne");
            ListField();
            return false;
        }

        void AppendLine(string s = "") { Sb.AppendLine(s); }
        void Append(string s) { Sb.Append(s); }

        string Quote(string s) => "`" + s + "`";
        string Bold(string s) => "**" + s + "**";
        string Italic(string s) => "_" + s + "_";

        string DisplayReplaceSpecial(string s) => s.Replace("_", "\\_");

        string Personne(Person p, bool formatted = true) 
            => formatted ? 
                p.prenom + " " + DisplayReplaceSpecial(p.nom) + " " + Italic("aka") + "  " + DisplayReplaceSpecial(p.pseudo) + " (" + Quote(p._Uid) + ")" :
                p.prenom + " " + p.nom + " aka " + p.pseudo + " (" + p._Uid + ")";

        void AppendQuote(string s)
        {
            if (s.Length == 0) { return; }
            Sb.Append(Quote(s));
        }

        void AppendCommand(string command)
        {
            Append("- ");
            AppendQuote(Prefix + command);
        }

        void Help(string command, string result, string description, bool admin_mode = false)
        {
            AppendCommand(command);
            if (result.Length > 0)
            {
                Append(" => ");
                AppendQuote(result);
            }
            Append(" ");
            if (admin_mode) 
            {
                Append(Bold("[admin]"));
            }
            Append("      ");
            AppendLine(description);
        }

        bool Help()
        {
            Help("help", "", "affiche l'aide");
            AppendLine();

            HelpAdd();
            //AppendLine();

            HelpEdit();
            //AppendLine();

            HelpInfo();
            //AppendLine();

            HelpSearch();
            //AppendLine();

            HelpAnagram();
            //AppendLine();

            HelpTree();
            //AppendLine();

            HelpIsParent();
            //AppendLine();

            HelpRandom();
            //AppendLine();

            HelpOrphan();
            //AppendLine();

            HelpBanned();
            //AppendLine();

            HelpAdmin();
            //AppendLine();

            HelpIsAdmin();
            //AppendLine();




            // Admin
            AppendLine();

            HelpBan();
            //AppendLine();

            HelpOp();
            //AppendLine();

            HelpDeop();
            //AppendLine();

            HelpUndo();
            //AppendLine();

            HelpSave();

            HelpSTOP();

            return true;
        }


        private string _GenerateUid(string inspiredBy, int nb = 0)
        {
            string uid = inspiredBy + "#" + nb.ToString();
            if (G.All.ContainsKey(uid) == false) { return uid; }
            return _GenerateUid(inspiredBy, nb + 1);
        }
        /*
         * 
         * .ToLowerInvariant().Where(c => string.IsNullOrEmpty(c.ToString()) == false)
        */

        private string CleanNameUid(string s) 
        {
            s = s.ToLowerInvariant();
            s = string.Join("", s.Where(c => string.IsNullOrEmpty(c.ToString()) == false).ToList()).Replace("(","").Replace(")", "");
            return s;
        }

        public string GenerateUid(string pseudo)
        {
            pseudo = CleanNameUid(pseudo);
            if (pseudo.Length >= 26) { pseudo = pseudo.Substring(0, 26); }
            if (pseudo.Contains('#')) 
            {
                pseudo = pseudo.Substring(0, pseudo.IndexOf('#'));
            }
            return _GenerateUid(pseudo);
        }

        public bool ListField()
        {
            AppendLine("Les champs modifiables sont : ");
            foreach (var v in Person.GetEditableField())
            {
                AppendQuote(v);
                Append(" ");
            }
            AppendLine();
            return false;
        }

        /// <summary>
        /// maybe null
        /// </summary>
        /// <returns></returns>
        Person GetPerson()
        {
            if (Parts.Count == 0) { return null; }
            string uid = CleanNameUid(Pop());

            if (uid.StartsWith("#")) 
            {
                uid = uid.Substring(1) + "#0";
            }
            if(uid.Contains("#") == false && uid != ".") 
            { 
                uid = uid + "#0";
            }

            Person p = G[uid];
            if(p == null) 
            {
                PersonNotFound(uid);
            }
            return p;
        }

        public bool PersonNotFound(string uid)
        {
            Append("La personne ");
            AppendQuote(uid);
            AppendLine(" n'est pas connu");
            return false;
        }

        string Peek()
        {
            if (Parts.Count == 0) { return ""; }
            string sw = Parts[0];
            return sw;
        }

        string Pop()
        {
            if (Parts.Count == 0) { return null; }
            string sw = Parts[0];
            Parts.RemoveAt(0);
            PartsNotModified.RemoveAt(0);
            return sw;
        }

        private bool _WriteTree(Person p, HashSet<Person> already_display, DisplayMode mode, int level, string indent = "", bool last = true, bool writeIndent = true)
        {
            if(level <= 0) { last = true; }
            if (writeIndent)
            {
                Append(indent);
                if (last)
                {
                    Append("└─");
                    indent += "  ";
                }
                else
                {
                    Append("├─");
                    indent += "│ ";
                }
            }
            if (level <= 0) { Append("..."); return false; }

            if(mode.Uid && mode.Pseudo && mode.Nom && mode.Prenom) 
            {
                Append(Personne(p, false));
            }
            else 
            {
                if (mode.Nom) { Append(p.nom); Append(" "); }
                if (mode.Prenom) { Append(p.prenom); Append(" "); }
                if (mode.Pseudo) { Append(p.pseudo); Append(" "); }
                if (mode.Uid) { Append(p.pseudo); Append(" "); }
            }
            if (already_display.Contains(p) == false) 
            {
                already_display.Add(p);
                for (int i = 0; i < p._Childs.Count; i++)
                {
                    AppendLine();
                    if(_WriteTree(p._Childs[i], already_display, mode, level - 1, indent, (i == p._Childs.Count - 1), writeIndent) == false) 
                    {
                        break;
                    }
                }
            }
            else 
            {
                Append(" ...");
            }
            return true;
        }

        private void DisplayInfo(Person p) 
        {
            Append("Informations sur ");
            AppendQuote(p._Uid);
            AppendLine(":");
            AppendLine();

            AppendLine("- parrain/marraine : ");
            foreach (var parent in p._Parents)
            {
                Append(Personne(parent) + " ,");
            }
            if (p._Parents.Count != 0) { AppendLine(); }
            AppendLine();

            AppendLine("- filleul/filleule : ");
            foreach (var f in p._Childs)
            {
                Append(Personne(f) + " ,");
            }
            if (p._Childs.Count != 0) { AppendLine(); }
            AppendLine();


            foreach (var v in Person.GetEditableField())
            {
                AppendQuote(v);
                Append(" : ");
                AppendLine(p.Get(v));
            }
        }

        public List<Match> Find(Func<string, int> ScoreNomPrenomPseudo, int maxLength = -1) 
        {
            if(maxLength == 0) { return new List<Match>(); }

            var match = G.All.Select(c => c.Value).ToList();
            match.Remove(G.Root);

            var match_tuple = match.Select(p => new Match(p, 
                Math.Min(ScoreNomPrenomPseudo(p.nom.ToLowerInvariant()), 
                Math.Min(ScoreNomPrenomPseudo(p.prenom.ToLowerInvariant()), 
                         ScoreNomPrenomPseudo(p.pseudo.ToLowerInvariant())
                        )
                ))).ToList();

            match_tuple.Sort((a, b) => a.Score.CompareTo(b.Score));

            if(maxLength >= 0 && match_tuple.Count > maxLength) 
            {
                match_tuple.RemoveRange(maxLength, match_tuple.Count - maxLength);
            }
            return match_tuple;
        }

        public bool Execute(Graphe g, bool admin_mode = false)
        {
            G = g;
            PartsNotModified = Cmd.Split(ArgSeparators).ToList();
            Parts = PartsNotModified.Select(s => s.ToLowerInvariant()).ToList();

            if (Parts.Count() == 0) { return UnknowCommand(); }

            switch (Pop())
            {
                case "help": Help(); return true;
                case "add":
                    {
                        if (PartsNotModified.Count() != 3) { return HelpAdd(); }

                        Person p = new Person(PartsNotModified[1], PartsNotModified[0], PartsNotModified[2], GenerateUid(PartsNotModified[2]));
                        G.Add(p);
                        Append("Tu as bien été ajouté, " + Bold(Person.GenerateDumbPseudo(p.prenom)) + " ! ("+ p.prenom + " "+ p.nom + ") avec l'uid : ");
                        AppendQuote(p._Uid);
                        AppendLine();
                        AppendLine("Tu peux utiliser cette commande pour modifier tes informations : " + Prefix + "edit");
                        //HelpEdit();
                        HaveSideEffect = true;
                        return true;
                    }
                case "edit":
                    {
                        var p = GetPerson();
                        if (p == null)
                        {
                            return HelpEdit();
                        }
                        if (Parts.Count() <= 1)
                        {
                            return HelpEdit();
                        }

                        string field = Pop();
                        string value = string.Join(" ", PartsNotModified);
                        string old_value = p.Get(field);
                        if (p.Edit(field, value) == false)
                        {
                            return ListField();
                        }
                        old_value = old_value == null ? "" : old_value;
                        Append(p.prenom + "." + field + " : ");
                        Append(old_value);
                        Append(" => ");
                        Append(value);
                        HaveSideEffect = true;
                        return true;
                    }
                case "info":
                    {
                        Person p = GetPerson();
                        if (p == null) { return false; }

                        DisplayInfo(p);
                        return true;
                    }
                case "find":
                case "search":
                case "grep":
                    {
                        string keyword = Pop();
                        if(keyword == null) { return HelpSearch(); }
                        while (true) 
                        {
                            string tmpKeyword = Pop();
                            if(tmpKeyword == null) { break; }
                            keyword += tmpKeyword;
                        }
                        keyword = keyword.ToLowerInvariant();

                        int maxDisplay = 17;
                        var match = Find(s => s.LevenshteinDistance(keyword), maxDisplay);

                        int nb_display = 0;

                        foreach(var t in match)
                        {
                            nb_display++;
                            Person p = t.Person;
                            AppendLine(" - #" + nb_display.ToString() + " : " + Personne(p) + " (search distance: " + t.Score + ")");
                            if(nb_display >= 3 && t.Score >= 4) /* to much difference */ { break; }
                        }

                        if(nb_display == 0) 
                        {
                            AppendLine("Désolé il y a personne");
                        }
                        return true;
                    }
                case "is_parent_of":
                case "set_parent":
                    {
                        var parent  = GetPerson();
                        var child  = GetPerson();
                        if(parent == null || child == null) { return HelpIsParent(); }
                        
                        if (parent._Childs.Contains(child) == false) 
                        {
                            parent._Childs.Add(child);
                            child._Parents.Add(parent);
                            Append(Personne(parent) + " est maitenant le parrain/marraine de " + Personne(child) + " !");
                            HaveSideEffect = true;
                            return true;
                        }
                        Append(Personne(parent) + " était déjà le parrain/marraine de " + Personne(child));
                        return true;
                    }
                case "tree":
                case "list":
                    {
                        var racine = GetPerson();
                        if (racine == null) { return HelpTree(); }

                        DisplayMode mode = new DisplayMode(true);
                        bool custom_mode = false;
                        int profondeur = 5;

                        while (true)
                        {
                            string args = Pop();
                            if (args == null) { break; }

                            string argsLowercase = args.ToLower();


                            if (int.TryParse(args, out int p)) { profondeur = p; continue; }
                            if(custom_mode == false) 
                            {
                                custom_mode = true;
                                mode.Uid = false;
                                mode.Pseudo = false;
                                mode.Nom = false;
                                mode.Prenom = false;
                            }

                            if (argsLowercase.Contains("e")) { mode = new DisplayMode(false, false, false, false); }
                            if (argsLowercase.Contains("l") || args.Contains("P")) { mode.Pseudo = true; }
                            if (args.Contains("p")) { mode.Prenom = true; }
                            if (argsLowercase.Contains("n")) { mode.Nom = true; }
                            if (argsLowercase.Contains("u") || argsLowercase.Contains("i")) { mode.Uid = true; }
                        }


                        AppendLine("```");
                        _WriteTree(racine, new HashSet<Person>(), mode, profondeur);
                        AppendLine("```");
                        return true;
                    }
                case "random":
                    {
                        if (G.All.Count == 0) 
                        {
                            AppendLine("Je connais personne");
                        }
                        else
                        {
                            var all = G.All.Select((a) => a.Value).ToList();
                            var p = all[(Rng.Next() % all.Count())];

                            DisplayInfo(p);
                            //AppendLine(Personne(p) + Italic(", je t'ai choisi !"));
                        }
                        return true;
                    }
                case "ban":
                    {
                        if (admin_mode) 
                        {
                            string uid = Pop();
                            if(uid == null) { return HelpBan(); }
                            if (Program.DiscordUIDBannedTmp.Add(uid)) 
                            {
                                AppendLine(Quote(uid) + " a été temporairement ban");
                            }
                            else 
                            {
                                AppendLine(Quote(uid) + " est déjà ban");
                            }
                            return true;
                        }
                        AppendLine("Té Ki Pour Ban ?");
                        return false;
                    }
                case "op":
                    {
                        if (admin_mode)
                        {
                            string uid = Pop();
                            if (uid == null) { return HelpOp(); }
                            if (Program.DiscordUIDAdmin.Add(uid))
                            {
                                AppendLine(Quote(uid) + " est maintenant un/une admin");
                                HaveSideEffect = true;
                            }
                            else
                            {
                                AppendLine(Quote(uid) + " est déjà un/une admin");
                            }
                            return true;
                        }
                        AppendLine("Té Ki Pour Mettre en Admin ?");
                        return false;
                    }
                case "deop":
                    {
                        if (admin_mode)
                        {
                            string uid = Pop();
                            if (uid == null) { return HelpDeop(); }
                            if (Program.DiscordUIDAdmin.Add(uid))
                            {
                                AppendLine(Quote(uid) + " est maitenant un simple utilisateur");
                                HaveSideEffect = true;
                            }
                            else
                            {
                                AppendLine(Quote(uid) + " n'était pas admin");
                            }
                            return true;
                        }
                        AppendLine("Té Ki Pour Retirer un Admin ?");
                        return false;
                    }
                case "undo":
                    {
                        if (admin_mode)
                        {
                            string nbStr = Pop();
                            if (nbStr == null) { return HelpUndo(); }
                            if(int.TryParse(nbStr, out int nb))
                            {
                                if(nb <= 0) 
                                {
                                    AppendLine("Essaie avec une quantité " + Bold("strictement") + " positive");
                                    return true;
                                }
                                for (int i = 0; i < nb; i++) 
                                {
                                    if(G.Commandes.Count() == 0) { break; }
                                    G.Commandes.RemoveAt(G.Commandes.Count - 1);
                                }
                                var cmd = G.Commandes;
                                G.Commandes = null;
                                G.Reset();
                                if(G.Reload(cmd) == false) { AppendLine("beug!"); }
                                AppendLine("Les " + nb + " dernières commandes ont été annulé !");
                                IsUndo = true;
                                return true;
                            }
                            else 
                            {
                                AppendLine("Je connais pas le nombre " + nbStr);
                            }
                        }
                        else
                        {
                            AppendLine("Té Ki Pour Undo ?");
                        }
                        return true;
                    }
                case "orphan": 
                case "fatherless": 
                case "motherless": 
                    {
                        var l = G.All.Select(t=>t.Value).Where(t=>t._Parents.Count() == 0).ToList();
                        l.Remove(G.Root);

                        AppendLine("Les personnes seules sont :");
                        foreach(var p in l) 
                        {
                            AppendLine("- "+ Personne(p));
                        }
                        return true;
                    }
                case "admin":
                case "admins":
                    {
                        AppendLine("Les admins sont :");

                        foreach (var v in Program.DiscordUIDAdmin) 
                        {
                            AppendLine("- " + Quote(v));
                        }
                        return true;
                    }
                case "banned":
                    {
                        AppendLine("Les bannis sont :");

                        foreach (var v in Program.DiscordUIDBannedTmp)
                        {
                            AppendLine("- " + Quote(v));
                        }
                        return true;
                    }
                case "admin?":
                    {
                        if (admin_mode) 
                        {
                            AppendLine("Oui, tu es un/une admin");
                        }
                        else 
                        {
                            AppendLine("Nope tu n'es pas admin");
                        }
                        return true;
                    }
                case "nickname":
                case "pseudo":
                    {
                        string prenom = Peek();
                        var p = GetPerson();
                        if(p != null) { prenom = p.prenom; }
                        Append(prenom + ", je te surnome " + Person.GenerateDumbPseudo(prenom));
                        return true;
                    }
                case "save":
                    {
                        if (admin_mode) 
                        {
                            ShouldSendSaveFile = true;
                            return true;
                        }
                        return HelpSave();
                    }
                    break;
                case "STOP":
                case "stop":
                    {
                        if (admin_mode)
                        {
                            ShouldStop = true;
                            AppendLine("Bye bye !");
                            return true;
                        }
                        return HelpSTOP();
                    }
                    break;
                case "anagramme":
                case "anagram": 
                    {
                        string mot = Pop();
                        if(mot == null) { return HelpAnagram(); }

                        var m = mot.ToLowerInvariant().ToList();
                        m.Sort();
                        var motStringSet = string.Join("", m);

                        var match = Find(input => 
                                        {
                                            var inputSet = input.ToLowerInvariant().ToList();
                                            inputSet.Sort();
                                            return string.Join("", inputSet).LevenshteinDistance(motStringSet);
                                        }, 3);


                        int nb_display = 0;
                        foreach (var t in match)
                        {
                            nb_display++;
                            Person p = t.Person;
                            AppendLine(" - #" + nb_display.ToString() + " : " + Personne(p) + (t.Score == 0 ? " ANAGRAMME PARFAIT !!!" : (" (anagram distance: " + t.Score + ")")));
                        }
                        return true;
                    }
                default: return false;
            }
            return false;
        }
    }
}

/*
help => show help
add nom prenom => uid
edit uid field value => change the value
tree uid => display the tree
info uid => display info about th person
is_parent_of uid_parent uid_child
search name|prenom|pseudo => uid
random => random person

op uid
deop uid
ban username
undo nb_roll_back => cancel last command
admin? => bool
orphan => return a list of orphan person (no parent)

 */