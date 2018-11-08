using System.Data.SqlTypes;
using System.Xml;
using System.Xml.Linq;
using System.Management;
using System;
using System.Security;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlXml clr_wmi_request(string server, string query, string headers)
    {
        XElement returnXml;

        try
        {
            ManagementScope scope = new ManagementScope("\\\\" + server + "\\root\\cimv2");
            string qLanguage = "WQL";

            // Add in any headers provided
            if (!string.IsNullOrWhiteSpace(headers))
            {
                // Parse provided headers as XML and loop through header elements
                var xmlData = XElement.Parse(headers);
                foreach (XElement headerElement in xmlData.Descendants())
                {
                    // Retrieve header's name and value
                    var headerName = headerElement.Attribute("Name").Value;
                    var headerValue = headerElement.Value;

                    switch (headerName)
                    {
                        case "Authentication":
                            AuthenticationLevel tmp_1;
                            if (Enum.TryParse<AuthenticationLevel>(headerValue, out tmp_1))
                                scope.Options.Authentication = tmp_1;
                            break;
                        case "Authority":
                            scope.Options.Authority = headerValue;
                            break;
                        case "QueryLanguage":
                            qLanguage = headerValue;
                            break;
                        case "Impersonation":
                            ImpersonationLevel tmp_2;
                            if (Enum.TryParse<ImpersonationLevel>(headerValue, out tmp_2))
                                scope.Options.Impersonation = tmp_2;
                            break;
                        case "EnablePriviliges":
                            bool tmp_3;
                            if (bool.TryParse(headerValue, out tmp_3))
                                scope.Options.EnablePrivileges = tmp_3;
                            break;
                        case "Username":
                            scope.Options.Username = headerValue;
                            break;
                        case "Password":
                            scope.Options.Password = headerValue;
                            break;
                        case "SecurePassword":
                            SecureString securepassword = new SecureString();

                            foreach (char c in headerValue)
                            {
                                securepassword.AppendChar(c);
                            }

                            scope.Options.SecurePassword = securepassword;

                            break;
                        case "Timeout":
                            int tmp_4;
                            if (int.TryParse(headerValue, out tmp_4))
                                scope.Options.Timeout = new TimeSpan(0, 0, 0, 0, tmp_4);
                            break;
                        case "Authorization-Basic-Credentials":
                            //request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(headerValue)));
                            break;
                        case "Authorization-Network-Credentials":
                            //request.Credentials = new NetworkCredential(headerValue.Split(':')[0], headerValue.Split(':')[1]);
                            break;
                        default: // other headers
                            break;
                    }
                }
            }

            scope.Connect();
            ObjectQuery queryObj = new ObjectQuery(qLanguage, query); // "SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, queryObj);
            ManagementObjectCollection results = searcher.Get();

            // Get headers
            var headersXml = new XElement("Headers",
                    new XElement("Success", "True"),
                    new XElement("Scope", searcher.Scope.Path),
                    new XElement("Query", searcher.Query.QueryString),
                    new XElement("QueryLanguage", searcher.Query.QueryLanguage),
                    new XElement("Count", results.Count),
                    new XElement("Authentication", scope.Options.Authentication),
                    new XElement("Authority", scope.Options.Authority),
                    new XElement("Impersonation", scope.Options.Impersonation),
                    new XElement("EnablePriviliges", scope.Options.EnablePrivileges),
                    new XElement("Username", scope.Options.Username),
                    new XElement("Timeout", scope.Options.Timeout.Milliseconds)
                );

            // Get values
            var valuesXml = new XElement("Result");

            foreach (ManagementObject item in results)
            {
                var propertiesXml = new XElement("Properties");

                foreach (PropertyData prop in item.Properties)
                {
                    // The Name property is automatically added as an attribute
                    if (prop.Name != "Name")
                    {
                        // Add this property with its values to the headers xml
                        propertiesXml.Add(
                            new XElement("Property",
                                new XAttribute("Name", prop.Name),
                                prop.Value
                                )
                        );
                    }
                }

                valuesXml.Add(
                        new XElement("Item",
                                new XAttribute("Name", item.GetPropertyValue("Name")),
                                new XElement("Path", item.Path.Path),
                                propertiesXml
                            )
                        );
            }

            returnXml =
                new XElement("Response",
                    headersXml,
                    valuesXml
                );
        }
        catch (Exception e)
        {
            returnXml =
                new XElement("Response",
                    new XElement("Headers",
                        new XElement("Success", "False"),
                        new XElement("Server", server),
                        new XElement("Query", query),
                        new XElement("Count", 0)
                        ),
                    new XElement("Result",
                        new XElement("Error", 
                            new XAttribute("Source", e.Source),
                            new XAttribute("HResult", e.HResult),
                            new XAttribute("Message", e.Message),
                            e.StackTrace
                            )
                        )
                );

        }

        return new SqlXml(returnXml.CreateReader());
    }

}