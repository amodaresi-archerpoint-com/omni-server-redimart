
DELETE FROM [dbo].[TenantConfig]
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOUser', N'', N'string', N'User name for LS Central Web Services', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOPassword', N'', N'string', N'User password for LS Central Web Services', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOUrl', N'', N'string', N'LS Central Web Services URL', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOQryUrl', N'', N'string', N'LS Central Query Web Service URL', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOODataUrl', N'', N'string', N'LS Central OData v4 Web Service URL', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOTenant', N'', N'string', N'LS Central Tenant id', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOProtocol', N'', N'string', N'Protocol for LS Central Web Services', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOSql', N'', N'string', N'Back office SQL connection string', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOTimeout', N'20', N'int', N'Back office timeout in seconds', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'BOEncoding', N'utf-8', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'EComUrl', N'', N'string', N'Webhook URL to eCommerce to handle payment requests or order status updates', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'LSNAV_Version', N'', N'string', N'LS Central Version to use', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'LSNAV_Timeout', N'10', N'int', N'Time in Minutes to refresch LS Central Version Cache', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Central_Token', N'', N'string', N'LS Central SaaS Token', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Central_TokenTime', N'', N'string', N'LS Central SaaS Token Expire time', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Cache_Image_DurationInMinutes', N'525600', N'int', N'Image Cache Duration in minutes, 0 is no cache', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Currency_Code', N'GBP', N'string', N'Local currency code. LS Commerce uses this currency code - not getting it from NAV', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Currency_Culture', N'', N'string', N'Ex: en-US  de-DE. By default the UI region on server decides but can be overwritten here', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Currency_LoyCode', N'LOY', N'string', N'Currency to use for loyalty points, default is LOY', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Demo_Print_Enabled', N'false', N'bool', N'true/false, true then print receipt in NAV will not be called', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'GiftCard_DataEntryType', N'GIFTCARDNO', N'string', N'Data entry type used for Gift card', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Password_Policy', N'5-character minimum; case sensitive', N'string', N'Password policy, enforced in back office', 1)
--  5-character minimum; a digit; upper-case; a special character; case sensitive
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'forgotpassword_code_encrypted', N'false', N'bool', N'Reset Code is Encrypted', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'ScanPayGo_Staff', N'150', N'string', N'Staff used for ScanPayGo Transactions', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'ScanPayGo_Terminal', N'P0032', N'string', N'POS Terminal used for ScanPayGo Transactions', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'SPG_Notify_FollowerUpdate', N'false', N'bool', N'Send notification when wish list gets follower changes', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'SPG_Notify_ItemUpdate', N'false', N'bool', N'Send notification when wish list gets item changes', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Inventory_Mask_IncludeCycleCounting', N'false', N'bool', N'Include Cycle counting lines in Mask Replication', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'TenderType_Mapping', N'0=1,1=3,2=10,3=11,4=8', N'string', N'Mapping between LS Commerce and LS Central TenderTypeIds.  CommerceTenderTypeId=LSCentralTenderTypeId,  for ex. cash is 1=1, GiftCard is 10=10 etc.  See enum TenderType', 0)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Timezone_HoursOffset', N'0', N'int', N'NAV stores everything in UTC, Data Director replicated data may need to use time offset', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'URL_Displayed_On_Client', N'http://www.lsretail.com', N'string', N'A URL displayed on client, App or eCommerce', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Proxy_Server', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Proxy_Port', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Proxy_User', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Proxy_Password', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Proxy_Domain', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'NavAppId', N'', N'string', N'NAV specific - ID to track replication', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'NavAppType', N'ECOM', N'string', N'NAV specific - Type to determine app (ECOM/MPOS/INV)', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'SkipBase64Conversion', N'false', N'bool', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Base64MinXmlSizeInKB', N'100', N'int', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'OfflinePrintTemplate', N'', N'string', N'', 1)
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType], [Comment], [Advanced]) 
VALUES (N'', N'Allow_Dublicate_Email', N'false', N'bool', N'Allow Same Email on one or more Member Contacts', 1)
GO
INSERT [dbo].[Users] ([Username], [Password], [Admin]) 
VALUES (N'CommerceUser', N'R1PFRJHFYEGUKIVW/10DQFGBYP6RWAGYYLQ83EJT2VHILYKHFFJ9YEG1W+TFTGJWHP8XGOAHF5CUMCKJQH2N2W==', 1)
GO
INSERT [dbo].[LSKeys] ([LSKey], [Description], [Active]) 
VALUES (N'', N'DEFAULT', 1)
GO