﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class CurrencyExchRateRepository : BaseRepository
    {
        // Key : Currency Code, Starting Date
        const int TABLEID = 330;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public CurrencyExchRateRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Currency Code],mt.[Starting Date],mt.[Relational Currency Code],mt.[Relational Exch_ Rate Amount],mt.[Relational Currency Code],mt.[Exchange Rate Amount]," +
                         "mt2.[LSC POS Exchange Rate Amount$5ecfc871-5d82-43f1-9c54-59685e82318d],mt2.[LSC POS Rel_ Exch_ Rate Amount$5ecfc871-5d82-43f1-9c54-59685e82318d]";

            sqlfrom = " FROM [" + navCompanyName + "Currency Exchange Rate$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" +
                      " JOIN [" + navCompanyName + "Currency Exchange Rate$437dbf0e-84ff-417a-965d-ed2bb9650972$ext] mt2 ON mt2.[Currency Code]=mt.[Currency Code] AND mt2.[Starting Date]=mt.[Starting Date]";
        }

        public List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Currency Exchange Rate$437dbf0e-84ff-417a-965d-ed2bb9650972");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplCurrencyExchRate> list = new List<ReplCurrencyExchRate>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, true);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    if (fullReplication)
                    {
                        JscActions act = new JscActions(lastKey);
                        SetWhereValues(command, act, keys, true, true);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int cnt = 0;
                            while (reader.Read())
                            {
                                list.Add(ReaderToCurrencyExchRate(reader, out lastKey, true));
                                cnt++;
                            }
                            reader.Close();
                            recordsRemaining -= cnt;
                        }
                        if (recordsRemaining <= 0)
                            lastKey = maxKey;   // this should be the highest PreAction id;
                    }
                    else
                    {
                        bool first = true;
                        foreach (JscActions act in actions)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                string[] par = act.ParamValue.Split(';');
                                if (par.Length < 2 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplCurrencyExchRate()
                                {
                                    CurrencyCode = par[0],
                                    StartingDate = ConvertTo.GetDateTimeFromNav(par[1]),
                                    IsDeleted = true
                                });
                                continue;
                            }

                            if (SetWhereValues(command, act, keys, first) == false)
                                continue;

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToCurrencyExchRate(reader, out string ts, true));
                                }
                                reader.Close();
                            }
                            first = false;
                        }
                        if (string.IsNullOrEmpty(maxKey))
                            maxKey = lastKey;
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }

        public List<ReplCurrencyExchRate> ReplicateCurrencyExchRateTM(int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ProcessLastKey(lastKey, out string mainKey, out string delKey);
            List<JscKey> keys = GetPrimaryKeys("Currency Exchange Rate$437dbf0e-84ff-417a-965d-ed2bb9650972");

            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, false);
            recordsRemaining = GetRecordCountTM(mainKey, sql, keys);

            List<JscActions> actions = LoadDeleteActions(fullReplication, TABLEID, "Currency Exchange Rate$437dbf0e-84ff-417a-965d-ed2bb9650972", keys, batchSize, ref delKey);
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(true, keys, true);

            List<ReplCurrencyExchRate> list = new List<ReplCurrencyExchRate>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    JscActions actKey = new JscActions(mainKey);
                    SetWhereValues(command, actKey, keys, true, true);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            list.Add(ReaderToCurrencyExchRate(reader, out mainKey, true));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    foreach (JscActions act in actions)
                    {
                        string[] par = act.ParamValue.Split(';');
                        if (par.Length < 2 || par.Length != keys.Count)
                            continue;

                        list.Add(new ReplCurrencyExchRate()
                        {
                            CurrencyCode = par[0],
                            StartingDate = ConvertTo.GetDateTimeFromNav(par[1]),
                            IsDeleted = true
                        });
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            lastKey = $"R={mainKey};D={delKey};";
            return list;
        }

        public ReplCurrencyExchRate CurrencyExchRateGetById(string code)
        {
            ReplCurrencyExchRate exchrate = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP(1) " + sqlcolumns + sqlfrom + " WHERE mt.[Currency Code]=@id ORDER BY mt.[Starting Date] DESC";
                    command.Parameters.AddWithValue("@id", code.ToUpper());
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            exchrate = ReaderToCurrencyExchRate(reader, out string ts, false);
                            break;
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return exchrate;
        }

        private ReplCurrencyExchRate ReaderToCurrencyExchRate(SqlDataReader reader, out string timestamp, bool getTS)
        {
            ReplCurrencyExchRate currency = new ReplCurrencyExchRate()
            {
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                RelationalCurrencyCode = SQLHelper.GetString(reader["Relational Currency Code"]),
                StartingDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Starting Date"]), config.IsJson)
            };

            if (SQLHelper.GetDecimal(reader, "LSC POS Exchange Rate Amount") != 0)
            {
                currency.CurrencyFactor = ((1 / SQLHelper.GetDecimal(reader, "LSC POS Exchange Rate Amount")) * SQLHelper.GetDecimal(reader, "LSC POS Rel_ Exch_ Rate Amount"));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(currency.RelationalCurrencyCode))
                {
                    if (SQLHelper.GetDecimal(reader, "Exchange Rate Amount") != 0)
                    {
                        currency.CurrencyFactor = ((1 / SQLHelper.GetDecimal(reader, "Exchange Rate Amount")) * SQLHelper.GetDecimal(reader, "Relational Exch_ Rate Amount"));
                    }
                }
                else
                {
                    using (ReplCurrencyExchRate relcur = CurrencyExchRateGetById(currency.RelationalCurrencyCode))
                    {
                        currency.CurrencyFactor = relcur.CurrencyFactor;
                    }
                }
            }

            if (getTS)
                timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);
            else
                timestamp = "0";

            return currency;
        }
    }
}
 