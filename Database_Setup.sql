-- =============================================
-- CREATE / RECREATE DATABASE
-- =============================================
USE master;
GO

IF DB_ID('InventoryDBs') IS NOT NULL
BEGIN
    ALTER DATABASE InventoryDBs SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InventoryDBs;
END
GO

CREATE DATABASE InventoryDBs;
GO

USE InventoryDBs;
GO

-- =============================================
-- 1. USERS TABLE
-- =============================================
CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName     NVARCHAR(100),
    Role         NVARCHAR(20)  CHECK (Role IN ('Admin','Cashier','InventoryManager','Customer')),
    IsActive     BIT           DEFAULT 1,
    CreatedDate  DATETIME      DEFAULT GETDATE()
);
GO

-- =============================================
-- 2. CATEGORIES TABLE
-- =============================================
CREATE TABLE Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) UNIQUE NOT NULL,
    Description  NVARCHAR(255)
);
GO

-- =============================================
-- 3. SUPPLIERS TABLE
-- =============================================
CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    Name       NVARCHAR(100) NOT NULL,
    Phone      NVARCHAR(20),
    Email      NVARCHAR(100),
    Address    NVARCHAR(255)
);
GO

-- =============================================
-- 4. PRODUCTS TABLE
-- =============================================
CREATE TABLE Products (
    ProductID    INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode  NVARCHAR(50)   UNIQUE NOT NULL,
    Name         NVARCHAR(100),
    CategoryID   INT FOREIGN KEY REFERENCES Categories(CategoryID),
    SupplierID   INT FOREIGN KEY REFERENCES Suppliers(SupplierID),
    Quantity     INT            DEFAULT 0,
    UnitPrice    DECIMAL(18,2),
    ReorderLevel INT            DEFAULT 10,
    LastUpdated  DATETIME       DEFAULT GETDATE()
);
GO

