using System;
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
    public class Person
    {
        public string _Uid = "";

        //public enum AdminMode { User, Admin, SuperAdmin};
        //AdminMode _Mode = AdminMode.User;

        // Underscore = Editable Field
        public string prenom = "";
        public string pseudo = "";
        public string nom = "";
        public string pronoms = "";
        public string mail = "";
        public string anniv = "";
        public string discord = "";
        public bool _HaveDiscord => discord.Length > 0;

        public string joue_a_lol = "";
        public string description = "";
        public string pouicable = "";

        public string promo = "";
        public string filiere = "";
        public string club = "";
        public string provenance = "";


        public static List<string> GetEditableField()
        {
            List<string> underscoreFields = new List<string>();

            Type type = typeof(Person);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.Name.StartsWith("_") == false)
                {
                    underscoreFields.Add(field.Name);
                }
            }

            return underscoreFields;
        }

        public bool Edit(string fieldName, string newValue)
        {
            FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)!;

            if (field != null)
            {
                field.SetValue(this, newValue);
                return true;
            }
            return false;
        }

        // Method to get the field value by name
        public string? Get(string fieldName)
        {
            FieldInfo field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)!;

            if (field != null)
            {
                return (string)field.GetValue(this)!;
            }

            return null; // Field not found
        }

        public List<Person> _Childs = new List<Person>();
        public List<Person> _Parents = new List<Person>();

        public static string GenerateDumbPseudo(string input)
        {
            input = input.ToLowerInvariant();
            if (string.IsNullOrEmpty(input)) { return ""; }
            if (input.Length <= 4) { return input; }

            string pseudo = "";
            bool voyelle = false;
            bool consonne = false;

            for(int i = 0;i< input.Length; i++) 
            {
                char c = input[i];
                
                if(c == 'h') { continue; } // muet
                
                if ("zrtpqsdfhjklmwxcvbnp".Contains(c)) { pseudo += c; consonne = true; }

                if ("aeyuioéè".Contains(c)) 
                {
                    voyelle = true;
                    while (i < input.Length && "aeyuioéè".Contains(input[i]))
                    {
                        c = input[i];
                        pseudo += c;
                        i++;
                    }
                    i--;
                }

                
                if(voyelle && consonne) { goto end; }
            }
            end:

            if (pseudo.Length <= 4)
            {
                pseudo = pseudo + pseudo;
            }
            pseudo = pseudo[0].ToString().ToUpperInvariant() + (pseudo.Length > 1 ? pseudo.Substring(1) : "");
            return pseudo;
        }

        public Person(string nom, string prenom, string pseudo, string uid)
        {
            this.prenom = prenom;
            this.nom = nom;
            this.pseudo = pseudo;
            _Uid = uid;
        }
    }
}


