using BLL.Class;
using ManageLogFile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BLL.BLL
{
    public class SitesBL
    {
        private dBHelper helper = null;

        // Determin the ConnectionString
        string connectionString = dBFunctions.ConnectionStringSQLite;
        private LogFile errorLog;
        private LogFile infoLog;

        public SitesBL(LogFile errorLog, LogFile infoLog)
        {
            helper = new dBHelper(connectionString);
            this.errorLog = errorLog;
            this.infoLog = infoLog;
        }

        public DataTable UpdateFreelancerimTableRowsStatus(DataTable FreelancerimTable)
        {
            if (FreelancerimTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(FreelancerimTable);
                string rowsValues = getRowsValues(FreelancerimTable);
                commandText.Append(" CREATE TEMP TABLE FreelancerimPageTable(" + cols + ");");
                commandText.Append(" INSERT INTO FreelancerimPageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT Url, Name, (SELECT count(Id) FROM Freelancerim WHERE FreelancerimPageTable.Url = Url AND FreelancerimPageTable.Name = Name ");
                commandText.Append(" ) AS countRows FROM FreelancerimPageTable ;");
                commandText.Append("  DROP TABLE   FreelancerimPageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateFreelancerimTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }


        public bool AddFreelancerimPageTable(DataTable FreelancerimTable)
        {
            try
            {
                if (FreelancerimTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(FreelancerimTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO Freelancerim (" + cols + " ) ");
                string rowsValues = getRowsValues(FreelancerimTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddHomelessVehiclePageTable  " + ex.StackTrace);
                return false;
            }
        }

        public DataTable addAdVehicleTable(DataTable AdVehicleTable)
        {
            if (AdVehicleTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(AdVehicleTable);
                string rowsValues = getRowsValues(AdVehicleTable);
                commandText.Append(" CREATE TEMP TABLE AdVehicleTable(" + cols + ");");
                commandText.Append(" INSERT INTO AdVehicleTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues + ";");
                commandText.Append(" INSERT INTO AdVehicle (" + cols + " ) ");
                commandText.Append("  SELECT * FROM AdVehicleTable WHERE NOT EXISTS ");
                commandText.Append(" (SELECT * FROM AdVehicle WHERE AdVehicle.Manufacture = AdVehicleTable.Manufacture AND AdVehicle.Model = AdVehicleTable.Model AND AdVehicle.Year = AdVehicleTable.Year    ");
                commandText.Append(" AND  AdVehicle.City = AdVehicleTable.City AND AdVehicle.Price = AdVehicleTable.Price AND AdVehicle.Name = AdVehicleTable.Name AND AdVehicle.Phone1 = AdVehicleTable.Phone1 ");
                commandText.Append("  AND AdVehicle.phone2 = AdVehicleTable.phone2 AND AdVehicle.RowDate = AdVehicleTable.RowDate );");
                commandText.Append(" DROP TABLE AdVehicleTable;");
                if (helper.Load(commandText.ToString(), "") == true)
                    return null;
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable addNadlanSaleTable(DataTable NadlanSaleTable)
        {
            if (NadlanSaleTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(NadlanSaleTable);
                string rowsValues = getRowsValues(NadlanSaleTable);
                commandText.Append(" CREATE TEMP TABLE NadlanSaleTable(" + cols + ");");
                commandText.Append(" INSERT INTO NadlanSaleTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues + ";");
                commandText.Append(" INSERT INTO NadlanSale (" + cols + " ) ");
                commandText.Append("  SELECT * FROM NadlanSaleTable WHERE NOT EXISTS ");
                commandText.Append(" (SELECT * FROM NadlanSale WHERE NadlanSale.Address = NadlanSaleTable.Address AND NadlanSale.Type = NadlanSaleTable.Type AND NadlanSale.EntrenceDate = NadlanSaleTable.EntrenceDate    ");
                commandText.Append(" AND  NadlanSale.Rooms = NadlanSaleTable.Rooms AND NadlanSale.City = NadlanSaleTable.City AND NadlanSale.Floor = NadlanSaleTable.Floor AND NadlanSale.Phone1 = NadlanSaleTable.Phone1 ");
                commandText.Append(" AND  NadlanSale.Name = NadlanSaleTable.Name AND NadlanSale.Price = NadlanSaleTable.Price AND NadlanSale.phone2 = NadlanSaleTable.phone2 AND NadlanSale.RowDate = NadlanSaleTable.RowDate );");
                commandText.Append(" DROP TABLE NadlanSaleTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return null;
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable addNadlanRentTable(DataTable NadlanRentTable)
        {
            if (NadlanRentTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(NadlanRentTable);
                string rowsValues = getRowsValues(NadlanRentTable);
                commandText.Append(" CREATE TEMP TABLE NadlanRentTable(" + cols + ");");
                commandText.Append(" INSERT INTO NadlanRentTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues + ";");
                commandText.Append(" INSERT INTO NadlanRent (" + cols + " ) ");
                commandText.Append("  SELECT * FROM NadlanRentTable WHERE NOT EXISTS ");
                commandText.Append(" (SELECT * FROM NadlanRent WHERE NadlanRent.Address = NadlanRentTable.Address AND NadlanRent.Type = NadlanRentTable.Type AND NadlanRent.EntrenceDate = NadlanRentTable.EntrenceDate    ");
                commandText.Append(" AND  NadlanRent.Rooms = NadlanRentTable.Rooms AND NadlanRent.City = NadlanRentTable.City AND NadlanRent.Floor = NadlanRentTable.Floor AND NadlanRent.Phone1 = NadlanRentTable.Phone1 ");
                commandText.Append(" AND  NadlanRent.Name = NadlanRentTable.Name AND NadlanRent.Price = NadlanRentTable.Price AND NadlanRent.phone2 = NadlanRentTable.phone2 AND NadlanRent.RowDate = NadlanRentTable.RowDate );");
                commandText.Append(" DROP TABLE NadlanRentTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return null;
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable UpdateHomelessClassesTableRowsStatus(DataTable HomelessClassesTable)
        {
            if (HomelessClassesTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(HomelessClassesTable);
                string rowsValues = getRowsValues(HomelessClassesTable);
                commandText.Append(" CREATE TEMP TABLE pageTable(" + cols + ");");
                commandText.Append(" INSERT INTO pageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT Type, Category, Area, CourseName,(SELECT count(Id) FROM HomelessClasses WHERE pageTable.Type = Type AND pageTable.Category = Category AND ");
                commandText.Append(" pageTable.Area= Area 	AND pageTable.CourseName= CourseName ) AS countRows FROM pageTable ;");
                commandText.Append("  DROP TABLE   pageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateHomelessClassesTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable UpdateWinwinVehicleTableRowsStatus(DataTable WinwinTable)
        {
            if (WinwinTable.Rows.Count == 0)
                return null;
            try
            {

                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(WinwinTable);
                string rowsValues = getRowsValues(WinwinTable);
                commandText.Append(" CREATE TEMP TABLE WinwinVehiclePageTable(" + cols + ");");
                commandText.Append(" INSERT INTO WinwinVehiclePageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT Type, EngineCapacity, Year, PrevOwnersNum,Gear,Price,Area,  RowDate, (SELECT count(Id) FROM WinwinVehicle WHERE WinwinVehiclePageTable.Type = Type AND WinwinVehiclePageTable.EngineCapacity = EngineCapacity AND ");
                commandText.Append(" WinwinVehiclePageTable.Year= Year 	AND WinwinVehiclePageTable.PrevOwnersNum= PrevOwnersNum AND WinwinVehiclePageTable.Gear= Gear AND WinwinVehiclePageTable.Price= Price AND WinwinVehiclePageTable.Area= Area AND WinwinVehiclePageTable.RowDate= RowDate) AS countRows FROM WinwinVehiclePageTable ;");
                commandText.Append("  DROP TABLE   WinwinVehiclePageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public bool AddHomelessVehiclePageTable(DataTable HomelessVehicleTable)
        {
            try
            {
                if (HomelessVehicleTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(HomelessVehicleTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO HomelessVehicle (" + cols + " ) ");
                string rowsValues = getRowsValues(HomelessVehicleTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddHomelessVehiclePageTable  " + ex.StackTrace);
                return false;
            }
        }
    
        public bool AddWinwinVehiclePageTable(DataTable WinwinVehicleTable)
        {
            try
            {
                if (WinwinVehicleTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(WinwinVehicleTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO WinwinVehicle (" + cols + " ) ");
                string rowsValues = getRowsValues(WinwinVehicleTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddWinwinVehiclePageTable  " + ex.StackTrace);
                return false;
            }
        }
    

        public DataTable UpdateHomelessVehicleTableRowsStatus(DataTable HomelessVehicleTable)
        {
            if (HomelessVehicleTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(HomelessVehicleTable);
                string rowsValues = getRowsValues(HomelessVehicleTable);
                commandText.Append(" CREATE TEMP TABLE pageTable(" + cols + ");");
                commandText.Append(" INSERT INTO pageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT Type, EngineCapacity, Year, PrevOwnersNum,Price,Area,RowDate,(SELECT count(Id) FROM HomelessVehicle WHERE pageTable.Type = Type AND pageTable.EngineCapacity = EngineCapacity AND ");
                commandText.Append(" pageTable.Year = Year AND pageTable.PrevOwnersNum = PrevOwnersNum AND pageTable.Price = Price AND pageTable.Area = Area AND pageTable.RowDate = RowDate ) AS countRows FROM pageTable ;");
                commandText.Append("  DROP TABLE pageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateHomelessVehicleTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }


        public bool AddHomelessClassesPageTable(DataTable HomelessClassesTable)
        {
            try
            {
                if (HomelessClassesTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(HomelessClassesTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO HomelessClasses (" + cols + " ) ");
                string rowsValues = getRowsValues(HomelessClassesTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddWinwinPageTable  " + ex.StackTrace);
                return false;
            }
        }

        public bool AddHomelessPageTable(DataTable HomelessTable)
        {
            try
            {
                if (HomelessTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(HomelessTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO Homeless (" + cols + " ) ");
                string rowsValues = getRowsValues(HomelessTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddHomelessPageTable  " + ex.StackTrace);
                return false;
            }
        }

        public DataTable UpdateHomelessTableRowsStatus(DataTable HomelessTable)
        {
            if (HomelessTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(HomelessTable);
                string rowsValues = getRowsValues(HomelessTable);
                commandText.Append(" CREATE TEMP TABLE pageTable(" + cols + ");");
                commandText.Append(" INSERT INTO pageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append("  SELECT Type, Rooms, Floor, Price, Address, City, EntrenceDate,RowDate,(SELECT count(Id)  FROM Homeless WHERE pageTable.Rooms = Rooms AND pageTable.Floor = Floor AND  ");
                commandText.Append("  pageTable.Price= Price AND pageTable.Address= Address AND pageTable.City= City AND pageTable.EntrenceDate= EntrenceDate  AND pageTable.rowDate= rowDate) AS countRows FROM pageTable  ;");
                commandText.Append("  DROP TABLE   pageTable ");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable UpdateWinwinProfessionalTableRowsStatus(DataTable WinwinTable)
        {
            if (WinwinTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(WinwinTable);
                string rowsValues = getRowsValues(WinwinTable);
                commandText.Append(" CREATE TEMP TABLE WinwinProfessionalPageTable(" + cols + ");");
                commandText.Append(" INSERT INTO WinwinProfessionalPageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT BusinessName, Profession, SubCategory, Area,  RowDate, (SELECT count(Id) FROM WinwinProfessional WHERE WinwinProfessionalPageTable.BusinessName = BusinessName AND WinwinProfessionalPageTable.Profession = Profession AND ");
                commandText.Append(" WinwinProfessionalPageTable.SubCategory= SubCategory 	AND WinwinProfessionalPageTable.Area= Area AND WinwinProfessionalPageTable.RowDate= RowDate) AS countRows FROM WinwinProfessionalPageTable ;");
                commandText.Append("  DROP TABLE   WinwinProfessionalPageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public bool AddWinwinProfessionalPageTable(DataTable WinwinProfessionalTable)
        {
            try
            {
                if (WinwinProfessionalTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(WinwinProfessionalTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO WinwinProfessional (" + cols + " ) ");
                string rowsValues = getRowsValues(WinwinProfessionalTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddWinwinPageTable  " + ex.StackTrace);
                return false;
            }
        }

        public bool AddWinwinPageTable(DataTable WinwinTable)
        {
            try
            {
                if (WinwinTable.Rows.Count == 0)
                    return true;
                string cols = getTableCols(WinwinTable);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO Winwin (" + cols + " ) ");
                string rowsValues = getRowsValues(WinwinTable);
                commandText.Append("  SELECT " + rowsValues);

                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddWinwinPageTable  " + ex.StackTrace);
                return false;
            }
        }

        public bool AddYad2PageTable(DataTable Yad2Table)
        {
            try
            {

                string cols = getTableCols(Yad2Table);
                StringBuilder commandText = new StringBuilder("");
                commandText.Append("INSERT INTO Yad2 (" + cols + " ) ");
                string rowsValues = getRowsValues(Yad2Table);
                commandText.Append(@" SELECT " + rowsValues);
                commandText.Append(";");
                if (helper.Load(commandText.ToString(), "") == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at AddYad2PageTable  " + ex.StackTrace);
                return false;
            }
        }

        private static string getRowsValues(DataTable siteTable)
        {
            string rowsValues = "";
            for (int i = 0; i < siteTable.Rows.Count; i++)
            {

                if (i > 0)
                    rowsValues += (" UNION SELECT ");
                for (int j = 0; j < siteTable.Columns.Count; j++)
                {
                    if (siteTable.Columns[j].ColumnName == "ds_status")
                        continue;
                    if (j > 0)
                        rowsValues += ",";
                    rowsValues += "'" + siteTable.Rows[i][j].ToString().Replace("'", " ").Replace('"', ' ') + "'";
                }

            }
            return rowsValues;
        }

        private static string getTableCols(DataTable WinwinTable)
        {
            string cols = "";
            for (int i = 0; i < WinwinTable.Columns.Count; i++)
            {
                if (WinwinTable.Columns[i].ColumnName == "ds_status")
                    continue;
                if (WinwinTable.Columns[i].ColumnName == "ds_downloadStatus")
                    WinwinTable.Columns[i].ColumnName = "downloadStatus";
                if (WinwinTable.Columns[i].ColumnName == "ds_taskId")
                    WinwinTable.Columns[i].ColumnName = "taskId"; ;
                if (i > 0)
                    cols += ",";
                cols += WinwinTable.Columns[i].ColumnName;
            }
            return cols;
        }

        public DataTable UpdateWinwinTableRowsStatus(DataTable WinwinTable)
        {
            if (WinwinTable.Rows.Count == 0)
                return null;
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(WinwinTable);
                string rowsValues = getRowsValues(WinwinTable);
                commandText.Append(" CREATE TEMP TABLE pageTable(" + cols + ");");
                commandText.Append(" INSERT INTO pageTable (" + cols + " ) ");
                commandText.Append(" SELECT " + rowsValues);
                commandText.Append(" ;");
                commandText.Append(" SELECT Type, Price, Rooms, Address, EntrenceDate, RowDate, Region,(SELECT count(Id) FROM Winwin WHERE pageTable.Type = Type AND pageTable.Address = Address AND ");
                commandText.Append(" pageTable.Price= Price 	AND pageTable.Rooms= Rooms AND pageTable.EntrenceDate= EntrenceDate AND pageTable.rowDate= rowDate) AS countRows FROM pageTable ;");
                commandText.Append("  DROP TABLE   pageTable");
                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateWinwinTableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }

        public DataTable UpdateYad2TableRowsStatus(DataTable Yad2Table)
        {
            try
            {
                StringBuilder commandText = new StringBuilder("");
                string cols = getTableCols(Yad2Table);
                string rowsValues = getRowsValues(Yad2Table);
                commandText.Append(" CREATE TEMP TABLE pageTable(" + cols + ");");
                commandText.Append("INSERT INTO pageTable (" + cols + " ) ");
                commandText.Append(@" SELECT " + rowsValues);
                commandText.Append(" SELECT Type, Area, Address,	Price, Rooms, EntrenceDate, Floor, RowDate,	City, Neighborhood,	Address2, Size,	[FreeText],	Name, Phone1, Phone2, MunicipalRate, 'close' AS  ds_status , ");
                commandText.Append("        CASE WHEN ( SELECT      count(Id)");
                commandText.Append("                    FROM             Yad2");
                commandText.Append("                    WHERE pageTable.Type= Type AND pageTable.Area=  Area AND pageTable.Address= Address AND   pageTable.Price= Price AND pageTable.Rooms= Rooms        ");
                commandText.Append("        THEN 'Done'                                                                                                                     ");
                commandText.Append("        ELSE 'Waiting'                                                                                                                  ");
                commandText.Append("        END AS ds_downloadStatus, TaskId                                                                                                ");
                commandText.Append("        FROM pageTable                                                                                                       ");

                if (helper.Load(commandText.ToString(), "") == true)
                    return helper.DataSet.Tables[0];
                return null;
            }
            catch (Exception ex)
            {
                errorLog.handleException(ex);
                errorLog.writeToLogFile("at UpdateYad2TableRowsStatus  " + ex.StackTrace);
                return null;
            }
        }


    }
}
