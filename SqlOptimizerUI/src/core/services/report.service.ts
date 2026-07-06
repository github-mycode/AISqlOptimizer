import apiService from './api';
import type { 
  ReportRequest, 
  Report, 
  ReportType,
  ExportFormat 
} from '../types';

class ReportService {
  /**
   * Generate a new report
   */
  async generateReport(request: ReportRequest): Promise<Report> {
    return apiService.post<Report>('/Report/generate', request);
  }

  /**
   * Get all reports
   */
  async getReports(pageNumber: number = 1, pageSize: number = 10): Promise<any> {
    return apiService.get(`/Report/list?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  /**
   * Get report by ID
   */
  async getReportById(reportId: string): Promise<Report> {
    return apiService.get<Report>(`/Report/${reportId}`);
  }

  /**
   * Delete a report
   */
  async deleteReport(reportId: string): Promise<void> {
    return apiService.delete(`/Report/${reportId}`);
  }

  /**
   * Export report to different formats
   */
  async exportReport(reportId: string, format: ExportFormat): Promise<Blob> {
    const response = await apiService.getAxiosInstance().get(
      `/Report/${reportId}/export?format=${format}`,
      { responseType: 'blob' }
    );
    return response.data;
  }

  /**
   * Generate performance report
   */
  async generatePerformanceReport(connectionInfo: any, dateRange?: { start: Date; end: Date }): Promise<Report> {
    return apiService.post<Report>('/Report/performance', {
      connectionInfo,
      dateRange
    });
  }

  /**
   * Generate database summary report
   */
  async generateDatabaseSummary(connectionInfo: any): Promise<Report> {
    return apiService.post<Report>('/Report/database-summary', { connectionInfo });
  }

  /**
   * Generate optimization report
   */
  async generateOptimizationReport(analysisResults: any[]): Promise<Report> {
    return apiService.post<Report>('/Report/optimization', { analysisResults });
  }

  /**
   * Schedule a report
   */
  async scheduleReport(request: ReportRequest, schedule: any): Promise<any> {
    return apiService.post('/Report/schedule', {
      reportRequest: request,
      schedule
    });
  }

  /**
   * Get scheduled reports
   */
  async getScheduledReports(): Promise<any[]> {
    return apiService.get('/Report/scheduled');
  }

  /**
   * Download report file
   */
  downloadReportFile(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}

export const reportService = new ReportService();
export default reportService;
