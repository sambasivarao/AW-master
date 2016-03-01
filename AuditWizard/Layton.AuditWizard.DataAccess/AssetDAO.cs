﻿//////////////////////////////////////////////////////////////////////////////////////////////
//
// History
// =======
//
//	CMD 8.4.2 - Problem 36 - Exception when deleting asset with child assets
//
//////////////////////////////////////////////////////////////////////////////////////////////



using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Layton.AuditWizard.DataAccess
{
    public class AssetDAO
    {
        #region Data

        protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isDebugEnabled = logger.IsDebugEnabled;
        private readonly bool isCompactDatabase;
        private Configuration config = ConfigurationManager.OpenExeConfiguration(Path.Combine(Application.StartupPath, "AuditWizardv8.exe"));

        #endregion

        public AssetDAO()
        {
            isCompactDatabase = Convert.ToBoolean(config.AppSettings.Settings["CompactDatabaseType"].Value);
        }

        #region Assets Table

        public string RationaliseManufacturerNames(string aManufacturerName)
        {
            string lManufacturerName = aManufacturerName.ToUpper();

            if (lManufacturerName.StartsWith("MICROSOFT"))
                aManufacturerName = DataStrings.MICROSOFT;

            else if (lManufacturerName.StartsWith("LAYTON"))
                aManufacturerName = "Layton Technology, Inc.";

            else if (lManufacturerName.StartsWith("APPLE"))
                aManufacturerName = "Apple Inc.";

            else if (lManufacturerName.StartsWith("DELL"))
                aManufacturerName = "Dell Computer Corporation, Inc.";

            else if (lManufacturerName.StartsWith("IBM"))
                aManufacturerName = "IBM Corporation, Inc.";

            else if (lManufacturerName.StartsWith("INTEL"))
                aManufacturerName = "Intel Corporation";

            else if (lManufacturerName.StartsWith("MACROMEDIA"))
                aManufacturerName = "Macromedia Corporation, Inc.";

            else if (lManufacturerName.StartsWith("SUN MICROSYSTEMS"))
                aManufacturerName = "Sun Microsystems, Inc.";

            else if (lManufacturerName.StartsWith("SYMANTEC"))
                aManufacturerName = "Symantec Corporation, Inc.";

            else if (lManufacturerName.StartsWith("HEWLETT"))
                aManufacturerName = "Hewlett-Packard";

            else if (lManufacturerName == "HP")
                aManufacturerName = "Hewlett-Packard";

            else if (lManufacturerName.StartsWith("VMWARE"))
                aManufacturerName = "VMware, Inc";

            else if (lManufacturerName.StartsWith("ACER"))
                aManufacturerName = "Acer";

            else if (lManufacturerName.StartsWith("LENOVO"))
                aManufacturerName = "Lenovo";

            else if (lManufacturerName.StartsWith("TOSHIBA"))
                aManufacturerName = "Toshiba";

            return aManufacturerName;
        }

        /// <summary>
        /// Inserts a new asset into the database
        /// </summary>
        /// <param name="aAsset"></param>
        /// <returns></returns>
        public int AssetAdd(Asset aAsset)
        {
            if (isDebugEnabled) logger.Debug("AssetAdd in with asset id of : " + aAsset.AssetID);

            aAsset.Make = RationaliseManufacturerNames(aAsset.Make);

            int lInsertedAssetID = 0;

            if (AssetFind(aAsset) == 0)
            {
                try
                {
                    string commandText =
                                "INSERT INTO ASSETS " +
                                "(_UNIQUEID, " +
                                "_NAME, " +
                                "_LOCATIONID, " +
                                "_DOMAINID, " +
                                "_IPADDRESS, " +
                                "_MACADDRESS, " +
                                "_ASSETTYPEID, " +
                                "_MAKE, " +
                                "_MODEL, " +
                                "_SERIAL_NUMBER, " +
                                "_PARENT_ASSETID, " +
                                "_SUPPLIERID, " +
                                "_STOCK_STATUS, " +
                                "_AGENT_VERSION, " +
                                "_ALERTS_ENABLED, " +
                                "_ASSETTAG, " +
                                "_OVERWRITEDATA) " +
                                "VALUES (@UniqueID," +
                                "@Name, " +
                                "@LocationID, " +
                                "@DomainID, " +
                                "@IPAddress, " +
                                "@MACAddress, " +
                                "@AssetTypeID, " +
                                "@Make, " +
                                "@Model, " +
                                "@SerialNumber, " +
                                "@ParentAssetID, " +
                                "@SupplierID, " +
                                "@StockStatus, " +
                                "@AgentVersion, " +
                                "@AlertsEnabled, " +
                                "@AssetTag, " +
                                "@OverwriteData)";

                    if (isCompactDatabase)
                    {
                        using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                        {
                            using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                            {
                                command.Parameters.AddWithValue("@UniqueID", aAsset.UniqueID);
                                command.Parameters.AddWithValue("@Name", aAsset.Name);
                                command.Parameters.AddWithValue("@LocationID", aAsset.LocationID);
                                command.Parameters.AddWithValue("@DomainID", aAsset.DomainID);
                                command.Parameters.AddWithValue("@IPAddress", aAsset.IPAddress);
                                command.Parameters.AddWithValue("@MACAddress", aAsset.MACAddress);
                                command.Parameters.AddWithValue("@AssetTypeID", aAsset.AssetTypeID);
                                command.Parameters.AddWithValue("@Make", aAsset.Make);
                                command.Parameters.AddWithValue("@Model", aAsset.Model);
                                command.Parameters.AddWithValue("@SerialNumber", aAsset.SerialNumber);
                                command.Parameters.AddWithValue("@StockStatus", (int)aAsset.StockStatus);
                                command.Parameters.AddWithValue("@AgentVersion", aAsset.AgentVersion);
                                command.Parameters.AddWithValue("@ParentAssetID", aAsset.ParentAssetID);
                                command.Parameters.AddWithValue("@SupplierID", aAsset.SupplierID);
                                command.Parameters.AddWithValue("@AlertsEnabled", Convert.ToInt32(aAsset.AlertsEnabled));
                                command.Parameters.AddWithValue("@AssetTag", aAsset.AssetTag);
                                command.Parameters.AddWithValue("@OverwriteData", true);

                                command.ExecuteNonQuery();
                            }

                            // then get id of newly inserted record
                            using (SqlCeCommand commandReturnValue = new SqlCeCommand("SELECT @@IDENTITY", conn))
                            {
                                lInsertedAssetID = Convert.ToInt32(commandReturnValue.ExecuteScalar());
                            }
                        }
                    }
                    else
                    {
                        using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                        {
                            using (SqlCommand command = new SqlCommand(commandText, conn))
                            {
                                command.Parameters.AddWithValue("@UniqueID", aAsset.UniqueID);
                                command.Parameters.AddWithValue("@Name", aAsset.Name);
                                command.Parameters.AddWithValue("@LocationID", aAsset.LocationID);
                                command.Parameters.AddWithValue("@DomainID", aAsset.DomainID);
                                command.Parameters.AddWithValue("@IPAddress", aAsset.IPAddress);
                                command.Parameters.AddWithValue("@MACAddress", aAsset.MACAddress);
                                command.Parameters.AddWithValue("@AssetTypeID", aAsset.AssetTypeID);
                                command.Parameters.AddWithValue("@Make", aAsset.Make);
                                command.Parameters.AddWithValue("@Model", aAsset.Model);
                                command.Parameters.AddWithValue("@SerialNumber", aAsset.SerialNumber);
                                command.Parameters.AddWithValue("@StockStatus", (int)aAsset.StockStatus);
                                command.Parameters.AddWithValue("@AgentVersion", aAsset.AgentVersion);
                                command.Parameters.AddWithValue("@ParentAssetID", aAsset.ParentAssetID);
                                command.Parameters.AddWithValue("@SupplierID", aAsset.SupplierID);
                                command.Parameters.AddWithValue("@AlertsEnabled", Convert.ToInt32(aAsset.AlertsEnabled));
                                command.Parameters.AddWithValue("@AssetTag", aAsset.AssetTag);
                                command.Parameters.AddWithValue("@OverwriteData", true);

                                command.ExecuteNonQuery();
                            }

                            // then get id of newly inserted record
                            using (SqlCommand commandReturnValue = new SqlCommand("SELECT @@IDENTITY", conn))
                            {
                                lInsertedAssetID = Convert.ToInt32(commandReturnValue.ExecuteScalar());
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }

            }

            if (isDebugEnabled) logger.Debug("AssetAdd out with id of : " + lInsertedAssetID);

            return lInsertedAssetID;
        }

        /// <summary>
        /// Flag an asset as having been audited
        /// </summary>
        /// <param name="assetID"></param>
        /// <returns></returns>
        public int AssetIDByAssetName(string aAssetName)
        {
            if (isDebugEnabled) logger.Debug("AssetIDByAssetName in with asset name of : " + aAssetName);

            int lAssetID = -1;

            // which database type is this?
            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText = "SELECT MIN(_ASSETID) FROM ASSETS WHERE _NAME = @cName";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", aAssetName);
                            object result = command.ExecuteScalar();
                            if ((result != null) && (result.GetType() != typeof(DBNull)))
                            {
                                lAssetID = Convert.ToInt32(result);
                            }
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                lAssetID = lAuditWizardDataAccess.AssetIDByAssetName(aAssetName);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");

            return lAssetID;
        }



        /// <summary>
        /// Flag an asset as having been audited
        /// </summary>
        /// <param name="assetID"></param>
        /// <returns></returns>
        public void AssetAudited(int aAssetID, DateTime aDateLastAudit)
        {
            if (isDebugEnabled) logger.Debug("AssetAudited in with asset id of : " + aAssetID);

            // which database type is this?
            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText = "UPDATE ASSETS SET _LASTAUDIT = @DateLastAudit WHERE _ASSETID = @ASSETID";

                        SqlCeCommand command = new SqlCeCommand(commandText, conn);
                        command.Parameters.AddWithValue("@ASSETID", aAssetID);
                        command.Parameters.AddWithValue("@DateLastAudit", aDateLastAudit);

                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                lAuditWizardDataAccess.AssetAudited(aAssetID, aDateLastAudit);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
        }

        /// <summary>
        /// Return a count of the number of licensed assets in the database.  
        /// </summary>
        /// <returns></returns>
        public int LicensedAssetCount()
        {
            int count = 0;

            try
            {
                string commandText =
                    "SELECT COUNT(a._ASSETID) " +
                    "FROM APPLICATIONS ap " +
                    "INNER JOIN APPLICATION_INSTANCES ai ON (ai._APPLICATIONID = ap._APPLICATIONID) " +
                    "INNER JOIN ASSETS a ON (a._ASSETID = ai._ASSETID) " +
                    "WHERE ap._ISOS = 1 " +
					"AND a._LASTAUDIT <> '' " +
					"AND a._STOCK_STATUS < 3 " +						// 8.4.1 CMD - Disposed of assets do not count towards the license count!!!		8.4.1 CMD
					"AND a._PARENT_ASSETID = 0";

                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            count = (int)command.ExecuteScalar();
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(commandText, conn))
                        {
                            count = (int)command.ExecuteScalar();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (SqlCeException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error adding the asset, see logfile for more detail.",
                    "AuditWizard", MessageBoxButtons.OK, MessageBoxIcon.Error);

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out with count : " + count);
            return count;
        }

        /// <summary>
        /// Delete the specified asset from the database
        /// </summary>
        /// <param name="theAsset"></param>
        /// <returns></returns>
        public void AssetDelete(Asset aAsset)
        {
            AssetDelete(aAsset.AssetID);
        }

        /// <summary>
        /// Delete the specified asset from the database
        /// </summary>
        /// <param name="theAsset"></param>
        /// <returns></returns>
        public int AssetDelete(int aAssetId)
        {
            if (isDebugEnabled) logger.Debug("AssetDelete in with asset id : " + aAssetId);
            int lAssetsPurged = 0;

            if (aAssetId != 0)
            {
                try
                {
                    string commandText = "SELECT _ASSETID FROM ASSETS WHERE _PARENT_ASSETID = '" + aAssetId + "'";

					// CMD 8.4.2 
					List<int> listChildAssets = new List<int>();

                    if (isCompactDatabase)
                    {
                        using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                        {
                            // stage one is to get all of the child records for this asset ID
                            using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                            {
                                using (SqlCeDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int childAssetID = reader.GetInt32(0);
										listChildAssets.Add(childAssetID);
                                    }
                                }
                            }
	
							// Now actually delete the child assets
							foreach (int childAssetID in listChildAssets)
							{
								if (isDebugEnabled) logger.Debug("deleting child asset id : " + childAssetID);
								lAssetsPurged += DeleteAssetReferences(childAssetID, conn);
							}

                            // then handle the parent
                            lAssetsPurged += DeleteAssetReferences(aAssetId, conn);
                        }
                    }
                    else
                    {
                        using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                        {
                            // stage one is to get all of the child records for this asset ID - note that we have to create a list and delete afterwards as we cannot
							// have the reader open when we actuall;y perform the delete
                            using (SqlCommand command = new SqlCommand(commandText, conn))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int childAssetID = reader.GetInt32(0);
										listChildAssets.Add(childAssetID);
                                    }
                                }
                            }
	
							// Now actually delete the child assets
							foreach (int childAssetID in listChildAssets)
							{
								if (isDebugEnabled) logger.Debug("deleting child asset id : " + childAssetID);
								lAssetsPurged += DeleteAssetReferences(childAssetID, conn);
							}

                            // then handle the parent
                            lAssetsPurged += DeleteAssetReferences(aAssetId, conn);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);

                    return -1;
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);

                    return -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error deleting the asset, see logfile for more detail.",
                        "AuditWizard", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);

                    return -1;
                }
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return lAssetsPurged;
        }

        private int DeleteAssetReferences(int assetID, SqlConnection conn)
        {
            int lRecordsPurged = 0;

            try
            {
                string commandText = "DELETE FROM APPLICATION_INSTANCES WHERE _ASSETID = '" + assetID + "'";
                SqlCommand deleteCommand = new SqlCommand(commandText, conn);

                lRecordsPurged = deleteCommand.ExecuteNonQuery();

                commandText =
                    "DELETE FROM APPLICATIONS " +
                    "WHERE _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM APPLICATION_INSTANCES) " +
                    "AND _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM LICENSES) " +
                    "AND _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM ACTIONS) " +
                    "AND _APPLICATIONID NOT IN (SELECT _ALIASED_TOID FROM APPLICATIONS) " +
                    "AND _ALIASED_TOID = 0 " +
                    "AND _ASSIGNED_FILEID = 0";

                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM AUDITEDITEMS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM AUDITTRAIL WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM DOCUMENTS WHERE _SCOPE=0 AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM NOTES WHERE _SCOPE=0 AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM OPERATIONS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM USERDATA_VALUES WHERE _PARENTTYPE IN (0, 2) AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM FS_FILES WHERE _ASSETID=" + assetID;
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM FS_FOLDERS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM ASSET_SUPPORTCONTRACT WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText =
                    "DELETE FROM ALERTS WHERE _ASSETNAME IN " +
                    "(SELECT _NAME FROM ASSETS WHERE _ASSETID = " + assetID + ")";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM ASSETS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lRecordsPurged;
        }

        private int DeleteAssetReferences(int assetID, SqlCeConnection conn)
        {
            int lRecordsPurged = 0;

            try
            {
                string commandText = "DELETE FROM APPLICATION_INSTANCES WHERE _ASSETID = '" + assetID + "'";
                SqlCeCommand deleteCommand = new SqlCeCommand(commandText, conn);

                lRecordsPurged = deleteCommand.ExecuteNonQuery();

                commandText =
                    "DELETE FROM APPLICATIONS " +
                    "WHERE _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM APPLICATION_INSTANCES) " +
                    "AND _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM LICENSES) " +
                    "AND _APPLICATIONID NOT IN (SELECT _APPLICATIONID FROM ACTIONS) " +
                    "AND _APPLICATIONID NOT IN (SELECT _ALIASED_TOID FROM APPLICATIONS) " +
                    "AND _ALIASED_TOID = 0 " +
                    "AND _ASSIGNED_FILEID = 0";

                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM AUDITEDITEMS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM AUDITTRAIL WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM DOCUMENTS WHERE _SCOPE=0 AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM NOTES WHERE _SCOPE=0 AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM OPERATIONS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM USERDATA_VALUES WHERE _PARENTTYPE IN (0, 2) AND _PARENTID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM FS_FILES WHERE _ASSETID=" + assetID;
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM FS_FOLDERS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText =
                    "DELETE FROM ALERTS WHERE _ASSETNAME IN " +
                    "(SELECT _NAME FROM ASSETS WHERE _ASSETID = " + assetID + ")";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();

                commandText = "DELETE FROM ASSETS WHERE _ASSETID = '" + assetID + "'";
                deleteCommand = new SqlCeCommand(commandText, conn);
                lRecordsPurged += deleteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lRecordsPurged;
        }


		/// <summary>
		/// +CMD 8.3.5
		/// Get a list of assets with the specified serial number noting that we discount some non-valid serial numbers 
		/// as it is always possible that a generic serial number has been returned by the audit
		/// </summary>
		/// <param name="aSerialNumber"></param>
		/// <returns></returns>
        private DataTable GetAssetsBySerialNumber(string validatedSerialNumber)
        {
			if (!string.IsNullOrEmpty(validatedSerialNumber))
			{
				string cmdText = String.Format("SELECT _ASSETID FROM ASSETS WHERE _SERIAL_NUMBER = '{0}'", validatedSerialNumber);
				return PerformQuery(cmdText);
			}

			return new DataTable();
        }

		private DataTable GetAssetsByNameAndDomain(string aAssetName, string aDomainName)
		{
			string cmdText =
						String.Format(
						"SELECT _ASSETID " +
						"FROM ASSETS a " +
						"INNER JOIN DOMAINS d ON (d._DOMAINID = a._DOMAINID) " +
						"WHERE a._NAME = '{0}' " +
						"AND d._NAME = '{1}'",
						aAssetName, aDomainName);

			return PerformQuery(cmdText);
		}
		

		/// <summary>
		/// Return a list of assets with the specified name noting that we also return the domain name
		/// </summary>
		/// <param name="aAssetName"></param>
		/// <returns></returns>
		private DataTable GetAssetsByNameOnly(string aAssetName)
		{
			string cmdText =
						String.Format(
						"SELECT _ASSETID, d._NAME " +
						"FROM ASSETS a " +
						"INNER JOIN DOMAINS d ON (d._DOMAINID = a._DOMAINID) " +
						"WHERE a._NAME = '{0}' ",
						aAssetName);

			return PerformQuery(cmdText);
		}

        private string GetUUIDByAssetID(int aAssetID)
        {
            string cmdText =
                        String.Format(
                        "SELECT _VALUE " +
                        "FROM AUDITEDITEMS " +
                        "WHERE _CATEGORY = 'Hardware|BIOS' " +
                        "AND _NAME = 'Internal System ID' " +
                        "AND _ASSETID = {0}", aAssetID);

            DataTable dt = PerformQuery(cmdText);
            return (dt.Rows.Count == 0) ? String.Empty : dt.Rows[0][0].ToString();
        }

        private DataTable GetUuidsBySerialModel(Asset aAsset)
        {
            string cmdText =
                        String.Format(
                        "SELECT a._ASSETID, ai._VALUE " +
                        "FROM ASSETS a " +
                        "INNER JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                        "WHERE a._SERIAL_NUMBER = '{0}' " +
                        "AND a._MODEL = '{1}' " +
                        "AND ai._CATEGORY = 'Hardware|BIOS' " +
                        "AND ai._NAME = 'Internal System ID'",
                        aAsset.SerialNumber, aAsset.Model);

            return PerformQuery(cmdText);
        }

        private DataTable GetUuidsBySerialModelName(Asset aAsset)
        {
            string cmdText =
                        String.Format(
                        "SELECT a._ASSETID, ai._VALUE " +
                        "FROM ASSETS a " +
                        "INNER JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                        "INNER JOIN DOMAINS d ON (d._DOMAINID = a._DOMAINID) " +
                        "WHERE a._SERIAL_NUMBER = '{0}' " +
                        "AND a._MODEL = '{1}' " +
                        "AND ai._CATEGORY = 'Hardware|BIOS' " +
                        "AND ai._NAME = 'Internal System ID' " +
                        "AND a._NAME = '{2}' " +
                        "AND d._NAME = '{3}'",
                        aAsset.SerialNumber, aAsset.Model, aAsset.Name, aAsset.Domain);

            return PerformQuery(cmdText);
        }

        private DataTable GetAssetBySerialUuidModel(Asset aAsset, string validatedSerialNumber)
        {
            string cmdText =
                        String.Format(
                        "SELECT _ASSETID " +
                        "FROM ASSETS " +
                        "WHERE _SERIAL_NUMBER = '{0}' " +
                        "AND _UNIQUEID = '{1}' " +
                        "AND _MODEL = '{2}'",
                        validatedSerialNumber, aAsset.UniqueID, aAsset.Model);

            DataTable lDataTable = PerformQuery(cmdText);

            // if the above query has returned one or more results, return the DataTable
            if (lDataTable.Rows.Count > 0) return lDataTable;

            // if the query returns zero rows it could either mean:
            // 1. new asset, or
            // 2. existing asset but it is pre-8.1.6 and has no UUID yet - we need to check for this

            foreach (DataRow row in GetUuidsBySerialModel(aAsset).Rows)
            {
                if (aAsset.UniqueID == row[1].ToString())
                {
                    lDataTable.ImportRow(row);
                }
            }

            return lDataTable;
        }


        private DataTable GetAssetBySerialDomainName(Asset aAsset, string validatedSerialNumber)
        {
            string cmdText =
                        String.Format(
                        "SELECT _ASSETID " +
                        "FROM ASSETS a " +
                        "INNER JOIN DOMAINS d ON (d._DOMAINID = a._DOMAINID) " +
                        "WHERE a._SERIAL_NUMBER = '{0}' " +
                        "AND a._NAME = '{2}' " +
                        "AND d._NAME = '{3}'",
                        validatedSerialNumber, aAsset.Model, aAsset.Name, aAsset.Domain);

            return PerformQuery(cmdText);
        }

        private DataTable GetAssetBySerialUuidDomainName(Asset aAsset, string validatedSerialNumber)
        {
            string cmdText =
                        String.Format(
                        "SELECT _ASSETID " +
                        "FROM ASSETS a " +
                        "INNER JOIN DOMAINS d ON (d._DOMAINID = a._DOMAINID) " +
                        "WHERE a._SERIAL_NUMBER = '{0}' " +
                        "AND a._UNIQUEID = '{1}' " +
						"AND a._NAME = '{3}' " +
                        "AND d._NAME = '{4}'",
                        validatedSerialNumber, aAsset.UniqueID, aAsset.Model, aAsset.Name, aAsset.Domain);

            DataTable lDataTable = PerformQuery(cmdText);

            // if the above query has returned one or more results, return the DataTable
            if (lDataTable.Rows.Count == 0) 
			{
	            // if the query returns zero rows it could either mean:
		        // 1. new asset, or
			    // 2. existing asset but it is pre-8.1.6 and has no UUID yet - we need to check for this
				// Note that we cannot check if we have an empty UUID as this would match multiple items
	            foreach (DataRow row in GetUuidsBySerialModelName(aAsset).Rows)
		        {
			        if (aAsset.UniqueID == row[1].ToString())
					    lDataTable.ImportRow(row);
	            }
			}
            return lDataTable;
        }


		/// <summary>
		/// Return a list of assets matching ONLY on the name of the asset
		/// </summary>
		/// <param name="aAsset"></param>
		/// <returns></returns>
        private DataTable GetAssetByName(Asset aAsset) 
        {
            string cmdText;
            if (aAsset.AgentVersion == "SNMP")
            {
                cmdText = String.Format("SELECT _ASSETID FROM ASSETS WHERE _NAME = '{0}' AND _ASSETTYPEID='{1}' AND _MACADDRESS='{2}'", aAsset.Name, aAsset.AssetTypeID, aAsset.MACAddress);
            }
            else
            {
                cmdText = String.Format("SELECT _ASSETID FROM ASSETS WHERE _NAME = '{0}'", aAsset.Name);
            }
            return PerformQuery(cmdText);
        }


		/// <summary>
		/// Return a list of assets matching on the IP and MAC addresses
		/// </summary>
		/// <param name="aAsset"></param>
		/// <returns></returns>
        private DataTable GetAssetByIPAndMac(Asset aAsset)
        {
            string cmdText;
            cmdText = String.Format("SELECT _ASSETID FROM ASSETS WHERE _IPADDRESS = '{0}'", aAsset.IPAddress);            
            return PerformQuery(cmdText);
        }
     



        /// <summary>
        /// 
        /// </summary>
        /// <param name="aAsset"></param>
        /// <returns></returns>
        public int AssetFind(Asset aAsset)
        {
            logger.Debug(String.Format("AssetFind in with name [{0}], domain [{1}], serial [{2}], uuid [{3}], make [{4}], model [{5}]",
                aAsset.Name, aAsset.Domain, aAsset.SerialNumber, aAsset.UniqueID, aAsset.Make, aAsset.Model));

			// Create an empty initial data table for matches
            DataTable dt = new DataTable();

            // if we are dealing with a USB device, we can only match on NAME
            // cannot allow this check for any other asset type as unreliable
            if ((aAsset.AgentVersion == "5.2.0.9 USB") || (aAsset.AgentVersion == "SNMP"))
            {
                logger.Debug(String.Format("USB device - can only try and match on name [{0}]", aAsset.Name));
                dt = GetAssetByName(aAsset);

                logger.Debug(String.Format("found {0} asset(s)", dt.Rows.Count));
                return (dt.Rows.Count == 1) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
            }

            //This section is added to deal with the duplication

            if (aAsset.AgentVersion == "DUP")
            {
                dt = GetAssetByIPAndMac(aAsset);
            }

            else if (aAsset.AgentVersion != "SNMP")
            {
                /* 
                 * Some assets have no useful data to use in the asset find process. This is often the result
                 * of been purchased from non-standard manufacturers, who don't add items such as UUID, make,
                 * model or serial number. As a concession to these users, a flag has been added to allow them 
                 * to specify that the asset find method will only use name and domain. We know this to be 
                 * limited (i.e. if the asset/domain name has been changed) but for some users this is the only option
                 */
                if (new SettingsDAO().GetSettingAsBoolean("FindAssetByName", false))
                {
                    logger.Debug("user has asked to use only name and domain to find asset");
                    dt = GetAssetsByNameAndDomain(aAsset.Name, aAsset.Domain);
                }

				else
				{
					// We are NOT using the simplified search algorithmn
					//
					// first test is via serial number
					// if we get zero results then we can assume this is a new asset
					// if we get one or more results we need to go into second level checks
					logger.Debug(String.Format("trying to find asset by serial number [{0}]", aAsset.SerialNumber));
					string validatedSerialNumber = ValidateSerialNumber(aAsset.SerialNumber);

					// If no serial number then we obviously cannot match on that
					if (!string.IsNullOrEmpty(validatedSerialNumber))
					{
						dt = GetAssetsBySerialNumber(validatedSerialNumber);
					}

					// OK have we matched any assets ?
					if (dt.Rows.Count == 0)
					{
						// If we have no matches on the serial number then we either have a new asset or the asset may exist in the database having
						// been discovered but not yet audited.  To this end we must do additional checks to find an asset with the same name as the
						// one being uploaded
						//
						// At this stage we cannot include domain as discovered assets often report a different domain than that returned via an audit
						// as domain suffixes may be returned such as LAYTON.LOCAL
						//
						// We will therefore first try and find a match on the asset name only
						logger.Debug(String.Format("found zero assets with this serial, now checking for name only ({0})", aAsset.Name));
						dt = GetAssetsByNameOnly(aAsset.Name);
						//
						if (dt.Rows.Count > 1)
						{
							// OK multiple matches to name - include domain to see if we can narror that down further
							logger.Debug(String.Format("found > 1 assets with this name, now checking for name and domain ({0}\\{1})", aAsset.Domain, aAsset.Name));
							dt = GetAssetsByNameAndDomain(aAsset.Name, aAsset.Domain);
						}
					}

					else if (dt.Rows.Count > 1)
					{
						// Multiple matches which is a pain as we would hope that the serial number is unique which is patently is not
						// We therefore expand the search to match on Serial Number, Model and name 
						// We do this prior to matching on UUID as it is common for UUID to not be returned in the audit.
						dt = GetAssetBySerialDomainName(aAsset, validatedSerialNumber);

						// If multiple matches then we obviously have not specified enough criteria to uniquely identify the asset
						// using Serial Number, Model and Name so extend to include the UUID
						if (dt.Rows.Count > 1)
						{
							dt = GetAssetBySerialUuidDomainName(aAsset, validatedSerialNumber);
						}

						else 
						{
							// We have 1 or 0 matches using Serial Number, Model and name
							// If we have specified a UUID then see if that will help
							if (aAsset.UniqueID != "")
								dt = GetAssetBySerialUuidModel(aAsset, validatedSerialNumber);
						}
					}
				}

				// Return the first asset identified if any
				logger.Debug(String.Format("found {0} asset(s)", dt.Rows.Count));
				return (dt.Rows.Count == 0) ? 0 : Convert.ToInt32(dt.Rows[0][0]);
			}

            if (isDebugEnabled)
                logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");

            return 0;
        }

		protected string ValidateSerialNumber(string serialNumber)
		{
			string validatedSerialNumber = serialNumber.Trim();

			// To be 'valid', the serial number must
			//	NOT BE blank or a space
			//  Must contain at least 2 numeric digits
			if (validatedSerialNumber.Length > 0)
			{
				int digitsCount = 0;
				foreach(char c in serialNumber)
				{
					if (Char.IsDigit(c))
						digitsCount++;
				}
				if (digitsCount >= 2)
					return validatedSerialNumber;
			}

			return "";
		}



        /// <summary>
        /// return the ID of the asset with the specified name only
        /// </summary>
        /// <param name="aAsset"></param>
        /// <returns></returns>
        public int AssetFind(string name)
        {
            logger.Debug(String.Format("AssetFind in with name [{0}]", name));

			string cmdText =
						String.Format(
						"SELECT _ASSETID " +
						"FROM ASSETS " +
						"WHERE ASSETS._NAME = '{0}' ",
						name);

			DataTable dt = PerformQuery(cmdText);
			return (dt.Rows.Count == 1) ? Convert.ToInt32(dt.Rows[0][0]) : 0;
		}


        /// <summary>
        /// Return the details of the specified asset
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Asset AssetGetDetails(int aAssetID)
        {
            if (isDebugEnabled) logger.Debug("AssetGetDetails in");

            DataTable table = new DataTable(TableNames.ASSETS);
            Asset returnAsset = null;

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "SELECT ASSETS.* " +
                            ", ASSET_TYPES._NAME AS ASSETTYPENAME " +
                            ", ASSET_TYPES._ICON AS ICON " +
                            ", ASSET_TYPES._AUDITABLE AS AUDITABLE " +
                            ", LOCATIONS._NAME AS LOCATIONNAME " +
                            ", LOCATIONS._FULLNAME AS FULLLOCATIONNAME " +
                            ", DOMAINS._NAME AS DOMAINNAME " +
                            ", SUPPLIERS._NAME AS SUPPLIER_NAME " +
                            "FROM ASSETS " +
                            "LEFT JOIN ASSET_TYPES ON (ASSETS._ASSETTYPEID=ASSET_TYPES._ASSETTYPEID) " +
                            "LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID=LOCATIONS._LOCATIONID) " +
                            "LEFT JOIN DOMAINS ON (ASSETS._DOMAINID=DOMAINS._DOMAINID) " +
                            "LEFT JOIN SUPPLIERS ON (ASSETS._SUPPLIERID = SUPPLIERS._SUPPLIERID) " +
                            "WHERE _ASSETID = @assetID";

                        SqlCeCommand command = new SqlCeCommand(commandText, conn);
                        command.Parameters.AddWithValue("@assetID", aAssetID);

                        new SqlCeDataAdapter(command).Fill(table);
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }

                // If we were returned any rows then create an asset from that returned
                if (table.Rows.Count == 1)
                    returnAsset = new Asset(table.Rows[0]);
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                returnAsset = lAuditWizardDataAccess.AssetGetDetails(aAssetID);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return returnAsset;
        }

        /// <summary>
        /// Flag the specified asset as being hidden (or clear this flag)
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="hide"></param>
        /// <returns></returns>
        public int AssetRequestAudit(int aAssetID, bool aSetOrClear)
        {
            if (isDebugEnabled) logger.Debug("AssetRequestAudit in");

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText = "UPDATE ASSETS SET _REQUESTAUDIT = @SetOrClear WHERE _ASSETID = @AssetID";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@SetOrClear", Convert.ToInt32(aSetOrClear));
                            command.Parameters.AddWithValue("@AssetID", aAssetID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                lAuditWizardDataAccess.AssetRequestAudit(aAssetID, aSetOrClear);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return 0;
        }


        /// <summary>
        /// Update the definition stored for the specified asset
        /// </summary>
        /// <param name="aAsset"></param>
        /// <returns></returns>
        public void AssetUpdate(Asset aAsset)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            aAsset.Make = RationaliseManufacturerNames(aAsset.Make);

            try
            {
                string commandText =
                            "UPDATE ASSETS SET _NAME = @Name" +
                            ", _UNIQUEID = @UniqueID" +
                            ", _LOCATIONID = @LocationID" +
                            ",_DOMAINID = @DomainID" +
                            ",_IPADDRESS = @IPAddress" +
                            ",_MACADDRESS = @MACAddress" +
                            ",_ASSETTYPEID = @AssetTypeID" +
                            ",_MAKE = @Make" +
                            ",_MODEL = @Model" +
                            ",_SERIAL_NUMBER = @SerialNumber" +
                            ",_PARENT_ASSETID = @ParentAssetID" +
                            ",_SUPPLIERID = @SupplierID" +
                            ",_STOCK_STATUS = @StockStatus" +
                            ",_AGENT_VERSION = @AgentVersion" +
                            ",_ALERTS_ENABLED = @AlertsEnabled " +
                            ",_ASSETTAG = @AssetTag " +
                            ",_OVERWRITEDATA = @OverwriteData " +
                            "WHERE _ASSETID=@nAssetID";

                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@nAssetID", aAsset.AssetID);
                            command.Parameters.AddWithValue("@Name", aAsset.Name);
                            command.Parameters.AddWithValue("@UniqueID", aAsset.UniqueID);
                            command.Parameters.AddWithValue("@LocationID", aAsset.LocationID);
                            command.Parameters.AddWithValue("@DomainID", aAsset.DomainID);
                            command.Parameters.AddWithValue("@IPAddress", aAsset.IPAddress);
                            command.Parameters.AddWithValue("@MACAddress", aAsset.MACAddress);
                            command.Parameters.AddWithValue("@AssetTypeID", aAsset.AssetTypeID);
                            command.Parameters.AddWithValue("@Make", aAsset.Make);
                            command.Parameters.AddWithValue("@Model", aAsset.Model);
                            command.Parameters.AddWithValue("@SerialNumber", aAsset.SerialNumber);
                            command.Parameters.AddWithValue("@ParentAssetID", aAsset.ParentAssetID);
                            command.Parameters.AddWithValue("@SupplierID", aAsset.SupplierID);
                            command.Parameters.AddWithValue("@StockStatus", aAsset.StockStatus);
                            command.Parameters.AddWithValue("@AgentVersion", aAsset.AgentVersion);
                            command.Parameters.AddWithValue("@AlertsEnabled", aAsset.AlertsEnabled);
                            command.Parameters.AddWithValue("@AssetTag", aAsset.AssetTag);
                            command.Parameters.AddWithValue("@OverwriteData", aAsset.OverwriteData);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@nAssetID", aAsset.AssetID);
                            command.Parameters.AddWithValue("@Name", aAsset.Name);
                            command.Parameters.AddWithValue("@UniqueID", aAsset.UniqueID);
                            command.Parameters.AddWithValue("@LocationID", aAsset.LocationID);
                            command.Parameters.AddWithValue("@DomainID", aAsset.DomainID);
                            command.Parameters.AddWithValue("@IPAddress", aAsset.IPAddress);
                            command.Parameters.AddWithValue("@MACAddress", aAsset.MACAddress);
                            command.Parameters.AddWithValue("@AssetTypeID", aAsset.AssetTypeID);
                            command.Parameters.AddWithValue("@Make", aAsset.Make);
                            command.Parameters.AddWithValue("@Model", aAsset.Model);
                            command.Parameters.AddWithValue("@SerialNumber", aAsset.SerialNumber);
                            command.Parameters.AddWithValue("@ParentAssetID", aAsset.ParentAssetID);
                            command.Parameters.AddWithValue("@SupplierID", aAsset.SupplierID);
                            command.Parameters.AddWithValue("@StockStatus", aAsset.StockStatus);
                            command.Parameters.AddWithValue("@AgentVersion", aAsset.AgentVersion);
                            command.Parameters.AddWithValue("@AlertsEnabled", aAsset.AlertsEnabled);
                            command.Parameters.AddWithValue("@AssetTag", aAsset.AssetTag);
                            command.Parameters.AddWithValue("@OverwriteData", aAsset.OverwriteData);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlCeException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }


            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
        }

        /// <summary>
        /// Update the Deployment Status stored for the specified asset
        /// </summary>
        /// <param name="theAsset"></param>
        /// <returns></returns>
        public int AssetUpdateAssetStatus(int aAssetID, Asset.AGENTSTATUS aNewStatus)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "UPDATE ASSETS SET _AGENT_STATUS = @NewStatus WHERE _ASSETID = @AssetID";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@NewStatus", (int)aNewStatus);
                            command.Parameters.AddWithValue("@AssetID", aAssetID);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                return lAuditWizardDataAccess.AssetUpdateAssetStatus(aAssetID, aNewStatus);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return 0;
        }



        /// <summary>
        /// Update the Stock Status stored for the specified asset
        /// </summary>
        /// <param name="theAsset"></param>
        /// <returns></returns>
        public int AssetUpdateStockStatus(int aAssetID, Asset.STOCKSTATUS aNewStatus)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "UPDATE ASSETS SET _STOCK_STATUS = @NewStatus WHERE _ASSETID = @AssetID";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@NewStatus", (int)aNewStatus);
                            command.Parameters.AddWithValue("@AssetID", aAssetID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                lAuditWizardDataAccess.AssetUpdateStockStatus(aAssetID, aNewStatus);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return 0;
        }

		//public DataTable FindAssetByName(string aSearchString, string aAssetIDList)
		//{
		//    string commandText = String.Format(
		//            "SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._NAME a " +
		//            "FROM ASSETS " +
		//            "LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
		//            "WHERE ASSETS._NAME LIKE ('{0}') " +
		//            "AND ASSETS._ASSETID IN ({1}) " +
		//            "ORDER BY ASSETS._NAME", aSearchString, aAssetIDList);

		//    return PerformQuery(commandText);
		//}


		public DataTable FindAssetByName(string aSearchString, string aGroupIDList)
		{
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._NAME a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._NAME LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._NAME a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._NAME LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

			return PerformQuery(commandText);
		}


		public DataTable FindAssetByTag(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._ASSETTAG a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._ASSETTAG LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._ASSETTAG a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._ASSETTAG LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByMake(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MAKE a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MAKE LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MAKE a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MAKE LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByModel(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MODEL a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MODEL LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MODEL a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MODEL LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetBySerialNumber(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._SERIAL_NUMBER a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._SERIAL_NUMBER LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._SERIAL_NUMBER a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._SERIAL_NUMBER LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByIPAddress(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._IPADDRESS a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._IPADDRESS LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._IPADDRESS a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._IPADDRESS LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByMACAddress(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MACADDRESS a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MACADDRESS LIKE ('{0}') " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, ASSETS._MACADDRESS a " +
					"FROM ASSETS " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE ASSETS._MACADDRESS LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByUserData(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
                    "SELECT ASSETS._NAME, LOCATIONS._FULLNAME, uv._VALUE " +
                    "FROM USERDATA_VALUES uv " +
                    "LEFT JOIN ASSETS ON (ASSETS._ASSETID = uv._PARENTID) " +
                    "LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
                    "WHERE uv._VALUE LIKE ('{0}') " +
                    "ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME, LOCATIONS._FULLNAME, uv._VALUE " +
					"FROM USERDATA_VALUES uv " +
					"LEFT JOIN ASSETS ON (ASSETS._ASSETID = uv._PARENTID) " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE uv._VALUE LIKE ('{0}') " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByHardware(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
                    "SELECT a._NAME, l._FULLNAME, ai._CATEGORY + '|' + ai._NAME + '|' + ai._VALUE " +
                    "FROM ASSETS a " +
                    "LEFT JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "WHERE ai._CATEGORY LIKE 'Hardware|%' " +
                    "AND ai._VALUE LIKE ('{0}')" +
                    "ORDER BY a._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
                    "SELECT a._NAME, l._FULLNAME, ai._CATEGORY + '|' + ai._NAME + '|' + ai._VALUE " +
                    "FROM ASSETS a " +
                    "LEFT JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "WHERE ai._CATEGORY LIKE 'Hardware|%' " +
                    "AND ai._VALUE LIKE ('{0}')" +
					"AND a._LOCATIONID IN ({1}) " +
					"ORDER BY a._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByFiles(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
                    "SELECT a._NAME, l._FULLNAME, fl._NAME + '\\' + f._NAME " +
                    "FROM FS_FILES f " +
                    "LEFT JOIN ASSETS a ON (a._ASSETID = f._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "INNER JOIN FS_FOLDERS fl ON (fl._FOLDERID = f._PARENTID) " +
                    "WHERE f._NAME LIKE ('{0}')" +
                    "ORDER BY a._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT a._NAME, l._FULLNAME, fl._NAME + '\\' + f._NAME " +
					"FROM FS_FILES f " +
					"LEFT JOIN ASSETS a ON (a._ASSETID = f._ASSETID) " +
					"LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
					"INNER JOIN FS_FOLDERS fl ON (fl._FOLDERID = f._PARENTID) " +
					"WHERE f._NAME LIKE ('{0}')" +
					"AND a._LOCATIONID IN ({1}) " +
					"ORDER BY a._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByInternet(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
                    "SELECT ASSETS._NAME AS ASSETNAME, LOCATIONS._FULLNAME, _CATEGORY " +
                    "FROM AUDITEDITEMS " +
                    "LEFT JOIN ASSETS ON (AUDITEDITEMS._ASSETID = ASSETS._ASSETID) " +
                    "LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
                    "WHERE AUDITEDITEMS._ASSETID <> 0 " +
                    "AND AUDITEDITEMS._CATEGORY LIKE 'Internet|History|%' " +
                    "AND AUDITEDITEMS._CATEGORY LIKE '{0}' " +
					"ORDER BY ASSETS._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT ASSETS._NAME AS ASSETNAME, LOCATIONS._FULLNAME, _CATEGORY " +
					"FROM AUDITEDITEMS " +
					"LEFT JOIN ASSETS ON (AUDITEDITEMS._ASSETID = ASSETS._ASSETID) " +
					"LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID = LOCATIONS._LOCATIONID) " +
					"WHERE AUDITEDITEMS._ASSETID <> 0 " +
					"AND AUDITEDITEMS._CATEGORY LIKE 'Internet|History|%' " +
					"AND AUDITEDITEMS._CATEGORY LIKE '{0}' " +
					"AND ASSETS._LOCATIONID IN ({1}) " +
					"ORDER BY ASSETS._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByApplicationName(string aSearchString, string aGroupIDList)
        {
			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
				commandText = String.Format(
                    "SELECT a._NAME as \"Asset Name\", l._FULLNAME as \"Location\", ap._NAME \"Application Name\" " +
                    "FROM APPLICATIONS ap " +
                    "LEFT JOIN APPLICATION_INSTANCES ai ON (ai._APPLICATIONID = ap._APPLICATIONID) " +
                    "LEFT JOIN ASSETS a ON (a._ASSETID = ai._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "WHERE ap._NAME LIKE '{0}'" +
                    "ORDER BY a._NAME", aSearchString);
			}
			else
			{
				commandText = String.Format(
					"SELECT a._NAME as \"Asset Name\", l._FULLNAME as \"Location\", ap._NAME \"Application Name\" " +
					"FROM APPLICATIONS ap " +
					"LEFT JOIN APPLICATION_INSTANCES ai ON (ai._APPLICATIONID = ap._APPLICATIONID) " +
					"LEFT JOIN ASSETS a ON (a._ASSETID = ai._ASSETID) " +
					"LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
					"WHERE ap._NAME LIKE '{0}'" +
					"AND a._LOCATIONID IN ({1}) " +
					"ORDER BY a._NAME", aSearchString, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

		public DataTable FindAssetByApplicationNameAndVersion(string aApplicationName, string aApplicationVersion, string aGroupIDList)
        {
 			string commandText;
			if (String.IsNullOrEmpty(aGroupIDList))
			{
 				commandText = String.Format(
                   "SELECT a._NAME as \"Asset Name\", l._FULLNAME as \"Location\", ap._NAME \"Application Name\" " +
                    "FROM APPLICATIONS ap " +
                    "LEFT JOIN APPLICATION_INSTANCES ai ON (ai._APPLICATIONID = ap._APPLICATIONID) " +
                    "LEFT JOIN ASSETS a ON (a._ASSETID = ai._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "WHERE ap._NAME LIKE '{0}'" +
                    "AND ap._VERSION = '{1}' " +
                    "ORDER BY a._NAME", aApplicationName, aApplicationVersion);
			}
			else
			{
				commandText = String.Format(
					"SELECT a._NAME as \"Asset Name\", l._FULLNAME as \"Location\", ap._NAME \"Application Name\" " +
                    "FROM APPLICATIONS ap " +
                    "LEFT JOIN APPLICATION_INSTANCES ai ON (ai._APPLICATIONID = ap._APPLICATIONID) " +
                    "LEFT JOIN ASSETS a ON (a._ASSETID = ai._ASSETID) " +
                    "LEFT JOIN LOCATIONS l ON (l._LOCATIONID = a._LOCATIONID) " +
                    "WHERE ap._NAME LIKE '{0}'" +
                    "AND ap._VERSION = '{1}' " +
					"AND a._LOCATIONID IN ({2}) " +
					"ORDER BY a._NAME", aApplicationName, aApplicationVersion, aGroupIDList);
			}

            return PerformQuery(commandText);
        }

        public DataTable GetAssetNameById(int assetID)
        {
            string commandText = "SELECT _NAME FROM ASSETS WHERE _ASSETID = " + assetID;
            return PerformQuery(commandText);
        }

        public DataTable GetAssetNamesByIds(string aAssetIDs)
        {
            string commandText =
                "SELECT _ASSETID, _NAME " +
                "FROM ASSETS " +
                "WHERE _ASSETID IN (" + aAssetIDs + ")";

            return PerformQuery(commandText);
        }

        public DataTable GetAllAssetIds()
        {
            string commandText = "SELECT _ASSETID FROM ASSETS";
            return PerformQuery(commandText);
        }

        public DataTable ApplicationsByAsset(bool showIncluded, bool showIgnored)
        {
            string commandText =
                "SELECT A._NAME AS \"Asset Name\", APP._NAME AS \"Application\", APP._VERSION AS \"Version\" " +
                "FROM ASSETS A " +
                "INNER JOIN APPLICATION_INSTANCES AI ON (A._ASSETID = AI._ASSETID) " +
                "INNER JOIN APPLICATIONS APP ON (AI._APPLICATIONID = APP._APPLICATIONID) " +
                "WHERE A._ASSETID IN (" + new AssetDAO().GetSelectedAssets() + ") ";

            if (showIncluded && !showIgnored)
                commandText += "AND APP._IGNORED = 0 ";

            else if (!showIncluded && showIgnored)
                commandText += "AND APP._IGNORED = 1 ";

            commandText += "ORDER BY A._NAME, APP._NAME";

            return PerformQuery(commandText);
        }

        public string GetAllAssetIdsAsString()
        {
            StringBuilder sb = new StringBuilder();

            DataTable lAllAssetDataTable = new AssetDAO().GetAllAssetIds();
            foreach (DataRow assetRow in lAllAssetDataTable.Rows)
            {
                sb.Append(assetRow[0] + ",");
            }

            return sb.ToString().TrimEnd(',');
        }

        /// <summary>
        /// Return a table containing the definitions for assets within the specified group
        /// optionally filtering out those whose status does not match that requested
        /// </summary>
        /// <returns></returns>
        public DataTable GetAssets(int aForGroupID, AssetGroup.GROUPTYPE aGroupType, bool aApplyStates)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable table = new DataTable(TableNames.ASSETS);

            if (isCompactDatabase)
            {
                bool showStock = false;
                bool showInUse = true;
                bool showPending = false;
                bool showDisposed = false;

                // Are we applying states?  If not then request ALL states
                if (aApplyStates)
                {
                    // Get the current 'show' flags
                    Configuration config = ConfigurationManager.OpenExeConfiguration(Path.Combine(Application.StartupPath, "AuditWizardv8.exe"));
                    try
                    {
                        showStock = Convert.ToBoolean(config.AppSettings.Settings["ShowStock"].Value);
                        showInUse = Convert.ToBoolean(config.AppSettings.Settings["ShowInUse"].Value);
                        showPending = Convert.ToBoolean(config.AppSettings.Settings["ShowPending"].Value);
                        showDisposed = Convert.ToBoolean(config.AppSettings.Settings["ShowDisposed"].Value);
                    }
                    catch (Exception)
                    { }
                }

                else
                {
                    showStock = showInUse = showPending = showDisposed = true;
                }

                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "SELECT ASSETS.* " +
                            ", ASSET_TYPES._NAME AS ASSETTYPENAME " +
                            ", ASSET_TYPES._ICON AS ICON " +
                            ", ASSET_TYPES._AUDITABLE AS AUDITABLE " +
                            ", LOCATIONS._NAME AS LOCATIONNAME " +
                            ", LOCATIONS._FULLNAME AS FULLLOCATIONNAME " +
                            ", DOMAINS._NAME AS DOMAINNAME " +
                            ", SUPPLIERS._NAME AS SUPPLIER_NAME " +
                            "FROM ASSETS " +
                            "LEFT JOIN ASSET_TYPES ON (ASSETS._ASSETTYPEID=ASSET_TYPES._ASSETTYPEID) " +
                            "LEFT JOIN LOCATIONS ON (ASSETS._LOCATIONID=LOCATIONS._LOCATIONID) " +
                            "LEFT JOIN DOMAINS ON (ASSETS._DOMAINID=DOMAINS._DOMAINID) " +
                            "LEFT JOIN SUPPLIERS ON (ASSETS._SUPPLIERID = SUPPLIERS._SUPPLIERID) " +
                            "WHERE ASSETS._PARENT_ASSETID = 0";

                        if (aGroupType == AssetGroup.GROUPTYPE.userlocation)
                        {
                            if (aForGroupID != 0)
                                commandText += " AND ASSETS._LOCATIONID = " + aForGroupID;
                        }
                        else if (aGroupType == AssetGroup.GROUPTYPE.domain)
                        {
                            if (aForGroupID != 0)
                                commandText += " AND ASSETS._DOMAINID = " + aForGroupID;
                        }

                        bool whereClause = false;

                        if (showStock)
                        {
                            whereClause = true;
                            commandText += " AND (_STOCK_STATUS = 0";
                        }

                        if (showInUse)
                        {
                            if (!whereClause)
                                commandText += " AND (_STOCK_STATUS = 1";
                            else
                                commandText += " OR _STOCK_STATUS = 1";

                            whereClause = true;
                        }

                        if (showPending)
                        {
                            if (!whereClause)
                                commandText += " AND (_STOCK_STATUS = 2";
                            else
                                commandText += " OR _STOCK_STATUS = 2";

                            whereClause = true;
                        }

                        if (showDisposed)
                        {
                            if (!whereClause)
                                commandText += " AND (_STOCK_STATUS = 3";
                            else
                                commandText += " OR _STOCK_STATUS = 3";

                            whereClause = true;
                        }

                        if (whereClause)
                            commandText += ")";

                        commandText += " ORDER BY ASSETS._NAME";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            if (whereClause)
                                new SqlCeDataAdapter(command).Fill(table);
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                table = lAuditWizardDataAccess.GetAssets(aForGroupID, aGroupType, aApplyStates);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return table;
        }



        /// <summary>
        /// Return a table containing all of the Child Assets for an asset
        /// </summary>
        /// <returns></returns>
        public DataTable EnumerateChildAssets(int aAssetID)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in with id : " + aAssetID);

            DataTable table = new DataTable(TableNames.ASSETS);

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "SELECT ASSETS.* " +
                            ", ASSET_TYPES._NAME AS ASSETTYPENAME " +
                            ", ASSET_TYPES._ICON AS ICON " +
                            ", ASSET_TYPES._AUDITABLE AS AUDITABLE " +
                            ", LOCATIONS._NAME AS LOCATIONNAME " +
                            ", LOCATIONS._FULLNAME AS FULLLOCATIONNAME " +
                            ", DOMAINS._NAME AS DOMAINNAME " +
                            ", SUPPLIERS._NAME AS SUPPLIER_NAME " +
                            "FROM ASSETS " +
                            "LEFT JOIN ASSET_TYPES ON (ASSETS._ASSETTYPEID=ASSET_TYPES._ASSETTYPEID) " +
                            "LEFT JOIN LOCATIONS   ON (ASSETS._LOCATIONID=LOCATIONS._LOCATIONID) " +
                            "LEFT JOIN DOMAINS	  ON (ASSETS._DOMAINID=DOMAINS._DOMAINID) " +
                            "LEFT JOIN SUPPLIERS   ON (ASSETS._SUPPLIERID = SUPPLIERS._SUPPLIERID) " +
                            "WHERE ASSETS._PARENT_ASSETID = @AssetID";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@AssetID", aAssetID);
                            new SqlCeDataAdapter(command).Fill(table);
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                table = lAuditWizardDataAccess.EnumerateChildAssets(aAssetID);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return table;
        }


        /// <summary>
        /// Return a table containing all of the Assets which have the AuditAgent deployed to them
        /// </summary>
        /// <returns></returns>
        public DataTable EnumerateDeployedAssets()
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable table = new DataTable(TableNames.ASSETS);

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                            "SELECT ASSETS.* " +
                            ", ASSET_TYPES._NAME AS ASSETTYPENAME " +
                            ", ASSET_TYPES._ICON AS ICON " +
                            ", ASSET_TYPES._AUDITABLE AS AUDITABLE " +
                            ", LOCATIONS._NAME AS LOCATIONNAME " +
                            ", LOCATIONS._FULLNAME AS FULLLOCATIONNAME " +
                            ", DOMAINS._NAME AS DOMAINNAME " +
                            ", SUPPLIERS._NAME AS SUPPLIER_NAME " +
                            "FROM ASSETS " +
                            "LEFT JOIN ASSET_TYPES ON (ASSETS._ASSETTYPEID=ASSET_TYPES._ASSETTYPEID) " +
                            "LEFT JOIN LOCATIONS   ON (ASSETS._LOCATIONID=LOCATIONS._LOCATIONID) " +
                            "LEFT JOIN DOMAINS	  ON (ASSETS._DOMAINID=DOMAINS._DOMAINID) " +
                            "LEFT JOIN SUPPLIERS   ON (ASSETS._SUPPLIERID = SUPPLIERS._SUPPLIERID) " +
                            "WHERE _AGENT_STATUS <> 0";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            new SqlCeDataAdapter(command).Fill(table);
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                table = lAuditWizardDataAccess.EnumerateDeployedAssets();
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return table;
        }

        public DateTime GetLastAuditDate(int assetID)
        {
            DateTime returnDateTime = DateTime.MinValue;

            string commandText = String.Format(
                "select _lastaudit from assets " +
                "where _assetid = {0}", assetID);

            DataTable table = PerformQuery(commandText);

            if (table.Rows.Count > 0)
            {
                if (table.Rows[0].ItemArray[0].GetType() != typeof(DBNull))
                    returnDateTime = Convert.ToDateTime(table.Rows[0].ItemArray[0]);
            }

            return returnDateTime;
        }

        private DataTable PerformQuery(string aCommandText)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable statisticsTable = new DataTable();
            try
            {
                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(aCommandText, conn))
                        {
                            new SqlCeDataAdapter(command).Fill(statisticsTable);
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(aCommandText, conn))
                        {
                            new SqlDataAdapter(command).Fill(statisticsTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 701 || ex.Number == 8623)
                    throw ex;

                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (SqlCeException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }


            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return statisticsTable;
        }

        private DataTable PerformQuery2(string aCommandText, SqlConnection conn)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable statisticsTable = new DataTable();
            try
            {
                using (SqlCommand command = new SqlCommand(aCommandText, conn))
                {
                    new SqlDataAdapter(command).Fill(statisticsTable);
                }
            }
            catch (SqlException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return statisticsTable;
        }

        private DataTable PerformQuery1(string aCommandText, SqlCeConnection conn)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable statisticsTable = new DataTable();
            try
            {
                using (SqlCeCommand command = new SqlCeCommand(aCommandText, conn))
                {
                    new SqlCeDataAdapter(command).Fill(statisticsTable);
                }
            }
            catch (SqlCeException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return statisticsTable;
        }

        private string PerformScalarQuery(string commandText)
        {
            string returnValue = String.Empty;

            try
            {
                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            returnValue = command.ExecuteScalar().ToString();
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(commandText, conn))
                        {
                            returnValue = command.ExecuteScalar().ToString();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (SqlCeException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            catch (Exception ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");

                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }

            return returnValue;
        }

        //public DataTable GetCompliantAssetValue(string assetCriteria, string assetValue, string compliantIds)
        //{
        //    string commandText = String.Format(
        //        "select _assetid " +
        //        "from assets " +
        //        "where {0} = {1} " +
        //        "and _assetid in ({2})", assetCriteria, assetValue, compliantIds);

        //    return PerformQuery(commandText);
        //}

        public DataTable GetCompliantAssetValue(string assetCriteria, string assetValue)
        {
            string commandText = String.Format(
                "select _assetid " +
                "from assets " +
                "where {0} = {1}", assetCriteria, assetValue);

            return PerformQuery(commandText);
        }

        public DataTable GetCompliantAssetValueForLastAudit(string whereClause)
        {
            string commandText = String.Format(
                "select _assetid " +
                "from assets " +
                "where _lastaudit {0}", whereClause);

            return PerformQuery(commandText);
        }

        public DataTable GetAssetPickerValues(string columnName)
        {
            string commandText = String.Format(
                "select distinct {0} " +
                "from assets", columnName);

            return PerformQuery(commandText);
        }

        public DataTable GetAssetPickerValuesForLastAudit()
        {
            string commandText =
                "SELECT DISTINCT CONVERT(nvarchar(30), _lastaudit, 20) " +
                "FROM assets " +
                "ORDER BY CONVERT(nvarchar(30), _lastaudit, 20)";

            return PerformQuery(commandText);
        }

        public DataTable RunCustomUnionStatement(string aCommandText)
        {
            return PerformQuery(aCommandText);
        }

        public DataTable RunCustomUnionStatement(string aCommandText, SqlCeConnection conn)
        {
            return PerformQuery1(aCommandText, conn);
        }

        public DataTable RunCustomUnionStatement(string aCommandText, SqlConnection conn)
        {
            return PerformQuery2(aCommandText, conn);
        }

        public string GetCompliantAssetValueByAssetId(string complianceField, int assetID)
        {
            string commandText = String.Format(
                "select {0} " +
                "from assets " +
                "where _assetid = {1}", complianceField, assetID);

            return PerformScalarQuery(commandText);
        }

        public DataTable GetCompliantAssetValues(string complianceField, string assetIDs)
        {
            string commandText = String.Format(
                "select {0} " +
                "from assets " +
                "where _assetid in ({1})", complianceField, assetIDs);

            return PerformQuery(commandText);
        }

        public DataTable GetNonCompliantAssetIds(string compliantAssetIds, string selectedAssetIds)
        {
            string commandText;

            if (compliantAssetIds == String.Empty)
                commandText = "SELECT _ASSETID FROM ASSETS WHERE _ASSETID IN (" + selectedAssetIds + ")";
            else
            {
                commandText =
                    "SELECT _ASSETID FROM ASSETS " +
                    "WHERE _ASSETID IN (" + selectedAssetIds + ")" +
                    "AND _ASSETID NOT IN (" + compliantAssetIds + ")";
            }

            return PerformQuery(commandText);
        }

        public string ConvertNameListToIds(string aNameList)
        {
            StringBuilder lReturnString = new StringBuilder();

            // format the list into valid SQL param
            aNameList = "'" + aNameList.Replace(";", "', '") + "'";
            DataTable dt = PerformQuery("SELECT _ASSETID FROM ASSETS WHERE _NAME IN (" + aNameList + ")");

            foreach (DataRow row in dt.Rows)
            {
                lReturnString.Append(row[0] + ";");
            }

            return lReturnString.ToString().TrimEnd(';');
        }

        public string ConvertIdListToNames(string aIdList, char aSeperator)
        {
            StringBuilder lReturnString = new StringBuilder();
            aIdList = aIdList.Replace(',', ';');
            aIdList = "'" + aIdList.Replace(";", "', '") + "'";

            DataTable dt = PerformQuery("SELECT _NAME FROM ASSETS WHERE _ASSETID IN (" + aIdList + ")");

            foreach (DataRow row in dt.Rows)
            {
                lReturnString.Append(row[0] + ";");
            }

            return lReturnString.ToString().TrimEnd(';');
        }

        public string GetSearchLocationAssets()
        {
            string lAssets = new LocationSettingsDAO().GetSetting("SearchLocationFilter");

            if (lAssets == "")
            {
                StringBuilder sb = new StringBuilder();
                DataTable lAllAssetDataTable = new AssetDAO().GetAllAssetIds();

                foreach (DataRow assetRow in lAllAssetDataTable.Rows)
                {
                    //lAssets += assetRow[0].ToString() + ",";
                    sb.Append(assetRow[0] + ",");
                }

                //lAssets = lAssets.TrimEnd(',');
                lAssets = sb.ToString().TrimEnd(',');
            }

            string commandText = String.Format(
                "SELECT _ASSETID " +
                "FROM ASSETS " +
                "WHERE _ASSETID IN ({0}) " +
                "AND _ASSETID NOT IN " +
                "(SELECT _ASSETID " +
                "FROM ASSETS " +
                "WHERE _STOCK_STATUS = 3)", lAssets);

            //string lReturnedIds = "";
            StringBuilder lReturnedIds = new StringBuilder();

            DataTable lRequiredAssetsDataTable = PerformQuery(commandText);

            foreach (DataRow assetRow in lRequiredAssetsDataTable.Rows)
            {
                //lReturnedIds += assetRow[0].ToString() + ",";
                lReturnedIds.Append(assetRow[0] + ",");
            }

            //lReturnedIds = lReturnedIds.TrimEnd(',');
            //return lReturnedIds;

            return lReturnedIds.ToString().TrimEnd(',');
        }

        public string GetSelectedAssets()
        {
            return GetSelectedAssets(false);
        }

        public string GetSelectedAssets(bool includeDisposed)
        {
            // get any selected assets
            string lAssets = new LocationSettingsDAO().GetSetting("LocationFilter");

            // now get any selected groups, ensuring we get any child groups
            string selectedGroups = Utility.ListToString(new LocationsDAO().GetChildLocationIds(new LocationSettingsDAO().GetSetting("LocationFilterGroups")), ',');

            string cmdText =
                "SELECT _ASSETID " +
                "FROM ASSETS ";

            cmdText += includeDisposed ? "WHERE _STOCK_STATUS <> 100 " : "WHERE _STOCK_STATUS <> 3 ";

            if (selectedGroups != "")
            {
                cmdText += " AND _LOCATIONID IN (" + selectedGroups + ") ";

                if (lAssets != "")
                    cmdText += "OR _ASSETID IN (" + lAssets + ") ";
            }
            else
            {
                if (lAssets != "")
                    cmdText += "AND _ASSETID IN (" + lAssets + ") ";
            }

            StringBuilder sb = new StringBuilder();

            DataTable lRequiredAssetsDataTable = PerformQuery(cmdText);

            foreach (DataRow assetRow in lRequiredAssetsDataTable.Rows)
            {
                sb.Append(assetRow[0] + ",");
            }

            lAssets = sb.ToString().TrimEnd(',');

            if (lAssets == "")
            {
                sb = new StringBuilder();
                DataTable lAllAssetDataTable = new AssetDAO().GetAllAssetIds();
                foreach (DataRow assetRow in lAllAssetDataTable.Rows)
                {
                    sb.Append(assetRow[0] + ",");
                }

                lAssets = sb.ToString().TrimEnd(',');
            }

            return lAssets;
        }

        public void UpdateEmptyUuidValues()
        {
            string cmdText = "";

            PerformQuery(cmdText);
        }

        public DataTable GetAssetMakesByType(int assetTypeId)
        {
            string cmdText =
                "SELECT DISTINCT _MAKE " +
                "FROM ASSETS " +
                "WHERE _ASSETTYPEID = " + assetTypeId + " " +
                "ORDER BY _MAKE";

            return PerformQuery(cmdText);
        }

        public DataTable GetAssetModelsByType(int assetTypeId)
        {
            string cmdText =
                "SELECT DISTINCT _MODEL " +
                "FROM ASSETS " +
                "WHERE _ASSETTYPEID = " + assetTypeId + " " +
                "ORDER BY _MODEL";

            return PerformQuery(cmdText);
        }

        public DataTable GetInstalledBrowserAssetId(string browserName)
        {
            string cmdText =
                "SELECT a._ASSETID " +
                "FROM ASSETS a " +
                "INNER JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                "WHERE _CATEGORY = 'Internet|" + browserName + "' " +
                "AND ai._NAME = 'Default Browser' " +
                "AND ai._VALUE = 'Yes' ";

            return PerformQuery(cmdText);
        }

        public DataTable GetBrowserVersionAssetId(string browserName, string browserVersion)
        {
            string cmdText =
                "SELECT a._ASSETID " +
                "FROM ASSETS a " +
                "INNER JOIN AUDITEDITEMS ai ON (ai._ASSETID = a._ASSETID) " +
                "WHERE _CATEGORY = 'Internet|" + browserName + "' " +
                "AND ai._VALUE = '" + browserVersion + "'";

            return PerformQuery(cmdText);
        }

        public DataTable GetDistinctBrowsersWithVersions()
        {
            string cmdText =
                "SELECT DISTINCT SUBSTRING(_CATEGORY, 10, LEN(_CATEGORY) - 9) " +
                "FROM AUDITEDITEMS " +
                "WHERE _CATEGORY LIKE 'Internet|%' " +
                "AND _NAME = 'Version'";

            return PerformQuery(cmdText);
        }

        #endregion Assets Table
        #region Support Contract
        public void AddSupportContract(SupportContract objSupportContract)
        {
            string strContractNmber = GetContractNumberByAssetID(objSupportContract.AssetID);
            if (strContractNmber == "")
            {
                int iSupplierID = GetSupplierID(objSupportContract.Supplier);

                try
                {
                    string commandText =
                            "INSERT INTO ASSET_SUPPORTCONTRACT " +
                            "(_CONTRACT_NUMBER, " +
                            "_CONTRACT_VALUE, " +
                            "_SUPPORT_EXPIRY, " +
                            "_ALERTFLAG, " +
                            "_NOOFDAYS, " +
                            "_ALERTBYEMAIL, " +
                            "_NOTES, " +
                            "_ASSETID, " +
                            "_SUPPLIERID) " +
                            "VALUES (@ContractNumber," +
                            "@ContractValue, " +
                            "@ExpiryDate, " +
                            "@AlertFlag, " +
                            "@NoOfDays, " +
                            "@AlertByEmail, " +
                            "@Notes, " +
                            "@AssetID, " +
                            "@Supplier)";

                    if (isCompactDatabase)
                    {
                        using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                        {
                            using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                            {
                                command.Parameters.AddWithValue("@ContractNumber", objSupportContract.ContractNumber);
                                command.Parameters.AddWithValue("@ContractValue", objSupportContract.ContractValue);
                                command.Parameters.AddWithValue("@ExpiryDate", objSupportContract.ExpiryDate);
                                command.Parameters.AddWithValue("@AlertFlag", objSupportContract.AlertFlag);
                                command.Parameters.AddWithValue("@NoOfDays", objSupportContract.NoOfDays);
                                command.Parameters.AddWithValue("@AlertByEmail", objSupportContract.AlertByEmail);
                                command.Parameters.AddWithValue("@Notes", objSupportContract.Notes);
                                command.Parameters.AddWithValue("@AssetID", objSupportContract.AssetID);
                                command.Parameters.AddWithValue("@Supplier", iSupplierID);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                        {
                            using (SqlCommand command = new SqlCommand(commandText, conn))
                            {
                                command.Parameters.AddWithValue("@ContractNumber", objSupportContract.ContractNumber);
                                command.Parameters.AddWithValue("@ContractValue", objSupportContract.ContractValue);
                                command.Parameters.AddWithValue("@ExpiryDate", objSupportContract.ExpiryDate);
                                command.Parameters.AddWithValue("@AlertFlag", objSupportContract.AlertFlag);
                                command.Parameters.AddWithValue("@NoOfDays", objSupportContract.NoOfDays);
                                command.Parameters.AddWithValue("@AlertByEmail", objSupportContract.AlertByEmail);
                                command.Parameters.AddWithValue("@Notes", objSupportContract.Notes);
                                command.Parameters.AddWithValue("@AssetID", objSupportContract.AssetID);
                                command.Parameters.AddWithValue("@Supplier", iSupplierID);

                                command.ExecuteNonQuery();
                            }


                        }
                    }
                }
                catch (SqlException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                UpdateSupportContract(objSupportContract);  
            }


        }

        public void UpdateSupportContract(SupportContract objSupportContract)
        {
            int iSupplierID = GetSupplierID(objSupportContract.Supplier);

            try
            {
                string commandText =
                            "UPDATE ASSET_SUPPORTCONTRACT SET " +
                                "_CONTRACT_NUMBER=@ContractNumber, " +
                                "_CONTRACT_VALUE=@ContractValue, " +
                                "_SUPPORT_EXPIRY=@ExpiryDate, " +
                                "_ALERTFLAG=@AlertFlag, " +
                                "_NOOFDAYS=@NoOfDays, " +
                                "_ALERTBYEMAIL=@AlertByEmail, " +
                                "_NOTES=@Notes, " +
                                "_ASSETID=@AssetID, " +
                                "_SUPPLIERID=@Supplier "+
                                "WHERE _ASSETID=@AssetID";

                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@ContractNumber", objSupportContract.ContractNumber);
                            command.Parameters.AddWithValue("@ContractValue", objSupportContract.ContractValue);
                            command.Parameters.AddWithValue("@ExpiryDate", objSupportContract.ExpiryDate);
                            command.Parameters.AddWithValue("@AlertFlag", objSupportContract.AlertFlag);
                            command.Parameters.AddWithValue("@NoOfDays", objSupportContract.NoOfDays);
                            command.Parameters.AddWithValue("@AlertByEmail", objSupportContract.AlertByEmail);
                            command.Parameters.AddWithValue("@Notes", objSupportContract.Notes);
                            command.Parameters.AddWithValue("@AssetID", objSupportContract.AssetID);
                            command.Parameters.AddWithValue("@Supplier", iSupplierID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@ContractNumber", objSupportContract.ContractNumber);
                            command.Parameters.AddWithValue("@ContractValue", objSupportContract.ContractValue);
                            command.Parameters.AddWithValue("@ExpiryDate", objSupportContract.ExpiryDate);
                            command.Parameters.AddWithValue("@AlertFlag", objSupportContract.AlertFlag);
                            command.Parameters.AddWithValue("@NoOfDays", objSupportContract.NoOfDays);
                            command.Parameters.AddWithValue("@AlertByEmail", objSupportContract.AlertByEmail);
                            command.Parameters.AddWithValue("@Notes", objSupportContract.Notes);
                            command.Parameters.AddWithValue("@AssetID", objSupportContract.AssetID);
                            command.Parameters.AddWithValue("@Supplier", iSupplierID);

                            command.ExecuteNonQuery();
                        }


                    }
                }
            }
            catch (SqlException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public int GetSupplierID(string strSupplierName)
        {
                 

            int iSupplierID = -1;

            // which database type is this?
            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText = "SELECT _SUPPLIERID FROM SUPPLIERS WHERE _NAME = @cName";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", strSupplierName);
                            object result = command.ExecuteScalar();
                            if ((result != null) && (result.GetType() != typeof(DBNull)))
                            {
                                iSupplierID = Convert.ToInt32(result);
                            }
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                iSupplierID = lAuditWizardDataAccess.GetSupplierIDBySupplierName(strSupplierName);               
                
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");

            return iSupplierID;
                
        }

        /// <summary>
        /// Return a table containing all supportContract for Assets
        /// </summary>
        /// <returns></returns>
        public DataTable EnumerateSupportContract()
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable supportContractTable = new DataTable(TableNames.ALERTS);

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                               "SELECT _CONTRACT_NUMBER, _CONTRACT_VALUE ,_SUPPORT_EXPIRY ,_ALERTFLAG ,_NOOFDAYS, _ALERTBYEMAIL ,_NOTES ,_ASSETID, _SUPPLIERID " +
                               "FROM ASSET_SUPPORTCONTRACT " +
                               "ORDER BY _CONTRACT_NUMBER";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            new SqlCeDataAdapter(command).Fill(supportContractTable);
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                supportContractTable = lAuditWizardDataAccess.EnumerateSupportContract();
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return supportContractTable;
        }
        public string GetContractNumberByAssetID(int iAssetID)
        {
            string strContractID = "";

            // which database type is this?
            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText = "SELECT _CONTRACT_NUMBER FROM ASSET_SUPPORTCONTRACT WHERE _ASSETID = @cName";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", iAssetID);
                            object result = command.ExecuteScalar();
                            if ((result != null) && (result.GetType() != typeof(DBNull)))
                            {
                                strContractID = result.ToString();
                            }
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                strContractID = lAuditWizardDataAccess.GetContractNumberByAssetID(iAssetID);

            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");

            return strContractID;            

        }
        public DataTable GetSupportContractDetailsByID(int iAssetID)
        {
            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " in");

            DataTable supportContractTable = new DataTable(TableNames.ALERTS);

            if (isCompactDatabase)
            {
                try
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        string commandText =
                               "SELECT _CONTRACT_NUMBER ,_CONTRACT_VALUE ,_SUPPLIERID ,_SUPPORT_EXPIRY ,_ALERTFLAG ,_NOOFDAYS,_ALERTBYEMAIL ,_NOTES " + 
                               "FROM ASSET_SUPPORTCONTRACT "+
                               "WHERE _ASSETID=@cName";

                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", iAssetID);
                            new SqlCeDataAdapter(command).Fill(supportContractTable);
                        }
                    }
                }
                catch (SqlCeException ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");
                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                catch (Exception ex)
                {
                    Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                            "Please see the log file for further details.");

                    logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            else
            {
                AuditWizardDataAccess lAuditWizardDataAccess = new AuditWizardDataAccess();
                supportContractTable = lAuditWizardDataAccess.GetSupportContractDetailsByID(iAssetID);
            }

            if (isDebugEnabled) logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " out");
            return supportContractTable;
        }

        public void DeleteSupportContract(int iAssetID)
        {
            try
            {
                string commandText = "DELETE FROM ASSET_SUPPORTCONTRACT WHERE _ASSETID=@cName";
                            

                if (isCompactDatabase)
                {
                    using (SqlCeConnection conn = DatabaseConnection.CreateOpenCEConnection())
                    {
                        using (SqlCeCommand command = new SqlCeCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", iAssetID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = DatabaseConnection.CreateOpenStandardConnection())
                    {
                        using (SqlCommand command = new SqlCommand(commandText, conn))
                        {
                            command.Parameters.AddWithValue("@cName", iAssetID);                            
                            command.ExecuteNonQuery();
                        }


                    }
                }
            }
            catch (SqlException ex)
            {
                Utility.DisplayErrorMessage("A database error has occurred in AuditWizard." + Environment.NewLine + Environment.NewLine +
                        "Please see the log file for further details.");
                logger.Error("Exception in " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }


        }

        #endregion
    }
}