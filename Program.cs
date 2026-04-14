using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

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
        static List<string> auleDisponibili = new List<string>();
        
        static int contatoreId = 1;
        const int CAPIENZA_MASSIMA = 15;
        static string pathPrenotazioni = "prenotazioni.json";
        static string pathAule = "aule.json";

        static string[] fasceOrarie = { "09:00-11:00", "11:00-13:00", "14:00-16:00", "16:00-18:00" };

        static void Main(string[] args)
        {
            CaricaDati();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA GESTIONE AULE STUDIO V5 ===");
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
                File.WriteAllText(pathAule, JsonSerializer.Serialize(auleDisponibili, new JsonSerializerOptions { WriteIndented = true }));
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
                    auleDisponibili = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(pathAule));
                }
                else
                {
                    auleDisponibili = new List<string> { "Aula A", "Aula B" };
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
                Console.WriteLine("2. Elimina una singola prenotazione");
                Console.WriteLine("3. Aggiungi nuova aula");
                Console.WriteLine("4. ELIMINA AULA");
                Console.WriteLine("5. Visualizza elenco aule");
                Console.WriteLine("0. Torna indietro");
                Console.Write("\nScelta: ");

                string scelta = Console.ReadLine();
                if (scelta == "1") VisualizzaTutteLePrenotazioni();
                else if (scelta == "2") EliminaPrenotazioneAdmin();
                else if (scelta == "3") AggiungiAula();
                else if (scelta == "4") EliminaAula();
                else if (scelta == "5") { VisualizzaAule(); Console.ReadKey(); }
                else if (scelta == "0") break;
            }
        }

        static void EliminaAula()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAZIONE AULA STUDIO ---");
            VisualizzaAule();
            Console.Write("\nInserisci il numero dell'aula da eliminare permanentemente: ");
            
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= auleDisponibili.Count)
            {
                string aulaDaRimuovere = auleDisponibili[idx - 1];

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nATTENZIONE: Eliminando '{aulaDaRimuovere}' verranno cancellate anche tutte le prenotazioni associate!");
                Console.ResetColor();
                Console.Write("Confermare l'operazione? (S/N): ");
                
                if (Console.ReadLine().ToUpper() == "S")
                {
                    // 1. Rimuoviamo l'aula dalla lista delle aule
                    auleDisponibili.RemoveAt(idx - 1);

                    // 2. Rimuoviamo tutte le prenotazioni che hanno quell'aula
                    int rimosse = databasePrenotazioni.RemoveAll(p => p.Aula == aulaDaRimuovere);

                    SalvaTutto();
                    Console.WriteLine($"\n✅ Aula '{aulaDaRimuovere}' eliminata.");
                    Console.WriteLine($"✅ Rimosse {rimosse} prenotazioni associate.");
                }
                else { Console.WriteLine("\nOperazione annullata."); }
            }
            else { Console.WriteLine("\nScelta non valida."); }
            Console.ReadKey();
        }

        static void VisualizzaTutteLePrenotazioni()
        {
            Console.Clear();
            Console.WriteLine("--- ELENCO COMPLETO PRENOTAZIONI ---");
            if (databasePrenotazioni.Count == 0) Console.WriteLine("Nessuna prenotazione.");
            foreach (var p in databasePrenotazioni)
                Console.WriteLine($"ID: {p.Id} | Studente: {p.Studente} | Aula: {p.Aula} | {p.Data:dd/MM/yyyy} | {p.FasciaOraria}");
            Console.ReadKey();
        }

        static void EliminaPrenotazioneAdmin()
        {
            Console.Clear();
            foreach (var p in databasePrenotazioni) Console.WriteLine($"ID: {p.Id} | {p.Studente} - {p.Aula}");
            Console.Write("\nID prenotazione da rimuovere: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id);
                if (p != null) { databasePrenotazioni.Remove(p); SalvaTutto(); Console.WriteLine("✅ Rimossa."); }
            }
            Console.ReadKey();
        }

        static void AggiungiAula()
        {
            Console.Clear();
            Console.Write("Nome nuova aula: ");
            string nome = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nome) && !auleDisponibili.Contains(nome))
            {
                auleDisponibili.Add(nome);
                SalvaTutto();
                Console.WriteLine("✅ Aula aggiunta.");
            }
            Console.ReadKey();
        }
        #endregion

        #region MENU STUDENTE
        static void MenuStudente()
        {
            Console.Write("\nNome studente: ");
            string nomeStudente = Console.ReadLine();
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- MENU STUDENTE: {nomeStudente.ToUpper()} ---");
                Console.WriteLine("1. Prenota");
                Console.WriteLine("2. Mie Prenotazioni");
                Console.WriteLine("3. Modifica");
                Console.WriteLine("4. Cancella");
                Console.WriteLine("0. Esci");
                string scelta = Console.ReadLine();
                if (scelta == "1") PrenotaAula(nomeStudente);
                else if (scelta == "2") VisualizzaMiePrenotazioni(nomeStudente);
                else if (scelta == "3") ModificaPrenotazione(nomeStudente);
                else if (scelta == "4") CancellaPrenotazione(nomeStudente);
                else if (scelta == "0") break;
            }
        }

        static void VisualizzaAule()
        {
            for (int i = 0; i < auleDisponibili.Count; i++)
                Console.WriteLine($"{i + 1}. {auleDisponibili[i]}");
        }

        static void PrenotaAula(string studente)
        {
            Console.Clear();
            VisualizzaAule();
            Console.Write("Scelta aula (n.): ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= auleDisponibili.Count)
            {
                string aula = auleDisponibili[idx - 1];
                Console.Write("Data (gg/mm/aaaa): ");
                DateTime data = DateTime.Parse(Console.ReadLine());
                for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
                int oIdx = int.Parse(Console.ReadLine()) - 1;
                string ora = fasceOrarie[oIdx];

                int occ = databasePrenotazioni.Count(p => p.Aula == aula && p.Data.Date == data.Date && p.FasciaOraria == ora);
                if (occ >= CAPIENZA_MASSIMA) Console.WriteLine("\n❌ Errore: L'aula è già completamente occupata in questo orario!");
                else
                {
                    databasePrenotazioni.Add(new Prenotazione { Id = contatoreId++, Studente = studente, Aula = aula, Data = data, FasciaOraria = ora });
                    SalvaTutto();
                    Console.WriteLine("✅ Prenotato!");
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

        static void ModificaPrenotazione(string studente)
        {
            VisualizzaMiePrenotazioni(studente);
            Console.Write("ID da modificare: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente == studente);
                if (p != null)
                {
                    Console.Write("Nuova data (gg/mm/aaaa): ");
                    DateTime nD = DateTime.Parse(Console.ReadLine());
                    for (int i = 0; i < fasceOrarie.Length; i++) Console.WriteLine($"{i + 1}. {fasceOrarie[i]}");
                    int oI = int.Parse(Console.ReadLine()) - 1;
                    if (databasePrenotazioni.Count(x => x.Aula == p.Aula && x.Data.Date == nD.Date && x.FasciaOraria == fasceOrarie[oI] && x.Id != id) < CAPIENZA_MASSIMA)
                    {
                        p.Data = nD; p.FasciaOraria = fasceOrarie[oI]; SalvaTutto(); Console.WriteLine("✅ Modificato.");
                    }
                    else Console.WriteLine("❌ Aula piena.");
                }
            }
            Console.ReadKey();
        }

        static void CancellaPrenotazione(string studente)
        {
            VisualizzaMiePrenotazioni(studente);
            Console.Write("ID da cancellare: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var p = databasePrenotazioni.FirstOrDefault(x => x.Id == id && x.Studente == studente);
                if (p != null) { databasePrenotazioni.Remove(p); SalvaTutto(); Console.WriteLine("✅ Cancellato."); }
            }
            Console.ReadKey();
        }
        #endregion
    }
}