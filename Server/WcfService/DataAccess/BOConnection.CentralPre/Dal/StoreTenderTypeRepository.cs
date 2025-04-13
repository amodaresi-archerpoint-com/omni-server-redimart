﻿using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class StoreTenderTypeRepository : BaseRepository
    {
        // Key : Store No., Code
        const int TABLEID = 99001462;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        //FUNCTION = 0  AND  FOREIGNCURRENCY = 0  ER CASH
        //FUNCTION = 0  AND  FOREIGNCURRENCY = 1  ER CURRENCY
        //FUNCTION = 1 er card

        public StoreTenderTypeRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "mt.[Store No_],mt.[Code],mt.[Description],mt.[Function],mt.[Valid on Mobile POS]," +
                         "mt.[Counting Required],mt.[Change Tend_ Code],mt.[Above Min_ Change Tender Type]," +
                         "mt.[Min_ Change],mt.[Rounding],mt.[Rounding To],mt.[Overtender Allowed],mt.[Overtender Max_ Amt_]," +
                         "mt.[Undertender Allowed],mt.[Drawer Opens],mt.[Return_Minus Allowed],mt.[Foreign Currency]";

            sqlfrom = " FROM [" + navCompanyName + "LSC Tender Type$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Tender Type$5ecfc871-5d82-43f1-9c54-59685e82318d");

            SQLHelper.CheckForSQLInjection(storeId);
            string where = " AND mt.[Store No_]='" + storeId + "'";

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlfrom + GetWhereStatement(true, keys, where, false);
            }
            recordsRemaining = GetRecordCount(TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplStoreTenderType> list = new List<ReplStoreTenderType>();
            string tenderMap = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);
    
            // get records
            sql = GetSQL(fullReplication, batchSize) + sqlcolumns + sqlfrom + GetWhereStatement(fullReplication, keys, where, true);

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
                                list.Add(ReaderToStoreTenderType(reader, tenderMap, out lastKey));
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

                                list.Add(new ReplStoreTenderType()
                                {
                                    StoreID = par[0],
                                    TenderTypeId = par[1],
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
                                    list.Add(ReaderToStoreTenderType(reader, tenderMap, out string ts));
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

        private string GetInfoCodes(string storeNo, string tenderType)
        {
            List<string> list = new List<string>();
            char del = (char)177;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT [Loc_ Group Filter Delimiter] FROM [" + navCompanyName + "LSC Scheduler Setup$5ecfc871-5d82-43f1-9c54-59685e82318d]";
                    char ch = (char)command.ExecuteNonQuery();

                    if (ch != '\uffff')
                        del = ch;

                    string value = $"{storeNo}{del}{tenderType}";
                    command.CommandText = "SELECT t.[Infocode Code],i.[Data Entry Type] " +
                                          "FROM [" + navCompanyName + "LSC Table Specific Infocode$5ecfc871-5d82-43f1-9c54-59685e82318d] t " +
                                          "JOIN [" + navCompanyName + "LSC Infocode$5ecfc871-5d82-43f1-9c54-59685e82318d] i ON i.[Code]=t.[Infocode Code] " +
                                          "WHERE t.[Table ID]=99001462 AND t.[Value]=@val";
                    command.Parameters.AddWithValue("@val", value);
                    TraceSqlCommand(command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = SQLHelper.GetString(reader["Data Entry Type"]);
                            if (list.Contains(code) == false)
                                list.Add(code);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            string infoCode = string.Empty;
            foreach (string val in list)
                infoCode += val + ";";
            return infoCode;
        }

        private ReplStoreTenderType ReaderToStoreTenderType(SqlDataReader reader, string tenderMap, out string timestamp)
        {
            timestamp = ConvertTo.ByteArrayToString(reader["timestamp"] as byte[]);

            string navId = SQLHelper.GetString(reader["Code"]);
            string omniId = ConfigSetting.TenderTypeMapping(tenderMap, navId, true);

            ReplStoreTenderType ttype = new ReplStoreTenderType()
            {
                StoreID = SQLHelper.GetString(reader["Store No_"]),
                TenderTypeId = navId,
                OmniTenderTypeId = omniId,
                Name = SQLHelper.GetString(reader["Description"]),
                TenderFunction = SQLHelper.GetInt32(reader["Function"]),
                ChangeTenderId = SQLHelper.GetString(reader["Change Tend_ Code"]),
                AboveMinimumTenderId = SQLHelper.GetString(reader["Above Min_ Change Tender Type"]),
                MinimumChangeAmount = SQLHelper.GetDecimal(reader, "Min_ Change"),
                Rounding = SQLHelper.GetDecimal(reader, "Rounding To"),
                RoundingMethode = SQLHelper.GetInt32(reader["Rounding"]),
                ValidOnMobilePOS = SQLHelper.GetInt32(reader["Valid on Mobile POS"]),
                ReturnAllowed = SQLHelper.GetInt32(reader["Return_Minus Allowed"]),
                ForeignCurrency = SQLHelper.GetInt32(reader["Foreign Currency"]),
                MaximumOverTenderAmount = SQLHelper.GetDecimal(reader, "Overtender Max_ Amt_"),
                CountingRequired = SQLHelper.GetInt32(reader["Counting Required"]),
                AllowOverTender = SQLHelper.GetInt32(reader["Overtender Allowed"]),
                AllowUnderTender = SQLHelper.GetInt32(reader["Undertender Allowed"]),
                OpenDrawer = SQLHelper.GetInt32(reader["Drawer Opens"]),
            };
            ttype.DataEntryCodes = GetInfoCodes(ttype.StoreID, ttype.TenderTypeId);
            return ttype;
        }
    }
}
 