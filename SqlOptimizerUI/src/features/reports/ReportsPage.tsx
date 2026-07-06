import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  Card,
  CardContent,
  Grid,
  Paper,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Fade,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import {
  Assessment as AssessmentIcon,
  PictureAsPdf as PdfIcon,
  Code as HtmlIcon,
  Visibility as PreviewIcon,
  Download as DownloadIcon,
  CheckCircle as CheckCircleIcon,
  TableChart as ExcelIcon,
  DataObject as JsonIcon,
} from '@mui/icons-material';
import { useMutation } from '@tanstack/react-query';
import { reportService } from '../../core/services/report.service';
import { databaseService } from '../../core/services/database.service';
import type { Report, ReportType } from '../../core/types';

export const ReportsPage = () => {
  const [previewOpen, setPreviewOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [generatedReport, setGeneratedReport] = useState<Report | null>(null);
  const [connection, setConnection] = useState<any>(null);
  const [previewReport, setPreviewReport] = useState<Report | null>(null);

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (!currentConnection) {
      // Create mock connection
      const mockConnection = {
        databaseType: 0,
        server: 'localhost',
        database: 'TestDB',
      };
      setConnection(mockConnection);
    } else {
      setConnection(currentConnection);
    }
  }, []);

  const generateReportMutation = useMutation({
    mutationFn: async (type: ReportType) => {
      console.log('Generating report of type:', type);
      
      // Generate mock report with comprehensive data
      const report: Report = {
        id: `RPT-${Date.now()}`,
        reportType: type,
        title: getReportTitle(type),
        generatedAt: new Date().toISOString(),
        data: generateMockReportData(type),
        summary: generateReportSummary(type),
      };
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      return report;
    },
    onSuccess: (data) => {
      setGeneratedReport(data);
      setSuccessMessage(`${data.title} generated successfully!`);
      setTimeout(() => setSuccessMessage(''), 3000);
    },
    onError: (error) => {
      console.error('Report generation failed:', error);
      setSuccessMessage('Failed to generate report');
      setTimeout(() => setSuccessMessage(''), 3000);
    },
  });

  const getReportTitle = (type: ReportType): string => {
    switch (type) {
      case 'Performance':
        return 'Performance Analysis Report';
      case 'DatabaseSummary':
        return 'Database Summary Report';
      case 'Optimization':
        return 'Optimization Recommendations Report';
      default:
        return 'Custom Report';
    }
  };

  const generateMockReportData = (type: ReportType) => {
    switch (type) {
      case 'Performance':
        return {
          totalProcedures: 25,
          averageScore: 78,
          criticalIssues: 3,
          warnings: 12,
          suggestions: 18,
          topIssues: [
            { procedure: 'usp_GetCustomerOrders', issue: 'Missing Index', impact: 'High', improvement: '45%' },
            { procedure: 'usp_UpdateInventory', issue: 'Table Scan', impact: 'High', improvement: '38%' },
            { procedure: 'usp_SearchProducts', issue: 'Implicit Conversion', impact: 'Medium', improvement: '22%' },
            { procedure: 'usp_GenerateInvoice', issue: 'Nested Loops', impact: 'Medium', improvement: '18%' },
            { procedure: 'usp_GetReports', issue: 'Function in WHERE', impact: 'Low', improvement: '12%' },
          ],
          executionStats: {
            avgExecutionTime: '245ms',
            maxExecutionTime: '3.2s',
            minExecutionTime: '12ms',
            totalExecutions: 15420,
          },
        };
      case 'DatabaseSummary':
        return {
          totalTables: 45,
          totalProcedures: 25,
          totalFunctions: 12,
          totalIndexes: 128,
          databaseSize: '2.4 GB',
          dataSize: '1.8 GB',
          indexSize: '600 MB',
          tableStats: [
            { name: 'Orders', rows: 125000, size: '450 MB', indexes: 8 },
            { name: 'Customers', rows: 45000, size: '180 MB', indexes: 5 },
            { name: 'Products', rows: 8500, size: '95 MB', indexes: 4 },
            { name: 'OrderDetails', rows: 380000, size: '520 MB', indexes: 6 },
            { name: 'Inventory', rows: 12000, size: '75 MB', indexes: 3 },
          ],
        };
      case 'Optimization':
        return {
          totalRecommendations: 33,
          highPriority: 8,
          mediumPriority: 15,
          lowPriority: 10,
          estimatedImprovement: '42%',
          recommendations: [
            { 
              title: 'Add Missing Indexes', 
              priority: 'High', 
              impact: '45%', 
              effort: 'Low',
              description: 'Create 5 missing indexes on frequently queried columns',
              affectedObjects: ['Orders', 'Customers', 'Products']
            },
            { 
              title: 'Optimize Join Operations', 
              priority: 'High', 
              impact: '38%', 
              effort: 'Medium',
              description: 'Rewrite 3 procedures using inefficient join patterns',
              affectedObjects: ['usp_GetCustomerOrders', 'usp_SearchProducts']
            },
            { 
              title: 'Remove Unnecessary Subqueries', 
              priority: 'Medium', 
              impact: '22%', 
              effort: 'Low',
              description: 'Replace correlated subqueries with JOIN operations',
              affectedObjects: ['usp_CalculateTotals', 'usp_GetSummary']
            },
            { 
              title: 'Update Statistics', 
              priority: 'Medium', 
              impact: '18%', 
              effort: 'Low',
              description: 'Update outdated statistics on 12 tables',
              affectedObjects: ['Multiple Tables']
            },
            { 
              title: 'Refactor Cursor Logic', 
              priority: 'High', 
              impact: '35%', 
              effort: 'High',
              description: 'Replace cursor-based processing with set-based operations',
              affectedObjects: ['usp_ProcessBatch', 'usp_UpdateRecords']
            },
          ],
        };
      default:
        return {};
    }
  };

  const generateReportSummary = (type: ReportType): string => {
    switch (type) {
      case 'Performance':
        return 'Analyzed 25 stored procedures. Found 3 critical performance issues and 12 warnings. Average performance score: 78%. Recommended improvements could yield up to 45% performance gain.';
      case 'DatabaseSummary':
        return 'Database contains 45 tables, 25 stored procedures, and 128 indexes. Total size: 2.4 GB. Top 5 tables account for 68% of total storage.';
      case 'Optimization':
        return 'Generated 33 optimization recommendations (8 high priority, 15 medium, 10 low). Implementing high-priority items could improve overall performance by approximately 42%.';
      default:
        return 'Report generated successfully.';
    }
  };

  const handleDownload = async (format: 'PDF' | 'HTML' | 'Excel' | 'JSON') => {
    try {
      if (!generatedReport) return;
      
      setSuccessMessage(`Preparing ${format} download...`);
      
      // Generate file content based on format
      let content: string;
      let filename: string;
      let mimeType: string;
      
      switch (format) {
        case 'PDF':
          content = generatePDFContent(generatedReport);
          filename = `${generatedReport.id}.pdf`;
          mimeType = 'application/pdf';
          break;
        case 'HTML':
          content = generateHTMLContent(generatedReport);
          filename = `${generatedReport.id}.html`;
          mimeType = 'text/html';
          break;
        case 'Excel':
          content = generateExcelContent(generatedReport);
          filename = `${generatedReport.id}.csv`;
          mimeType = 'text/csv';
          break;
        case 'JSON':
          content = JSON.stringify(generatedReport, null, 2);
          filename = `${generatedReport.id}.json`;
          mimeType = 'application/json';
          break;
        default:
          return;
      }
      
      // Create and download file
      const blob = new Blob([content], { type: mimeType });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      setSuccessMessage(`${format} downloaded successfully!`);
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Download failed:', error);
      setSuccessMessage('Download failed');
      setTimeout(() => setSuccessMessage(''), 3000);
    }
  };

  const generateHTMLContent = (report: Report): string => {
    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${report.title}</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }
        h1 { color: #1976d2; border-bottom: 3px solid #1976d2; padding-bottom: 10px; }
        h2 { color: #333; margin-top: 30px; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border: 1px solid #ddd; }
        th { background-color: #1976d2; color: white; }
        tr:nth-child(even) { background-color: #f9f9f9; }
        .summary { background: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: bold; }
        .high { background: #ffebee; color: #c62828; }
        .medium { background: #fff3e0; color: #ef6c00; }
        .low { background: #e8f5e9; color: #2e7d32; }
    </style>
</head>
<body>
    <h1>${report.title}</h1>
    <p><strong>Report ID:</strong> ${report.id}</p>
    <p><strong>Generated:</strong> ${new Date(report.generatedAt).toLocaleString()}</p>
    
    <div class="summary">
        <h2>Summary</h2>
        <p>${report.summary}</p>
    </div>
    
    ${generateHTMLReportContent(report)}
</body>
</html>`;
  };

  const generateHTMLReportContent = (report: Report): string => {
    const data = report.data;
    
    if (report.reportType === 'Performance') {
      return `
        <h2>Performance Metrics</h2>
        <table>
            <tr><td><strong>Total Procedures:</strong></td><td>${data.totalProcedures}</td></tr>
            <tr><td><strong>Average Score:</strong></td><td>${data.averageScore}%</td></tr>
            <tr><td><strong>Critical Issues:</strong></td><td>${data.criticalIssues}</td></tr>
            <tr><td><strong>Warnings:</strong></td><td>${data.warnings}</td></tr>
        </table>
        
        <h2>Top Performance Issues</h2>
        <table>
            <thead>
                <tr>
                    <th>Procedure</th>
                    <th>Issue</th>
                    <th>Impact</th>
                    <th>Improvement</th>
                </tr>
            </thead>
            <tbody>
                ${data.topIssues.map((issue: any) => `
                    <tr>
                        <td>${issue.procedure}</td>
                        <td>${issue.issue}</td>
                        <td><span class="badge ${issue.impact.toLowerCase()}">${issue.impact}</span></td>
                        <td>${issue.improvement}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>`;
    } else if (report.reportType === 'DatabaseSummary') {
      return `
        <h2>Database Statistics</h2>
        <table>
            <tr><td><strong>Total Tables:</strong></td><td>${data.totalTables}</td></tr>
            <tr><td><strong>Total Procedures:</strong></td><td>${data.totalProcedures}</td></tr>
            <tr><td><strong>Total Indexes:</strong></td><td>${data.totalIndexes}</td></tr>
            <tr><td><strong>Database Size:</strong></td><td>${data.databaseSize}</td></tr>
        </table>
        
        <h2>Top Tables by Size</h2>
        <table>
            <thead>
                <tr>
                    <th>Table Name</th>
                    <th>Rows</th>
                    <th>Size</th>
                    <th>Indexes</th>
                </tr>
            </thead>
            <tbody>
                ${data.tableStats.map((table: any) => `
                    <tr>
                        <td>${table.name}</td>
                        <td>${table.rows.toLocaleString()}</td>
                        <td>${table.size}</td>
                        <td>${table.indexes}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>`;
    } else if (report.reportType === 'Optimization') {
      return `
        <h2>Optimization Overview</h2>
        <table>
            <tr><td><strong>Total Recommendations:</strong></td><td>${data.totalRecommendations}</td></tr>
            <tr><td><strong>High Priority:</strong></td><td>${data.highPriority}</td></tr>
            <tr><td><strong>Medium Priority:</strong></td><td>${data.mediumPriority}</td></tr>
            <tr><td><strong>Estimated Improvement:</strong></td><td>${data.estimatedImprovement}</td></tr>
        </table>
        
        <h2>Top Recommendations</h2>
        <table>
            <thead>
                <tr>
                    <th>Recommendation</th>
                    <th>Priority</th>
                    <th>Impact</th>
                    <th>Effort</th>
                </tr>
            </thead>
            <tbody>
                ${data.recommendations.map((rec: any) => `
                    <tr>
                        <td>
                            <strong>${rec.title}</strong><br/>
                            <small>${rec.description}</small>
                        </td>
                        <td><span class="badge ${rec.priority.toLowerCase()}">${rec.priority}</span></td>
                        <td>${rec.impact}</td>
                        <td>${rec.effort}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>`;
    }
    
    return '';
  };

  const generatePDFContent = (report: Report): string => {
    return `PDF Report: ${report.title}\n\nReport ID: ${report.id}\nGenerated: ${new Date(report.generatedAt).toLocaleString()}\n\n${report.summary}\n\nNote: This is a text representation. In production, use a PDF library like jsPDF for proper PDF generation.`;
  };

  const generateExcelContent = (report: Report): string => {
    let csv = `${report.title}\nReport ID,${report.id}\nGenerated,${new Date(report.generatedAt).toLocaleString()}\n\n`;
    
    if (report.reportType === 'Performance' && report.data.topIssues) {
      csv += 'Procedure,Issue,Impact,Improvement\n';
      report.data.topIssues.forEach((issue: any) => {
        csv += `${issue.procedure},${issue.issue},${issue.impact},${issue.improvement}\n`;
      });
    }
    
    return csv;
  };

  const handlePreview = (report: Report) => {
    setPreviewReport(report);
    setPreviewOpen(true);
  };

  const reportTypes = [
    {
      title: 'Performance Report',
      description: 'Comprehensive analysis of query performance metrics and execution statistics',
      icon: <AssessmentIcon sx={{ fontSize: 40 }} />,
      color: 'primary',
      type: 'Performance' as ReportType,
    },
    {
      title: 'Database Summary',
      description: 'Overview of database objects, storage statistics, and schema information',
      icon: <HtmlIcon sx={{ fontSize: 40 }} />,
      color: 'success',
      type: 'DatabaseSummary' as ReportType,
    },
    {
      title: 'Optimization Report',
      description: 'Detailed recommendations, priority analysis, and improvement strategies',
      icon: <PdfIcon sx={{ fontSize: 40 }} />,
      color: 'error',
      type: 'Optimization' as ReportType,
    },
  ];

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        {/* Header */}
        <Fade in timeout={500}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <AssessmentIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
                <Box>
                  <Typography variant="h4" component="h1" fontWeight={600}>
                    Reports
                  </Typography>
                  <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                    Generate and export analytical reports
                  </Typography>
                </Box>
              </Box>
            </Box>

            <Divider sx={{ mb: 4 }} />
          </Box>
        </Fade>

        {/* Success Message */}
        {successMessage && (
          <Fade in>
            <Alert 
              severity="success" 
              icon={<CheckCircleIcon />}
              sx={{ mb: 3 }}
              onClose={() => setSuccessMessage('')}
            >
              {successMessage}
            </Alert>
          </Fade>
        )}

        {/* Report Types */}
        <Fade in timeout={700}>
          <Grid container spacing={3} sx={{ mb: 4 }}>
            {reportTypes.map((report, index) => (
              <Grid item xs={12} md={4} key={report.title}>
                <Fade in timeout={700 + index * 200}>
                  <Card 
                    sx={{ 
                      height: '100%',
                      transition: 'transform 0.2s, box-shadow 0.2s',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: 6,
                      },
                    }}
                  >
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ color: `${report.color}.main`, mb: 2 }}>
                        {report.icon}
                      </Box>
                      <Typography variant="h6" gutterBottom fontWeight={600}>
                        {report.title}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                        {report.description}
                      </Typography>
                      <Button
                        variant="contained"
                        color={report.color as any}
                        fullWidth
                        onClick={() => generateReportMutation.mutate(report.type)}
                        disabled={generateReportMutation.isPending}
                        startIcon={generateReportMutation.isPending ? <CircularProgress size={20} /> : undefined}
                      >
                        {generateReportMutation.isPending ? 'Generating...' : 'Generate'}
                      </Button>
                    </CardContent>
                  </Card>
                </Fade>
              </Grid>
            ))}
          </Grid>
        </Fade>

        {/* Latest Report */}
        <Fade in timeout={1000}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Latest Report
              </Typography>
              
              {generateReportMutation.isPending ? (
                <Stack spacing={2} sx={{ mt: 2 }}>
                  <Skeleton variant="rectangular" height={60} />
                  <Skeleton variant="rectangular" height={200} />
                </Stack>
              ) : generateReportMutation.isSuccess && generatedReport ? (
                <Paper 
                  sx={{ 
                    p: 3, 
                    mt: 2,
                    border: '2px solid',
                    borderColor: 'success.light',
                    bgcolor: 'success.50',
                  }}
                >
                  <Stack spacing={2}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Box>
                        <Typography variant="h6" fontWeight={600}>
                          {generatedReport.title}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Generated on {new Date(generatedReport.generatedAt).toLocaleString()}
                        </Typography>
                      </Box>
                      <CheckCircleIcon color="success" sx={{ fontSize: 40 }} />
                    </Box>

                    <Divider />

                    {/* Summary Section */}
                    <Box sx={{ bgcolor: 'background.paper', p: 2, borderRadius: 1 }}>
                      <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                        Executive Summary
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {generatedReport.summary}
                      </Typography>
                    </Box>

                    <Grid container spacing={2}>
                      <Grid item xs={12} sm={4}>
                        <Typography variant="body2" color="text.secondary">
                          Report ID
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {generatedReport.id}
                        </Typography>
                      </Grid>
                      <Grid item xs={12} sm={4}>
                        <Typography variant="body2" color="text.secondary">
                          Status
                        </Typography>
                        <Chip label="Completed" color="success" size="small" />
                      </Grid>
                      <Grid item xs={12} sm={4}>
                        <Typography variant="body2" color="text.secondary">
                          Report Type
                        </Typography>
                        <Typography variant="body1" fontWeight={600}>
                          {generatedReport.reportType}
                        </Typography>
                      </Grid>
                    </Grid>

                    {/* Key Metrics based on report type */}
                    {generatedReport.reportType === 'Performance' && (
                      <Box sx={{ bgcolor: 'background.paper', p: 2, borderRadius: 1 }}>
                        <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                          Key Performance Metrics
                        </Typography>
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Total Procedures</Typography>
                            <Typography variant="h6" fontWeight={600}>{generatedReport.data.totalProcedures}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Avg Score</Typography>
                            <Typography variant="h6" fontWeight={600} color="primary.main">{generatedReport.data.averageScore}%</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Critical Issues</Typography>
                            <Typography variant="h6" fontWeight={600} color="error.main">{generatedReport.data.criticalIssues}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Warnings</Typography>
                            <Typography variant="h6" fontWeight={600} color="warning.main">{generatedReport.data.warnings}</Typography>
                          </Grid>
                        </Grid>
                      </Box>
                    )}

                    {generatedReport.reportType === 'DatabaseSummary' && (
                      <Box sx={{ bgcolor: 'background.paper', p: 2, borderRadius: 1 }}>
                        <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                          Database Statistics
                        </Typography>
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Tables</Typography>
                            <Typography variant="h6" fontWeight={600}>{generatedReport.data.totalTables}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Procedures</Typography>
                            <Typography variant="h6" fontWeight={600}>{generatedReport.data.totalProcedures}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Indexes</Typography>
                            <Typography variant="h6" fontWeight={600}>{generatedReport.data.totalIndexes}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Database Size</Typography>
                            <Typography variant="h6" fontWeight={600} color="primary.main">{generatedReport.data.databaseSize}</Typography>
                          </Grid>
                        </Grid>
                      </Box>
                    )}

                    {generatedReport.reportType === 'Optimization' && (
                      <Box sx={{ bgcolor: 'background.paper', p: 2, borderRadius: 1 }}>
                        <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                          Optimization Overview
                        </Typography>
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Total Items</Typography>
                            <Typography variant="h6" fontWeight={600}>{generatedReport.data.totalRecommendations}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">High Priority</Typography>
                            <Typography variant="h6" fontWeight={600} color="error.main">{generatedReport.data.highPriority}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Medium Priority</Typography>
                            <Typography variant="h6" fontWeight={600} color="warning.main">{generatedReport.data.mediumPriority}</Typography>
                          </Grid>
                          <Grid item xs={6} sm={3}>
                            <Typography variant="body2" color="text.secondary">Est. Improvement</Typography>
                            <Typography variant="h6" fontWeight={600} color="success.main">{generatedReport.data.estimatedImprovement}</Typography>
                          </Grid>
                        </Grid>
                      </Box>
                    )}

                    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mt: 2 }}>
                      <Button
                        variant="contained"
                        startIcon={<PdfIcon />}
                        onClick={() => handleDownload('PDF')}
                        fullWidth
                      >
                        Download PDF
                      </Button>
                      <Button
                        variant="contained"
                        startIcon={<HtmlIcon />}
                        onClick={() => handleDownload('HTML')}
                        fullWidth
                      >
                        Download HTML
                      </Button>
                      <Button
                        variant="contained"
                        startIcon={<ExcelIcon />}
                        onClick={() => handleDownload('Excel')}
                        fullWidth
                      >
                        Download CSV
                      </Button>
                      <Button
                        variant="outlined"
                        startIcon={<PreviewIcon />}
                        onClick={() => handlePreview(generatedReport)}
                        fullWidth
                      >
                        Preview Report
                      </Button>
                    </Stack>
                  </Stack>
                </Paper>
              ) : (
                <Paper sx={{ p: 6, textAlign: 'center', mt: 2 }}>
                  <AssessmentIcon sx={{ fontSize: 60, color: 'text.disabled', mb: 2 }} />
                  <Typography variant="body1" color="text.secondary">
                    No reports generated yet
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Generate your first report to get started
                  </Typography>
                </Paper>
              )}
            </CardContent>
          </Card>
        </Fade>

        {/* Preview Dialog */}
        <Dialog 
          open={previewOpen} 
          onClose={() => setPreviewOpen(false)}
          maxWidth="lg"
          fullWidth
        >
          <DialogTitle>
            <Typography variant="h6" fontWeight={600}>
              Report Preview
            </Typography>
          </DialogTitle>
          <DialogContent dividers>
            {previewReport && (
              <Box>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  {previewReport.title}
                </Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  {previewReport.summary}
                </Typography>

                {/* Performance Report Preview */}
                {previewReport.reportType === 'Performance' && (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ mt: 3 }}>
                      Top Performance Issues
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell><strong>Procedure</strong></TableCell>
                            <TableCell><strong>Issue</strong></TableCell>
                            <TableCell><strong>Impact</strong></TableCell>
                            <TableCell><strong>Improvement</strong></TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {previewReport.data.topIssues.map((issue: any, idx: number) => (
                            <TableRow key={idx}>
                              <TableCell>{issue.procedure}</TableCell>
                              <TableCell>{issue.issue}</TableCell>
                              <TableCell>
                                <Chip 
                                  label={issue.impact} 
                                  size="small"
                                  color={issue.impact === 'High' ? 'error' : issue.impact === 'Medium' ? 'warning' : 'success'}
                                />
                              </TableCell>
                              <TableCell><strong>{issue.improvement}</strong></TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>

                    <Box sx={{ mt: 3, p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
                      <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                        Execution Statistics
                      </Typography>
                      <Grid container spacing={2}>
                        <Grid item xs={6} sm={3}>
                          <Typography variant="caption" color="text.secondary">Avg Execution</Typography>
                          <Typography variant="body2" fontWeight={600}>{previewReport.data.executionStats.avgExecutionTime}</Typography>
                        </Grid>
                        <Grid item xs={6} sm={3}>
                          <Typography variant="caption" color="text.secondary">Max Execution</Typography>
                          <Typography variant="body2" fontWeight={600}>{previewReport.data.executionStats.maxExecutionTime}</Typography>
                        </Grid>
                        <Grid item xs={6} sm={3}>
                          <Typography variant="caption" color="text.secondary">Min Execution</Typography>
                          <Typography variant="body2" fontWeight={600}>{previewReport.data.executionStats.minExecutionTime}</Typography>
                        </Grid>
                        <Grid item xs={6} sm={3}>
                          <Typography variant="caption" color="text.secondary">Total Executions</Typography>
                          <Typography variant="body2" fontWeight={600}>{previewReport.data.executionStats.totalExecutions.toLocaleString()}</Typography>
                        </Grid>
                      </Grid>
                    </Box>
                  </Box>
                )}

                {/* Database Summary Preview */}
                {previewReport.reportType === 'DatabaseSummary' && (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ mt: 3 }}>
                      Top Tables by Size
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell><strong>Table Name</strong></TableCell>
                            <TableCell align="right"><strong>Rows</strong></TableCell>
                            <TableCell align="right"><strong>Size</strong></TableCell>
                            <TableCell align="right"><strong>Indexes</strong></TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {previewReport.data.tableStats.map((table: any, idx: number) => (
                            <TableRow key={idx}>
                              <TableCell>{table.name}</TableCell>
                              <TableCell align="right">{table.rows.toLocaleString()}</TableCell>
                              <TableCell align="right">{table.size}</TableCell>
                              <TableCell align="right">{table.indexes}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                )}

                {/* Optimization Report Preview */}
                {previewReport.reportType === 'Optimization' && (
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ mt: 3 }}>
                      Top Optimization Recommendations
                    </Typography>
                    <Stack spacing={2}>
                      {previewReport.data.recommendations.slice(0, 5).map((rec: any, idx: number) => (
                        <Paper key={idx} variant="outlined" sx={{ p: 2 }}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 1 }}>
                            <Typography variant="subtitle2" fontWeight={600}>
                              {rec.title}
                            </Typography>
                            <Chip 
                              label={rec.priority} 
                              size="small"
                              color={rec.priority === 'High' ? 'error' : rec.priority === 'Medium' ? 'warning' : 'success'}
                            />
                          </Box>
                          <Typography variant="body2" color="text.secondary" paragraph>
                            {rec.description}
                          </Typography>
                          <Grid container spacing={2}>
                            <Grid item xs={4}>
                              <Typography variant="caption" color="text.secondary">Impact</Typography>
                              <Typography variant="body2" fontWeight={600}>{rec.impact}</Typography>
                            </Grid>
                            <Grid item xs={4}>
                              <Typography variant="caption" color="text.secondary">Effort</Typography>
                              <Typography variant="body2" fontWeight={600}>{rec.effort}</Typography>
                            </Grid>
                            <Grid item xs={4}>
                              <Typography variant="caption" color="text.secondary">Affected Objects</Typography>
                              <Typography variant="body2" fontWeight={600}>{rec.affectedObjects.length}</Typography>
                            </Grid>
                          </Grid>
                        </Paper>
                      ))}
                    </Stack>
                  </Box>
                )}
              </Box>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setPreviewOpen(false)}>Close</Button>
            <Button 
              variant="contained" 
              startIcon={<DownloadIcon />}
              onClick={() => previewReport && handleDownload('HTML')}
            >
              Download Full Report
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Container>
  );
};
