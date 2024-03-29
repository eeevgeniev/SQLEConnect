﻿using BenchmarkDotNet.Running;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using SQLEConnect;
using SQLEConnectTests.Benchmarks.Models;
using SQLEConnectTests.Benchmarks.Parsers;
using SQLEConnectTests.Benchmarks.Tests;
using SQLEConnectTests.SettingParser;
using SQLEConnectTests.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SQLEConnectTests.Benchmarks
{
    internal class Program
    {
        private const string SETTING_NAME = "settings.json";
        private readonly static List<string> files = new List<string>()
        {
            "test-data-1.json",
            "test-data-2.json",
            "test-data-3.json",
            "test-data-4.json",
            "test-data-5.json",
            "test-data-6.json",
            "test-data-7.json",
            "test-data-8.json",
            "test-data-9.json",
            "test-data-10.json",
            "test-data-11.json",
            "test-data-12.json",
            "test-data-13.json",
            "test-data-14.json",
            "test-data-15.json",
            "test-data-16.json",
            "test-data-17.json",
            "test-data-18.json",
            "test-data-19.json",
            "test-data-20.json",
        };

        static void Main(string[] args)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), SETTING_NAME);
            Parser settingParser = new Parser();
            Setting setting = settingParser.ParserConfiguration(path);

            CreateTableData(setting.ConnectionString);

            if (!TableHasResults(setting.ConnectionString))
            {
                InsertData(setting.ConnectionString);
            }

#if DEBUG
            List<Model> first = QuerySQLEConnect(setting.ConnectionString);
            IEnumerable<Model> second = QueryDapper(setting.ConnectionString);
            List<StructModel> third = QuerySQLEConnectForStruct(setting.ConnectionString);
            List<Dictionary<string, object>> fourth = QuerySQLEConnectWithDictionary(setting.ConnectionString);
            List<Model> fifth = QuerySQLEConnectWithCustomParser(setting.ConnectionString);
            (List<Model> models, List<int> integers, List<string> strings, List<DateTime> dates, List<decimal> decimals, List<long> longs) sixth = QueryMultipleValues(setting.ConnectionString);
            List<dynamic> seventh = QuerySQLEConnectWithDynamic(setting.ConnectionString);

            Debug.Assert(first.Count == second.ToList().Count);

            Debug.Assert(first.SequenceEqual(second));

            Debug.Assert(first.Count == third.Count);

            for (int i = 0; i < first.Count; i++)
            {
                Debug.Assert(first[i].Equals(third[i]));
            }

            Debug.Assert(first.Count == fourth.Count);

            for (int i = 0; i < first.Count; i++)
            {
                Debug.Assert(first[i].Equals(fourth[i]));
            }

            Debug.Assert(first.SequenceEqual(fifth));

            Debug.Assert(first.SequenceEqual(sixth.models));

            for (int i = 0; i < sixth.models.Count; i++)
            {
                Debug.Assert(sixth.models[i].Id == sixth.integers[i]);
                Debug.Assert(sixth.models[i].Strp == sixth.strings[i]);
                Debug.Assert(sixth.models[i].Date == sixth.dates[i]);
                Debug.Assert(sixth.models[i].Dcml == sixth.decimals[i]);
                Debug.Assert(sixth.models[i].Lng == sixth.longs[i]);
            }

            Debug.Assert(first.Count == seventh.Count);

            for(int i = 0; i < first.Count; i++)
            {
                first[i].Equals(seventh[i]);
            }
#endif

#if RELEASE
            BenchmarkRunner.Run<QueryWrapperBenchmark>();
