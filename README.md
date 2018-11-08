# ClrWmiRequest

SQL Server CLR function that uses System.Management to run WMI queries and return results as XML.

# Example Usage

Results return in the form of XML:
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

The third parameter (headers) is not fully tested yet.
