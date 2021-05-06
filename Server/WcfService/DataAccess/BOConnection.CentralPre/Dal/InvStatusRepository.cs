﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class InvStatusRepository : BaseRepository
    {
        public InvStatusRepository(BOConfiguration config) : base(config)
        {
        }

        public virtual List<ReplInvStatus> ReplicateInventoryStatus(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            string sqlcolumns = "mt.[timestamp],mt.[Item No_],mt.[Variant Code],mt.[Store No_],mt.[Net Inventory],mt.[Replication Counter]";
            string sqlfrom = " FROM [" + navCompanyName + "LSC Inventory Lookup Table$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
            string sqlwhere = " WHERE " + ((fullReplication) ? "mt.[timestamp]>@cnt" : "mt.[Replication Counter]>@cnt");
            sqlwhere += string.IsNullOrEmpty(storeId) ? string.Empty : " AND mt.[Store No_]=@sid";

            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplInvStatus> list = new List<ReplInvStatus>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    // get records remaining
                    command.CommandText = "SELECT COUNT(*)" + sqlfrom + sqlwhere;
                    command.Parameters.AddWithValue("@sid", storeId);
                    if (fullReplication)
                    {
                        SqlParameter par = new SqlParameter("@cnt", SqlDbType.Timestamp);
                        if (lastKey == "0")
                            par.Value = new byte[] { 0 };
                        else
                            par.Value = StringToByteArray(lastKey);
                        command.Parameters.Add(par);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@cnt", lastKey);
                    }
                    TraceSqlCommand(command);
                    recordsRemaining = (int)command.ExecuteScalar();

                    if (string.IsNullOrEmpty(maxKey) || maxKey == "0")
                    {
                        // get max value
                        command.CommandText = "SELECT MAX([Replication Counter])" + sqlfrom + sqlwhere;
                        var ret = command.ExecuteScalar();
                        maxKey = (ret == DBNull.Value) ? "0" : ret.ToString();
                    }

                    // get data
                    command.CommandText = "SELECT " + ((batchSize > 0) ? "TOP(" + batchSize.ToString() + ") " : string.Empty) + sqlcolumns + sqlfrom + sqlwhere + " ORDER BY " +
                        ((fullReplication) ? "mt.[timestamp]" : "mt.[Replication Counter]");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int cnt = 0;
                        while (reader.Read())
                        {
                            if (fullReplication)
                                lastKey = ByteArrayToString(reader["timestamp"] as byte[]);
                            else
                                lastKey = SQLHelper.GetString(reader["Replication Counter"]);

                            list.Add(new ReplInvStatus()
                            {
                                ItemId = SQLHelper.GetString(reader["Item No_"]),
                                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                                StoreId = SQLHelper.GetString(reader["Store No_"]),
                                Quantity = SQLHelper.GetDecimal(reader, "Net Inventory"),
                                IsDeleted = false
                            });
                            cnt++;
                        }
                        reader.Close();
                        recordsRemaining -= cnt;
                    }

                    if (recordsRemaining <= 0)
                        lastKey = maxKey;   // this should be the highest PreAction id;

                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }
    }
}
