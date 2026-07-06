import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  Card,
  CardContent,
  LinearProgress,
  Alert,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  Fade,
  Skeleton,
  Grid,
} from '@mui/material';
import {
  Analytics as AnalyticsIcon,
  PlayArrow as PlayIcon,
  Refresh as RefreshIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { useMutation, useQuery } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';
import { analysisService } from '../../core/services/analysis.service';

interface ProcedureAnalysis {
  id: number;
  procedure: string;
  score: number;
  severity: 'High' | 'Medium' | 'Low';
  improvement: string;
  status: 'Completed' | 'In Progress' | 'Pending' | 'Failed';
}

export const DatabaseAnalysisPage = () => {
  const navigate = useNavigate();
  const [connection, setConnection] = useState<any>(null);
  const [analyzing, setAnalyzing] = useState(false);
  const [progress, setProgress] = useState(0);
  const [analysisResults, setAnalysisResults] = useState<ProcedureAnalysis[]>([]);

  useState(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (currentConnection) {
      setConnection(currentConnection);
    }
  });

  const { data: procedures, isLoading: loadingProcedures } = useQuery({
    queryKey: ['procedures', connection],
    queryFn: () => connection ? databaseService.getStoredProcedures(connection) : Promise.resolve([]),
    enabled: !!connection,
  });

  const handleAnalyzeDatabase = async () => {
    if (!procedures || procedures.length === 0) return;

    setAnalyzing(true);
    setProgress(0);
    const results: ProcedureAnalysis[] = [];

    console.log('Starting database analysis. Total procedures:', procedures.length);

    for (let i = 0; i < procedures.length; i++) {
      const proc = procedures[i];
      const procAny = proc as any;
      
      // Get procedure name with fallbacks
      const procName = proc.name || procAny.procedureName || procAny.ROUTINE_NAME || `Procedure_${i}`;
      
      console.log(`Analyzing procedure ${i + 1}/${procedures.length}:`, procName, 'Raw data:', proc);
      
      // Simulate analysis
      await new Promise(resolve => setTimeout(resolve, 500));
      
      // Mock analysis result
      const score = Math.floor(Math.random() * 100);
      const severity = score >= 80 ? 'Low' : score >= 60 ? 'Medium' : 'High';
      
      results.push({
        id: i,
        procedure: procName,
        score,
        severity,
        improvement: `${Math.floor(Math.random() * 40)}%`,
        status: 'Completed',
      });

      setProgress(((i + 1) / procedures.length) * 100);
      setAnalysisResults([...results]);
    }

    console.log('Database analysis completed. Results:', results);
    setAnalyzing(false);
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High': return 'error';
      case 'Medium': return 'warning';
      case 'Low': return 'success';
      default: return 'default';
    }
  };

  const getScoreColor = (score: number) => {
    if (score >= 80) return 'success';
    if (score >= 60) return 'warning';
    return 'error';
  };

  const columns: GridColDef[] = [
    {
      field: 'procedure',
      headerName: 'Procedure',
      flex: 1,
      minWidth: 200,
    },
    {
      field: 'score',
      headerName: 'Score',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={params.value}
          size="small"
          color={getScoreColor(params.value)}
        />
      ),
    },
    {
      field: 'severity',
      headerName: 'Severity',
      width: 130,
      renderCell: (params) => (
        <Chip
          label={params.value}
          size="small"
          color={getSeverityColor(params.value) as any}
          icon={
            params.value === 'High' ? <ErrorIcon /> :
            params.value === 'Medium' ? <WarningIcon /> :
            <InfoIcon />
          }
        />
      ),
    },
    {
      field: 'improvement',
      headerName: 'Improvement',
      width: 130,
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 140,
      renderCell: (params) => (
        <Chip
          label={params.value}
          size="small"
          variant="outlined"
          color={params.value === 'Completed' ? 'success' : 'default'}
          icon={params.value === 'Completed' ? <CheckCircleIcon /> : undefined}
        />
      ),
    },
  ];

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

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        {/* Header */}
        <Fade in timeout={500}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <AnalyticsIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
                <Box>
                  <Typography variant="h4" component="h1" fontWeight={600}>
                    Database Analysis
                  </Typography>
                  <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                    Comprehensive analysis of all stored procedures
                  </Typography>
                </Box>
              </Box>
              <Stack direction="row" spacing={2}>
                <Tooltip title="Refresh procedures">
                  <IconButton color="primary" disabled={analyzing}>
                    <RefreshIcon />
                  </IconButton>
                </Tooltip>
                <Button
                  variant="contained"
                  size="large"
                  startIcon={analyzing ? undefined : <PlayIcon />}
                  onClick={handleAnalyzeDatabase}
                  disabled={analyzing || !procedures || procedures.length === 0}
                  sx={{ minWidth: 200 }}
                >
                  {analyzing ? 'Analyzing...' : 'Analyze Entire Database'}
                </Button>
              </Stack>
            </Box>

            <Divider sx={{ mb: 4 }} />
          </Box>
        </Fade>

        {/* Progress Indicator */}
        {analyzing && (
          <Fade in>
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Stack spacing={2}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                      Analysis in Progress...
                    </Typography>
                    <Typography variant="h6" color="primary">
                      {Math.round(progress)}%
                    </Typography>
                  </Box>
                  <LinearProgress 
                    variant="determinate" 
                    value={progress} 
                    sx={{ height: 8, borderRadius: 1 }}
                  />
                  <Typography variant="body2" color="text.secondary">
                    Analyzing {analysisResults.length} of {procedures?.length || 0} procedures
                  </Typography>
                </Stack>
              </CardContent>
            </Card>
          </Fade>
        )}

        {/* Summary Cards */}
        {analysisResults.length > 0 && (
          <Fade in timeout={800}>
            <Grid container spacing={3} sx={{ mb: 4 }}>
              <Grid item xs={12} sm={6} md={3}>
                <Card>
                  <CardContent>
                    <Typography color="text.secondary" variant="overline">
                      Total Analyzed
                    </Typography>
                    <Typography variant="h3" fontWeight={600}>
                      {analysisResults.length}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card>
                  <CardContent>
                    <Typography color="text.secondary" variant="overline">
                      Average Score
                    </Typography>
                    <Typography variant="h3" fontWeight={600} color="success.main">
                      {Math.round(analysisResults.reduce((sum, r) => sum + r.score, 0) / analysisResults.length)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card>
                  <CardContent>
                    <Typography color="text.secondary" variant="overline">
                      Critical Issues
                    </Typography>
                    <Typography variant="h3" fontWeight={600} color="error.main">
                      {analysisResults.filter(r => r.severity === 'High').length}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card>
                  <CardContent>
                    <Typography color="text.secondary" variant="overline">
                      Avg Improvement
                    </Typography>
                    <Typography variant="h3" fontWeight={600} color="primary.main">
                      {Math.round(analysisResults.reduce((sum, r) => sum + parseInt(r.improvement), 0) / analysisResults.length)}%
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Fade>
        )}

        {/* Results Table */}
        <Fade in timeout={1000}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Analysis Results
              </Typography>
              {loadingProcedures ? (
                <Stack spacing={1}>
                  <Skeleton variant="rectangular" height={60} />
                  <Skeleton variant="rectangular" height={400} />
                </Stack>
              ) : (
                <Box sx={{ height: 600, width: '100%', mt: 2 }}>
                  <DataGrid
                    rows={analysisResults}
                    columns={columns}
                    pageSizeOptions={[10, 25, 50, 100]}
                    disableRowSelectionOnClick
                    onRowClick={(params) => {
                      navigate(`/procedures/${encodeURIComponent(params.row.procedure)}`);
                    }}
                    sx={{
                      '& .MuiDataGrid-row': {
                        cursor: 'pointer',
                        '&:hover': {
                          backgroundColor: 'action.hover',
                        },
                      },
                    }}
                  />
                </Box>
              )}
            </CardContent>
          </Card>
        </Fade>
      </Box>
    </Container>
  );
};
