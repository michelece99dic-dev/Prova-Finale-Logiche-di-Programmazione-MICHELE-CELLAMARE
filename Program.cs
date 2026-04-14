using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace GestioneAuleStudio
{
    // Modello per l'Aula con capienza personalizzata
    class Aula
    {
        public string Nome { get; set; }
        public int Capienza { get; set; }
    }

    class Prenotazione
    {
        public int Id { get; set; }
        public string Studente { get; set; }
        public string Aula { get; set; } // Nome dell'aula di riferimento
        public DateTime Data { get; set; }
        public string FasciaOraria { get; set; }
    }

    class Program
    {
        static List<Prenotazione> databasePrenotazioni = new List<Prenotazione>();
        static List<Aula> elencoAule = new List<Aula>(); // Lista di oggetti Aula
        
        static int contatoreId = 1;
        const int CAPIENZA_DEFAULT = 15;

        static string pathPrenotazioni = "prenotazioni.json";
        static string pathAule = "aule_v7.json"; // Nuovo file per la nuova struttura

        static string[] fasceOrarie = { "09:00-11:00", "11:00-13:00", "14:00-16:00", "16:00-18:00" };

        static void Main(string[] args)
        {
            CaricaDati();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA GESTIONE AULE STUDIO V7 ===");
                Console.WriteLine("1. Accesso Studente");
                Console.WriteLine("2. Accesso Amministratore (Manager)");
                Console.WriteLine("0. Esci");
                Console.Write("\nSeleziona un'opzione: ");

                string scelta = Console.ReadLine();
                if (scelta == "1") MenuStudente();
                else if (scelta == "2") MenuAdmin();
                else if (scelta == "0") break;
            }
        }

        #region PERSISTENZA
        static void SalvaTutto()
        {
            try
            {
                File.WriteAllText(pathPrenotazioni, JsonSerializer.Serialize(databasePrenotazioni, new JsonSerializerOptions { WriteIndented = true }));
                File.WriteAllText(pathAule, JsonSerializer.Serialize(elencoAule, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { Console.WriteLine($"Errore salvataggio: {ex.Message}"); }
        }

        static void CaricaDati()
        {
            try
            {
                if (File.Exists(pathPrenotazioni))
                {
                    databasePrenotazioni = JsonSerializer.Deserialize<List<Prenotazione>>(File.ReadAllText(pathPrenotazioni));
                    if (databasePrenotazioni.Count > 0) contatoreId = databasePrenotazioni.Max(p => p.Id) + 1;
                }
                
                if (File.Exists(pathAule))
                {
                    elencoAule = JsonSerializer.Deserialize<List<Aula>>(File.ReadAllText(pathAule));
                }
                else
                {
                    // Aule iniziali di default
                    elencoAule = new List<Aula> 
                    { 
                        new Aula { Nome = "Aula A", Capienza = CAPIENZA_DEFAULT },
                        new Aula { Nome = "Aula B", Capienza = CAPIENZA_DEFAULT }
                    };
                    SalvaTutto();
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region MENU ADMIN
        static void MenuAdmin()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- PANNELLO AMMINISTRATORE ---");
                Console.WriteLine("1. Visualizza tutte le prenotazioni");
                Console.WriteLine("2. Elimina una prenotazione");
                Console.WriteLine("3. Aggiungi nuova aula");
                Console.WriteLine("4. Elimina aula");
                Console.WriteLine("5. Modifica capienza aule");
                Console.WriteLine("0. Torna indietro");
                Console.Write("\nScelta: ");

                string scelta = Console.ReadLine();
                if (scelta == "1") VisualizzaTutteLePrenotazioni();
                else if (scelta == "2") EliminaPrenotazioneAdmin();
                else if (scelta == "3") AggiungiAula();
                else if (scelta == "4") EliminaAula();
                else if (scelta == "5") AumentaCapienzaAulaSpecifica();
                else if (scelta == "0") break;
            }
        }

        static void AumentaCapienzaAulaSpecifica()
        {
            Console.Clear();
            Console.WriteLine("--- MODIFICA CAPIENZA PER AULA ---");
            VisualizzaAule();
            
            Console.Write("\nSeleziona il numero dell'aula da modificare: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= elencoAule.Count)
            {
                Aula aulaSelezionata = elencoAule[idx - 1];
                Console.WriteLine($"\nAula scelta: {aulaSelezionata.Nome}");
                Console.WriteLine($"Capienza attuale: {aulaSelezionata.Capienza} posti");
                
                Console.Write("Inserisci la nuova capienza (solo aumento): ");
                if (int.TryParse(Console.ReadLine(), out int nuovaCapienza))
                {
                    if (nuovaCapienza > aulaSelezionata.Capienza)
                    {
                        aulaSelezionata.Capienza = nuovaCapienza;
                        SalvaTutto();
                        Console.WriteLine($"\n✅ Successo! La capienza di {aulaSelezionata.Nome} è ora di {nuovaCapienza} posti.");
                    }
                    else
                    {
                        Console.WriteLine($"\n❌ Errore: Il nuovo valore deve essere superiore a {aulaSelezionata.Capienza}.");
                    }
                }
            }
            else Console.WriteLine("\n❌ Selezione non valida.");
            Console.ReadKey();
        }

        static void AggiungiAula()
        {
            Console.Clear();
            Console.Write("Nome nuova aula: ");
            string nome = Console.ReadLine();
            Console.Write("Capienza iniziale: ");
            if (int.TryParse(Console.ReadLine(), out int cap) && !string.IsNullOrWhiteSpace(nome))
            {
                elencoAule.Add(new Aula { Nome = nome, Capienza = cap });
                SalvaTutto();
                Console.WriteLine("✅ Aula creata.");
            }
            Console.ReadKey();
        }

        static void EliminaAula()
        {
            Console.Clear();
            VisualizzaAule();
            Console.Write("\nNumero aula da eliminare: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= elencoAule.Count)
            {
                string nomeAula = elencoAule[idx - 1].Nome;
                elencoAule.RemoveAt(idx - 1);
                databasePrenotazioni.RemoveAll(p => p.Aula == nomeAula);
                SalvaTutto();
                Console.WriteLine($"✅ Aula {nomeAula} e relative prenotazioni rimosse.");
            }
            Console.ReadKey();
        }

        static void VisualizzaTutteLePrenotazioni()
        {
            Console.Clear();
            foreach (var p in databasePrenotazioni)
                Console.WriteLine($"ID: {p.Id} | {p.Studente} | {p.Aula} | {p.Data:dd/MM/yyyy} | {p.FasciaOraria}");
            if (!databasePrenotazioni.Any()) Console.WriteLine("Nessuna prenotazione.");
            Console.ReadKey();
        }

        static void EliminaPrenotazioneAdmin()
        {
            Console.Clear();
            foreach (var p in databasePrenotazioni) Console.WriteLine($"ID: {p.Id} | {p.Studente} - {p.Aula}");
            Console.Write("\nID da rimuovere: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                databasePrenotazioni.RemoveAll(x => x.Id == id);
                SalvaTutto();
                Console.WriteLine("✅ Rimossa.");
            }
            Console.ReadKey();
        }
        #endregion

        #region MENU STUDENTE
        static void MenuStudente()
        {
            Console.Write("\nInserisci nome studente: ");
            string nome = Console.ReadLine();
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- STUDENTE: {nome.ToUpper()} ---");
                Console.WriteLine("1. Prenota");
                Console.WriteLine("2. Mie Prenotazioni");
                Console.WriteLine("0. Esci");
                string scelta = Console.ReadLine();
                if (scelta == "1") PrenotaAula(nome);
                else if (scelta == "2") VisualizzaMiePrenotazioni(nome);
                else if (scelta == "0") break;
            }
        }

        static void VisualizzaAule()
        {
            for (int i = 0; i < elencoAule.Count; i++)
                Console.WriteLine($"{i + 1}. {elencoAule[i].Nome} (Capienza: {elencoAule[i].Capienza} posti)");
        }

        static void PrenotaAula(string studente)
        {
            Console.Clear();
            VisualizzaAule();
            Console.Write("\nScegli aula (n.): ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= elencoAule.Count)
            {
                Aula aulaScelta = elencoAule[idx - 1];
                Console.Write("Data (gg/mm/aaaa): ");
                DateTime data = DateTime.Parse(Console.ReadLine());
                for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
                int oIdx = int.Parse(Console.ReadLine()) - 1;
                string ora = fasceOrarie[oIdx];

                // Controllo capienza SPECIFICA di quell'aula
                int occupati = databasePrenotazioni.Count(p => p.Aula == aulaScelta.Nome && p.Data.Date == data.Date && p.FasciaOraria == ora);

                if (occupati >= aulaScelta.Capienza)
                {
                    Console.WriteLine($"\n❌ Errore: L'aula {aulaScelta.Nome} è già piena ({aulaScelta.Capienza}/{aulaScelta.Capienza} posti occupati)!");
                }
                else
                {
                    databasePrenotazioni.Add(new Prenotazione { Id = contatoreId++, Studente = studente, Aula = aulaScelta.Nome, Data = data, FasciaOraria = ora });
                    SalvaTutto();
                    Console.WriteLine($"\n✅ Prenotato! Posto {occupati + 1} di {aulaScelta.Capienza}.");
                }
            }
            Console.ReadKey();
        }

        static void VisualizzaMiePrenotazioni(string studente)
        {
            Console.Clear();
            var mie = databasePrenotazioni.Where(p => p.Studente.Equals(studente, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var p in mie) Console.WriteLine($"ID: {p.Id} | {p.Aula} | {p.Data:dd/MM/yyyy} | {p.FasciaOraria}");
            Console.ReadKey();
        }
        #endregion
    }
}