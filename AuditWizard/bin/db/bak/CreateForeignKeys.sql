--------------------------------------------------------------------------------------------------
--                                                                            
--  File Name:   CreateForeignKeys.sql                                   
--                                                                            
--  Description: Blank SQL script                                          
--                                                                            
--  Comments:    Creates the foreign keys with the AuditWizard database
--                                                                                                               
---------------------------------------------------------------------------------------------------
 
--use AuditWizard

--
-- FOREIGN KEYS
-- 
-- ACTIONS links to APPLICATIONS 
ALTER TABLE [dbo].[ACTIONS]  WITH CHECK ADD  CONSTRAINT [FK_ACTIONS_APPLICATIONS] FOREIGN KEY([_APPLICATIONID])
REFERENCES [dbo].[APPLICATIONS] ([_APPLICATIONID])
GO
ALTER TABLE [dbo].[ACTIONS] CHECK CONSTRAINT [FK_ACTIONS_APPLICATIONS]
GO                                            

-- APPLICATION_INSTANCES links to APPLICATIONS
ALTER TABLE [dbo].[APPLICATION_INSTANCES]  WITH CHECK ADD  CONSTRAINT [FK_APPLICATION_INSTANCES_APPLICATIONS] FOREIGN KEY([_APPLICATIONID])
REFERENCES [dbo].[APPLICATIONS] ([_APPLICATIONID])
GO
ALTER TABLE [dbo].[APPLICATION_INSTANCES] CHECK CONSTRAINT [FK_APPLICATION_INSTANCES_APPLICATIONS]
GO

-- APPLICATION_INSTANCES links to ASSETS
ALTER TABLE [dbo].[APPLICATION_INSTANCES]  WITH CHECK ADD  CONSTRAINT [FK_APPLICATION_INSTANCES_ASSETS] FOREIGN KEY([_ASSETID])
REFERENCES [dbo].[ASSETS] ([_ASSETID])
GO
ALTER TABLE [dbo].[APPLICATION_INSTANCES] CHECK CONSTRAINT [FK_APPLICATION_INSTANCES_ASSETS]
GO                                      

-- LICENSES links to APPLICATIONS
ALTER TABLE [dbo].[LICENSES]  WITH CHECK ADD  CONSTRAINT [FK_LICENSES_APPLICATIONS] FOREIGN KEY([_APPLICATIONID])
REFERENCES [dbo].[APPLICATIONS] ([_APPLICATIONID])
GO
ALTER TABLE [dbo].[LICENSES] CHECK CONSTRAINT [FK_LICENSES_APPLICATIONS]
GO                               

-- LICENSES links to LICENSE_TYPES
ALTER TABLE [dbo].[LICENSES]  WITH CHECK ADD  CONSTRAINT [FK_LICENSES_LICENSE_TYPES] FOREIGN KEY([_LICENSETYPEID])
REFERENCES [dbo].[LICENSE_TYPES] ([_LICENSETYPEID])
GO
ALTER TABLE [dbo].[LICENSES] CHECK CONSTRAINT [FK_LICENSES_LICENSE_TYPES]
GO  

-- LICENSES links to SUPPLIERS
ALTER TABLE [dbo].[LICENSES]  WITH CHECK ADD  CONSTRAINT [FK_LICENSES_SUPPLIERS] FOREIGN KEY([_SUPPLIERID])
REFERENCES [dbo].[SUPPLIERS] ([_SUPPLIERID])
GO
ALTER TABLE [dbo].[LICENSES] CHECK CONSTRAINT [FK_LICENSES_SUPPLIERS]
GO

-- ASSETS links to ASSET_TYPES
ALTER TABLE [dbo].[ASSETS]  WITH CHECK ADD  CONSTRAINT [FK_ASSETS_ASSET_TYPES] FOREIGN KEY([_ASSETTYPEID])
REFERENCES [dbo].[ASSET_TYPES] ([_ASSETTYPEID])
GO
ALTER TABLE [dbo].[ASSETS] CHECK CONSTRAINT [FK_ASSETS_ASSET_TYPES]
GO