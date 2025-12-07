using BenchmarkDotNet.Attributes;
using Npgsql;
using SQLEConnect;
using SQLEConnectTests.Benchmarks.Models;
using SQLEConnectTests.SettingParser;
using SQLEConnectTests.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;

namespace SQLEConnectTests.Benchmarks.Tests
{
    [MemoryDiagnoser(true)]
    public class QueryWrapperBenchmark
    {
        private const string SETTING_NAME = "settings.json";
        private const string SMALLLISTQUERY = "SELECT * FROM models ORDER BY id LIMIT 10 OFFSET 1011;";
        private const string LARGELISTQUERY = "SELECT * FROM models ORDER BY id LIMIT 98989 OFFSET 1011;";
        private const string SINGLEQUERY = "SELECT * FROM models ORDER BY id LIMIT 1 OFFSET 52333;";

        private readonly Parser _parser;
        private readonly Setting _setting;

        public QueryWrapperBenchmark()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), SETTING_NAME);

            this._parser = new Parser();
            this._setting = this._parser.ParserConfiguration(path);
        }
        
        [Benchmark]
        public List<Model> SmallQuerySQLEConnectEntity()
        {
            List<Model> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Model>(new EntityDescriptor<Model>(), SMALLLISTQUERY, null, false, false);
            }

            return models;
        }
        
        [Benchmark]
        public List<Model> LargeQuerySQLEConnectEntity()
        {
            List<Model> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Model>(new EntityDescriptor<Model>(), LARGELISTQUERY, null, false, false);
            }

            return models;
        }

        [Benchmark]
        public List<Model> SmallQuerySQLEConnect()
        {
            List<Model> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Model>(SMALLLISTQUERY, null);
            }

            return models;
        }

        [Benchmark]
        public List<Model> LargeQuerySQLEConnect()
        {
            List<Model> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Model>(LARGELISTQUERY, null);
            }

            return models;
        }

        //[Benchmark]
        public List<Dictionary<string, object>> SmallQuerySQLEConnectDict()
        {
            List<Dictionary<string, object>> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Dictionary<string, object>>(SMALLLISTQUERY, null);
            }

            return models;
        }

        //[Benchmark]
        public List<Dictionary<string, object>> LargeQuerySQLEConnectDict()
        {
            List<Dictionary<string, object>> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<Dictionary<string, object>>(LARGELISTQUERY, null);
            }

            return models;
        }

        //[Benchmark]
        public List<dynamic> SmallQuerySQLEConnectDynamic()
        {
            List<dynamic> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<dynamic>(SMALLLISTQUERY, null);
            }

            return models;
        }

        //[Benchmark]
        public List<dynamic> LargeQuerySQLEConnectDynamic()
        {
            List<dynamic> models = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                models = connection.Query<dynamic>(LARGELISTQUERY, null);
            }

            return models;
        }

        //[Benchmark]
        public List<string> LargeStringQuerySQLEConnect()
        {
            List<string> strings = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                strings = connection.Query<string>("SELECT Strp FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }

            return strings;
        }

        //[Benchmark]
        public List<int> LargeIntQuerySQLEConnect()
        {
            List<int> integers = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                integers = connection.Query<int>("SELECT Id FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }

            return integers;
        }

        //[Benchmark]
        public List<DateTime> LargeDateTimesQuerySQLEConnect()
        {
            List<DateTime> dateTimes = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                dateTimes = connection.Query<DateTime>("SELECT Date FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }

            return dateTimes;
        }

        //[Benchmark]
        public List<decimal> LargeDecimalsQuerySQLEConnect()
        {
            List<decimal> decimals = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                decimals = connection.Query<decimal>("SELECT Dcml FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }

            return decimals;
        }

        //[Benchmark]
        public List<long> LargeLongsQuerySQLEConnect()
        {
            List<long> longs = null;

            using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString))
            {
                longs = connection.Query<long>("SELECT Lng FROM models ORDER BY id LIMIT 98989 OFFSET 1011;", null);
            }

            return longs;
        }

        //[Benchmark]
        public Model SmallQuerySQLEConnectSingleModel()
        {
            using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString);

            var (hasResults, model) = connection.Single<Model>(SINGLEQUERY, null);

            return model;
        }

        //[Benchmark]
        public Dictionary<string, object> SmallQuerySQLEConnectSingleDict()
        {
            using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(this._setting.ConnectionString);

            var (hasResults, dict) = connection.Single<Dictionary<string, object>>(SINGLEQUERY, null);

            return dict;
        }
        
        [Benchmark]
        public List<Model> QueryDapperSmall()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this._setting.ConnectionString))
            {
                IEnumerable<Model> results = connection.Query<Model>(SMALLLISTQUERY, null);
                
                return results.ToList();
            }
        }
        
        [Benchmark]
        public List<Model> QueryDapperLarge()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this._setting.ConnectionString))
            {
                IEnumerable<Model> results = connection.Query<Model>(LARGELISTQUERY, null);
                
                return results.ToList();
            }
        }
    }
}