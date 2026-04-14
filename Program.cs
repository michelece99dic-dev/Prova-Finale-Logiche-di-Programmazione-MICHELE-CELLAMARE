using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Necessario per gestire i file
using System.Text.Json; // Necessario per il formato JSON

namespace GestioneAuleStudio
{
    class Prenotazione
    {
        public int Id { get; set; }
        public string Studente { get; set; }
        public string Aula { get; set; }
        public DateTime Data { get; set; }
        public string FasciaOraria { get; set; }
    }

    class Program
    {
        static List<Prenotazione> databasePrenotazioni = new List<Prenotazione>();
        static int contatoreId = 1;
        const int CAPIENZA_MASSIMA = 15;
        
        // Nome del file dove verranno salvati i dati
        static string pathFile = "prenotazioni.json";

        static string[] auleDisponibili = { "Aula A", "Aula B", "Aula Informatica", "Laboratorio" };
        static string[] fasceOrarie = { "09:00-11:00", "11:00-13:00", "14:00-16:00", "16:00-18:00" };

        static void Main(string[] args)
        {
            // All'avvio, carichiamo i dati salvati precedentemente
            CaricaDati();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA PRENOTAZIONE AULE (DATI PERSISTENTI) ===");
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

        #region GESTIONE FILE (PERSISTENZA)
        static void SalvaDati()
        {
            try
            {
                // Serializza la lista in una stringa JSON
                string jsonString = JsonSerializer.Serialize(databasePrenotazioni, new JsonSerializerOptions { WriteIndented = true });
                // Scrive la stringa nel file
                File.WriteAllText(pathFile, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio: {ex.Message}");
            }
        }

        static void CaricaDati()
        {
            try
            {
                if (File.Exists(pathFile))
                {
                    string jsonString = File.ReadAllText(pathFile);
                    databasePrenotazioni = JsonSerializer.Deserialize<List<Prenotazione>>(jsonString);
                    
                    // Aggiorniamo il contatore ID per non sovrascrivere quelli esistenti
                    if (databasePrenotazioni.Count > 0)
                    {
                        contatoreId = databasePrenotazioni.Max(p => p.Id) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento: {ex.Message}");
                databasePrenotazioni = new List<Prenotazione>();
            }
        }
        #endregion

        #region LOGICA STUDENTE (CON SALVATAGGIO AUTOMATICO)
        static void MenuStudente()
        {
            Console.Write("\nInserisci il tuo nome: ");
            string nomeStudente = Console.ReadLine();

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- MENU STUDENTE: {nomeStudente.ToUpper()} ---");
                Console.WriteLine("1. Prenota un posto");
                Console.WriteLine("2. Visualizza mie prenotazioni");
                Console.WriteLine("3. Modifica prenotazione");
                Console.WriteLine("4. Cancella prenotazione");
                Console.WriteLine("0. Torna indietro");
                
                string scelta = Console.ReadLine();
                if (scelta == "1") PrenotaAula(nomeStudente);
                else if (scelta == "2") VisualizzaPrenotazioni(nomeStudente);
                else if (scelta == "3") ModificaPrenotazione(nomeStudente);
                else if (scelta == "4") CancellaPrenotazione(nomeStudente);
                else if (scelta == "0") break;
            }
        }

        static void PrenotaAula(string studente)
        {
            Console.Clear();
            Console.WriteLine("--- NUOVA PRENOTAZIONE ---");
            for (int i = 0; i < auleDisponibili.Length; i++) Console.WriteLine($"{i + 1}. {auleDisponibili[i]}");
            Console.Write("Aula (n.): ");
            int indexAula = int.Parse(Console.ReadLine()) - 1;

            Console.Write("Data (gg/mm/aaaa): ");
            DateTime data = DateTime.Parse(Console.ReadLine());

            for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
            Console.Write("Orario (n.): ");
            int indexOra = int.Parse(Console.ReadLine()) - 1;

            string aulaScelta = auleDisponibili[indexAula];
            string oraScelta = fasceOrarie[indexOra];

            int postiOccupati = databasePrenotazioni.Count(p => p.Aula == aulaScelta && p.Data.Date == data.Date && p.FasciaOraria == oraScelta);

            if (postiOccupati >= CAPIENZA_MASSIMA)
            {
                Console.WriteLine("\n❌ Errore: L'aula è già completamente occupata in questo orario!");
            }
            else
            {
                databasePrenotazioni.Add(new Prenotazione {
                    Id = contatoreId++,
                    Studente = studente,
                    Aula = aulaScelta,
                    Data = data,
                    FasciaOraria = oraScelta
                });
                SalvaDati(); // SALVATAGGIO SU FILE
                Console.WriteLine($"\n✅ Prenotazione confermata! Posto {postiOccupati + 1}/15.");
            }
            Console.ReadKey();
        }

        static void ModificaPrenotazione(string studente)
        {
            VisualizzaPrenotazioni(studente);
            Console.Write("\nID da modificare: ");
            int id = int.Parse(Console.ReadLine());
            var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente.Equals(studente, StringComparison.OrdinalIgnoreCase));

            if (p != null)
            {
                Console.Write("Nuova data (gg/mm/aaaa): ");
                DateTime nData = DateTime.Parse(Console.ReadLine());
                for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
                int indexOra = int.Parse(Console.ReadLine()) - 1;

                int postiOccupati = databasePrenotazioni.Count(x => x.Aula == p.Aula && x.Data.Date == nData.Date && x.FasciaOraria == fasceOrarie[indexOra] && x.Id != id);

                if (postiOccupati < CAPIENZA_MASSIMA)
                {
                    p.Data = nData;
                    p.FasciaOraria = fasceOrarie[indexOra];
                    SalvaDati(); // AGGIORNAMENTO SU FILE
                    Console.WriteLine("✅ Modifica salvata su disco.");
                }
                else Console.WriteLine("\n❌ Errore: Aula piena nel nuovo orario!");
            }
            Console.ReadKey();
        }

        static void CancellaPrenotazione(string studente)
        {
            VisualizzaPrenotazioni(studente);
            Console.Write("\nID da cancellare: ");
            int id = int.Parse(Console.ReadLine());
            var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente.Equals(studente, StringComparison.OrdinalIgnoreCase));
            if (p != null) 
            { 
                databasePrenotazioni.Remove(p); 
                SalvaDati(); // RIMOZIONE DAL FILE
                Console.WriteLine("✅ Prenotazione eliminata."); 
            }
            Console.ReadKey();
        }
        #endregion

        static void VisualizzaPrenotazioni(string studente)
        {
            Console.Clear();
            var mie = databasePrenotazioni.Where(p => p.Studente.Equals(studente, StringComparison.OrdinalIgnoreCase)).ToList();
            if (mie.Count == 0) Console.WriteLine("Nessuna prenotazione trovata.");
            else
            {
                foreach (var p in mie)
                    Console.WriteLine($"ID: {p.Id} | {p.Aula} | {p.Data:dd/MM/yyyy} | {p.FasciaOraria}");
            }
            Console.ReadKey();
        }

        static void MenuAdmin()
        {
            Console.Clear();
            Console.WriteLine("--- REPORT MANAGER (DATI CARICATI DA FILE) ---");
            if (databasePrenotazioni.Count == 0) Console.WriteLine("Database vuoto.");
            foreach (var p in databasePrenotazioni)
            {
                Console.WriteLine($"ID: {p.Id} | [{p.Aula}] | {p.Data:dd/MM} | {p.FasciaOraria} | Studente: {p.Studente}");
            }
            Console.WriteLine("\nPremi un tasto...");
            Console.ReadKey();
        }
    }
}