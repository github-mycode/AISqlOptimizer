import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  Container,
  Grid,
  Typography,
  Divider,
  Paper,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  Storage as StorageIcon,
  TableChart as TableIcon,
  ViewList as ViewIcon,
  Code as CodeIcon,
  Speed as SpeedIcon,
  Warning as WarningIcon,
  TrendingUp as TrendingUpIcon,
} from '@mui/icons-material';
import { Chart as ChartJS, ArcElement, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';
import { Doughnut, Bar } from 'react-chartjs-2';
import { databaseService } from '../../core/services/database.service';

ChartJS.register(ArcElement, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

export const DashboardPage = () => {
  const navigate = useNavigate();
  const [connection, setConnection] = useState<any>(null);
  const [stats, setStats] = useState({
    databaseName: 'Not Connected',
    procedureCount: 0,
    tableCount: 0,
    viewCount: 0,
    avgPerformanceScore: 0,
    criticalIssues: 0,
  });

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    if (currentConnection) {
      setConnection(currentConnection);
      const dbName = localStorage.getItem('connectedDatabase') || 'Unknown';
      setStats(prev => ({ ...prev, databaseName: dbName }));
      
      // Load stats
      loadStats(currentConnection);
    }
  }, []);

  const loadStats = async (conn: any) => {
    try {
      const [tables, views, procedures] = await Promise.all([
        databaseService.getTables(conn),
        databaseService.getViews(conn),
        databaseService.getStoredProcedures(conn),
      ]);

      setStats(prev => ({
        ...prev,
        tableCount: tables?.length || 0,
        viewCount: views?.length || 0,
        procedureCount: procedures?.length || 0,
        avgPerformanceScore: 78, // Mock data
        criticalIssues: 3, // Mock data
      }));
    } catch (error) {
      console.error('Failed to load stats:', error);
    }
  };

  const performanceChartData = {
    labels: ['Excellent (80-100)', 'Good (60-79)', 'Needs Work (0-59)'],
    datasets: [
      {
        data: [12, 8, 5],
        backgroundColor: [
          'rgba(76, 175, 80, 0.8)',
          'rgba(255, 152, 0, 0.8)',
          'rgba(244, 67, 54, 0.8)',
        ],
        borderColor: [
          'rgba(76, 175, 80, 1)',
          'rgba(255, 152, 0, 1)',
          'rgba(244, 67, 54, 1)',
        ],
        borderWidth: 2,
      },
    ],
  };

  const issueChartData = {
    labels: ['Critical', 'Medium', 'Low', 'Info'],
    datasets: [
      {
        data: [3, 8, 12, 5],
        backgroundColor: [
          'rgba(244, 67, 54, 0.8)',
          'rgba(255, 152, 0, 0.8)',
          'rgba(33, 150, 243, 0.8)',
          'rgba(158, 158, 158, 0.8)',
        ],
        borderColor: [
          'rgba(244, 67, 54, 1)',
          'rgba(255, 152, 0, 1)',
          'rgba(33, 150, 243, 1)',
          'rgba(158, 158, 158, 1)',
        ],
        borderWidth: 2,
      },
    ],
  };

  const recentAnalyses = [
    { name: 'GetCustomerOrders', score: 85, issues: 2, date: '2026-07-05' },
    { name: 'UpdateInventory', score: 62, issues: 5, date: '2026-07-05' },
    { name: 'ProcessPayment', score: 45, issues: 8, date: '2026-07-04' },
    { name: 'GenerateReport', score: 78, issues: 3, date: '2026-07-04' },
    { name: 'ValidateUser', score: 92, issues: 1, date: '2026-07-03' },
  ];

  const mainStats = [
    {
      title: 'Database',
      value: stats.databaseName,
      icon: <StorageIcon sx={{ fontSize: 40 }} />,
      color: 'primary',
    },
    {
      title: 'Stored Procedures',
      value: stats.procedureCount,
      icon: <CodeIcon sx={{ fontSize: 40 }} />,
      color: 'success',
    },
    {
      title: 'Tables',
      value: stats.tableCount,
      icon: <TableIcon sx={{ fontSize: 40 }} />,
      color: 'info',
    },
    {
      title: 'Views',
      value: stats.viewCount,
      icon: <ViewIcon sx={{ fontSize: 40 }} />,
      color: 'warning',
    },
    {
      title: 'Avg Performance',
      value: `${stats.avgPerformanceScore}%`,
      icon: <SpeedIcon sx={{ fontSize: 40 }} />,
      color: 'secondary',
    },
    {
      title: 'Critical Issues',
      value: stats.criticalIssues,
      icon: <WarningIcon sx={{ fontSize: 40 }} />,
      color: 'error',
    },
  ];

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <DashboardIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
          <Box>
            <Typography variant="h4" component="h1" fontWeight={600}>
              Dashboard
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
              Overview of your database optimization metrics
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {/* Stats Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {mainStats.map((stat) => (
            <Grid item xs={12} sm={6} md={4} lg={2} key={stat.title}>
              <Card>
                <CardContent>
                  <Box
                    sx={{
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      textAlign: 'center',
                    }}
                  >
                    <Box sx={{ color: `${stat.color}.main`, mb: 1 }}>
                      {stat.icon}
                    </Box>
                    <Typography variant="h4" fontWeight={600}>
                      {stat.value}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                      {stat.title}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {/* Charts */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  Performance Score Distribution
                </Typography>
                <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <Doughnut
                    data={performanceChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'bottom',
                        },
                      },
                    }}
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                  Issue Distribution
                </Typography>
                <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <Doughnut
                    data={issueChartData}
                    options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'bottom',
                        },
                      },
                    }}
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Recent Analyses Table */}
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" fontWeight={600}>
                Recent Analyses
              </Typography>
              <Button
                variant="outlined"
                size="small"
                startIcon={<TrendingUpIcon />}
                onClick={() => navigate('/procedures')}
              >
                View All
              </Button>
            </Box>
            <TableContainer component={Paper} variant="outlined">
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Procedure Name</strong></TableCell>
                    <TableCell align="center"><strong>Score</strong></TableCell>
                    <TableCell align="center"><strong>Issues</strong></TableCell>
                    <TableCell align="center"><strong>Date</strong></TableCell>
                    <TableCell align="center"><strong>Status</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {recentAnalyses.map((analysis) => (
                    <TableRow key={analysis.name} hover>
                      <TableCell>{analysis.name}</TableCell>
                      <TableCell align="center">
                        <Chip
                          label={analysis.score}
                          size="small"
                          color={
                            analysis.score >= 80
                              ? 'success'
                              : analysis.score >= 60
                              ? 'warning'
                              : 'error'
                          }
                        />
                      </TableCell>
                      <TableCell align="center">{analysis.issues}</TableCell>
                      <TableCell align="center">{analysis.date}</TableCell>
                      <TableCell align="center">
                        <Chip
                          label={analysis.score >= 80 ? 'Optimized' : 'Needs Work'}
                          size="small"
                          variant="outlined"
                          color={analysis.score >= 80 ? 'success' : 'warning'}
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};
