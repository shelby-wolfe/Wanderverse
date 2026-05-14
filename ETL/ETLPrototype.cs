using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Wanderverse.Data;

namespace Wanderverse.ETL
{
    public static class ETLPrototype
    {
        public static void RunETL()
        {
            string dbPath = "game.db";

            // Initialize database
            var db = new Database(dbPath);

            db.InitializeDatabase();

            // Load CSV data
            ETLLoader.LoadLocations(db, "Data/Locations.csv");
            ETLLoader.LoadChoices(db, "Data/Choices.csv");
            ETLLoader.LoadPlayers(db, "Data/Players.csv");

            // Generate random player paths (20 moves per player)
            ETLLoader.LoadRandomPlayerPaths(db, 20);

            Console.WriteLine("ETL completed successfully!");
        }
    }

    static class ETLLoader
    {
        public static void LoadLocations(Database db, string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Locations CSV not found: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++) // skip header
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 3) continue;

                if (!int.TryParse(parts[0], out int locationId))
                {
                    Console.WriteLine($"Skipping invalid LocationId on line {i+1}");
                    continue;
                }

                string name = parts[1].Trim();
                string description = parts[2].Trim();

                db.AddLocation(locationId, name, description);
            }
        }

        public static void LoadChoices(Database db, string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Choices CSV not found: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 4) continue;

                if (!int.TryParse(parts[0], out int choiceId) ||
                    !int.TryParse(parts[1], out int fromId) ||
                    !int.TryParse(parts[2], out int toId))
                {
                    Console.WriteLine($"Skipping invalid choice IDs on line {i+1}");
                    continue;
                }

                string choiceText = parts[3].Trim();

                db.AddChoice(choiceId, fromId, toId, choiceText);
            }
        }

        public static void LoadPlayers(Database db, string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Players CSV not found: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            for (int i = 1; i < lines.Length; i++)
            {
                string username = lines[i].Trim();
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine($"Skipping empty username on line {i+1}");
                    continue;
                }

                db.AddPlayer(username);
            }
        }

        public static void LoadRandomPlayerPaths(Database db, int movesPerPlayer)
        {
            var playerIds = db.GetAllPlayerIds();
            var locationIds = db.GetAllLocationIds();
            var rand = new Random();

            foreach (var playerId in playerIds)
            {
                for (int i = 0; i < movesPerPlayer; i++)
                {
                    int locationId = locationIds[rand.Next(locationIds.Count)];
                    db.RecordPlayerMove(playerId, locationId);
                }
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            string current = "";

            foreach (var c in line)
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes)
                {
                    parts.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            parts.Add(current);
            return parts.ToArray();
        }
    }
}