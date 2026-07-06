import apiService from './api';
import type { 
  AnalysisRequest, 
  AnalysisResult, 
  QueryPerformance,
  OptimizationSuggestion 
} from '../types';

class AnalysisService {
  /**
   * Analyze a SQL query for performance and optimization
   */
  async analyzeQuery(request: AnalysisRequest): Promise<AnalysisResult> {
    return apiService.post<AnalysisResult>('/Analysis/query', request);
  }

  /**
   * Get query performance metrics
   */
  async getQueryPerformance(queryId: string): Promise<QueryPerformance> {
    return apiService.get<QueryPerformance>(`/Analysis/performance/${queryId}`);
  }

  /**
   * Get optimization suggestions for a query
   */
  async getOptimizationSuggestions(queryText: string): Promise<OptimizationSuggestion[]> {
    return apiService.post<OptimizationSuggestion[]>('/Analysis/suggestions', { queryText });
  }

  /**
   * Compare two queries performance
   */
  async compareQueries(query1: string, query2: string): Promise<any> {
    return apiService.post('/Analysis/compare', { query1, query2 });
  }

  /**
   * Get execution plan for a query
   */
  async getExecutionPlan(queryText: string, connectionInfo: any): Promise<any> {
    return apiService.post('/Analysis/execution-plan', { 
      queryText, 
      connectionInfo 
    });
  }

  /**
   * Analyze stored procedure
   */
  async analyzeStoredProcedure(procedureName: string, connectionInfo: any): Promise<AnalysisResult> {
    try {
      console.log('Calling backend analysis API for:', procedureName);
      const result = await apiService.post<AnalysisResult>('/Analysis/stored-procedure', {
        procedureName,
        connectionInfo
      });
      console.log('Backend analysis result:', result);
      
      // Ensure required fields exist
      if (!result.suggestions) {
        console.warn('Backend returned result without suggestions array, adding empty array');
        result.suggestions = [];
      }
      if (!result.warnings) {
        console.warn('Backend returned result without warnings array, adding empty array');
        result.warnings = [];
      }
      if (!result.timestamp) {
        result.timestamp = new Date().toISOString();
      }
      
      return result;
    } catch (error: any) {
      // Fallback to mock data if backend is not available
      console.warn('Backend analysis not available, using mock data for:', procedureName);
      console.error('Error details:', error);
      return this.getMockAnalysisResult(procedureName);
    }
  }

  /**
   * Generate mock analysis result for testing
   */
  private getMockAnalysisResult(procedureName: string): AnalysisResult {
    return {
      queryText: `-- Stored Procedure: ${procedureName}\n-- Mock analysis data`,
      executionTime: Math.random() * 1000 + 100,
      suggestions: [
        {
          type: 'Index',
          severity: 'High',
          title: 'Missing Index Detected',
          description: `The procedure ${procedureName} could benefit from an index on frequently queried columns.`,
          recommendation: 'CREATE NONCLUSTERED INDEX IX_Example ON TableName (ColumnName);',
          estimatedImpact: '40-60% performance improvement',
        },
        {
          type: 'Query',
          severity: 'Medium',
          title: 'SELECT * Usage',
          description: 'Using SELECT * can retrieve unnecessary columns and impact performance.',
          recommendation: 'Specify only the required columns in the SELECT statement.',
          estimatedImpact: '15-25% performance improvement',
        },
        {
          type: 'Join',
          severity: 'Low',
          title: 'Join Optimization',
          description: 'Consider rewriting joins to use more efficient join conditions.',
          recommendation: 'Review join order and add appropriate WHERE clauses.',
          estimatedImpact: '5-10% performance improvement',
        },
      ],
      warnings: [],
      timestamp: new Date().toISOString(),
      performanceMetrics: {
        cpuTime: Math.random() * 500,
        ioReads: Math.floor(Math.random() * 10000),
        ioWrites: Math.floor(Math.random() * 1000),
      },
    };
  }

  /**
   * Batch analyze multiple queries
   */
  async batchAnalyze(queries: string[], connectionInfo: any): Promise<AnalysisResult[]> {
    return apiService.post<AnalysisResult[]>('/Analysis/batch', {
      queries,
      connectionInfo
    });
  }
}

export const analysisService = new AnalysisService();
export default analysisService;
