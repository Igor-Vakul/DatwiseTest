using Microsoft.Extensions.Logging.Abstractions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SafetyPortal.Api.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Reflection;

namespace SafetyPortal.Api.Services
{
    public static class ExcelOrCsvCreator
    {

        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        private static ILogger Logger => LoggerFactory.CreateLogger(typeof(ExcelOrCsvCreator));

        public static async Task<byte[]?> CreateExcel<T>(IEnumerable<T> dto, string filename, string title)
        {
            // create DataTable by DTO
            var dtbTable = ExcelOrCsvCreator.BuildDataTable(dto);

            if (dtbTable.Rows.Count <= 0)
                return null;

            //  var dtReversed = ReversDataTable (dtbTable);

            return await ExcelCreator<T>(filename, title, dtbTable);
        }

        private static async Task<byte[]?> ExcelCreator<T>(string filename, string title, DataTable dtReversed)
        {
            var ds = new DataSet();
            ds.Tables.Add(dtReversed);
            await using var stream = ExcelOrCsvCreator.GenerateExcel(ds, filename, title);
            return stream?.ToArray();
        }

        private static DataTable ReversDataTable(DataTable dt)
        {
            return dt.AsEnumerable().Reverse().CopyToDataTable();
        }

        public static MemoryStream GetCsv(DataTable data)
        {
            string[] fieldsToExpose = new string[data.Columns.Count];
            for (int i = 0; i < data.Columns.Count; i++)
            {
                fieldsToExpose[i] = data.Columns[i].ColumnName;
            }

            return GetCsv(fieldsToExpose, data);
        }

        public static MemoryStream GetCsv(string[] fieldsToExpose, DataTable data)
        {
            MemoryStream stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, true))
            {
                for (int i = 0; i < fieldsToExpose.Length; i++)
                {
                    if (i != 0)
                    {
                        writer.Write(",");
                    }
                    writer.Write(fieldsToExpose[i].Replace("\"", "\"\"").Replace("\"", ""));
                }
                writer.Write("\n");

                foreach (DataRow row in data.Rows)
                {
                    for (int i = 0; i < fieldsToExpose.Length; i++)
                    {
                        if (i != 0)
                        {
                            writer.Write(",");
                        }
                        writer.Write((row[fieldsToExpose[i]]?.ToString() ?? string.Empty)
                            .Replace("\"", "\"\"").Replace("\"", ""));
                    }

                    writer.Write("\n");
                }
            }