-- =============================================
-- 5. CUSTOMERS TABLE
-- =============================================
CREATE TABLE Customers (
    CustomerID  INT IDENTITY(1,1) PRIMARY KEY,
    FullName    NVARCHAR(100),
    Phone       NVARCHAR(20),
    Email       NVARCHAR(100),
    Address     NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- 6. STOCK IN TABLE (PURCHASES)
-- =============================================
CREATE TABLE StockIn (
    StockInID  INT IDENTITY(1,1) PRIMARY KEY,
    ProductID  INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity   INT            NOT NULL,
    UnitCost   DECIMAL(18,2),
    SupplierID INT FOREIGN KEY REFERENCES Suppliers(SupplierID),
    Date       DATETIME       DEFAULT GETDATE(),
    Notes      NVARCHAR(255)
);
GO

-- =============================================
-- 6b. STOCK OUT TABLE (MANUAL STOCK REMOVALS)
-- =============================================
CREATE TABLE StockOut (
    StockOutID   INT IDENTITY(1,1) PRIMARY KEY,
    ProductID    INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity     INT            NOT NULL,
    UnitPrice    DECIMAL(18,2),
    CustomerName NVARCHAR(100),
    Notes        NVARCHAR(255),
    Date         DATETIME       DEFAULT GETDATE()
);
GO

-- =============================================
-- 7. SALES TABLE (INVOICE HEADER)
-- =============================================
CREATE TABLE Sales (
    SaleID          INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber   NVARCHAR(20)   UNIQUE NOT NULL,
    CustomerID      INT FOREIGN KEY REFERENCES Customers(CustomerID),
    SaleDate        DATETIME       DEFAULT GETDATE(),
    Subtotal        DECIMAL(18,2),
    DiscountPercent DECIMAL(5,2)   DEFAULT 0,
    DiscountAmount  DECIMAL(18,2)  DEFAULT 0,
    TaxPercent      DECIMAL(5,2)   DEFAULT 0,
    TaxAmount       DECIMAL(18,2)  DEFAULT 0,
    TotalAmount     DECIMAL(18,2),
    PaymentMethod   NVARCHAR(20),
    PaymentStatus   NVARCHAR(20)   DEFAULT 'Pending Verification',
    OrderStatus     NVARCHAR(20)   DEFAULT 'Pending',
    PaymentProof    NVARCHAR(MAX),
    TransactionRef  NVARCHAR(100),
    VerifiedBy      INT FOREIGN KEY REFERENCES Users(UserID),
    VerifiedDate    DATETIME,
    RejectionReason NVARCHAR(255),
    Notes           NVARCHAR(255),
    CreatedBy       INT FOREIGN KEY REFERENCES Users(UserID)
);
GO

-- =============================================
-- 8. SALE ITEMS TABLE (INVOICE DETAILS)
-- =============================================
CREATE TABLE SaleItems (
    SaleItemID INT IDENTITY(1,1) PRIMARY KEY,
    SaleID     INT FOREIGN KEY REFERENCES Sales(SaleID) ON DELETE CASCADE,
    ProductID  INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity   INT,
    UnitPrice  DECIMAL(18,2),
    TotalPrice DECIMAL(18,2)
);
GO

-- =============================================
-- 8b. BANK TRANSFER DETAILS TABLE
-- =============================================
CREATE TABLE BankTransferDetails (
    BankTransferID INT IDENTITY(1,1) PRIMARY KEY,
    SaleID         INT FOREIGN KEY REFERENCES Sales(SaleID) ON DELETE CASCADE,
    BankName       NVARCHAR(100) NOT NULL,
    AccountNumber  NVARCHAR(50)  NOT NULL,
    AccountHolder  NVARCHAR(100) NOT NULL,
    CreatedDate    DATETIME      DEFAULT GETDATE()
);
GO

-- =============================================
-- 9. INVENTORY LOGS TABLE (STOCK MOVEMENT HISTORY)
-- =============================================
CREATE TABLE InventoryLogs (
    LogID          INT IDENTITY(1,1) PRIMARY KEY,
    ProductID      INT FOREIGN KEY REFERENCES Products(ProductID),
    QuantityChange INT,
    NewQuantity    INT,
    Reason         NVARCHAR(100),
    Reference      NVARCHAR(50),
    LogDate        DATETIME DEFAULT GETDATE(),
    UserID         INT FOREIGN KEY REFERENCES Users(UserID)
);
GO

-- =============================================
-- 10. TRIGGERS
-- =============================================

-- StockIn Trigger (auto-increase stock and log)
CREATE TRIGGER trg_StockIn_Update
ON StockIn AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Quantity    = p.Quantity + i.Quantity,
        p.LastUpdated = GETDATE()
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;

    INSERT INTO InventoryLogs (ProductID, QuantityChange, NewQuantity, Reason, Reference, UserID)
    SELECT i.ProductID, i.Quantity, p.Quantity,
           'Purchase', 'PO-' + CAST(i.StockInID AS NVARCHAR), 1
    FROM inserted i
    INNER JOIN Products p ON i.ProductID = p.ProductID;
END;
GO

-- Sale Trigger (auto-decrease stock and log)
CREATE TRIGGER trg_Sale_UpdateStock
ON SaleItems AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Quantity    = p.Quantity - i.Quantity,
        p.LastUpdated = GETDATE()
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;

    INSERT INTO InventoryLogs (ProductID, QuantityChange, NewQuantity, Reason, Reference, UserID)
    SELECT i.ProductID, -i.Quantity, p.Quantity,
           'Sale', s.InvoiceNumber, s.CreatedBy
    FROM inserted i
    INNER JOIN Sales s    ON i.SaleID    = s.SaleID
    INNER JOIN Products p ON i.ProductID = p.ProductID;
END;
GO

-- StockOut Trigger (auto-decrease stock and log)
CREATE TRIGGER trg_StockOut_Update
ON StockOut AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Quantity    = p.Quantity - i.Quantity,
        p.LastUpdated = GETDATE()
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;

    INSERT INTO InventoryLogs (ProductID, QuantityChange, NewQuantity, Reason, Reference, UserID)
    SELECT i.ProductID, -i.Quantity, p.Quantity,
           'StockOut', 'SO-' + CAST(i.StockOutID AS NVARCHAR), 1
    FROM inserted i
    INNER JOIN Products p ON i.ProductID = p.ProductID;
END;
GO

-- =============================================
-- 11. STORED PROCEDURES
-- =============================================
CREATE PROCEDURE sp_LowStock
AS
BEGIN
    SELECT * FROM Products WHERE Quantity <= ReorderLevel;
END;
GO

CREATE PROCEDURE sp_TopSellingProducts
    @TopN INT = 5
AS
BEGIN
    SELECT TOP (@TopN)
        p.ProductID,
        p.Name,
        SUM(si.Quantity) AS TotalSold
    FROM SaleItems si
    INNER JOIN Products p ON si.ProductID = p.ProductID
    GROUP BY p.ProductID, p.Name
    ORDER BY TotalSold DESC;
END;
GO

-- =============================================
-- 12. SAMPLE DATA
-- =============================================

-- Users
INSERT INTO Users (Username, PasswordHash, FullName, Role) VALUES
('admin',      'admin123', 'System Administrator', 'Admin'),
('cashier1',   'cash123',  'John Cash',            'Cashier'),
('invmanager', 'inv123',   'Mary Stock',           'InventoryManager'),
('customer1',  'cust123',  'Jane Doe',             'Customer');
GO

-- Categories
INSERT INTO Categories (CategoryName) VALUES
('Electronics'),
('Furniture'),
('Clothing');
GO

-- Suppliers
INSERT INTO Suppliers (Name, Phone) VALUES
('Tech Supply Co.', '123-456-7890'),
('Furniture World', '234-567-8901');
GO

-- Products
INSERT INTO Products (ProductCode, Name, CategoryID, SupplierID, Quantity, UnitPrice, ReorderLevel) VALUES
('P001', 'Laptop',      1, 1, 50,  1200.00, 5),
('P002', 'Office Desk', 2, 2, 20,   450.00, 3),
('P003', 'T-Shirt',     3, 1, 100,   19.99, 20);
GO

-- Customers
INSERT INTO Customers (FullName) VALUES
('Walk-in Customer'),
('John Doe');
GO

PRINT '==========================================';
PRINT 'Database created successfully!';
PRINT '==========================================';
PRINT 'Default Logins:';
PRINT '  Admin:             admin      / admin123';
PRINT '  Cashier:           cashier1   / cash123';
PRINT '  Inventory Manager: invmanager / inv123';
PRINT '  Customer:          customer1  / cust123';
PRINT '==========================================';
GO
