// Common types
export interface ApiResponse<T = any> {
  data?: T;
  success: boolean;
  message?: string;
  errors?: Record<string, string[]>;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Database types
export interface DatabaseConnection {
  databaseType: 0 | 1; // 0 = SqlServer, 1 = MySql
  server: string;
  port?: number;
  database: string;
  username?: string;
  password?: string;
  trustServerCertificate?: boolean;
}

export interface Database {
  databaseName: string;
  createDate?: string;
  sizeMB?: number;
}

export interface TableMetadata {
  tableName: string;
  schemaName?: string;
  rowCount?: number;
  createDate?: string;
}

export interface StoredProcedure {
  name: string;
  schemaName?: string;
  definition?: string;
  createDate?: string;
  modifyDate?: string;
}

export interface ViewMetadata {
  viewName: string;
  schemaName?: string;
  definition?: string;
  createDate?: string;
}

// Analysis types
export interface AnalysisRequest {
  queryText: string;
  connectionInfo: DatabaseConnection;
  includeExecutionPlan?: boolean;
}

export interface AnalysisResult {
  queryText: string;
  executionTime?: number;
  estimatedCost?: number;
  suggestions: OptimizationSuggestion[];
  executionPlan?: any;
  warnings?: string[];
  timestamp?: string;
  performanceMetrics?: {
    cpuTime?: number;
    ioReads?: number;
    ioWrites?: number;
  };
}

export interface QueryPerformance {
  queryId: string;
  executionTime: number;
  cpuTime: number;
  ioReads: number;
  ioWrites: number;
  rowCount: number;
  timestamp: string;
}

export interface OptimizationSuggestion {
  type: 'Index' | 'Query' | 'Schema' | 'Configuration';
  severity: 'High' | 'Medium' | 'Low';
  title: string;
  description: string;
  recommendation: string;
  estimatedImpact?: string;
}

// Report types
export type ReportType = 'Performance' | 'DatabaseSummary' | 'Optimization' | 'Custom';
export type ExportFormat = 'PDF' | 'Excel' | 'CSV' | 'JSON';

export interface ReportRequest {
  reportType: ReportType;
  connectionInfo: DatabaseConnection;
  parameters?: Record<string, any>;
  dateRange?: {
    start: Date;
    end: Date;
  };
}

export interface Report {
  id: string;
  reportType: ReportType;
  title: string;
  generatedAt: string;
  data: any;
  summary?: string;
}

// Theme
export type ThemeMode = 'light' | 'dark';
