USE WorkLogDb;
GO

UPDATE Users 
SET PasswordHash = '$2a$11$PXJswxf4o10iq2vzhkCJE.E7D3iWySGWips2a7SJAXsv0P0Zt2dvm' 
WHERE Username = 'admin';
GO

SELECT Id, Username, Email, PasswordHash 
FROM Users 
WHERE Username = 'admin';
GO
