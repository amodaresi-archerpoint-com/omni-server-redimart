﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class ItemCategoryRepository : BaseRepository
    {
        // Key: Code
        const int TABLEID = 5722;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public ItemCategoryRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            sqlcolumns = "mt.[Code],mt.[Description]";

            sqlfrom = " FROM [" + navCompanyName + "Item Category$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" +
                      " LEFT JOIN [" + navCompanyName + "Retail Product Group$5ecfc871-5d82-43f1-9c54-59685e82318d] pg ON pg.[Item Category Code]=mt.[Code]" +
                      " LEFT JOIN [" + navCompanyName + "Item$5ecfc871-5d82-43f1-9c54-59685e82318d] it ON it.[Retail Product Code]=pg.[Code]";
        }

        public List<ReplItemCategory> ReplicateItemCategory(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("Item Category$437dbf0e-84ff-417a-965d-ed2bb9650972");
            string prevlastkey = lastKey;

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(DISTINCT mt.[Code])" + sqlfrom + GetWhereStatementWithStoreDist(true, keys, "it.[No_]", storeId, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplItemCategory> list = new List<ReplItemCategory>();

            // get records
            sql = GetSQL(fullReplication, batchSize, true, true) + sqlcolumns + sqlfrom + GetWhereStatementWithStoreDist(fullReplication, keys, "it.[No_]", storeId, true);

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
                                list.Add(ReaderToItemCategory(reader, out lastKey));
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
                                list.Add(new ReplItemCategory()
                                {
                                    Id = act.ParamValue,
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
                                    list.Add(ReaderToItemCategory(reader, out string ts));
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

        public List<ItemCategory> ItemCategoriesGetAll(string storeId, string culture)
        {
            List<ItemCategory> list = new List<ItemCategory>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT DISTINCT " + sqlcolumns + sqlfrom + 
                            " WHERE EXISTS(" +
                            " SELECT 1 FROM [" + navCompanyName + "Retail Product Group$5ecfc871-5d82-43f1-9c54-59685e82318d] pg " +
                            " JOIN [" + navCompanyName + "Item$5ecfc871-5d82-43f1-9c54-59685e82318d] i ON i.[Retail Product Code]=pg.[Code] " +
                            " WHERE mt.[Code]=pg.[Item Category Code])" +
                            GetSQLStoreDist("it.[No_]", storeId, true) +
                            " ORDER BY mt.[Description]";
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItemCategory(reader, culture));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public ItemCategory ItemCategoryGetById(string id)
        {
            ItemCategory cat = new ItemCategory();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT " + sqlcolumns + " FROM [" + navCompanyName + "Item Category$437dbf0e-84ff-417a-965d-ed2bb9650972] mt WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cat = ReaderToLoyItemCategory(reader, string.Empty);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return cat;
        }

        public List<ItemCategory> ItemCategorySearch(string search)
        {
            List<ItemCategory> list = new List<ItemCategory>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sqlWhere = SQLHelper.SetSearchParameters(command, search, GetDbCICollation());
                    command.CommandText = "SELECT " + sqlcolumns + " FROM [" + navCompanyName + "Item Category$437dbf0e-84ff-417a-965d-ed2bb9650972] mt" + sqlWhere;
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyItemCategory(reader, string.Empty));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private ReplItemCategory ReaderToItemCategory(SqlDataReader reader, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            return new ReplItemCategory()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"])
            };
        }

        private ItemCategory ReaderToLoyItemCategory(SqlDataReader reader, string culture)
        {
            //item table data
            ItemCategory itemcategory = new ItemCategory()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Description = SQLHelper.GetString(reader["Description"])
            };

            ProductGroupRepository prdrep = new ProductGroupRepository(config, NavVersion);
            itemcategory.ProductGroups = prdrep.ProductGroupGetByItemCategoryId(itemcategory.Id, culture, false, false);

            ImageRepository imgrep = new ImageRepository(config, NavVersion);
            itemcategory.Images = imgrep.ImageGetByKey("Item Category", itemcategory.Id, string.Empty, string.Empty, 0, false);

            return itemcategory;
        }
    }
}