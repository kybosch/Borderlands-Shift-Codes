using BorderlandsShiftKeys.App_Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BorderlandsShiftKeys
{
    class ShiftKeyUpdater
    {
        public List<string> GetShiftKeys(string url)
        {
            string tableExpression = "<table[^>]*>(.*?)</table>";
            string tableRowsExpression = "<tr[^>]*>(.*?)</tr>";
            string tableCellExpression = "<td[^>]*>(.*?)</td>";

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            MatchCollection tables = Regex.Matches(s, tableExpression, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (tables.Count > 0)
            {
                Match[] tableRows = Regex.Matches(tables[1].Value, tableRowsExpression, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase).Cast<Match>().ToArray();
                if (tableRows.Length > 0)
                {
                    Match[] tableCells;
                    List<string> tableData = new List<string>();
                    foreach (var row in tableRows)
                    {
                        tableCells = Regex.Matches(row.Value, tableCellExpression, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase).Cast<Match>().ToArray();
                        if (tableCells.FirstOrDefault() != null)
                        {
                            tableData.Add(tableCells[4].Value.Replace("<td>", "").Replace("</td>", "").Replace("<span style=\"color:red\">", "").Replace("</span>", "").Replace("<span>", ""));
                        }
                    }
                    data.Close();
                    reader.Close();
                    return tableData;

                }
                else
                {
                    data.Close();
                    reader.Close();
                    return null;
                }
            }
            else
            {
                data.Close();
                reader.Close();
                return null;
            }

        }

        public void CheckDatabaseKeys(List<string> shiftKeys)
        {
            using (var dbContext = new ShiftKeysEntities())
            {
                foreach (var key in shiftKeys)
                {
                    if (!dbContext.tblShiftKeys.Any(x => x.ShiftKey == key))
                    {
                        Console.WriteLine(key);
                        dbContext.tblShiftKeys.Add(new tblShiftKey { ShiftKey = key, DateAdded = DateTime.Now.Date });
                        dbContext.SaveChanges();
                    }
                }
            }
        }

        public void SendEmail()
        {
            using (var dbContext = new ShiftKeysEntities())
            {
                var keys = dbContext.tblShiftKeys.Select(x => new { shiftKey = x.ShiftKey, dateAdded = x.DateAdded }).Where(x => x.dateAdded == DbFunctions.TruncateTime(DateTime.Now));
                if (keys.Count() > 0)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Hey,<br/>");
                        sb.AppendLine("Heres todays shiftkeys motherfuckers.<br/>");
                        sb.Append("<table>");
                        sb.Append("<tr><th>Shift Key</th><th>Date Added</th></tr>");
                        foreach (var key in keys)
                        {
                            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", key.shiftKey, key.dateAdded);
                        }
                        sb.Append("</table><br/><br/>");
                        sb.AppendLine("Now go out there, get some legendary gear and blow shit up. ITS YOUR DUTY.<br/><br/>");
                        sb.AppendLine("Fairy Fucking Godmother");

                        MailMessage mail = new MailMessage("email", "email");
                        mail.Subject = "Shift keys";
                        mail.IsBodyHtml = true;
                        mail.Body = sb.ToString();

                        var client = new SmtpClient("smtp.yourmailserver.com")
                        {
                            Credentials = new NetworkCredential("username", "password")
                        };
                        client.Send(mail);
                        Console.WriteLine("Sent");
                        Console.ReadLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                    }
                }
            }
        }
    }
}
