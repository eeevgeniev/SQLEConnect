using System.Diagnostics;
using Npgsql;
using SQLEConnect;
using SQLEConnectTests.Models;
using SQLEConnectTests.SettingParser;
using SQLEConnectTests.Settings;

namespace SQLEConnectTests.QueryPostgreSQLQueries;

class Program
{
    private const string SETTING_NAME = "settings.json";
    private const string COMMAND = "SELECT * FROM GenericTable";
    private const string SINGLECOMMAND = "SELECT * FROM GenericTable WHERE ByteNumber IS NOT NULL";

    static void Main(string[] args)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), SETTING_NAME);
        Parser settingParser = new Parser();
        Setting setting = settingParser.ParserConfiguration(path);

        InsertData(setting.ConnectionString);

        GetObject(setting.ConnectionString);
        
        GetSingleObject(setting.ConnectionString);
        
    }

    private static void GetObject(string connectionString)
    {
        List<GenericTable> objectResults;

        using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString))
        {
            objectResults = connection.Query<GenericTable>(new EntityDescriptor<GenericTable>(), COMMAND, null, false, false);
        }

        CheckObject(objectResults);
    }
    
    private static void GetSingleObject(string connectionString)
    {
        using Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString);

        var (hasResult, result) = connection.Single<GenericTable>(SINGLECOMMAND, null);

        Debug.Assert(hasResult == true);

        CheckSingleObject(result);
    }

    private static void CheckObject(List<GenericTable> results)
    {
        Debug.Assert(results.Count == 2);

        Debug.Assert(results[0].ByteNumber == default);
        Debug.Assert(results[0].NullableByteNumber == default);
        Debug.Assert(results[0].BoolValue == default);
        Debug.Assert(results[0].StringValue == default);
        Debug.Assert(results[0].NullableBoolValue == default);
        Debug.Assert(results[0].Bytes == default);
        Debug.Assert(results[0].Chars == default);
        Debug.Assert(results[0].Letter == default);
        Debug.Assert(results[0].NullableLetter == default);
        Debug.Assert(results[0].DateTimeValue == default);
        Debug.Assert(results[0].NullableDateTimeValue == default);
        Debug.Assert(results[0].DecimalNumber == default);
        Debug.Assert(results[0].NullableDecimalNumber == default);
        Debug.Assert(results[0].DoubleNumber == default);
        Debug.Assert(results[0].NullableDoubleNumber == default);
        Debug.Assert(results[0].FloatNumber == default);
        Debug.Assert(results[0].NullableFloatNumber == default);
        Debug.Assert(results[0].GuidValue == default);
        Debug.Assert(results[0].NullableGuidValue == default);
        Debug.Assert(results[0].IntNumber == default);
        Debug.Assert(results[0].NullableIntNumber == default);
        Debug.Assert(results[0].LongNumber == default);
        Debug.Assert(results[0].NullableLongNumber == default);
        Debug.Assert(results[0].SByteNumber == default);
        Debug.Assert(results[0].NullableSByteNumber == default);
        Debug.Assert(results[0].ShortNumber == default);
        Debug.Assert(results[0].NullableShortNumber == default);
        Debug.Assert(results[0].StreamValue == default);
        Debug.Assert(results[0].StringValue == default);
        Debug.Assert(results[0].UIntNumber == default);
        Debug.Assert(results[0].NullableUIntNumber == default);
        Debug.Assert(results[0].ULongNumber == default);
        Debug.Assert(results[0].NullableULongNumber == default);
        Debug.Assert(results[0].UShortNumber == default);
        Debug.Assert(results[0].NullableUShortNumber == default);

        Debug.Assert(results[1].ByteNumber == Constants.ByteNumber2);
        Debug.Assert(results[1].NullableByteNumber == Constants.NullableByteNumber2);
        Debug.Assert(results[1].BoolValue == Constants.BoolValue2);
        Debug.Assert(results[1].NullableBoolValue == Constants.NullableBoolValue2);
        Debug.Assert(results[1].Bytes.SequenceEqual(Constants.Bytes2));
        Debug.Assert(results[1].Chars.SequenceEqual(Constants.Chars2.ToCharArray()));
        Debug.Assert(results[1].Letter == Constants.Letter2);
        Debug.Assert(results[1].NullableLetter == Constants.NullableLetter2);
        Debug.Assert(results[1].DateTimeValue.ToLocalTime() == Constants.DateTimeValue2);
        Debug.Assert(results[1].NullableDateTimeValue?.ToLocalTime() == Constants.NullableDateTimeValue2);
        Debug.Assert(results[1].DecimalNumber == Constants.DecimalNumber2);
        Debug.Assert(results[1].NullableDecimalNumber == Constants.NullableDecimalNumber2);
        Debug.Assert(results[1].DoubleNumber == Constants.DoubleNumber2);
        Debug.Assert(results[1].NullableDoubleNumber == Constants.NullableDoubleNumber2);
        Debug.Assert(results[1].FloatNumber == Constants.FloatNumber2);
        Debug.Assert(results[1].NullableFloatNumber == Constants.NullableFloatNumber2);
        Debug.Assert(results[1].GuidValue == Constants.GuidValue2);
        Debug.Assert(results[1].NullableGuidValue == Constants.NullableGuidValue2);
        Debug.Assert(results[1].IntNumber == Constants.IntNumber2);
        Debug.Assert(results[1].NullableIntNumber == Constants.NullableIntNumber2);
        Debug.Assert(results[1].LongNumber == Constants.LongNumber2);
        Debug.Assert(results[1].NullableLongNumber == Constants.NullableLongNumber2);
        Debug.Assert(results[1].SByteNumber == (sbyte)Constants.SByteNumber2);
        Debug.Assert(results[1].NullableSByteNumber == (sbyte?)Constants.NullableSByteNumber2);
        Debug.Assert(results[1].ShortNumber == Constants.ShortNumber2);
        Debug.Assert(results[1].NullableShortNumber == Constants.NullableShortNumber2);

        MemoryStream ms = new MemoryStream();
        results[1].StreamValue.CopyTo(ms);

        Debug.Assert(ms.ToArray().SequenceEqual(Constants.StreamValue2));
        Debug.Assert(results[1].StringValue == Constants.StringValue2);
        Debug.Assert(results[1].UIntNumber == Constants.UIntNumber2);
        Debug.Assert(results[1].NullableUIntNumber == Constants.NullableUIntNumber2);
        Debug.Assert(results[1].ULongNumber == (ulong)Constants.ULongNumber2);
        Debug.Assert(results[1].NullableULongNumber == (ulong?)Constants.NullableULongNumber2);
        Debug.Assert(results[1].UShortNumber == (ushort)Constants.UShortNumber2);
        Debug.Assert(results[1].NullableUShortNumber == (ushort?)Constants.NullableUShortNumber2);
    }
    
    private static void CheckSingleObject(GenericTable genericTable)
    {
        Debug.Assert(genericTable.ByteNumber == Constants.ByteNumber2);
        Debug.Assert(genericTable.NullableByteNumber == Constants.NullableByteNumber2);
        Debug.Assert(genericTable.BoolValue == Constants.BoolValue2);
        Debug.Assert(genericTable.NullableBoolValue == Constants.NullableBoolValue2);
        Debug.Assert(genericTable.Bytes.SequenceEqual(Constants.Bytes2));
        Debug.Assert(genericTable.Chars.SequenceEqual(Constants.Chars2.ToCharArray()));
        Debug.Assert(genericTable.Letter == Constants.Letter2);
        Debug.Assert(genericTable.NullableLetter == Constants.NullableLetter2);
        Debug.Assert(genericTable.DateTimeValue.ToLocalTime() == Constants.DateTimeValue2);
        Debug.Assert(genericTable.NullableDateTimeValue?.ToLocalTime() == Constants.NullableDateTimeValue2);
        Debug.Assert(genericTable.DecimalNumber == Constants.DecimalNumber2);
        Debug.Assert(genericTable.NullableDecimalNumber == Constants.NullableDecimalNumber2);
        Debug.Assert(genericTable.DoubleNumber == Constants.DoubleNumber2);
        Debug.Assert(genericTable.NullableDoubleNumber == Constants.NullableDoubleNumber2);
        Debug.Assert(genericTable.FloatNumber == Constants.FloatNumber2);
        Debug.Assert(genericTable.NullableFloatNumber == Constants.NullableFloatNumber2);
        Debug.Assert(genericTable.GuidValue == Constants.GuidValue2);
        Debug.Assert(genericTable.NullableGuidValue == Constants.NullableGuidValue2);
        Debug.Assert(genericTable.IntNumber == Constants.IntNumber2);
        Debug.Assert(genericTable.NullableIntNumber == Constants.NullableIntNumber2);
        Debug.Assert(genericTable.LongNumber == Constants.LongNumber2);
        Debug.Assert(genericTable.NullableLongNumber == Constants.NullableLongNumber2);
        Debug.Assert(genericTable.SByteNumber == (sbyte)Constants.SByteNumber2);
        Debug.Assert(genericTable.NullableSByteNumber == (sbyte?)Constants.NullableSByteNumber2);
        Debug.Assert(genericTable.ShortNumber == Constants.ShortNumber2);
        Debug.Assert(genericTable.NullableShortNumber == Constants.NullableShortNumber2);

        MemoryStream ms = new MemoryStream();
        genericTable.StreamValue.CopyTo(ms);

        Debug.Assert(ms.ToArray().SequenceEqual(Constants.StreamValue2));
        Debug.Assert(genericTable.StringValue == Constants.StringValue2);
        Debug.Assert(genericTable.UIntNumber == Constants.UIntNumber2);
        Debug.Assert(genericTable.NullableUIntNumber == Constants.NullableUIntNumber2);
        Debug.Assert(genericTable.ULongNumber == (ulong)Constants.ULongNumber2);
        Debug.Assert(genericTable.NullableULongNumber == (ulong?)Constants.NullableULongNumber2);
        Debug.Assert(genericTable.UShortNumber == (ushort)Constants.UShortNumber2);
        Debug.Assert(genericTable.NullableUShortNumber == (ushort?)Constants.NullableUShortNumber2);
    }
    
    private static void InsertData(string connectionString)
    {
        using (Connection<NpgsqlConnection> connection = new Connection<NpgsqlConnection>(connectionString, true))
        {
            connection.NonQuery(@"
                            CREATE TABLE IF NOT EXISTS GenericTable 
                            (
                                ByteNumber SMALLINT NULL,
                                NullableByteNumber SMALLINT NULL,
                                BoolValue BOOLEAN NULL,
                                NullableBoolValue BOOLEAN NULL,
                                Bytes BYTEA NULL,
                                Chars TEXT NULL,
                                Letter CHAR NULL,
                                NullableLetter CHAR NULL,
                                DateTimeValue TIMESTAMPTZ NULL,
                                NullableDateTimeValue TIMESTAMPTZ NULL,
                                DecimalNumber NUMERIC(19, 5) NULL,
                                NullableDecimalNumber NUMERIC(19, 5) NULL,
                                DoubleNumber FLOAT8 NULL,
                                NullableDoubleNumber FLOAT8 NULL,
                                FloatNumber REAL NULL,
                                NullableFloatNumber REAL NULL,
                                GuidValue UUID NULL,
                                NullableGuidValue UUID NULL,
                                IntNumber INTEGER NULL,
                                NullableIntNumber INTEGER NULL,
                                LongNumber BIGINT NULL,
                                NullableLongNumber BIGINT NULL,
                                SByteNumber SMALLINT NULL,
                                NullableSByteNumber SMALLINT NULL,
                                ShortNumber SMALLINT NULL,
                                NullableShortNumber SMALLINT NULL,
                                StreamValue BYTEA NULL,
                                StringValue TEXT NULL,
                                UIntNumber INTEGER NULL,
                                NullableUIntNumber INTEGER NULL,
                                ULongNumber BIGINT NULL,
                                NullableULongNumber BIGINT NULL,
                                UShortNumber SMALLINT NULL,
                                NullableUShortNumber SMALLINT NULL
                            );", null);

            connection.NonQuery(@"
                                    DELETE FROM GenericTable;
                                    ", null);

            connection.NonQuery(
                @"
                        INSERT INTO GenericTable
                        (
                            ByteNumber,
                            NullableByteNumber,
                            BoolValue,
                            NullableBoolValue,
                            Bytes,
                            Chars,
                            Letter,
                            NullableLetter,
                            DateTimeValue,
                            NullableDateTimeValue,
                            DecimalNumber,
                            NullableDecimalNumber,
                            DoubleNumber,
                            NullableDoubleNumber,
                            FloatNumber,
                            NullableFloatNumber,
                            GuidValue,
                            NullableGuidValue,
                            IntNumber,
                            NullableIntNumber,
                            LongNumber,
                            NullableLongNumber,
                            SByteNumber,
                            NullableSByteNumber,
                            ShortNumber,
                            NullableShortNumber,
                            StreamValue,
                            StringValue,
                            UIntNumber,
                            NullableUIntNumber,
                            ULongNumber,
                            NullableULongNumber,
                            UShortNumber,
                            NullableUShortNumber
                        )
                        VALUES
                        (
                                @ByteNumber1,
                                @NullableByteNumber1,
                                @BoolValue1,
                                @NullableBoolValue1,
                                @Bytes1,
                                @Chars1,
                                @Letter1,
                                @NullableLetter1,
                                @DateTimeValue1,
                                @NullableDateTimeValue1,
                                @DecimalNumber1,
                                @NullableDecimalNumber1,
                                @DoubleNumber1,
                                @NullableDoubleNumber1,
                                @FloatNumber1,
                                @NullableFloatNumber1,
                                @GuidValue1,
                                @NullableGuidValue1,
                                @IntNumber1,
                                @NullableIntNumber1,
                                @LongNumber1,
                                @NullableLongNumber1,
                                @SByteNumber1,
                                @NullableSByteNumber1,
                                @ShortNumber1,
                                @NullableShortNumber1,
                                @StreamValue1,
                                @StringValue1,
                                @UIntNumber1,
                                @NullableUIntNumber1,
                                @ULongNumber1,
                                @NullableULongNumber1,
                                @UShortNumber1,
                                @NullableUShortNumber1
                            ),
                            (
                                @ByteNumber2,
                                @NullableByteNumber2,
                                @BoolValue2,
                                @NullableBoolValue2,
                                @Bytes2,
                                @Chars2,
                                @Letter2,
                                @NullableLetter2,
                                @DateTimeValue2,
                                @NullableDateTimeValue2,
                                @DecimalNumber2,
                                @NullableDecimalNumber2,
                                @DoubleNumber2,
                                @NullableDoubleNumber2,
                                @FloatNumber2,
                                @NullableFloatNumber2,
                                @GuidValue2,
                                @NullableGuidValue2,
                                @IntNumber2,
                                @NullableIntNumber2,
                                @LongNumber2,
                                @NullableLongNumber2,
                                @SByteNumber2,
                                @NullableSByteNumber2,
                                @ShortNumber2,
                                @NullableShortNumber2,
                                @StreamValue2,
                                @StringValue2,
                                @UIntNumber2,
                                @NullableUIntNumber2,
                                @ULongNumber2,
                                @NullableULongNumber2,
                                @UShortNumber2,
                                @NullableUShortNumber2
                            );",
                new List<SqlEParameter>()
                {
                    new SqlEParameter("@ByteNumber1", typeof(byte)),
                    new SqlEParameter("@NullableByteNumber1", typeof(byte?)),
                    new SqlEParameter("@BoolValue1", typeof(bool)),
                    new SqlEParameter("@NullableBoolValue1", typeof(bool?)),
                    new SqlEParameter("@Bytes1", typeof(byte[])),
                    new SqlEParameter("@Chars1", typeof(char[])),
                    new SqlEParameter("@Letter1", typeof(char)),
                    new SqlEParameter("@NullableLetter1", typeof(char?)),
                    new SqlEParameter("@DateTimeValue1", typeof(DateTime)),
                    new SqlEParameter("@NullableDateTimeValue1", typeof(DateTime?)),
                    new SqlEParameter("@DecimalNumber1", typeof(decimal)),
                    new SqlEParameter("@NullableDecimalNumber1", typeof(decimal?)),
                    new SqlEParameter("@DoubleNumber1", typeof(double)),
                    new SqlEParameter("@NullableDoubleNumber1", typeof(double?)),
                    new SqlEParameter("@FloatNumber1", typeof(float)),
                    new SqlEParameter("@NullableFloatNumber1", typeof(float?)),
                    new SqlEParameter("@GuidValue1", typeof(Guid)),
                    new SqlEParameter("@NullableGuidValue1", typeof(Guid?)),
                    new SqlEParameter("@IntNumber1", typeof(int)),
                    new SqlEParameter("@NullableIntNumber1", typeof(int?)),
                    new SqlEParameter("@LongNumber1", typeof(long)),
                    new SqlEParameter("@NullableLongNumber1", typeof(long?)),
                    new SqlEParameter("@SByteNumber1", typeof(byte)),
                    new SqlEParameter("@NullableSByteNumber1", typeof(byte?)),
                    new SqlEParameter("@ShortNumber1", typeof(short)),
                    new SqlEParameter("@NullableShortNumber1", typeof(short?)),
                    new SqlEParameter("@StreamValue1", typeof(byte[])),
                    new SqlEParameter("@StringValue1", typeof(string)),
                    new SqlEParameter("@UIntNumber1", typeof(int)),
                    new SqlEParameter("@NullableUIntNumber1", typeof(int?)),
                    new SqlEParameter("@ULongNumber1", typeof(long)),
                    new SqlEParameter("@NullableULongNumber1", typeof(long?)),
                    new SqlEParameter("@UShortNumber1", typeof(short)),
                    new SqlEParameter("@NullableUShortNumber1", typeof(short?)),
                    new SqlEParameter("@ByteNumber2", Constants.ByteNumber2),
                    new SqlEParameter("@NullableByteNumber2", Constants.NullableByteNumber2),
                    new SqlEParameter("@BoolValue2", Constants.BoolValue2),
                    new SqlEParameter("@NullableBoolValue2", Constants.NullableBoolValue2),
                    new SqlEParameter("@Bytes2", Constants.Bytes2),
                    new SqlEParameter("@Chars2", Constants.Chars2),
                    new SqlEParameter("@Letter2", Constants.Letter2),
                    new SqlEParameter("@NullableLetter2", Constants.NullableLetter2),
                    new SqlEParameter("@DateTimeValue2", Constants.DateTimeValue2),
                    new SqlEParameter("@NullableDateTimeValue2", Constants.NullableDateTimeValue2),
                    new SqlEParameter("@DecimalNumber2", Constants.DecimalNumber2),
                    new SqlEParameter("@NullableDecimalNumber2", Constants.NullableDecimalNumber2),
                    new SqlEParameter("@DoubleNumber2", Constants.DoubleNumber2),
                    new SqlEParameter("@NullableDoubleNumber2", Constants.NullableDoubleNumber2),
                    new SqlEParameter("@FloatNumber2", Constants.FloatNumber2),
                    new SqlEParameter("@NullableFloatNumber2", Constants.NullableFloatNumber2),
                    new SqlEParameter("@GuidValue2", Constants.GuidValue2),
                    new SqlEParameter("@NullableGuidValue2", Constants.NullableGuidValue2),
                    new SqlEParameter("@IntNumber2", Constants.IntNumber2),
                    new SqlEParameter("@NullableIntNumber2", Constants.NullableIntNumber2),
                    new SqlEParameter("@LongNumber2", Constants.LongNumber2),
                    new SqlEParameter("@NullableLongNumber2", Constants.NullableLongNumber2),
                    new SqlEParameter("@SByteNumber2", Constants.SByteNumber2),
                    new SqlEParameter("@NullableSByteNumber2", Constants.NullableSByteNumber2),
                    new SqlEParameter("@ShortNumber2", Constants.ShortNumber2),
                    new SqlEParameter("@NullableShortNumber2", Constants.NullableShortNumber2),
                    new SqlEParameter("@StreamValue2", Constants.StreamValue2),
                    new SqlEParameter("@StringValue2", Constants.StringValue2),
                    new SqlEParameter("@UIntNumber2", Constants.UIntNumber2),
                    new SqlEParameter("@NullableUIntNumber2", Constants.NullableUIntNumber2),
                    new SqlEParameter("@ULongNumber2", Constants.ULongNumber2),
                    new SqlEParameter("@NullableULongNumber2", Constants.NullableULongNumber2),
                    new SqlEParameter("@UShortNumber2", Constants.UShortNumber2),
                    new SqlEParameter("@NullableUShortNumber2", Constants.NullableUShortNumber2)
                }, false, false);
        }
    }
}