-- Create SqlQueries table
CREATE TABLE SqlQueries (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    QueryText NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DatabaseName NVARCHAR(100) NOT NULL,
    ExecutionTimeMs DECIMAL(18,2) NULL,
    OptimizedQueryText NVARCHAR(MAX) NULL,
    OptimizationNotes NVARCHAR(2000) NULL,
    IsOptimized BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Create indexes for better performance
CREATE INDEX IX_SqlQueries_DatabaseName ON SqlQueries(DatabaseName) WHERE IsDeleted = 0;
CREATE INDEX IX_SqlQueries_IsOptimized ON SqlQueries(IsOptimized) WHERE IsDeleted = 0;
CREATE INDEX IX_SqlQueries_CreatedAt ON SqlQueries(CreatedAt DESC) WHERE IsDeleted = 0;
