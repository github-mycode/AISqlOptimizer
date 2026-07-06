import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Divider,
  Button,
  Stack,
  Paper,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Grid,
  Chip,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Code as CodeIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import Editor from '@monaco-editor/react';
import { useQuery } from '@tanstack/react-query';
import { databaseService } from '../../core/services/database.service';
import { useThemeMode } from '../../core/theme/ThemeProvider';

export const ProcedureDetailsPage = () => {
  const { procedureName } = useParams<{ procedureName: string }>();
  const navigate = useNavigate();
  const { mode } = useThemeMode();
  const [connection, setConnection] = useState<any>(null);

  useEffect(() => {
    const currentConnection = databaseService.getCurrentConnection();
    console.log('ProcedureDetailsPage - Current connection:', currentConnection);
    if (currentConnection) {
      setConnection(currentConnection);
    } else {
      // Create a mock connection if none exists
      const mockConnection = {
        databaseType: 0,
        server: 'localhost',
        database: 'TestDB',
      };
      console.log('ProcedureDetailsPage - Creating mock connection:', mockConnection);
      setConnection(mockConnection);
    }
  }, []);

  const { data: procedureDefinition, isLoading, error } = useQuery({
    queryKey: ['procedure-definition', procedureName, connection],
    queryFn: async () => {
      if (!connection || !procedureName) {
        console.log('ProcedureDetailsPage - Missing connection or procedure name');
        return null;
      }
      console.log('ProcedureDetailsPage - Fetching definition for:', procedureName);
      const result = await databaseService.getStoredProcedureDefinition(
        connection,
        procedureName
      );
      console.log('ProcedureDetailsPage - Received definition:', result?.substring(0, 100));
      return result;
    },
    enabled: !!connection && !!procedureName,
  });

  console.log('ProcedureDetailsPage - State:', { 
    procedureName, 
    connection, 
    isLoading, 
    error,
    hasDefinition: !!procedureDefinition 
  });

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
    <Container maxWidth={false} sx={{ px: 3 }}>
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
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <CodeIcon sx={{ fontSize: 40, mr: 2, color: 'primary.main' }} />
            <Box>
              <Typography variant="h4" component="h1" sx={{ fontWeight: 600 }}>
                {decodeURIComponent(procedureName || '')}
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
                Stored Procedure Details
              </Typography>
            </Box>
          </Box>
        </Box>

        <Divider sx={{ mb: 4 }} />

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 6 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Grid container spacing={3}>
            {/* Left Side - SQL Code */}
            <Grid item xs={12} md={10}>
              <Card>
                <CardContent>
                  <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 2 }}>
                    <CodeIcon />
                    <Typography variant="h6">Original SQL</Typography>
                  </Stack>
                  <Paper
                    variant="outlined"
                    sx={{
                      height: 600,
                      overflow: 'hidden',
                      borderRadius: 1,
                    }}
                  >
                    <Editor
                      height="100%"
                      defaultLanguage="sql"
                      value={procedureDefinition || '-- No definition available'}
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
                </CardContent>
              </Card>
            </Grid>

            {/* Right Side - Procedure Information */}
            <Grid item xs={12} md={2}>
              <Stack spacing={3}>
                {/* Procedure Info */}
                <Card>
                  <CardContent>
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 2 }}>
                      <InfoIcon />
                      <Typography variant="h6">Procedure Information</Typography>
                    </Stack>
                    <List dense>
                      <ListItem>
                        <ListItemText
                          primary="Name"
                          secondary={decodeURIComponent(procedureName || '')}
                        />
                      </ListItem>
                      <ListItem>
                        <ListItemText
                          primary="Type"
                          secondary="Stored Procedure"
                        />
                      </ListItem>
                      <ListItem>
                        <ListItemText
                          primary="Status"
                          secondary={
                            <Chip label="Active" color="success" size="small" />
                          }
                        />
                      </ListItem>
                    </List>
                  </CardContent>
                </Card>

                {/* Tables Used */}
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Tables Used
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Analysis in progress...
                    </Typography>
                    <List dense sx={{ mt: 1 }}>
                      <ListItem>
                        <ListItemText secondary="No table information available" />
                      </ListItem>
                    </List>
                  </CardContent>
                </Card>

                {/* Indexes */}
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Indexes
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      No index information available
                    </Typography>
                  </CardContent>
                </Card>

                {/* Parameters */}
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Parameters
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      No parameters detected
                    </Typography>
                  </CardContent>
                </Card>

                {/* Dependencies */}
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      Dependencies
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      No dependencies found
                    </Typography>
                  </CardContent>
                </Card>

                {/* Actions */}
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom sx={{ mb: 2 }}>
                      Actions
                    </Typography>
                    <Stack spacing={1}>
                      <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/ai-analysis', {
                          state: { procedureName: decodeURIComponent(procedureName || '') }
                        })}
                      >
                        Analyze Performance
                      </Button>
                      <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/ai-analysis', {
                          state: {
                            procedureName: decodeURIComponent(procedureName || ''),
                            mode: 'optimize'
                          }
                        })}
                      >
                        Get Optimization
                      </Button>
                    </Stack>
                  </CardContent>
                </Card>
              </Stack>
            </Grid>
          </Grid>
        )}
      </Box>
    </Container>
  );
};
