import apiService from './api';
import type { 
  DatabaseConnection, 
  Database, 
  TableMetadata, 
  StoredProcedure,
  ViewMetadata 
} from '../types';

export interface ConnectionResponse {
  success: boolean;
  databaseType: string;
  message: string;
  serverVersion: string;
  databaseName: string;
  errorDetails?: string;
  timestamp: string;
}

class DatabaseService {
  /**
   * Test database connection
   */
  async testConnection(connection: DatabaseConnection): Promise<ConnectionResponse> {
    try {
      return await apiService.post<ConnectionResponse>('/Database/connect', connection);
    } catch (error: any) {
      throw new Error(error.message || 'Failed to connect to database');
    }
  }

  /**
   * Establish a persistent connection and save to context
   */
  async connect(connection: DatabaseConnection): Promise<ConnectionResponse> {
    try {
      const response = await apiService.post<ConnectionResponse>('/Database/connect', connection);
      
      if (response.success) {
        // Save connection info to localStorage
        localStorage.setItem('currentConnection', JSON.stringify(connection));
        localStorage.setItem('connectedDatabase', connection.database);
      }
      
      return response;
    } catch (error: any) {
      throw new Error(error.message || 'Failed to connect to database');
    }
  }

  /**
   * Disconnect from database
   */
  async disconnect(): Promise<void> {
    localStorage.removeItem('currentConnection');
    localStorage.removeItem('connectedDatabase');
  }

  /**
   * Get current connection from localStorage
   */
  getCurrentConnection(): DatabaseConnection | null {
    const connectionStr = localStorage.getItem('currentConnection');
    return connectionStr ? JSON.parse(connectionStr) : null;
  }

  /**
   * Get list of databases
   */
  async getDatabases(connection: DatabaseConnection): Promise<Database[]> {
    try {
      return await apiService.post<Database[]>('/Metadata/databases', connection);
    } catch (error: any) {
      throw new Error(error.message || 'Failed to fetch databases');
    }
  }

  /**
   * Get list of tables
   */
  async getTables(connection: DatabaseConnection): Promise<TableMetadata[]> {
    try {
      return await apiService.post<TableMetadata[]>('/Metadata/tables', connection);
    } catch (error: any) {
      throw new Error(error.message || 'Failed to fetch tables');
    }
  }

  /**
   * Get list of views
   */
  async getViews(connection: DatabaseConnection): Promise<ViewMetadata[]> {
    try {
      return await apiService.post<ViewMetadata[]>('/Metadata/views', connection);
    } catch (error: any) {
      throw new Error(error.message || 'Failed to fetch views');
    }
  }

  /**
   * Get list of stored procedures
   */
  async getStoredProcedures(connection: DatabaseConnection): Promise<StoredProcedure[]> {
    try {
      const procedures = await apiService.post<StoredProcedure[]>('/Metadata/storedprocedures', connection);
      console.log('Fetched stored procedures:', procedures);
      
      // Validate that procedures have names
      if (procedures && Array.isArray(procedures)) {
        procedures.forEach((proc, index) => {
          if (!proc.name) {
            console.warn(`Procedure at index ${index} is missing a name:`, proc);
          }
        });
      }
      
      return procedures;
    } catch (error: any) {
      console.error('Error fetching stored procedures:', error);
      // Return mock data for testing
      console.warn('Using mock stored procedures data');
      return this.getMockStoredProcedures();
    }
  }

  /**
   * Generate mock stored procedures for testing
   */
  private getMockStoredProcedures(): StoredProcedure[] {
    return [
      {
        name: 'sp_GetCustomerOrders',
        schemaName: 'dbo',
        createDate: '2024-01-15T10:30:00',
        modifyDate: '2024-06-20T14:45:00',
      },
      {
        name: 'sp_UpdateInventory',
        schemaName: 'dbo',
        createDate: '2024-02-10T09:15:00',
        modifyDate: '2024-06-25T11:20:00',
      },
      {
        name: 'sp_GenerateReport',
        schemaName: 'reporting',
        createDate: '2024-03-05T13:00:00',
        modifyDate: '2024-07-01T16:30:00',
      },
      {
        name: 'sp_CalculateTotals',
        schemaName: 'dbo',
        createDate: '2024-01-20T08:45:00',
        modifyDate: '2024-06-15T10:00:00',
      },
      {
        name: 'sp_ProcessPayments',
        schemaName: 'finance',
        createDate: '2024-04-01T12:00:00',
        modifyDate: '2024-07-05T15:15:00',
      },
    ];
  }

