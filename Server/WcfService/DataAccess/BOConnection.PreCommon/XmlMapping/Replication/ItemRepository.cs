﻿using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication
{
    public class ItemRepository : BaseRepository
    {
        public ItemRepository(BOConfiguration config) : base(config)
        {
        }

        public List<UnitOfMeasure> GetUnitOfMeasure(XMLTableData table)
        {
            List<UnitOfMeasure> list = new List<UnitOfMeasure>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                UnitOfMeasure rec = new UnitOfMeasure();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Qty. per Unit of Measure": rec.QtyPerUom = ConvertTo.SafeDecimal(field.Values[i]); break;
                    }
                }
                rec.Decimals = 0;
                list.Add(rec);
            }
            return list;
        }

        public List<VariantRegistration> GetVariantRegistrations(XMLTableData table)
        {
            List<VariantRegistration> list = new List<VariantRegistration>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                VariantRegistration rec = new VariantRegistration();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Variant": rec.Id = field.Values[i]; break;
                        case "Framework Code": rec.FrameworkCode = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Variant Dimension 1": rec.Dimension1 = field.Values[i]; break;
                        case "Variant Dimension 2": rec.Dimension2 = field.Values[i]; break;
                        case "Variant Dimension 3": rec.Dimension3 = field.Values[i]; break;
                        case "Variant Dimension 4": rec.Dimension4 = field.Values[i]; break;
                        case "Variant Dimension 5": rec.Dimension5 = field.Values[i]; break;
                        case "Variant Dimension 6": rec.Dimension6 = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<VariantExt> GetVariantExt(XMLTableData table)
        {
            List<VariantExt> list = new List<VariantExt>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                VariantExt rec = new VariantExt();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Dimension": rec.Dimension = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<Price> GetPrices(XMLTableData table)
        {
            List<Price> list = new List<Price>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                Price rec = new Price();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Unit of Measure Code": rec.UomId = field.Values[i]; break;
                        case "Unit Price": rec.Amt = ConvertTo.SafeDecimal(field.Values[i]); break;
                    }
                }
                rec.Amount = rec.Amt.ToString();
                list.Add(rec);
            }
            return list;
        }

        public List<DimValue> GetDimValues(XMLTableData table)
        {
            List<DimValue> list = new List<DimValue>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                DimValue rec = new DimValue();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Value": rec.Value = field.Values[i]; break;
                        case "Logical Order": rec.DisplayOrder = ConvertTo.SafeInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public LoyItem ItemGet(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return null;

            LoyItem rec = new LoyItem();
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "No.": rec.Id = field.Values[0]; break;
                    case "Description": rec.Description = field.Values[0]; break;
                    case "LSC Retail Product Code": rec.ProductGroupId = field.Values[0]; break;
                    case "Product Group Code": rec.ProductGroupId = (string.IsNullOrEmpty(rec.ProductGroupId)) ? field.Values[0] : string.Empty; break;
                    case "Item Category Code": rec.ItemCategoryCode = field.Values[0]; break;
                    case "Sales Unit of Measure": rec.SalesUomId = field.Values[0]; break;
                    case "Item Tracking Code": rec.ItemTrackingCode = field.Values[0]; break;
                    case "LSC Season Code": rec.SeasonCode = field.Values[0]; break;
                    case "LSC Item Family Code": rec.ItemFamilyCode = field.Values[0]; break;
                    case "Blocked": rec.Blocked = ConvertTo.SafeBoolean(field.Values[0]); break;
                    case "LSC Scale Item": rec.ScaleItem = ConvertTo.SafeBoolean(field.Values[0]); break;
                    case "Gross Weight": rec.GrossWeight = ConvertTo.SafeDecimal(field.Values[0]); break;
                    case "Unit Volume": rec.UnitVolume = ConvertTo.SafeDecimal(field.Values[0]); break;
                    case "Units per Parcel": rec.UnitsPerParcel = ConvertTo.SafeDecimal(field.Values[0]); break;
                    case "Unit Price": rec.Price = field.Values[0]; break;
                }
            }
            return rec;
        }

        public List<LoyItem> ItemsGet(XMLTableData table)
        {
            List<LoyItem> list = new List<LoyItem>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                LoyItem rec = new LoyItem();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "LSC Retail Product Code": rec.ProductGroupId = field.Values[i]; break;
                        case "Product Group Code": rec.ProductGroupId = field.Values[i]; break;
                        case "Item Category Code": rec.ItemCategoryCode = field.Values[i]; break;
                        case "Sales Unit of Measure": rec.SalesUomId = field.Values[i]; break;
                        case "LSC Season Code": rec.SeasonCode = field.Values[i]; break;
                        case "LSC Item Family Code": rec.ItemFamilyCode = field.Values[i]; break;
                        case "Blocked": rec.Blocked = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "LSC Scale Item": rec.ScaleItem = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "Gross Weight": rec.GrossWeight = ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Unit Volume": rec.UnitVolume = ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Units per Parcel": rec.UnitsPerParcel = ConvertTo.SafeDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ItemCategory> ItemCategoryGet(XMLTableData table)
        {
            List<ItemCategory> list = new List<ItemCategory>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ItemCategory rec = new ItemCategory();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ProductGroup> ProductGroupGet(XMLTableData table)
        {
            List<ProductGroup> list = new List<ProductGroup>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ProductGroup rec = new ProductGroup();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Item Category Code": rec.ItemCategoryId = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<HospAvailabilityResponse> CurrentAvail(XMLTableData table)
        {
            List<HospAvailabilityResponse> list = new List<HospAvailabilityResponse>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                HospAvailabilityResponse rec = new HospAvailabilityResponse();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Number = field.Values[i]; break;
                        case "Unit of Measure": rec.UnitOfMeasure = field.Values[i]; break;
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Available Qty.": rec.Quantity = ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Type": rec.IsDeal = ConvertTo.SafeBoolean(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
