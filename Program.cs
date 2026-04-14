using System;
using System.Collections.Generic;
using System.Linq;

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
        // Costante per la capienza
        const int CAPIENZA_MASSIMA = 15; 

        static string[] auleDisponibili = { "Aula A", "Aula B", "Aula Informatica", "Laboratorio" };
        static string[] fasceOrarie = { "09:00-11:00", "11:00-13:00", "14:00-16:00", "16:00-18:00" };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA PRENOTAZIONE AULE (MAX 15 POSTI) ===");
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
            Console.Write("Scegli l'aula (numero): ");
            int indexAula = int.Parse(Console.ReadLine()) - 1;

            Console.Write("Data (gg/mm/aaaa): ");
            DateTime data = DateTime.Parse(Console.ReadLine());

            for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
            Console.Write("Fascia oraria (numero): ");
            int indexOra = int.Parse(Console.ReadLine()) - 1;

            string aulaScelta = auleDisponibili[indexAula];
            string oraScelta = fasceOrarie[indexOra];

            // MODIFICA: Conteggio delle prenotazioni esistenti per quel turno
            int postiOccupati = databasePrenotazioni.Count(p => 
                p.Aula == aulaScelta && 
                p.Data.Date == data.Date && 
                p.FasciaOraria == oraScelta);

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
                Console.WriteLine($"\n✅ Prenotazione confermata! (Posto {postiOccupati + 1} di {CAPIENZA_MASSIMA})");
            }
            Console.ReadKey();
        }

        static void VisualizzaPrenotazioni(string studente)
        {
            Console.Clear();
            var mie = databasePrenotazioni.Where(p => p.Studente.Equals(studente, StringComparison.OrdinalIgnoreCase)).ToList();
            if (mie.Count == 0) Console.WriteLine("Nessuna prenotazione.");
            else
            {
                foreach (var p in mie)
                    Console.WriteLine($"ID: {p.Id} | {p.Aula} | {p.Data:dd/MM/yyyy} | {p.FasciaOraria}");
            }
            Console.ReadKey();
        }

        static void ModificaPrenotazione(string studente)
        {
            VisualizzaPrenotazioni(studente);
            Console.Write("\nID da modificare: ");
            int id = int.Parse(Console.ReadLine());
            var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente == studente);

            if (p != null)
            {
                Console.Write("Nuova data (gg/mm/aaaa): ");
                DateTime nData = DateTime.Parse(Console.ReadLine());
                for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
                int indexOra = int.Parse(Console.ReadLine()) - 1;

                // MODIFICA: Controllo capienza anche per la modifica
                int postiOccupati = databasePrenotazioni.Count(x => 
                    x.Aula == p.Aula && 
                    x.Data.Date == nData.Date && 
                    x.FasciaOraria == fasceOrarie[indexOra] && 
                    x.Id != id); // Escludiamo la prenotazione stessa che stiamo modificando

                if (postiOccupati < CAPIENZA_MASSIMA)
                {
                    p.Data = nData;
                    p.FasciaOraria = fasceOrarie[indexOra];
                    Console.WriteLine("✅ Modifica effettuata.");
                }
                else Console.WriteLine("\n❌ Errore: L'aula è già completamente occupata nel nuovo orario scelto!");
            }
            Console.ReadKey();
        }

        static void CancellaPrenotazione(string studente)
        {
            VisualizzaPrenotazioni(studente);
            Console.Write("\nID da cancellare: ");
            int id = int.Parse(Console.ReadLine());
            var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente == studente);
            if (p != null) { databasePrenotazioni.Remove(p); Console.WriteLine("✅ Cancellata."); }
            Console.ReadKey();
        }

        static void MenuAdmin()
        {
            Console.Clear();
            Console.WriteLine("--- REPORT MANAGER (TUTTE LE PRENOTAZIONI) ---");
            foreach (var p in databasePrenotazioni)
            {
                Console.WriteLine($"[{p.Aula}] {p.Data:dd/MM} {p.FasciaOraria} - Studente: {p.Studente}");
            }
            Console.WriteLine("\nPremi un tasto...");
            Console.ReadKey();
        }
    }
}