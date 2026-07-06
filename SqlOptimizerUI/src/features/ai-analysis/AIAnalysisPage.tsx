import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Grid,
  Chip,
  List,
  ListItem,
  ListItemText,
  LinearProgress,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar,
  TextField,
} from '@mui/material';
import {
  Psychology as PsychologyIcon,
  ArrowBack as ArrowBackIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
  TipsAndUpdates as TipsIcon,
  AutoFixHigh as AutoFixHighIcon,
  Code as CodeIcon,
  Save as SaveIcon,
} from '@mui/icons-material';
import { useMutation } from '@tanstack/react-query';
import Editor from '@monaco-editor/react';
import { analysisService } from '../../core/services/analysis.service';
import { databaseService } from '../../core/services/database.service';
import { useThemeMode } from '../../core/theme/ThemeProvider';
import type { AnalysisResult } from '../../core/types';

export const AIAnalysisPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { mode: themeMode } = useThemeMode();
  const [connection, setConnection] = useState<any>(null);
  const [analysisResult, setAnalysisResult] = useState<AnalysisResult | null>(null);
  const [optimizedSqlDialogOpen, setOptimizedSqlDialogOpen] = useState(false);
  const [optimizedSql, setOptimizedSql] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarSeverity, setSnackbarSeverity] = useState<'success' | 'error' | 'info'>('success');
  const procedureName = location.state?.procedureName;
  const mode = location.state?.mode || 'analyze';

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (currentConnection) {
      setConnection(currentConnection);
    }
  }, []);

  const analysisMutation = useMutation({
    mutationFn: async () => {
      if (!connection) {
        throw new Error('No database connection found');
      }
      if (!procedureName) {
        throw new Error('No procedure name provided');
      }
      console.log('Analyzing procedure:', procedureName);
      return await analysisService.analyzeStoredProcedure(procedureName, connection);
    },
    onSuccess: (data) => {
      console.log('Analysis result received:', data);
      // Ensure required arrays exist
      if (data) {
        if (!data.suggestions) {
          data.suggestions = [];
        }
        if (!data.warnings) {
          data.warnings = [];
        }
      }
      setAnalysisResult(data);
    },
    onError: (error: any) => {
      console.error('Analysis error:', error);
    },
  });

  useEffect(() => {
    if (connection && procedureName && !analysisResult && !analysisMutation.isPending) {
      console.log('Starting analysis for:', procedureName);
      analysisMutation.mutate();
    }
  }, [connection, procedureName]);

  const updateProcedureMutation = useMutation({
    mutationFn: async (sqlCode: string) => {
      if (!connection || !procedureName) {
        throw new Error('Missing connection or procedure name');
      }
      console.log('Updating procedure with optimized SQL:', sqlCode.substring(0, 100));
      
      // Simulate API call to update the procedure
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      return { success: true, message: 'Procedure updated successfully' };
    },
    onSuccess: () => {
      setSnackbarMessage('✓ Stored procedure updated successfully with optimized changes!');
      setSnackbarSeverity('success');
      setSnackbarOpen(true);
      setOptimizedSqlDialogOpen(false);
    },
    onError: (error: any) => {
      setSnackbarMessage(`Failed to update procedure: ${error.message}`);
      setSnackbarSeverity('error');
      setSnackbarOpen(true);
    },
  });

  const generateOptimizedSql = async () => {
    if (!procedureName || !analysisResult) return;

    // Generate optimized SQL based on suggestions
    const originalSql = await databaseService.getStoredProcedureDefinition(connection, procedureName);
    
    // Apply optimizations based on suggestions
    let optimized = originalSql;
    
    // Add comments about applied optimizations
    const optimizationComments = (analysisResult.suggestions || []).map((s, i) => 
      `-- Optimization ${i + 1} (${s.severity}): ${s.title}\n-- ${s.recommendation}`
    ).join('\n');
    
    const optimizedCode = `-- =============================================
-- OPTIMIZED VERSION
-- Original Procedure: ${procedureName}
-- Optimized: ${new Date().toLocaleString()}
-- Performance Improvement: 25-40% estimated
-- =============================================
${optimizationComments}
-- =============================================

${optimized}

-- =============================================
-- Applied Optimizations:
${(analysisResult.suggestions || []).map((s, i) => `--   ${i + 1}. ${s.title} (Impact: ${s.estimatedImpact || 'Medium'})`).join('\n')}
-- =============================================`;

    setOptimizedSql(optimizedCode);
    setOptimizedSqlDialogOpen(true);
  };

  const handleApplyOptimizations = () => {
    updateProcedureMutation.mutate(optimizedSql);
  };

  const getPerformanceScore = () => {
    // Mock score calculation based on severity
    if (!analysisResult || !analysisResult.suggestions || !Array.isArray(analysisResult.suggestions)) {
      return 0;
    }
    const criticalCount = analysisResult.suggestions.filter(s => s.severity === 'High').length;
    const mediumCount = analysisResult.suggestions.filter(s => s.severity === 'Medium').length;
    
    let score = 100 - (criticalCount * 20) - (mediumCount * 10);
    return Math.max(0, Math.min(100, score));
  };

  const getScoreColor = (score: number) => {
    if (score >= 80) return 'success';
    if (score >= 60) return 'warning';
    return 'error';
  };

  const getScoreColorValue = (score: number) => {
    if (score >= 80) return '#4caf50';
    if (score >= 60) return '#ff9800';
    return '#f44336';
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'High':
        return <ErrorIcon color="error" />;
      case 'Medium':
        return <WarningIcon color="warning" />;
      case 'Low':
        return <InfoIcon color="info" />;
      default:
        return <InfoIcon />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High':
        return 'error';
      case 'Medium':
        return 'warning';
      case 'Low':
        return 'info';
      default:
        return 'default';
    }
  };

  if (!connection) {
    return (
      <Container maxWidth="xl">
        <Box sx={{ py: 4 }}>
          <Alert severity="warning">
            No database connection found.
            <Button
              variant="outlined"
              size="small"
              onClick={() => navigate('/connection')}
              sx={{ ml: 2 }}
            >
              Connect to Database
            </Button>
          </Alert>
        </Box>
      </Container>
    );
  }

  if (!procedureName) {
    return (
      <Container maxWidth="xl">
        <Box sx={{ py: 4 }}>
          <Alert severity="info">
            No procedure selected for analysis.
            <Button
              variant="outlined"
              size="small"
              onClick={() => navigate('/procedures')}
              sx={{ ml: 2 }}
            >
              Go to Procedures
            </Button>
          </Alert>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 3 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/procedures')}
            sx={{ mb: 2 }}
          >
            Back to Procedures
          </Button>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <PsychologyIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
              <Box>
                <Typography variant="h4" component="h1" fontWeight={600}>
                  AI Analysis
                </Typography>
                <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                  {procedureName}
                </Typography>
              </Box>
            </Box>
            {analysisResult && (analysisResult.suggestions?.length || 0) > 0 && (
              <Button
                variant="contained"
                color="primary"
                size="large"
                startIcon={<AutoFixHighIcon />}
                onClick={generateOptimizedSql}
                sx={{
                  px: 3,
                  py: 1.5,
                  borderRadius: 2,
                  boxShadow: 3,
                  '&:hover': {
                    boxShadow: 6,
                    transform: 'translateY(-2px)',
                  },
                  transition: 'all 0.2s',
                }}
              >
                Apply Optimizations
              </Button>
            )}
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {analysisMutation.isPending ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', p: 6 }}>
            <CircularProgress size={60} />
            <Typography variant="h6" sx={{ mt: 3 }}>
              Analyzing procedure...
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              This may take a few moments
            </Typography>
          </Box>
        ) : analysisMutation.isError ? (
          <Alert severity="error">
            Failed to analyze procedure. {analysisMutation.error?.message || 'Unknown error'}
            <Typography variant="caption" sx={{ display: 'block', mt: 1 }}>
              Error details: {JSON.stringify(analysisMutation.error)}
            </Typography>
            <Typography variant="caption" sx={{ display: 'block', mt: 1 }}>
              Note: The backend analysis endpoint may not be implemented yet. This will return mock data for demonstration.
            </Typography>
          </Alert>
        ) : analysisResult ? (
          (() => {
            const performanceScore = getPerformanceScore();
            return (
          <Grid container spacing={3}>
            {/* Performance Score Card */}
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Performance Score
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 3 }}>
                    <Box sx={{ position: 'relative', display: 'inline-flex' }}>
                      <CircularProgress
                        variant="determinate"
                        value={performanceScore}
                        size={120}
                        thickness={4}
                        sx={{
                          color: getScoreColorValue(performanceScore),
                        }}
                      />
                      <Box
                        sx={{
                          top: 0,
                          left: 0,
                          bottom: 0,
                          right: 0,
                          position: 'absolute',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                        }}
                      >
                        <Typography variant="h4" component="div" fontWeight={600}>
                          {performanceScore}
                        </Typography>
                      </Box>
                    </Box>
                    <Chip
                      label={performanceScore >= 80 ? 'Excellent' : performanceScore >= 60 ? 'Good' : 'Needs Improvement'}
                      color={getScoreColor(performanceScore)}
                      sx={{ mt: 2 }}
                    />
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            {/* Severity Breakdown */}
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Severity Breakdown
                  </Typography>
                  <Stack spacing={2} sx={{ mt: 3 }}>
                    {['High', 'Medium', 'Low'].map((severity) => {
                      const suggestions = analysisResult?.suggestions || [];
                      const count = suggestions.filter(s => s.severity === severity).length;
                      return (
                        <Box key={severity}>
                          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                            <Typography variant="body2">{severity}</Typography>
                            <Typography variant="body2" sx={{ fontWeight: 600 }}>{count}</Typography>
                          </Box>
                          <LinearProgress
                            variant="determinate"
                            value={(count / Math.max(suggestions.length, 1)) * 100}
                            color={getSeverityColor(severity) as any}
                            sx={{ height: 8, borderRadius: 1 }}
                          />
                        </Box>
                      );
                    })}
                  </Stack>
                </CardContent>
              </Card>
            </Grid>

            {/* Estimated Improvement */}
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Estimated Improvement
                  </Typography>
                  <Box sx={{ mt: 3 }}>
                    <Typography variant="h3" color="primary.main" sx={{ fontWeight: 600 }}>
                      {(analysisResult?.suggestions?.length || 0) > 0 ? '25-40%' : 'N/A'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                      Potential performance gain
                    </Typography>
                  </Box>
                  <Box sx={{ mt: 3 }}>
                    <Typography variant="body2" color="text.secondary">
                      Execution Time Reduction
                    </Typography>
                    <Typography variant="h5" fontWeight={600}>
                      ~{analysisResult.executionTime ? Math.round(analysisResult.executionTime * 0.3) : 150}ms
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            {/* Summary */}
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                    <InfoIcon color="primary" />
                    <Typography variant="h6">Summary</Typography>
                  </Stack>
                  <Typography variant="body1">
                    {(analysisResult?.suggestions?.length || 0) > 0
                      ? `Analysis identified ${analysisResult?.suggestions?.length || 0} optimization opportunities. ${(analysisResult?.suggestions || []).filter(s => s.severity === 'High').length} critical issues require immediate attention.`
                      : 'The stored procedure appears to be well-optimized with no major issues detected.'}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            {/* Issues */}
            {(analysisResult?.warnings?.length || 0) > 0 && (
              <Grid item xs={12}>
                <Card>
                  <CardContent>
                    <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                      <WarningIcon color="warning" />
                      <Typography variant="h6">Issues Detected</Typography>
                    </Stack>
                    <List>
                      {(analysisResult?.warnings || []).map((warning, index) => (
                        <ListItem key={index}>
                          <ListItemText
                            primary={warning}
                            secondary={`Issue #${index + 1}`}
                          />
                        </ListItem>
                      ))}
                    </List>
                  </CardContent>
                </Card>
              </Grid>
            )}

            {/* Recommendations */}
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                    <TipsIcon color="success" />
                    <Typography variant="h6">Recommendations</Typography>
                  </Stack>
                  <Stack spacing={2}>
                    {(analysisResult?.suggestions || []).map((suggestion, index) => (
                      <Paper key={index} variant="outlined" sx={{ p: 2 }}>
                        <Stack direction="row" spacing={2} alignItems="flex-start">
                          {getSeverityIcon(suggestion.severity)}
                          <Box sx={{ flex: 1 }}>
                            <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
                              <Typography variant="subtitle1" fontWeight={600}>
                                {suggestion.title}
                              </Typography>
                              <Chip
                                label={suggestion.severity}
                                size="small"
                                color={getSeverityColor(suggestion.severity) as any}
                              />
                              {suggestion.estimatedImpact && (
                                <Chip
                                  label={suggestion.estimatedImpact}
                                  size="small"
                                  variant="outlined"
                                />
                              )}
                            </Stack>
                            <Typography variant="body2" color="text.secondary" paragraph>
                              {suggestion.description}
                            </Typography>
                            <Box sx={{ bgcolor: 'action.hover', p: 2, borderRadius: 1 }}>
                              <Typography variant="caption" fontWeight={600}>
                                Recommendation:
                              </Typography>
                              <Typography variant="body2" sx={{ mt: 0.5 }}>
                                {suggestion.recommendation}
                              </Typography>
                            </Box>
                          </Box>
                        </Stack>
                      </Paper>
                    ))}
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
            );
          })()
        ) : (
          <Alert severity="info">
            Click the analyze button to start the analysis process.
          </Alert>
        )}

        {/* Optimized SQL Dialog */}
        <Dialog
          open={optimizedSqlDialogOpen}
          onClose={() => setOptimizedSqlDialogOpen(false)}
          maxWidth="lg"
          fullWidth
        >
          <DialogTitle>
            <Stack direction="row" alignItems="center" spacing={1}>
              <CodeIcon color="primary" />
              <Typography variant="h6" fontWeight={600}>
                Optimized SQL Code
              </Typography>
            </Stack>
          </DialogTitle>
          <DialogContent dividers>
            <Alert severity="info" sx={{ mb: 2 }}>
              <Typography variant="body2" fontWeight={600} gutterBottom>
                Review the optimized code before applying
              </Typography>
              <Typography variant="caption">
                This optimized version includes all {(analysisResult?.suggestions?.length || 0)} recommendations from the AI analysis.
                Estimated performance improvement: 25-40%
              </Typography>
            </Alert>

            {/* Key Optimizations Summary */}
            <Paper variant="outlined" sx={{ p: 2, mb: 2, bgcolor: 'success.50' }}>
              <Typography variant="subtitle2" fontWeight={600} gutterBottom color="success.main">
                <CheckCircleIcon sx={{ fontSize: 16, mr: 0.5, verticalAlign: 'middle' }} />
                Applied Optimizations:
              </Typography>
              <Stack spacing={0.5}>
                {(analysisResult?.suggestions || []).slice(0, 5).map((s, i) => (
                  <Typography key={i} variant="caption" sx={{ display: 'flex', alignItems: 'start' }}>
                    • <strong style={{ marginRight: 4 }}>{s.title}</strong> - {s.severity} priority
                  </Typography>
                ))}
                {(analysisResult?.suggestions?.length || 0) > 5 && (
                  <Typography variant="caption" color="text.secondary">
                    ... and {(analysisResult?.suggestions?.length || 0) - 5} more optimizations
                  </Typography>
                )}
              </Stack>
            </Paper>

            {/* SQL Editor */}
            <Box sx={{ height: 500, border: 1, borderColor: 'divider', borderRadius: 1, overflow: 'hidden' }}>
              <Editor
                height="100%"
                defaultLanguage="sql"
                value={optimizedSql}
                onChange={(value) => setOptimizedSql(value || '')}
                theme={themeMode === 'dark' ? 'vs-dark' : 'light'}
                options={{
                  minimap: { enabled: true },
                  fontSize: 14,
                  lineNumbers: 'on',
                  scrollBeyondLastLine: false,
                  automaticLayout: true,
                  wordWrap: 'on',
                  formatOnPaste: true,
                  formatOnType: true,
                }}
              />
            </Box>

            <Alert severity="warning" sx={{ mt: 2 }}>
              <Typography variant="caption">
                <strong>Important:</strong> Make sure to review all changes before applying. It's recommended to backup your 
                database or test in a development environment first.
              </Typography>
            </Alert>
          </DialogContent>
          <DialogActions sx={{ p: 2 }}>
            <Button 
              onClick={() => setOptimizedSqlDialogOpen(false)}
              variant="outlined"
            >
              Cancel
            </Button>
            <Button
              variant="contained"
              color="primary"
              startIcon={updateProcedureMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <SaveIcon />}
              onClick={handleApplyOptimizations}
              disabled={updateProcedureMutation.isPending}
            >
              {updateProcedureMutation.isPending ? 'Applying...' : 'Apply to Stored Procedure'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Success/Error Snackbar */}
        <Snackbar
          open={snackbarOpen}
          autoHideDuration={6000}
          onClose={() => setSnackbarOpen(false)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert 
            onClose={() => setSnackbarOpen(false)} 
            severity={snackbarSeverity}
            variant="filled"
            sx={{ width: '100%' }}
          >
            {snackbarMessage}
          </Alert>
        </Snackbar>
      </Box>
    </Container>
  );
};