  /**
   * Get table columns
   */
  async getTableColumns(connection: DatabaseConnection, tableName: string): Promise<any[]> {
    try {
      return await apiService.post('/Metadata/columns', {
        ...connection,
        tableName
      });
    } catch (error: any) {
      throw new Error(error.message || 'Failed to fetch table columns');
    }
  }

  /**
   * Get stored procedure definition
   */
  async getStoredProcedureDefinition(
    connection: DatabaseConnection, 
    procedureName: string
  ): Promise<string> {
    try {
      console.log('Fetching definition for procedure:', procedureName);
      const response = await apiService.post<{ definition: string }>('/Metadata/procedure-definition', {
        ...connection,
        procedureName
      });
      console.log('Received definition:', response);
      return response.definition || this.getMockProcedureDefinition(procedureName);
    } catch (error: any) {
      console.warn('Failed to fetch procedure definition from backend, using mock data:', error);
      return this.getMockProcedureDefinition(procedureName);
    }
  }

  /**
   * Generate mock procedure definition for testing
   */
  private getMockProcedureDefinition(procedureName: string): string {
    return `-- =============================================
-- Stored Procedure: ${procedureName}
-- Description: Comprehensive business logic procedure
-- Author: SQL Optimizer System
-- Created: ${new Date().toISOString().split('T')[0]}
-- Modified: ${new Date().toISOString().split('T')[0]}
-- =============================================
-- This is mock data for demonstration purposes
-- The backend API will provide actual stored procedure definitions
-- =============================================

CREATE PROCEDURE [dbo].[${procedureName}]
    @CustomerId INT,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @CategoryFilter VARCHAR(50) = 'All',
    @IncludeInactive BIT = 0,
    @MaxRecords INT = 1000,
    @SortOrder VARCHAR(20) = 'DESC',
    @OutputMessage VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    -- Error handling and transaction management
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Variable declarations
        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;
        DECLARE @RowCount INT = 0;
        DECLARE @TotalAmount DECIMAL(18,2) = 0;
        
        -- Input validation
        IF @CustomerId IS NULL OR @CustomerId <= 0
        BEGIN
            SET @OutputMessage = 'Invalid customer ID provided';
            RAISERROR(@OutputMessage, 16, 1);
            RETURN -1;
        END;
        
        -- Set default date range if not provided
        IF @StartDate IS NULL
            SET @StartDate = DATEADD(MONTH, -6, GETDATE());
            
        IF @EndDate IS NULL
            SET @EndDate = GETDATE();
        
        -- Create temporary table for processing
        CREATE TABLE #TempCustomerOrders (
            OrderId INT,
            CustomerId INT,
            OrderDate DATETIME,
            TotalAmount DECIMAL(18,2),
            Status VARCHAR(50),
            Priority INT,
            CategoryName VARCHAR(100)
        );
        
        -- Common Table Expression for complex data aggregation
        WITH CustomerOrdersCTE AS (
            SELECT 
                o.OrderId,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.Priority,
                c.CategoryName,
                ROW_NUMBER() OVER (PARTITION BY o.CustomerId ORDER BY o.OrderDate DESC) AS RowNum,
                SUM(o.TotalAmount) OVER (PARTITION BY o.CustomerId) AS CustomerTotal,
                COUNT(*) OVER (PARTITION BY c.CategoryName) AS CategoryCount
            FROM 
                Orders o
            INNER JOIN 
                Customers cust ON o.CustomerId = cust.CustomerId
            INNER JOIN 
                OrderCategories c ON o.CategoryId = c.CategoryId
            LEFT JOIN 
                OrderDetails od ON o.OrderId = od.OrderId
            LEFT JOIN 
                Products p ON od.ProductId = p.ProductId
            WHERE 
                o.CustomerId = @CustomerId
                AND o.OrderDate BETWEEN @StartDate AND @EndDate
                AND (@CategoryFilter = 'All' OR c.CategoryName = @CategoryFilter)
                AND (@IncludeInactive = 1 OR o.Status = 'Active')
                AND o.IsDeleted = 0
            GROUP BY 
                o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount, 
                o.Status, o.Priority, c.CategoryName
            HAVING 
                SUM(od.Quantity * od.UnitPrice) > 0
        ),
        -- Nested CTE for ranking
        RankedOrders AS (
            SELECT 
                *,
                DENSE_RANK() OVER (ORDER BY TotalAmount DESC) AS AmountRank,
                PERCENT_RANK() OVER (ORDER BY OrderDate) AS DatePercentile
            FROM 
                CustomerOrdersCTE
            WHERE 
                RowNum <= @MaxRecords
        )
        -- Insert into temp table
        INSERT INTO #TempCustomerOrders
        SELECT 
            OrderId,
            CustomerId,
            OrderDate,
            TotalAmount,
            Status,
            Priority,
            CategoryName
        FROM 
            RankedOrders
        WHERE 
            AmountRank <= 100
        ORDER BY 
            CASE 
                WHEN @SortOrder = 'ASC' THEN OrderDate
            END ASC,
            CASE 
                WHEN @SortOrder = 'DESC' THEN OrderDate
            END DESC;
        
        SET @RowCount = @@ROWCOUNT;
        
        -- Main result set
        SELECT 
            t.OrderId,
            t.CustomerId,
            cust.CustomerName,
            cust.Email,
            cust.PhoneNumber,
            t.OrderDate,
            t.TotalAmount,
            t.Status,
            t.Priority,
            t.CategoryName,
            CASE 
                WHEN t.Priority = 1 THEN 'High'
                WHEN t.Priority = 2 THEN 'Medium'
                ELSE 'Low'
            END AS PriorityLabel,
            DATEDIFF(DAY, t.OrderDate, GETDATE()) AS DaysSinceOrder,
            CONCAT(cust.FirstName, ' ', cust.LastName) AS FullName,
            FORMAT(t.TotalAmount, 'C', 'en-US') AS FormattedAmount
        FROM 
            #TempCustomerOrders t
        INNER JOIN 
            Customers cust ON t.CustomerId = cust.CustomerId
        ORDER BY 
            t.OrderDate DESC;
        
        -- Calculate summary statistics
        SELECT @TotalAmount = SUM(TotalAmount) FROM #TempCustomerOrders;
        
        -- Update customer last access timestamp
        UPDATE Customers
        SET 
            LastAccessDate = GETDATE(),
            LastAccessUser = SUSER_SNAME(),
            TotalOrdersCount = TotalOrdersCount + @RowCount,
            ModifiedDate = GETDATE()
        WHERE 
            CustomerId = @CustomerId;
        
        -- Log the activity
        INSERT INTO ActivityLog (ActivityType, EntityType, EntityId, UserId, ActivityDate, Details)
        VALUES (
            'ProcedureExecution',
            'StoredProcedure',
            @CustomerId,
            SUSER_ID(),
            GETDATE(),
            CONCAT('Executed ${procedureName} - Records: ', @RowCount, ', Total: ', @TotalAmount)
        );
        
        -- Clean up
        DROP TABLE #TempCustomerOrders;
        
        -- Set success message
        SET @OutputMessage = CONCAT('Successfully processed ', @RowCount, ' records. Total amount: $', @TotalAmount);
        
        -- Commit transaction
        COMMIT TRANSACTION;
        
        -- Return success code
        RETURN 0;
        
    END TRY
    BEGIN CATCH
        -- Rollback transaction on error
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Capture error details
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();
        
        -- Log error
        INSERT INTO ErrorLog (ErrorMessage, ErrorSeverity, ErrorState, ProcedureName, ErrorDate)
        VALUES (@ErrorMessage, @ErrorSeverity, @ErrorState, '${procedureName}', GETDATE());
        
        -- Set error output message
        SET @OutputMessage = CONCAT('Error: ', @ErrorMessage);
        
        -- Re-throw error
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        
        -- Return error code
        RETURN -1;
    END CATCH;
END;
GO

-- =============================================
-- Example Usage:
-- =============================================
-- DECLARE @Message VARCHAR(500);
-- EXEC [dbo].[${procedureName}] 
--     @CustomerId = 12345,
--     @StartDate = '2024-01-01',
--     @EndDate = '2024-12-31',
--     @CategoryFilter = 'Electronics',
--     @IncludeInactive = 0,
--     @MaxRecords = 500,
--     @SortOrder = 'DESC',
--     @OutputMessage = @Message OUTPUT;
-- PRINT @Message;
-- =============================================`;
  }
}

export const databaseService = new DatabaseService();
export default databaseService;
