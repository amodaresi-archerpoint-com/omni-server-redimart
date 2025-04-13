﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class ItemRecipeRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 90;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemRecipeRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Parent Item No_],mt.[Item No_],mt.Exclusion,mt.[Price on Exclusion]," +
                         "mt2.[Description],mt2.[Line No_],mt2.[Unit of Measure Code],mt2.[Quantity per]," +
                         "(SELECT TOP(1) il.[Image Id] FROM [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il " +
                         "WHERE il.[KeyValue]=mt.[Item No_] AND il.[TableName]='Item' " +
                         "ORDER BY il.[Display Order]) AS ImageId";
            sqlfrom = " FROM [" + navCompanyName + "BOM Component$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                      " JOIN [" + navCompanyName + "BOM Component$437dbf0e-84ff-417a-965d-ed2bb9650972] mt2 ON mt2.[Parent Item No_]=mt.[Parent Item No_] AND mt2.[Line No_]=mt.[Line No_]";
        }

        public List<ReplItemRecipe> ReplicateItemRecipe(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            // get records remaining
            string sql = string.Empty;
            List<JscKey> keys = GetPrimaryKeys("BOM Component$437dbf0e-84ff-417a-965d-ed2bb9650972");
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Parent Item No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemRecipe> list = new List<ReplItemRecipe>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "mt.[Parent Item No_]", storeId, false);

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
                                list.Add(ReaderToRecipe(reader, out lastKey));
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

                                list.Add(new ReplItemRecipe()
                                {
                                    RecipeNo = par[0],
                                    LineNo = Convert.ToInt32(par[1]),
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
                                    list.Add(ReaderToRecipe(reader, out string ts));
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

        public List<ItemRecipe> RecipeGetByItemId(string id)
        {
            List<ItemRecipe> list = new List<ItemRecipe>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom +
                        " WHERE mt.[Parent Item No_]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToRecipe(reader));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplItemRecipe ReaderToRecipe(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemRecipe()
            {
                RecipeNo = SQLHelper.GetString(reader["Parent Item No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                ItemNo = SQLHelper.GetString(reader["Item No_"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"]),
                Exclusion = SQLHelper.GetBool(reader["Exclusion"]),
                ExclusionPrice = SQLHelper.GetDecimal(reader, "Price on Exclusion"),
                QuantityPer = SQLHelper.GetDecimal(reader, "Quantity per"),
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
        }

        private ItemRecipe ReaderToRecipe(SqlDataReader reader)
        {
            return new ItemRecipe()
            {
                Id = SQLHelper.GetString(reader["Item No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                UnitOfMeasure = SQLHelper.GetString(reader["Unit of Measure Code"]),
                Exclusion = SQLHelper.GetBool(reader["Exclusion"]),
                ExclusionPrice = SQLHelper.GetDecimal(reader, "Price on Exclusion"),
                QuantityPer = SQLHelper.GetDecimal(reader, "Quantity per"),
                ImageId = SQLHelper.GetString(reader["ImageId"])
            };
        }
    }
}