            stream.Position = 0;
            return stream;
        }


        public static DataTable RelevantOnlyTitleInDataTable(DataTable table, Dictionary<string, string> column)
        {
            List<string> relColumnListTb = new List<string>();

            /* var query = table.AsEnumerable().Where(r => r.Field<string>("EmployeeCode") == "1");
             foreach (var row in query.ToList())
             {
                 row.Delete();
             }
             */

            var relevantColumnOnly = column;


            List<string> allColumnListTb = (from dc in table.Columns.Cast<DataColumn>()
                                            select dc.ColumnName).ToList();

            foreach (var r in relevantColumnOnly)
            {
                if (table.Columns[r.Key] != null)
                {
                    relColumnListTb.Add(r.Key);
                    table.Columns[r.Key]!.ColumnName = r.Value;
                    table.AcceptChanges();
                }
            }

            var differencesList = allColumnListTb.Where(x => relColumnListTb.All(x1 => x1 != x))
                .Union(relColumnListTb.Where(x => allColumnListTb.All(x1 => x1 != x)));

            foreach (var r in differencesList)
            {
                if (table.Columns[r] != null)
                {
                    table.Columns.Remove(r);
                }
            }

            table.AcceptChanges();

            return table;
        }

        //public static bool CreateExcelOrCsvFile<T>(IEnumerable<T> data, Dictionary<string, string> columns, string title, bool isExcelFile = true)
        //{
        //    bool retVal = false;
        //    DataTable dtbTable = BuildDataTable(data);

        //    DataTable relTable = new DataTable();
        //    relTable = columns.Count == 0 ? dtbTable : RelevantOnlyTitleInDataTable(dtbTable, columns);

        //    DataSet ds = new DataSet();
        //    ds.Tables.Add(relTable);

        //    if (relTable.Rows.Count > 0)
        //    {

        //        if (isExcelFile)
        //        {
        //            var memoryStream = GenerateExcel<T>(title, ds, out retVal);
        //        }
        //        else
        //        {
        //            var memoryStream = GenerateExcel<T>(title, ds, out retVal);
        //        }
        //        return retVal;
        //    }

        //    return false;
        //}



        /// <summary>
        ///     This function does the following things:
        ///     1. Check if the directory (where the excel file is created) exists - if not create the directory
        ///     2. After the directory is ready it checks if there is same file already in this directory - if exists - the prev file
        ///     will be deleted
        ///     3. Creating the excel file - there is option to design the file
        /// </summary>
        /// <param name="dataToExcel">the data that the excel file will have</param>
        /// <param name="newExcelFile">the excel file name</param>
        /// <param name="excelSheetName">the excel sheet name</param>
        /// <returns>MemoryStream of the generated Excel file</returns>
        public static MemoryStream? GenerateExcelSlow(DataSet dataToExcel, string newExcelFile, string excelSheetName)
        {
            try
            {
                var stream = new MemoryStream();

                var newEmptyExcel = CreateFile(newExcelFile);

                using (var package = new ExcelPackage(newEmptyExcel))
                {
                    var workbook = package.Workbook;
                    if (dataToExcel.Tables.Count > 0)
                    {
                        var sheetName = excelSheetName;
                        var i = 0;

                        foreach (DataTable dTB in dataToExcel.Tables)
                        {
                            i++;
                            var worksheet = package.Workbook.Worksheets.Add(sheetName);
                            worksheet.View.RightToLeft = true;

                            CreateCaptionFormat(dTB, worksheet);

                            worksheet.Cells["A1"].LoadFromDataTable(dTB, true);

                            //worksheet.Cells[2, 2, 2, dTB.Columns.Count].Style.Numberformat.Format = "#";


                            worksheet.Cells[1, 1, 1, dTB.Columns.Count].AutoFilter = true;
                            worksheet.Cells[1, 1, 1, dTB.Columns.Count].AutoFitColumns();

                            for (var k = 1; k <= dTB.Columns.Count; k++)
                            {
                                worksheet.Column(k).Width = GetTrueColumnWidth(10);
                                worksheet.Column(k).AutoFit();
                                worksheet.Column(k).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                        }
                    }

                    package.SaveAs(stream);
                    stream.Position = 0; // ADDED: Reset stream position for EPPlus 8.2.1
                    return stream;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static MemoryStream? GenerateExcel(DataSet dataToExcel, string newExcelFile, string excelSheetName)
        {
            var stream = new MemoryStream();

            try
            {
                Logger.LogInformation($"Start GenerateExcel file:{newExcelFile} sheet name:{excelSheetName}");


                //TODO: Add working with licence in future
                ExcelPackage.License.SetNonCommercialOrganization("TransportationWebApi NonCommercial");

                using (var package = new ExcelPackage())
                {
                    if (dataToExcel.Tables.Count > 0)
                    {
                        foreach (DataTable dt in dataToExcel.Tables)
                        {
                            var worksheet = package.Workbook.Worksheets.Add(excelSheetName);

                            // Keep Right-to-Left view
                            worksheet.View.RightToLeft = true;

                            // Load data including header row
                            worksheet.Cells["A1"].LoadFromDataTable(dt, true, OfficeOpenXml.Table.TableStyles.None);

                            int totalColumns = dt.Columns.Count;

                            // Style header row
                            using (var headerRange = worksheet.Cells[1, 1, 1, totalColumns])
                            {
                                headerRange.Style.Font.Bold = true;
                                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                headerRange.Style.Fill.BackgroundColor.SetColor(Color.SteelBlue);
                                headerRange.Style.Font.Color.SetColor(Color.LightGoldenrodYellow);
                            }

                            // Set column widths and alignment
                            for (int col = 1; col <= totalColumns; col++)
                            {
                                //string headerName = dt.Columns[col - 1].ColumnName;
                                //if (headerName == "שם תחנת יעד" || headerName == "שם תחנת מוצא")
                                //    worksheet.Column(col).Width = 30; // special width
                                //else
                                //worksheet.Column(col).Width = 20; // default width
                                worksheet.Column(col).Width = 20; // adjust as needed
                                worksheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                        }
                    }

                    package.SaveAs(stream);
                }

                stream.Position = 0;

                Logger.LogInformation($"Excel generated successfully! file:{newExcelFile} sheet name:{excelSheetName}");

                return stream;
            }
            catch (Exception e)
            {
                Logger.LogError($"Excel generation failed! File:{newExcelFile} sheet name:{excelSheetName}{Environment.NewLine} Exception: {e.Message}");
                return null;
            }
        }

        private static void CreateCaptionFormat(DataTable dTB, ExcelWorksheet worksheet)
        {
            using (var range = worksheet.Cells[1, 1, 1, dTB.Columns.Count])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.SteelBlue);
                range.Style.Font.Color.SetColor(Color.LightGoldenrodYellow);
            }
        }

        private static double GetTrueColumnWidth(double width)
        {
            //DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
            var z = 1d;
            if (width >= 1 + 2 / 3)
            {
                z = Math.Round((Math.Round(7 * (width - 1 / 256), 0) - 5) / 7, 2);
            }
            else
            {
                z = Math.Round((Math.Round(12 * (width - 1 / 256), 0) - Math.Round(5 * width, 0)) / 12, 2);
            }

            //HOW FAR OFF? (WILL BE LESS THAN 1)
            var errorAmt = width - z;

            //CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING
            var adj = 0d;
            if (width >= 1 + 2 / 3)
            {
                adj = Math.Round(7 * errorAmt - 7 / 256, 0) / 7;
            }
            else
            {
                adj = Math.Round(12 * errorAmt - 12 / 256, 0) / 12 + 2 / 12;
            }

            //RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
            if (z > 0)
            {
                return width + adj;
            }

            return 0d;
        }

        /// <summary>
        ///     createFile
        /// </summary>
        /// <param name="newExcelFile"></param>
        /// <returns></returns>
        public static FileInfo CreateFile(string newExcelFile)
        {
            if (File.Exists(newExcelFile))
            {
                File.Delete(newExcelFile);
            }

            var newExcelFileOut = new FileInfo(newExcelFile);
            return newExcelFileOut;
        }

        public static DataTable BuildDataTable<T>(IEnumerable<T> data, bool applyIncludeInDocumentFilter = false)
        {
            if (data.Any())
            {
                return CreateDataTable(data, applyIncludeInDocumentFilter);
            }
            else
            {
                return new DataTable();
            }
        }

        private static DataTable CreateDataTable<T>(IEnumerable<T> data, bool applyIncludeInDocumentFilter)
        {
            //Get column headers
            PropertyInfo[] props = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                {
                    if (!applyIncludeInDocumentFilter) return true;
                    var attr = p.GetCustomAttribute<IncludeInDocumentAttribute>();
                    return attr == null || attr.Include;
                })];

            string[] headers = new string[props.Length];
            DataColumn[] columns = new DataColumn[props.Length];
            bool isDisplayNameAttributeDefined = false;

            int colCount = 0;
            foreach (PropertyInfo prop in props)
            {
                isDisplayNameAttributeDefined = Attribute.IsDefined(prop, typeof(DisplayNameAttribute));

                if (isDisplayNameAttributeDefined)
                {
                    DisplayNameAttribute? dna =
                        (DisplayNameAttribute?)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                    if (dna != null)
                        headers[colCount] = dna.DisplayName;
                }
                else
                    headers[colCount] = prop.Name;

                if (
                    prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?) ||
                    prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?) ||
                    prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?) ||
                    prop.PropertyType == typeof(short) || prop.PropertyType == typeof(short?) ||
                    prop.PropertyType == typeof(byte) || prop.PropertyType == typeof(byte?)
                    )

                    columns[colCount] = new DataColumn(headers[colCount], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                else
                    columns[colCount] = new DataColumn(headers[colCount]);

                colCount++;
                isDisplayNameAttributeDefined = false;
            }

            DataTable dataTable = new DataTable(typeof(T).Name);

            //Add column headers to datatable
            foreach (var column in columns)
            {
                dataTable.Columns.Add(column);
            }

            FillDataTable(data, props, dataTable);

            return dataTable;
        }

        private static void FillDataTable<T>(IEnumerable<T> data, PropertyInfo[] Props, DataTable dataTable)
        {
            //Add datalist to datatable
            foreach (T item in data)
            {
                object[] values = new object[Props.Length];
                for (int col = 0; col < Props.Length; col++)
                {
                    var displayFormatAttribute =
                        Props[col].GetCustomAttributes(typeof(DisplayFormatAttribute), false).FirstOrDefault() as
                            DisplayFormatAttribute;
                    if (displayFormatAttribute != null && Props[col].GetValue(item, null) != null)
                    {
                        values[col] = String.Format(displayFormatAttribute.DataFormatString ?? string.Empty,
                            Props[col].GetValue(item, null));
                    }
                    else
                    {
                        values[col] = Props[col].GetValue(item, null)!;
                    }


                    var assemQualifiedName = ((Props[col]).PropertyType).AssemblyQualifiedName;
                    if (assemQualifiedName != null && assemQualifiedName.Contains("DateTime"))
                    {
                        values[col] = $"{values[col]:dd/MM/yyyy}";
                    }
                    else if (assemQualifiedName != null && Props[col].GetValue(item, null) != null &&
                             (assemQualifiedName.Contains("Bool") || assemQualifiedName.Contains("Boolean")))
                    {
                        values[col] = Convert.ToBoolean(Props[col].GetValue(item, null)) ? "כן" : "לא";
                    }
                }

                dataTable.Rows.Add(values);
            }

            //dataTable = RemoveUnusedColumnsAndRows(dataTable);
        }

        /// <summary>
        /// GetTimestamp
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssff");
        }
    }
}
