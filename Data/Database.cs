using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Wanderverse.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        // Creates tables if they don't exist
        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Player (
                PlayerId INTEGER PRIMARY KEY,
                Username TEXT NOT NULL UNIQUE
            );
            CREATE TABLE IF NOT EXISTS Location (
                LocationId INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT
            );
            CREATE TABLE IF NOT EXISTS Choice (
                ChoiceId INTEGER PRIMARY KEY,
                FromLocationId INTEGER NOT NULL,
                ToLocationId INTEGER NOT NULL,
                ChoiceText TEXT NOT NULL,
                FOREIGN KEY (FromLocationId) REFERENCES Location(LocationId) ON DELETE CASCADE,
                FOREIGN KEY (ToLocationId) REFERENCES Location(LocationId) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS PlayerPath (
                PathId INTEGER PRIMARY KEY,
                PlayerId INTEGER NOT NULL,
                LocationId INTEGER NOT NULL,
                Timestamp TEXT,
                FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId) ON DELETE CASCADE,
                FOREIGN KEY (LocationId) REFERENCES Location(LocationId) ON DELETE CASCADE
            );
            ";
            cmd.ExecuteNonQuery();
        }

        public void AddPlayer(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Player WHERE Username = $username";
            checkCmd.Parameters.AddWithValue("$username", username);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
            if (exists)            {
                throw new Exception($"Player with username '{username}' already exists.");
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Player (Username) VALUES ($username)";
            cmd.Parameters.AddWithValue("$username", username);
            cmd.ExecuteNonQuery();
        }

        public void AddLocation(int locationId, string name, string description)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Location (LocationId, Name, Description) VALUES ($id, $name, $desc)";
            cmd.Parameters.AddWithValue("$id", locationId);
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$desc", description);
            cmd.ExecuteNonQuery();
        }

        public void AddChoice(int choiceId, int fromId, int toId, string choiceText)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT OR IGNORE INTO Choice 
                (ChoiceId, FromLocationId, ToLocationId, ChoiceText)
                VALUES ($id, $from, $to, $text)";
            cmd.Parameters.AddWithValue("$id", choiceId);
            cmd.Parameters.AddWithValue("$from", fromId);
            cmd.Parameters.AddWithValue("$to", toId);
            cmd.Parameters.AddWithValue("$text", choiceText);
            cmd.ExecuteNonQuery();
        }

        public void RecordPlayerMove(int playerId, int locationId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PlayerPath (PlayerId, LocationId, Timestamp) 
                VALUES ($playerId, $locId, $timestamp)";
            cmd.Parameters.AddWithValue("$playerId", playerId);
            cmd.Parameters.AddWithValue("$locId", locationId);
            cmd.Parameters.AddWithValue("$timestamp", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public List<int> GetAllPlayerIds()
        {
            var list = new List<int>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT PlayerId FROM Player";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetInt32(0));
            }
            return list;
        }

        public List<int> GetAllLocationIds()
        {
            var list = new List<int>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT LocationId FROM Location";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetInt32(0));
            }
            return list;
        }

        public List<string> GetPlayerPaths(int playerId)
        {
            var paths = new List<string>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Location.Name, PlayerPath.Timestamp
                FROM PlayerPath
                JOIN Location ON PlayerPath.LocationId = Location.LocationId
                WHERE PlayerPath.PlayerId = $playerId
                ORDER BY PlayerPath.Timestamp";
            cmd.Parameters.AddWithValue("$playerId", playerId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                paths.Add($"{reader.GetString(0)} at {reader.GetString(1)}");
            }
            return paths;
        }
    }
}