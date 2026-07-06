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
  Grid,
  Paper,
  Chip,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  CompareArrows as CompareIcon,
  ContentCopy as CopyIcon,
  Download as DownloadIcon,
  Check as CheckIcon,
} from '@mui/icons-material';
import Editor from '@monaco-editor/react';
import { useThemeMode } from '../../core/theme/ThemeProvider';

const originalSQL = `CREATE PROCEDURE GetCustomerOrders
    @CustomerId INT
AS
BEGIN
    SELECT * FROM Orders 
    WHERE CustomerId = @CustomerId
    ORDER BY OrderDate
END`;

const optimizedSQL = `CREATE PROCEDURE GetCustomerOrders
    @CustomerId INT
AS
BEGIN
    -- Added index hint and specific columns
    SELECT 
        OrderId,
        OrderDate,
        TotalAmount,
        Status
    FROM Orders WITH (INDEX(IX_CustomerId_OrderDate))
    WHERE CustomerId = @CustomerId
    ORDER BY OrderDate
    OPTION (MAXDOP 1)
END`;

export const ComparisonPage = () => {
  const navigate = useNavigate();
  const { mode } = useThemeMode();
  const [copiedOriginal, setCopiedOriginal] = useState(false);
  const [copiedOptimized, setCopiedOptimized] = useState(false);

  const handleCopy = (text: string, side: 'original' | 'optimized') => {
    navigator.clipboard.writeText(text);
    if (side === 'original') {
      setCopiedOriginal(true);
      setTimeout(() => setCopiedOriginal(false), 2000);
    } else {
      setCopiedOptimized(true);
      setTimeout(() => setCopiedOptimized(false), 2000);
    }
  };

  const handleDownload = (text: string, filename: string) => {
    const blob = new Blob([text], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 3 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/ai-analysis')}
            sx={{ mb: 2 }}
          >
            Back to Analysis
          </Button>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <CompareIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
            <Box>
              <Typography variant="h4" component="h1" fontWeight={600}>
                SQL Comparison
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                Compare original and optimized SQL side by side
              </Typography>
            </Box>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {/* Optimization Summary */}
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Estimated Improvement
                </Typography>
                <Typography variant="h3" color="success.main" fontWeight={600}>
                  35%
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                  Performance gain expected
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Execution Time
                </Typography>
                <Stack direction="row" spacing={2} alignItems="baseline">
                  <Typography variant="h4" color="error.main" fontWeight={600}>
                    450ms
                  </Typography>
                  <Typography variant="h5" color="text.secondary">→</Typography>
                  <Typography variant="h4" color="success.main" fontWeight={600}>
                    290ms
                  </Typography>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Optimization Summary
                </Typography>
                <Stack spacing={1} sx={{ mt: 1 }}>
                  <Chip label="Index Hint Added" size="small" color="success" />
                  <Chip label="Column Selection" size="small" color="success" />
                  <Chip label="Query Hint Added" size="small" color="success" />
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Split Screen Comparison */}
        <Grid container spacing={3}>
          {/* Original SQL */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
                  <Typography variant="h6">Original SQL</Typography>
                  <Stack direction="row" spacing={1}>
                    <Tooltip title="Copy">
                      <IconButton
                        size="small"
                        onClick={() => handleCopy(originalSQL, 'original')}
                        color={copiedOriginal ? 'success' : 'default'}
                      >
                        {copiedOriginal ? <CheckIcon /> : <CopyIcon />}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Download">
                      <IconButton
                        size="small"
                        onClick={() => handleDownload(originalSQL, 'original_procedure.sql')}
                      >
                        <DownloadIcon />
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </Stack>
                <Paper
                  variant="outlined"
                  sx={{
                    height: 500,
                    overflow: 'hidden',
                    borderRadius: 1,
                    border: '2px solid',
                    borderColor: 'error.light',
                  }}
                >
                  <Editor
                    height="100%"
                    defaultLanguage="sql"
                    value={originalSQL}
                    theme={mode === 'dark' ? 'vs-dark' : 'light'}
                    options={{
                      readOnly: true,
                      minimap: { enabled: false },
                      fontSize: 14,
                      lineNumbers: 'on',
                      scrollBeyondLastLine: false,
                      automaticLayout: true,
                      wordWrap: 'on',
                    }}
                  />
                </Paper>
                <Box sx={{ mt: 2 }}>
                  <Chip label="Before Optimization" color="error" variant="outlined" />
                </Box>
              </CardContent>
            </Card>
          </Grid>

          {/* Optimized SQL */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
                  <Typography variant="h6">Optimized SQL</Typography>
                  <Stack direction="row" spacing={1}>
                    <Tooltip title="Copy">
                      <IconButton
                        size="small"
                        onClick={() => handleCopy(optimizedSQL, 'optimized')}
                        color={copiedOptimized ? 'success' : 'default'}
                      >
                        {copiedOptimized ? <CheckIcon /> : <CopyIcon />}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Download">
                      <IconButton
                        size="small"
                        onClick={() => handleDownload(optimizedSQL, 'optimized_procedure.sql')}
                      >
                        <DownloadIcon />
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </Stack>
                <Paper
                  variant="outlined"
                  sx={{
                    height: 500,
                    overflow: 'hidden',
                    borderRadius: 1,
                    border: '2px solid',
                    borderColor: 'success.light',
                  }}
                >
                  <Editor
                    height="100%"
                    defaultLanguage="sql"
                    value={optimizedSQL}
                    theme={mode === 'dark' ? 'vs-dark' : 'light'}
                    options={{
                      readOnly: true,
                      minimap: { enabled: false },
                      fontSize: 14,
                      lineNumbers: 'on',
                      scrollBeyondLastLine: false,
                      automaticLayout: true,
                      wordWrap: 'on',
                    }}
                  />
                </Paper>
                <Box sx={{ mt: 2 }}>
                  <Chip label="After Optimization" color="success" variant="outlined" />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Key Differences */}
        <Card sx={{ mt: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Key Differences
            </Typography>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} md={4}>
                <Paper sx={{ p: 2, bgcolor: 'success.50' }}>
                  <Typography variant="subtitle2" fontWeight={600} color="success.main">
                    ✓ Specific Column Selection
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    Replaced SELECT * with specific columns to reduce data transfer
                  </Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={4}>
                <Paper sx={{ p: 2, bgcolor: 'success.50' }}>
                  <Typography variant="subtitle2" fontWeight={600} color="success.main">
                    ✓ Index Hint Added
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    WITH (INDEX) hint to force optimal index usage
                  </Typography>
                </Paper>
              </Grid>
              <Grid item xs={12} md={4}>
                <Paper sx={{ p: 2, bgcolor: 'success.50' }}>
                  <Typography variant="subtitle2" fontWeight={600} color="success.main">
                    ✓ Query Optimization
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    MAXDOP hint added to control parallelism
                  </Typography>
                </Paper>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
};
