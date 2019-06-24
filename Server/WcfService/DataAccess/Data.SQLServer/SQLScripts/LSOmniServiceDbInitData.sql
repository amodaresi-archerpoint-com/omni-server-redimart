
DELETE FROM [dbo].[TenantConfig]
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Cache_Image_DurationInMinutes', N'525600', N'int')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Currency_Code', N'GBP', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Currency_Culture', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Demo_Print_Enabled', N'false', N'bool')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Image_Save_AbsolutePath', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'TenderType_Mapping', N'0=1,1=3,2=10,3=11,4=8', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Timezone_HoursOffset', N'0', N'int')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Password_Policy', N'5-character minimum; case sensitive', N'string')
--  5-character minimum; a digit; upper-case; a special character; case sensitive
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSNAV_Version', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'URL_Displayed_On_Client', N'http://www.lsretail.com', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'GiftCard_DataEntryType', N'GIFTCARDNO', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOUser', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOPassword', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOUrl', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'EcommUrl', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'forgotpassword_code_encrypted', N'false', N'bool')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOSql', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOTimeout', N'20', N'int')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'BOEncoding', N'utf-8', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Proxy_Server', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Proxy_Port', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Proxy_User', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Proxy_Password', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Proxy_Domain', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'NavAppId', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'NavAppType', N'ECOM', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'SkipBase64Conversion', N'false', N'bool')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'Base64MinXmlSizeInKB', N'100', N'int')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'OfflinePrintTemplate', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'PDF_Save_FolderName', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_AzureAccountKey', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_AzureName', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_EndPointUrl', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_NumberOfRecommendedItems', N'10', N'int')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_AccountConnection', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_CalculateStock', N'false', N'bool')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_WsURI', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_WsUserName', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_WsPassword', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_WsDomain', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_StoreNo', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_Location', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'LSReccomend_MinStock', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'POS_System_Inventory', N'', N'string')
GO
INSERT [dbo].[TenantConfig] ([LSKey], [Key], [Value], [DataType]) 
VALUES (N'', N'POS_System_Inventory_Lookup', N'', N'string')
GO
INSERT [dbo].[Users] ([Username], [Password], [Admin]) 
VALUES (N'LSOmniUser', N'BEMW9GBMBA4N1RWZ50WZ+0XVE286MHRCCEFKGNE6+BCXRXXWKUDXW9VU3H+G5DFU9WKQPBL+1QT6U9TP+LTUAQ==', N'1')
GO
INSERT [dbo].[LSKeys] ([LSKey], [Description], [Active]) 
VALUES (N'', N'DEFAULT', N'1')
GO