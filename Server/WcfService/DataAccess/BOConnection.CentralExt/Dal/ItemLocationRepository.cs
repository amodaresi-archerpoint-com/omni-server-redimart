﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class ItemLocationRepository : BaseRepository
    {
        // Key: Item No.,Store No.,Section Code,Shelf Code
        const int TABLEID = 99001533;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemLocationRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlcolumns = "mt.[Item No_],mt.[Store No_],mt.[Section Code],st.[Description] AS STDESC,mt.[Shelf Code],se.[Description] AS SEDESC";

            sqlfrom = " FROM [" + navCompanyName + "LSC Item Section Location$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                      " LEFT JOIN [" + navCompanyName + "LSC Section Shelf$5ecfc871-5d82-43f1-9c54-59685e82318d] se" +
                      " ON se.[Section Code]=mt.[Section Code] AND se.[Code]=mt.[Shelf Code] AND se.[Store No_]=mt.[Store No_]" +
                      " LEFT JOIN [" + navCompanyName + "LSC Store Section$5ecfc871-5d82-43f1-9c54-59685e82318d] st" +
                      " ON st.[Code]=mt.[Section Code] AND st.[Store No_]=mt.[Store No_]";
        }

        public List<ReplItemLocation> ReplicateItemLocation(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Item Section Location$5ecfc871-5d82-43f1-9c54-59685e82318d");
            string prevlastkey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemLocation> list = new List<ReplItemLocation>();

            // get records
            sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "mt.[Item No_]", storeId, true);

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
                                list.Add(ReaderToItemLocation(reader, out lastKey));
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
                                if (par.Length < 4 || par.Length != keys.Count)
                                    continue;

                                list.Add(new ReplItemLocation()
                                {
                                    ItemId = par[0],
                                    StoreId = par[1],
                                    SectionCode = par[2],
                                    ShelfCode = par[3],
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
                                    list.Add(ReaderToItemLocation(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }

                        if (actions.Count == 0)
                            lastKey = prevlastkey;

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

        public List<ReplItemLocation> ReplicateItemLocationTM(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ProcessLastKey(lastKey, out string mainKey, out string delKey);
            List<JscKey> keys = GetPrimaryKeys("LSC Item Section Location$5ecfc871-5d82-43f1-9c54-59685e82318d");

            string sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, false);
            recordsRemaining = GetRecordCountTM(mainKey, sql, keys);

            List<JscActions> actions = LoadDeleteActions(fullReplication, TABLEID, "LSC Item Section Location$5ecfc871-5d82-43f1-9c54-59685e82318d", keys, batchSize, ref delKey);
            sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Item No_]", storeId, true);

            List<ReplItemLocation> list = new List<ReplItemLocation>();
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
                            list.Add(ReaderToItemLocation(reader, out mainKey));
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    foreach (JscActions act in actions)
                    {
                        string[] par = act.ParamValue.Split(';');
                        if (par.Length < 4 || par.Length != keys.Count)
                            continue;

                        list.Add(new ReplItemLocation()
                        {
                            ItemId = par[0],
                            StoreId = par[1],
                            SectionCode = par[2],
                            ShelfCode = par[3],
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

        public List<ItemLocation> ItemLocationGetByItemId(string itemId, string storeId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<ItemLocation> list = new List<ItemLocation>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Item No_]=@id";
                    command.Parameters.AddWithValue("@id", itemId);
                    if (string.IsNullOrEmpty(storeId) == false) 
                    {
                        command.CommandText += " AND mt.[Store No_]=@sid";
                        command.Parameters.AddWithValue("@sid", storeId);
                    }
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItemLocation(reader));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        private ReplItemLocation ReaderToItemLocation(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemLocation()
            {
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                SectionCode = SQLHelper.GetString(reader["Section Code"]),
                SectionDescription = SQLHelper.GetString(reader["STDESC"]),
                ShelfCode = SQLHelper.GetString(reader["Shelf Code"]),
                ShelfDescription = SQLHelper.GetString(reader["SEDESC"]),
            };
        }

        private ItemLocation ReaderToLoyItemLocation(SqlDataReader reader)
        {
            //item table data
            return new ItemLocation()
            {
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                SectionCode = SQLHelper.GetString(reader["Section Code"]),
                SectionDescription = SQLHelper.GetString(reader["STDESC"]),
                ShelfCode = SQLHelper.GetString(reader["Shelf Code"]),
                ShelfDescription = SQLHelper.GetString(reader["SEDESC"]),
            };
        }
    }
}