#endif
        }

        private static List<Model> QuerySQLEConnect(string connectionString)
        {
            List<Model> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                models = connection.Query<Model>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(models?.Count);

            return models;
        }

        private static List<StructModel> QuerySQLEConnectForStruct(string connectionString)
        {
            List<StructModel> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                models = connection.Query<StructModel>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(models?.Count);

            return models;
        }

        private static List<Dictionary<string, object>> QuerySQLEConnectWithDictionary(string connectionString)
        {
            List<Dictionary<string, object>> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                models = connection.Query<Dictionary<string, object>>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(models?.Count);

            return models;
        }

        private static IEnumerable<Model> QueryDapper(string connectionString)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                return connection.Query<Model>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }
        }

        private static List<Model> QuerySQLEConnectWithCustomParser(string connectionString)
        {
            List<Model> models = null;

            Connection<NpgsqlConnection>.AddOrUpdateParser(new ModelParser());

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                models = connection.Query<Model>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                connection.ClearParsers();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(models?.Count);

            return models;
        }

        private static List<dynamic> QuerySQLEConnectWithDynamic(string connectionString)
        {
            List<dynamic> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                models = connection.Query<dynamic>("SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(models?.Count);

            return models;
        }

        private static (List<Model> models, List<int> integers, List<string> strings, List<DateTime> dates, List<decimal> decimals, List<long> longs) QueryMultipleValues(string connectionString)
        {
            (List<Model> models, List<int> integers, List<string> strings, List<DateTime> times, List<decimal> decimals, List<long> longs) result;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                result = connection.QueryMultiple<Model, int, string, DateTime, decimal, long>(@"SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011; 
                    SELECT id FROM models ORDER BY id LIMIT 98989 OFFSET 1011; 
                    SELECT strp FROM models ORDER BY id LIMIT 98989 OFFSET 1011;
                    SELECT date FROM models ORDER BY id LIMIT 98989 OFFSET 1011;
                    SELECT dcml FROM models ORDER BY id LIMIT 98989 OFFSET 1011;
                    SELECT lng FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

            return result;
        }

        private static void CreateTableData(string connectionString)
        {
            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
            {
                connection.NonQuery(@"CREATE TABLE IF NOT EXISTS models
                                    (
                                        id INTEGER GENERATED ALWAYS AS IDENTITY,
                                        strp TEXT,
                                        chr TEXT,
                                        character TEXT,
                                        date TIMESTAMPTZ,
                                        ndate TIMESTAMPTZ,
                                        strs TEXT,
                                        strw TEXT,
                                        dcml NUMERIC(14, 4),
                                        lng BIGINT,
                                        PRIMARY KEY (id)
                                    );", null);
            }
        }

        private static void InsertData(string connectionString)
        {
            string path;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString, true))
            {
                foreach (string file in files)
                {
                    path = Path.Combine(Directory.GetCurrentDirectory(), file);

                    if (!File.Exists(path))
                    {
                        throw new Exception($"File not found - {path}");
                    }

                    List<Model> models = JsonConvert.DeserializeObject<List<Model>>(File.ReadAllText(path));

                    Random random = new Random();

                    foreach (Model model in models)
                    {
                        model.Dcml = (random.Next(0, 1000000) * 0.1M);
                        model.Lng = random.Next(0, 1000000);

                        connection.Query(@"INSERT INTO models
                            (strp, chr, character, date, ndate, strs, strw, dcml, lng)
                        VALUES
                            (@strp, @chr, @character, @date, @ndate, @strs, @strw, @dcml, @lng)",
                        new List<SqlEParameter>()
                        {
                            new SqlEParameter("@strp", model.Strp),
                            new SqlEParameter("@chr", model.Chr),
                            new SqlEParameter("@character", model.Character),
                            new SqlEParameter("@date", model.Date),
                            new SqlEParameter("@ndate", model.NDate),
                            new SqlEParameter("@strs", model.Strs),
                            new SqlEParameter("@strw", model.Strw),
                            new SqlEParameter("@dcml", model.Dcml),
                            new SqlEParameter("@lng", model.Lng)
                        });
                    }
                }

                connection.CommitTransaction();
            }
        }

        private static bool TableHasResults(string connectionString)
        {
            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString, true))
            {
                (bool hasResult, bool areThereRecords) = connection.Single<bool>(@"SELECT EXISTS (SELECT id FROM models);", null);

                return areThereRecords;
            }
        }
    }
}
