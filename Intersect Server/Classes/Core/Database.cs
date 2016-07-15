﻿/*
    Intersect Game Engine (Server)
    Copyright (C) 2015  JC Snider, Joe Bridges
    
    Website: http://ascensiongamedev.com
    Contact Email: admin@ascensiongamedev.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Intersect_Library;
using Intersect_Library.GameObjects;
using Intersect_Library.GameObjects.Events;
using Intersect_Library.GameObjects.Maps;
using Intersect_Library.GameObjects.Maps.MapList;
using Intersect_Server.Classes.Entities;
using Intersect_Server.Classes.General;
using Intersect_Server.Classes.Items;
using Intersect_Server.Classes.Maps;
using Intersect_Server.Classes.Networking;
using MapInstance = Intersect_Server.Classes.Maps.MapInstance;
using Mono.Data.Sqlite;

namespace Intersect_Server.Classes.Core
{
    public static class Database
    {
        private static SqliteConnection _dbConnection;
        private static Object _dbLock = new Object();
        private const int DbVersion = 2;
        private const string DbFilename = "resources/intersect.db";

        //Database Variables
        private const string INFO_TABLE = "info";
        private const string DB_VERSION = "dbversion";

        //Ban Table Constants
        private const string BAN_TABLE = "bans";
        private const string BAN_ID = "id";
        private const string BAN_TIME = "time";
        private const string BAN_USER = "user";
        private const string BAN_IP = "ip";
        private const string BAN_DURATION = "duration";
        private const string BAN_REASON = "reason";
        private const string BAN_BANNER = "banner";

        //Mute Table Constants
        private const string MUTE_TABLE = "mutes";
        private const string MUTE_ID = "id";
        private const string MUTE_TIME = "time";
        private const string MUTE_USER = "user"; 
        private const string MUTE_IP = "ip"; 
        private const string MUTE_DURATION = "duration";
        private const string MUTE_REASON = "reason";
        private const string MUTE_MUTER = "muter";

        //Log Table Constants
        private const string LOG_TABLE = "logs";
        private const string LOG_ID = "id";
        private const string LOG_TIME = "time";
        private const string LOG_TYPE = "type";
        private const string LOG_INFO = "info";

        //User Table Constants
        private const string USERS_TABLE = "users";
        private const string USER_ID = "id";
        private const string USER_NAME = "user";
        private const string USER_PASS = "pass";
        private const string USER_SALT = "salt";
        private const string USER_EMAIL = "email";
        private const string USER_POWER = "power";

        //Character Table Constants
        private const string CHAR_TABLE = "characters";
        private const string CHAR_ID = "id";
        private const string CHAR_USER_ID = "user_id";
        private const string CHAR_NAME = "name";
        private const string CHAR_MAP = "map";
        private const string CHAR_X = "x";
        private const string CHAR_Y = "y";
        private const string CHAR_Z = "z";
        private const string CHAR_DIR = "dir";
        private const string CHAR_SPRITE = "sprite";
        private const string CHAR_FACE = "face";
        private const string CHAR_CLASS = "class";
        private const string CHAR_GENDER = "gender";
        private const string CHAR_LEVEL = "level";
        private const string CHAR_EXP = "exp";
        private const string CHAR_VITALS = "vitals";
        private const string CHAR_MAX_VITALS = "maxvitals";
        private const string CHAR_STATS = "stats";
        private const string CHAR_STAT_POINTS = "statpoints";
        private const string CHAR_EQUIPMENT = "equipment";

        //Char Inventory Table Constants
        private const string CHAR_INV_TABLE = "char_inventory";
        private const string CHAR_INV_CHAR_ID = "char_id";
        private const string CHAR_INV_SLOT = "slot";
        private const string CHAR_INV_ITEM_NUM = "itemnum";
        private const string CHAR_INV_ITEM_VAL = "itemval";
        private const string CHAR_INV_ITEM_STATS = "itemstats";

        //Char Spells Table Constants
        private const string CHAR_SPELL_TABLE = "char_spells";
        private const string CHAR_SPELL_CHAR_ID = "char_id";
        private const string CHAR_SPELL_SLOT = "slot";
        private const string CHAR_SPELL_NUM = "spellnum";
        private const string CHAR_SPELL_CD = "spellcd";

        //Char Hotbar Table Constants
        private const string CHAR_HOTBAR_TABLE = "char_hotbar";
        private const string CHAR_HOTBAR_CHAR_ID = "char_id";
        private const string CHAR_HOTBAR_SLOT = "slot";
        private const string CHAR_HOTBAR_TYPE = "type";
        private const string CHAR_HOTBAR_ITEMSLOT = "itemslot";

        //Char Bank Table Constants
        private const string CHAR_BANK_TABLE = "char_bank";
        private const string CHAR_BANK_CHAR_ID = "char_id";
        private const string CHAR_BANK_SLOT = "slot";
        private const string CHAR_BANK_ITEM_NUM = "itemnum";
        private const string CHAR_BANK_ITEM_VAL = "itemval";
        private const string CHAR_BANK_ITEM_STATS = "itemstats";

        //Char Switches Table Constants
        private const string CHAR_SWITCHES_TABLE = "char_switches";
        private const string CHAR_SWITCH_CHAR_ID = "char_id";
        private const string CHAR_SWITCH_SLOT = "slot";
        private const string CHAR_SWITCH_VAL = "val";

        //Char Variables Table Constants
        private const string CHAR_VARIABLES_TABLE = "char_variables";
        private const string CHAR_VARIABLE_CHAR_ID = "char_id";
        private const string CHAR_VARIABLE_SLOT = "slot";
        private const string CHAR_VARIABLE_VAL = "val";

        //Char Quests Table Constants
        private const string CHAR_QUESTS_TABLE = "char_quests";
        private const string CHAR_QUEST_CHAR_ID = "char_id";
        private const string CHAR_QUEST_ID = "quest_id";
        private const string CHAR_QUEST_TASK = "task";
        private const string CHAR_QUEST_TASK_PROGRESS = "task_progress";
        private const string CHAR_QUEST_COMPLETED = "completed";

        //GameObject Table Constants
        private const string GAME_OBJECT_ID = "id";
        private const string GAME_OBJECT_DELETED = "deleted";
        private const string GAME_OBJECT_DATA = "data";

        //Map List Table Constants
        private const string MAP_LIST_TABLE = "map_list";
        private const string MAP_LIST_DATA = "data";

        public static object MapGridLock = new Object();
        public static List<MapGrid> MapGrids = new List<MapGrid>();

        //Check Directories
        public static void CheckDirectories()
        {
            if (!Directory.Exists("resources")) { Directory.CreateDirectory("resources"); }
        }

        //Database setup, version checking
        public static bool InitDatabase()
        {
            lock (_dbLock)
            {
                if (!File.Exists(DbFilename)) CreateDatabase();
                if (_dbConnection == null)
                {
                    _dbConnection = new SqliteConnection("Data Source=" + DbFilename + ",Version=3");
                    _dbConnection.Open();
                }
                if (GetDatabaseVersion() != DbVersion)
                {
                    Console.WriteLine("Database is out of date! Version: " + GetDatabaseVersion() +
                                      " Expected Version: " + DbVersion + ". Please run the included migration tool!");
                    return false;
                }
                LoadAllGameObjects();
                return true;
            }
        }
        private static long GetDatabaseVersion()
        {
            long version = -1;
            var cmd = "SELECT " + DB_VERSION + " from " + INFO_TABLE + ";";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                version = (long)createCommand.ExecuteScalar();
            }
            return version;
        }
        private static void CreateDatabase()
        {
            _dbConnection = new SqliteConnection("Data Source=" + DbFilename + ",Version=3,New=True");
            _dbConnection.Open();
            CreateInfoTable();
            CreateUsersTable();
            CreateCharactersTable();
            CreateCharacterInventoryTable();
            CreateCharacterSpellsTable();
            CreateCharacterHotbarTable();
            CreateCharacterBankTable();
            CreateCharacterSwitchesTable();
            CreateCharacterVariablesTable();
            CreateCharacterQuestsTable();
            CreateGameObjectTables();
            CreateMapListTable();
            CreateBansTable();
            CreateMutesTable();
            CreateLogsTable();
        }
        private static void CreateInfoTable()
        {
            var cmd = "CREATE TABLE " + INFO_TABLE + " (" + DB_VERSION + " INTEGER NOT NULL);";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
            cmd = "INSERT into " + INFO_TABLE + " (" + DB_VERSION + ") VALUES (" + DbVersion + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateLogsTable()
        {
            var cmd = "CREATE TABLE " + LOG_TABLE + " ("
                        + LOG_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                        + LOG_TIME + " TEXT,"
                        + LOG_TYPE + " TEXT,"
                        + LOG_INFO + " TEXT"
                        + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateMutesTable()
        {
            var cmd = "CREATE TABLE " + MUTE_TABLE + " ("
                + MUTE_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + MUTE_TIME + " TEXT,"
                + MUTE_USER + " INTEGER,"
                + MUTE_IP + " TEXT,"
                + MUTE_DURATION + " INTEGER,"
                + MUTE_REASON + " TEXT,"
                + MUTE_MUTER + " INTEGER"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateBansTable()
        {
            var cmd = "CREATE TABLE " + BAN_TABLE + " ("
                + BAN_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + BAN_TIME + " TEXT,"
                + BAN_USER + " INTEGER,"
                + BAN_IP + " TEXT,"
                + BAN_DURATION + " INTEGER,"
                + BAN_REASON + " TEXT,"
                + BAN_BANNER + " INTEGER"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateUsersTable()
        {
            var cmd = "CREATE TABLE " + USERS_TABLE + " ("
                + USER_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + USER_NAME + " TEXT,"
                + USER_PASS + " TEXT,"
                + USER_SALT + " TEXT,"
                + USER_EMAIL + " TEXT,"
                + USER_POWER + " INTEGER"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharactersTable()
        {
            var cmd = "CREATE TABLE " + CHAR_TABLE + " ("
                + CHAR_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + CHAR_USER_ID + " INTEGER,"
                + CHAR_NAME + " TEXT,"
                + CHAR_MAP + " INTEGER,"
                + CHAR_X + " INTEGER,"
                + CHAR_Y + " INTEGER,"
                + CHAR_Z + " INTEGER,"
                + CHAR_DIR + " INTEGER,"
                + CHAR_SPRITE + " TEXT,"
                + CHAR_FACE + " TEXT,"
                + CHAR_CLASS + " INTEGER,"
                + CHAR_GENDER + " INTEGER,"
                + CHAR_LEVEL + " INTEGER,"
                + CHAR_EXP + " INTEGER,"
                + CHAR_VITALS + " TEXT,"
                + CHAR_MAX_VITALS + " TEXT,"
                + CHAR_STATS + " TEXT,"
                + CHAR_STAT_POINTS + " INTEGER,"
                + CHAR_EQUIPMENT + " TEXT"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterInventoryTable()
        {
            var cmd = "CREATE TABLE " + CHAR_INV_TABLE + " ("
                + CHAR_INV_CHAR_ID + " INTEGER,"
                + CHAR_INV_SLOT + " INTEGER,"
                + CHAR_INV_ITEM_NUM + " INTEGER,"
                + CHAR_INV_ITEM_VAL + " INTEGER,"
                + CHAR_INV_ITEM_STATS + " TEXT,"
                + " unique(`" + CHAR_INV_CHAR_ID + "`,`" + CHAR_INV_SLOT + "`)"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterSpellsTable()
        {
            var cmd = "CREATE TABLE " + CHAR_SPELL_TABLE + " ("
                + CHAR_SPELL_CHAR_ID + " INTEGER,"
                + CHAR_SPELL_SLOT + " INTEGER,"
                + CHAR_SPELL_NUM + " INTEGER,"
                + CHAR_SPELL_CD + " INTEGER,"
                + " unique('" + CHAR_SPELL_CHAR_ID + "','" + CHAR_SPELL_SLOT + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterHotbarTable()
        {
            var cmd = "CREATE TABLE " + CHAR_HOTBAR_TABLE + " ("
                + CHAR_HOTBAR_CHAR_ID + " INTEGER,"
                + CHAR_HOTBAR_SLOT + " INTEGER,"
                + CHAR_HOTBAR_TYPE + " INTEGER,"
                + CHAR_HOTBAR_ITEMSLOT + " INTEGER,"
                + " unique('" + CHAR_HOTBAR_CHAR_ID + "','" + CHAR_HOTBAR_SLOT + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterBankTable()
        {
            var cmd = "CREATE TABLE " + CHAR_BANK_TABLE + " ("
                + CHAR_BANK_CHAR_ID + " INTEGER,"
                + CHAR_BANK_SLOT + " INTEGER,"
                + CHAR_BANK_ITEM_NUM + " INTEGER,"
                + CHAR_BANK_ITEM_VAL + " INTEGER,"
                + CHAR_BANK_ITEM_STATS + " TEXT,"
                + " unique('" + CHAR_BANK_CHAR_ID + "','" + CHAR_BANK_SLOT + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterSwitchesTable()
        {
            var cmd = "CREATE TABLE " + CHAR_SWITCHES_TABLE + " ("
                + CHAR_SWITCH_CHAR_ID + " INTEGER,"
                + CHAR_SWITCH_SLOT + " INTEGER,"
                + CHAR_SWITCH_VAL + " INTEGER,"
                + " unique('" + CHAR_SWITCH_CHAR_ID + "','" + CHAR_SWITCH_SLOT + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterVariablesTable()
        {
            var cmd = "CREATE TABLE " + CHAR_VARIABLES_TABLE + " ("
                + CHAR_VARIABLE_CHAR_ID + " INTEGER,"
                + CHAR_VARIABLE_SLOT + " INTEGER,"
                + CHAR_VARIABLE_VAL + " INTEGER,"
                + " unique('" + CHAR_VARIABLE_CHAR_ID + "','" + CHAR_VARIABLE_SLOT + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateCharacterQuestsTable()
        {
            var cmd = "CREATE TABLE " + CHAR_QUESTS_TABLE + " ("
                + CHAR_QUEST_CHAR_ID + " INTEGER,"
                + CHAR_QUEST_ID + " INTEGER,"
                + CHAR_QUEST_TASK + " INTEGER,"
                + CHAR_QUEST_TASK_PROGRESS + " INTEGER,"
                + CHAR_QUEST_COMPLETED + " INTEGER,"
                + " unique('" + CHAR_QUEST_CHAR_ID + "','" + CHAR_QUEST_ID + "')"
                + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateGameObjectTables()
        {
            foreach (var val in Enum.GetValues(typeof(GameObject)))
            {
                CreateGameObjectTable((GameObject)val);
            }
        }
        private static void CreateGameObjectTable(GameObject gameObject)
        {
            var cmd = "CREATE TABLE " + GetGameObjectTable(gameObject) + " ("
                + GAME_OBJECT_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + GAME_OBJECT_DELETED + " INTEGER NOT NULL DEFAULT 0,"
                + GAME_OBJECT_DATA + " BLOB" + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }
        private static void CreateMapListTable()
        {
            var cmd = "CREATE TABLE " + MAP_LIST_TABLE + " (" + MAP_LIST_DATA + " BLOB);";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
            InsertMapList();
        }
        private static void InsertMapList()
        {
            var cmd = "INSERT into " + MAP_LIST_TABLE + " (" + MAP_LIST_DATA + ") VALUES (" + "NULL" + ");";
            using (var createCommand = _dbConnection.CreateCommand())
            {
                createCommand.CommandText = cmd;
                createCommand.ExecuteNonQuery();
            }
        }

        //Get Last Insert Row Id
        private static long GetLastInsertRowId()
        {
            if (_dbConnection == null || _dbConnection.State == ConnectionState.Closed) return -1;
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "SELECT last_insert_rowid()";
            return (long)cmd.ExecuteScalar();
        }

        //Players General
        public static void LoadPlayerDatabase()
        {
            Console.WriteLine("Using SQLite database for account and data storage.");
        }
        public static Client GetPlayerClient(string username)
        {
                //Try to fetch a player entity by username, online or offline.
                //Check Online First
                for (int i = 0; i < Globals.Clients.Count; i++)
                {
                    if (Globals.Clients[i] != null && Globals.Clients[i].IsConnected() &&
                        Globals.Clients[i].Entity != null)
                    {
                        if (Globals.Clients[i].MyAccount == username)
                        {
                            return Globals.Clients[i];
                        }
                    }
                }

                //Didn't find the player online, lets load him from our database.
                Client fakeClient = new Client(-1, -1, null);
                Player en = new Player(-1, fakeClient);
                fakeClient.Entity = en;
                fakeClient.MyAccount = username;
                LoadUser(fakeClient);
                LoadCharacter(fakeClient);
                return fakeClient;
        }
        public static void SetPlayerPower(string username, int power)
        {
            if (AccountExists(username))
            {
                Client client = GetPlayerClient(username);
                client.Power = power;
                SaveUser(client);
                if (client.ClientIndex > -1)
                {
                    PacketSender.SendPlayerMsg(client, "Your power has been modified!");
                }
                Console.WriteLine(username + "'s power has been set to " + power + "!");
            }
            else
            {
                Console.WriteLine("Account does not exist!");
            }
        }

        //User Info
        public static bool AccountExists(string accountname)
        {
            lock (_dbLock)
            {
                long count = -1;
                var query = "SELECT COUNT(*)" + " from " + USERS_TABLE + " WHERE LOWER(" + USER_NAME + ")=@" + USER_NAME +
                            ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_NAME, accountname.ToLower().Trim()));
                    count = (long) cmd.ExecuteScalar();
                }
                return (count > 0);
            }
        }
        public static bool EmailInUse(string email)
        {
            lock (_dbLock)
            {
                long count = -1;
                var query = "SELECT COUNT(*)" + " from " + USERS_TABLE + " WHERE LOWER(" + USER_EMAIL + ")=@" +
                            USER_EMAIL + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_EMAIL, email.ToLower().Trim()));
                    count = (long) cmd.ExecuteScalar();
                }
                return (count > 0);
            }
        }
        public static bool CharacterNameInUse(string name)
        {
            lock (_dbLock)
            {
                long count = -1;
                var query = "SELECT COUNT(*)" + " from " + CHAR_TABLE + " WHERE LOWER(" + CHAR_NAME + ")=@" + CHAR_NAME +
                            ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_NAME, name.ToLower().Trim()));
                    count = (long) cmd.ExecuteScalar();
                }
                return (count > 0);
            }
        }
        public static long GetRegisteredPlayers()
        {
            lock (_dbLock)
            {
                long count = -1;
                var cmd = "SELECT COUNT(*)" + " from " + USERS_TABLE + ";";
                using (var createCommand = _dbConnection.CreateCommand())
                {
                    createCommand.CommandText = cmd;
                    count = (long) createCommand.ExecuteScalar();
                }
                return count;
            }
        }
        public static void CreateAccount(Client client, string username, string password, string email)
        {
            var sha = new SHA256Managed();
            client.MyAccount = username;

            //Generate a Salt
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[20];
            rng.GetBytes(buff);
            client.MySalt = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Convert.ToBase64String(buff)))).Replace("-", "");

            //Hash the Password
            client.MyPassword = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password + client.MySalt))).Replace("-", "");

            client.MyEmail = email;

            if (GetRegisteredPlayers() == 0)
            {
                client.Power = 2;
            }
            client.MyId = SaveUser(client, true);
        }
        private static long SaveUser(Client client, bool newUser = false)
        {
            lock (_dbLock)
            {
                if (client == null) return -1;
                var insertQuery = "INSERT into " + USERS_TABLE + " (" + USER_NAME + "," + USER_EMAIL + "," + USER_PASS +
                                  "," + USER_SALT + "," + USER_POWER + ")" + "VALUES (@" + USER_NAME + ",@" + USER_EMAIL +
                                  ",@" + USER_PASS + ",@" + USER_SALT + ",@" + USER_POWER + ");";
                var updateQuery = "UPDATE " + USERS_TABLE + " SET " + USER_NAME + "=@" + USER_NAME + "," + USER_EMAIL +
                                  "=@" + USER_EMAIL + "," + USER_PASS + "=@" + USER_PASS + "," + USER_SALT + "=@" +
                                  USER_SALT + "," + USER_POWER + "=@" + USER_POWER + " WHERE " + USER_ID + "=@" +
                                  USER_ID + ";";
                using (SqliteCommand cmd = new SqliteCommand(newUser ? insertQuery : updateQuery, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_NAME, client.MyAccount));
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_EMAIL, client.MyEmail));
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_PASS, client.MyPassword));
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_SALT, client.MySalt));
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_POWER, client.Power));
                    if (!newUser) cmd.Parameters.Add(new SqliteParameter("@" + USER_ID, client.MyId));
                    cmd.ExecuteNonQuery();
                }
                return (GetLastInsertRowId());
            }
        }
        public static bool CheckPassword(string username, string password)
        {
            lock (_dbLock)
            {
                var sha = new SHA256Managed();
                var query = "SELECT " + USER_SALT + "," + USER_PASS + " from " + USERS_TABLE + " WHERE LOWER(" +
                            USER_NAME + ")=@" + USER_NAME + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_NAME, username.ToLower().Trim()));
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows && dataReader.Read())
                    {
                        string pass = dataReader[USER_PASS].ToString();
                        string salt = dataReader[USER_SALT].ToString();
                        string temppass =
                            BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt)))
                                .Replace("-", "");
                        if (temppass == pass)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public static long CheckPower(string username)
        {
            lock (_dbLock)
            {
                long power = 0;
                var query = "SELECT " + USER_POWER + " from " + USERS_TABLE + " WHERE LOWER(" + USER_NAME + ")=@" +
                            USER_NAME + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_NAME, username.ToLower().Trim()));
                    power = (long) cmd.ExecuteScalar();
                }
                return power;
            }
        }
        public static bool LoadUser(Client client)
        {
            lock (_dbLock)
            {
                var query = "SELECT * from " + USERS_TABLE + " WHERE LOWER(" + USER_NAME + ")=@" + USER_NAME + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + USER_NAME, client.MyAccount.ToLower().Trim()));
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows && dataReader.Read())
                    {
                        client.MyAccount = dataReader[USER_NAME].ToString();
                        client.MyPassword = dataReader[USER_PASS].ToString();
                        client.MySalt = dataReader[USER_SALT].ToString();
                        client.MyEmail = dataReader[USER_EMAIL].ToString();
                        client.Power = Convert.ToInt32(dataReader[USER_POWER]);
                        client.MyId = Convert.ToInt32(dataReader[USER_ID]);
                        return true;
                    }
                }
                return false;
            }
        }

        //Character Saving/Loading
        public static long SaveCharacter(Player player, bool newCharacter = false)
        {
            lock (_dbLock)
            {
                if (player == null)
                {
                    return -1;
                }
                if (player.MyClient.MyAccount == "") return -1;
                if (!newCharacter && player.MyId == -1) return -1;
                var insertQuery = "INSERT into " + CHAR_TABLE + " (" + CHAR_USER_ID + "," + CHAR_NAME + "," + CHAR_MAP +
                                  "," + CHAR_X + "," + CHAR_Y + "," + CHAR_Z + "," + CHAR_DIR + "," + CHAR_SPRITE + "," +
                                  CHAR_FACE + "," + CHAR_CLASS + "," + CHAR_GENDER + "," + CHAR_LEVEL + "," + CHAR_EXP +
                                  "," + CHAR_VITALS + "," + CHAR_MAX_VITALS + "," + CHAR_STATS + "," + CHAR_STAT_POINTS +
                                  "," + CHAR_EQUIPMENT + ")" + " VALUES (@" + CHAR_USER_ID + ",@" + CHAR_NAME + ",@" +
                                  CHAR_MAP + ",@" + CHAR_X + ",@" + CHAR_Y + ",@" + CHAR_Z + ",@" + CHAR_DIR + ",@" +
                                  CHAR_SPRITE + ",@" + CHAR_FACE + ",@" + CHAR_CLASS + ",@" + CHAR_GENDER + ",@" +
                                  CHAR_LEVEL + ",@" + CHAR_EXP + ",@" + CHAR_VITALS + ",@" + CHAR_MAX_VITALS + ",@" +
                                  CHAR_STATS + ",@" + CHAR_STAT_POINTS + ",@" + CHAR_EQUIPMENT + ");";

                var updateQuery = "UPDATE " + CHAR_TABLE + " SET " + CHAR_USER_ID + "=@" + CHAR_USER_ID + "," +
                                  CHAR_NAME + "=@" + CHAR_NAME + "," + CHAR_MAP + "=@" + CHAR_MAP + "," + CHAR_X + "=@" +
                                  CHAR_X + "," + CHAR_Y + "=@" + CHAR_Y + "," + CHAR_Z + "=@" + CHAR_Z + "," + CHAR_DIR +
                                  "=@" + CHAR_DIR + "," + CHAR_SPRITE + "=@" + CHAR_SPRITE + "," + CHAR_FACE + "=@" +
                                  CHAR_FACE + "," + CHAR_CLASS + "=@" + CHAR_CLASS + "," + CHAR_GENDER + "=@" +
                                  CHAR_GENDER + "," + CHAR_LEVEL + "=@" + CHAR_LEVEL + "," + CHAR_EXP + "=@" + CHAR_EXP +
                                  "," + CHAR_VITALS + "=@" + CHAR_VITALS + "," + CHAR_MAX_VITALS + "=@" +
                                  CHAR_MAX_VITALS + "," + CHAR_STATS + "=@" + CHAR_STATS + "," + CHAR_STAT_POINTS + "=@" +
                                  CHAR_STAT_POINTS + "," + CHAR_EQUIPMENT + "=@" + CHAR_EQUIPMENT + " WHERE " + CHAR_ID +
                                  "=@" + CHAR_ID + ";";
                using (SqliteCommand cmd = new SqliteCommand(newCharacter ? insertQuery : updateQuery, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_USER_ID, player.MyClient.MyId));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_NAME, player.MyName));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_MAP, player.CurrentMap));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_X, player.CurrentX));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_Y, player.CurrentY));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_Z, player.CurrentZ));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_DIR, player.Dir));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPRITE, player.MySprite));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_FACE, player.Face));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_CLASS, player.Class));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_GENDER, player.Gender));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_LEVEL, player.Level));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_EXP, player.Experience));
                    var vitals = "";
                    for (int i = 0; i < player.Vital.Length; i++)
                    {
                        vitals += player.Vital[i] + ",";
                    }
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_VITALS, vitals));
                    var maxVitals = "";
                    for (int i = 0; i < player.MaxVital.Length; i++)
                    {
                        maxVitals += player.MaxVital[i] + ",";
                    }
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_MAX_VITALS, maxVitals));
                    var stats = "";
                    for (int i = 0; i < player.Stat.Length; i++)
                    {
                        stats += player.Stat[i].Stat + ",";
                    }
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_STATS, stats));
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_STAT_POINTS, player.StatPoints));
                    var equipment = "";
                    for (int i = 0; i < player.Equipment.Length; i++)
                    {
                        equipment += player.Equipment[i] + ",";
                    }
                    cmd.Parameters.Add(new SqliteParameter("@" + CHAR_EQUIPMENT, equipment));
                    if (!newCharacter) cmd.Parameters.Add(new SqliteParameter("@" + CHAR_ID, player.MyId));
                    cmd.ExecuteNonQuery();
                }
                if (newCharacter) player.MyId = GetLastInsertRowId();
                SaveCharacterInventory(player);
                SaveCharacterSpells(player);
                SaveCharacterBank(player);
                SaveCharacterHotbar(player);
                SaveCharacterSwitches(player);
                SaveCharacterVariables(player);
                return (GetLastInsertRowId());
            }
        }
        private static void SaveCharacterInventory(Player player)
        {
            lock (_dbLock)
            {
                for (int i = 0; i < Options.MaxInvItems; i++)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_INV_TABLE + " (" + CHAR_INV_CHAR_ID + "," +
                                CHAR_INV_SLOT + "," + CHAR_INV_ITEM_NUM + "," + CHAR_INV_ITEM_VAL + "," +
                                CHAR_INV_ITEM_STATS + ")" + " VALUES " + " (@" + CHAR_INV_CHAR_ID + ",@" + CHAR_INV_SLOT +
                                ",@" + CHAR_INV_ITEM_NUM + ",@" + CHAR_INV_ITEM_VAL + ",@" + CHAR_INV_ITEM_STATS + ")";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_SLOT, i));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_ITEM_NUM, player.Inventory[i].ItemNum));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_ITEM_VAL, player.Inventory[i].ItemVal));
                        var stats = "";
                        for (int x = 0; x < player.Inventory[i].StatBoost.Length; x++)
                        {
                            stats += player.Inventory[i].StatBoost[x] + ",";
                        }
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_ITEM_STATS, stats));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private static void SaveCharacterSpells(Player player)
        {
            lock (_dbLock)
            {
                for (int i = 0; i < Options.MaxPlayerSkills; i++)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_SPELL_TABLE + " (" + CHAR_SPELL_CHAR_ID + "," +
                                CHAR_SPELL_SLOT + "," + CHAR_SPELL_NUM + "," + CHAR_SPELL_CD + ")" + " VALUES " + " (@" +
                                CHAR_SPELL_CHAR_ID + ",@" + CHAR_SPELL_SLOT + ",@" + CHAR_SPELL_NUM + ",@" +
                                CHAR_SPELL_CD + ");";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPELL_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPELL_SLOT, i));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPELL_NUM, player.Spells[i].SpellNum));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPELL_CD,
                            (player.Spells[i].SpellCD > Environment.TickCount
                                ? Environment.TickCount - player.Spells[i].SpellCD
                                : 0)));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private static void SaveCharacterBank(Player player)
        {
            lock (_dbLock)
            {
                for (int i = 0; i < Options.MaxBankSlots; i++)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_BANK_TABLE + " (" + CHAR_BANK_CHAR_ID + "," +
                                CHAR_BANK_SLOT + "," + CHAR_BANK_ITEM_NUM + "," + CHAR_BANK_ITEM_VAL + "," +
                                CHAR_BANK_ITEM_STATS + ")" + " VALUES " + " (@" + CHAR_BANK_CHAR_ID + ",@" +
                                CHAR_BANK_SLOT + ",@" + CHAR_BANK_ITEM_NUM + ",@" + CHAR_BANK_ITEM_VAL + ",@" +
                                CHAR_BANK_ITEM_STATS + ");";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_SLOT, i));
                        if (player.Bank[i] != null)
                        {
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_NUM, player.Bank[i].ItemNum));
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_VAL, player.Bank[i].ItemVal));
                            var stats = "";
                            for (int x = 0; x < player.Bank[i].StatBoost.Length; x++)
                            {
                                stats += player.Bank[i].StatBoost[x] + ",";
                            }
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_STATS, stats));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_NUM, -1));
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_VAL, -1));
                            var stats = "";
                            for (int x = 0; x < Options.MaxStats; x++)
                            {
                                stats += "-1,";
                            }
                            cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_ITEM_STATS, stats));
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private static void SaveCharacterHotbar(Player player)
        {
            lock (_dbLock)
            {
                for (int i = 0; i < Options.MaxHotbar; i++)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_HOTBAR_TABLE + " (" + CHAR_HOTBAR_CHAR_ID + "," +
                                CHAR_HOTBAR_SLOT + "," + CHAR_HOTBAR_TYPE + "," + CHAR_HOTBAR_ITEMSLOT + ")" +
                                " VALUES " + " (@" + CHAR_HOTBAR_CHAR_ID + ",@" + CHAR_HOTBAR_SLOT + ",@" +
                                CHAR_HOTBAR_TYPE + ",@" + CHAR_HOTBAR_ITEMSLOT + ");";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_HOTBAR_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_HOTBAR_SLOT, i));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_HOTBAR_TYPE, player.Hotbar[i].Type));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_HOTBAR_ITEMSLOT, player.Hotbar[i].Slot));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private static void SaveCharacterSwitches(Player player)
        {
            lock (_dbLock)
            {
                foreach (var playerSwitch in player.Switches)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_SWITCHES_TABLE + " (" + CHAR_SWITCH_CHAR_ID + "," +
                                CHAR_SWITCH_SLOT + "," + CHAR_SWITCH_VAL + ")" + " VALUES " + " (@" +
                                CHAR_SWITCH_CHAR_ID + ",@" + CHAR_SWITCH_SLOT + ",@" + CHAR_SWITCH_VAL + ");";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SWITCH_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SWITCH_SLOT, playerSwitch.Key));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SWITCH_VAL,
                            Convert.ToInt32(playerSwitch.Value)));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private static void SaveCharacterVariables(Player player)
        {
            lock (_dbLock)
            {
                foreach (var playerVariable in player.Variables)
                {
                    var query = "INSERT OR REPLACE into " + CHAR_VARIABLES_TABLE + " (" + CHAR_VARIABLE_CHAR_ID + "," +
                                CHAR_VARIABLE_SLOT + "," + CHAR_VARIABLE_VAL + ")" + " VALUES " + " (@" +
                                CHAR_VARIABLE_CHAR_ID + ",@" + CHAR_VARIABLE_SLOT + ",@" + CHAR_VARIABLE_VAL + ");";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_VARIABLE_CHAR_ID, player.MyId));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_VARIABLE_SLOT, playerVariable.Key));
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_VARIABLE_VAL,
                            Convert.ToInt32(playerVariable.Value)));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        public static bool LoadCharacter(Client client)
        {
            lock (_dbLock)
            {
                var en = client.Entity;
                var commaSep = new char[1];
                commaSep[0] = ',';
                if (client.MyId == -1) return false;
                try
                {
                    var query = "SELECT * from " + CHAR_TABLE + " WHERE " + CHAR_USER_ID + "=@" + CHAR_USER_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_USER_ID, client.MyId));
                        var dataReader = cmd.ExecuteReader();
                        if (dataReader.HasRows && dataReader.Read())
                        {
                            en.MyId = Convert.ToInt32(dataReader[CHAR_ID]);
                            en.MyName = dataReader[CHAR_NAME].ToString();
                            en.CurrentMap = Convert.ToInt32(dataReader[CHAR_MAP]);
                            en.CurrentX = Convert.ToInt32(dataReader[CHAR_X]);
                            en.CurrentY = Convert.ToInt32(dataReader[CHAR_Y]);
                            en.CurrentZ = Convert.ToInt32(dataReader[CHAR_Z]);
                            en.Dir = Convert.ToInt32(dataReader[CHAR_DIR]);
                            en.MySprite = dataReader[CHAR_SPRITE].ToString();
                            en.Face = dataReader[CHAR_FACE].ToString();
                            en.Class = Convert.ToInt32(dataReader[CHAR_CLASS]);
                            en.Gender = Convert.ToInt32(dataReader[CHAR_GENDER]);
                            en.Level = Convert.ToInt32(dataReader[CHAR_LEVEL]);
                            en.Experience = Convert.ToInt32(dataReader[CHAR_EXP]);
                            var vitalString = dataReader[CHAR_VITALS].ToString();
                            var vitals = vitalString.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < (int) Vitals.VitalCount && i < vitals.Length; i++)
                            {
                                en.Vital[i] = Int32.Parse(vitals[i]);
                            }
                            var maxVitalString = dataReader[CHAR_MAX_VITALS].ToString();
                            var maxVitals = maxVitalString.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < (int) Vitals.VitalCount && i < maxVitals.Length; i++)
                            {
                                en.MaxVital[i] = Int32.Parse(maxVitals[i]);
                            }
                            var statsString = dataReader[CHAR_STATS].ToString();
                            var stats = statsString.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < (int) Stats.StatCount && i < stats.Length; i++)
                            {
                                en.Stat[i].Stat = Int32.Parse(stats[i]);
                            }
                            en.StatPoints = Convert.ToInt32(dataReader[CHAR_STAT_POINTS]);
                            var equipmentString = dataReader[CHAR_EQUIPMENT].ToString();
                            var equipment = equipmentString.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < (int) Options.EquipmentSlots.Count && i < equipment.Length; i++)
                            {
                                en.Equipment[i] = Int32.Parse(equipment[i]);
                            }
                            if (!LoadCharacterInventory(en)) return false;
                            if (!LoadCharacterSpells(en)) return false;
                            if (!LoadCharacterBank(en)) return false;
                            if (!LoadCharacterHotbar(en)) return false;
                            if (!LoadCharacterSwitches(en)) return false;
                            if (!LoadCharacterVariables(en)) return false;
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterInventory(Player player)
        {
            lock (_dbLock)
            {
                var commaSep = new char[1];
                commaSep[0] = ',';
                try
                {
                    var query = "SELECT * from " + CHAR_INV_TABLE + " WHERE " + CHAR_INV_CHAR_ID + "=@" +
                                CHAR_INV_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_INV_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var slot = Convert.ToInt32(dataReader[CHAR_INV_SLOT]);
                            if (slot >= 0 && slot < Options.MaxInvItems)
                            {
                                player.Inventory[slot].ItemNum = Convert.ToInt32(dataReader[CHAR_INV_ITEM_NUM]);
                                player.Inventory[slot].ItemVal = Convert.ToInt32(dataReader[CHAR_INV_ITEM_VAL]);
                                var statBoostStr = dataReader[CHAR_INV_ITEM_STATS].ToString();
                                var stats = statBoostStr.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < (int) Stats.StatCount && i < stats.Length; i++)
                                {
                                    player.Inventory[slot].StatBoost[i] = Int32.Parse(stats[i]);
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterSpells(Player player)
        {
            lock (_dbLock)
            {
                try
                {
                    var query = "SELECT * from " + CHAR_SPELL_TABLE + " WHERE " + CHAR_SPELL_CHAR_ID + "=@" +
                                CHAR_SPELL_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SPELL_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var slot = Convert.ToInt32(dataReader[CHAR_SPELL_SLOT]);
                            if (slot >= 0 && slot < Options.MaxPlayerSkills)
                            {
                                player.Spells[slot].SpellNum = Convert.ToInt32(dataReader[CHAR_SPELL_SLOT]);
                                player.Spells[slot].SpellCD = Environment.TickCount +
                                                              Convert.ToInt32(dataReader[CHAR_SPELL_CD]);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterBank(Player player)
        {
            lock (_dbLock)
            {
                var commaSep = new char[1];
                commaSep[0] = ',';
                try
                {
                    var query = "SELECT * from " + CHAR_BANK_TABLE + " WHERE " + CHAR_BANK_CHAR_ID + "=@" +
                                CHAR_BANK_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_BANK_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var slot = Convert.ToInt32(dataReader[CHAR_BANK_SLOT]);
                            if (slot >= 0 && slot < Options.MaxBankSlots)
                            {
                                if (player.Bank[slot] == null) player.Bank[slot] = new ItemInstance();
                                player.Bank[slot].ItemNum = Convert.ToInt32(dataReader[CHAR_BANK_ITEM_NUM]);
                                player.Bank[slot].ItemVal = Convert.ToInt32(dataReader[CHAR_BANK_ITEM_VAL]);
                                var statBoostStr = dataReader[CHAR_BANK_ITEM_STATS].ToString();
                                var stats = statBoostStr.Split(commaSep, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < (int) Stats.StatCount && i < stats.Length; i++)
                                {
                                    player.Bank[slot].StatBoost[i] = Int32.Parse(stats[i]);
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterHotbar(Player player)
        {
            lock (_dbLock)
            {
                try
                {
                    var query = "SELECT * from " + CHAR_HOTBAR_TABLE + " WHERE " + CHAR_HOTBAR_CHAR_ID + "=@" +
                                CHAR_HOTBAR_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_HOTBAR_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var slot = Convert.ToInt32(dataReader[CHAR_HOTBAR_SLOT]);
                            if (slot >= 0 && slot < Options.MaxHotbar)
                            {
                                player.Hotbar[slot].Type = Convert.ToInt32(dataReader[CHAR_HOTBAR_TYPE]);
                                player.Hotbar[slot].Slot = Convert.ToInt32(dataReader[CHAR_HOTBAR_ITEMSLOT]);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterSwitches(Player player)
        {
            lock (_dbLock)
            {
                try
                {
                    var query = "SELECT * from " + CHAR_SWITCHES_TABLE + " WHERE " + CHAR_SWITCH_CHAR_ID + "=@" +
                                CHAR_SWITCH_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_SWITCH_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var id = Convert.ToInt32(dataReader[CHAR_SWITCH_SLOT]);
                            if (player.Switches.ContainsKey(id))
                            {
                                player.Switches[id] = Convert.ToBoolean(Convert.ToInt32(dataReader[CHAR_SWITCH_VAL]));
                            }
                            else
                            {
                                player.Switches.Add(id, Convert.ToBoolean(Convert.ToInt32(dataReader[CHAR_SWITCH_VAL])));
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static bool LoadCharacterVariables(Player player)
        {
            lock (_dbLock)
            {
                try
                {
                    var query = "SELECT * from " + CHAR_VARIABLES_TABLE + " WHERE " + CHAR_VARIABLE_CHAR_ID + "=@" +
                                CHAR_VARIABLE_CHAR_ID + ";";
                    using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + CHAR_VARIABLE_CHAR_ID, player.MyId));
                        var dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            var id = Convert.ToInt32(dataReader[CHAR_VARIABLE_SLOT]);
                            if (player.Variables.ContainsKey(id))
                            {
                                player.Variables[id] = Convert.ToInt32(dataReader[CHAR_VARIABLE_VAL]);
                            }
                            else
                            {
                                player.Variables.Add(id, Convert.ToInt32(dataReader[CHAR_VARIABLE_VAL]));
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        //Game Object Saving/Loading
        private static void LoadAllGameObjects()
        {
            foreach (var val in Enum.GetValues(typeof(GameObject)))
            {
                LoadGameObjects((GameObject)val);
                if ((GameObject)val == GameObject.Map)
                {
                    OnMapsLoaded();
                }
                else if ((GameObject) val == GameObject.Class)
                {
                    OnClassesLoaded();
                }
            }
        }
        private static string GetGameObjectTable(GameObject type)
        {
            var tableName = "";
            switch (type)
            {
                case GameObject.Animation:
                    tableName = AnimationBase.DatabaseTable;
                    break;
                case GameObject.Class:
                    tableName = ClassBase.DatabaseTable;
                    break;
                case GameObject.Item:
                    tableName = ItemBase.DatabaseTable;
                    break;
                case GameObject.Npc:
                    tableName = NpcBase.DatabaseTable;
                    break;
                case GameObject.Projectile:
                    tableName = ProjectileBase.DatabaseTable;
                    break;
                case GameObject.Quest:
                    tableName = QuestBase.DatabaseTable;
                    break;
                case GameObject.Resource:
                    tableName = ResourceBase.DatabaseTable;
                    break;
                case GameObject.Shop:
                    tableName = ShopBase.DatabaseTable;
                    break;
                case GameObject.Spell:
                    tableName = SpellBase.DatabaseTable;
                    break;
                case GameObject.Map:
                    tableName = MapBase.DatabaseTable;
                    break;
                case GameObject.CommonEvent:
                    tableName = EventBase.DatabaseTable;
                    break;
                case GameObject.PlayerSwitch:
                    tableName = PlayerSwitchBase.DatabaseTable;
                    break;
                case GameObject.PlayerVariable:
                    tableName = PlayerVariableBase.DatabaseTable;
                    break;
                case GameObject.ServerSwitch:
                    tableName = ServerSwitchBase.DatabaseTable;
                    break;
                case GameObject.ServerVariable:
                    tableName = ServerVariableBase.DatabaseTable;
                    break;
                case GameObject.Tileset:
                    tableName = TilesetBase.DatabaseTable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return tableName;
        }
        private static void ClearGameObjects(GameObject type)
        {
            switch (type)
            {
                case GameObject.Animation:
                    AnimationBase.ClearObjects();
                    break;
                case GameObject.Class:
                    ClassBase.ClearObjects();
                    break;
                case GameObject.Item:
                    ItemBase.ClearObjects();
                    break;
                case GameObject.Npc:
                    NpcBase.ClearObjects();
                    break;
                case GameObject.Projectile:
                    ProjectileBase.ClearObjects();
                    break;
                case GameObject.Quest:
                    QuestBase.ClearObjects();
                    break;
                case GameObject.Resource:
                    ResourceBase.ClearObjects();
                    break;
                case GameObject.Shop:
                    ShopBase.ClearObjects();
                    break;
                case GameObject.Spell:
                    SpellBase.ClearObjects();
                    break;
                case GameObject.Map:
                    MapBase.ClearObjects();
                    break;
                case GameObject.CommonEvent:
                    EventBase.ClearObjects();
                    break;
                case GameObject.PlayerSwitch:
                    PlayerSwitchBase.ClearObjects();
                    break;
                case GameObject.PlayerVariable:
                    PlayerVariableBase.ClearObjects();
                    break;
                case GameObject.ServerSwitch:
                    ServerSwitchBase.ClearObjects();
                    break;
                case GameObject.ServerVariable:
                    ServerVariableBase.ClearObjects();
                    break;
                case GameObject.Tileset:
                    TilesetBase.ClearObjects();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        private static void LoadGameObject(GameObject type, int index, byte[] data)
        {
            switch (type)
            {
                case GameObject.Animation:
                    var anim = new AnimationBase(index);
                    anim.Load(data);
                    AnimationBase.AddObject(index, anim);
                    break;
                case GameObject.Class:
                    var cls = new ClassBase(index);
                    cls.Load(data);
                    ClassBase.AddObject(index, cls);
                    break;
                case GameObject.Item:
                    var itm = new ItemBase(index);
                    itm.Load(data);
                    ItemBase.AddObject(index, itm);
                    break;
                case GameObject.Npc:
                    var npc = new NpcBase(index);
                    npc.Load(data);
                    NpcBase.AddObject(index, npc);
                    break;
                case GameObject.Projectile:
                    var proj = new ProjectileBase(index);
                    proj.Load(data);
                    ProjectileBase.AddObject(index, proj);
                    break;
                case GameObject.Quest:
                    var qst = new QuestBase(index);
                    qst.Load(data);
                    QuestBase.AddObject(index, qst);
                    break;
                case GameObject.Resource:
                    var res = new ResourceBase(index);
                    res.Load(data);
                    ResourceBase.AddObject(index, res);
                    break;
                case GameObject.Shop:
                    var shp = new ShopBase(index);
                    shp.Load(data);
                    ShopBase.AddObject(index, shp);
                    break;
                case GameObject.Spell:
                    var spl = new SpellBase(index);
                    spl.Load(data);
                    SpellBase.AddObject(index, spl);
                    break;
                case GameObject.Map:
                    var map = new MapInstance(index);
                    MapInstance.AddObject(index, map);
                    map.Load(data);
                    break;
                case GameObject.CommonEvent:
                    var buffer = new ByteBuffer();
                    buffer.WriteBytes(data);
                    var evt = new EventBase(index, buffer, true);
                    EventBase.AddObject(index, evt);
                    buffer.Dispose();
                    break;
                case GameObject.PlayerSwitch:
                    var pswitch = new PlayerSwitchBase(index);
                    pswitch.Load(data);
                    PlayerSwitchBase.AddObject(index, pswitch);
                    break;
                case GameObject.PlayerVariable:
                    var pvar = new PlayerVariableBase(index);
                    pvar.Load(data);
                    PlayerVariableBase.AddObject(index, pvar);
                    break;
                case GameObject.ServerSwitch:
                    var sswitch = new ServerSwitchBase(index);
                    sswitch.Load(data);
                    ServerSwitchBase.AddObject(index, sswitch);
                    break;
                case GameObject.ServerVariable:
                    var svar = new ServerVariableBase(index);
                    svar.Load(data);
                    ServerVariableBase.AddObject(index, svar);
                    break;
                case GameObject.Tileset:
                    var tset = new TilesetBase(index);
                    tset.Load(data);
                    TilesetBase.AddObject(index, tset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static void LoadGameObjects(GameObject type)
        {
            var nullIssues = "";
            lock (_dbLock)
            {
                var tableName = GetGameObjectTable(type);
                ClearGameObjects(type);
                var query = "SELECT * from " + tableName + " WHERE " + GAME_OBJECT_DELETED + "=@" + GAME_OBJECT_DELETED +
                            ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_DELETED, 0.ToString()));
                    var dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        var index = Convert.ToInt32(dataReader[GAME_OBJECT_ID]);
                        if (dataReader[MAP_LIST_DATA].GetType() != typeof (System.DBNull))
                        {
                            LoadGameObject(type, index, (byte[]) dataReader[GAME_OBJECT_DATA]);
                        }
                        else
                        {
                            nullIssues += "Tried to load null value for index " + index + " of " + tableName + Environment.NewLine;
                        }
                    }
                }
            }
            if (nullIssues != "")
            {
                throw (new Exception("Tried to load one or more null game objects!" + Environment.NewLine + nullIssues));
            }
        }
        public static void SaveGameObject(DatabaseObject gameObject)
        {
            lock (_dbLock)
            {
                var insertQuery = "UPDATE " + gameObject.GetTable() + " set " + GAME_OBJECT_DELETED + "=@" +
                                  GAME_OBJECT_DELETED + "," + GAME_OBJECT_DATA + "=@" + GAME_OBJECT_DATA + " WHERE " +
                                  GAME_OBJECT_ID + "=@" + GAME_OBJECT_ID + ";";
                using (SqliteCommand cmd = new SqliteCommand(insertQuery, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_ID, gameObject.GetId()));
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_DELETED, 0.ToString()));
                    if (gameObject != null && gameObject.GetData() != null)
                    {
                        cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_DATA, gameObject.GetData()));
                    }
                    else
                    {
                        throw (new Exception("Tried to save a null game object (should be deleted instead?) Table: " +
                                             gameObject.GetTable() + " Id: " + gameObject.GetId()));
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static DatabaseObject AddGameObject(GameObject type)
        {
            lock (_dbLock)
            {
                var insertQuery = "INSERT into " + GetGameObjectTable(type) + " DEFAULT VALUES" + ";";
                int index = -1;
                using (SqliteCommand cmd = new SqliteCommand(insertQuery, _dbConnection))
                {
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    index = (int) ((long) cmd.ExecuteScalar());
                }
                if (index > -1)
                {
                    DatabaseObject obj = null;
                    switch (type)
                    {
                        case GameObject.Animation:
                            obj = new AnimationBase(index);
                            AnimationBase.AddObject(index, obj);
                            break;
                        case GameObject.Class:
                            obj = new ClassBase(index);
                            ClassBase.AddObject(index, obj);
                            break;
                        case GameObject.Item:
                            obj = new ItemBase(index);
                            ItemBase.AddObject(index, obj);
                            break;
                        case GameObject.Npc:
                            obj = new NpcBase(index);
                            NpcBase.AddObject(index, obj);
                            break;
                        case GameObject.Projectile:
                            obj = new ProjectileBase(index);
                            ProjectileBase.AddObject(index, obj);
                            break;
                        case GameObject.Quest:
                            obj = new QuestBase(index);
                            QuestBase.AddObject(index, obj);
                            break;
                        case GameObject.Resource:
                            obj = new ResourceBase(index);
                            ResourceBase.AddObject(index, obj);
                            break;
                        case GameObject.Shop:
                            obj = new ShopBase(index);
                            ShopBase.AddObject(index, obj);
                            break;
                        case GameObject.Spell:
                            obj = new SpellBase(index);
                            SpellBase.AddObject(index, obj);
                            break;
                        case GameObject.Map:
                            obj = new MapInstance(index);
                            MapInstance.AddObject(index, obj);
                            break;
                        case GameObject.CommonEvent:
                            obj = new EventBase(index, -1, -1, true);
                            EventBase.AddObject(index, obj);
                            break;
                        case GameObject.PlayerSwitch:
                            obj = new PlayerSwitchBase(index);
                            PlayerSwitchBase.AddObject(index, obj);
                            break;
                        case GameObject.PlayerVariable:
                            obj = new PlayerVariableBase(index);
                            PlayerVariableBase.AddObject(index, obj);
                            break;
                        case GameObject.ServerSwitch:
                            obj = new ServerSwitchBase(index);
                            ServerSwitchBase.AddObject(index, obj);
                            break;
                        case GameObject.ServerVariable:
                            obj = new ServerVariableBase(index);
                            ServerVariableBase.AddObject(index, obj);
                            break;
                        case GameObject.Tileset:
                            obj = new TilesetBase(index);
                            TilesetBase.AddObject(index, obj);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                    SaveGameObject(obj);
                    return obj;
                }
                return null;
            }
        }
        public static void DeleteGameObject(DatabaseObject gameObject)
        {
            lock (_dbLock)
            {
                var insertQuery = "UPDATE " + gameObject.GetTable() + " set " + GAME_OBJECT_DELETED + "=@" +
                                  GAME_OBJECT_DELETED + " WHERE " +
                                  GAME_OBJECT_ID + "=@" + GAME_OBJECT_ID + ";";
                using (SqliteCommand cmd = new SqliteCommand(insertQuery, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_ID, gameObject.GetId()));
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_DELETED, 1.ToString()));
                    cmd.Parameters.Add(new SqliteParameter("@" + GAME_OBJECT_DATA, gameObject.GetData()));
                    cmd.ExecuteNonQuery();
                }
                gameObject.Delete();
            }
        }

        //Post Loading Functions
        private static void OnMapsLoaded()
        {
            if (MapBase.ObjectCount() == 0)
            {
                Console.WriteLine("No maps found! - Creating an empty map!");
                AddGameObject(GameObject.Map);
            }

            GenerateMapGrids();
            LoadMapFolders();
            CheckAllMapConnections();
        }
        private static void OnClassesLoaded()
        {
            if (ClassBase.ObjectCount() == 0)
            {
                Console.WriteLine("No classes found! - Creating a default class!");
                var cls = (ClassBase)AddGameObject(GameObject.Class);
                cls.Name = "Default";
                ClassSprite defaultMale = new ClassSprite();
                defaultMale.Sprite = "1.png";
                defaultMale.Gender = 0;
                ClassSprite defaultFemale = new ClassSprite();
                defaultFemale.Sprite = "2.png";
                defaultFemale.Gender = 1;
                cls.Sprites.Add(defaultMale);
                cls.Sprites.Add(defaultFemale);
                for (int i = 0; i < (int)Vitals.VitalCount; i++)
                {
                    cls.BaseVital[i] = 20;
                }
                for (int i = 0; i < (int)Stats.StatCount; i++)
                {
                    cls.BaseStat[i] = 20;
                }
                SaveGameObject(cls);
            }
        }

        //Extra Map Helper Functions
        public static void CheckAllMapConnections()
        {
            foreach (var map in MapInstance.GetObjects())
            {
                CheckMapConnections(map.Value);
            }
        }
        public static void CheckMapConnections(MapBase map)
        {
            bool updated = false;
            if (!MapInstance.GetObjects().ContainsKey(map.Up))
            {
                map.Up = -1;
                updated = true;
            }
            if (!MapInstance.GetObjects().ContainsKey(map.Down))
            {
                map.Down = -1;
                updated = true;
            }
            if (!MapInstance.GetObjects().ContainsKey(map.Left))
            {
                map.Left = -1;
                updated = true;
            }
            if (!MapInstance.GetObjects().ContainsKey(map.Right))
            {
                map.Right = -1;
                updated = true;
            }
            if (updated)
            {
                SaveGameObject(map);
                PacketSender.SendMapToEditors(map.MyMapNum);
            }
        }
        public static void GenerateMapGrids()
        {
            lock (MapGridLock)
            {
                MapGrids.Clear();
                foreach (var map in MapInstance.GetObjects())
                {
                    if (MapGrids.Count == 0)
                    {
                        MapGrids.Add(new MapGrid(map.Value.MyMapNum, 0));
                    }
                    else
                    {
                        for (var y = 0; y < MapGrids.Count; y++)
                        {
                            if (!MapGrids[y].HasMap(map.Value.MyMapNum))
                            {
                                if (y != MapGrids.Count - 1) continue;
                                MapGrids.Add(new MapGrid(map.Value.MyMapNum, MapGrids.Count));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                foreach (var map in MapInstance.GetObjects())
                {
                    map.Value.SurroundingMaps.Clear();
                    var myGrid = map.Value.MapGrid;
                    for (var x = map.Value.MapGridX - 1; x <= map.Value.MapGridX + 1; x++)
                    {
                        for (var y = map.Value.MapGridY - 1; y <= map.Value.MapGridY + 1; y++)
                        {
                            if ((x == map.Value.MapGridX) && (y == map.Value.MapGridY))
                                continue;
                            if (x >= MapGrids[myGrid].XMin && x < MapGrids[myGrid].XMax && y >= MapGrids[myGrid].YMin && y < MapGrids[myGrid].YMax && MapGrids[myGrid].MyGrid[x, y] > -1)
                            {
                                map.Value.SurroundingMaps.Add(MapGrids[myGrid].MyGrid[x, y]);
                            }
                        }
                    }
                }
            }
        }

        //Map Folders
        private static void LoadMapFolders()
        {
            lock (_dbLock)
            {
                var query = "SELECT * from " + MAP_LIST_TABLE + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            if (dataReader[MAP_LIST_DATA].GetType() != typeof (System.DBNull))
                            {
                                var data = (byte[]) dataReader[MAP_LIST_DATA];
                                ByteBuffer myBuffer = new ByteBuffer();
                                myBuffer.WriteBytes(data);
                                MapList.GetList().Load(myBuffer, MapBase.GetObjects(), true, true);
                            }
                        }
                    }
                    else
                    {
                        InsertMapList();
                    }
                }
                foreach (var map in MapBase.GetObjects())
                {
                    if (MapList.GetList().FindMap(map.Value.MyMapNum) == null)
                    {
                        MapList.GetList().AddMap(map.Value.MyMapNum, MapBase.GetObjects());
                    }
                }
                SaveMapFolders();
                PacketSender.SendMapListToAll();
            }
        }
        public static void SaveMapFolders()
        {
            lock (_dbLock)
            {
                var query = "UPDATE " + MAP_LIST_TABLE + " set " + MAP_LIST_DATA + "=@" + MAP_LIST_DATA + ";";
                using (SqliteCommand cmd = new SqliteCommand(query, _dbConnection))
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + MAP_LIST_DATA,
                        MapList.GetList().Data(MapBase.GetObjects())));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

