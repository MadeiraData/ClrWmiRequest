# ClrWmiRequest

SQL Server CLR function that uses System.Management to run WMI queries and return results as XML.

# Example Use Cases

Query processor information for current machine:

```
DECLARE @server NVARCHAR(255) = '.'
DECLARE @query NVARCHAR(MAX) = 'SELECT * FROM Win32_Processor'
DECLARE @headers NVARCHAR(MAX)

DECLARE @results XML

SET @results = dbo.clr_wmi_request(@server, @query, @headers)

SELECT
	X.value('(@Name)[1]', 'nvarchar(max)'),
	X.value('(./Path/text())[1]', 'nvarchar(max)'),
	X.query('./Properties')
FROM @results.nodes('Response/Result/Item') AS T(X)

```

# The CLR Function
This is a SQL Server CLR function that calls an assembly written in C#, utilizing its ManagementScope class to run a WMI query and return the response.

## Input Parameters
These are the parameters that can be passed into the function:

### server (string)
Use "." to query from the current machine (i.e. the one where the SQL Server instance is installed).
You can also use this parameter to specify a remote server, but in order for it to work, you must make sure
that the SQL Server instance has permissions to perform remote WMI queries.
Use the steps in this guide to set it up: https://www.netwrix.com/kb/1630

### query (string)
This is the WMI query that you want to run against the specified server.
For example: "SELECT * FROM Win32_Processor"

### headers (string, in XML format)
This allows you to set headers for the WMI query. They are passed as XML following this format:
```
	<Headers>
			<Header Name="MyHeader">My Header's Value</Header>
			<Header Name="…">…</Header>
			<Header Name="…">…</Header>
	</Headers>
```
You can use these headers to specify options for the ManagementScope object.
The various supported headers are:

* **Authentication** (string) - Must be a string value from the *AuthenticationLevel* enum:
  * Unchanged
  * Default
  * None
  * Connect
  * Call
  * Packet
  * PacketIntegrity
  * PacketPrivacy
* **QueryLanguage** (string) - Sets the query language to be used (default: WQL).
* **Impersonation** (string) - Must be a string value from the *ImpersonationLevel* enum:
  * Default
  * Anonymous
  * Identify
  * Impersonate
  * Delegate
* **EnablePriviliges** (boolean) - Sets whether user privileges need to be enabled for the connection operation.
* **Username** (string) - Sets the user name to be used for the connection operation.
* **Password** (string) - Sets the password for the specified user.
* **Authority** (string) - Sets the authority used to authenticate the specified user.
* **SecurePassword** (string) - Sets the password for the specified user, created as a SecureString object.
* **Timeout** (int) - Sets the timeout, in milliseconds, for the WMI operation.


## Results

The result from this function is an XML document generated from the ManagementScope and ManagementObjectSearcher class objects, and the results of the WMI query formatted in XML.

* Response - this is the root element
  * Headers - each header will get its own node here
    * Name
    * Value
  * Result - this will contain the content from the response
    * Item - each "Item" element represents a single row returned from the WMI query
      * Name - each Item element has the "Name" attribute
      * Path - each Item element has its full "Path" as a sub-element
      * Properties - this will contain the collection of properties of each item (i.e. the "fields" from the query)
        * Property - this contains a single property of a given item
	    * Name - each property element has a single "Name" attribute
	    * Value - the contents of the Property element is its value
	    
